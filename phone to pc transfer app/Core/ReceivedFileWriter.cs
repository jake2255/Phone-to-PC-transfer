using System;
using System.Collections.Generic;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    public static class ReceivedFileWriter
    {
        public static async Task<string> SaveFileAsync(string fileName, Stream source, long length, CancellationToken cancellationToken = default)
        {
#if WINDOWS
            Directory.CreateDirectory(AppSettings.SaveFolder);
            var destinationPath = Path.Combine(AppSettings.SaveFolder, fileName);

            await using (var fileStream = new FileStream(
                destinationPath, 
                FileMode.Create, 
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920, 
                useAsync: true))
            {
                await TransferProtocol.CopyExactlyAsync(source, fileStream, length, cancellationToken);
            }

            return destinationPath;

#elif ANDROID
            var resolver = Android.App.Application.Context.ContentResolver ?? throw new IOException("Could not access ContentResolver");
            var values = new Android.Content.ContentValues();

            values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
            values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "application/octet-stream");
            values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads + "/DataTransfer");

            var destinationUri = resolver.Insert(Android.Provider.MediaStore.Downloads.ExternalContentUri!, values)
                ?? throw new IOException("MediaStore refused to create the destination entry");

            await using (var outputStream = resolver.OpenOutputStream(destinationUri)
                ?? throw new IOException("Could not open output stream for the destination entry"))
            {
                await TransferProtocol.CopyExactlyAsync(source, outputStream, length, cancellationToken);
            }

            return $"Downloads/DataTransfer/{fileName}";

#else
            Directory.CreateDirectory(AppSettings.SaveFolder);
            var destinationPath = Path.Combine(AppSettings.SaveFolder, fileName);

            await using (var fileStream = File.Create(destinationPath))
            {
                await TransferProtocol.CopyExactlyAsync(source, fileStream, length, cancellationToken);
            }

            return destinationPath;
#endif

        }
    }
}
