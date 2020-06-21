using CommLibrary.Enums;
using BitMexLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BitMexLibrary
{
    [DataContract]
    public class Candles : List<Candle>
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
                        foreach (Candle candle in this)
                            candle.TimeOpen = candle.TimeStamp.AddMinutes(-(int)value);
                }
            }
        }

        public new void Add(Candle candle)
        {
            if (Enum.IsDefined(typeof(BinSizeEnum), BinSize))
                candle.TimeOpen = candle.TimeStamp.AddMinutes(-(int)BinSize);
            base.Add(candle);
        }

    }

    [DataContract]
    public class Candle
    {
        #region свойства от BitMex
        [DataMember(Name = "id", IsRequired = false)]
        public int ID { get; set; }
        [DataMember(Name = "timestamp", IsRequired = true)]
        public DateTime TimeStamp { get; set; }
        [DataMember(Name = "symbol", IsRequired = true)]
        public string Symbol { get; set; }
        [DataMember(Name = "open", IsRequired = true)]
        public decimal Open { get; set; }
        [DataMember(Name = "high", IsRequired = true)]
        public decimal High { get; set; }
        [DataMember(Name = "low", IsRequired = true)]
        public decimal Low { get; set; }
        [DataMember(Name = "close", IsRequired = true)]
        public decimal Close { get; set; }
        [DataMember(Name = "trades", IsRequired = false)]
        public int Trades { get; set; }
        [DataMember(Name = "volume", IsRequired = true)]
        public decimal Volume { get; set; }
        [DataMember(Name = "vwap", IsRequired = false)]
        public decimal? Vwap { get; set; }
        [DataMember(Name = "lastSize", IsRequired = false)]
        public int? LastSize { get; set; }
        [DataMember(Name = "turnover", IsRequired = false)]
        public long TurnOver { get; set; }
        [DataMember(Name = "homeNotional", IsRequired = false)]
        public decimal HomeNotional { get; set; }
        [DataMember(Name = "foreignNotional", IsRequired = false)]
        public int ForeignNotional { get; set; }
        #endregion

        #region Дополнительные свойства
        [DataMember(Name = "timeopen", IsRequired = false)]
        public DateTime TimeOpen { get; set; }
        #endregion

    }

}
