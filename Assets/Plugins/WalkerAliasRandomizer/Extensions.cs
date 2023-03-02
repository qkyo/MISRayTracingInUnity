using System;
using System.Collections.Generic;
using System.Linq;

namespace WalkerAliasRandomizer
{
    public static class Extensions
    {
        public static T Pop<T>(this IList<T> source)
        {
            var item = source.Last();
            source.Remove(item);

            return item;
        }
    }
}