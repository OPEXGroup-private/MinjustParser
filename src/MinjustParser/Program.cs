// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ITCC.Logging.Core;

namespace MinjustParser
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }

        private static void InitLoggers()
        {
            Logger.Level = LogLevel.Trace;
        }
    }
}
