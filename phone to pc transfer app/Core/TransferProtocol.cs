using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace phone_to_pc_transfer_app.Core
{
    public class TransferHeader
    {
        public string Type { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long PayloadLength { get; set; }

    }
    public static class TransferProtocol
    {
        public const string TypeFile = "file";
        public const string TypeText = "text";
        private const int MaxHeaderBytes = 64 * 1024;

        public static async Task WriteHeaderAsync(Stream stream, TransferHeader header, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(header);
            var headerBytes = Encoding.UTF8.GetBytes(json);

            var lengthPrefix = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(lengthPrefix, headerBytes.Length);

            await stream.WriteAsync(lengthPrefix, cancellationToken);
            await stream.WriteAsync(headerBytes, cancellationToken);
        }

        public static async Task<TransferHeader?> ReadHeaderAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var lengthPrefix = new byte[4];
            if (!await ReadExactlyAsync(stream, lengthPrefix, cancellationToken))
                return null;

            var headerLength = BinaryPrimitives.ReadInt32BigEndian(lengthPrefix);
            if (headerLength <= 0 || headerLength > MaxHeaderBytes)
                throw new InvalidDataException($"Invalid headerlength: {headerLength}");

            var headerBytes = new byte[headerLength];
            if (!await ReadExactlyAsync(stream, headerBytes, cancellationToken))
                throw new EndOfStreamException("Connection closed mid header.");

            var json = Encoding.UTF8.GetString(headerBytes);
            return JsonSerializer.Deserialize<TransferHeader>(json);
        }

        public static async Task CopyExactlyAsync(Stream source, Stream destination, long count, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[81920];
            long remaining = count;

            while (remaining > 0)
            {
                var toRead = (int)Math.Min(buffer.Length, remaining);
                var read = await source.ReadAsync(buffer.AsMemory(0, toRead), cancellationToken);

                if (read == 0)
                    throw new EndOfStreamException($"Connection closed with {remaining} bytes still expected.");

                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                remaining -= read;
            }

        }

        private static async Task<bool> ReadExactlyAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            var offset = 0;

            while (offset < buffer.Length)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);
               
                if (read ==0)
                    return offset != 0 ? throw new EndOfStreamException("Connection closed mid read.") : false;

                offset += read;
            }

            return true;

        }
    }
}
