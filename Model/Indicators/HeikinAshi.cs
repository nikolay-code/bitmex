using System;
using System.Collections.Generic;
using BitMexLibrary;

namespace MachinaTrader.Indicators
{
    public static partial class Extensions
    {
        public static List<Candle> HeikinAshi(this List<Candle> source)
        {
            var result = new List<Candle>();
            Candle previous = null;

            foreach (var item in source)
            {
                var candle = new Candle
                {
                    ID = item.ID,
                    TimeStamp = item.TimeStamp,
                    Volume = item.Volume
                };

                if (previous != null)
                {
                    candle.Close = (item.High + item.Low + item.Close + item.Open) / 4;
                    candle.Open = (previous.Open + previous.Close) / 2;
                    candle.Low = Math.Min(Math.Min(item.Low, item.Open), item.Close);
                    candle.High = Math.Max(Math.Max(item.High, item.Open), item.Close);
                }
                else
                {
                    candle.Close = (item.High + item.Low + item.Close + item.Open) / 4;
                    candle.Open = (item.Open + item.Close) / 2;
                    candle.Low = Math.Min(Math.Min(item.Low, item.Open), item.Close);
                    candle.High = Math.Max(Math.Max(item.High, item.Open), item.Close);
                }

                result.Add(candle);

                previous = item;
            }

            return result;
        }
    }
}
