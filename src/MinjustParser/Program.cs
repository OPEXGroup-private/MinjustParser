using System;
using System.Diagnostics;
using ITCC.Logging.Core;
using ITCC.Logging.Windows.Loggers;
using MinjustParser.Browser;

namespace MinjustParser
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Logger.Level = LogLevel.Debug;
                Logger.RegisterReceiver(new ColouredConsoleLogger());

                var selenium = Process.Start("chromedriver.exe", "--port=5555");

                var onGoogleSearchPage = new Browser.MinjustParser();
                onGoogleSearchPage.Parse();
                selenium?.Kill();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(1);
            }
        }
    }
}
