using System.Management.Automation;
using AdminAssistant.Core.Interfaces;
using AdminAssistant.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdminAssistant.Services;

public class DhcpService : IDhcpService
{
    private readonly IConfiguration _config;
    private readonly ILogger<DhcpService> _logger;

    public DhcpService(IConfiguration config, ILogger<DhcpService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private string GetDhcpServer() =>
        _config["Dhcp:Server"] ?? "lpbkadsrv002.ad.lpbk-mv.de";

    private PowerShell CreatePs()
    {
        var ps = PowerShell.Create();

        ps.AddCommand("Import-Module").AddArgument("DhcpServer");
        ps.Invoke();

        if (ps.HadErrors)
        {
            _logger.LogError("DhcpServer-Modul konnte nicht geladen werden: {Errors}",
                string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
        }

        ps.Commands.Clear();
        return ps;
    }

    private void AddRemoteServerParameter(PowerShell ps)
    {
        ps.AddParameter("ComputerName", GetDhcpServer());
    }

    private static string FormatMac(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var clean = new string(raw.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (clean.Length != 12) return raw;
        return string.Join("-", Enumerable.Range(0, 6).Select(i => clean.Substring(i * 2, 2)));
    }

    public async Task<IEnumerable<DhcpScope>> GetScopesAsync()
    {
        return await Task.Run(() =>
        {
            using var ps = CreatePs();
            ps.AddCommand("Get-DhcpServerv4Scope");
            AddRemoteServerParameter(ps);

            var results = ps.Invoke();

            if (ps.HadErrors)
            {
                _logger.LogError("Get-DhcpServerv4Scope Fehler: {Errors}",
                    string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
                return Enumerable.Empty<DhcpScope>();
            }

            return results.Select(r =>
            {
                dynamic d = r.BaseObject;

                return new DhcpScope
                {
                    ScopeId = d.ScopeId.ToString(),
                    Name = d.Name,
                    SubnetMask = d.SubnetMask.ToString(),
                    StartRange = d.StartRange.ToString(),
                    EndRange = d.EndRange.ToString(),
                    TotalAddresses = 0,
                    UsedAddresses = 0,
                    FreeAddresses = 0,
                    State = d.State.ToString()
                };
            }).ToList();
        });
    }

    public async Task<IEnumerable<DhcpLease>> GetLeasesAsync(string scopeId)
    {
        return await Task.Run(() =>
        {
            using var ps = CreatePs();
            ps.AddCommand("Get-DhcpServerv4Lease")
              .AddParameter("ScopeId", scopeId);
            AddRemoteServerParameter(ps);

            var results = ps.Invoke();

            if (ps.HadErrors)
            {
                _logger.LogError("Get-DhcpServerv4Lease Fehler (Scope {Scope}): {Errors}",
                    scopeId, string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
                return Enumerable.Empty<DhcpLease>();
            }

            return results.Select(r =>
            {
                dynamic d = r.BaseObject;
                return new DhcpLease
                {
                    IpAddress = d.IPAddress.ToString(),
                    MacAddress = FormatMac(d.ClientId.ToString()),
                    Hostname = d.HostName,
                    LeaseExpires = d.LeaseExpiryTime,
                    ScopeId = d.ScopeId.ToString(),
                    AddressState = d.AddressState.ToString()
                };
            }).ToList();
        });
    }

    public async Task<bool> RemoveLeaseAsync(string scopeId, string ipAddress, string performedBy)
    {
        return await Task.Run(() =>
        {
            using var ps = CreatePs();
            ps.AddCommand("Remove-DhcpServerv4Lease")
              .AddParameter("ScopeId", scopeId)
              .AddParameter("IPAddress", ipAddress)
              .AddParameter("Confirm", false);
            AddRemoteServerParameter(ps);

            ps.Invoke();

            if (ps.HadErrors)
            {
                _logger.LogError(
                    "Remove-DhcpServerv4Lease Fehler (Scope {Scope}, IP {Ip}): {Errors}",
                    scopeId, ipAddress,
                    string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
                return false;
            }

            return true;
        });
    }

    public async Task<IEnumerable<DhcpReservation>> GetReservationsAsync(string scopeId)
    {
        return await Task.Run(() =>
        {
            using var ps = CreatePs();
            ps.AddCommand("Get-DhcpServerv4Reservation")
              .AddParameter("ScopeId", scopeId);
            AddRemoteServerParameter(ps);

            var results = ps.Invoke();

            if (ps.HadErrors)
            {
                _logger.LogError("Get-DhcpServerv4Reservation Fehler (Scope {Scope}): {Errors}",
                    scopeId, string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
                return Enumerable.Empty<DhcpReservation>();
            }

            return results.Select(r =>
            {
                dynamic d = r.BaseObject;
                return new DhcpReservation
                {
                    IpAddress = d.IPAddress.ToString(),
                    MacAddress = FormatMac(d.ClientId.ToString()),
                    Name = d.Name,
                    ScopeId = d.ScopeId.ToString(),
                    Description = d.Description
                };
            }).ToList();
        });
    }

    public async Task<bool> AddReservationAsync(DhcpReservation reservation, string performedBy)
    {
        return await Task.Run(() =>
        {
            using var ps = CreatePs();
            ps.AddCommand("Add-DhcpServerv4Reservation")
              .AddParameter("ScopeId", reservation.ScopeId)
              .AddParameter("IPAddress", reservation.IpAddress)
              .AddParameter("ClientId", reservation.MacAddress)
              .AddParameter("Name", reservation.Name)
              .AddParameter("Description", reservation.Description ?? string.Empty);
            AddRemoteServerParameter(ps);

            ps.Invoke();

            if (ps.HadErrors)
            {
                _logger.LogError(
                    "Add-DhcpServerv4Reservation Fehler (Scope {Scope}, IP {Ip}): {Errors}",
                    reservation.ScopeId, reservation.IpAddress,
                    string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
                return false;
            }

            return true;
        });
    }

    public async Task<bool> RemoveReservationAsync(string scopeId, string ipAddress, string performedBy)
    {
        return await Task.Run(() =>
        {
            using var ps = CreatePs();
            ps.AddCommand("Remove-DhcpServerv4Reservation")
              .AddParameter("ScopeId", scopeId)
              .AddParameter("IPAddress", ipAddress)
              .AddParameter("Confirm", false);
            AddRemoteServerParameter(ps);

            ps.Invoke();

            if (ps.HadErrors)
            {
                _logger.LogError(
                    "Remove-DhcpServerv4Reservation Fehler (Scope {Scope}, IP {Ip}): {Errors}",
                    scopeId, ipAddress,
                    string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
                return false;
            }

            return true;
        });
    }
}
