
namespace BitMexLibrary.Automapping
{
    public class PositionUpdateModel
    {
        public System.DateTimeOffset Timestamp { get; set; }

        public long Account { get; set; }
        
        public string Symbol { get; set; }
        
        public string Currency { get; set; }
        
        public string Underlying { get; set; }
        
        public string QuoteCurrency { get; set; }

        public decimal CurrentQty { get; set; }

        public string Direction { get; set; }

        public decimal Commission { get; set; }

        public decimal? MarkPrice { get; set; }

        public decimal? MarginCallPrice { get; set; }

        public decimal? LiquidationPrice { get; set; }

        public decimal? LastPrice { get; set; }

        public decimal UnrealisedPnl { get; set; }

        public decimal InitMarginReq { get; set; }
        
        public decimal MaintMarginReq { get; set; }
                
        public decimal Leverage { get; set; }
        
        public bool CrossMargin { get; set; }
        
        public decimal? DeleveragePercentile { get; set; }
        
        public decimal RebalancedPnl { get; set; }
        
        public decimal PrevRealisedPnl { get; set; }
        
        public decimal PrevUnrealisedPnl { get; set; }
        
        public decimal PrevClosePrice { get; set; }

        public decimal RiskLimit { get; set; }

        public System.DateTimeOffset OpeningTimestamp { get; set; }
        
        public decimal OpeningQty { get; set; }
        
        public decimal OpeningCost { get; set; }
        
        public decimal OpeningComm { get; set; }
        
        public decimal OpenOrderBuyQty { get; set; }
        
        public decimal OpenOrderBuyCost { get; set; }
        
        public decimal OpenOrderBuyPremium { get; set; }
        
        public decimal OpenOrderSellQty { get; set; }
        
        public decimal OpenOrderSellCost { get; set; }
        
        public decimal OpenOrderSellPremium { get; set; }
        
        public decimal ExecBuyQty { get; set; }
        
        public decimal ExecBuyCost { get; set; }
        
        public decimal ExecSellQty { get; set; }
        
        public decimal ExecSellCost { get; set; }
        
        public decimal ExecQty { get; set; }
        
        public decimal ExecCost { get; set; }
        
        public decimal ExecComm { get; set; }
        
        public System.DateTimeOffset CurrentTimestamp { get; set; }
        
        public decimal CurrentCost { get; set; }
        
        public decimal? CurrentComm { get; set; }
        
        public decimal RealisedCost { get; set; }
        
        public decimal UnrealisedCost { get; set; }
        
        public decimal GrossOpenCost { get; set; }
        
        public decimal GrossOpenPremium { get; set; }
        
        public decimal GrossExecCost { get; set; }
        
        public bool IsOpen { get; set; }
              
        public decimal MarkValue { get; set; }
        
        public decimal RiskValue { get; set; }
        
        public decimal HomeNotional { get; set; }
        
        public decimal ForeignNotional { get; set; }
        
        public string PosState { get; set; }
        
        public decimal PosCost { get; set; }
        
        public decimal PosCost2 { get; set; }
        
        public decimal PosCross { get; set; }
        
        public decimal PosInit { get; set; }
        
        public decimal PosComm { get; set; }
        
        public decimal PosLoss { get; set; }
        
        public decimal PosMargin { get; set; }
        
        public decimal PosMaint { get; set; }
        
        public decimal PosAllowance { get; set; }
        
        public decimal TaxableMargin { get; set; }
        
        public decimal InitMargin { get; set; }
        
        public decimal MaintMargin { get; set; }
        
        public decimal SessionMargin { get; set; }
        
        public decimal TargetExcessMargin { get; set; }
        
        public decimal VarMargin { get; set; }
        
        public decimal RealisedGrossPnl { get; set; }
        
        public decimal RealisedTax { get; set; }
        
        public decimal RealisedPnl { get; set; }
        
        public decimal UnrealisedGrossPnl { get; set; }
        
        public decimal LongBankrupt { get; set; }
        
        public decimal ShortBankrupt { get; set; }
        
        public decimal TaxBase { get; set; }
        
        public decimal? IndicativeTaxRate { get; set; }
        
        public decimal IndicativeTax { get; set; }
        
        public decimal UnrealisedTax { get; set; }
                
        public decimal UnrealisedPnlPcnt { get; set; }

        public decimal UnrealisedRoePcnt { get; set; }
        
        public decimal? AvgCostPrice { get; set; }

        public decimal? AvgEntryPrice { get; set; }
        
        public decimal? BreakEvenPrice { get; set; }
                
