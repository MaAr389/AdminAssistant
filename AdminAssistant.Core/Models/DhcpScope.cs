namespace AdminAssistant.Core.Models;

public class DhcpScope
{
    public string ScopeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SubnetMask { get; set; } = string.Empty;
    public string StartRange { get; set; } = string.Empty;
    public string EndRange { get; set; } = string.Empty;
    public int TotalAddresses { get; set; }
    public int UsedAddresses { get; set; }
    public int FreeAddresses { get; set; }
    public string State { get; set; } = string.Empty;
}
