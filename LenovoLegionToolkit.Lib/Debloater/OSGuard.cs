using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace LenovoLegionToolkit.Lib.Debloater;

/// <summary>
/// Detects OS version, edition, and policy constraints for conditional tweak availability.
/// </summary>
public class OSGuard
{
    public enum WindowsVersion
    {
        Windows10,
        Windows11,
        Unknown
    }

    public enum WindowsEdition
    {
        Home,
        Pro,
        Enterprise,
        Education,
        Unknown
    }

    public static WindowsVersion GetWindowsVersion()
    {
        try
        {
            var buildNumber = GetBuildNumber();
            var isWin11 = buildNumber >= 22000;
            return isWin11 ? WindowsVersion.Windows11 : WindowsVersion.Windows10;
        }
        catch
        {
            return WindowsVersion.Unknown;
        }
    }

    public static WindowsEdition GetWindowsEdition()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var editionId = key?.GetValue("EditionID")?.ToString() ?? "";
            
            return editionId switch
            {
                "Core" => WindowsEdition.Home,
                "Professional" => WindowsEdition.Pro,
                "Enterprise" => WindowsEdition.Enterprise,
                "Education" => WindowsEdition.Education,
                _ => WindowsEdition.Unknown
            };
        }
        catch
        {
            return WindowsEdition.Unknown;
        }
    }

    public static int GetBuildNumber()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var buildStr = key?.GetValue("CurrentBuildNumber")?.ToString() ?? "0";
            return int.TryParse(buildStr, out var build) ? build : 0;
        }
        catch
        {
            return 0;
        }
    }

    public static bool IsPolicyManaged()
    {
        try
        {
            // Check if Group Policy domain is applied
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies");
            return key?.GetValue("Gpo") != null;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsMDMEnrolled()
    {
        try
        {
            // Check if device is MDM-enrolled (Intune/similar)
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\MDM");
            return key?.GetValue("EnrollmentStatus")?.ToString() == "1";
        }
        catch
        {
            return false;
        }
    }

    public static string GetSystemProtectionStatus()
    {
        try
        {
            // Check if System Restore is enabled
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\VSS");
            var startType = key?.GetValue("Start");
            return startType switch
            {
                2 or 4 => "Enabled",
                _ => "Disabled"
            };
        }
        catch
        {
            return "Unknown";
        }
    }

    public static bool IsDefenderActive()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender");
            var disableAntiSpyware = key?.GetValue("DisableAntiSpyware");
            return disableAntiSpyware is not 1;
        }
        catch
        {
            return true; // Assume enabled if we can't check
        }
    }

    public static string GetSystemDeviceName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\ComputerName\ComputerName");
            return key?.GetValue("ComputerName")?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    public static string GetSystemInfo()
    {
        var version = GetWindowsVersion();
        var edition = GetWindowsEdition();
        var build = GetBuildNumber();
        var protection = GetSystemProtectionStatus();
        var defender = IsDefenderActive();
        var managed = IsPolicyManaged() || IsMDMEnrolled();

        return $"Windows {version} {edition} (Build {build}) | Protection: {protection} | Defender: {(defender ? "Active" : "Disabled")} | Managed: {(managed ? "Yes" : "No")}";
    }
}
