using BitMexLibrary.Enums;
using MachinaTrader.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TA = TicTacTec.TA.Library.Core;

namespace BitMexLibrary
{
    public static class TradingAlgos
    {
        public static List<(double tci, double mf, double willy)> Alroritm(List<Candle> candles, int EMA1Period, int EMA2Period, int MfPeriod)
        {
            var candlesSort = candles.OrderBy(x => x.TimeStamp).ToList();

            Pine close = candlesSort.Close().ToPine();
            Pine open = candlesSort.Open().ToPine();
            Pine volume = candlesSort.Volume().ToPine();
            Pine change = candlesSort.Select(cnd => cnd.Close - cnd.Open).ToPine();




            //tci(src) => ema((src - ema(src, 35)) / (0.025 * ema(abs(src - ema(src, 35)), 35)), 5) + 50
            Pine tci;
            {
                Pine emaSrc35 = Ema(close, EMA1Period);
                Pine src_emaSrc35 = close - emaSrc35;
                Pine abs_src_emaSrc35 = Abs(src_emaSrc35);
                tci = Ema(src_emaSrc35, EMA1Period) / (0.025 * Ema(abs_src_emaSrc35, EMA2Period)) + 50;
            }

            // mf(src) => rsi(sum(volume * (change(src) <= 0 ? 0 : src), 3), sum(volume * (change(src) >= 0 ? 0 : src), 3))
            var mf = Mf(candlesSort, MfPeriod);


            /// willy(src) => 60 * (src - highest(src, 5)) / (highest(src, 5) - lowest(src, 5)) + 80
            var willy = ZipEnd(candles.Close().Select(x => (double)x), Highest(candles.Close(), 5), Lowest(candles.Close(), 5), (c, h, l) => 60.0 * (c - h) / (h - l) + 80).ToList();


            //var mfi = Mfi(candlesSort, RSIPeriod);
            //var rsi = RSI(candlesSort.Close(), RSIPeriod);



            ////csi(src) => avg(rsi(src, 3),tsi(src0,35,5)*50+50)
            //var csi = ZipEnd(RSI(candles.Close(), 3), TCI)


            //int countTrad = new int[] { tsi.Count, mfi.Count, rsi.Count }.Min();

            //var tradition = ZipEnd(tsi, mfi, rsi, (t, m, r) => (t + r + m) / 3.0);
            //tsi.Skip(tsi.Count - countTrad).Zip(mfi.Skip(mfi.Count - countTrad), (t, m) => t + m).Zip(rsi.Skip(rsi.Count - countTrad), (r, tm) => (r + tm) / 3.0).ToList();

            return ZipEnd(tci, mf, willy, (t, m, w) => (t, m, w)).ToList();
        }

        public static Pine Highest(Pine source, int length) => Highest((IEnumerable<double>)source, length).ToPine();
        public static List<double> Highest(IEnumerable<double> source, int length)
        {
            var tmp = Enumerable.Range(0, source.Count() - length + 1).Select(num => source.Skip(num).Take(length)).ToList();
            return tmp.Select(list => list.Max()).ToList();
        }
        public static List<double> Highest(List<decimal> source, int length) => Highest(source.Select(x => (double)x), length);
        public static List<double> Lowest(List<decimal> source, int length) => Lowest(source.Select(x => (double)x), length);
        public static Pine Lowest(Pine source, int length) => Lowest((IEnumerable<double>)source, length).ToPine();
        public static List<double> Lowest(IEnumerable<double> source, int length)
        {
            var tmp = Enumerable.Range(0, source.Count() - length + 1).Select(num => source.Skip(num).Take(length));
            return tmp.Select(list => list.Min()).ToList();
        }

