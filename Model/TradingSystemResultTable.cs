using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace BitMexLibrary
{
    [Serializable]
    public class CalculationResult
    {
        private DateTimeOffset _TimeStamp;
        private string _Close;
        private string _low;
        private string _high;
        private string _csi;
        private string _wt1;
        private string _lowlow1;
        private string _longCond;
        private string _shortCond;
        private double _longShortCond;
        private string _handCond;
        private string _signal;
        private string _signalHandler;
        private string _longcond1;
        private string _longcond2;
        private string _longcond3;
        private string _longcond4;

        public DateTimeOffset TimeStamp { get => _TimeStamp; set => _TimeStamp = value; }
        public string Close { get => _Close; set => _Close = value; }
        public string Low { get => _low; set => _low = value; }
        public string High { get => _high; set => _high = value; }
        public string Csi { get => _csi; set => _csi = value; }
        public string Wt1 { get => _wt1; set => _wt1 = value; }
        public string Lowlow1 { get => _lowlow1; set => _lowlow1 = value; }
        public string LongCond { get => _longCond; set => _longCond = value; }
        public string ShortCond { get => _shortCond; set => _shortCond = value; }
        public double LongShortCond { get => _longShortCond; set => _longShortCond = value; }
        public string HandCond { get => _handCond; set => _handCond = value; }
        public string Signal { get => _signal; set => _signal = value; }
        public string SignalHandler { get => _signalHandler; set => _signalHandler = value; }
        public string Longcond1 { get => _longcond1; set => _longcond1 = value; }
        public string Longcond2 { get => _longcond2; set => _longcond2 = value; }
        public string Longcond3 { get => _longcond3; set => _longcond3 = value; }
        public string Longcond4 { get => _longcond4; set => _longcond4 = value; }





        public CalculationResult(DateTimeOffset timeStamp, string close, string low, string high, string csi, string wt1, string lowlow1, string longCond, string shortCond, double longShortCond, string handCond, string longcond1, string longcond2, string longcond3, string longcond4)
        {
            TimeStamp = timeStamp;
            Close = close;
            Low = low;
            High = high;
            Csi = csi;
            Wt1 = wt1;
            Lowlow1 = lowlow1;

            LongCond = longCond;
            ShortCond = shortCond;
            LongShortCond = longShortCond;
            HandCond = handCond;
            SignalHandler = "";
            Longcond1 = longcond1;
            Longcond2 = longcond2;
            Longcond3 = longcond3;
            Longcond4 = longcond4;

        }
    }

    public static class ExtensionMethods
    {
        // Deep clone
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
