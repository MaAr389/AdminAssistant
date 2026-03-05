namespace AdminAssistant.Core.Models;

public class DatabaseStatistics
{
    public required string DatabasePath { get; set; }
    public bool Exists { get; set; }
    public long SizeBytes { get; set; }
    public DateTime? LastModifiedUtc { get; set; }
    public Dictionary<string, int> TableRowCounts { get; set; } = new();
}
