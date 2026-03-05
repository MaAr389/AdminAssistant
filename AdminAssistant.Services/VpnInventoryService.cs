using AdminAssistant.Core.Interfaces;
using AdminAssistant.Core.Models;
using AdminAssistant.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AdminAssistant.Data;

public class VpnInventoryService : IVpnInventoryService
{
    private const int DefaultTotal = 150;
    private readonly AdminAssistantDbContext _db;

    public VpnInventoryService(AdminAssistantDbContext db)
    {
        _db = db;
    }

    public async Task<VpnInventoryOverview> GetOverviewAsync()
    {
        var assignedReaders = await _db.VpnSmartcardReaders.CountAsync(x => !string.IsNullOrWhiteSpace(x.AssignedAdUser));
        var assignedCards = await _db.VpnAccessCards.CountAsync(x => !string.IsNullOrWhiteSpace(x.AssignedAdUser));

        return new VpnInventoryOverview
        {
            TotalLicenses = DefaultTotal,
            TotalReaders = DefaultTotal,
            TotalCards = DefaultTotal,
            AssignedReaders = assignedReaders,
            AssignedCards = assignedCards
        };
    }

    public Task<List<VpnSmartcardReader>> GetReadersAsync() =>
        _db.VpnSmartcardReaders.OrderBy(x => x.SerialNumber).ToListAsync();

    public Task<List<VpnAccessCard>> GetCardsAsync() =>
        _db.VpnAccessCards.OrderBy(x => x.CardNumber).ToListAsync();

    public async Task<VpnSmartcardReader> AddReaderAsync(VpnSmartcardReader reader)
    {
        NormalizeReader(reader);
        _db.VpnSmartcardReaders.Add(reader);
        await _db.SaveChangesAsync();
        return reader;
    }

    public async Task<VpnSmartcardReader> UpdateReaderAsync(VpnSmartcardReader reader)
    {
        NormalizeReader(reader);
        _db.VpnSmartcardReaders.Update(reader);
        await _db.SaveChangesAsync();
        return reader;
    }

    public async Task DeleteReaderAsync(int id)
    {
        var entity = await _db.VpnSmartcardReaders.FindAsync(id);
        if (entity == null)
            return;

        _db.VpnSmartcardReaders.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<VpnAccessCard> AddCardAsync(VpnAccessCard card)
    {
        NormalizeCard(card);
        _db.VpnAccessCards.Add(card);
        await _db.SaveChangesAsync();
        return card;
    }

    public async Task<VpnAccessCard> UpdateCardAsync(VpnAccessCard card)
    {
        NormalizeCard(card);
        _db.VpnAccessCards.Update(card);
        await _db.SaveChangesAsync();
        return card;
    }

    public async Task DeleteCardAsync(int id)
    {
        var entity = await _db.VpnAccessCards.FindAsync(id);
        if (entity == null)
            return;

        _db.VpnAccessCards.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private static void NormalizeReader(VpnSmartcardReader reader)
    {
        reader.SerialNumber = reader.SerialNumber.Trim();
        reader.AssignedAdUser = NormalizeString(reader.AssignedAdUser);
        reader.Description = NormalizeString(reader.Description);
    }

    private static void NormalizeCard(VpnAccessCard card)
    {
        card.CardNumber = card.CardNumber.Trim();
        card.AssignedAdUser = NormalizeString(card.AssignedAdUser);
        card.Pin = NormalizeString(card.Pin);
        card.Notes = NormalizeString(card.Notes);
    }

    private static string? NormalizeString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        return input.Trim();
    }
}