namespace OFXApi.Models.Dtos.TransferDtos;

public class RecipientRequest
{
    public required string Name { get; init; }
    public required string AccountNumber { get; init; }
    public required string BankCode { get; init; }
    public required string BankName { get; init; }
}
