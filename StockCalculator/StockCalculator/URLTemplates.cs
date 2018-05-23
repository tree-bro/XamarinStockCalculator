namespace StockCalculator
{
    class URLTemplates
    {
        public static string szseURLTemplateByID =
            @"http://www.szse.cn/szseWeb/ShowReport.szse?CATALOGID=1815_stock&txtDMorJC=[_STOCK_ID_]&txtBeginDate=[_BEGIN_DATE_]&txtEndDate=[_END_DATE_]&tab1PAGENO=1&ENCODE=1&TABKEY=tab1";

        public static string sseURLTemplateByID =
            @"http://www.sse.com.cn/assortment/stock/list/info/turnover/index.shtml?COMPANY_CODE=[_STOCK_ID_]";

        public static string baiduTemplateByID =
            @"https://gupiao.baidu.com/stock/[_STOCK_ID_].html";

        public static string ifengCaiWuTemplateByID =
            @"http://app.finance.ifeng.com/data/stock/tab_cwjk.php?symbol=[_STOCK_ID_]";

        public static string ifengProfitSharingTemplateByID =
            @"http://app.finance.ifeng.com/data/stock/tab_fhpxjl.php?symbol=[_STOCK_ID_]";
    }
}
