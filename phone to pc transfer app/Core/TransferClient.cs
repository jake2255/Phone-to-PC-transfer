using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    public class TransferClient
    {
        public async Task SendFileAsync(string targetIp, int targetPort, string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var fileInfo = new FileInfo(filePath);

            await using var fileStream = File.OpenRead(filePath);

            using var client = new TcpClient();
            await client.ConnectAsync(targetIp, targetPort, cancellationToken);

            await using var networkStream = client.GetStream();

            await TransferProtocol.WriteHeaderAsync(networkStream, new TransferHeader
            {
                Type = TransferProtocol.TypeFile,
                FileName = fileInfo.Name,
                PayloadLength = fileInfo.Length
            }, cancellationToken);

            await fileStream.CopyToAsync(networkStream, cancellationToken);
            await networkStream.FlushAsync(cancellationToken);
        }

        public async Task SendTextAsync(string targetIp, int targetPort, string message, CancellationToken cancellationToken = default)
        {
            var payload = Encoding.UTF8.GetBytes(message);

            using var client = new TcpClient();
            await client.ConnectAsync(targetIp, targetPort, cancellationToken);

            await using var networkStream = client.GetStream();

            await TransferProtocol.WriteHeaderAsync(networkStream, new TransferHeader
            {
                Type = TransferProtocol.TypeText,
                PayloadLength = payload.Length
            }, cancellationToken);

            await networkStream.WriteAsync(payload, cancellationToken);
            await networkStream.FlushAsync(cancellationToken);
        }
    }
}
