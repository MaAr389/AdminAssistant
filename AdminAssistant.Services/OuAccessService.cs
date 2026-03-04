using AdminAssistant.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AdminAssistant.Services;

public class OuAccessService : IOuAccessService
{
    private readonly IConfiguration _config;

    public OuAccessService(IConfiguration config)
    {
        _config = config;
    }

    public bool CanAccessUser(string distinguishedName, bool isAdmin)
    {
        if (isAdmin) return true;

        var allowedOUs = _config.GetSection("ActiveDirectory:KeyUserAllowedOUs")
                                .Get<List<string>>();

        if (allowedOUs == null || !allowedOUs.Any()) return false;

        return allowedOUs.Any(ou =>
            distinguishedName.Contains(ou, StringComparison.OrdinalIgnoreCase));
    }

}
