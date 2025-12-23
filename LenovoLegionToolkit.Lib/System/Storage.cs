using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace LenovoLegionToolkit.Lib.System;

public class DriveHealthInfo
{
    public string DeviceName { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string MediaType { get; set; } = string.Empty; // "SSD", "HDD", "Unknown"
    public int? TemperatureC { get; set; }
    public int? HealthPercentage { get; set; }
    public int? PowerOnHours { get; set; }
    public int? PowerCycleCount { get; set; }
    public string Status { get; set; } = "Unknown";
}

public static class Storage
{
    public static async Task<List<DriveHealthInfo>> GetDriveHealthInfoAsync()
    {
        return await Task.Run(() =>
        {
            var drives = new List<DriveHealthInfo>();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject disk in searcher.Get())
                {
                    var driveInfo = new DriveHealthInfo
                    {
                        DeviceName = disk["DeviceID"]?.ToString() ?? "Unknown",
                        Model = disk["Model"]?.ToString() ?? "Unknown",
                        SerialNumber = disk["SerialNumber"]?.ToString()?.Trim() ?? "Unknown",
                        SizeBytes = Convert.ToInt64(disk["Size"] ?? 0),
                        MediaType = disk["MediaType"]?.ToString() ?? "Unknown",
                        Status = disk["Status"]?.ToString() ?? "Unknown"
                    };

                    // Try to get S.M.A.R.T. data
                    try
                    {
                        var smartData = GetSmartData(driveInfo.DeviceName);
                        if (smartData != null)
                        {
                            driveInfo.TemperatureC = smartData.Temperature;
                            driveInfo.HealthPercentage = smartData.HealthPercentage;
                            driveInfo.PowerOnHours = smartData.PowerOnHours;
                            driveInfo.PowerCycleCount = smartData.PowerCycleCount;
                        }
                    }
                    catch
                    {
                        // S.M.A.R.T. not available for this drive
                    }

                    // Determine if SSD or HDD
                    if (driveInfo.MediaType == "Unknown")
                    {
                        var model = driveInfo.Model.ToLower();
                        if (model.Contains("ssd") || model.Contains("nvme"))
                            driveInfo.MediaType = "SSD";
                        else if (model.Contains("hdd") || model.Contains("hard"))
                            driveInfo.MediaType = "HDD";
                    }

                    drives.Add(driveInfo);
                }
            }
            catch
            {
                // Unable to query drives
            }

            return drives;
        });
    }

    private static SmartData? GetSmartData(string deviceId)
    {
        try
        {
            // Extract physical drive number from device ID (e.g., "\\\\.\\PHYSICALDRIVE0" -> 0)
            var driveNumber = deviceId.Replace("\\\\.\\PHYSICALDRIVE", "");

            using var searcher = new ManagementObjectSearcher($"SELECT * FROM MSStorageDriver_ATAPISmartData WHERE InstanceName LIKE '%PhysicalDrive{driveNumber}%'");
            var collection = searcher.Get();

            if (collection.Count == 0)
                return null;

            foreach (ManagementObject data in collection)
            {
                var vendorSpecific = data["VendorSpecific"] as byte[];
                if (vendorSpecific == null || vendorSpecific.Length < 362)
                    continue;

                var smartData = new SmartData();

                // Parse S.M.A.R.T. attributes
                // Attributes start at offset 2, each attribute is 12 bytes
                for (int i = 2; i < 362; i += 12)
                {
                    var attributeId = vendorSpecific[i];
                    if (attributeId == 0) continue;

                    var rawValue = BitConverter.ToInt64(new byte[] {
                        vendorSpecific[i + 5],
                        vendorSpecific[i + 6],
                        vendorSpecific[i + 7],
                        vendorSpecific[i + 8],
                        vendorSpecific[i + 9],
                        vendorSpecific[i + 10],
                        0, 0
                    }, 0);

                    var currentValue = vendorSpecific[i + 3];
                    var worstValue = vendorSpecific[i + 4];

                    switch (attributeId)
                    {
                        case 0x09: // Power-On Hours
                            smartData.PowerOnHours = (int)rawValue;
                            break;
                        case 0x0C: // Power Cycle Count
                            smartData.PowerCycleCount = (int)rawValue;
                            break;
                        case 0xC2: // Temperature
                            smartData.Temperature = (int)(rawValue & 0xFF);
                            break;
                        case 0x05: // Reallocated Sectors Count
                        case 0xC5: // Current Pending Sector Count
                        case 0xC6: // Uncorrectable Sector Count
                            // These affect health percentage
                            if (rawValue > 0)
                                smartData.HealthPercentage = Math.Min(smartData.HealthPercentage ?? 100, currentValue);
                            break;
                    }
                }

                // Calculate overall health if not set
                if (!smartData.HealthPercentage.HasValue)
                    smartData.HealthPercentage = 100;

                return smartData;
            }
        }
        catch
        {
            // S.M.A.R.T. data unavailable
        }

        return null;
    }

    private class SmartData
    {
        public int? Temperature { get; set; }
        public int? HealthPercentage { get; set; } = 100;
        public int? PowerOnHours { get; set; }
        public int? PowerCycleCount { get; set; }
    }
}
