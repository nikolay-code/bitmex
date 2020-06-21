using BitMexLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMexLibrary
{
    public static class PineExtensions
    {
        public static PineBool ToPineBool(this IEnumerable<bool> source)
        {
            PineBool ret = new PineBool();
            foreach (bool num in source)
                ret.Add(num);
            return ret;
        }

        public static PineBool ToPineBool(this IEnumerable<double?> source)
        {
            PineBool ret = new PineBool();
            foreach (double? num in source)
                ret.Add(num != null);
            return ret;
        }

        public static Pine ToPine(this IEnumerable<double> source)
        {
            Pine ret = new Pine();
            foreach (double num in source)
                ret.Add(num);
            return ret;
        }
        public static Pine ToPine(this IEnumerable<decimal> source) => source.Select(x => Convert.ToDouble(x)).ToPine();
        public static Pine ToPine(this IEnumerable<decimal?> source) => source.Select(x => x == null ? null : (double?)Convert.ToDouble(x)).ToPine();

        public static Pine ToPine_v1(this IEnumerable<decimal?> source)
        { 
            Pine p = new Pine();
            List<double?> boo = new List<double?>();
            foreach (var item in source) boo.Add( decimal.ToDouble((decimal)item));

            return p;
        }

        public static Pine ToPine(this IEnumerable<double?> source, Approximation approximation = Approximation.LinearNew)
        {
            switch (approximation)
            {
                case Approximation.Linear: return LinearApproximation();
                case Approximation.LinearNew: return LinearNewApproximation();
                case Approximation.Step: return StepApproximation();
                default: return null;
            }

            Pine LinearNewApproximation()
            {
                int countNum = source.Count(x => x != null); // количество имеющих значение в последовательности
                if (countNum == 0)
                    return Enumerable.Range(0, source.Count()).Select(x => 0.0).ToPine();
                if (countNum == 1)
                {
                    double _value = source.First(x => x != null).Value;
                    return Enumerable.Range(0, source.Count()).Select(x => _value).ToPine();
                }

                Pine ret = new Pine(); // возвращаемый массив

                if (countNum != source.Count())
                {
                    double value1 = 0, value2 = 0, delta = 0; // последние значащие и приращение на каждый шаг
                    IEnumerable<double?> sour = source.SkipWhile(x => x == null);  // текущее состояние последовательности = source.SkipWhile(x => x == null); // пропуск первых не значащих
                    int numberCurr; // номер обрабатываемого значения
                    int countNull; // количество незначащих значений в следующем промежутке

                    for
                    (
                        numberCurr = 1; // номер обрабатываемого значения
                        numberCurr < countNum;
                        /*sour = sour.SkipWhile(x => x == null),*/ numberCurr++
                    )
                    {
                        value1 = sour.First().Value; // первое значащае число
                        ret.Add(value1);
                        sour = sour.Skip(1);

                        if (sour.First() == null)
                        {
                            value2 = sour.First(x => x != null).Value; // второе значащае число
                            countNull = sour.TakeWhile(x => x == null).Count(); // количество не имеющих значения между первым и вторым числом
                            delta = (value2 - value1) / (countNull + 1); // приращение на каждый шаг

                            for (int ind = 0; ind < countNull; ind++)
                            {
                                ret.Add(value1 += delta); // заполнение последовательности

                            }

                            numberCurr += countNull;
                            sour = sour.SkipWhile(x => x == null);

                        }
                    }

                    // обработка конца последовательности
                    ret.Add(value2);
                    sour = sour.Skip(1);
                    countNull = sour.Count(); // количество последних не имеющих значения
                    for (int ind = 0; ind < countNull; ind++)
                        ret.Add(value2 += delta); // заполнение последовательности
                }
                else
                {
                    foreach (var item in source)
                    {
                        ret.Add(Convert.ToDouble(item));
                    }
                }

                return ret;
            }

            Pine LinearApproximation()
            {
                int countNum = source.Count(x => x != null); // количество имеющих значение в последовательности
                if (countNum == 0)
                    return Enumerable.Range(0, source.Count()).Select(x => 0.0).ToPine();
                if (countNum == 1)
                {
                    double _value = source.First(x => x != null).Value;
                    return Enumerable.Range(0, source.Count()).Select(x => _value).ToPine();
                }

                Pine ret = new Pine(); // возвращаемый массив
                double value1 = 0, value2 = 0, delta = 0; // последние значащие и приращение на каждый шаг
                IEnumerable<double?> sour = source.SkipWhile(x => x == null);  // текущее состояние последовательности = source.SkipWhile(x => x == null); // пропуск первых не значащих
                int numberCurr; // номер обрабатываемого значения
                int countNull; // количество незначащих значений в следующем промежутке

                for
                (
                    numberCurr = 1; // номер обрабатываемого значения
                    numberCurr < countNum;
                    sour = sour.SkipWhile(x => x == null), numberCurr++
                )
                {
                    value1 = sour.First().Value; // первое значащае число
                    ret.Add(value1);
                    sour = sour.Skip(1);
                    value2 = sour.First(x => x != null).Value; // второе значащае число
                    countNull = sour.TakeWhile(x => x == null).Count(); // количество не имеющих значения между первым и вторым числом
                    delta = (value2 - value1) / (countNull + 1); // приращение на каждый шаг
                    for (int ind = 0; ind < countNull; ind++)
                        ret.Add(value1 += delta); // заполнение последовательности
                }

                // обработка конца последовательности
                ret.Add(value2);
                sour = sour.Skip(1);
                countNull = sour.Count(); // количество последних не имеющих значения
                for (int ind = 0; ind < countNull; ind++)
                    ret.Add(value2 += delta); // заполнение последовательности

                return ret;
            }

            Pine StepApproximation()
            {
                int countNum = source.Count(x => x != null); // количество имеющих значение в последовательности
                if (countNum == 0)
                    return Enumerable.Range(0, source.Count()).Select(x => 0.0).ToPine();
                if (countNum == 1)
                {
                    double _value = source.First(x => x != null).Value;
                    return Enumerable.Range(0, source.Count()).Select(x => _value).ToPine();
                }

                Pine ret = new Pine(); // возвращаемый массив
                double value1 = 0, value2 = 0; // последние значащие и приращение на каждый шаг
                IEnumerable<double?> sour = source.SkipWhile(x => x == null);  // текущее состояние последовательности = source.SkipWhile(x => x == null); // пропуск первых не значащих
                int numberCurr; // номер обрабатываемого значения
                int countNull; // количество незначащих значений в следующем промежутке

                for
                (
                    numberCurr = 1; // номер обрабатываемого значения
                    numberCurr < countNum;
                    sour = sour.SkipWhile(x => x == null), numberCurr++
                )
                {
                    value1 = sour.First().Value; // первое значащае число
                    ret.Add(value1);
                    sour = sour.Skip(1);
                    value2 = sour.First(x => x != null).Value; // второе значащае число
                    countNull = sour.TakeWhile(x => x == null).Count(); // количество не имеющих значения между первым и вторым числом
                    for (int ind = 0; ind < countNull; ind++)
                        ret.Add(value1); // заполнение последовательности
                }

                // обработка конца последовательности
                ret.Add(value2);
                sour = sour.Skip(1);
                countNull = sour.Count(); // количество последних не имеющих значения
                for (int ind = 0; ind < countNull; ind++)
                    ret.Add(value2); // заполнение последовательности

                return ret;
            }

        }

        public static Pine Drop(this Pine source, uint count) => source.Take(source.Count() - (int)count).ToPine();
        public static PineNA Drop(this PineNA source, uint count) => source.Take(source.Count() - (int)count).ToPineNA();

        public static PineNA ToPineNA(this IEnumerable<double?> source)
        {
            PineNA ret = new PineNA();
            foreach (double? num in source)
                ret.Add(num);
            return ret;
        }
        public static PineNA ToPineNA(this IEnumerable<decimal?> source)
        {
            PineNA ret = new PineNA();
            foreach (decimal? num in source)
                ret.Add((double?)num);
            return ret;
        }
    }
}
