using BitMexLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MachinaTrader.Indicators
{
    public static partial class Extensions
    {
        public static List<decimal?> Atr(this List<Candle> source, int period = 20)
        {
            // int outBegIdx, outNbElement;
            double[] atrValues = new double[source.Count];

            var highs = source.Select(x => Convert.ToDouble(x.High)).ToArray();
            var lows = source.Select(x => Convert.ToDouble(x.Low)).ToArray();
            var closes = source.Select(x => Convert.ToDouble(x.Close)).ToArray();

            var adx = TicTacTec.TA.Library.Core.Atr(0, source.Count - 1, highs, lows, closes, period, out int outBegIdx, out int outNbElement, atrValues);

            if (adx == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return Extensions.FixIndicatorOrdering(atrValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate ATR!");
        }
    }
}
