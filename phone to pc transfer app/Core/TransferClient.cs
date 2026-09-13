using System;
using System.Collections.Generic;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    internal class TransferClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public TransferClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public async Task SendFileAsync(string targetIp, int targetPort, string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var fileInfo = new FileInfo(filePath);
            var url = $"http://{targetIp}:{targetPort}/file";

            await using var fileStream = File.OpenRead(filePath);

            var content = new StreamContent(fileStream);
            content.Headers.Add("X-File-Name", fileInfo.Name);

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendTextAsync(string targetIp, int targetPort, string message, CancellationToken cancellationToken = default)
        {
            var url = $"http://{targetIp}:{targetPort}/text";
            var content = new StringContent(message);

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
