using System.Collections.Concurrent;

namespace OFXApi.Services.ExchangeRateService;
public class ExchangeRateService : IExchangeRateService
{
    private readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<string, (decimal rate, DateTime expiry)> _cache = new();

    // OFX exchange rates for allowed currency pairs.
    private readonly Dictionary<string, decimal> _baseRates = new(StringComparer.OrdinalIgnoreCase)
    {
        { "AUD-USD", 0.6162m },
        { "AUD-INR", 52.8198m },
        { "AUD-PHP", 35.102m },
        { "USD-INR", 84.3258m },
        { "USD-PHP", 56.0393m },
        { "EUR-USD", 1.0148m },
        { "EUR-INR", 86.977m },
        { "EUR-PHP", 57.7993m }
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

