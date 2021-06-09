using System;
using System.IO;
using System.Linq;

namespace eevgen
{
    class Arguments
    {
        const string HowToFormatMessage = "Please use the following format: 'eevgen \"C:/Program Files/Steam/steammaps/common/Skyrim/Data\" \"Thaumaturgy - An Enchanting Overhaul.esp\"'";

        public string DataFolderPath { get; }
        public string ModPath { get; }
        public Arguments(string dataPath, string fileName) => (DataFolderPath, ModPath) = (dataPath, fileName);

        internal static Arguments Parse(string[] raw)
        {
            if (raw.Length != 2)
                throw new ArgumentException("Invalid arguments format! " + HowToFormatMessage);

            (string dataPath, string modPath) = (raw[0], raw[1]);

            if (!Directory.Exists(dataPath))
                throw new ArgumentException("Directory doesn't exist! " + HowToFormatMessage);
            if (!Directory.EnumerateFiles(dataPath).Any(x => x.EndsWith("Skyrim.esm")))
                throw new ArgumentException("Path doesn't lead to Skyrim directory! " + HowToFormatMessage);
            if (!File.Exists(modPath))
                throw new ArgumentException("Path to the mod doesn't lead to a file! " + HowToFormatMessage);

            return new Arguments(dataPath, modPath);
        }
    }
}
