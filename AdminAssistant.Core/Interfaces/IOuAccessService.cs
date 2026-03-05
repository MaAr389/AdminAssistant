namespace AdminAssistant.Core.Interfaces;

public interface IOuAccessService
{
    bool CanAccessUser(string distinguishedName, bool isAdmin);
    bool CanAccessGroup(string distinguishedName, bool isAdmin);
    Task<IReadOnlyList<Models.OuPermission>> GetPermissionsAsync();
    Task AddPermissionAsync(string area, string distinguishedName, string performedBy);
    Task<bool> RemovePermissionAsync(int id, string performedBy);
}
