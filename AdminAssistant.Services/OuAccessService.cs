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
        => CanAccess(distinguishedName, isAdmin, PermissionAreas.GroupManagement, "GroupManagement:AllowedOUs", containsMatch: false);

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

        var normalizedDn = distinguishedName.Trim();
        var exists = await _dbContext.OuPermissions.AnyAsync(p =>
            p.Area == area && p.DistinguishedName.ToLower() == normalizedDn.ToLower());

        if (exists)
            return;

        _dbContext.OuPermissions.Add(new OuPermission
        {
            Area = area,
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

        var allowedOUs = _dbContext.OuPermissions
            .AsNoTracking()
            .Where(p => p.Area == area)
            .Select(p => p.DistinguishedName)
            .ToList();

        if (!allowedOUs.Any())
        {
            allowedOUs = _config.GetSection(fallbackConfigPath)
                .Get<List<string>>() ?? new List<string>();
        }

        if (!allowedOUs.Any())
            return false;

        return containsMatch
            ? allowedOUs.Any(ou => distinguishedName.Contains(ou, StringComparison.OrdinalIgnoreCase))
            : allowedOUs.Any(ou => distinguishedName.EndsWith(ou, StringComparison.OrdinalIgnoreCase));
    }
}
