using BitMexLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BitMexLibrary
{
    public static class CandleExtensions
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

        public static List<decimal> High(this List<Candle> source)
        {
            return source.OrderBy(x => x.TimeStamp).Select(x => x.High).ToList();
        }

        public static List<decimal> Low(this List<Candle> source)
        {
            return source.OrderBy(x => x.TimeStamp).Select(x => x.Low).ToList();
        }

        public static List<decimal> Open(this List<Candle> source)
        {
            return source.OrderBy(x => x.TimeStamp).Select(x => x.Open).ToList();
        }

        public static List<decimal> Close(this List<Candle> source)
        {
            return source.OrderBy(x => x.TimeStamp).Select(x => x.Close).ToList();
        }

        public static List<DateTime> TimeStamp(this List<Candle> source)
        {
            return source.OrderBy(x => x.TimeStamp).Select(x => x.TimeStamp).ToList();
        }

        public static List<DateTime> TimeOpen(this List<Candle> source)
        {
            return source.OrderBy(x => x.TimeStamp).Select(x => x.TimeOpen).ToList();
        }

        public static List<decimal> Volume(this List<Candle> source)
        {
            return source.OrderBy(x => x.TimeStamp).Select(x => x.Volume).ToList();
        }

        public static List<decimal> Hl2(this List<Candle> source)
        {
            return source.OrderBy(x => x.TimeStamp).Select(x => (x.High + x.Low) / 2).ToList();
        }

        public static List<decimal> Hlc3(this List<Candle> source)
        {
            return source.OrderBy(x => x.TimeStamp).Select(x => (x.High + x.Low + x.Close) / 3).ToList();
        }



        /// <summary>
        /// Для данных свечи с неравными интервалами 
        /// (т.е. для объёмом сделок, которые не имеют активности между двумя периодами), 
        /// заполняя промежутки, расширяя свечу, предшествующую промежутку до следующей свечи. 
        /// Обычно это большая проблема с малых объемов с короткими интервалами.        /// </summary>
        /// <param name="candles">Candle list containing time gaps.</param>
        /// <param name="period">Period of candle.</param>
        /// <returns></returns>
        public static List<Candle> FillCandleGaps(this List<Candle> candles, Period period)
        {
            if (!candles.Any())
                return candles;

            // Candle response
            var filledCandles = new List<Candle>();
            var orderedCandles = candles.OrderBy(x => x.TimeStamp).ToList();

            // Datetime variables
            DateTime nextTime;
            DateTime startDate = orderedCandles.First().TimeStamp;
            DateTime endDate = DateTime.UtcNow;

            // Walk through the candles and fill any gaps
            for (int i = 0; i < orderedCandles.Count() - 1; i++)
            {
                var c1 = orderedCandles[i];
                var c2 = orderedCandles[i + 1];
                filledCandles.Add(c1);
                nextTime = c1.TimeStamp.AddMinutes(period.ToMinutesEquivalent());
                while (nextTime < c2.TimeStamp)
                {
                    var cNext = c1;
                    cNext.TimeStamp = nextTime;
                    filledCandles.Add(cNext);
                    nextTime = cNext.TimeStamp.AddMinutes(period.ToMinutesEquivalent());

                }
            }

            return filledCandles;
        }
    }
}
