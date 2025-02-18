namespace OFXApi.Models.Dtos.TransferDtos;

public class TransferDetails
{
    public required Guid QuoteId { get; init; }
    public required PayerRequest Payer { get; init; }
    public required RecipientRequest Recipient { get; init; }
}