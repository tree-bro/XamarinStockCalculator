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

        private string preferStockListFileName = "PreferStockList.csv";

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
                    ProfitSharingPerShare.Text = Convert.ToString(marketPrice / stockInfo.ProfitSharingDictionary.Values.Where(n => n > 0m).Take(5).Sum() / 5);
                }
            }
        }

        private string getHistoryFilePath()
        {
            //return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "CalculationHistory.csv");
            //return "CalculationHistory.csv"; 
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            return Path.Combine(rootFolder.Path, "CalculationHistory.csv");
        }

        private async void BtnHistoryManagementClicked(object sender, EventArgs e)
        {
            string result = await DisplayActionSheet("历史资料管理",null,null,new string[] { "保存当前资料","查看历史资料"});
            if(result == "保存当前资料")
            {
                //DBUtils.initDBConnection("Data Source=StockCalculationDB.db3");
                //DBUtils.executeCommand("DELETE FROM CalculationHistory WHERE StockID=" + StockID.Text);
                //DBUtils.executeCommand("INSERT INTO CalculationHistory VALUES (" + StockID.Text+",," + MarketPrice.Text + "," + ProfitPerShare.Text+ "," + ProfitSharingPerShare.Text+"," + TradeTax.Text + "," + CompanyDuration.Text + "," + DiscountRate.Text + "," + NaturalGrowthRate.Text + "," + HighSpeedGrowthRate.Text + "," + HighSpeedGrowthDuration.Text + "," + ProfitSharingTax.Text + "," + DepressionCycle.Text + "," + DepressionLossRate.Text + "," + StockHeldDuration.Text + ")");
                //DBUtils.closeDBConnection();
                //if (!File.Exists(getHistoryFilePath()))
                //{
                //    File.Create(getHistoryFilePath());
                //}
                
                string[] originalContent = File.ReadAllLines(getHistoryFilePath());
                List<string> inputContent = originalContent.Where(s => !s.StartsWith(StockID.Text + ",") && s.Trim() != "").ToList();
                inputContent.Add(StockID.Text + "," + MarketPrice.Text + "," + ProfitPerShare.Text + "," + ProfitSharingPerShare.Text + "," + TradeTax.Text + "," + CompanyDuration.Text + "," + DiscountRate.Text + "," + NaturalGrowthRate.Text + "," + HighSpeedGrowthRate.Text + "," + HighSpeedGrowthDuration.Text + "," + ProfitSharingTax.Text + "," + DepressionCycle.Text + "," + DepressionLossRate.Text + "," + StockHeldDuration.Text);
                File.WriteAllLines(getHistoryFilePath(), inputContent.ToArray());
            }else if(result == "查看历史资料")
            {
                if (File.Exists(getHistoryFilePath()))
                {
                    DisplayAlert("历史记录", File.ReadAllText(getHistoryFilePath()), "OK");
                }
                
            }
        }



        private void parseInputParameters()
        {
            decimal.TryParse(MarketPrice.Text.Trim(), out marketPrice);
            decimal.TryParse(TradeTax.Text.Trim(), out tradingTaxRate);
            tradingTaxRate = tradingTaxRate / 100M;
            decimal.TryParse(ProfitPerShare.Text.Trim(), out profitPerShare);
            decimal.TryParse(ProfitSharingTax.Text.Trim(), out profitSharingRate);
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
