namespace AdminAssistant.Core.Models;

public class DhcpReservation
{
    public string IpAddress { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ScopeId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
