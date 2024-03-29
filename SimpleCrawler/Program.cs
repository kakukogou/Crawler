﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace SimpleCrawler
{
    public class City
    {
        public string CityName { get; set; }

        public Uri Uri { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {
            RegisterBig5Encoding();
            CrawlAll();
        }

        private static void RegisterBig5Encoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var big5 = Encoding.GetEncoding(950);
            Console.WriteLine($"Name : {big5.EncodingName} , CodePage :{big5.CodePage}");
        }

        private static void CrawlAll()
        {
            var startMonth = new DateTime(2010, 1, 1);
            var today = DateTime.Today;
            var endMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

            for (var month = startMonth; month <= endMonth; month = month.AddMonths(1))
            {
                //var url = "https://mops.twse.com.tw/nas/t21/sii/t21sc03_108_6_0.html";
                var url = $"https://mops.twse.com.tw/nas/t21/sii/t21sc03_{month.Year - 1911}_{month.Month}_0.html";
                FinanceCrawler(url);
            }

        }

        private static void FinanceCrawler(string url)
        {
            //var url = "https://mops.twse.com.tw/nas/t21/sii/t21sc03_108_6_0.html";
            var crawler = new SimpleCrawler();

            crawler.CookiesContainer = new System.Net.CookieContainer();

            crawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬虫开始抓取地址：" + e.Uri.ToString());
            };

            crawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬虫抓取出现错误：" + e.Uri.ToString() + "，异常消息：" + e.Exception.Message);
            };

            crawler.OnCompleted += (s, e) =>
            {
                // Get year month.
                var tokens = e.Uri.ToString().Split(new char[] { '/', '_', '.' });
                var year = int.Parse(tokens[10]);
                var month = int.Parse(tokens[11]);

                // Parse.
                var links = new Parsers.MonthlyRevenueParser().Parse(year, month, e.PageSource).ToList();
                Console.WriteLine("===============================================");
                Console.WriteLine("爬虫抓取任务完成！合计 " + links.Count + " 个城市。");
                Console.WriteLine("耗时：" + e.Milliseconds + "毫秒");
                Console.WriteLine("线程：" + e.ThreadId);
                Console.WriteLine("地址：" + e.Uri.ToString());

                // Write to file.
                var text = JsonConvert.SerializeObject(links);
                
                var filePath = $@"/Users/kakukogou/Projects/MonthlyRevenueData/{year}_{month}.txt";
                System.IO.File.WriteAllText(filePath, text);
            };

            crawler.Start(new Uri(url)).Wait();
        }

        public static void CityCrawler()
        {

            var cityUrl = "http://hotels.ctrip.com/citylist";//定义爬虫入口URL
            var cityList = new List<City>();//定义泛型列表存放城市名称及对应的酒店URL
            var cityCrawler = new SimpleCrawler();//调用刚才写的爬虫程序
            cityCrawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬虫开始抓取地址：" + e.Uri.ToString());
            };
            cityCrawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬虫抓取出现错误：" + e.Uri.ToString() + "，异常消息：" + e.Exception.Message);
            };
            cityCrawler.OnCompleted += (s, e) =>
            {
                //使用正则表达式清洗网页源代码中的数据
                var links = Regex.Matches(e.PageSource, @"<a[^>]+href=""*(?<href>/hotel/[^>\s]+)""\s*[^>]*>(?<text>(?!.*img).*?)</a>", RegexOptions.IgnoreCase);
                foreach (Match match in links)
                {
                    var city = new City
                    {
                        CityName = match.Groups["text"].Value,
                        Uri = new Uri("http://hotels.ctrip.com" + match.Groups["href"].Value
                    )
                    };
                    if (!cityList.Contains(city)) cityList.Add(city);//将数据加入到泛型列表
                    Console.WriteLine(city.CityName + "|" + city.Uri);//将城市名称及URL显示到控制台
                }
                Console.WriteLine("===============================================");
                Console.WriteLine("爬虫抓取任务完成！合计 " + links.Count + " 个城市。");
                Console.WriteLine("耗时：" + e.Milliseconds + "毫秒");
                Console.WriteLine("线程：" + e.ThreadId);
                Console.WriteLine("地址：" + e.Uri.ToString());
            };
            cityCrawler.Start(new Uri(cityUrl)).Wait();//没被封锁就别使用代理：60.221.50.118:8090
        }
    }
}
