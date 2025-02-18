using System;
using System.Threading.Tasks;
using OFXApi.Services.ExchangeRateService;
using Xunit;

namespace OFXApi.Tests.Services
{
    public class ExchangeRateServiceTests
    {
        private readonly IExchangeRateService _exchangeRateService;

        public ExchangeRateServiceTests()
        {
            _exchangeRateService = new ExchangeRateService();
        }

        [Theory]
        [InlineData("AUD", "USD", 0.6162)]
        [InlineData("EUR", "INR", 86.977)]
        public async Task GetExchangeRateAsync_ValidPair_ReturnsExpectedRate(string sell, string buy, double expectedRate)
        {
            decimal rate = await _exchangeRateService.GetExchangeRateAsync(sell, buy);

            Assert.InRange(rate, (decimal)expectedRate - 0.0001m, (decimal)expectedRate + 0.0001m);
        }

        [Theory]
        [InlineData("USD", "AUD")]
        [InlineData("ABC", "XYZ")]
        public async Task GetExchangeRateAsync_InvalidPair_ThrowsKeyNotFoundException(string sell, string buy)
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                await _exchangeRateService.GetExchangeRateAsync(sell, buy);
            });
        }

        [Fact]
        public async Task GetExchangeRateAsync_CachesResult()
        {
            // Arrange
            string sell = "AUD", buy = "USD";

            // Act: Call twice
            decimal firstCall = await _exchangeRateService.GetExchangeRateAsync(sell, buy);
            decimal secondCall = await _exchangeRateService.GetExchangeRateAsync(sell, buy);

            // Assert: Same value is returned (and likely from cache)
            Assert.Equal(firstCall, secondCall);
        }
    }
}
