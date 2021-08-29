using CommandLine;

namespace GuestClient
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            RequestOptions? options = null;

            Parser.Default.ParseArguments(args, RequestOptions.AllTypes)
                .WithParsed(o => options = o as RequestOptions);

            if (options == null)
            {
                return;
            }

            GuestClient guestClient = new GuestClient(options);
            try
            {
                guestClient.Run().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}