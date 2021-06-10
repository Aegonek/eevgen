using MoreLinq;
using System;
using System.ComponentModel;
using System.Linq;

namespace eevgen.Utilities
{
    public static class EnumExtensions
    {
        public static TAttribute? GetAttribute<TAttribute>(this Enum value)
            where TAttribute : Attribute
        {
            Type? type = value.GetType();
            string name = Enum.GetName(type, value)!;
            return type?.GetField(name)?
                .GetCustomAttributes(false)?
                .OfType<TAttribute>()?
                .SingleOrDefault();
        }

        public static string[] GetValuesAsStrings<T>()
            where T : struct, Enum
        {
            return Enum.GetValues<T>().Select(x => x.ToString()).ToArray();
        }

        public static string[] GetDescriptions<T>()
            where T : struct, Enum
        {
            return Enum.GetValues<T>()?
                .Choose<T, string?>(x => (x.GetAttribute<DescriptionAttribute>()?.Description) switch
                {
                    var desc when desc is not null => (true, desc),
                    _ => (false, null)
                })
                .ToArray()!;
        }
    }
}
