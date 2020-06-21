using System;
using System.Collections.Generic;
using System.Linq;
using BitMexLibrary;

namespace MachinaTrader.Indicators
{
    public static partial class Extensions
    {
        public static List<decimal?> Cmo(this List<Candle> source, int period = 14)
        {
            // int outBegIdx, outNbElement;
            double[] cmoValues = new double[source.Count];
            double[] valuesToCheck = source.Select(x => Convert.ToDouble(x.Close)).ToArray();

            var ema = TicTacTec.TA.Library.Core.Cmo(0, source.Count - 1, valuesToCheck, period, out int outBegIdx,
                out int outNbElement, cmoValues);

            if (ema == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(cmoValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate CMO!");
        }
    }
}
