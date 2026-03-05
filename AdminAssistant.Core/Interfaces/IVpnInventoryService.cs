using AdminAssistant.Core.Models;

namespace AdminAssistant.Core.Interfaces;

public interface IVpnInventoryService
{
    Task<VpnInventoryOverview> GetOverviewAsync();
    Task<List<VpnSmartcardReader>> GetReadersAsync();
    Task<List<VpnAccessCard>> GetCardsAsync();
    Task<VpnSmartcardReader> AddReaderAsync(VpnSmartcardReader reader);
    Task<VpnSmartcardReader> UpdateReaderAsync(VpnSmartcardReader reader);
    Task DeleteReaderAsync(int id);
    Task<VpnAccessCard> AddCardAsync(VpnAccessCard card);
    Task<VpnAccessCard> UpdateCardAsync(VpnAccessCard card);
    Task DeleteCardAsync(int id);
}