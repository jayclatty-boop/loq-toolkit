namespace LenovoLegionToolkit.Lib.Downloader;

public class Program
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string DownloadUrl { get; set; } = "";
    public string SizeDescription { get; set; } = "Unknown";
    public ProgramCategory Category { get; set; } = ProgramCategory.Other;
    public string Version { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string Icon { get; set; } = ""; // Base64 or URL
    public bool IsFreeware { get; set; } = true;
}
