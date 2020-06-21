using System;

namespace Bitmex.NET.Models
{
    public partial class TradeBucketedGETRequestParams
    {
        public static TradeBucketedGETRequestParams GetCandleHistory(string symbol, decimal? count, string binSize, bool partial = false, string columns = null, decimal? start = null, bool reverse = false, DateTime? startTime = null, DateTime? endTime = null)
        {
            return new TradeBucketedGETRequestParams
            {
                Symbol = symbol,
                Count = count,
                BinSize = binSize,
                Partial = partial,
                Columns = columns,
                Start = start,
                Reverse = reverse,
                StartTime = startTime,
                EndTime = endTime
            };
        }
    }

    public partial class TradeGETRequestParams
    {
        public static TradeGETRequestParams GetTradeGETRequest(string symbol, decimal? count, string columns = null, decimal? start = null, bool reverse = false, DateTime? startTime = null, DateTime? endTime = null)
        {
            return new TradeGETRequestParams
            {
                Symbol = symbol,
                Count = count,
                Columns = columns,
                Start = start,
                Reverse = reverse,
                StartTime = startTime,
                EndTime = endTime
            };
        }
    }
}
