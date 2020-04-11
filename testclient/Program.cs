using System;
using System.Collections.Generic;
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
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(
            //        new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            //    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            //Find the average volume of MSFT in the past 7 days
            var stringTask = client.GetStringAsync("https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=MSFT&apikey=NIWJI7M0ZZM7YOBA&datatype=csv");
            var json = await stringTask;// JsonConvert.DeserializeObject<dynamic>(stringTask);
            var outputList = json.FromCsv <List<QueryModel>>();
           // var dailyList = test["Time Series (Daily)"];
            decimal volumeTotal = 0;
            foreach (var daily in outputList)
            {
               
                        var currentDate = DateTime.Now;
                if (daily.TimeStamp < DateTime.Now.AddDays(-7))
                    break;

                        volumeTotal += (daily.Volume);
                    
                
            }
            var averageVolume = volumeTotal / 7;
            Console.Write(averageVolume);

            //Find the highest closing price of AAPL in the past 6 months
            stringTask = client.GetStringAsync("https://www.alphavantage.co/query?function=TIME_SERIES_MONTHLY&symbol=AAPL&apikey=NIWJI7M0ZZM7YOBA&datatype=csv");
            json = await stringTask;// JsonConvert.DeserializeObject<dynamic>(stringTask);
            outputList = json.FromCsv<List<QueryModel>>();
            // var dailyList = test["Time Series (Daily)"];
            decimal closingPrice = 0;
            foreach (var montly in outputList)
            {

                var currentDate = DateTime.Now;
                if (montly.TimeStamp < DateTime.Now.AddMonths(-6))
                    break;

                if (montly.Close > closingPrice)
                    closingPrice = montly.Close;



            }
            Console.Write(closingPrice);
            //Find the difference between open and close price for BA for every day in the last month
            stringTask = client.GetStringAsync("https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=BA&apikey=NIWJI7M0ZZM7YOBA&datatype=csv");
            json = await stringTask;// JsonConvert.DeserializeObject<dynamic>(stringTask);
            outputList = json.FromCsv<List<QueryModel>>();
            // var dailyList = test["Time Series (Daily)"];
            var dict = new Dictionary<DateTime, decimal>();

            foreach (var daily in outputList)
            {

                var currentDate = DateTime.Now;
                if (daily.TimeStamp < DateTime.Now.AddMonths(-1))
                    break;

                dict.Add(daily.TimeStamp, Math.Abs(daily.Open - daily.Close));

            }
            foreach (var day in dict)
            {
                Console.Write(string.Format("{0} {1}", day.Key, day.Value));

            }
            //Given a list of stock symbols, find the symbol with the largest return over the past month
            //return = (p1 - p0) + D / p0
            stringTask = client.GetStringAsync("https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=MSFT&apikey=NIWJI7M0ZZM7YOBA");


            var msg = await stringTask;
            Console.Write(msg);


        }
    }
}
