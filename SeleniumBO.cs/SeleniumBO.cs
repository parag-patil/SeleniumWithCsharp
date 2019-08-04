using NormalDistributionNamespace;
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

        public DataTable ExecuteSelenium(string symbolValue, DateTime fromDateVal, DateTime toDateVal,string option_Type,string instrumentType)
        {
            _driver = new ChromeDriver();
            _driver.Navigate().GoToUrl(_url);
            _driver.Manage().Window.Maximize();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
            DataTable d;
            try
            {

                //Find and set Instrument Type Dropdown
                IWebElement element = _driver.FindElement(By.Name("instrumentType"));
                var selectElement = new SelectElement(element);
                selectElement.SelectByText(instrumentType);

                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

                //Find and set Instrument Type Dropdown
                IWebElement symbolElement = _driver.FindElement(By.Name("symbol"));
                var symbol = new SelectElement(symbolElement);
                symbol.SelectByValue(symbolValue.ToUpper());

                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(15);
                //Find and set Instrument Type Dropdown
                IWebElement optionTypeElement = _driver.FindElement(By.Name("optionType"));
                var optionType = new SelectElement(optionTypeElement);
                optionType.SelectByText(option_Type);


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
            catch (Exception ex)
            {
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
                        if (valueArray[i].Replace("\"", "").Replace(" ", "_") == "Date" || valueArray[i].Replace("\"", "").Replace(" ", "_") == "Expiry")
                        {
                            dtCSV.Columns.Add(valueArray[i].Replace("\"", "").Replace(" ", "_"), typeof(DateTime));
                        }
                        else
                        {
                            dtCSV.Columns.Add(valueArray[i].Replace("\"", "").Replace(" ", "_"), typeof(String));
                        }
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

            DataColumn riskFreeRateColumn = new DataColumn("Risk_Free_Rate", typeof(double));
            riskFreeRateColumn.DefaultValue = 0.1;
            dtCSV.Columns.Add(riskFreeRateColumn);

            DataColumn guessVolColumn = new DataColumn("Guess_Volatility", typeof(double));
            guessVolColumn.DefaultValue = 0.3;
            dtCSV.Columns.Add(guessVolColumn);

            DataColumn divYieldColumn = new DataColumn("Dividend_Yield", typeof(double));
            divYieldColumn.DefaultValue = 0;
            dtCSV.Columns.Add(divYieldColumn);

            DataColumn TTEColumn = new DataColumn("TTE", typeof(int));
            dtCSV.Columns.Add(TTEColumn);

            DataColumn TimeToMaturityYrsColumns = new DataColumn("Time_To_Maturity_Years", typeof(decimal));
            dtCSV.Columns.Add(TimeToMaturityYrsColumns);

            DataColumn IVColumn = new DataColumn("IV", typeof(double));
            dtCSV.Columns.Add(IVColumn);

            DataColumn DeltaColumn = new DataColumn("Delta", typeof(double));
            dtCSV.Columns.Add(DeltaColumn);

            DataColumn ThetaColumn = new DataColumn("Theta", typeof(double));
            dtCSV.Columns.Add(ThetaColumn);

            DataColumn GammaColumn = new DataColumn("Gamma", typeof(double));
            dtCSV.Columns.Add(GammaColumn);

            DataColumn VegaColumn = new DataColumn("Vega", typeof(double));
            dtCSV.Columns.Add(VegaColumn);

            foreach (DataRow row in dtCSV.Rows)
            {
                if (row["Expiry"] != DBNull.Value)
                {
                    row["TTE"] = ((int)(Convert.ToDateTime(row["Expiry"]) - Convert.ToDateTime(row["Date"])).TotalDays);
                    row["Time_To_Maturity_Years"] = Convert.ToDouble(row["TTE"]) / 365;
                    row["IV"] = ImpliedVolatility(row["Option_Type"]
                        , (row["Underlying_Value"].ToString() == "-" ? 0 : row["Underlying_Value"])
                        , row["Strike_Price"], row["Risk_Free_Rate"],
                        row["Time_To_Maturity_Years"], row["Dividend_Yield"], row["Close"], row["Guess_Volatility"]);
                    if ((Convert.ToDateTime(row["Date"]) == Convert.ToDateTime(row["Expiry"])) || (row["Underlying_Value"].ToString()=="-"))
                    {
                        row["Delta"] = 0;
                        row["Theta"] = 0;
                        row["Gamma"] = 0;
                        row["Vega"] = 0;
                    }
                    else
                    {
                        row["Delta"] = OptionDelta(row["Option_Type"].ToString()
                            , (row["Underlying_Value"].ToString() == "-" ? 0 : Convert.ToDouble(row["Underlying_Value"]))
                            , Convert.ToDouble(row["Strike_Price"]), Convert.ToDouble(row["Risk_Free_Rate"])
                            , Convert.ToDouble(row["Time_To_Maturity_Years"]), Convert.ToDouble(row["IV"]) / 100
                            , Convert.ToDouble(row["Dividend_Yield"]));
                        row["Theta"] = OptionTheta(row["Option_Type"].ToString()
                            , (row["Underlying_Value"].ToString() == "-" ? 0 : Convert.ToDouble(row["Underlying_Value"]))
                            , Convert.ToDouble(row["Strike_Price"]), Convert.ToDouble(row["Risk_Free_Rate"]),
                           Convert.ToDouble(row["Time_To_Maturity_Years"]), Convert.ToDouble(row["IV"]) / 100
                           , Convert.ToDouble(row["Dividend_Yield"]));
                        row["Gamma"] = OptionGamma((row["Underlying_Value"].ToString() == "-" ? 0 : Convert.ToDouble(row["Underlying_Value"]))
                            , Convert.ToDouble(row["Strike_Price"]), Convert.ToDouble(row["Risk_Free_Rate"])
                            , Convert.ToDouble(row["Time_To_Maturity_Years"]), Convert.ToDouble(row["IV"]) / 100
                            , Convert.ToDouble(row["Dividend_Yield"]));
                        row["Vega"] = OptionVega((row["Underlying_Value"].ToString() == "-" ? 0 : Convert.ToDouble(row["Underlying_Value"]))
                            , Convert.ToDouble(row["Strike_Price"]), Convert.ToDouble(row["Risk_Free_Rate"])
                            , Convert.ToDouble(row["Time_To_Maturity_Years"]), Convert.ToDouble(row["IV"]) / 100
                            , Convert.ToDouble(row["Dividend_Yield"]));
                    }
                }
            }



            return dtCSV;

        }

        public double EuropeanOption(string CallOrPut, double S, double K, double v, double r, double T, double q)
        {
            double retValue;
            double d1;
            double d2;
            double nd1;
            double nd2;
            double nnd1;
            double nnd2;

            d1 = (Math.Log(S / K) + (r - q + 0.5 * Math.Pow(v, 2)) * T) / (double)(v * Math.Sqrt(T));
            d2 = (Math.Log(S / (double)K) + (r - q - 0.5 * Math.Pow(v, 2)) * T) / (double)(v * Math.Sqrt(T));
            nd1 = NormsDistribution.N(d1);
            nd2 = NormsDistribution.N(d2);
            nnd1 = NormsDistribution.N(-d1);
            nnd2 = NormsDistribution.N(-d2);

            if (CallOrPut == "CE")
                retValue = S * Math.Exp(-q * T) * nd1 - K * Math.Exp(-r * T) * nd2;
            else
                retValue = -S * Math.Exp(-q * T) * nnd1 + K * Math.Exp(-r * T) * nnd2;

            return retValue;
        }

        public double ImpliedVolatility(object CallOrPut, object S, object K, object r, object T, object q, object OptionValue, object guess)
        {
            double retValue;

            double epsilon;
            double dVol;
            double vol_1;
            int i;
            int maxIter;
            double Value_1;
            double vol_2;
            double Value_2;
            double dx;

            dVol = 0.00001;
            epsilon = 0.00001;
            maxIter = 100;
            vol_1 = Convert.ToDouble(guess);
            i = 1;
            do
            {
                Value_1 = EuropeanOption(CallOrPut.ToString(), Convert.ToDouble(S), Convert.ToDouble(K), vol_1, Convert.ToDouble(r), Convert.ToDouble(T), Convert.ToDouble(q));
                vol_2 = vol_1 - dVol;
                Value_2 = EuropeanOption(CallOrPut.ToString(), Convert.ToDouble(S), Convert.ToDouble(K), vol_2, Convert.ToDouble(r), Convert.ToDouble(T), Convert.ToDouble(q));
                dx = (Value_2 - Value_1) / dVol;
                if (Math.Abs(dx) < epsilon | i == maxIter)
                    break;
                vol_1 = vol_1 - (Convert.ToDouble(OptionValue) - Value_1) / dx;
                i = i + 1;
            }
            while (true);
            retValue = vol_1;

            return Math.Round(retValue * 100, 2);
        }

        #region Greeks
        public double dOne(double S, double X, double T, double r, double v, double d)
        {
            return (Math.Log(S / X) + (r - d + 0.5 * Math.Pow(v, 2)) * T) / (v * (Math.Sqrt(T)));
        }

        public double NdOne(double S, double X, double T, double r, double v, double d)
        {
            return Math.Exp(-(Math.Pow(dOne(S, X, T, r, v, d), 2)) / 2) / (Math.Sqrt(2 * Math.PI));
        }

        public double dTwo(double S, double X, double T, double r, double v, double d)
        {
            return dOne(S, X, T, r, v, d) - (v) * Math.Sqrt(T);
        }

        public double NdTwo(double S, double X, double T, double r, double v, double d)
        {
            return NormsDistribution.N(dTwo(S, X, T, r, v, d));
        }

        //public void OptionPrice(object OptionType, object S, object X, object T, object r, object v, object d)
        //{
        //    if (OptionType == "Call")
        //        OptionPrice = Exp(-d * T) * S * Application.NormSDist(dOne(S, X, T, r, v, d)) - X * Exp(-r * T) * Application.NormSDist(dOne(S, X, T, r, v, d) - v * Sqr(T));
        //    else if (OptionType == "Put")
        //        OptionPrice = X * Exp(-r * T) * Application.NormSDist(-dTwo(S, X, T, r, v, d)) - Exp(-d * T) * S * Application.NormSDist(-dOne(S, X, T, r, v, d));
        //}

        public double OptionDelta(string OptionType, double S, double X, double r, double T, double v, double d)
        {
            double retValue = 0;
            if (OptionType.ToString() == "CE")
                retValue = NormsDistribution.N(dOne(S, X, T, r, v, d));
            else if (OptionType.ToString() == "PE")
                retValue = NormsDistribution.N(dOne(S, X, T, r, v, d)) - 1;

            return Math.Round(retValue, 2);
        }

        public double OptionTheta(string OptionType, double S, double X, double r, double T, double v, double d)
        {
            double retValue = 0;
            if (OptionType.ToString() == "CE")
                retValue = (-(S * v * NdOne(S, X, T, r, v, d)) / (double)(2 * Math.Sqrt(T)) - r * X * Math.Exp(-r * (T)) * NdTwo(S, X, T, r, v, d)) / 365;
            else if (OptionType.ToString() == "PE")
                retValue = (-(S * v * NdOne(S, X, T, r, v, d)) / (double)(2 * Math.Sqrt(T)) + r * X * Math.Exp(-r * (T)) * (1 - NdTwo(S, X, T, r, v, d))) / 365;

            return Math.Round(retValue, 2);
        }

        public double OptionGamma(double S, double X, double r, double T, double v, double d)
        {
            return Math.Round(NdOne(S, X, T, r, v, d) / (S * (v * Math.Sqrt(T))), 4);
        }

        public double OptionVega(double S, double X, double r, double T, double v, double d)
        {
            return Math.Round(0.01 * S * Math.Sqrt(T) * NdOne(S, X, T, r, v, d), 4);
        }

        //public void OptionRho(object OptionType, object S, object X, object T, object r, object v, object d)
        //{
        //    if (OptionType == "Call")
        //        OptionRho = 0.01 * X * T * Exp(-r * T) * Application.NormSDist(dTwo(S, X, T, r, v, d));
        //    else if (OptionType == "Put")
        //        OptionRho = -0.01 * X * T * Exp(-r * T) * (1 - Application.NormSDist(dTwo(S, X, T, r, v, d)));
        //}
        #endregion Greeks
    }
}
