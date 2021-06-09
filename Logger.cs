using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eevgen
{
    class Logger: IDisposable
    {
        StreamWriter logFile;

        /// <param name="filePath">If no file path is given, log will be dumped to desktop with name based on time, when tool started working.</param>
        public Logger(string? filePath = null)
        {
            if (filePath is null)
                filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    DateTime.Now.ToString("hhMMss") + "_log.txt");
            logFile = new StreamWriter(filePath);
        }

        public void Info(string message)
        {
            logFile.WriteLine($"{DateTime.Now:hh:mm:ss} INFO: {message}");
            Console.WriteLine($"{DateTime.Now:hh:mm:ss} INFO: {message}");
        }
        
        public void Warning(string message)
        {
            logFile.WriteLine($"{DateTime.Now:hh:mm:ss} WARNING: {message}");
            Console.WriteLine($"{DateTime.Now:hh:mm:ss} WARNING: {message}");
        }

        public void Error(string message)
        {
            logFile.WriteLine($"{DateTime.Now:hh:mm:ss} ERROR: {message}");
            Console.WriteLine($"{DateTime.Now:hh:mm:ss} ERROR: {message}");
        }

        public void Dispose()
        {
            logFile.Dispose();
        }
    }
}
