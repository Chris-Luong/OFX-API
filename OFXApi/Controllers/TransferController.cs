using OFXApi.Models.Dtos.TransferDtos;
using OFXApi.Services.TransferService;
using Microsoft.AspNetCore.Mvc;

namespace OFXApi.Controllers;

[ApiController]
[Route("transfers")]
[Produces("application/json")]

public class TransferController : ControllerBase
{
    private readonly ITransferService _transferService;
    private readonly ILogger<TransferController> _logger;

    public TransferController(ITransferService transferService, ILogger<TransferController> logger)
    {
        _transferService = transferService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateTransfer([FromBody] TransferRequest request)
    {
        _logger.LogInformation("Received CreateTransfer request for QuoteId: {QuoteId}", request.QuoteId);

        try
        {
            var transfer = _transferService.CreateTransfer(request);
            var response = new TransferResponse
            {
                TransferId = transfer.Id,
                Status = transfer.Status.ToString(),
                TransferDetails = new TransferDetails
                {
                    QuoteId = transfer.QuoteId,
                    Payer = new PayerRequest
                    {
                        Id = transfer.Payer.Id,
                        Name = transfer.Payer.Name,
                        TransferReason = transfer.Payer.TransferReason
                    },
                    Recipient = new RecipientRequest
                    {
                        Name = transfer.Recipient.Name,
                        AccountNumber = transfer.Recipient.AccountNumber.ToString(),
                        BankCode = transfer.Recipient.BankCode.ToString(),
                        BankName = transfer.Recipient.BankName
                    }
                },
                EstimatedDeliveryDate = transfer.EstimatedDeliveryDate
            };

            _logger.LogInformation("Transfer created successfully with TransferId: {TransferId}", transfer.Id);
            return CreatedAtAction(nameof(GetTransfer), new { transferId = transfer.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation failed in CreateTransfer: {Error}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating transfer");
            return Problem(ex.Message);
        }
    }

    [HttpGet("{transferId:guid}")]
    [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetTransfer(Guid transferId)
    {
        _logger.LogInformation("Retrieving transfer with TransferId: {TransferId}", transferId);
        var transfer = _transferService.GetTransfer(transferId);
        if (transfer is null)
        {
            _logger.LogWarning("Transfer with TransferId {TransferId} not found", transferId);
            return NotFound(new { error = "Transfer not found." });
        }

        var response = new TransferResponse
        {
            TransferId = transfer.Id,
            Status = transfer.Status.ToString(),
            TransferDetails = new TransferDetails
            {
                QuoteId = transfer.QuoteId,
                Payer = new PayerRequest
                {
                    Id = transfer.Payer.Id,
                    Name = transfer.Payer.Name,
                    TransferReason = transfer.Payer.TransferReason
                },
                Recipient = new RecipientRequest
                {
                    Name = transfer.Recipient.Name,
                    AccountNumber = transfer.Recipient.AccountNumber.ToString(),
                    BankCode = transfer.Recipient.BankCode.ToString(),
                    BankName = transfer.Recipient.BankName
                }
            },
            EstimatedDeliveryDate = transfer.EstimatedDeliveryDate
        };

        _logger.LogInformation("Transfer with TransferId {TransferId} retrieved successfully", transferId);
        return Ok(response);
    }
}
