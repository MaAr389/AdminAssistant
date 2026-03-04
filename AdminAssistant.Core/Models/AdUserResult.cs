namespace AdminAssistant.Core.Models;

public class AdUserResult
{
    public string SamAccountName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool IsLockedOut { get; set; } = false;
    public DateTime? LastLogon { get; set; }

    //04032026
    public string UserPrincipalName { get; set; } = string.Empty;
}
