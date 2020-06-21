
namespace BitMexLibrary.Automapping
{
    public class OrderHistoryModel
    {
        public System.DateTimeOffset TransactTime { get; set; }

        public string OrderId { get; set; }
        
        //public string ClOrdId { get; set; }
        
        //public string ClOrdLinkId { get; set; }
        
        public long? Account { get; set; }
        
        public string Symbol { get; set; }
        
        public string Side { get; set; }
        
        public decimal? OrderQty { get; set; }
        
        public decimal? Price { get; set; }
        
        public decimal? DisplayQty { get; set; }
        
        //public decimal? StopPx { get; set; }
        
        //public decimal? PegOffsetValue { get; set; }
        
        //public string PegPriceType { get; set; }
        
        public string Currency { get; set; }
        
        public string SettlCurrency { get; set; }
        
        public string OrdType { get; set; }
        
        public string TimeInForce { get; set; }
        
        public string ExecInst { get; set; }
        
        public string ExDestination { get; set; }
        
        public string OrdStatus { get; set; }
        
        //public string Triggered { get; set; }
        
        //public bool WorkingIndicator { get; set; }
        
        public string OrdRejReason { get; set; }
        
        public decimal? LeavesQty { get; set; }
        
        public decimal CumQty { get; set; }
        
        public decimal? AvgPx { get; set; }
        
        //public string MultiLegReportingType { get; set; }
        
        public string Text { get; set; }
        
        //public System.DateTimeOffset TransactTime { get; set; }
        
        public System.DateTimeOffset Timestamp { get; set; }
    }
}
