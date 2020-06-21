using System;

namespace Bitmex.NET.Models
{
    public partial class QuoteGETRequestParams
    {
        public static QuoteGETRequestParams GetQuoteRequest(string symbol, decimal? count, string columns = null, decimal? start = null, bool reverse = false, DateTime? startTime = null, DateTime? endTime = null)
        {
            return new QuoteGETRequestParams
            {
                Symbol = symbol,
                Columns = columns,
                Count = count,
                Start = start,
                Reverse = reverse,
                StartTime = startTime,
                EndTime = endTime
            };
        }
    }
}
