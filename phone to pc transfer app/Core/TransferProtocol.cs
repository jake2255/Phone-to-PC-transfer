using System;
using System.Collections.Generic;
using System.Text;

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

        public static async Task WriteHeaderAsync()
        {

        }

        public static async Task<TransferHeader?> ReadHeaderAsync()
        {

        }

        public static async Task CopyExactlyAsync()
        {

        }

        private static async Task<bool> ReadExactlyAsync()
        {

        }
    }
}
