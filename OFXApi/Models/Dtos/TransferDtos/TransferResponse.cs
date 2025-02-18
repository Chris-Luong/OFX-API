namespace OFXApi.Models.Dtos.TransferDtos;

public class TransferResponse
{
    public required Guid TransferId { get; init; }
    public required string Status { get; init; }
    public required TransferDetails TransferDetails { get; init; }
    public required DateTime EstimatedDeliveryDate { get; init; }
}