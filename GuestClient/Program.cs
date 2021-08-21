﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CommandLine;
using HagiShared.Api;
using HagiShared.Network;
using Microsoft.Extensions.Logging.Abstractions;

namespace GuestClient
{
    class Program
    {

        private static async Task GetHost()
        {
            HostDetection hostDetection = new HostDetection(new NullLogger<HostDetection>());
            IPAddress ipAddress = await hostDetection.FindHost();
            Console.WriteLine(ipAddress.ToString());
        }
        static void Main(string[] args)
        {
            RequestOptions? options = null;

            Parser.Default.ParseArguments(args, RequestOptions.AllTypes)
                .WithParsed(o => options = o as RequestOptions);

            if (options == null)
            {
                return;
            }

            Config.Load(options.ConfigFile);

            options.Host ??= Config.Current["host"];

            Program.ResolvePaths(options);
            Program.MakeRequest(options).Wait();
        }

        private static void JoinHost(RequestOptions options)
        {
            JoinRequest request = new JoinRequest()
            {
            };

            Program.MakeRequest(options, request).Wait();
        }


        private static void ResolvePaths(RequestOptions options)
        {
            foreach (PropertyInfo propertyInfo in options.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(pi => pi.CanWrite && pi.Name.EndsWith("Path") && pi.PropertyType == typeof(string)))
            {
                string? path = propertyInfo.GetValue(options) as string;
                if (!string.IsNullOrEmpty(path))
                {
                    bool isUri = Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out Uri? uri);
                    if ((!isUri || uri?.IsFile == true) && !Path.IsPathRooted(path))
                    {
                        propertyInfo.SetValue(options, Path.GetFullPath(path, Directory.GetCurrentDirectory()));
                    }
                }
            }
        }

        private static async Task MakeRequest(RequestOptions options, HostRequest? request = null)
        {
            request ??= options.GetRequest();
            request.Guest ??= Config.Current["guest"];

            UriBuilder builder = new UriBuilder("http", options.Host, 5580);
            builder.Path = options.RequestUrl;
            Uri uri = builder.Uri;

            HttpClient client = new HttpClient();

            StringContent content = new StringContent(JsonSerializer.Serialize(request, request.GetType()), Encoding.UTF8,
                "application/json");

            Console.WriteLine(await content.ReadAsStringAsync());

            HttpResponseMessage responseMessage = await client.PostAsync(uri, content);
            string response = await responseMessage.Content.ReadAsStringAsync();
            Console.WriteLine(response);
        }


    }
}