        public static List<double> TCI(List<decimal> source, int EMA1Period, int EMA2Period)
            => TCI(source.Select(x => (double)x).ToList(), EMA1Period, EMA2Period);
        public static List<double> TCI(List<double> source, int EMA1Period, int EMA2Period)
        {
            //tci(src) => ema((src - ema(src, 35)) / (0.025 * ema(abs(src - ema(src, 35)), 35)), 5) + 50

            var ema1 = Ema(source, EMA1Period); // ema(src, 35)
            //var hlc3 = source.Skip(source.Count - ema1.Count).Select(x => (double)(x.High + x.Low + x.Close) / 3.0).ToList();
            //var ema2 = hlc3.Zip(ema1, (h, e) => h - e).ToList();
            var ema2 = ZipEnd(source.Select(x => (double)x), ema1, (c, e) => c - e); // (src - ema(src, 35))
            var abs_ema2 = ema2.Select(x => x < 0 ? -x : x).ToList(); // abs(src - ema(src, 35))
            var ema3 = Ema(abs_ema2, EMA1Period); // ema(abs(src - ema(src, 35)), 35)
            var ema4 = Ema(ZipEnd(ema2, ema3, (e2, e3) => e2 / (0.025 * e3)).ToList(), EMA2Period).ToList(); // ema((src - ema(src, 35)) / (0.025 * ema(abs(src - ema(src, 35)), 35)), 5)
            return ema4.Select(x => x + 50).ToList(); // ema((src - ema(src, 35)) / (0.025 * ema(abs(src - ema(src, 35)), 35)), 5) + 50
        }

        public static Pine Sum(Pine source, int SumPeriod) => SumSliding(source, SumPeriod).ToPine();
        public static List<double> SumSliding(List<decimal> source, int SumPeriod) => SumSliding(source.Select(x => (double)x), SumPeriod);
        public static List<double> SumSliding(IEnumerable<double> source, int SumPeriod)
        {
            var tmp = Enumerable.Range(0, source.Count() - SumPeriod + 1).Select(num => source.Skip(num).Take(SumPeriod)).ToList();
            return tmp.Select(list => list.Sum()).ToList();
        }


        public static List<double> Mf(List<Candle> source, int MfiPeriod)
        {
            // mf(src) => rsi(sum(volume * (change(src) <= 0 ? 0 : src), 3), sum(volume * (change(src) >= 0 ? 0 : src), 3))
            var change = source.Select(candle => candle.Close - candle.Open); // Изменение цены
            var more = source.Close().Zip(change, (cl, ch) => ch <= 0 ? 0 : cl); // change(src) <= 0 ? 0 : src
            var less = source.Close().Zip(change, (cl, ch) => ch >= 0 ? 0 : cl); // change(src) >= 0 ? 0 : src
            var sumM = SumSliding(source.Volume().Zip(more, (v, m) => v * m).ToList(), MfiPeriod); // sum(volume * (change(src) <= 0 ? 0 : src), 3)
            var sumL = SumSliding(source.Volume().Zip(less, (v, l) => v * l).ToList(), MfiPeriod); // sum(volume * (change(src) >= 0 ? 0 : src), 3)

            return RSI(sumM, sumL).ToList();

        }
        public static List<double> Mfi(List<Candle> source, int MfiPeriod)
        {
            double[] mfiValues = new double[source.Count];

            var highs = source.Select(x => Convert.ToDouble(x.High)).ToArray();
            var lows = source.Select(x => Convert.ToDouble(x.Low)).ToArray();
            var closes = source.Select(x => Convert.ToDouble(x.Close)).ToArray();
            var volumes = source.Select(x => Convert.ToDouble(x.Volume)).ToArray();

            var mfi = TA.Mfi(0, source.Count - 1, highs, lows, closes, volumes, MfiPeriod, out int outBegIdx, out int outNbElement, mfiValues);

            if (mfi == TA.RetCode.Success)
            {
                return mfiValues.Take(outNbElement + 1).Skip(outBegIdx).ToList();
            }

            throw new Exception("Could not calculate MFI!");

        }

        public static IEnumerable<T> ZipEnd<T, T1, T2, T3>(IEnumerable<T1> first, IEnumerable<T2> second, IEnumerable<T3> third, Func<T1, T2, T3, T> result)
        {
            var fR = first.Reverse();
            var sR = second.Reverse();
            var tR = third.Reverse();

            var fsr = fR.Zip(sR, (f, s) => (f, s)).Zip(tR, (fs, t) => (fs.f, fs.s, t));

            return fsr.Reverse().Select(x => result(x.f, x.s, x.t));
        }

