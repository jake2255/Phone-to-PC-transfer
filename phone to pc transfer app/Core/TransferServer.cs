using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    public class TransferServer : IDisposable
    {
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _acceptLoop;

        public event EventHandler<FileReceivedEventArgs>? FileReceived;

        public event EventHandler<TextReceivedEventArgs>? TextReceived;

        public event EventHandler<Exception>? TransferFailed;

        public TransferServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            if (_acceptLoop != null)
                return;

            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();

            _cts = new CancellationTokenSource();
            _acceptLoop = RunAcceptLoopAsync(_cts.Token);
        }

        private async Task RunAcceptLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client;

                try
                {
                    client = await _listener!.AcceptTcpClientAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    continue;
                }

                _ = HandleClientAsync(client, token);
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            var senderIp = (client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "Unknown";

            try
            {
                using (client)
                await using (var stream = client.GetStream())
                {
                    var header = await TransferProtocol.ReadHeaderAsync(stream, token);
                    
                    if (header is null)
                        return;

                    if (header.Type == TransferProtocol.TypeText)
                        await ReceiveTextAsync(stream, header, senderIp, token);

                    else if (header.Type == TransferProtocol.TypeFile)
                        await ReceiveFileAsync(stream, header, senderIp, token);
                }
            }
            catch (OperationCanceledException)
            {
                // nothing
            }
            catch (Exception ex)
            {
                TransferFailed?.Invoke(this, ex);
            }
        }

        private async Task ReceiveTextAsync(Stream stream, TransferHeader header, string senderIp, CancellationToken token)
        {
            using var buffer = new MemoryStream();
            await TransferProtocol.CopyExactlyAsync(stream, buffer, header.PayloadLength, token);

            var text = Encoding.UTF8.GetString(buffer.ToArray());

            TextReceived?.Invoke(this, new TextReceivedEventArgs
            {
                Text = text,
                SenderIpAddress = senderIp
            });
        }

        private async Task ReceiveFileAsync(Stream stream, TransferHeader header, string senderIp, CancellationToken token)
        {
            var safeFileName = Path.GetFileName(header.FileName);

            if (string.IsNullOrWhiteSpace(safeFileName))
                throw new InvalidDataException("Received file with an invalid name.");

            var savedLocation = await ReceivedFileWriter.SaveFileAsync(safeFileName, stream, header.PayloadLength, token);

            FileReceived?.Invoke(this, new FileReceivedEventArgs
            {
                FileName = safeFileName,
                SavedPath = savedLocation,
                SizeBytes = header.PayloadLength,
                SenderIpAddress = senderIp
            });
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _listener = null;
            _acceptLoop = null;
            _cts?.Dispose();
            _cts = null;
        }

        public void Dispose() => Stop();
    }
}
