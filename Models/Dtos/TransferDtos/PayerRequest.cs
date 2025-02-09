namespace OFXApi.Models.Dtos.TransferDtos;
public class PayerRequest
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string TransferReason { get; init; }
}
