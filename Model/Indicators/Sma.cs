using System;
using System.Collections.Generic;
using System.Linq;
using BitMexLibrary;
using BitMexLibrary.Enums;

namespace MachinaTrader.Indicators
{
    public static partial class Extensions
    {
        public static List<decimal?> Sma(this List<Candle> source, int period = 30, CandleVariable type = CandleVariable.Close)
        {
            // int outBegIdx, outNbElement;
            double[] smaValues = new double[source.Count];
            List<double?> outValues = new List<double?>();
            double[] valuesToCheck;

            switch (type)
            {
                case CandleVariable.Open:
                    valuesToCheck = source.Select(x => Convert.ToDouble(x.Open)).ToArray();
                    break;
                case CandleVariable.Low:
                    valuesToCheck = source.Select(x => Convert.ToDouble(x.Low)).ToArray();
                    break;
                case CandleVariable.High:
                    valuesToCheck = source.Select(x => Convert.ToDouble(x.High)).ToArray();
                    break;
                default:
                    valuesToCheck = source.Select(x => Convert.ToDouble(x.Close)).ToArray();
                    break;
            }

            var sma = TicTacTec.TA.Library.Core.Sma(0, source.Count - 1, valuesToCheck, period, out int outBegIdx, out int outNbElement, smaValues);

            if (sma == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(smaValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate SMA!");
        }

        public static List<decimal?> Sma(this List<decimal> source, int period = 30)
        {
            // int outBegIdx, outNbElement;
            double[] smaValues = new double[source.Count];
            List<double?> outValues = new List<double?>();

            var sourceFix = source.Select(x => Convert.ToDouble(x)).ToArray();

            var sma = TicTacTec.TA.Library.Core.Sma(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, smaValues);

            if (sma == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrdering(smaValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate SMA!");
        }
        public static List<double> Sma(this List<double> source, int period = 30)
        {
            // int outBegIdx, outNbElement;
            double[] smaValues = new double[source.Count];
            //List<double?> outValues = new List<double?>();

            var sourceFix = source.ToArray();

            var sma = TicTacTec.TA.Library.Core.Sma(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, smaValues);

            if (sma == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                return FixIndicatorOrderingD(smaValues.ToList(), outBegIdx, outNbElement);
            }

            throw new Exception("Could not calculate SMA!");
        }

        public static List<decimal?> Sma(this List<decimal?> source, int period = 30)
        {
            // int outBegIdx, outNbElement;
            double[] smaValues = new double[source.Count];
            List<double?> outValues = new List<double?>();

            var sourceNullCount = source.Count(x => x == null);
            var sourceFix = source.Where(x => x != null).Select(x => Convert.ToDouble(x)).ToArray();

            var sma = TicTacTec.TA.Library.Core.Sma(0, source.Count - 1 - sourceNullCount, sourceFix, period, out int outBegIdx, out int outNbElement, smaValues);

            if (sma == TicTacTec.TA.Library.Core.RetCode.Success)
            {
                var smas = FixIndicatorOrdering(smaValues.ToList(), outBegIdx, outNbElement);

                for (int i = 0; i < sourceNullCount; i++)
                    smas.Insert(0, null);

                return smas;
            }

            throw new Exception("Could not calculate SMA!");
        }
    }
}
