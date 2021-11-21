using System;
using System.Collections.Generic;

namespace SimpleCrawler.Models
{
    public class MonthlyRevenueModel
    {
        public int ReportYear { get; set; }

        public int ReportMonth { get; set; }

        public string StockId { get; set; }

        public string StockName { get; set; }

        public long? ThisMonthRevenue { get; set; }

        public long? LastMonthRevenue { get; set; }
        
        public long? LastYearSameMonthRevenue { get; set; }
        
        public double? DeltaToLastMonth { get; set; }

        public double? DeltaToLastYearSameMonth { get; set; }

        public long? ThisYearAggregatedRevenue { get; set; }

        public long? LastYearAggregatedRevenue { get; set; }

        public double? DeltaToLastYearAggregatedRevenue { get; set; }
    }
}
