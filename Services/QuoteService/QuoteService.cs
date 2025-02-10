using OFXApi.Data;
using OFXApi.Models.Dtos.QuoteDtos;
using OFXApi.Models.Domain;
using OFXApi.Services.ExchangeRateService;

namespace OFXApi.Services.QuoteService;

public class QuoteService : IQuoteService
{
    // Supported currencies (can be adjusted or externalized)
    private static readonly HashSet<string> SupportedSellCurrencies = ["AUD", "USD", "EUR"];
    private static readonly HashSet<string> SupportedBuyCurrencies = ["USD", "INR", "PHP"];

    private readonly IExchangeRateService _exchangeRateService;

    public QuoteService(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    private static void ValidateCurrencies(QuoteRequest request, string sellCurrency, string buyCurrency)
    {
        if (!SupportedSellCurrencies.Contains(sellCurrency))
        {
            throw new ArgumentException($"Sell currency '{request.SellCurrency}' is not supported.");
        }
        if (!SupportedBuyCurrencies.Contains(buyCurrency))
        {
            throw new ArgumentException($"Buy currency '{request.BuyCurrency}' is not supported.");
        }

        if (sellCurrency == buyCurrency)
        {
            throw new ArgumentException("Sell and buy currencies cannot be identical.");
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }
    }

    public async Task<Quote> CreateQuoteAsync(QuoteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SellCurrency) ||
            string.IsNullOrWhiteSpace(request.BuyCurrency))
        {
            throw new ArgumentException("sellCurrency and buyCurrency are required.");
        }

        var sellCurrency = request.SellCurrency.ToUpper();
        var buyCurrency = request.BuyCurrency.ToUpper();

        ValidateCurrencies(request, sellCurrency, buyCurrency);

        decimal rate = await _exchangeRateService.GetExchangeRateAsync(sellCurrency, buyCurrency);
        decimal inverseRate = rate != 0 ? Math.Round(1 / rate, 6) : 0;
        decimal convertedAmount = Math.Round(request.Amount * rate, 2);

        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            SellCurrency = sellCurrency,
            BuyCurrency = buyCurrency,
            Amount = request.Amount,
            OfxRate = Math.Round(rate, 6),
            InverseOfxRate = inverseRate,
            ConvertedAmount = convertedAmount,
            CreatedAt = DateTime.UtcNow
        };

        MemoryStore.Quotes[quote.Id] = quote;
        return quote;
    }

    public Quote? GetQuote(Guid quoteId)
    {
        MemoryStore.Quotes.TryGetValue(quoteId, out var quote);
        return quote;
    }
}
