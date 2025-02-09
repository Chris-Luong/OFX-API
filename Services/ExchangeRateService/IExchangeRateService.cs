namespace OFXApi.Services.ExchangeRateService;

public interface IExchangeRateService
{
    Task<decimal> GetExchangeRateAsync(string sellCurrency, string buyCurrency);
}