namespace OFXApi.Models.Dtos.QuoteDtos;

public class QuoteRequest
{
    public required string SellCurrency { get; init; }
    public required string BuyCurrency { get; init; }
    public required decimal Amount { get; init; }
}