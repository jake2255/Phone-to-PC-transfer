using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace phone_to_pc_transfer_app.Core
{
    public class DeviceListener : IDisposable
    {
        private readonly string _localDeviceId;
        private UdpClient? _udpClient;
        private CancellationTokenSource? _cts;
        private Task? _listenLoop;

        public event EventHandler<DiscoveredDevice>? DeviceAnnounced;

        public DeviceListener(string localDeviceId)
        {
            _localDeviceId = localDeviceId;
        }

        public void Start()
        {
            if (_listenLoop != null)
                return;

            _udpClient = new UdpClient();

            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, DeviceBeacon.DiscoveryPort));

            _cts = new CancellationTokenSource();
            _listenLoop = RunLoopAsync(_cts.Token);
        }

        private async Task RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                UdpReceiveResult result;

                try
                {
                    result = await _udpClient!.ReceiveAsync(token);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (SocketException)
                {
                    continue;
                }

                TryHandlePacket(result);
            }
        }

        private void TryHandlePacket(UdpReceiveResult result)
        {
            BeaconPayload? payload;

            try
            {
                var json = Encoding.UTF8.GetString(result.Buffer);
                payload = JsonSerializer.Deserialize<BeaconPayload>(json);
            }
            catch (JsonException)
            {
                return;
            }

            if (payload is null || string.IsNullOrEmpty(payload.DeviceId))
                return;

            if (payload.DeviceId == _localDeviceId)
                return;

            var device = new DiscoveredDevice
            {
                DeviceId = payload.DeviceId,
                Name = payload.Name,
                Platform = payload.Platform,
                TransferPort = payload.TransferPort,
                IpAddress = result.RemoteEndPoint.Address.ToString(),
                lastSeen = DateTime.UtcNow
            };

            DeviceAnnounced?.Invoke(this, device);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _udpClient?.Close();
            _udpClient?.Dispose();
            _udpClient = null;
            _listenLoop = null;
            _cts?.Dispose();
            _cts = null;
        }

        public void Dispose() => Stop();
    }
}
