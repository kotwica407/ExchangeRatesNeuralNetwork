using System;

namespace ExchangeRateApiClient
{
    public class ExchangeRate
    {
        public string BaseCurrency { get; set; }
        public string Currency { get; set; }
        public decimal Rate { get; set; }
        public DateTime Date { get; set; }
    }
}
