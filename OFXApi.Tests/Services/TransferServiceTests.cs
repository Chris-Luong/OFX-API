using System;
using OFXApi.Models.Dtos.TransferDtos;
using OFXApi.Models.Domain;
using OFXApi.Services.TransferService;
using OFXApi.Data;
using Xunit;

namespace OFXApi.Tests.Services
{
    public class TransferServiceTests
    {
        private readonly ITransferService _transferService;

        public TransferServiceTests()
        {
            _transferService = new TransferService();
            // Clear the in-memory stores to isolate tests.
            MemoryStore.Quotes.Clear();
            MemoryStore.Transfers.Clear();
        }

        [Fact]
        public void CreateTransfer_ValidRequest_ReturnsTransferWithCorrectValues()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var dummyQuote = new Quote
            {
                Id = quoteId,
                SellCurrency = "AUD",
                BuyCurrency = "USD",
                Amount = 1000m,
                OfxRate = 0.6162m,
                InverseOfxRate = Math.Round(1 / 0.6162m, 6),
                ConvertedAmount = Math.Round(1000m * 0.6162m, 2),
                CreatedAt = DateTime.UtcNow
            };
            MemoryStore.Quotes[quoteId] = dummyQuote;

            var request = new TransferRequest
            {
                QuoteId = quoteId,
                Payer = new OFXApi.Models.Dtos.TransferDtos.PayerRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    TransferReason = "Invoice"
                },
                Recipient = new OFXApi.Models.Dtos.TransferDtos.RecipientRequest
                {
                    Name = "Jane Doe",
                    AccountNumber = "123456789",
                    BankCode = "001",
                    BankName = "Test Bank"
                }
            };

            // Act
            Transfer transfer = _transferService.CreateTransfer(request);

            // Assert
            Assert.NotEqual(Guid.Empty, transfer.Id);
            Assert.Equal(quoteId, transfer.QuoteId);
            Assert.Equal(TransferStatus.Processing, transfer.Status);
            Assert.Equal(request.Payer.Id, transfer.Payer.Id);
            Assert.Equal(request.Payer.Name, transfer.Payer.Name);
            Assert.Equal(request.Payer.TransferReason, transfer.Payer.TransferReason);
            Assert.Equal(request.Recipient.Name, transfer.Recipient.Name);
            Assert.Equal(int.Parse(request.Recipient.AccountNumber), transfer.Recipient.AccountNumber);
            Assert.Equal(int.Parse(request.Recipient.BankCode), transfer.Recipient.BankCode);
            Assert.Equal(request.Recipient.BankName, transfer.Recipient.BankName);
            Assert.True(transfer.EstimatedDeliveryDate > DateTime.UtcNow);
        }

        [Fact]
        public void CreateTransfer_InvalidRecipientAccountNumber_ThrowsArgumentException()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var dummyQuote = new Quote
            {
                Id = quoteId,
                SellCurrency = "AUD",
                BuyCurrency = "USD",
                Amount = 1000m,
                OfxRate = 0.6162m,
                InverseOfxRate = Math.Round(1 / 0.6162m, 6),
                ConvertedAmount = Math.Round(1000m * 0.6162m, 2),
                CreatedAt = DateTime.UtcNow
            };
            MemoryStore.Quotes[quoteId] = dummyQuote;

            var request = new TransferRequest
            {
                QuoteId = quoteId,
                Payer = new OFXApi.Models.Dtos.TransferDtos.PayerRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    TransferReason = "Invoice"
                },
                Recipient = new OFXApi.Models.Dtos.TransferDtos.RecipientRequest
                {
                    Name = "Jane Doe",
                    AccountNumber = "invalidAccountNumber",
                    BankCode = "001",
                    BankName = "Test Bank"
                }
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _transferService.CreateTransfer(request));
        }

        [Fact]
        public void CreateTransfer_InvalidRecipientBankCode_ThrowsArgumentException()
        {
            // Arrange
            var quoteId = Guid.NewGuid();
            var dummyQuote = new Quote
            {
                Id = quoteId,
                SellCurrency = "AUD",
                BuyCurrency = "USD",
                Amount = 1000m,
                OfxRate = 0.6162m,
                InverseOfxRate = Math.Round(1 / 0.6162m, 6),
                ConvertedAmount = Math.Round(1000m * 0.6162m, 2),
                CreatedAt = DateTime.UtcNow
            };
            MemoryStore.Quotes[quoteId] = dummyQuote;

            var request = new TransferRequest
            {
                QuoteId = quoteId,
                Payer = new OFXApi.Models.Dtos.TransferDtos.PayerRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    TransferReason = "Invoice"
                },
                Recipient = new OFXApi.Models.Dtos.TransferDtos.RecipientRequest
                {
                    Name = "Jane Doe",
                    AccountNumber = "123456789",
                    BankCode = "invalidCode",
                    BankName = "Test Bank"
                }
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _transferService.CreateTransfer(request));
        }

        [Fact]
        public void CreateTransfer_InvalidQuoteId_ThrowsArgumentException()
        {
            // Arrange
            var nonExistingQuoteId = Guid.NewGuid();

            var request = new TransferRequest
            {
                QuoteId = nonExistingQuoteId,
                Payer = new OFXApi.Models.Dtos.TransferDtos.PayerRequest
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    TransferReason = "Invoice"
                },
                Recipient = new OFXApi.Models.Dtos.TransferDtos.RecipientRequest
                {
                    Name = "Jane Doe",
                    AccountNumber = "invalidAccountNumber",
                    BankCode = "001",
                    BankName = "Test Bank"
                }
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _transferService.CreateTransfer(request));
        }

        [Fact]
        public void GetTransfer_ExistingTransfer_ReturnsTransfer()
        {
            // Arrange
            var transferId = Guid.NewGuid();
            var transfer = new Transfer
            {
                Id = transferId,
                QuoteId = Guid.NewGuid(),
                Status = TransferStatus.Processing,
                Payer = new Payer { Id = Guid.NewGuid(), Name = "John Doe", TransferReason = "Invoice" },
                Recipient = new Recipient { Name = "Jane Doe", AccountNumber = 123456789, BankCode = 1, BankName = "Test Bank" },
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(1)
            };

            MemoryStore.Transfers[transferId] = transfer;

            // Act
            Transfer? result = _transferService.GetTransfer(transferId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(transferId, result!.Id);
        }

        [Fact]
        public void GetTransfer_NonExistingTransfer_ReturnsNull()
        {
            // Arrange
            Guid nonExistingId = Guid.NewGuid();

            // Act
            Transfer? result = _transferService.GetTransfer(nonExistingId);

            // Assert
            Assert.Null(result);
        }
    }
}
