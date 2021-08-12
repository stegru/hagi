namespace Hagi.HagiGuest
{
    using System;
    using CommandLine;
    using Install;

    class Program
    {
        static void Main(string[] args)
        {
            ClientOptions? options = null;

            Parser.Default.ParseArguments(args, ClientOptions.AllTypes)
                .WithParsed(o => options = o as ClientOptions);

            switch (options)
            {
                case null:
                    break;

                case UninstallOptions uninstallOptions:
                    Console.WriteLine("Uninstalling");
                    Installer.RemoveAll(uninstallOptions);
                    break;

                case InstallOptions installOptions:
                    Console.WriteLine("Installing");
                    Installer.InstallAll(installOptions);
                    break;

                case RequestOptions requestOptions:
                {
                    GuestClient guestClient = new GuestClient(requestOptions);
                    try
                    {
                        guestClient.Run().Wait();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    break;
                }
            }
        }
    }
}