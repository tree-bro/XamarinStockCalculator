using System.Collections.Generic;

namespace StockCalculator
{
    public class CompanyFileInfo
    {
        public Dictionary<string, string> FieldMapping { get; }

        public CompanyFileInfo()
        {
            //Here simply put all posibilities to the dic for ease of search
            this.FieldMapping = new Dictionary<string, string>
            {
                { "market price", "txtMarketPrice" },
                { "trading tax rate %", "txtTradeTaxRate" },
                { "profit per share", "txtProfitPerShare" },
                { "profit sharing rate %", "txtProfitSharingRate" },
                { "company duration", "txtCompanyDuration" },
                { "discount rate %", "txtDiscountRate" },
                { "normal growth rate %", "txtNormalGrowthRate" },
                { "high speed growth rate %", "txtHighSpeedGrowthRate" },
                { "high speed growth duration", "txtHighSpeedGrowthDuration" },
                { "profit sharing tax rate %", "txtProfitSharingTax" },
                { "depression frequency", "txtDepressionFrequency" },
                { "depression loss rate %", "txtDepressionLossRate" },
                { "stock held duration", "txtStockHeldDuration" },

                { "市场价", "txtMarketPrice" },
                { "交易印花税", "txtTradeTaxRate" },
                { "每股盈利", "txtProfitPerShare" },
                { "每股分红派系比例", "txtProfitSharingRate" },
                { "企业存续期", "txtCompanyDuration" },
                { "折现率", "txtDiscountRate" },
                { "自然增长率", "txtNormalGrowthRate" },
                { "高速增长率", "txtHighSpeedGrowthRate" },
                { "高速增长期", "txtHighSpeedGrowthDuration" },
                { "红股税率", "txtProfitSharingTax" },
                { "衰退周期", "txtDepressionFrequency" },
                { "衰退期损失率", "txtDepressionLossRate" },
                { "持股周期", "txtStockHeldDuration" }
            };
        }

        public string getControlName(string fieldName)
        {
            return this.FieldMapping[fieldName] == null ? "" : this.FieldMapping[fieldName];
        }
    }
}
