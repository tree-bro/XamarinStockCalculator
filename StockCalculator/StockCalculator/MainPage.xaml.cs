using PCLStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StockCalculator
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}


        //parse input parameters before calculation
        private decimal marketPrice;
        private decimal tradingTaxRate;
        private decimal profitPerShare;
        private decimal profitSharingRate;
        private short companyDuration;
        private decimal discountRate;
        private decimal normalGrowthRate;
        private decimal highSpeedGrowthRate;
        private short highSpeedGrwothDuration;
        private decimal profitSharingTaxRate;
        private short depressionFrequency;
        private decimal depressionLossRate;
        private short stockHeldDuration;

        private decimal assumedStockPriceGrowth = new decimal(0.02);

        //temp variables for calculation
        private decimal totalInnerValue;
        private decimal currentInterest;
        private decimal totalProfitSharing;
        private decimal totalTradingTaxPaid;
        private decimal resultForSell;
        private decimal feePaid;
        private decimal totalBuyTax;
        private decimal totalSellTax;
        private decimal currentGrowth;

        private string successMessage = PromptMessages.successMessageZH;

        public void BtnCalculateClicked(object sender, EventArgs e)
        {
            //Retrieve input params.
            parseInputParameters();

            //Declare temp variables for calculation
            resetTempCalVariables();

            for (var idx = 1; idx <= companyDuration; idx++)
            {
                //calculate the risk interest rate for current year
                currentInterest = currentInterest * (decimal.One / (decimal.One + discountRate));

                //If HSG is larger than 0, and current year has not reach the upper limit for HSG, then calculate for HSG
                if (highSpeedGrowthRate > 0 && highSpeedGrwothDuration >= idx)
                {
                    currentGrowth = currentGrowth * (decimal.One + highSpeedGrowthRate);
                }
                else
                {
                    currentGrowth = currentGrowth * (decimal.One + normalGrowthRate);
                }

                if (idx % depressionFrequency > 0)
                {
                    totalProfitSharing = totalProfitSharing + currentInterest * profitPerShare * currentGrowth * profitSharingRate * (decimal.One - profitSharingTaxRate);
                    totalTradingTaxPaid = totalTradingTaxPaid + currentInterest * profitPerShare * currentGrowth * profitSharingRate * profitSharingTaxRate;
                    totalInnerValue = totalInnerValue + currentInterest * profitPerShare * currentGrowth * (1 - profitSharingRate);
                    resultForSell = resultForSell + profitPerShare * currentGrowth * (1 - profitSharingRate);
                }
                else
                {
                    totalProfitSharing = totalProfitSharing + currentInterest * profitPerShare * currentGrowth * profitSharingRate * (decimal.One - profitSharingTaxRate) * (decimal.One - depressionLossRate);
                    totalTradingTaxPaid = totalTradingTaxPaid + currentInterest * profitPerShare * currentGrowth * profitSharingRate * profitSharingTaxRate * (decimal.One - depressionLossRate);
                    totalInnerValue = totalInnerValue + currentInterest * profitPerShare * currentGrowth * (decimal.One - profitSharingRate) * (decimal.One - depressionLossRate);
                    resultForSell = resultForSell + profitPerShare * currentGrowth * (decimal.One - profitSharingRate) * (decimal.One - depressionLossRate);
                }

                if (stockHeldDuration > 0 && idx % stockHeldDuration == 0)
                {
                    totalBuyTax = totalBuyTax + marketPrice * (decimal.One + idx * assumedStockPriceGrowth) * tradingTaxRate * currentInterest;
                    totalSellTax = totalSellTax + marketPrice * (decimal.One + idx * assumedStockPriceGrowth) * tradingTaxRate * currentInterest;
                }
            }
            totalSellTax = totalSellTax + resultForSell * currentInterest * tradingTaxRate;

            string displaySuccessMessage = successMessage
                                                .Replace("[_MARKET_PRICE_]", Convert.ToString(decimal.Round(marketPrice, 5)))
                                                .Replace("[_INNER_VALUE_DEDUCT_PROFIT_SHARING_]", Convert.ToString(decimal.Round(totalInnerValue, 5)))
                                                .Replace("[_PROFIT_SHARING_]", Convert.ToString(decimal.Round(totalProfitSharing, 5)))
                                                .Replace("[_TRADING_TAX_PAID_]", Convert.ToString(decimal.Round(totalTradingTaxPaid, 5)))
                                                .Replace("[_BUY_TAX_PAID_]", Convert.ToString(decimal.Round(totalBuyTax, 5)))
                                                .Replace("[_SELL_TAX_PAID_]", Convert.ToString(decimal.Round(totalSellTax, 5)))
                                                .Replace("[_INNER_VALUE_]", Convert.ToString(decimal.Round(totalInnerValue + totalProfitSharing - totalBuyTax - totalSellTax - totalTradingTaxPaid, 5)))
                                                .Replace("[_MARKET_PRICE_TO_INNER_VALUE_]", Convert.ToString(decimal.Round(marketPrice / (totalInnerValue + totalProfitSharing - totalBuyTax - totalSellTax - totalTradingTaxPaid) * 100, 5)));

            DisplayAlert("计算结果", displaySuccessMessage, "OK");
        }

        public async void BtnParseCompanyInfoClicked(object sender, EventArgs e)
        {
            string stockID = StockID.Text.Trim();
            string originalStockID = stockID;
            StockMarketTypes marketType = Utils.checkMarketType(stockID);

            switch (marketType)
            {
                case StockMarketTypes.CHINA_SZ_EXCHANGE_MARKET:
                    stockID = "sz" + stockID;
                    break;
                case StockMarketTypes.CHINA_SH_EXCHANGE_MARKET:
                    stockID = "sh" + stockID;
                    break;
                case StockMarketTypes.HK_EXCHANGE_MARKET:
                    stockID = "hk" + stockID;
                    break;
                case StockMarketTypes.UNKNOWN:
                    await DisplayAlert("未知的股票编码类型", PromptMessages.unknownStockIDMessageZH, "Dismiss");
                    break;
            }

            string stockInfoRequestURL = URLTemplates.baiduTemplateByID.Replace("[_STOCK_ID_]", stockID);
            string profitPerShareRequestURL = URLTemplates.ifengCaiWuTemplateByID.Replace("[_STOCK_ID_]", originalStockID);
            string lastProfitSharingRequestURL = URLTemplates.ifengProfitSharingTemplateByID.Replace("[_STOCK_ID_]", originalStockID);

            StockInfo stockInfo = new StockInfo();

            Utils.parseCompanyBasicInfo(stockInfoRequestURL, ref stockInfo);
            Utils.parseCompanyProfitSharing(lastProfitSharingRequestURL, ref stockInfo);

            bool parseInfo = await DisplayAlert("是否导入公司资料？", stockInfo.printInfo(), "YES", "NO");
            if (parseInfo)
            {
                MarketPrice.Text = stockInfo.LastTradingPrice;
                ProfitPerShare.Text = stockInfo.CompanyProfitPerShare;
                decimal marketPrice = 0m;
                decimal.TryParse(stockInfo.LastTradingPrice, out marketPrice);
                if(marketPrice > 0m)
                {
                    ProfitSharingPerShare.Text = marketPrice > 0 ? Convert.ToString(Math.Round(stockInfo.ProfitSharingDictionary.Values.Where(n => n > 0m).Take(5).Sum() / 5 / marketPrice * 100,2)) : "0";
                }
                StockName.Text = stockInfo.CompanyName;
            }
        }

        private string getHistoryFilePath()
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            return Path.Combine(rootFolder.Path, "CalculationHistory.csv");
        }

        private async void BtnHistoryManagementClicked(object sender, EventArgs e)
        {
            string result = await DisplayActionSheet("历史记录管理",null,null,new string[] { "保存当前记录","管理历史记录","清除历史记录"});
            if(result == "保存当前记录")
            {
                //DBUtils.initDBConnection("Data Source=StockCalculationDB.db3");
                //DBUtils.executeCommand("DELETE FROM CalculationHistory WHERE StockID=" + StockID.Text);
                //DBUtils.executeCommand("INSERT INTO CalculationHistory VALUES (" + StockID.Text+",," + MarketPrice.Text + "," + ProfitPerShare.Text+ "," + ProfitSharingPerShare.Text+"," + TradeTax.Text + "," + CompanyDuration.Text + "," + DiscountRate.Text + "," + NaturalGrowthRate.Text + "," + HighSpeedGrowthRate.Text + "," + HighSpeedGrowthDuration.Text + "," + ProfitSharingTax.Text + "," + DepressionCycle.Text + "," + DepressionLossRate.Text + "," + StockHeldDuration.Text + ")");
                //DBUtils.closeDBConnection();
                //if (!File.Exists(getHistoryFilePath()))
                //{
                //    File.Create(getHistoryFilePath());
                //}
                StringBuilder sb = new StringBuilder();
                sb.Append("当前记录即将被保存至代码[");
                sb.Append(StockID.Text);
                sb.Append("]下，是否确认保存？");
                bool confirmSaveResult = await DisplayAlert("确认保存记录？", sb.ToString(), "OK", "Cancel");
                if (confirmSaveResult)
                {
                    string[] originalContent = File.Exists(getHistoryFilePath()) ? File.ReadAllLines(getHistoryFilePath()) : null;
                    List<string> inputContent = originalContent == null || originalContent.Length == 0 ? new List<string>() : originalContent.Where(s => !s.StartsWith(StockID.Text + ",") && s.Trim() != "").ToList();
                    StringBuilder contentBuilder = new StringBuilder();
                    contentBuilder.Append(StockID.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(StockName.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(MarketPrice.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(ProfitPerShare.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(ProfitSharingPerShare.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(TradeTax.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(CompanyDuration.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(DiscountRate.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(NaturalGrowthRate.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(HighSpeedGrowthRate.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(HighSpeedGrowthDuration.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(ProfitSharingTax.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(DepressionCycle.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(DepressionLossRate.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(StockHeldDuration.Text);
                    contentBuilder.Append(",");
                    contentBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    inputContent.Add(contentBuilder.ToString());
                    File.WriteAllLines(getHistoryFilePath(), inputContent.ToArray());
                }
            }else if(result == "管理历史记录")
            {
                if (File.Exists(getHistoryFilePath()))
                {
                    //DisplayAlert("历史记录", File.ReadAllText(getHistoryFilePath()), "OK");
                    string[] contentArray = File.ReadAllLines(getHistoryFilePath());
                    string[] displayArray = contentArray.Select(s => s.Split(',')).Select(sArr=>"股票代码:"+sArr[0]+"，股票名称:"+sArr[1]+"，更新日期:"+sArr.Last()).ToArray();
                    string checkHistoryResult = await DisplayActionSheet("管理历史记录", null, null, displayArray);
                    if (!displayArray.Contains(checkHistoryResult)) return;
                    string checkHistoryActionResult = await DisplayActionSheet("如何操作？", null, null, new string[] { "导入","删除","退出"});
                    int importIdx = displayArray.ToList().FindIndex(0, new Predicate<string>(s => s == checkHistoryResult));
                    if (checkHistoryActionResult == "导入")
                    {
                        if (importIdx >= 0)
                        {
                            string[] importContentArray = contentArray[importIdx].Split(',');
                            int idx = 0;
                            StockID.Text = importContentArray[idx];
                            idx++;
                            StockName.Text = importContentArray[idx];
                            idx++;
                            MarketPrice.Text = importContentArray[idx];
                            idx++;
                            ProfitPerShare.Text = importContentArray[idx];
                            idx++;
                            ProfitSharingPerShare.Text = importContentArray[idx];
                            idx++;
                            TradeTax.Text = importContentArray[idx];
                            idx++;
                            CompanyDuration.Text = importContentArray[idx];
                            idx++;
                            DiscountRate.Text = importContentArray[idx];
                            idx++;
                            NaturalGrowthRate.Text = importContentArray[idx];
                            idx++;
                            HighSpeedGrowthRate.Text = importContentArray[idx];
                            idx++;
                            HighSpeedGrowthDuration.Text = importContentArray[idx];
                            idx++;
                            ProfitSharingTax.Text = importContentArray[idx];
                            idx++;
                            DepressionCycle.Text = importContentArray[idx];
                            idx++;
                            DepressionLossRate.Text = importContentArray[idx];
                            idx++;
                            StockHeldDuration.Text = importContentArray[idx];
                        }
                    }
                    else if(checkHistoryActionResult == "删除")
                    {
                        File.WriteAllLines(getHistoryFilePath(),contentArray.Where(s=>s!= contentArray[importIdx]));
                    }
                    
                }
                
            }else if(result == "清除历史记录")
            {
                bool clearHistoryResult = await DisplayAlert("清除历史记录","是否确认清除所有历史记录？","OK","Cancel");
                if (clearHistoryResult)
                {
                    File.WriteAllText(getHistoryFilePath(), "");
                }
            }
        }



        private void parseInputParameters()
        {
            decimal.TryParse(MarketPrice.Text.Trim(), out marketPrice);
            decimal.TryParse(TradeTax.Text.Trim(), out tradingTaxRate);
            tradingTaxRate = tradingTaxRate / 100M;
            decimal.TryParse(ProfitPerShare.Text.Trim(), out profitPerShare);
            decimal.TryParse(ProfitSharingPerShare.Text.Trim(), out profitSharingRate);
            profitSharingRate = profitSharingRate / 100M;
            short.TryParse(CompanyDuration.Text.Trim(), out companyDuration);
            decimal.TryParse(DiscountRate.Text.Trim(), out discountRate);
            discountRate = discountRate / 100M;
            decimal.TryParse(NaturalGrowthRate.Text.Trim(), out normalGrowthRate);
            normalGrowthRate = normalGrowthRate / 100M;
            decimal.TryParse(HighSpeedGrowthRate.Text.Trim(), out highSpeedGrowthRate);
            highSpeedGrowthRate = highSpeedGrowthRate / 100M;
            short.TryParse(HighSpeedGrowthDuration.Text.Trim(), out highSpeedGrwothDuration);
            decimal.TryParse(ProfitSharingTax.Text.Trim(), out profitSharingTaxRate);
            profitSharingTaxRate = profitSharingTaxRate / 100M;
            short.TryParse(DepressionCycle.Text.Trim(), out depressionFrequency);
            decimal.TryParse(DepressionLossRate.Text.Trim(), out depressionLossRate);
            depressionLossRate = depressionLossRate / 100M;
            short.TryParse(StockHeldDuration.Text.Trim(), out stockHeldDuration);
        }

        private void resetTempCalVariables()
        {
            totalInnerValue = decimal.Zero;
            currentInterest = decimal.One;
            totalProfitSharing = decimal.Zero;
            totalTradingTaxPaid = decimal.Zero;
            resultForSell = decimal.Zero;
            feePaid = decimal.Zero;
            totalBuyTax = marketPrice * tradingTaxRate;
            totalSellTax = decimal.Zero;
            currentGrowth = decimal.One;
        }
    }
}
