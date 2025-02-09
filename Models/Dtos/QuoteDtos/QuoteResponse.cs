namespace OFXApi.Models.Dtos.QuoteDtos;

public class QuoteResponse
{
    public required Guid QuoteId { get; init; }
    public required decimal OfxRate { get; init; }
    public required decimal InverseOfxRate { get; init; }
    public required decimal ConvertedAmount { get; init; }
}