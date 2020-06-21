using System.Collections.Generic;
using System.Linq;
using TA = BitMexLibrary.TradingAlgos;
using System.Xml.Linq;
using Bitmex.NET.Dtos;
using System;

namespace BitMexLibrary
{
    //Только доп. функции
    public partial class TradingSystem
    {
        Pine TSIfor3min(Pine pine)
        {//csi = 90+91 * (ema(ema(low-low[1], 36), 2) / ema(ema(abs(low-low[1]), 36), 2))

            Pine emaSrc36 = TA.Ema(pine, 36);
            Pine emaSrc36abs = TA.Ema(TA.Abs(pine), 36);
            Pine emaemaSrc36 = TA.Ema(emaSrc36, 2);
            Pine emaemaSrc36abs = TA.Ema(emaSrc36abs, 2);
            Pine ret = 90 + (91 * emaemaSrc36) / emaemaSrc36abs;
            return ret;
        }
        Pine TCI(Pine pine)
        {
            //Pine emaSrc35, src_emaSrc35, abs_src_emaSrc35, emaAbs, comm, ret;
            Pine
            emaSrc35 = TA.Ema(pine, 300);
            Pine src_emaSrc35 = pine - emaSrc35;
            Pine abs_src_emaSrc35 = TA.Abs(src_emaSrc35);
            Pine emaAbs = TA.Ema(abs_src_emaSrc35, 300);
            Pine comm = src_emaSrc35 / (0.025 * emaAbs);
            Pine ret = TA.Ema(comm, 50) + 50;
            return ret;
        }
        Pine MF(Pine pine, Pine pVolume)
        {
            var pChange = TA.Change(pine);
            var chnM = TA.ZipEnd(pChange, pine, (ch, cl) => ch <= 0 ? 0 : cl).ToPine();
            var sumChnM = TA.Sum(pVolume * chnM, 15);

            var chnL = TA.ZipEnd(pChange, pine, (ch, cl) => ch >= 0 ? 0 : cl).ToPine();
            var sumChnL = TA.Sum(pVolume * chnL, 15);

            //mf(src) => rsi(sum(volume * (change(src) <= 0 ? 0 : src), 3), sum(volume * (change(src) >= 0 ? 0 : src), 3))

            return TA.RSI(sumChnM, sumChnL);

            //var result = TradingAlgos.Alroritm(candles6, 35, 5, 3);
        }

        Pine Willy(Pine pine)
        {
            var highest = TA.Highest(pine, 3);
            var lowest = TA.Lowest(pine, 3);
            return 60.0 * (pine - highest) / (highest - lowest) + 80;
        }
        Pine Tradition(Pine pine, Pine pVolume)
        {

            var tci = TCI(pine);
            var mf = MF(pine, pVolume);
            var rsi = TA.RSI(pine, 15);

            //OutValues.Add("tci", tci);
            //OutValues.Add("mf", mf);
            //OutValues.Add("rsi", rsi);
            return Pine.ZipEnd(tci, mf, rsi, (t, m, r) => (t + m + r) / 3.0);
        }

        Pine f_fractalize(Pine pine)
        {
            double minMaxFract(IEnumerable<double> segm)
            {
                int index = segm.Count() / 2;
                double elm = segm.ElementAt(index);
                IEnumerable<double> items = segm.Take(index).Concat(segm.Skip(index + 1));

                if (items.Max() < elm) return 1.0;
                if (items.Min() > elm) return -1.0;
                return 0.0;
            }

            return pine.SplitSegments(5).Select(x => minMaxFract(x)).ToPine();
        }

        #region Переименование TA функций
        Pine Change(Pine pine) => TA.Change(pine);
        Pine Sma(Pine pine, int period) => TA.Sma(pine, period);
        Pine Linreg(Pine source, int period, int offset) => TA.Linreg(source, period, offset);
        Pine Ema(Pine source, int period) => TA.Ema(source, period);
        Pine ValueWhen(PineNA condition, Pine source, uint occurrence) => TA.ValueWhen(condition, source, occurrence).ToPine(Enums.Approximation.Step);
        Pine BarsSince(PineBool source) => TA.BarsSince(source);
        PineBool CrossUnder(Pine xSource, Pine ySource) => TA.CrossUnder(xSource, ySource);
        PineBool CrossOver(Pine xSource, Pine ySource) => TA.CrossOver(xSource, ySource);
        #endregion

