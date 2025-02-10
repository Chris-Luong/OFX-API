using OFXApi.Data;
using OFXApi.Models.Dtos.TransferDtos;
using OFXApi.Models.Domain;

namespace OFXApi.Services.TransferService;

public class TransferService : ITransferService
{
    private static void ValidateRequest(TransferRequest request)
    {
        if (request.QuoteId == Guid.Empty)
        {
            throw new ArgumentException("quoteId is required.");
        }
        if (!MemoryStore.Quotes.ContainsKey(request.QuoteId))
        {
            throw new ArgumentException("Invalid quoteId. Quote does not exist.");
        }

        if (request.Payer == null ||
            request.Payer.Id == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Payer.Name) ||
            string.IsNullOrWhiteSpace(request.Payer.TransferReason))
        {
            throw new ArgumentException("Invalid payer details.");
        }

        if (request.Recipient == null ||
            string.IsNullOrWhiteSpace(request.Recipient.Name) ||
            string.IsNullOrWhiteSpace(request.Recipient.AccountNumber) ||
            !Int32.TryParse(request.Recipient.AccountNumber, out _) ||
            string.IsNullOrWhiteSpace(request.Recipient.BankCode) ||
            !Int32.TryParse(request.Recipient.BankCode, out _) ||
            string.IsNullOrWhiteSpace(request.Recipient.BankName))
        {
            throw new ArgumentException("Invalid recipient details.");
        }
    }

    public Transfer CreateTransfer(TransferRequest request)
    {
        ValidateRequest(request);

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            QuoteId = request.QuoteId,
            Status = TransferStatus.Processing,
            Payer = new Payer
            {
                Id = request.Payer.Id,
                Name = request.Payer.Name,
                TransferReason = request.Payer.TransferReason
            },
            Recipient = new Recipient
            {
                Name = request.Recipient.Name,
                AccountNumber = Int32.Parse(request.Recipient.AccountNumber),
                BankCode = Int32.Parse(request.Recipient.BankCode),
                BankName = request.Recipient.BankName
            },
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(1)
        };

        MemoryStore.Transfers[transfer.Id] = transfer;
        return transfer;
    }

    public Transfer? GetTransfer(Guid transferId)
    {
        MemoryStore.Transfers.TryGetValue(transferId, out var transfer);
        return transfer;
    }
}
