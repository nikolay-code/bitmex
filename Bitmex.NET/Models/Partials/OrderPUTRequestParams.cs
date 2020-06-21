using System;

namespace Bitmex.NET.Models
{
    public partial class OrderPUTRequestParams
    {
        public static OrderPUTRequestParams GetOrderPutRequest(string orderId, decimal? price)
        {
            return new OrderPUTRequestParams
            {
                OrderID = orderId,
                Price = price
            };
        }
    }
}


