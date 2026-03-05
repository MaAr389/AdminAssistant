namespace AdminAssistant.Core.Models;

public class VpnSmartcardReader
{
    public int Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string? AssignedAdUser { get; set; }
    public string? Description { get; set; }
    public DateTime? AssignedAt { get; set; }
}