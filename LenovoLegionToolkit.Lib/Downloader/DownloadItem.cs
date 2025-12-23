namespace LenovoLegionToolkit.Lib.Downloader;

public enum DownloadStatus
{
    Pending,
    Downloading,
    Paused,
    Completed,
    Failed,
    Cancelled
}

public class DownloadItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Program Program { get; set; } = new();
    public string SavePath { get; set; } = "";
    public long TotalBytes { get; set; } = 0;
    public long DownloadedBytes { get; set; } = 0;
    public DownloadStatus Status { get; set; } = DownloadStatus.Pending;
    public double ProgressPercent => TotalBytes > 0 ? (DownloadedBytes * 100.0) / TotalBytes : 0;
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime CompletedAt { get; set; } = DateTime.MinValue;
    public string ErrorMessage { get; set; } = "";

    public TimeSpan ElapsedTime => DateTime.Now - StartedAt;
    public TimeSpan EstimatedTimeRemaining
    {
        get
        {
            if (DownloadedBytes == 0 || ElapsedTime.TotalSeconds < 1)
                return TimeSpan.MaxValue;
            
            var bytesPerSecond = DownloadedBytes / ElapsedTime.TotalSeconds;
            var remainingBytes = TotalBytes - DownloadedBytes;
            var secondsRemaining = remainingBytes / bytesPerSecond;
            return TimeSpan.FromSeconds(secondsRemaining);
        }
    }
}
