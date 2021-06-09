using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;

namespace eevgen
{
    class Program
    {
        static void Main(string[] args)
        {
            using Logger logger = new Logger();
            try
            {
                Arguments parsed = Arguments.Parse(args);
                ISkyrimMod mod = SkyrimMod.CreateFromBinary(parsed.ModPath, SkyrimRelease.SkyrimSE);
                IWorker generator = new EnchantedVariantsGenerator();
                generator.Work(mod, logger, parsed);
            }
            catch (Exception ex)
            {
                logger.Error("Unexpected error! " + ex.Message);
            }
        }
    }
}