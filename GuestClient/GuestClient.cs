namespace Hagi.HagiGuest
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HagiShared.Api;
    using HagiShared.Network;
    using HagiShared.Platform;

    /// <summary>
    /// Deals with the host.
    /// </summary>
    public class GuestClient
    {
        private static readonly JsonSerializerOptions SerializerOptions =
            new JsonSerializerOptions(JsonSerializerDefaults.Web);

        private readonly RequestOptions options;

        public GuestClient(RequestOptions options)
        {
            this.options = options;
        }

        private static async Task<string> GetHost()
        {
            HostDetection hostDetection = new HostDetection();
            IPAddress ipAddress = await hostDetection.FindHost();
            return ipAddress.ToString();
        }

        /// <summary>
        /// Start the request.
        /// </summary>
        public async Task Run()
        {
            bool retry;
            bool autoJoin = true;
            int loopGuard = 5;

            if (this.options is JoinRequestOptions joinRequest)
            {
                this.options.GuestId = joinRequest.Guest;
                autoJoin = false;
            }

            do
            {
                retry = false;
                this.options.Host ??= Config.Current["host"] ?? await GuestClient.GetHost();
                if (string.IsNullOrEmpty(this.options.GuestId)) {
                    this.options.GuestId = Config.Current["guest"] ?? string.Empty;
                }
                this.options.Secret = Config.Current["secret"];

                if (Config.Current["machine-name"] != SmbShare.FullName && autoJoin)
                {
                    // Machine name has changed since last time.
                    await this.JoinHost();
                    retry = true;
                    autoJoin = false;
                    continue;
                }

                this.ResolvePaths();

                try
                {
                    await this.MakeRequest<HostResponse>();
                }
                catch (RequestException requestException)
                    when (requestException.StatusCode == HttpStatusCode.Unauthorized && this.options is not JoinRequestOptions && autoJoin)
                {
                    await this.JoinHost();
                    retry = true;
                    autoJoin = false;
                }

            } while (retry && --loopGuard > 0);
        }

        private static string? GetRequestUrl(HostRequest request)
        {
            return request.GetType().GetCustomAttribute<RequestAttribute>()?.Path;
        }

        private void ResolvePaths()
        {
            foreach (PropertyInfo propertyInfo in this.options.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => pi.CanWrite && pi.Name.EndsWith("Path") && pi.PropertyType == typeof(string)))
            {
                string? path = propertyInfo.GetValue(this.options) as string;
                if (!string.IsNullOrEmpty(path))
                {
                    bool isUri = Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? uri);
                    if ((!isUri || uri?.IsFile == true) && !Path.IsPathRooted(path))
                    {
                        propertyInfo.SetValue(this.options, Path.GetFullPath(path, Directory.GetCurrentDirectory()));
                    }
                }
            }
        }

        private async Task<T> MakeRequest<T>(HostRequest? request = null)
            where T : HostResponse
        {
            request ??= this.options.GetRequest();

            UriBuilder builder = new UriBuilder("http", this.options.Host, 5580)
            {
                Path = GuestClient.GetRequestUrl(request) ?? this.options.RequestUrl
            };

            Uri uri = builder.Uri;

            HttpClient client = new HttpClient();

            StringContent content = new StringContent(JsonSerializer.Serialize(request, request.GetType()),
                Encoding.UTF8,
                "application/json");

            content.Headers.Add("X-Guest", this.options.GuestId);
            content.Headers.Add("X-Secret", this.options.Secret);

            Console.WriteLine(await content.ReadAsStringAsync());

            HttpResponseMessage responseMessage = await client.PostAsync(uri, content);

            string responseText = await responseMessage.Content.ReadAsStringAsync()
                                  ?? throw new ApplicationException("No response");

            Console.WriteLine(responseText);

            if (!responseMessage.IsSuccessStatusCode)
            {
                ErrorResponse errorResponse =
                    JsonSerializer.Deserialize<ErrorResponse>(responseText, GuestClient.SerializerOptions)
                    ?? throw new ApplicationException("Unable to parse error response");

                throw new RequestException(errorResponse, responseMessage.StatusCode);
            }

            T response = JsonSerializer.Deserialize<T>(responseText, GuestClient.SerializerOptions)
                         ?? throw new ApplicationException("Unable to parse response");

            if (response is JoinResponse joinResponse)
            {

                Config.Current["guest"] = joinResponse.GuestId;
                Config.Current["secret"] = joinResponse.GuestSecret;
                Config.Current["host"] = this.options.Host;

                JoinRequest? joinRequest = request as JoinRequest;
                Config.Current["machine-name"] = joinRequest?.MachineName ?? SmbShare.FullName;

                Config.Current.SaveFile();
            }

            return response;
        }

        /// <summary>Generates a name for this guest machine.</summary>
        private static string GenerateName()
        {
            Random random = new Random();
            return $"{Environment.MachineName}-{OS.Current.Name}-{random.Next():x}";
        }

        /// <summary>
        /// Join the host machine.
        /// </summary>
        private async Task JoinHost()
        {
            string guest = Config.Current["guest"] ?? GuestClient.GenerateName();

            JoinRequest request = new JoinRequest()
            {
                Guest = guest,
                MachineName = SmbShare.FullName
            };

            await this.MakeRequest<JoinResponse>(request);
        }

        public class RequestException : Exception
        {
            public ErrorResponse Response { get; }
            public HttpStatusCode StatusCode { get; }

            public RequestException(ErrorResponse response, HttpStatusCode statusCode)
            {
                this.Response = response;
                this.StatusCode = statusCode;
            }
        }

    }
}