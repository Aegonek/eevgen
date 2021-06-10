using Mutagen.Bethesda.Skyrim;
using System;
using System.Collections.Generic;
using System.IO;

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
                
                IWorker generator = new GenerateEnchantedWeaponVariants(mod, logger, parsed);
                generator.Work();
            }
            catch (Exception ex)
            {
                logger.Error("Unexpected error! " + ex.Message);
            }
        }

        static void Backup(string path)
        {
            File.Copy(path, path.Replace(".esp", "") + $"_backup_{DateTime.Now.ToShortTimeString()}.xml");
        }
    }
}