        public static IEnumerable<T> ZipEnd<T1, T2, T3, T4, T5, T6, T7, T>(
            IEnumerable<T1> first, IEnumerable<T2> second, IEnumerable<T3> third,
            IEnumerable<T4> fourth, IEnumerable<T5> fifth, IEnumerable<T6> sixth,
            IEnumerable<T7> seventh, Func<T1, T2, T3, T4, T5, T6, T7, T> result)
        {
            var fR = first.Reverse();
            var sR = second.Reverse();
            var tR = third.Reverse();
            var fH = fourth.Reverse();
            var fF = fifth.Reverse();
            var sX = sixth.Reverse();
            var sV = seventh.Reverse();


            var fs = fR.Zip(sR, (s1, s2) => (s1, s2));
            var tf = tR.Zip(fH, (s3, s4) => (s3, s4));
            var f6 = fF.Zip(sX, (s5, s6) => (s5, s6)).Zip(sV, (f, s7) => (f.s5, f.s6, s7));

            var fsrf = fs.Zip(tf, (f, s) => (f.s1, f.s2, s.s3, s.s4)).Zip(f6, (f, ff) => (f.s1, f.s2, f.s3, f.s4, ff.s5, ff.s6, ff.s7));


            return fsrf.Reverse().Select(x => result(x.s1, x.s2, x.s3, x.s4, x.s5, x.s6, x.s7));
        }

        public static IEnumerable<T> ZipEnd<T1, T2, T3, T4, T5, T6, T7, T8, T>(
            IEnumerable<T1> first, IEnumerable<T2> second, IEnumerable<T3> third,
            IEnumerable<T4> fourth, IEnumerable<T5> fifth, IEnumerable<T6> sixth,
            IEnumerable<T7> seventh, IEnumerable<T8> eighth, Func<T1, T2, T3, T4, T5, T6, T7, T8, T> result)
        {
            var fR = first.Reverse();
            var sR = second.Reverse();
            var tR = third.Reverse();
            var fH = fourth.Reverse();
            var fF = fifth.Reverse();
            var sX = sixth.Reverse();
            var sV = seventh.Reverse();
            var eG = eighth.Reverse();


            var fs = fR.Zip(sR, (s1, s2) => (s1, s2));
            var tf = tR.Zip(fH, (s3, s4) => (s3, s4));
            var f6 = fF.Zip(sX, (s5, s6) => (s5, s6));
            var sE = sV.Zip(eG, (s7, s8) => (s7, s8));

            var fstf = fs.Zip(tf, (f, s) => (f.s1, f.s2, s.s3, s.s4));
            var fsse = f6.Zip(sE, (f, s) => (f.s5, f.s6, s.s7, s.s8));

            var ret = fstf.Zip(fsse, (f, ff) => (f.s1, f.s2, f.s3, f.s4, ff.s5, ff.s6, ff.s7, ff.s8));


            return ret.Reverse().Select(x => result(x.s1, x.s2, x.s3, x.s4, x.s5, x.s6, x.s7, x.s8));
        }

        public static IEnumerable<T> ZipEnd<T, T1, T2>(IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, T2, T> result)
        {
            var fR = first.Reverse();
            var sR = second.Reverse();

            var fs = fR.Zip(sR, (f, s) => (f, s));

            return fs.Reverse().Select(x => result(x.f, x.s));
        }

        public static List<double> Ema(List<double> source, int period)
        {
            // int outBegIdx, outNbElement;
            double[] emaValues = new double[source.Count];
            //List<double?> outValues = new List<double?>();

            var sourceFix = source.ToArray();

            var sma = TA.Ema(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, emaValues);

            if (sma == TA.RetCode.Success)
            {
                return emaValues.Take(outNbElement).ToList();
            }

            throw new Exception("Could not calculate EMA!");
        }

        public static List<double> Ema(List<decimal> source, int period) => Ema(source.Select(x => Convert.ToDouble(x)).ToList(), period);
        public static Pine Ema(Pine source, int period)
        {
            double[] emaValues = new double[source.Count];

            var sourceFix = source.ToArray();

            var sma = TA.Ema(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, emaValues);

            if (sma == TA.RetCode.Success)
                return emaValues.Take(outNbElement).ToPine();

            throw new Exception("Could not calculate EMA!");
        }


