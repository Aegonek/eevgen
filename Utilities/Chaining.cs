using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eevgen
{
    static class Chaining
    {
        public static T Then<T, U>(this U obj, Func<U, T> continuation) => continuation(obj);
        public static T Also<T>(this T obj, Action<T> continuation)
        {
            continuation(obj);
            return obj;
        }
    }
}
