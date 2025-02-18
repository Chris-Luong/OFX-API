using OFXApi.Models.Domain;
using OFXApi.Models.Dtos.QuoteDtos;

namespace OFXApi.Services.QuoteService;

public interface IQuoteService
{
    Task<Quote> CreateQuoteAsync(QuoteRequest request);
    Quote? GetQuote(Guid quoteId);
}