        //int countAdd = -1;

        //void plot<T>(string Header, IEnumerable<T> Items)
        //{
        //    Header = Header.Trim();
        //    if (Header == "Время" && OutValues.ColumnHeaders.Contains("Время"))
        //    {
        //        var timeCol = OutValues.Columns["Время"];
        //        countAdd = timeCol.Items.Count(x => DateTime.Parse(x,"t"))
        //    }
        //}
        //OutValues.Add("Close", close);



    }

    public static class TradeBucketedDtoExtensions
    {
        public static bool TryParseXML(this string source, out XElement xElement)
        {
            try
            {
                xElement = XElement.Parse(source);
                return true;
            }
            catch (Exception)
            {
                xElement = null;
                return false;
            }
        }

        public static List<decimal?> High(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => x.High).ToList();
        }

        public static Pine HighToPine(this List<TradeBucketedDto> source)
        {
            Pine p = new Pine();
            List<double?> boo = new List<double?>();
            foreach (var item in source.OrderBy(x => x.Timestamp))
            {
                boo.Add(item.High == null ? null : (double?)Convert.ToDouble(item.High));//       
            }

            p = boo.ToPine(Enums.Approximation.LinearNew);

            return p;
        }

        public static List<decimal?> Low(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => x.Low).ToList();
        }

        public static Pine LowToPine(this List<TradeBucketedDto> source)
        {
            Pine p = new Pine();
            List<double?> boo = new List<double?>();
            foreach (var item in source.OrderBy(x => x.Timestamp))
            {
                boo.Add(item.Low == null ? null : (double?)Convert.ToDouble(item.Low));//       
            }

            p = boo.ToPine(Enums.Approximation.LinearNew);

            return p;
        }

        public static List<decimal?> Open(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => x.Open).ToList();
        }

        public static List<decimal?> Close(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => x.Close).ToList();
        }
        public static Pine CloseToPine(this List<TradeBucketedDto> source)
        {
            Pine p = new Pine();
            List<double?> boo = new List<double?>();
            foreach (var item in source.OrderBy(x => x.Timestamp))
            {
                boo.Add(item.Close == null ? null : (double?)Convert.ToDouble(item.Close));
            }

            p = boo.ToPine(Enums.Approximation.LinearNew);

            return p;
        }

        public static List<decimal?> getClose(this List<TradeBucketedDto> source)
        {
            return source.Select(x => x.Close).ToList();
        }

        public static List<DateTime> TimeStamp(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => x.Timestamp.UtcDateTime).ToList();
        }

        public static List<DateTime> TimeStampDateTime(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => x.Timestamp.DateTime).ToList();
        }

        public static List<DateTime> TimeOpen(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => x.Timestamp.UtcDateTime).ToList();
        }

        public static List<decimal?> Volume(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => x.Volume).ToList();
        }

        public static Pine VolumeToPine(this List<TradeBucketedDto> source)
        {
            Pine p = new Pine();
            List<double?> boo = new List<double?>();
            foreach (var item in source.OrderBy(x => x.Timestamp))
            {
                boo.Add(item.Volume == null ? null : (double?)Convert.ToDouble(item.Volume));//       
            }

            p = boo.ToPine(Enums.Approximation.LinearNew);

            return p;
        }

        public static List<decimal?> Hl2(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => (x.High + x.Low) / 2).ToList();
        }

        public static List<decimal?> Hlc3(this List<TradeBucketedDto> source)
        {
            return source.OrderBy(x => x.Timestamp).Select(x => (x.High + x.Low + x.Close) / 3).ToList();
        }
    }
}
