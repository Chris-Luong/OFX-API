using System.Collections.Concurrent;

namespace OFXApi.Services.ExchangeRateService;
public class ExchangeRateService : IExchangeRateService
{
    private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);
    private readonly ConcurrentDictionary<string, (decimal rate, DateTime expiry)> _cache = new();

    // Simulated exchange rates for allowed currency pairs.
    private readonly Dictionary<string, decimal> _baseRates = new(StringComparer.OrdinalIgnoreCase)
    {
        { "AUD-USD", 0.768333m },
        { "AUD-INR", 55.0m },
        { "AUD-PHP", 36.5m },
        { "USD-INR", 72.0m },
        { "USD-PHP", 50.0m },
        { "EUR-USD", 1.1m },
        { "EUR-INR", 80.0m },
        { "EUR-PHP", 60.0m }
    };

    public Task<decimal> GetExchangeRateAsync(string sellCurrency, string buyCurrency)
    {
        string key = $"{sellCurrency.ToUpper()}-{buyCurrency.ToUpper()}";

        if (_cache.TryGetValue(key, out var entry) && entry.expiry > DateTime.UtcNow)
        {
            return Task.FromResult(entry.rate);
        }

        if (!_baseRates.TryGetValue(key, out var rate))
        {
            throw new KeyNotFoundException("Exchange rate not available for the provided currency pair.");
        }

        _cache[key] = (rate, DateTime.UtcNow.Add(CacheDuration));
        return Task.FromResult(rate);
    }
}

