namespace OFXApi.Models.Domain;

public class Recipient
{
    public required string Name { get; init; }
    public required int AccountNumber { get; init; }
    public required int BankCode { get; init; }
    public required string BankName  { get; init; }
}