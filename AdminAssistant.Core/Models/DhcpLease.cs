namespace AdminAssistant.Core.Models;

public class DhcpLease
{
    public string IpAddress { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public DateTime? LeaseExpires { get; set; }
    public string ScopeId { get; set; } = string.Empty;
    public string AddressState { get; set; } = string.Empty;
}
