using AdminAssistant.Core.Models;

namespace AdminAssistant.Core.Interfaces;

public interface IDhcpService
{
    Task<IEnumerable<DhcpScope>> GetScopesAsync();
    Task<IEnumerable<DhcpLease>> GetLeasesAsync(string scopeId);
    Task<IEnumerable<DhcpReservation>> GetReservationsAsync(string scopeId);
    Task<bool> AddReservationAsync(DhcpReservation reservation, string performedBy);
    Task<bool> RemoveReservationAsync(string scopeId, string ipAddress, string performedBy);
}
