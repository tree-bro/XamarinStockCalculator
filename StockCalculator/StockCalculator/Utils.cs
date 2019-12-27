using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace StockCalculator
{
    public class Utils
    {

        /// <summary>
        /// Delegate for handling any info parsing process
        /// </summary>
        /// <param name="url"></param>
        /// <param name="stockInfo"></param>
        private delegate void InfoParsingDel(string url, ref StockInfo stockInfo);

        public delegate void InvokeDelegate();
        public delegate void SetParameterDelegate(string input);

        public static HtmlAgilityPack.HtmlDocument loadHtmlDocument(string url, Encoding encoding)
        {
            HttpWebResponse response = (HttpWebResponse)WebRequest.CreateHttp(url).GetResponse();
            HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();

            using (StreamReader sr = new StreamReader(response.GetResponseStream(), encoding))
            {
                htmlDocument.Load(sr);
            }

            response.Close();

            return htmlDocument;
        }

        public static string loadJsonString(string url)
        {
            HttpWebResponse response = (HttpWebResponse)WebRequest.CreateHttp(url).GetResponse();
            string jsonString = "";
            using(StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                jsonString = sr.ReadToEnd();
            }
            return jsonString;
        }

        public static StockMarketTypes checkMarketType(string stockID)
        {
            Regex regexForChinaSZ = new Regex("^00[0-9]{4}$");
            Regex regexForChinaSH = new Regex("^60[0-9]{4}$");
            Regex regexForHK = new Regex("^0[0-9]{4}$");

            if (regexForChinaSZ.IsMatch(stockID))
            {
                return StockMarketTypes.CHINA_SZ_EXCHANGE_MARKET;
            }
            else if (regexForChinaSH.IsMatch(stockID))
            {
                return StockMarketTypes.CHINA_SH_EXCHANGE_MARKET;
            }
            else if (regexForHK.IsMatch(stockID))
            {
                return StockMarketTypes.HK_EXCHANGE_MARKET;
            }
            else
            {
                return StockMarketTypes.UNKNOWN;
            }
        }

        public static string[] readPreferStockIDList()
        {
            List<string> resultList = new List<string>();
            if (File.Exists("CalculationHistory.csv"))
            {
                foreach (string line in File.ReadAllLines("CalculationHistory.csv"))
                {
                    //resultList.Add(line.Split(',')[0]);
                    string[] splitLines = line.Split(',');
                    if(splitLines.Length > 2)
                    {
                        resultList.Add(line.Split(',')[0] + " - " + line.Split(',')[1]);
                    }
                }
            }
            return resultList.ToArray();
        }

        public static HtmlNode findNodeByText(HtmlNode parentNode, string xpath, string compareValue, int offset, bool exactMatch)
        {
            int count = 0;
            bool foundMatch = false;
            foreach (HtmlNode subNode in parentNode.SelectNodes(xpath))
            {
                if (exactMatch)
                {
                    if (subNode.InnerText.Equals(compareValue, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        foundMatch = true;
                    }
                }
                else
                {
                    if (subNode.InnerText.Contains(compareValue))
                    {
                        foundMatch = true;
                    }
                }
                
                if (foundMatch)
                {
                    if(count == offset)
                    {
                        return subNode;
                    }
                    else
                    {
                        count += 1;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Make the company basic info parsing process to be async
        /// </summary>
        /// <param name="url"></param>
        /// <param name="stockInfo"></param>
        public static void parseCompanyBasicInfo(string url, ref StockInfo stockInfo)
        {
            InfoParsingDel parseDel = new InfoParsingDel(parseCompanyBasicInfoCore);

            parseDel.Invoke(url, ref stockInfo);
        }

        /// <summary>
        /// This is the core process for parsing company basic info
        /// </summary>
        /// <param name="url"></param>
        /// <param name="stockInfo"></param>
        private static void parseCompanyBasicInfoCore(string url, ref StockInfo stockInfo)
        {
            //// The following implementation is based on Baidu Stock API.
            //// It might need to be changed if we decide to use other API instead.
            //HtmlAgilityPack.HtmlDocument StockInfoHtmlDocument = Utils.loadHtmlDocument(url, Encoding.GetEncoding("utf-8"));

            //HtmlNode stockInfoTableNode = StockInfoHtmlDocument.DocumentNode.SelectSingleNode("//div[@class='stock-bets']");

            //if (stockInfoTableNode != null)
            //{
            //    HtmlNode nameNode = stockInfoTableNode.SelectSingleNode("//a[@class='bets-name']");
            //    HtmlNode dateNode = stockInfoTableNode.SelectSingleNode("//span[@class='state f-up']");
            //    HtmlNode closePriceNode = stockInfoTableNode.SelectSingleNode("//strong[@class='_close']");
            //    HtmlNode detailNode = stockInfoTableNode.SelectSingleNode("//div[@class='bets-content']//div");

            //    stockInfo.CompanyName = nameNode.InnerText.Trim();
            //    stockInfo.LastTradingPrice = closePriceNode.InnerText.Trim();
            //    stockInfo.DateOfInfo = dateNode.InnerText.Trim().Replace("&nbsp;", "");
            //    decimal companyProfitPerShare = decimal.Zero;
            //    decimal peRatio = decimal.Zero;
            //    decimal lastTradingPrice = decimal.Zero;
            //    decimal.TryParse(closePriceNode.InnerText.Trim(), out lastTradingPrice);
            //    foreach (HtmlNode subNode in detailNode.ChildNodes)
            //    {
            //        if (subNode.HasChildNodes)
            //        {
            //            string firstChildText = subNode.FirstChild.InnerText.Trim();
            //            string lastChildText = subNode.LastChild.InnerText.Trim();
            //            if (firstChildText.Equals("每股收益", StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                decimal.TryParse(lastChildText, out companyProfitPerShare);
            //            }
            //            else if (firstChildText.Contains("市盈率"))
            //            {
            //                stockInfo.PERatio = lastChildText;
            //                decimal.TryParse(lastChildText, out peRatio);
            //            }
            //            // if failed to parse last trading price (most likely to happen during long holiday), then use previous closing price instead
            //            else if (lastTradingPrice == decimal.Zero &&
            //                firstChildText.Equals("昨收", StringComparison.CurrentCultureIgnoreCase))
            //            {
            //                decimal.TryParse(lastChildText, out lastTradingPrice);
            //                stockInfo.DateOfInfo += "(顺延前一收盘价)";
            //            }
            //        }
            //    }

            //    // Recalculate the profit per share by the past days.
            //    // Always use PERatio for bellow calculation if available.
            //    if (peRatio > 0)
            //    {
            //        //companyProfitPerShare = decimal.Round(lastTradingPrice / peRatio * 365M / DateTime.Today.DayOfYear, 4);
            //        companyProfitPerShare = decimal.Round(lastTradingPrice / peRatio, 4);
            //    }
            //    else if (companyProfitPerShare > 0)
            //    {
            //        //companyProfitPerShare = decimal.Round(companyProfitPerShare * 365M / DateTime.Today.DayOfYear, 4);
            //        companyProfitPerShare = decimal.Round(companyProfitPerShare, 4);
            //    }

            //    stockInfo.CompanyProfitPerShare = Convert.ToString(companyProfitPerShare);
            //}
            try
            {
                string jsonString = loadJsonString(url);
                StockBasicInfoParser parser = JsonConvert.DeserializeObject<StockBasicInfoParser>(jsonString);
                stockInfo.DateOfInfo = Convert.ToString(parser.snapShot.date) + " " + Convert.ToString(parser.snapShot.time);
                stockInfo.LastTradingPrice = Convert.ToString(Math.Round(parser.snapShot.preClose, 5));
                stockInfo.PERatio = Convert.ToString(Math.Round(parser.snapShot.peRatio, 5));
                stockInfo.CompanyProfitPerShare = Convert.ToString(Math.Round(parser.snapShot.perShareEarn, 5));
                stockInfo.CompanyName = parser.snapShot.stockBasic.stockName;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            
            
        }

        /// <summary>
        /// Change the company profit per share parsing process to be handled async
        /// </summary>
        /// <param name="url"></param>
        /// <param name="stockInfo"></param>
        public static void parseCompanyProfitPerShare(string url, ref StockInfo stockInfo)
        {
            InfoParsingDel parseDel = new InfoParsingDel(parseCompanyProfitPerShareCore);

            parseDel.Invoke(url, ref stockInfo);
        }


        /// <summary>
        /// This is the core process for parsing company profit per share
        /// </summary>
        /// <param name="url"></param>
        /// <param name="stockInfo"></param>
        private static void parseCompanyProfitPerShareCore(string url, ref StockInfo stockInfo)
        {
            // The following is to parse more accurate company profit per share from ifeng api.
            HtmlAgilityPack.HtmlDocument ProfitPerShareHtmlDocument = Utils.loadHtmlDocument(url, Encoding.GetEncoding("utf-8"));

            HtmlNode stockInfoTableNode = ProfitPerShareHtmlDocument.DocumentNode.SelectSingleNode("//table[@class='tab01 cDGray']");

            if (stockInfoTableNode != null)
            {
                for (int idx = 5; idx > 0; idx--)
                {
                    HtmlNode dateTdNode = Utils.findNodeByText(stockInfoTableNode, ".//tr/td", "截止日期", idx, false);
                    DateTime publishDate = new DateTime();
                    DateTime.TryParse(dateTdNode.InnerText.Trim(), out publishDate);
                    if (publishDate.Equals(DateTime.Today.AddDays(-DateTime.Today.DayOfYear)))
                    {
                        HtmlNode profitPerShareNode = Utils.findNodeByText(stockInfoTableNode, ".//tr/td", "每股收益(元)", idx, false);
                        stockInfo.CompanyProfitPerShare = profitPerShareNode.InnerText;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Change the company profit sharing parsing process to be async
        /// </summary>
        /// <param name="url"></param>
        /// <param name="stockInfo"></param>
        public static void parseCompanyProfitSharing(string url, ref StockInfo stockInfo)
        {
            InfoParsingDel parseDel = new InfoParsingDel(parseCompanyProfitSharingCore);

            parseDel.Invoke(url, ref stockInfo);
        }

        /// <summary>
        /// This is the core process for parsing company profit sharing
        /// </summary>
        /// <param name="url"></param>
        /// <param name="stockInfo"></param>
        private static void parseCompanyProfitSharingCore(string url, ref StockInfo stockInfo)
        {
            // The following implementation is based on IFeng Stock API.
            // It might need to be changed if we decide to use other API instead
            HtmlAgilityPack.HtmlDocument lastProfitSharingHtmlDocument = Utils.loadHtmlDocument(url, Encoding.GetEncoding("utf-8"));

            HtmlNodeCollection lastProfitSharingTableNodes = lastProfitSharingHtmlDocument.DocumentNode.SelectNodes("//table[@class='tab01']");

            if(lastProfitSharingTableNodes != null)
            {
                foreach (HtmlNode profitSharingTableNode in lastProfitSharingTableNodes)
                {
                    HtmlNode tdNode = Utils.findNodeByText(profitSharingTableNode, ".//tr/td", "公告日期", 1, true);
                    DateTime publishDate = new DateTime();
                    DateTime.TryParse(tdNode.InnerText.Trim(), out publishDate);
                    HtmlNode profitSharingNode = Utils.findNodeByText(profitSharingTableNode, ".//tr/td", "每10股现金(含税)", 1, true);
                    string profitSharingNodeText = profitSharingNode.InnerText.Substring(0, profitSharingNode.InnerText.Length - 1);
                    stockInfo.addProfitSharingInfo(publishDate, profitSharingNodeText);
                }
            }
        }

        public static string getValueFromArray(string[] inputArray, int idx)
        {
            return inputArray.Length > idx ? inputArray[idx] : "";
        }


    }
}
