namespace AdminAssistant.Core.Models;

public class AdGroupDetail
{
    public string Name { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<AdGroupMember> Members { get; set; } = new();
}

public class AdGroupMember
{
    public string SamAccountName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
}
