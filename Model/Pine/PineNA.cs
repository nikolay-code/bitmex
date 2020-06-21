using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMexLibrary
{
    public class PineNA : List<double?>
    {
        #region переопределение сложения с PineNA с числом
        public static PineNA operator +(PineNA pine, double? number)
        {
            PineNA ret = new PineNA();
            foreach (double? num in pine)
                ret.Add(num + number);
            return ret;
        }
        public static PineNA operator +(double? number, PineNA pine) => pine + number;
        public static PineNA operator +(decimal? number, PineNA pine) => pine + (double?)number;
        public static PineNA operator +(PineNA pine, decimal? number) => pine + (double?)number;
        public static PineNA operator +(int? number, PineNA pine) => pine + (double?)number;
        public static PineNA operator +(PineNA pine, int? number) => pine + (double?)number;
        #endregion

        #region переопределение умножения с PineNA с числом
        public static PineNA operator *(PineNA pine, double? number)
        {
            PineNA ret = new PineNA();
            foreach (double? num in pine)
                ret.Add(num * number);
            return ret;
        }
        public static PineNA operator *(double? number, PineNA pine) => pine * number;
        public static PineNA operator *(decimal? number, PineNA pine) => pine * (double?)number;
        public static PineNA operator *(PineNA pine, decimal? number) => pine * (double?)number;
        public static PineNA operator *(int? number, PineNA pine) => pine * (double?)number;
        public static PineNA operator *(PineNA pine, int? number) => pine * (double?)number;
        #endregion

        #region переопределение вычитания с PineNA с числом
        public static PineNA operator -(PineNA pine, double? number)
        {
            PineNA ret = new PineNA();
            foreach (double? num in pine)
                ret.Add(num - number);
            return ret;
        }

        public static PineNA operator -(double? number, PineNA pine)
        {
            PineNA ret = new PineNA();
            foreach (double? num in pine)
                ret.Add(number - num);
            return ret;
        }

        public static PineNA operator -(decimal? number, PineNA pine) => (double?)number - pine;
        public static PineNA operator -(PineNA pine, decimal? number) => pine - (double?)number;
        public static PineNA operator -(int? number, PineNA pine) => (double?)number - pine;
        public static PineNA operator -(PineNA pine, int? number) => pine - (double?)number;
        #endregion

        #region переопределение деления с PineNA с числом
        public static PineNA operator /(PineNA pine, double? number)
        {
            PineNA ret = new PineNA();
            foreach (double? num in pine)
                ret.Add(num / number);
            return ret;
        }
        public static PineNA operator /(double? number, PineNA pine)
        {
            PineNA ret = new PineNA();
            foreach (double? num in pine)
                ret.Add(number / num);
            return ret;
        }

        public static PineNA operator /(PineNA pine, decimal? number) => pine / (double?)number;
        public static PineNA operator /(decimal? number, PineNA pine) => (double?)number / pine;
        public static PineNA operator /(PineNA pine, int? number) => pine / (double?)number;
        public static PineNA operator /(int? number, PineNA pine) => (double?)number / pine;
        #endregion

        #region переопределение операторов с PineNA с PineNA
        public static PineNA operator +(PineNA pine1, PineNA pine2) => ZipEnd(pine1, pine2, (p1, p2) => p1 + p2);
        public static PineNA operator -(PineNA pine1, PineNA pine2) => ZipEnd(pine1, pine2, (p1, p2) => p1 - p2);
        public static PineNA operator *(PineNA pine1, PineNA pine2) => ZipEnd(pine1, pine2, (p1, p2) => p1 * p2);
        public static PineNA operator /(PineNA pine1, PineNA pine2) => ZipEnd(pine1, pine2, (p1, p2) => p1 / p2);
        #endregion

        #region Методы соединения PineNA с выравниванием по последнему элементу
        public static PineNA ZipEnd(PineNA first, PineNA second, PineNA third, Func<double?, double?, double?, double?> result)
        {
            var fR = ((IEnumerable<double?>)first).Reverse();
            var sR = ((IEnumerable<double?>)second).Reverse();
            var tR = ((IEnumerable<double?>)third).Reverse();

            var fsr = fR.Zip(sR, (f, s) => (f, s)).Zip(tR, (fs, t) => (fs.f, fs.s, t));

            return fsr.Reverse().Select(x => result(x.f, x.s, x.t)).ToPineNA();
        }

        public static PineNA ZipEnd(PineNA first, PineNA second, PineNA third, PineNA fourth, Func<double?, double?, double?, double?, double?> result)
        {
            var fR = ((IEnumerable<double?>)first).Reverse();
            var sR = ((IEnumerable<double?>)second).Reverse();
            var tR = ((IEnumerable<double?>)third).Reverse();
            var fH = ((IEnumerable<double?>)fourth).Reverse();


            var fs = fR.Zip(sR, (f, s) => (s1: f, s2: s));
            var tf = tR.Zip(fH, (t, f4) => (s3: t, s4: f4));

            var fsrf = fs.Zip(tf, (f, s) => (f.s1, f.s2, s.s3, s.s4));

            return fsrf.Reverse().Select(x => result(x.s1, x.s2, x.s3, x.s4)).ToPineNA();
        }
        public static PineNA ZipEnd(
            PineNA first, PineNA second, PineNA third, PineNA fourth,
            PineNA fifth, PineNA sixth, PineNA seventh, PineNA eighth,
            Func<double?, double?, double?, double?, double?, double?, double?, double?, double?> result)
        {
            var fR = ((IEnumerable<double?>)first).Reverse();
            var sR = ((IEnumerable<double?>)second).Reverse();
            var tR = ((IEnumerable<double?>)third).Reverse();
            var fH = ((IEnumerable<double?>)fourth).Reverse();
            var fF = ((IEnumerable<double?>)fifth).Reverse();
            var sX = ((IEnumerable<double?>)sixth).Reverse();
            var sV = ((IEnumerable<double?>)seventh).Reverse();
            var eG = ((IEnumerable<double?>)eighth).Reverse();


            var fs = fR.Zip(sR, (s1, s2) => (s1, s2));
            var tf = tR.Zip(fH, (s3, s4) => (s3, s4));
            var f6 = fF.Zip(sX, (s5, s6) => (s5, s6));
            var sE = sV.Zip(eG, (s7, s8) => (s7, s8));

            var fstf = fs.Zip(tf, (f, s) => (f.s1, f.s2, s.s3, s.s4));
            var fsse = f6.Zip(sE, (f, s) => (f.s5, f.s6, s.s7, s.s8));

            var ret = fstf.Zip(fsse, (f, ff) => (f.s1, f.s2, f.s3, f.s4, ff.s5, ff.s6, ff.s7, ff.s8));

            return ret.Reverse().Select(x => result(x.s1, x.s2, x.s3, x.s4, x.s5, x.s6, x.s7, x.s8)).ToPineNA();
        }
        public static PineNA ZipEnd(PineNA first, PineNA second, PineNA third, PineNA fourth, PineNA fifth, PineNA sixth, PineNA seventh,
            Func<double?, double?, double?, double?, double?, double?, double?, double?> result)
        {
            var fR = ((IEnumerable<double?>)first).Reverse();
            var sR = ((IEnumerable<double?>)second).Reverse();
            var tR = ((IEnumerable<double?>)third).Reverse();
            var fH = ((IEnumerable<double?>)fourth).Reverse();
            var fF = ((IEnumerable<double?>)fifth).Reverse();
            var sX = ((IEnumerable<double?>)sixth).Reverse();
            var sV = ((IEnumerable<double?>)seventh).Reverse();


            var fs = fR.Zip(sR, (s1, s2) => (s1, s2));
            var tf = tR.Zip(fH, (s3, s4) => (s3, s4));
            var f6 = fF.Zip(sX, (s5, s6) => (s5, s6)).Zip(sV, (f, s7) => (f.s5, f.s6, s7));

            var fsrf = fs.Zip(tf, (f, s) => (f.s1, f.s2, s.s3, s.s4)).Zip(f6, (f, ff) => (f.s1, f.s2, f.s3, f.s4, ff.s5, ff.s6, ff.s7));

            return fsrf.Reverse().Select(x => result(x.s1, x.s2, x.s3, x.s4, x.s5, x.s6, x.s7)).ToPineNA();
        }
        public static PineNA ZipEnd<T>(PineNA first, PineNA second, PineNA third, PineNA fourth, PineNA fifth, PineNA sixth,
            Func<double?, double?, double?, double?, double?, double?, double?> result)
        {
            var fR = ((IEnumerable<double?>)first).Reverse();
            var sR = ((IEnumerable<double?>)second).Reverse();
            var tR = ((IEnumerable<double?>)third).Reverse();
            var fH = ((IEnumerable<double?>)fourth).Reverse();
            var fF = ((IEnumerable<double?>)fifth).Reverse();
            var sX = ((IEnumerable<double?>)sixth).Reverse();


            var fs = fR.Zip(sR, (f, s) => (s1: f, s2: s));
            var tf = tR.Zip(fH, (t, f4) => (s3: t, s4: f4));
            var f6 = fF.Zip(sX, (f5, s6) => (s5: f5, s6));

            var fsrf = fs.Zip(tf, (f, s) => (f.s1, f.s2, s.s3, s.s4)).Zip(f6, (f, ff) => (f.s1, f.s2, f.s3, f.s4, ff.s5, ff.s6));

            return fsrf.Reverse().Select(x => result(x.s1, x.s2, x.s3, x.s4, x.s5, x.s6)).ToPineNA();
        }

        public static PineNA ZipEnd(PineNA first, PineNA second, PineNA third, PineNA fourth, PineNA fifth, Func<double?, double?, double?, double?, double?, double?> result)
        {
            var fR = ((IEnumerable<double?>)first).Reverse();
            var sR = ((IEnumerable<double?>)second).Reverse();
            var tR = ((IEnumerable<double?>)third).Reverse();
            var fH = ((IEnumerable<double?>)fourth).Reverse();
            var fF = ((IEnumerable<double?>)fifth).Reverse();


            var fs = fR.Zip(sR, (f, s) => (s1: f, s2: s));
            var tf = tR.Zip(fH, (t, f4) => (s3: t, s4: f4));

            var fsrff = fs.Zip(tf, (f, s) => (f.s1, f.s2, s.s3, s.s4)).Zip(fF, (f, ff) => (f.s1, f.s2, f.s3, f.s4, s5: ff));

            return fsrff.Reverse().Select(x => result(x.s1, x.s2, x.s3, x.s4, x.s5)).ToPineNA();
        }

        public static PineNA ZipEnd(PineNA first, PineNA second, Func<double?, double?, double?> result)
        {
            var fR = ((IEnumerable<double?>)first).Reverse();
            var sR = ((IEnumerable<double?>)second).Reverse();

            var fs = fR.Zip(sR, (f, s) => (f, s));

            return fs.Reverse().Select(x => result(x.f, x.s)).ToPineNA();
        }
        #endregion

        #region Метод IIF - условного выбора
        /// <summary>Выбирает возвращаемые значения из элементов двух последовательностей по условию в первой последовательности</summary>
        /// <param name="pineBool">Последовательность задающая условие выбора</param>
        /// <param name="pineTrue">Последовательность для True</param>
        /// <param name="pineFalse">Последовательность для False</param>
        /// <returns></returns>
        public static PineNA IIF(PineBool pineBool, PineNA pineTrue, PineNA pineFalse)
        {
            var bl = ((IEnumerable<bool>)pineBool).Reverse();
            var tr = ((IEnumerable<double?>)pineTrue).Reverse();
            var fl = ((IEnumerable<double?>)pineFalse).Reverse();

            var btf = bl.Zip(tr, (b, t) => (b, t)).Zip(fl, (bt, f) => (bt.b, bt.t, f));

            return btf.Reverse().Select(x => x.b ? x.t : x.f).ToPineNA();
        }
        public static PineNA IIF(PineBool pineBool, double? pineTrue, PineNA pineFalse)
        {
            var bl = ((IEnumerable<bool>)pineBool).Reverse();
            var tr = pineTrue;
            var fl = ((IEnumerable<double?>)pineFalse).Reverse();

            var bf = bl.Zip(fl, (b, f) => (b, f));

            return bf.Reverse().Select(x => x.b ? tr : x.f).ToPineNA();
        }

        public static PineNA IIF(PineBool pineBool, PineNA pineTrue, double? pineFalse)
        {
            var bl = ((IEnumerable<bool>)pineBool).Reverse();
            var tr = ((IEnumerable<double?>)pineTrue).Reverse();
            var fl = pineFalse;

            var bt = bl.Zip(tr, (b, t) => (b, t));

            return bt.Reverse().Select(x => x.b ? x.t : fl).ToPineNA();
        }
        public static PineNA IIF(PineBool pineBool, Pine pineTrue, PineNA pineFalse)
        {
            var bl = ((IEnumerable<bool>)pineBool).Reverse();
            var tr = ((IEnumerable<double>)pineTrue).Reverse();
            var fl = ((IEnumerable<double?>)pineFalse).Reverse();

            var btf = bl.Zip(tr, (b, t) => (b, t)).Zip(fl, (bt, f) => (bt.b, bt.t, f));

            return btf.Reverse().Select(x => x.b ? x.t : x.f).ToPineNA();
        }
        public static PineNA IIF(PineBool pineBool, PineNA pineTrue, Pine pineFalse)
        {
            var bl = ((IEnumerable<bool>)pineBool).Reverse();
            var tr = ((IEnumerable<double?>)pineTrue).Reverse();
            var fl = ((IEnumerable<double>)pineFalse).Reverse();

            var btf = bl.Zip(tr, (b, t) => (b, t)).Zip(fl, (bt, f) => (bt.b, bt.t, f));

            return btf.Reverse().Select(x => x.b ? x.t : x.f).ToPineNA();
        }
        public static PineNA IIF(PineBool pineBool, double? pineTrue, Pine pineFalse)
        {
            var bl = ((IEnumerable<bool>)pineBool).Reverse();
            var tr = pineTrue;
            var fl = ((IEnumerable<double>)pineFalse).Reverse();

            var bf = bl.Zip(fl, (b, f) => (b, f));

            return bf.Reverse().Select(x => x.b ? tr : x.f).ToPineNA();
        }
        public static PineNA IIF(PineBool pineBool, Pine pineTrue, double? pineFalse)
        {
            var bl = ((IEnumerable<bool>)pineBool).Reverse();
            var tr = ((IEnumerable<double>)pineTrue).Reverse();
            var fl = pineFalse;

            var bt = bl.Zip(tr, (b, t) => (b, t));

            return bt.Reverse().Select(x => x.b ? x.t : fl).ToPineNA();
        }

        public static PineNA IIF(PineBool pineBool, double? pineTrue, double? pineFalse)
        {
            var bl = pineBool;
            var tr = pineTrue;
            var fl = pineFalse;

            return bl.Select(x => x ? tr : fl).ToPineNA();
        }

        #endregion

    }
}
