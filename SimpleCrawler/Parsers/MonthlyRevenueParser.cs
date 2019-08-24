using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using HtmlAgilityPack;

namespace SimpleCrawler.Parsers
{
    public class MonthlyRevenueParser
    {
        private TR GetHtmlNodesThenDoFunc<TR>(string htmlString, string xPath, Func<HtmlNodeCollection, TR> func)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);
            return func(doc.DocumentNode.SelectNodes(xPath));
        }

        public IEnumerable<MonthlyRevenueData> Parse(string htmlString)
        {
            var industryTablesXPath = "/html[1]/body[1]/center[1]/center[1]/table[1]/tr[1]/td[1]/table";
            return GetHtmlNodesThenDoFunc(htmlString, industryTablesXPath, htmlNodeCollection =>
            {
                return htmlNodeCollection.ToList().SelectMany(industryTable => ParseSingleIndustry(industryTable));
            });
        }

        private IEnumerable<MonthlyRevenueData> ParseSingleIndustry(HtmlNode htmlNode)
        {

            var singleCompanyXPath = "./tr[2]/td[1]/table[1]/tr";
            return GetHtmlNodesThenDoFunc(htmlNode.InnerHtml, singleCompanyXPath, htmlNodeCollection =>
            {
                var companyRows = htmlNodeCollection.ToList().Skip(2).ToList();
                companyRows.RemoveAt(companyRows.Count - 1);

                return companyRows.Select(companyRow => ParseSingleCompany(companyRow));
            });
        }

        private MonthlyRevenueData ParseSingleCompany(HtmlNode htmlNode)
        {

            var companyItemsXPath = "./td";
            return GetHtmlNodesThenDoFunc(htmlNode.InnerHtml, companyItemsXPath, htmlNodeCollection =>
            {
                var items = htmlNodeCollection.ToList().Select(node => node.InnerHtml.Trim()).ToList();

                return new MonthlyRevenueData()
                {
                    stockId = items[0],
                    companyName = items[1],
                    thisMonthRevenue = ParseToLongNullable(items[2]),
                    lastMonthRevenue = ParseToLongNullable(items[3]),
                    lastYearSameMonthRevenue = ParseToLongNullable(items[4]),
                    deltaToLastMonth = ParseToDoubleNullable(items[5]),
                    deltaToLastYearSameMonth = ParseToDoubleNullable(items[6]),
                    aggregatedRevenue = ParseToLongNullable(items[7]),
                    lastYearAggregatedRevenue = ParseToLongNullable(items[8]),
                    deltaToLastXXX = ParseToDoubleNullable(items[9])
                };
            });
        }

        private static bool IsNonBreakingSpace(string cellValue) => cellValue.Equals("&nbsp;");
        private static long? ParseToLongNullable(string value) => IsNonBreakingSpace(value) ? (long?)null : long.Parse(value, NumberStyles.Integer | NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands);
        private static double? ParseToDoubleNullable(string value) => IsNonBreakingSpace(value) ? (double?)null : double.Parse(value, NumberStyles.Float | NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands);
    }
}
