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

        public IEnumerable<MonthlyRevenueData> Parse(int year, int month, string htmlString)
        {
            if (year <= 100)
            {
                var industryTablesXPath = "/html[1]/body[1]/center[1]/center[1]/table";
                return GetHtmlNodesThenDoFunc(htmlString, industryTablesXPath, htmlNodeCollection =>
                {
                    return htmlNodeCollection.ToList().Skip(1).SelectMany(industryTable => ParseSingleIndustry(year, month, industryTable));
                });
            }
            else if ((year == 101))
            {
                var industryTablesXPath = "/html[1]/body[1]/center[1]/center[1]/table[2]/tr[1]/td[1]/table";
                return GetHtmlNodesThenDoFunc(htmlString, industryTablesXPath, htmlNodeCollection =>
                {
                    return htmlNodeCollection.ToList().SelectMany(industryTable => ParseSingleIndustry(year, month, industryTable));
                });
            }
            else
            {
                var industryTablesXPath = "/html[1]/body[1]/center[1]/center[1]/table[1]/tr[1]/td[1]/table";
                return GetHtmlNodesThenDoFunc(htmlString, industryTablesXPath, htmlNodeCollection =>
                {
                    var industryRows = htmlNodeCollection.ToList().SkipLast(1).ToList();
                    return industryRows.SelectMany(industryTable => ParseSingleIndustry(year, month, industryTable));
                });
            }
        }

        private IEnumerable<MonthlyRevenueData> ParseSingleIndustry(int year, int month, HtmlNode htmlNode)
        {
            var singleCompanyXPath = string.Empty;
            if (year == 102 && month == 1)
            {
                singleCompanyXPath = "./tr[3]/td[1]/table[1]/tr";
            }
            else
            {
                singleCompanyXPath = "./tr[2]/td[1]/table[1]/tr";
            }

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

        private static bool IsNonDigits(string cellVlue) => IsNonBreakingSpace(cellVlue) || IsNonAppliableInChinese(cellVlue);
        private static bool IsNonBreakingSpace(string cellValue) => cellValue.Equals("&nbsp;");
        private static bool IsNonAppliableInChinese(string cellValue) => cellValue.Equals("不適用");
        private static long? ParseToLongNullable(string value) => IsNonDigits(value) ? (long?)null : long.Parse(value, NumberStyles.Integer | NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands);
        private static double? ParseToDoubleNullable(string value) => IsNonDigits(value) ? (double?)null : double.Parse(value, NumberStyles.Float | NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands);
    }
}