        public static Pine RSI(Pine source, int period) => RSI((IEnumerable<double>)source, period).ToPine();
        public static List<double> RSI(List<decimal> source, int period)
        {
            // int outBegIdx, outNbElement;
            double[] rsiValues = new double[source.Count];

            var sourceFix = source.Select(x => Convert.ToDouble(x)).ToArray();

            var ema = TA.Rsi(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, rsiValues);

            if (ema == TA.RetCode.Success)
            {
                return rsiValues.Take(outNbElement).ToList();
            }

            throw new Exception("Could not calculate RSI!");
        }
        public static List<double> RSI(IEnumerable<double> source, int period)
        {
            // int outBegIdx, outNbElement;
            double[] rsiValues = new double[source.Count()];

            var sourceFix = source.ToArray();

            var ema = TA.Rsi(0, source.Count() - 1, sourceFix, period, out int outBegIdx, out int outNbElement, rsiValues);

            if (ema == TA.RetCode.Success)
            {
                return rsiValues.Take(outNbElement).ToList();
            }

            throw new Exception("Could not calculate RSI!");
        }
        public static List<double> RSI(List<double> sourceOne, List<double> sourceTwo)
        {
            var rs = ZipEnd(sourceOne, sourceTwo, (s1, s2) => (s1, s2));
            return rs.Select(r => r.s2 == 0 ? 100.0 : 100.0 - 100.0 / (1 + r.s1 / r.s2)).ToList();
        }

        public static Pine RSI(Pine sourceOne, Pine sourceTwo)
        {
            var rs = ZipEnd(sourceOne, sourceTwo, (s1, s2) => (s1, s2));
            return rs.Select(r => r.s2 == 0 ? 100.0 : 100.0 - 100.0 / (1 + r.s1 / r.s2)).ToPine();
        }

        public static Pine Abs(Pine pine) => pine.Select(num => Math.Abs(num)).ToPine();

        public static Pine Change(Pine pine) => pine.Skip(1).Zip(pine, (val, old) => val - old).ToPine();

        public static Pine Slope(Pine source, int period = 14)
        {
            // int outBegIdx, outNbElement;
            double[] outValues = new double[source.Count];

            var sourceFix = source.ToArray();

            var ema = TA.LinearRegSlope(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, outValues);

            if (ema == TA.RetCode.Success)
            {
                return outValues.Take(outNbElement).ToPine();
            }

            throw new Exception("Could not calculate RSI!");
        }
        public static Pine Intercept(Pine source, int period = 14)
        {
            // int outBegIdx, outNbElement;
            double[] outValues = new double[source.Count];

            var sourceFix = source.ToArray();

            var ema = TA.LinearRegIntercept(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, outValues);

            if (ema == TA.RetCode.Success)
            {
                return outValues.Take(outNbElement).ToPine();
            }

            throw new Exception("Could not calculate RSI!");
        }
        public static Pine Linreg(Pine source, int period, int offset = 0)
        {

            Pine intercept = Intercept(source, period);
            Pine slope = Slope(source, period);

            return intercept + slope * (period - 1 - offset);
        }
        public static Pine Linreg(Pine source, int period = 14)
        {
            // int outBegIdx, outNbElement;
            double[] outValues = new double[source.Count];

            var sourceFix = source.ToArray();

            var ema = TA.LinearReg(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, outValues);

            if (ema == TA.RetCode.Success)
            {
                return outValues.Take(outNbElement).ToPine();
            }

            throw new Exception("Could not calculate RSI!");
        }

        public static List<double> Linreg(List<double> source, int period = 14)
        {
            // int outBegIdx, outNbElement;
            double[] outValues = new double[source.Count];

            var sourceFix = source.ToArray();

            var ema = TA.LinearReg(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, outValues);

            if (ema == TA.RetCode.Success)
            {
                return outValues.Take(outNbElement).ToList();
            }

            throw new Exception("Could not calculate RSI!");
        }

        public static List<decimal> Linreg(List<decimal> source, int period = 14)
        {
            // int outBegIdx, outNbElement;
            double[] outValues = new double[source.Count];

            var sourceFix = source.Select(x => Convert.ToDouble(x)).ToArray();

            var ema = TA.LinearReg(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, outValues);

            if (ema == TA.RetCode.Success)
            {
                return outValues.Take(outNbElement).Select(x => Convert.ToDecimal(x)).ToList();
            }

            throw new Exception("Could not calculate RSI!");
        }

