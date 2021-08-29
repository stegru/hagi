using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HagiShared.Network
{
    public class HostDetection
    {
        private const string DetectionRequestString = "hagi-detect";
        private const string DetectionReplyString = "hagi-reply";

        private readonly byte[] _detectionRequest = Encoding.UTF8.GetBytes(HostDetection.DetectionRequestString);
        private readonly byte[] _detectionReply = Encoding.UTF8.GetBytes(HostDetection.DetectionReplyString);

        public HostDetection()
        {
        }

        /// <summary>
        /// Gets the ip address of the network interface that's used to connect to the given external address.
        /// </summary>
        /// <param name="dest">The external IP address.</param>
        /// <returns>The local IP address.</returns>
        private async Task<IPAddress?> GetLocalAddress(IPAddress dest)
        {
            // Connecting a udp socket causes the local end-point to be known.
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Unspecified);
            await socket.ConnectAsync(dest, 0);
            return (socket.LocalEndPoint as IPEndPoint)?.Address;
        }

        private CancellationTokenSource? _listenCancellation;

        /// <summary>
        /// Called by the host, to stop listening for host-detection requests.
        /// </summary>
        public void Stop()
        {
            this._listenCancellation?.Cancel();
        }

        /// <summary>
        /// Called by the host, to listen for host-detection requests.
        /// </summary>
        public async Task Listen(int port = 5580)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            if (this._listenCancellation == null)
            {
                this._listenCancellation = new CancellationTokenSource();
            }
            else
            {
                string message = $"{nameof(this.Listen)} called twice";
                throw new InvalidOperationException(message);
            }

            this._listenCancellation.Token.Register(() => taskCompletionSource.TrySetResult(true));

            UdpClient udpClient = new UdpClient(port);

            while (true)
            {
                Task<UdpReceiveResult> dataTask = udpClient.ReceiveAsync();

                // Wait for data, or cancellation
                Task task = await Task.WhenAny(dataTask, taskCompletionSource.Task);
                if (task != dataTask)
                {
                    throw new OperationCanceledException(this._listenCancellation.Token);
                }

                // Makes sure it was successful
                await dataTask;

                UdpReceiveResult udpResult = dataTask.Result;

                bool isDetection = udpResult.Buffer.Take(this._detectionRequest.Length)
                    .SequenceEqual(this._detectionRequest);

                if (isDetection)
                {
                    IPAddress? localAddress = await this.GetLocalAddress(udpResult.RemoteEndPoint.Address);
                    if (localAddress != null)
                    {
                        byte[] reply = this._detectionReply.Concat(localAddress.GetAddressBytes()).ToArray();
                        await udpClient.SendAsync(reply, reply.Length, udpResult.RemoteEndPoint);
                    }
                }
            }
        }

        public async Task<IPAddress> FindHost(int port = 5580)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;

            await udpClient.SendAsync(this._detectionRequest, this._detectionRequest.Length,
                new IPEndPoint(IPAddress.Broadcast, port));

            Task<UdpReceiveResult> dataTask = udpClient.ReceiveAsync();
            Task task = await Task.WhenAny(dataTask, Task.Delay(TimeSpan.FromSeconds(5)));

            IPAddress hostIp = IPAddress.None;

            if (task == dataTask)
            {
                await dataTask;
                int expectedLength = this._detectionReply.Length + 4;
                byte[] reply = dataTask.Result.Buffer.Take(expectedLength).ToArray();

                if (reply.Length == expectedLength &&
                    reply.Take(this._detectionReply.Length).SequenceEqual(this._detectionReply))
                {
                    hostIp = new IPAddress(reply.TakeLast(4).ToArray());
                }
            }

            return hostIp;
        }
    }
}