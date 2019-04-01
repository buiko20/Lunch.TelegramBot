using System;
using System.Collections.Generic;
using System.Linq;

namespace Lunch.TelegramBot.Common.Extensions
{
    public static class ArrayExtension
    {
        private static readonly Random Rnd = new Random();

        public static T GetRandom<T>(this T[] array) =>
            array[Rnd.Next(minValue: 0, maxValue: array.Length)];

        public static string Aggregate(this IEnumerable<DayOfWeek> array, string delimiter = ", ")
        {
            var strings = array.Select(_ => _.ToString()).ToArray();
            return strings.Aggregate(delimiter);
        }

        public static string Aggregate(this IEnumerable<string> array, string delimiter = ", ")
        {
            var enumerable = array as string[] ?? array.ToArray();
            return !enumerable.Any() ? string.Empty : enumerable.Aggregate((s, s1) => $"{s}{delimiter}{s1}");
        }
    }
}
