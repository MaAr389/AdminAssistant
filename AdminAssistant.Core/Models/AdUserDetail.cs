namespace AdminAssistant.Core.Models;

public class AdUserDetail
{
    public string SamAccountName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTime? LastLogon { get; set; }
    public DateTime? AccountExpires { get; set; }
    public string DistinguishedName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Office { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

}
