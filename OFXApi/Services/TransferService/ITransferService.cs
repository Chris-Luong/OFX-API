using OFXApi.Models.Domain;
using OFXApi.Models.Dtos.TransferDtos;

namespace OFXApi.Services.TransferService;

public interface ITransferService
{
    Transfer CreateTransfer(TransferRequest request);
    Transfer? GetTransfer(Guid transferId);
}