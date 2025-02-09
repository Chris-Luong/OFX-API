namespace OFXApi.Models.Domain;

public class Payer
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string TransferReason { get; init; }
}