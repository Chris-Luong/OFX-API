using Microsoft.AspNetCore.Mvc;
using OFXApi.Models.Dtos.QuoteDtos;
using OFXApi.Services.QuoteService;

namespace OFXApi.Controllers;

[ApiController]
[Route("transfers/quote")]
[Produces("application/json")]

public class QuoteController: ControllerBase
{
    private readonly IQuoteService _quoteService;
    private readonly ILogger<QuoteController> _logger;

    public QuoteController(IQuoteService quoteService, ILogger<QuoteController> logger)
    {
        _logger = logger;
        _quoteService = quoteService;
    }

        [HttpPost]
        [ProducesResponseType(typeof(QuoteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateQuote([FromBody] QuoteRequest request)
        {
            _logger.LogInformation("Received CreateQuote request: SellCurrency={SellCurrency}, BuyCurrency={BuyCurrency}, Amount={Amount}",
                request.SellCurrency, request.BuyCurrency, request.Amount);

            try
            {
                var quote = await _quoteService.CreateQuoteAsync(request);
                var response = new QuoteResponse
                {
                    QuoteId = quote.Id,
                    OfxRate = quote.OfxRate,
                    InverseOfxRate = quote.InverseOfxRate,
                    ConvertedAmount = quote.ConvertedAmount
                };

                _logger.LogInformation("Quote created successfully with QuoteId: {QuoteId}", quote.Id);
                return CreatedAtAction(nameof(GetQuote), new { quoteId = quote.Id }, response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation failed in CreateQuote: {ErrorMessage}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating quote: {ErrorMessage}", ex.Message);
                return Problem(ex.Message);
            }
        }


        [ProducesResponseType(typeof(QuoteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{quoteId:guid}")]
        public IActionResult GetQuote(Guid quoteId)
        {
            _logger.LogInformation("Retrieving quote with QuoteId: {QuoteId}", quoteId);


            var quote = _quoteService.GetQuote(quoteId);
            if (quote is null)
            {
                _logger.LogWarning("Quote with QuoteId {QuoteId} not found", quoteId);
                return NotFound(new { error = "Quote not found." });
            }

            var response = new QuoteResponse
            {
                QuoteId = quote.Id,
                OfxRate = quote.OfxRate,
                InverseOfxRate = quote.InverseOfxRate,
                ConvertedAmount = quote.ConvertedAmount
            };

            _logger.LogInformation("Quote with QuoteId {QuoteId} retrieved successfully", quoteId);
            return Ok(response);
        }
}
