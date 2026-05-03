using System;
using System.IO;

namespace MoznyZaklad.Server
{
    public static class Logger
    {
        private static readonly string LogSoubor = "server_log.txt";
        private static readonly object Zamek = new object();

        public static void Log(string zprava)
        {
            string radek = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {zprava}";

            Console.WriteLine(radek);

            lock (Zamek)
            {
                try
                {
                    File.AppendAllLines(LogSoubor, new[] { radek });
                }
                catch
                {

                }
            }
        }
    }
}
