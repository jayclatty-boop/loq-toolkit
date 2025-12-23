namespace LenovoLegionToolkit.Lib.Downloader;

public enum WindowsEditionType
{
    Home,
    Pro,
    Enterprise,
    Education
}

public enum WindowsVersionType
{
    Windows10_21H2,
    Windows11_22H2,
    Windows11_23H2
}

public enum LinuxDistro
{
    Ubuntu,
    Fedora,
    Debian,
    ArchLinux,
    Linux_Mint,
    Manjaro
}

public class OSImage
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public long SizeBytes { get; set; } = 0;
    public string DownloadUrl { get; set; } = "";
    public string ChecksumUrl { get; set; } = "";
    public DateTime ReleaseDate { get; set; } = DateTime.Now;
    public string Version { get; set; } = "";
}
