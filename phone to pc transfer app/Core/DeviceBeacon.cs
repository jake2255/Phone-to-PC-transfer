using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace phone_to_pc_transfer_app.Core
{
    public class DeviceBeacon : IDisposable
    {
        public const int DiscoveryPort = 53318;
        private readonly BeaconPayload _payload;
        private readonly TimeSpan _interval;
        private UdpClient? _udpClient;
        private CancellationTokenSource? _cts;
        private Task? _broadcastLoop;

        public DeviceBeacon(string deviceId, string deviceName, string platform, int transferPort, TimeSpan? interval = null)
        {
            _payload = new BeaconPayload
            {
                DeviceId = deviceId,
                Name = deviceName,
                Platform = platform,
                TransferPort = transferPort
            };

            _interval = interval ?? TimeSpan.FromSeconds(2.5);
        }

        public void Start()
        {
            if (_broadcastLoop != null)
                return;

            _udpClient = new UdpClient
            {
                EnableBroadcast = true
            };

            _cts = new CancellationTokenSource();
            _broadcastLoop = RunLoopAsync(_cts.Token);

        }

        private async Task RunLoopAsync(CancellationToken token)
        {
            var json = JsonSerializer.Serialize(_payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            var endpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    await _udpClient!.SendAsync(bytes, bytes.Length, endpoint);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    // Network unavailable
                }

                try
                {
                    await Task.Delay(_interval, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

        }

        public void Stop()
        {
            _cts?.Cancel();
            _udpClient?.Close();
            _udpClient?.Dispose();
            _udpClient = null;
            _broadcastLoop = null;
            _cts?.Dispose();
            _cts = null;
        }

        public void Dispose() => Stop();
    }
}
