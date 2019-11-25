using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ExchangeRateApiClient
{
    public static class Utils
    {
        static readonly HttpClient client = new HttpClient();

        public static async Task<IEnumerable<ExchangeRate>> ReadExchangeRatesForPeriod(string baseCurrency,
            string currency, DateTime firstDay, DateTime lastDay)
        {
            string firstDayString = firstDay.ToString("yyyy-MM-dd");
            string lastDayString = lastDay.ToString("yyyy-MM-dd");
            string uri =
                "https://api.exchangeratesapi.io/history?start_at=" +
                firstDayString +
                "&end_at=" +
                lastDayString +
                "&symbols=" +
                baseCurrency +
                "&base=" +
                currency;
            string jsonExchangeRate = await GetJsonResponseAsync(uri);
            JObject jObject = JObject.Parse(jsonExchangeRate);
            var rates = jObject["rates"];
            var result = new List<ExchangeRate>();
            foreach (var token in rates)
            {
                JProperty jProperty = token.ToObject<JProperty>();
                result.Add(new ExchangeRate
                {
                    BaseCurrency = baseCurrency,
                    Currency = currency,
                    Date = DateTime.ParseExact(jProperty.Name, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                    Rate = Decimal.Parse(jProperty.First.First.First.ToString())
                });
            }
                
            return result;
        }

        private static async Task<string> GetJsonResponseAsync(string uri)
        {
            return await client.GetStringAsync(uri);
        }
    }
}