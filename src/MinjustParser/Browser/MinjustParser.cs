using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace MinjustParser.Browser
{
    public class MinjustParser : IDisposable
    {
        private readonly RemoteWebDriver _driver;
        private const string BaseAddress = @"http://unro.minjust.ru/NKOs.aspx";

        public MinjustParser()
        {
            var capabilities = DesiredCapabilities.Chrome();
            _driver = new RemoteWebDriver(new Uri("http://localhost:5555"), capabilities);
        }

        public void Dispose() => _driver.Dispose();

        public void Parse()
        {
            PrepareFiles();

            _driver.Navigate().GoToUrl(BaseAddress);

            var upperTable = _driver.FindElement(By.Id("pdg"));
            var pgdCounts = upperTable.FindElements(By.ClassName("pdg_count"));
            var targetLink = pgdCounts.First(e => e.Text == "500");
            targetLink.Click();

            var currentPage = 1;
            while (true)
            {
                WriteWithTime($"Processing page {currentPage}");
                
                var rows = _driver.FindElements(By.TagName("tr"));
                var goodRows = rows
                    .Where(e => e.GetCssValue("cursor") == "auto" && e.GetAttribute("odd") != null)
                    .ToList();
                WriteWithTime($"Found {goodRows.Count} rows");

                var currentRow = 0;
                using (var fileStream = new FileStream("output.csv", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    foreach (var row in goodRows)
                    {
                        currentRow++;
                        WriteWithTime($"\tProcessing row {currentRow}");
                        WriteRow(streamWriter,row);
                    }
                    streamWriter.Flush();
                }

                currentPage++;
                WriteWithTime($"Going to page {currentPage}");
                _driver.ExecuteScript("__doPostBack('pdg','next')");
            }

            _driver.Quit();
        }

        private static void PrepareFiles()
        {
            if (File.Exists("output_last.csv"))
                File.Delete("output_last.csv");

            if (File.Exists("output.csv"))
                File.Move("output.csv", "output_last.csv");

            File.Create("output.csv");
        }

        private IWebElement GetNextPageButton(int currentPage)
        {
            var text = $"{currentPage + 1}";
            Console.WriteLine($"Looking for {text}");
            try
            {
                Thread.Sleep(2000);
                var pageButtons = _driver.FindElements(By.Id("pdg_next")).ToList();
                foreach (var pageButton in pageButtons)
                {
                    WriteWithTime($"Pagebuttons: {pageButton.Text}");
                }
                return pageButtons.FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private static void WriteRow(TextWriter streamWriter, IWebElement row)
        {
            var dataElements = new List<IWebElement>();
            var odd = row.GetAttribute("odd") == @"_odd";
            if (odd)
            {
                dataElements.AddRange(row.FindElements(By.ClassName("pdg_item_left_odd")));
                dataElements.AddRange(row.FindElements(By.ClassName("pdg_item_odd")));
                dataElements.AddRange(row.FindElements(By.ClassName("pdg_item_right_odd")));
            }
            else
            {
                dataElements.AddRange(row.FindElements(By.ClassName("pdg_item_left_even")));
                dataElements.AddRange(row.FindElements(By.ClassName("pdg_item_even")));
                dataElements.AddRange(row.FindElements(By.ClassName("pdg_item_right_even")));
            }
            
            var text = string.Join("\t", dataElements.Select(de => de.Text));
            text = text.Replace("-\n", string.Empty);
            if (string.IsNullOrWhiteSpace(text))
                return;

            streamWriter.WriteLine(text);
            
        }

        private void WriteWithTime(string message) => Console.WriteLine($"{DateTime.Now:T} {message}");
    }
}
