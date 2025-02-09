namespace OFXApi.Models.Domain;

public class Quote
{
    public required Guid Id { get; init; }
    public required string SellCurrency { get; init; }
    public required string BuyCurrency { get; init; }
    public required decimal Amount { get; init; }
    public required decimal OfxRate { get; init; }
    public required decimal InverseOfxRate { get; init; }
    public required decimal ConvertedAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}