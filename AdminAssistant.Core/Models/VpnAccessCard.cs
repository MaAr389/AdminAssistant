namespace AdminAssistant.Core.Models;

public class VpnAccessCard
{
    public int Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string? AssignedAdUser { get; set; }
    public string? Pin { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? Notes { get; set; }
}