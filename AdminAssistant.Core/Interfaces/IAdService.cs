using AdminAssistant.Core.Models;

namespace AdminAssistant.Core.Interfaces;

public interface IAdService
{
    Task<bool> ResetPasswordAsync(string targetUsername, string newPassword, string performedBy);
    Task<IEnumerable<AdUserResult>> SearchUsersAsync(string searchTerm);
    Task<AdUserDetail?> GetUserAsync(string samAccountName);
    Task<bool> SetAccountEnabledAsync(string samAccountName, bool enabled, string performedBy);
    Task<bool> SetMustChangePasswordAsync(string samAccountName, bool mustChange, string performedBy);
    Task<bool> UpdateUserAttributesAsync(
        string samAccountName, string email, string department, string title,
        string description, string office, string phoneNumber, string website,
        string street, string city, string state, string postalCode, string country,
        string performedBy);

    // Gruppenverwaltung
    Task<IEnumerable<AdGroupResult>> SearchGroupsAsync(string searchTerm, bool isAdmin = false);
    Task<IEnumerable<AdGroupResult>> GetAllGroupsAsync(bool isAdmin = false);
    Task<AdGroupDetail?> GetGroupAsync(string groupName);
    Task<bool> AddGroupMemberAsync(string groupName, string samAccountName, string performedBy);
    Task<bool> RemoveGroupMemberAsync(string groupName, string samAccountName, string performedBy);
    Task<IEnumerable<AdUserResult>> GetAllUsersAsync();
}
