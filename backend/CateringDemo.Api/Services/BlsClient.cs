using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace CateringDemo.Api.Services;

// Calls the BLS Public Data API v2: https://www.bls.gov/developers/
public class BlsClient(HttpClient http)
{
    // BLS allows up to 50 series per request with a registration key.
    private const int MaxSeriesPerRequest = 50;

    public async Task<Dictionary<string, MarketPrice>> FetchLatestAsync(
        IEnumerable<string> seriesIds, string apiKey, CancellationToken ct)
    {
        var results = new Dictionary<string, MarketPrice>();
        var thisYear = DateTime.UtcNow.Year;

        foreach (var batch in seriesIds.Chunk(MaxSeriesPerRequest))
        {
            var body = new
            {
                seriesid = batch,
                startyear = (thisYear - 1).ToString(CultureInfo.InvariantCulture),
                endyear = thisYear.ToString(CultureInfo.InvariantCulture),
                registrationkey = apiKey
            };

            using var response = await http.PostAsJsonAsync("publicAPI/v2/timeseries/data/", body, ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);

            foreach (var (seriesId, price) in ParseLatest(json))
            {
                results[seriesId] = price;
            }
        }

        return results;
    }

    // Pulls the most recent monthly value from each series in a BLS response.
    // BLS marks missing months with "-", so those are skipped.
    public static Dictionary<string, MarketPrice> ParseLatest(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var status = root.TryGetProperty("status", out var s) ? s.GetString() : null;
        if (status != "REQUEST_SUCCEEDED")
        {
            var message = root.TryGetProperty("message", out var m) ? m.ToString() : "no message";
            throw new InvalidOperationException($"BLS request failed ({status}): {message}");
        }

        var results = new Dictionary<string, MarketPrice>();
        if (!root.TryGetProperty("Results", out var res) || !res.TryGetProperty("series", out var seriesList))
        {
            return results;
        }

        foreach (var series in seriesList.EnumerateArray())
        {
            var seriesId = series.GetProperty("seriesID").GetString();
            if (seriesId is null || !series.TryGetProperty("data", out var data)) continue;

            MarketPrice? latest = null;
            var latestKey = "";

            foreach (var point in data.EnumerateArray())
            {
                var year = point.GetProperty("year").GetString() ?? "";
                var period = point.GetProperty("period").GetString() ?? "";
                var value = point.GetProperty("value").GetString();

                // Monthly periods are M01 to M12. M13 is an annual average, not a month.
                if (!period.StartsWith('M') || period == "M13") continue;
                if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var price)) continue;

                var key = $"{year}-{period[1..]}";
                if (string.CompareOrdinal(key, latestKey) > 0)
                {
                    latestKey = key;
                    latest = new MarketPrice(price, key);
                }
            }

            if (latest is not null) results[seriesId] = latest;
        }

        return results;
    }
}
