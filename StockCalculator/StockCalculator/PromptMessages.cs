namespace StockCalculator
{
    public class PromptMessages
    {
        public static string successMessageEN =
            @"
            *******************************
            Inner value after deduct profit sharing: [_INNER_VALUE_DEDUCT_PROFIT_SHARING_]
            Total inner value for this stock: [_INNER_VALUE_]
            Trading price: [_MARKET_PRICE_]
            *******************************

            *******************************
            Total profit sharing: [_PROFIT_SHARING_]
            Total trading tax paid: [_TRADING_TAX_PAID_]
            Total buy tax paid: [_BUY_TAX_PAID_]
            Total sell tax paid: [_SELL_TAX_PAID_]
            *******************************

            *******************************
            Market Price/Inner Value: [_MARKET_PRICE_TO_INNER_VALUE_]%
            *******************************
            ";
        public static string successMessageZH =
            @"
            *******************************
            扣除分红后的内在价值: [_INNER_VALUE_DEDUCT_PROFIT_SHARING_]
            总内在价值: [_INNER_VALUE_]
            交易价格: [_MARKET_PRICE_]
            *******************************

            *******************************
            总分红: [_PROFIT_SHARING_]
            总交易印花税: [_TRADING_TAX_PAID_]
            总买入印花税: [_BUY_TAX_PAID_]
            总卖出印花税: [_SELL_TAX_PAID_]
            *******************************

            *******************************
            市价/内在价值: [_MARKET_PRICE_TO_INNER_VALUE_]%
            *******************************
            ";

        public static string parseCompanyDetailsFormatErrorEN =
            @"The selected company details file must be a csv file!";

        public static string parseCompanyDetailsFormatErrorZH =
            @"所选公司资料文件必须为csv格式!";

        public static string parseCompanyDetailsSuccessMessageEN =
            @"Successfully parsed company details from file [[_FILE_PATH_]]!";

        public static string parseCompanyDetailsSuccessMessageZH =
            @"成功从下列文件中获取公司资料 [[_FILE_PATH_]]!";

        public static string parseCompanyDetailsFromServerSuccessMessageEN =
            @"Successfully parsed company details [[_COMPANY_NAME_]]!";

        public static string parseCompanyDetailsFromServerSuccessMessageZH =
            @"成功从服务器获取公司资料 [[_COMPANY_NAME_]]!";

        public static string parseCompanyDetailsFromServerErrorEN =
            @"Please retrieve the company info before parsing company details!";

        public static string parseCompanyDetailsFromServerErrorZH =
            @"请先获取公司资料!";

        public static string retrieveStockInfoSuccessMessageEN =
            @"Successfully retrieved the stock info!";

        public static string retrieveStockInfoSuccessMessageZH =
            @"成功获取股票信息！";

        public static string retrieveStockInfoFailedMessageEN =
            @"Stock info not existed! Please try again later.";

        public static string retrieveStockInfoFailedMessageZH =
            @"未能获取股票信息！请稍后再试。";

        public static string unknownStockIDMessageEN =
            @"The provided stock ID is unknown, please check and input again!";

        public static string unknownStockIDMessageZH =
            @"未知的股票代码!请查实后再输入一次！";

        public static string clearPreferStockListSuccessMessageEN =
            @"Successfully cleared prefer stock list!";

        public static string clearPreferStockListSuccessMessageZH =
            @"成功清除偏好股票列表！";
    }
}
