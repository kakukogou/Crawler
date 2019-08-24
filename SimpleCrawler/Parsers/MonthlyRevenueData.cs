using System.Collections.Generic;

namespace SimpleCrawler.Parsers
{
    public struct MonthlyRevenueData
    {
        public string stockId;
        public string companyName;
        public long? thisMonthRevenue;
        public long? lastMonthRevenue;
        public long? lastYearSameMonthRevenue;
        public double? deltaToLastMonth;
        public double? deltaToLastYearSameMonth;
        public long? aggregatedRevenue;
        public long? lastYearAggregatedRevenue;
        public double? deltaToLastXXX;
    }
}
