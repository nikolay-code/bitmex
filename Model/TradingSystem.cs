using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bitmex.NET.Dtos;
using BitMexLibrary.Enums;
using NLog;
using TA = TicTacTec.TA.Library.Core;
namespace BitMexLibrary
{
    partial class TradingSystem
    {
        readonly double? na = null;
        public Logger logger = LogManager.GetCurrentClassLogger();

        public TradingSystem()
        {
        }

        public List<CalculationResult> RunSystem(List<TradeBucketedDto> src)
        {
            return BestStrategy(src);
        }

        private List<CalculationResult> BestStrategy(List<TradeBucketedDto> src)
        {
            //Передаем src - массив свечей, в порядке возрастания от 01.01.2014 - 12.12.2014
            //на выходе талб с результатом
            List<CalculationResult> result = new List<CalculationResult>();
            try
            {
                Pine close = src.CloseToPine();//src.Close().ToPine();
                Pine volume = src.VolumeToPine();
                Pine high = src.HighToPine();
                Pine low = src.LowToPine();
                //Pine hlc3 = (high + low + close) / 3.0;
                IEnumerable<DateTime> timeStamp = src.TimeStampDateTime();
                IEnumerable<DateTime> timeOpen = src.TimeOpen();

                #region strategy for 3 min




                Pine low_1 = low.Drop(1);

                Pine lowlow1 = low - low_1;


                Pine fast_ma = Ema(low, 496);
                Pine slow_ma = Ema(low, 14);
                Pine macd = fast_ma - slow_ma;
                Pine signal = Sma(macd, 7);

                Pine wt0 = Tradition(low, volume) + 40;
                Pine swt0 = Sma(wt0, 300);
                Pine wt1 = Ema(wt0, 6);

                Pine csi = TSIfor3min(lowlow1);

                Pine csi_1 = csi.Drop(1);
                Pine wt1_1 = wt1.Drop(1);
                Pine high_1 = high.Drop(1);
                PineBool shortsignals = CrossUnder(macd, signal);
                PineBool longcond1 = (csi - csi_1 > 12 & csi < 112 & low_1 < low & high > high_1);
                PineBool longcond2 = (csi < 65 & low_1 < low & high > high_1);
                PineBool longcond3 = (csi < 112 & (wt1 - wt1_1) > 3);
                PineBool longcond4 = (csi < 65 & (wt1 - wt1_1) > 3);



                PineBool longCond = ((longcond1 | longcond2 | longcond3 | longcond4) & macd < -150);
                PineBool shortCond = shortsignals;

                Pine longShortCond = PineBool.ZipEnd(longCond, shortCond, (ln, sh) => ln ? 1.0 : sh ? -1.0 : ((double?)null)).ToPineNA().ToPine(Approximation.Step);

                var handCond = Pine.ZipEnd(longShortCond, longShortCond.Drop(1), (lsc, lscd) => (lsc > 0.5 && lscd < -0.5) ? "Long" : (lsc < 0.5 && lscd > -0.5) ? "Short" : "");

                #endregion

                timeStamp = timeStamp.Reverse();
                close.Reverse();
                low.Reverse();
                high.Reverse();
                csi.Reverse();
                wt1.Reverse();

                longcond1.Reverse();
                longcond2.Reverse();
                longcond3.Reverse();
                longcond4.Reverse();


                longCond.Reverse();
                shortCond.Reverse();
                handCond = handCond.Reverse();

                /*longShortCond[longShortCond.Count-1] = 1;
                longShortCond[longShortCond.Count-2] = -1;
                longShortCond[longShortCond.Count-3] = 0;*/

                longShortCond.Reverse();
                for (int i = 0; i < longCond.Count; i++)
                {
                    result.Add(
                        new CalculationResult(
                             i < timeStamp.Count() ? timeStamp.ElementAt(i) : DateTimeOffset.UtcNow,
                             i < close.Count() ? close.ElementAt(i).ToString() : "",
                             i < low.Count() ? low.ElementAt(i).ToString() : "",
                             i < high.Count() ? high.ElementAt(i).ToString() : "",
                             i < csi.Count() ? csi.ElementAt(i).ToString() : "",
                             i < wt1.Count() ? wt1.ElementAt(i).ToString() : "",
                             i < lowlow1.Count() ? lowlow1.ElementAt(i).ToString() : "",
                             i < longCond.Count() ? longCond.ElementAt(i) ? "Yes" : "" : "",
                             i < shortCond.Count() ? shortCond.ElementAt(i) ? "Yes" : "" : "",
                             i < longShortCond.Count() ? longShortCond.ElementAt(i) : 0,
                             i < handCond.Count() ? handCond.ElementAt(i).ToString() : "",
                              i < longcond1.Count() ? longcond1.ElementAt(i).ToString() : "",
                               i < longcond2.Count() ? longcond2.ElementAt(i).ToString() : "",
                                i < longcond3.Count() ? longcond3.ElementAt(i).ToString() : "",
                                 i < longcond4.Count() ? longcond4.ElementAt(i).ToString() : ""
                             ));
                }

                //string lastSignalPumpOrSliv = "";
                for (int i = result.Count - 1; i > 0; i--)
                {
                    if (result[i - 1].LongShortCond == 1.0 && result[i].LongShortCond == -1.0)/* ||
                        result[i].Pump == "1" && result[i+1].Pump == "0")*/
                    {
                        result[i - 1].Signal = "Buy";

                    }

                    if (result[i - 1].LongShortCond == -1.0 && result[i].LongShortCond == 1.0)/* ||
                        result[i].Sliv == "1" && result[i+1].Sliv == "0")*/
                    {
                        result[i - 1].Signal = "Sell";
                    }

                }

            }
            catch (Exception ex)
            {
                logger.Debug("BestStrategy error" + ex.Message);
            }

            return result;
        }
    }
}
