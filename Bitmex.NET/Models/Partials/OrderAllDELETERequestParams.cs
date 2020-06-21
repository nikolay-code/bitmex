using System;

namespace Bitmex.NET.Models
{
    public partial class OrderAllDELETERequestParams
    {
        public static OrderAllDELETERequestParams GetOrderAllDELETERequest(string symbol, string filter, string text)
        {
            return new OrderAllDELETERequestParams
            {
                Symbol = symbol
            };
        }
    }
}