        public decimal? BankruptPrice { get; set; }
               
        public decimal LastValue { get; set; }
    }

    public class PositionAdditionalUpdateModel
    {
        public System.DateTimeOffset Timestamp { get; set; }

        //public long Account { get; set; }

        public string Symbol { get; set; }

        //public string Currency { get; set; }

        //public string Underlying { get; set; }

        //public string QuoteCurrency { get; set; }
        public decimal? MarkPrice { get; set; }

        public decimal CurrentQty { get; set; }

        public decimal? LiquidationPrice { get; set; }

        public decimal UnrealisedPnl { get; set; }

        //public string Direction { get; set; }

        public decimal Commission { get; set; }

        //public decimal UnrealisedPnl { get; set; }

        //public decimal? MarginCallPrice { get; set; }

        //public decimal? LiquidationPrice { get; set; }

        //public decimal? LastPrice { get; set; }

        //public decimal InitMarginReq { get; set; }

        //public decimal MaintMarginReq { get; set; }

        public decimal Leverage { get; set; }

        public bool CrossMargin { get; set; }

       /* public decimal? DeleveragePercentile { get; set; }

        public decimal RebalancedPnl { get; set; }

        public decimal PrevRealisedPnl { get; set; }

        public decimal PrevUnrealisedPnl { get; set; }

        public decimal PrevClosePrice { get; set; }

        public decimal RiskLimit { get; set; }

        public System.DateTimeOffset OpeningTimestamp { get; set; }

        public decimal OpeningQty { get; set; }

        public decimal OpeningCost { get; set; }

        public decimal OpeningComm { get; set; }

        public decimal OpenOrderBuyQty { get; set; }

        public decimal OpenOrderBuyCost { get; set; }

        public decimal OpenOrderBuyPremium { get; set; }

        public decimal OpenOrderSellQty { get; set; }

        public decimal OpenOrderSellCost { get; set; }

        public decimal OpenOrderSellPremium { get; set; }

        public decimal ExecBuyQty { get; set; }

        public decimal ExecBuyCost { get; set; }

        public decimal ExecSellQty { get; set; }

        public decimal ExecSellCost { get; set; }

        public decimal ExecQty { get; set; }

        public decimal ExecCost { get; set; }

        public decimal ExecComm { get; set; }

        public System.DateTimeOffset CurrentTimestamp { get; set; }

        public decimal CurrentCost { get; set; }

        public decimal? CurrentComm { get; set; }

        public decimal RealisedCost { get; set; }

        public decimal UnrealisedCost { get; set; }

        public decimal GrossOpenCost { get; set; }

        public decimal GrossOpenPremium { get; set; }

        public decimal GrossExecCost { get; set; }

        public bool IsOpen { get; set; }

        public decimal MarkValue { get; set; }

        public decimal RiskValue { get; set; }

        public decimal HomeNotional { get; set; }

        public decimal ForeignNotional { get; set; }

        public string PosState { get; set; }

        public decimal PosCost { get; set; }

        public decimal PosCost2 { get; set; }

        public decimal PosCross { get; set; }

        public decimal PosInit { get; set; }

        public decimal PosComm { get; set; }

        public decimal PosLoss { get; set; }

        public decimal PosMargin { get; set; }

        public decimal PosMaint { get; set; }

        public decimal PosAllowance { get; set; }

        public decimal TaxableMargin { get; set; }

        public decimal InitMargin { get; set; }

        public decimal MaintMargin { get; set; }

        public decimal SessionMargin { get; set; }

        public decimal TargetExcessMargin { get; set; }

        public decimal VarMargin { get; set; }

        public decimal RealisedGrossPnl { get; set; }

        public decimal RealisedTax { get; set; }

        public decimal RealisedPnl { get; set; }

        public decimal UnrealisedGrossPnl { get; set; }

        public decimal LongBankrupt { get; set; }

        public decimal ShortBankrupt { get; set; }

        public decimal TaxBase { get; set; }

        public decimal? IndicativeTaxRate { get; set; }

        public decimal IndicativeTax { get; set; }

        public decimal UnrealisedTax { get; set; }

        public decimal UnrealisedPnlPcnt { get; set; }

        public decimal UnrealisedRoePcnt { get; set; }

        public decimal? AvgCostPrice { get; set; }

        public decimal? AvgEntryPrice { get; set; }

        public decimal? BreakEvenPrice { get; set; }

        public decimal? BankruptPrice { get; set; }

        public decimal LastValue { get; set; }*/
    }
}
