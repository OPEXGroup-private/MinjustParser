using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace MinjustParser.Browser
{
    public class OnGoogleSearchPage : IDisposable
    {
        private readonly IWebDriver _driver;
        private const string BaseAddress = @"http://unro.minjust.ru/NKOs.aspx";

        public OnGoogleSearchPage()
        {
            var capabilities = DesiredCapabilities.Chrome();
            _driver = new RemoteWebDriver(new Uri("http://localhost:5555"), capabilities);
        }

        public void Dispose() => _driver.Dispose();

        public void Should_find_search_box()
        {
            if (!File.Exists("output.csv"))
                File.Create("output.csv");

            _driver.Navigate().GoToUrl(BaseAddress);

            var upperTable = _driver.FindElement(By.Id("pdg"));
            var pgdCounts = upperTable.FindElements(By.ClassName("pdg_count"));
            var targetLink = pgdCounts.First(e => e.Text == "500");
            targetLink.Click();

            var currentPage = 1;
            while (true)
            {
                Thread.Sleep(5000);
                
                var rows = _driver.FindElements(By.TagName("tr"));
                var goodRows = rows.Where(e => e.GetCssValue("cursor") == "auto").ToList();
                Console.WriteLine($"Found {goodRows.Count} rows");

                using (var fileStream = new FileStream("output.csv", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8))
                {
                    foreach (var row in goodRows)
                    {
                        WriteRow(streamWriter,row);
                    }
                }

                var page = GetNextPageButton(currentPage);
                if (page == null)
                {
                    Console.WriteLine("Done");
                    break;
                }
                page.Click();
                currentPage++;
            }

            _driver.Quit();
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
                    Console.WriteLine($"Pagebuttons: {pageButton.Text}");
                }
                return pageButtons.FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private static void WriteRow(StreamWriter streamWriter, IWebElement row)
        {
            var dataElements = row.FindElements(By.ClassName("pdg_item_odd"));
            var text = string.Join("\t", dataElements.Select(de => de.Text));
            if (string.IsNullOrWhiteSpace(text))
                return;

            streamWriter.WriteLine(text);
            streamWriter.Flush();
        }
    }
}
