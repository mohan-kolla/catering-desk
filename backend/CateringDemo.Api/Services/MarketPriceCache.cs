using System.Collections.Concurrent;

namespace CateringDemo.Api.Services;

public record MarketPrice(decimal PricePerUnit, string Period);

// Latest BLS prices held in memory, keyed by series ID. Kept outside the database
// so a demo reset does not wipe prices or trigger extra calls to the BLS API.
public class MarketPriceCache
{
    private readonly ConcurrentDictionary<string, MarketPrice> _prices = new();

    public DateTime? LastUpdatedUtc { get; private set; }

    public void Update(IReadOnlyDictionary<string, MarketPrice> prices, DateTime fetchedAtUtc)
    {
        foreach (var (seriesId, price) in prices)
        {
            _prices[seriesId] = price;
        }
        LastUpdatedUtc = fetchedAtUtc;
    }

    public MarketPrice? Get(string region, string itemCode) =>
        _prices.TryGetValue(BlsCatalog.SeriesId(region, itemCode), out var price) ? price : null;
}
