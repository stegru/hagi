using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using HagiShared.Api;
using Microsoft.Extensions.Logging.Abstractions;

namespace GuestClient
{
    class Program
    {

        static void Main(string[] args)
        {

            HostRequest? request = null;

            Parser.Default.ParseArguments<OpenRequest, FileMapRequest>(args)
                .WithParsed((OpenRequest req) => request = req)
                .WithParsed((FileMapRequest req) => request = req);


            if (request != null)
            {
                Program.ResolvePaths(request);
                Program.MakeRequest(request).Wait();
            }
        }

        private static void ResolvePaths(HostRequest request)
        {
            foreach (PropertyInfo propertyInfo in request.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => pi.CanWrite && pi.Name.EndsWith("Path") && pi.PropertyType == typeof(string)))
            {
                string? path = propertyInfo.GetValue(request) as string;
                if (!string.IsNullOrEmpty(path))
                {
                    bool isUri = Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? uri);
                    if ((!isUri || uri?.IsFile == true) && !Path.IsPathRooted(path))
                    {
                        propertyInfo.SetValue(request, Path.GetFullPath(path, Directory.GetCurrentDirectory()));
                    }
                }
            }
        }

        private static async Task MakeRequest(HostRequest request)
        {
            IRequestOptions options = ((IRequestOptions)request);
            VerbAttribute verb = request.GetType().GetCustomAttribute<VerbAttribute>() ??
                                 throw new InvalidOperationException("HostRequest without VerbAttribute");

            UriBuilder builder = new UriBuilder("http", options.Host, 5580);
            builder.Path = $"hagi/{verb.Name}";
            Uri uri = builder.Uri;

            HttpClient client = new HttpClient();

            await client.PostAsync(uri, new StringContent(JsonSerializer.Serialize(request, request.GetType()), Encoding.UTF8, "application/json"));
        }
    }
}