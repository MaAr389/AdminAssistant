using AdminAssistant.Core.Interfaces;
using AdminAssistant.Core.Models;
using AdminAssistant.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AdminAssistant.Services;

public class OuAccessService : IOuAccessService
{
    private readonly IConfiguration _config;
    private readonly AdminAssistantDbContext _dbContext;

    public OuAccessService(IConfiguration config, AdminAssistantDbContext dbContext)
    {
        _config = config;
        _dbContext = dbContext;
    }

    public bool CanAccessUser(string distinguishedName, bool isAdmin)
        => CanAccess(distinguishedName, isAdmin, PermissionAreas.UserManagement, "ActiveDirectory:KeyUserAllowedOUs", containsMatch: true);

    public bool CanAccessGroup(string distinguishedName, bool isAdmin)
        => CanAccess(distinguishedName, isAdmin, PermissionAreas.GroupManagement, "GroupManagement:AllowedOUs", containsMatch: true);

    public async Task<IReadOnlyList<OuPermission>> GetPermissionsAsync()
    {
        return await _dbContext.OuPermissions
            .AsNoTracking()
            .OrderBy(p => p.Area)
            .ThenBy(p => p.DistinguishedName)
            .ToListAsync();
    }

    public async Task AddPermissionAsync(string area, string distinguishedName)
    {
        if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(distinguishedName))
            return;

        var normalizedArea = NormalizeArea(area);
        var normalizedDn = distinguishedName.Trim();
        var normalizedDnForCompare = NormalizeDn(normalizedDn);

        var existingDns = await _dbContext.OuPermissions
            .AsNoTracking()
            .Select(p => new { p.Area, p.DistinguishedName })
            .ToListAsync();

        var exists = existingDns
            .Where(p => NormalizeArea(p.Area) == normalizedArea)
            .Select(p => p.DistinguishedName)
            .Any(dn => NormalizeDn(dn) == normalizedDnForCompare);
        if (exists)
            return;

        _dbContext.OuPermissions.Add(new OuPermission
        {
            Area = normalizedArea,
            DistinguishedName = normalizedDn,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> RemovePermissionAsync(int id)
    {
        var entity = await _dbContext.OuPermissions.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null)
            return false;

        _dbContext.OuPermissions.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private bool CanAccess(string distinguishedName, bool isAdmin, string area, string fallbackConfigPath, bool containsMatch)
    {
        if (isAdmin)
            return true;

        var normalizedArea = NormalizeArea(area);
        var normalizedDn = NormalizeDn(distinguishedName);
        if (string.IsNullOrWhiteSpace(normalizedDn))
            return false;

        var allowedOUs = LoadAllowedOus(normalizedArea, fallbackConfigPath)
            .Select(NormalizeDn)
            .Where(ou => !string.IsNullOrWhiteSpace(ou))
            .Distinct()
            .ToList();

        if (!allowedOUs.Any())
            return false;

        return containsMatch
            ? allowedOUs.Any(ou => normalizedDn.Contains(ou, StringComparison.OrdinalIgnoreCase))
            : allowedOUs.Any(ou => normalizedDn.EndsWith(ou, StringComparison.OrdinalIgnoreCase));
    }

    private List<string> LoadAllowedOus(string area, string fallbackConfigPath)
    {
        var dbEntries = _dbContext.OuPermissions
            .AsNoTracking()
            .Select(p => new { p.Area, p.DistinguishedName })
            .ToList()
            .Where(p => NormalizeArea(p.Area) == area)
            .Select(p => p.DistinguishedName)
            .ToList();

        if (dbEntries.Any())
            return dbEntries;

        return _config.GetSection(fallbackConfigPath)
            .Get<List<string>>()?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList() ?? new List<string>();
    }

    private static string NormalizeArea(string? area)
    {
        if (string.IsNullOrWhiteSpace(area))
            return string.Empty;

        var value = area.Trim();

        if (value.Equals(PermissionAreas.UserManagement, StringComparison.OrdinalIgnoreCase)
            || value.Equals("User", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Users", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Benutzerverwaltung", StringComparison.OrdinalIgnoreCase))
        {
            return PermissionAreas.UserManagement;
        }

        if (value.Equals(PermissionAreas.GroupManagement, StringComparison.OrdinalIgnoreCase)
            || value.Equals("Group", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Groups", StringComparison.OrdinalIgnoreCase)
            || value.Equals("Gruppenverwaltung", StringComparison.OrdinalIgnoreCase))
        {
            return PermissionAreas.GroupManagement;
        }

        return value;
    }

    private static string NormalizeDn(string? dn)
    {
        if (string.IsNullOrWhiteSpace(dn))
            return string.Empty;

        return string.Join(",",
            dn.Split(',', StringSplitOptions.RemoveEmptyEntries)
              .Select(part => part.Trim()));
    }
}