        public static IEnumerable<IEnumerable<T>> SplitSegments<T>(this IEnumerable<T> source, int lengthSegment, SplitSegmentsOptions SplitOptions = SplitSegmentsOptions.Slide)
        {
            int stepSegment = 0;
            switch (SplitOptions)
            {
                case SplitSegmentsOptions.Slide: stepSegment = 1; break;
                case SplitSegmentsOptions.Step: stepSegment = lengthSegment; break;
            }
            IEnumerable<IEnumerable<T>> ret = Enumerable.Empty<IEnumerable<T>>();
            if (lengthSegment < 1)
                return ret;
            IEnumerable<T> tmp;
            while (((tmp = source.Take(lengthSegment)).Count() == lengthSegment))
            {
                ret = ret.Append(tmp);
                source = source.Skip(stepSegment);
            }
            return ret;
        }

        public static Pine Sma(Pine source, int period = 30)
        {
            // int outBegIdx, outNbElement;
            double[] smaValues = new double[source.Count];
            //List<double?> outValues = new List<double?>();

            var sourceFix = source.ToArray();

            var sma = TA.Sma(0, source.Count - 1, sourceFix, period, out int outBegIdx, out int outNbElement, smaValues);

            if (sma == TA.RetCode.Success)
            {
                return smaValues.Take(outNbElement).ToPine(); /*FixIndicatorOrderingD(smaValues.ToList(), outBegIdx, outNbElement);*/
            }

            throw new Exception("Could not calculate SMA!");
        }

        public static PineNA ValueWhen(PineNA condition, Pine source, uint occurrence = 0)
        {
            PineNA ret = new PineNA();
            Queue<double> Queue = new Queue<double>();
            foreach ((double? cond, double sour) tuple in ZipEnd(condition, source, (cond, sour) => (cond, sour)))
            {
                if (tuple.cond == null)
                    ret.Add(null);
                else
                {
                    Queue.Enqueue(tuple.sour);
                    if (occurrence == 0)
                        ret.Add(Queue.Dequeue());
                    else
                    {
                        ret.Add(null);
                        occurrence--;
                    }
                }

            }
            return ret;
        }

        public static PineBool CrossUnder(Pine xSource, Pine ySource)
        {
            IEnumerable<(double x, double y)> tuples = ZipEnd(xSource, ySource, (x, y) => (x, y));
            PineBool ret = new PineBool();
            (double x, double y) prev = tuples.First();
            ret.Add(prev.x == prev.y);
            tuples = tuples.Skip(1);
            foreach ((double x, double y) tupl in tuples)
            {
                ret.Add(tupl.x == tupl.y || (tupl.x < tupl.y && prev.x > prev.y));
                prev = tupl;
            }
            return ret;

        }
        public static PineBool CrossOver(Pine xSource, Pine ySource)
        {
            IEnumerable<(double x, double y)> tuples = ZipEnd(xSource, ySource, (x, y) => (x, y));
            PineBool ret = new PineBool();
            (double x, double y) prev = tuples.First();
            ret.Add(prev.x == prev.y);
            tuples = tuples.Skip(1);
            foreach ((double x, double y) tupl in tuples)
            {
                ret.Add(tupl.x == tupl.y || (tupl.x > tupl.y && prev.x < prev.y));
                prev = tupl;
            }
            return ret;

        }
        //public static Pine BarsSince(PineBool source)
        //{
        //    Pine ret = new Pine();
                      
        //        foreach (bool x in source)
        //    {
        //        if (x)
        //            count++;
        //         else if()   
                    
        //        else
        //            count = 0;
        //        ret.Add(count);
        //    }
        //    return ret;
            
        //}


        public static Pine BarsSince(PineBool source)
        {
            

            Pine buf = new Pine();
            double count = 0;
          
            for (int i=0; i<source.Count;i++)
            {
                if(source[i])
                {
                    count = 1;
                }
                else if (i > 0 && !source[i] && buf.ElementAt(i -1) > 0)
                {
                    count++;
                }
                else
                {
                    count = 0;
                }
                buf.Add(count);
                
            }
            buf.Reverse();
            return buf;
            

        }

    }
}
