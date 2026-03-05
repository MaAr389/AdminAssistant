namespace AdminAssistant.Core.Models;

public sealed class VpnInventoryOverview
{
    public int TotalLicenses { get; init; }
    public int TotalReaders { get; init; }
    public int TotalCards { get; init; }
    public int AssignedReaders { get; init; }
    public int AssignedCards { get; init; }

    public int AvailableReaders => TotalReaders - AssignedReaders;
    public int AvailableCards => TotalCards - AssignedCards;
    public int AvailableLicenses => TotalLicenses - Math.Max(AssignedReaders, AssignedCards);
}