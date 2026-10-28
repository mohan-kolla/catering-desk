namespace CateringDemo.Api.Services;

// Fetches BLS prices when the app starts and once a day after that.
// BLS publishes monthly, so a daily refresh is more than enough and keeps
// the app well under the API's daily request limit.
public class MarketPriceRefresher(
    IServiceScopeFactory scopeFactory,
    MarketPriceCache cache,
    IConfiguration config,
    ILogger<MarketPriceRefresher> logger) : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(24);
    private static readonly TimeSpan RetryInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiKey = config["BLS_API_KEY"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("BLS_API_KEY is not set. Using estimated ingredient prices only.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = RefreshInterval;
            try
            {
                using var scope = scopeFactory.CreateScope();
                var client = scope.ServiceProvider.GetRequiredService<BlsClient>();
                var prices = await client.FetchLatestAsync(BlsCatalog.AllSeriesIds(), apiKey, stoppingToken);
                cache.Update(prices, DateTime.UtcNow);
                logger.LogInformation("Loaded {Count} BLS price series.", prices.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // Keep serving the last good prices (or estimates) and try again soon.
                logger.LogError(ex, "BLS price refresh failed. Retrying in {Minutes} minutes.", RetryInterval.TotalMinutes);
                delay = RetryInterval;
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
