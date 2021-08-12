namespace Hagi.HostServer.GuestIntegration
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class ProcessExtensions
    {
        public static Task<int> StartAsync(this Process process)
        {
            process.EnableRaisingEvents = true;
            TaskCompletionSource<int> completionSource = new TaskCompletionSource<int>();

            process.Exited += (sender, eventArgs) =>
            {
                completionSource.TrySetResult(process.ExitCode);
            };

            process.Start();

            return completionSource.Task;
        }

        public static async Task<(int exitCode, string output)> StartCaptureAsync(this Process process)
        {
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            return (process.ExitCode, output);
        }

    }
}