using System;
using System.Collections.Generic;
using System.Linq;

namespace Lunch.TelegramBot.Common.Extensions
{
    public static class EnumerableExtensions
    {
        private static readonly Random Rnd = new Random();

        public static T GetRandom<T>(this IEnumerable<T> array)
        {
            var temp = array as T[] ?? array.ToArray();
            return temp[Rnd.Next(minValue: 0, maxValue: temp.Length)];
        }

        public static string Aggregate<T>(this IEnumerable<T> array, string delimiter = ", ")
        {
            var strings = array.Select(_ => _.ToString()).ToArray();
            return !strings.Any() ? string.Empty : strings.Aggregate((s, s1) => $"{s}{delimiter}{s1}");
        }
    }
}