using ExchangeRateApiClient;

namespace ExchangeRatesNeuralNetwork.Model
{
    public class InputOutputData
    {
        public ExchangeRate ExchangeRate { get; set; }
        public decimal Ma5 { get; set; }
        public decimal Ma10 { get; set; }
        public decimal Ma15 { get; set; }
        public decimal Ma20 { get; set; }
        public decimal Ma50 { get; set; }
        public decimal Macd { get; set; }
        public decimal NextDayRate { get; set; }
    }
}