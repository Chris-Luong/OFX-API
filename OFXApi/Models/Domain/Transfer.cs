namespace OFXApi.Models.Domain;

public class Transfer {
    public required Guid Id { get; init; }
    public required TransferStatus Status { get; set; }
    public required Guid QuoteId { get; init; }
    public required Payer Payer { get; init; }
    public required Recipient Recipient { get; init; }
    public required DateTime EstimatedDeliveryDate { get; init; }
}