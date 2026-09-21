using System;
using System.Collections.Generic;
using System.Text;

namespace phone_to_pc_transfer_app.Core
{
    public static class AppSettings
    {
        private const string DeviceIdKey = "device_id";
        private const string DeviceNameKey = "device_name";

        public const int TransferPort = 53317;

        public static string DeviceId
        {
            get
            {
                var existing = Preferences.Get(DeviceIdKey, string.Empty);
                if (!string.IsNullOrEmpty(existing))
                    return existing;

                var generated = Guid.NewGuid().ToString();
                Preferences.Set(DeviceIdKey, generated);
                return generated;
            }
        }

        public static string DeviceName
        {
            get => Preferences.Get(DeviceNameKey, DefaultDeviceName());
            set => Preferences.Set(DeviceNameKey, value);
        }

        public static string Platform => DeviceInfo.Platform == DevicePlatform.WinUI ? "Windows" : "Android";

        public static string SaveFolder => Path.Combine(FileSystem.AppDataDirectory, "ReceivedFiles");

        private static string DefaultDeviceName()
        {
            return DeviceInfo.Platform == DevicePlatform.WinUI ? Environment.MachineName : DeviceInfo.Model;
        }
    }
}
