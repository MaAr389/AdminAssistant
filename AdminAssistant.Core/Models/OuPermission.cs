namespace AdminAssistant.Core.Models;

public class OuPermission
{
    public int Id { get; set; }
    public string Area { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public static class PermissionAreas
{
    public const string UserManagement = "UserManagement";
    public const string GroupManagement = "GroupManagement";
}
