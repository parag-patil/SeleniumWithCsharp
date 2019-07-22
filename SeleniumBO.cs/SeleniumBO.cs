using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumLibrary.SeleniumBO
{
    public class SeleniumBO
    {
        IWebDriver _driver;
        string _url;
        public SeleniumBO(string url)
        {
            _url = url;
        }
       
        public DataTable ExecuteSelenium(string symbolValue,DateTime fromDateVal,DateTime toDateVal)
        {
            _driver = new ChromeDriver();
            _driver.Navigate().GoToUrl(_url);
            _driver.Manage().Window.Maximize();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            DataTable d;
            try
            {

                //Find and set Instrument Type Dropdown
                IWebElement element = _driver.FindElement(By.Name("instrumentType"));
                var selectElement = new SelectElement(element);
                selectElement.SelectByText("Stock Options");

                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

                //Find and set Instrument Type Dropdown
                IWebElement symbolElement = _driver.FindElement(By.Name("symbol"));
                var symbol = new SelectElement(symbolElement);
                symbol.SelectByValue(symbolValue.ToUpper());

                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                //Find and set Instrument Type Dropdown
                IWebElement optionTypeElement = _driver.FindElement(By.Name("optionType"));
                var optionType = new SelectElement(optionTypeElement);
                optionType.SelectByText("CE");


                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

                IWebElement dateRangeRbo = _driver.FindElement(By.Id("rdDateToDate"));
                dateRangeRbo.Click();

                IWebElement fromDate = _driver.FindElement(By.Id("fromDate"));
                fromDate.SendKeys(fromDateVal.ToString("dd-MMM-yyyy"));

                IWebElement toDate = _driver.FindElement(By.Id("toDate"));
                toDate.SendKeys(toDateVal.ToString("dd-MMM-yyyy"));

                ////Find and set Instrument Type Dropdown
                //IWebElement dateRangeElement = _driver.FindElement(By.Name("dateRange"));
                //var dateRange = new SelectElement(dateRangeElement);
                //dateRange.SelectByText("1 Day");

                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                IWebElement getButton = _driver.FindElement(By.Name("getButton"));
                getButton.Click();

                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);
                IWebElement downloadCSV = _driver.FindElement(By.Id("csvContentDiv"));
                string innerHtml = string.Empty;
                if (_driver is IJavaScriptExecutor js)
                {
                    innerHtml = (string)js.ExecuteScript("return arguments[0].innerHTML;", downloadCSV);
                }
                var csv = innerHtml.Split(':');
                d = createDataTable(csv);
                d.TableName = "symbolData";

                //System.IO.StringWriter writer = new System.IO.StringWriter();
                //d.WriteXml(writer, false);
                ////Close the browser
                //_driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
                return d;
            }
            catch (Exception ex) {
                return null;
            }
            finally
            {
                _driver.Close();
            }
           
        }
        private DataTable createDataTable(string[] csvArray)
        {
            DataTable dtCSV = new DataTable();
            int idx = 0;
            foreach (var str in csvArray)
            {
                var valueArray = str.Split(',');
                if (idx == 0)
                {
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        dtCSV.Columns.Add(valueArray[i].Replace("\"", "").Replace(" ","_"), typeof(String));
                    }
                    idx++;

                }
                else
                {
                    DataRow dr;
                    dr = dtCSV.NewRow();
                    for (int i = 0; i < valueArray.Length; i++)
                    {
                        dr[i] = valueArray[i].Replace("\"", "");
                    }
                    dtCSV.Rows.Add(dr);

                }
            }
            return dtCSV;

        }
    }
}
