using System;
using System.DirectoryServices.AccountManagement;
using AdminAssistant.Core.Enums;
using AdminAssistant.Core.Interfaces;
using AdminAssistant.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdminAssistant.Services;

public class AdService : IAdService
{
    private readonly IConfiguration _config;
    private readonly IAuditLogService _auditLog;
    private readonly ILogger<AdService> _logger;

    public AdService(IConfiguration config, IAuditLogService auditLog, ILogger<AdService> logger)
    {
        _config = config;
        _auditLog = auditLog;
        _logger = logger;
    }

    // ─── Hilfsmethoden ───────────────────────────────────────────────────────

    private (string domain, string serviceUser, string servicePass) GetAdConfig() => (
        _config["ActiveDirectory:Domain"]!,
        _config["ActiveDirectory:ServiceUser"]!,
        _config["ActiveDirectory:ServicePassword"]!
    );

    private static string Get(System.DirectoryServices.DirectoryEntry entry, string prop)
        => entry?.Properties[prop]?.Value?.ToString() ?? string.Empty;

    private static void Set(System.DirectoryServices.DirectoryEntry entry, string prop, string val)
    {
        if (string.IsNullOrWhiteSpace(val))
            entry.Properties[prop].Clear();
        else
            entry.Properties[prop].Value = val;
    }

    private async Task WriteAuditLog(
        string performedBy, string targetUser, string executedVia,
        bool success, string? error = null,
        AuditAction action = AuditAction.PasswordReset,
        string? details = null)
    {
        await _auditLog.LogAsync(new AuditLogEntry
        {
            Timestamp = DateTime.UtcNow,
            Action = action,
            PerformedBy = performedBy,
            TargetUser = targetUser,
            ExecutedVia = executedVia,
            Success = success,
            ErrorMessage = error,
            Details = details
        });
    }

    // ─── Password Reset ───────────────────────────────────────────────────────

    public async Task<bool> ResetPasswordAsync(string targetUsername, string newPassword, string performedBy)
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        try
        {
            using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
            using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, targetUsername);

            if (user == null)
            {
                _logger.LogWarning("Benutzer {User} nicht gefunden.", targetUsername);
                await WriteAuditLog(performedBy, targetUsername, serviceUser, false, "Benutzer nicht gefunden.");
                return false;
            }

            user.SetPassword(newPassword);
            user.ExpirePasswordNow();
            user.Save();

