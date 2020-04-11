using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceStack;

namespace testclient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        //"NIWJI7M0ZZM7YOBA"
        static async Task Main(string[] args)
        {

            await APICall();
        }

        static async Task APICall()
        {

            //Find the average volume of MSFT in the past 7 days
            var averageVolume = await GetAverageVolume("MSFT", 7);
            Console.WriteLine("Average Volume for MSFT in the past 7 days " + averageVolume);

            //Find the highest closing price of AAPL in the past 6 months
            var closingPrice = await GetHighestClosingPrice("AAPL", 6);
            Console.WriteLine("Highest Closing Price for AAPL in the past 6 months " + closingPrice);
           
            //Find the difference between open and close price for BA for every day in the last month
            var dict = await (GetDifferenceBetweenOpenAndClose("BA", 1));

            Console.WriteLine("the difference between open and close price for BA for every day in the last month");

            foreach (var day in dict)
            {
                Console.WriteLine(string.Format("{0} {1}", day.Key.ToString("MMMM-dd-yyyy"), day.Value));

            }
            //Given a list of stock symbols, find the symbol with the largest return over the past month
            //This needs to be run by itself as the api has a limit on the number of calls that can be run in a minute.
            var stockRet = await GetLargestStockReturn(new List<string> { "IBM", "BA", "AAPL" });
            Console.WriteLine("The symbol with the largest return is " + stockRet.Item1 + " with " + stockRet.Item2);


        }
        public static async Task<decimal> GetAverageVolume(string symbol, int range)
        {
            var stringTask = client.GetStringAsync($"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey=NIWJI7M0ZZM7YOBA&datatype=csv");
            var json = await stringTask;// JsonConvert.DeserializeObject<dynamic>(stringTask);
            var outputList = json.FromCsv<List<QueryModel>>();
            // var dailyList = test["Time Series (Daily)"];
            decimal volumeTotal = 0;
            foreach (var daily in outputList)
            {

                var currentDate = DateTime.Now;
                if (daily.TimeStamp < DateTime.Now.AddDays(-range))
                    break;

                volumeTotal += (daily.Volume);


            }
            var averageVolume = volumeTotal / range;
            return averageVolume;

        }
        public static async Task<decimal> GetHighestClosingPrice(string symbol, int range)
        {
           var stringTask = client.GetStringAsync($"https://www.alphavantage.co/query?function=TIME_SERIES_MONTHLY&symbol={symbol}&apikey=NIWJI7M0ZZM7YOBA&datatype=csv");
            var json = await stringTask;// JsonConvert.DeserializeObject<dynamic>(stringTask);
            var outputList = json.FromCsv<List<QueryModel>>();
            // var dailyList = test["Time Series (Daily)"];
            decimal closingPrice = 0;
            foreach (var monthly in outputList)
            {

                var currentDate = DateTime.Now;
                if (monthly.TimeStamp < DateTime.Now.AddMonths(-range))
                    break;

                if (monthly.Close > closingPrice)
                    closingPrice = monthly.Close;



            }

            return closingPrice;
        }
         public static async Task<Dictionary<DateTime, decimal>> GetDifferenceBetweenOpenAndClose(string symbol, int range)
        {
           var stringTask = client.GetStringAsync($"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey=NIWJI7M0ZZM7YOBA&datatype=csv");
           var  json = await stringTask;// JsonConvert.DeserializeObject<dynamic>(stringTask);
           var  outputList = json.FromCsv<List<QueryModel>>();
            // var dailyList = test["Time Series (Daily)"];
            var dict = new Dictionary<DateTime, decimal>();

            foreach (var daily in outputList)
            {

                var currentDate = DateTime.Now;
                if (daily.TimeStamp < DateTime.Now.AddMonths(-range))
                    break;

                dict.Add(daily.TimeStamp, Math.Abs(daily.Open - daily.Close));

            }

            return dict;
        }
         public static async Task<Tuple<string, decimal>> GetLargestStockReturn(List<string> stocks)
        {
            Tuple<string, decimal> largestStockReturn = null;

            foreach (var stock in stocks)
            {
                var output = await GetStockReturn(stock);
                if (largestStockReturn == null)
                    largestStockReturn = output;
                else {
                    if (largestStockReturn.Item2 < output.Item2)
                        largestStockReturn = output;
                }
            }
            return largestStockReturn;

        }
          public static async Task<Tuple<string, decimal>> GetStockReturn(string symbol)
        {
            var stringTask = client.GetStringAsync($"https://www.alphavantage.co/query?function=TIME_SERIES_MONTHLY&symbol={symbol}&apikey=NIWJI7M0ZZM7YOBA&datatype=csv");
            var json = await stringTask;// JsonConvert.DeserializeObject<dynamic>(stringTask);
            var outputList = json.FromCsv<List<QueryModel>>(); 

            var close = outputList.First().Close;
            var lastMonth = DateTime.Now.AddMonths(-1);
            var open = outputList.First(t => t.TimeStamp.Month == lastMonth.Month && t.TimeStamp.Year == lastMonth.Year).Open;
  
            return new Tuple<string, decimal>(symbol, close-open);
        }
    }
}
