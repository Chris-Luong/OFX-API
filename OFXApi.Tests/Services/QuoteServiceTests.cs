using System;
using System.Threading.Tasks;
using Moq;
using OFXApi.Models.Dtos.QuoteDtos;
using OFXApi.Models.Domain;
using OFXApi.Services.QuoteService;
using OFXApi.Services.ExchangeRateService;
using OFXApi.Data;
using Xunit;

namespace OFXApi.Tests.Services
{
    public class QuoteServiceTests
    {
        private readonly Mock<IExchangeRateService> _mockExchangeRateService;
        private readonly IQuoteService _quoteService;

        public QuoteServiceTests()
        {
            _mockExchangeRateService = new Mock<IExchangeRateService>();
            _quoteService = new QuoteService(_mockExchangeRateService.Object);
            
            // Clear the in-memory store for isolation between tests.
            MemoryStore.Quotes.Clear();
        }

        [Fact]
        public async Task CreateQuoteAsync_ValidRequest_ReturnsQuoteWithCorrectCalculation()
        {
            // Arrange
            var request = new QuoteRequest
            {
                SellCurrency = "AUD",
                BuyCurrency = "USD",
                Amount = 1000m
            };
            _mockExchangeRateService
                .Setup(s => s.GetExchangeRateAsync("AUD", "USD"))
                .ReturnsAsync(0.6162m);

            // Act
            Quote quote = await _quoteService.CreateQuoteAsync(request);

            // Assert
            Assert.NotEqual(Guid.Empty, quote.Id);
            Assert.Equal("AUD", quote.SellCurrency);
            Assert.Equal("USD", quote.BuyCurrency);
            Assert.Equal(1000m, quote.Amount);
            Assert.Equal(Math.Round(0.6162m, 6), quote.OfxRate);
            Assert.Equal(Math.Round(1 / 0.6162m, 6), quote.InverseOfxRate);
            Assert.Equal(Math.Round(1000m * 0.6162m, 2), quote.ConvertedAmount);
        }

        [Theory]
        [InlineData("", "USD", 1000)]
        [InlineData("AUD", "", 1000)]
        [InlineData("AUD", "AUD", 1000)]
        [InlineData("AUD", "USD", -50)]
        public async Task CreateQuoteAsync_InvalidRequest_ThrowsArgumentException(string sell, string buy, decimal amount)
        {
            // Arrange
            var request = new QuoteRequest
            {
                SellCurrency = sell,
                BuyCurrency = buy,
                Amount = amount
            };

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _quoteService.CreateQuoteAsync(request);
            });
        }

        [Fact]
        public void GetQuote_ExistingQuote_ReturnsQuote()
        {
            // Arrange
            var quote = new Quote
            {
                Id = Guid.NewGuid(),
                SellCurrency = "AUD",
                BuyCurrency = "USD",
                Amount = 1000m,
                OfxRate = 0.6162m,
                InverseOfxRate = Math.Round(1 / 0.6162m, 6),
                ConvertedAmount = Math.Round(1000m * 0.6162m, 2),
                CreatedAt = DateTime.UtcNow
            };

            MemoryStore.Quotes[quote.Id] = quote;

            // Act
            Quote? returnedQuote = _quoteService.GetQuote(quote.Id);

            // Assert
            Assert.NotNull(returnedQuote);
            Assert.Equal(quote.Id, returnedQuote!.Id);
        }

        [Fact]
        public void GetQuote_NonExistingQuote_ReturnsNull()
        {
            // Arrange
            Guid nonExistingId = Guid.NewGuid();

            // Act
            Quote? returnedQuote = _quoteService.GetQuote(nonExistingId);

            // Assert
            Assert.Null(returnedQuote);
        }
    }
}