            await WriteAuditLog(performedBy, targetUsername, serviceUser, true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Passwort-Reset fuer {User}", targetUsername);
            await WriteAuditLog(performedBy, targetUsername, serviceUser, false, ex.Message);
            return false;
        }
    }

    // ─── Benutzer suchen ─────────────────────────────────────────────────────

    public async Task<IEnumerable<AdUserResult>> GetAllUsersAsync()
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        return await Task.Run(() =>
        {
            using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
            using var searcher = new PrincipalSearcher(new UserPrincipal(context));

            return searcher.FindAll()
                .OfType<UserPrincipal>()
                .Select(u => new AdUserResult
                {
                    SamAccountName = u.SamAccountName ?? string.Empty,
                    DisplayName = u.DisplayName ?? u.SamAccountName ?? string.Empty,
                    Email = u.EmailAddress ?? string.Empty,
                    //04032026
                    UserPrincipalName = Get((u.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry)!, "userPrincipalName"),
                    DistinguishedName = u.DistinguishedName ?? string.Empty,
                    IsEnabled = u.Enabled ?? false,
                    IsLockedOut = u.IsAccountLockedOut(),
                    LastLogon = u.LastLogon
                })
                .OrderBy(u => u.DisplayName)
                .ToList();
        });
    }


    public async Task<IEnumerable<AdUserResult>> SearchUsersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            return Enumerable.Empty<AdUserResult>();

        var (domain, serviceUser, servicePass) = GetAdConfig();

        return await Task.Run(() =>
        {
            using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
            using var searcher = new PrincipalSearcher(new UserPrincipal(context)
            {
                //SamAccountName = $"*{searchTerm}*",
                //Enabled = true
                SamAccountName = $"*{searchTerm}*"
            });

            return searcher.FindAll()
                .OfType<UserPrincipal>()
                .Take(10)
                .Select(u => new AdUserResult
                {
                    SamAccountName = u.SamAccountName ?? string.Empty,
                    DisplayName = u.DisplayName ?? u.SamAccountName ?? string.Empty,
                    Email = u.EmailAddress ?? string.Empty,
                    //04032026
                    UserPrincipalName = Get((u.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry)!, "userPrincipalName"),
                    DistinguishedName = u.DistinguishedName ?? string.Empty,
                    IsEnabled = u.Enabled ?? false,
                    IsLockedOut = u.IsAccountLockedOut(),
                    LastLogon = u.LastLogon
                })
                .ToList();
        });
    }


    // ─── Benutzer laden ──────────────────────────────────────────────────────

    public async Task<AdUserDetail?> GetUserAsync(string samAccountName)
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        return await Task.Run(() =>
        {
            using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
            using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);

            if (user == null) return null;

            var entry = user.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry;

            var pwdLastSet = entry?.Properties["pwdLastSet"]?.Value;
            bool mustChange = false;
            if (pwdLastSet != null)
            {
                var t = pwdLastSet.GetType();
                var high = (int)t.InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, pwdLastSet, null)!;
                var low = (int)t.InvokeMember("LowPart", System.Reflection.BindingFlags.GetProperty, null, pwdLastSet, null)!;
                mustChange = (((long)high << 32) | (uint)low) == 0;
            }

            return new AdUserDetail
            {
                SamAccountName = user.SamAccountName ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                Email = user.EmailAddress ?? string.Empty,
                Department = Get(entry!, "department"),
                Title = Get(entry!, "title"),
                IsEnabled = user.Enabled ?? false,
                MustChangePassword = mustChange,
                LastLogon = user.LastLogon,
                AccountExpires = user.AccountExpirationDate,
                DistinguishedName = user.DistinguishedName ?? string.Empty,
                Description = Get(entry!, "description"),
                Office = Get(entry!, "physicalDeliveryOfficeName"),
                PhoneNumber = Get(entry!, "telephoneNumber"),
                Website = Get(entry!, "wWWHomePage"),
                Street = Get(entry!, "streetAddress"),
                City = Get(entry!, "l"),
                State = Get(entry!, "st"),
                PostalCode = Get(entry!, "postalCode"),
                Country = Get(entry!, "co")
            };
        });
    }

    // ─── Konto aktivieren / deaktivieren ─────────────────────────────────────

    public async Task<bool> SetAccountEnabledAsync(string samAccountName, bool enabled, string performedBy)
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        try
        {
            await Task.Run(() =>
            {
                using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
                using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName)
                    ?? throw new Exception("Benutzer nicht gefunden.");

                user.Enabled = enabled;
                user.Save();
            });

            await WriteAuditLog(performedBy, samAccountName, serviceUser, true,
                action: enabled ? AuditAction.AccountEnable : AuditAction.AccountDisable);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aendern des Kontostatus fuer {User}", samAccountName);
            await WriteAuditLog(performedBy, samAccountName, serviceUser, false, ex.Message,
                action: enabled ? AuditAction.AccountEnable : AuditAction.AccountDisable);
            return false;
        }
    }

    // ─── Passwort bei naechster Anmeldung aendern ────────────────────────────

    public async Task<bool> SetMustChangePasswordAsync(string samAccountName, bool mustChange, string performedBy)
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        try
        {
            await Task.Run(() =>
            {
                using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
                using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName)
                    ?? throw new Exception("Benutzer nicht gefunden.");

                if (mustChange)
                {
                    user.ExpirePasswordNow();
                }
                else
                {
                    var entry = user.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry;
                    if (entry != null)
                    {
                        entry.Properties["pwdLastSet"].Value = -1;
                        entry.CommitChanges();
                    }
                }

                user.Save();
            });

            await WriteAuditLog(performedBy, samAccountName, serviceUser, true,
                action: AuditAction.PasswordReset);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler bei MustChangePassword fuer {User}", samAccountName);
            await WriteAuditLog(performedBy, samAccountName, serviceUser, false, ex.Message,
                action: AuditAction.PasswordReset);
            return false;
        }
    }

    // ─── Attribute aktualisieren ─────────────────────────────────────────────

    public async Task<bool> UpdateUserAttributesAsync(
        string samAccountName,
        string email, string department, string title,
        string description, string office, string phoneNumber, string website,
        string street, string city, string state, string postalCode, string country,
        string performedBy)
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        try
        {
            var changes = new List<string>();

            await Task.Run(() =>
            {
                using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
                using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName)
                    ?? throw new Exception("Benutzer nicht gefunden.");

                var entry = user.GetUnderlyingObject() as System.DirectoryServices.DirectoryEntry
                    ?? throw new Exception("DirectoryEntry nicht gefunden.");

                void Track(string prop, string label, string newVal)
                {
                    var oldVal = entry.Properties[prop]?.Value?.ToString() ?? string.Empty;
                    if (!string.Equals(oldVal, newVal, StringComparison.OrdinalIgnoreCase))
                        changes.Add(label + ": " + oldVal + " -> " + newVal);
                    Set(entry, prop, newVal);
                }

                Track("mail", "E-Mail", email);
                Track("department", "Abteilung", department);
                Track("title", "Position", title);
                Track("description", "Beschreibung", description);
                Track("physicalDeliveryOfficeName", "Buero", office);
                Track("telephoneNumber", "Rufnummer", phoneNumber);
                Track("wWWHomePage", "Webseite", website);
                Track("streetAddress", "Strasse", street);
                Track("l", "Ort", city);
                Track("st", "Bundesland", state);
                Track("postalCode", "PLZ", postalCode);
                Track("co", "Land", country);

                entry.CommitChanges();
            });

            await WriteAuditLog(performedBy, samAccountName, serviceUser, true,
                details: changes.Any() ? string.Join(" | ", changes) : "Keine Aenderungen",
                action: AuditAction.UserAttributeUpdate);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Aktualisieren der Attribute fuer {User}", samAccountName);
            await WriteAuditLog(performedBy, samAccountName, serviceUser, false, ex.Message,
                action: AuditAction.UserAttributeUpdate);
            return false;
        }
    }

    // ─── Gruppen suchen ──────────────────────────────────────────────────────

    public async Task<IEnumerable<AdGroupResult>> GetAllGroupsAsync(bool isAdmin = false)
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        return await Task.Run(() =>
        {
            using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
            using var searcher = new PrincipalSearcher(new GroupPrincipal(context));

            var results = searcher.FindAll()
                .OfType<GroupPrincipal>()
                .Take(200)
                .Select(g => new AdGroupResult
                {
                    Name = g.Name ?? string.Empty,
                    DistinguishedName = g.DistinguishedName ?? string.Empty,
                    Description = g.Description ?? string.Empty
                })
                .OrderBy(g => g.Name)
                .ToList();

            if (isAdmin)
                return results;

            var allowedOUs = _config.GetSection("GroupManagement:AllowedOUs")
                                    .Get<List<string>>() ?? new();

            return results.Where(g => allowedOUs.Any(ou =>
                g.DistinguishedName.EndsWith(ou, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        });
    }


    public async Task<IEnumerable<AdGroupResult>> SearchGroupsAsync(string searchTerm, bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            return Enumerable.Empty<AdGroupResult>();

        var (domain, serviceUser, servicePass) = GetAdConfig();

        return await Task.Run(() =>
        {
            using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
            using var searcher = new PrincipalSearcher(new GroupPrincipal(context)
            {
                Name = $"*{searchTerm}*"
            });

            var results = searcher.FindAll()
                .OfType<GroupPrincipal>()
                .Take(50)
                .Select(g => new AdGroupResult
                {
                    Name = g.Name ?? string.Empty,
                    DistinguishedName = g.DistinguishedName ?? string.Empty,
                    Description = g.Description ?? string.Empty
                })
                .ToList();

            if (isAdmin)
                return results;

            var allowedOUs = _config.GetSection("GroupManagement:AllowedOUs")
                                    .Get<List<string>>() ?? new();

            return results.Where(g => allowedOUs.Any(ou =>
                g.DistinguishedName.EndsWith(ou, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        });
    }

    // ─── Gruppe laden ────────────────────────────────────────────────────────

    public async Task<AdGroupDetail?> GetGroupAsync(string groupName)
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        return await Task.Run(() =>
        {
            using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
            using var group = GroupPrincipal.FindByIdentity(context, groupName);

            if (group == null) return null;

            var members = group.GetMembers(false)
                .OfType<UserPrincipal>()
                .Select(u => new AdGroupMember
                {
                    SamAccountName = u.SamAccountName ?? string.Empty,
                    DisplayName = u.DisplayName ?? u.SamAccountName ?? string.Empty,
                    DistinguishedName = u.DistinguishedName ?? string.Empty
                })
                .OrderBy(m => m.DisplayName)
                .ToList();

            return new AdGroupDetail
            {
                Name = group.Name ?? string.Empty,
                DistinguishedName = group.DistinguishedName ?? string.Empty,
                Description = group.Description ?? string.Empty,
                Members = members
            };
        });
    }

    // ─── Mitglied hinzufuegen ────────────────────────────────────────────────

    public async Task<bool> AddGroupMemberAsync(string groupName, string samAccountName, string performedBy)
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        try
        {
            await Task.Run(() =>
            {
                using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
                using var group = GroupPrincipal.FindByIdentity(context, groupName)
                    ?? throw new Exception("Gruppe nicht gefunden.");
                using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName)
                    ?? throw new Exception("Benutzer nicht gefunden.");

                if (group.Members.Contains(user))
                    throw new Exception(samAccountName + " ist bereits Mitglied.");

                group.Members.Add(user);
                group.Save();
            });

            await WriteAuditLog(performedBy, samAccountName, serviceUser, true,
                details: "Gruppe: " + groupName,
                action: AuditAction.GroupMemberAdd);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Hinzufuegen von {User} zu {Group}", samAccountName, groupName);
            await WriteAuditLog(performedBy, samAccountName, serviceUser, false, ex.Message,
                action: AuditAction.GroupMemberAdd);
            return false;
        }
    }

    // ─── Mitglied entfernen ──────────────────────────────────────────────────

    public async Task<bool> RemoveGroupMemberAsync(string groupName, string samAccountName, string performedBy)
    {
        var (domain, serviceUser, servicePass) = GetAdConfig();

        try
        {
            await Task.Run(() =>
            {
                using var context = new PrincipalContext(ContextType.Domain, domain, serviceUser, servicePass);
                using var group = GroupPrincipal.FindByIdentity(context, groupName)
                    ?? throw new Exception("Gruppe nicht gefunden.");
                using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName)
                    ?? throw new Exception("Benutzer nicht gefunden.");

                group.Members.Remove(user);
                group.Save();
            });

            await WriteAuditLog(performedBy, samAccountName, serviceUser, true,
                details: "Gruppe: " + groupName,
                action: AuditAction.GroupMemberRemove);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fehler beim Entfernen von {User} aus {Group}", samAccountName, groupName);
            await WriteAuditLog(performedBy, samAccountName, serviceUser, false, ex.Message,
                action: AuditAction.GroupMemberRemove);
            return false;
        }
    }
}
