using OFXApi.Models.Domain;
using System.Collections.Concurrent;

namespace OFXApi.Data;
public static class MemoryStore
{
    public static ConcurrentDictionary<Guid, Quote> Quotes { get; } = new();
    public static ConcurrentDictionary<Guid, Transfer> Transfers { get; } = new();
}
