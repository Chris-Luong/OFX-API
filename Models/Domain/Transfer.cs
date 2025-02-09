namespace OFXApi.Models.Domain;

public class Transfer {
    public required Guid Id { get; init; }
    public required TransferStatus Status { get; set; }
    public required Guid QuoteId { get; set; }
    public required Payer Payer { get; set; }
    public required Recipient Recipient { get; set; }
    public required DateTime EstimatedDeliveryDate { get; set; }
}