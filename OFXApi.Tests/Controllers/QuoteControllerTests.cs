using System;
using System.Threading.Tasks;
using OFXApi.Controllers;
using OFXApi.Models.Dtos.QuoteDtos;
using OFXApi.Models.Domain;
using OFXApi.Services.QuoteService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace OFXApi.Tests.Controllers
{
    public class QuoteControllerTests
    {
        private readonly Mock<IQuoteService> _mockQuoteService;
        private readonly Mock<ILogger<QuoteController>> _mockLogger;
        private readonly QuoteController _controller;

        public QuoteControllerTests()
        {
            _mockQuoteService = new Mock<IQuoteService>();
            _mockLogger = new Mock<ILogger<QuoteController>>();
            _controller = new QuoteController(_mockQuoteService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateQuote_ValidRequest_ReturnsCreatedResult()
        {
            // Arrange
            var request = new QuoteRequest
            {
                SellCurrency = "AUD",
                BuyCurrency = "USD",
                Amount = 1000m
            };

            var quote = new Quote
            {
                Id = Guid.NewGuid(),
                SellCurrency = "AUD",
                BuyCurrency = "USD",
                Amount = 1000m,
                OfxRate = 0.75m,
                InverseOfxRate = 1.333333m,
                ConvertedAmount = 750m,
                CreatedAt = DateTime.UtcNow
            };

            _mockQuoteService
                .Setup(s => s.CreateQuoteAsync(request))
                .ReturnsAsync(quote);

            // Act
            var result = await _controller.CreateQuote(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<QuoteResponse>(createdAtActionResult.Value);
            Assert.Equal(quote.Id, response.QuoteId);
            Assert.Equal(quote.OfxRate, response.OfxRate);
        }

        [Fact]
        public async Task CreateQuote_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var request = new QuoteRequest
            {
                SellCurrency = "",
                BuyCurrency = "USD",
                Amount = -100m
            };

            _mockQuoteService
                .Setup(s => s.CreateQuoteAsync(request))
                .ThrowsAsync(new ArgumentException("sellCurrency and buyCurrency are required."));

            // Act
            var result = await _controller.CreateQuote(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("sellCurrency and buyCurrency", badRequestResult.Value.ToString());
        }

        [Fact]
        public void GetQuote_ExistingQuote_ReturnsOkResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var quote = new Quote
            {
                Id = id,
                SellCurrency = "AUD",
                BuyCurrency = "USD",
                Amount = 1000m,
                OfxRate = 0.75m,
                InverseOfxRate = 1.333333m,
                ConvertedAmount = 750m,
                CreatedAt = DateTime.UtcNow
            };

            _mockQuoteService.Setup(s => s.GetQuote(id)).Returns(quote);

            // Act
            var result = _controller.GetQuote(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<QuoteResponse>(okResult.Value);
            Assert.Equal(id, response.QuoteId);
        }

        [Fact]
        public void GetQuote_NonExistingQuote_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockQuoteService.Setup(s => s.GetQuote(id)).Returns((Quote)null);

            // Act
            var result = _controller.GetQuote(id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
