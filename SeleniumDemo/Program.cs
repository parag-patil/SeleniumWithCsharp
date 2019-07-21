using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.nseindia.com/products/content/derivatives/equities/historical_fo.htm");
            driver.Manage().Window.Maximize();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            //Find and set Instrument Type Dropdown
            IWebElement element = driver.FindElement(By.Name("instrumentType"));
            var selectElement = new SelectElement(element);
            selectElement.SelectByText("Stock Options");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            //Find and set Instrument Type Dropdown
            IWebElement symbolElement = driver.FindElement(By.Name("symbol"));
            var symbol = new SelectElement(symbolElement);
            symbol.SelectByValue("ACC");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            //Find and set Instrument Type Dropdown
            IWebElement optionTypeElement = driver.FindElement(By.Name("optionType"));
            var optionType = new SelectElement(optionTypeElement);
            optionType.SelectByText("CE");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            //Find and set Instrument Type Dropdown
            IWebElement dateRangeElement = driver.FindElement(By.Name("dateRange"));
            var dateRange = new SelectElement(dateRangeElement);
            dateRange.SelectByText("1 Day");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            IWebElement getButton = driver.FindElement(By.Name("getButton"));
            getButton.Click();

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
            IWebElement downloadCSV = driver.FindElement(By.Id("csvContentDiv"));
            string innerHtml = string.Empty;
            if (driver is IJavaScriptExecutor js)
            {
                innerHtml = (string)js.ExecuteScript("return arguments[0].innerHTML;", downloadCSV);
            }
            var csv = innerHtml.Split(':');
            DataTable d = createDataTable(csv);
            //Close the browser
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(5);


        }

        private static DataTable createDataTable(string[] csvArray)
        {
            DataTable dtCSV = new DataTable();
            int idx = 0;
            foreach (var str in csvArray)
            {
                var valueArray = str.Split(',');
                if (idx == 0)
                {
                    for(int i=0;i<valueArray.Length;i++)
                    {
                        dtCSV.Columns.Add(valueArray[i], typeof(String));
                    }
                    idx++;
                    
                } else
                {
                    dtCSV.Rows.Add(str);
                }
            }
            return dtCSV;

        }
    }
}
