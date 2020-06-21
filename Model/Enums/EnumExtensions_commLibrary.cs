using System;
using System.ComponentModel;
using System.Reflection;

namespace CommLibrary.Enums
{
    public static class EnumExtensions
    {
        public static string Print(this BinSizeEnum binSize)
        {
            switch (binSize)
            {
                case BinSizeEnum.Minute: return "1m";
                case BinSizeEnum.FiveMinutes: return "5m";
                case BinSizeEnum.Hour: return "1h";
                case BinSizeEnum.Day: return "1d";
            }
            return "Error: Invalid Value";
        }
    }
}
