using SiaNet.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Deedle.Internal;
using SiaNet;
using SiaNet.Layers;
using SiaNet.Regularizers;
using SiaNet.Engine;
using SiaNet.Backend.ArrayFire;
using SiaNet.Events;
using ExchangeRateApiClient;
using ExchangeRatesNeuralNetwork.Model;
using ServiceStack.Text;
using Newtonsoft.Json;
using SiaNet.Optimizers;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace ExchangeRatesNeuralNetwork
{
    class Program
    {
        static void Main(string[] args)
        {
            SaveRateToFileTrainData("USD");
            SaveRateToFileTestData("USD");
            Global.UseEngine(SiaNet.Backend.ArrayFire.SiaNetBackend.Instance, DeviceType.CUDA, true);


            var dataset = PreparingExchangeRateData.LoadTrain();
            var test = PreparingExchangeRateData.LoadTest();

            var (train, val) = dataset.Split(0.25);
            var model = new Sequential();
            model.EpochEnd += Model_EpochEnd;
            model.TrainingEnd += Model_TrainEnd;
            model.Add(new Dense(60, ActType.Sigmoid));
            model.Add(new Dense(60, ActType.Sigmoid));
            model.Add(new Dense(1, ActType.Linear));

            var optimizer = new SGD(0.01F, 0.2F);
            //Compile with Optimizer, Loss and Metric
            model.Compile(optimizer, LossType.MeanSquaredError, MetricType.Accuracy);
            // Train for 5000 epoch with batch size of 32
            model.Train(train, epochs: 10000, batchSize: 32, val);
            string projectDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
            model.SaveModel(projectDirectory + $"/NeuralNetwork/network1.json");
            using (StreamWriter sw = new StreamWriter(projectDirectory + $"/NeuralNetwork/network2.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, model);
            }
            //Create prediction data to evaluate
            //DataFrame2D predX = new DataFrame2D(2);
            //predX.Load(0, 0, 0, 1, 1, 0, 1, 1); //Result should be 0, 1, 1, 0
            var rawPred = model.Predict(test);
            rawPred.Print();
            Console.ReadLine();
        }

        private static void Model_EpochEnd(object sender, EpochEndEventArgs e)
        {
            Console.WriteLine("Epoch: {0}, Loss: {1}, Acc: {2}, Val_Loss: {3}, Val_Acc: {4} Duration: {5}ms", e.Epoch,
                e.Loss, e.Metric, e.ValidationLoss, e.ValidationMetric, e.Duration);
        }

        private static void Model_TrainEnd(object sender, TrainingEndEventArgs e)
        {
            Console.WriteLine("Training is finished\n========================================\n");
        }
        private static void SaveRateToFileTrainData(string currency)
        {
            var rates = Utils.ReadExchangeRatesForPeriod("PLN", currency, DateTime.Today.AddYears(-5),
                DateTime.Today.AddDays(-365)).Result.OrderBy(x => x.Date).ToList();
            var ma5 = TechnicalAnalysis.MovingAverage(rates, 5).ToList();
            var ma10 = TechnicalAnalysis.MovingAverage(rates, 10).ToList();
            var ma15 = TechnicalAnalysis.MovingAverage(rates, 15).ToList();
            var ma20 = TechnicalAnalysis.MovingAverage(rates, 20).ToList();
            var ma50 = TechnicalAnalysis.MovingAverage(rates, 50).ToList();
            var macd = TechnicalAnalysis.Macd(rates).ToList();

            var nextDayRate = new List<ExchangeRate>();
            for(int i = 0; i < rates.Count - 1; i++)
                nextDayRate.Add(rates.ElementAt(i+1));

            rates = rates.Skip(50).ToList();
            ma5 = ma5.Skip(50).ToList();
            ma10 = ma10.Skip(50).ToList();
            ma15 = ma15.Skip(50).ToList();
            ma20 = ma20.Skip(50).ToList();
            ma50 = ma50.Skip(50).ToList();
            macd = macd.Skip(50).ToList();
            nextDayRate = nextDayRate.Skip(50).ToList();

            List<InputOutputData> inputOutputDatas = new List<InputOutputData>();

            for (int i = 0; i < rates.Count - 1; i++)
            {
                inputOutputDatas.Add(new InputOutputData
                {
                    ExchangeRate = rates.ElementAt(i),
                    Ma5 = ma5.ElementAt(i).Rate,
                    Ma10 = ma10.ElementAt(i).Rate,
                    Ma15 = ma15.ElementAt(i).Rate,
                    Ma20 = ma20.ElementAt(i).Rate,
                    Ma50 = ma50.ElementAt(i).Rate,
                    Macd = macd.ElementAt(i).Rate,
                    NextDayRate = nextDayRate.ElementAt(i).Rate
                });
            }

            string projectDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
            PreparingExchangeRateData.SaveToFile(inputOutputDatas, projectDirectory + $"/Data/{inputOutputDatas.First().ExchangeRate.Currency}RateTrain.csv");
        }

        private static void SaveRateToFileTestData(string currency)
        {
            var rates = Utils.ReadExchangeRatesForPeriod("PLN", currency, DateTime.Today.AddDays(-364),
                DateTime.Today.AddDays(-1)).Result.OrderBy(x => x.Date).ToList();
            var ma5 = TechnicalAnalysis.MovingAverage(rates, 5).ToList();
            var ma10 = TechnicalAnalysis.MovingAverage(rates, 10).ToList();
            var ma15 = TechnicalAnalysis.MovingAverage(rates, 15).ToList();
            var ma20 = TechnicalAnalysis.MovingAverage(rates, 20).ToList();
            var ma50 = TechnicalAnalysis.MovingAverage(rates, 50).ToList();
            var macd = TechnicalAnalysis.Macd(rates).ToList();

            var nextDayRate = new List<ExchangeRate>();
            for (int i = 0; i < rates.Count - 1; i++)
                nextDayRate.Add(rates.ElementAt(i + 1));

            rates = rates.Skip(50).ToList();
            ma5 = ma5.Skip(50).ToList();
            ma10 = ma10.Skip(50).ToList();
            ma15 = ma15.Skip(50).ToList();
            ma20 = ma20.Skip(50).ToList();
            ma50 = ma50.Skip(50).ToList();
            macd = macd.Skip(50).ToList();
            nextDayRate = nextDayRate.Skip(50).ToList();

            List<InputOutputData> inputOutputDatas = new List<InputOutputData>();

            for (int i = 0; i < rates.Count - 1; i++)
            {
                inputOutputDatas.Add(new InputOutputData
                {
                    ExchangeRate = rates.ElementAt(i),
                    Ma5 = ma5.ElementAt(i).Rate,
                    Ma10 = ma10.ElementAt(i).Rate,
                    Ma15 = ma15.ElementAt(i).Rate,
                    Ma20 = ma20.ElementAt(i).Rate,
                    Ma50 = ma50.ElementAt(i).Rate,
                    Macd = macd.ElementAt(i).Rate,
                    NextDayRate = nextDayRate.ElementAt(i).Rate
                });
            }

            string projectDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
            PreparingExchangeRateData.SaveToFile(inputOutputDatas, projectDirectory + $"/Data/{inputOutputDatas.First().ExchangeRate.Currency}RateTest.csv");
        }
    }
}
