﻿using System;

namespace BitMexLibrary.Automapping
{
    [Serializable]
    public class OrderUpdateModel
    {
        public long? Account { get; set; }

        public DateTime NotificationDateTime { get; set; }

        public string OrderId { get; set; }

        public string ClOrdId { get; set; }

       // public long? Account { get; set; }

        public string Symbol { get; set; }

        public string Side { get; set; }

        public decimal? OrderQty { get; set; }

        public decimal? Price { get; set; }

        public string OrdStatus { get; set; }

        public string OrdType { get; set; }

        public System.DateTimeOffset TransactTime { get; set; }

        public System.DateTimeOffset Timestamp { get; set; }

        public decimal? LeavesQty { get; set; }
    }
}
