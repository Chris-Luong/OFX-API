using System;
using OFXApi.Controllers;
using OFXApi.Models.Dtos.QuoteDtos;
using OFXApi.Models.Dtos.TransferDtos;
using OFXApi.Models.Domain;
using OFXApi.Services.TransferService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace OFXApi.Tests.Controllers
{
    public class TransferControllerTests
    {
        private readonly Mock<ITransferService> _mockTransferService;
        private readonly Mock<ILogger<TransferController>> _mockLogger;
        private readonly TransferController _controller;

        public TransferControllerTests()
        {
            _mockTransferService = new Mock<ITransferService>();
            _mockLogger = new Mock<ILogger<TransferController>>();
            _controller = new TransferController(_mockTransferService.Object, _mockLogger.Object);
        }

        [Fact]
        public void CreateTransfer_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var request = new TransferRequest
            {
                QuoteId = quoteId,
                Payer = new PayerRequest { Id = Guid.NewGuid(), Name = "John Doe", TransferReason = "Invoice" },
                Recipient = new RecipientRequest { Name = "Jane Doe", AccountNumber = "123456789", BankCode = "001", BankName = "Test Bank" }
            };

            var transfer = new Transfer
            {
                Id = Guid.NewGuid(),
                QuoteId = quoteId,
                Status = TransferStatus.Processing,
                Payer = new Payer { Id = request.Payer.Id, Name = request.Payer.Name, TransferReason = request.Payer.TransferReason },
                Recipient = new Recipient { 
                    Name = request.Recipient.Name, 
                    AccountNumber = int.Parse(request.Recipient.AccountNumber),
                    BankCode = int.Parse(request.Recipient.BankCode),
                    BankName = request.Recipient.BankName 
                },
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(1)
            };

            _mockTransferService.Setup(s => s.CreateTransfer(request)).Returns(transfer);

            // Act
            var result = _controller.CreateTransfer(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<TransferResponse>(createdAtActionResult.Value);
            Assert.Equal(transfer.Id, response.TransferId);
            Assert.Equal("Processing", response.Status);
        }

        [Fact]
        public void GetTransfer_ExistingTransfer_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var transfer = new Transfer
            {
                Id = id,
                QuoteId = Guid.NewGuid(),
                Status = TransferStatus.Processing,
                Payer = new Payer { Id = Guid.NewGuid(), Name = "John Doe", TransferReason = "Invoice" },
                Recipient = new Recipient { Name = "Jane Doe", AccountNumber = 123456789, BankCode = 1, BankName = "Test Bank" },
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(1)
            };

            _mockTransferService.Setup(s => s.GetTransfer(id)).Returns(transfer);

            // Act
            var result = _controller.GetTransfer(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TransferResponse>(okResult.Value);
            Assert.Equal(id, response.TransferId);
        }

        [Fact]
        public void GetTransfer_NonExistingTransfer_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockTransferService.Setup(s => s.GetTransfer(id)).Returns((Transfer)null);

            // Act
            var result = _controller.GetTransfer(id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
