using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eevgen.Utilities
{
    static class RandomUtilities
    {
        public static uint GetRandomUint()
        {
            Random random = new Random();
            byte[] bytes = new byte[4];
            random.NextBytes(bytes);
            return BitConverter.ToUInt32(bytes);
        }
    }
}
