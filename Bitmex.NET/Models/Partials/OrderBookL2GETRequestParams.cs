using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitmex.NET.Models
{
    public partial class OrderBookL2GETRequestParams
    {
        public static OrderBookL2GETRequestParams getOrderBookL2GETRequest(string symbol, decimal? depth)
        {
            return new OrderBookL2GETRequestParams
            {
                Symbol = symbol,
                Depth = depth
            };
        }
    }
}
