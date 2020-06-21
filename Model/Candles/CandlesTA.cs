using CommLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BitMexLibrary
{
    [DataContract]
    public class CandlesTA : List<CandleTA>
    {
        BinSizeEnum _binSize = 0;
        /// <summary>Период свечей в минутах</summary>
        [DataMember(Name = "binSize", IsRequired = false)]
        public BinSizeEnum BinSize
        {
            get => _binSize; set
            {
                if (_binSize != value)
                {
                    _binSize = value;
                    if (Count != 0)
                        foreach (CandleTA candle in this)
                            candle.TimeOpen = candle.TimeStamp.AddMinutes(-(int)value);
                }
            }
        }

        public new void Add(CandleTA candle)
        {
            if (Enum.IsDefined(typeof(BinSizeEnum), BinSize))
                candle.TimeOpen = candle.TimeStamp.AddMinutes(-(int)BinSize);
            base.Add(candle);
        }
    }

    [DataContract]
    public class CandleTA : Candle
    {
        //[DataMember(Name = "timestamp", IsRequired = false)]
        //public DateTime TimeOpen { get; set; }
        //[DataMember(Name = "symbol", IsRequired = false)]
        //public string Symbol { get; set; }
        //[DataMember(Name = "open", IsRequired = false)]
        //public decimal Open { get; set; }
        //[DataMember(Name = "high", IsRequired = false)]
        //public decimal High { get; set; }
        //[DataMember(Name = "low", IsRequired = false)]
        //public decimal Low { get; set; }
        //[DataMember(Name = "close", IsRequired = false)]
        //public decimal Close { get; set; }
        //[DataMember(Name = "trades", IsRequired = false)]
        //public int Trades { get; set; }
        //[DataMember(Name = "volume", IsRequired = false)]
        //public decimal Volume { get; set; }
        //[DataMember(Name = "vwap", IsRequired = false)]
        //public decimal Vwap { get; set; }
        //[DataMember(Name = "lastSize", IsRequired = false)]
        //public int LastSize { get; set; }
        //[DataMember(Name = "turnover", IsRequired = false)]
        //public long TurnOver { get; set; }
        //[DataMember(Name = "homeNotional", IsRequired = false)]
        //public decimal HomeNotional { get; set; }
        //[DataMember(Name = "foreignNotional", IsRequired = false)]
        //public int ForeignNotional { get; set; }
    }

}
