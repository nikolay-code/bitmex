using System;

namespace Bitmex.NET.Models
{
    public partial class OrderGETRequestParams
    {
        public static OrderGETRequestParams GetOrderGetRequest(string symbol, decimal? count, string columns = null, decimal? start = null, bool reverse = false, DateTime? startTime = null, DateTime? endTime = null)
        {
            return new OrderGETRequestParams
            {
                Symbol = symbol,
                Columns = columns,
                Count = count,
                Start = start,
                Reverse = reverse,
                StartTime = startTime
            };
        }
    }
}


