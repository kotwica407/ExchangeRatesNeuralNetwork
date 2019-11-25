using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiaNet.Data;
using Deedle;
using ExchangeRateApiClient;
using ExchangeRatesNeuralNetwork.Model;

namespace ExchangeRatesNeuralNetwork
{
    public static class PreparingExchangeRateData
    {
        public static DataFrameIter LoadTrain()
        {
            string projectDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
            List<float> datas = new List<float>();
            using (StreamReader readFile = new StreamReader(projectDirectory + "/Data/USDRateTrain.csv"))
            {
                string line;
                string[] row;
                

                while ((line = readFile.ReadLine()) != null)
                {
                    row = line.Split('\t');
                    for(int i = 1; i < row.Length; i++)
                        datas.Add(float.Parse(row[i]));
                }
            }
            //var frame = Frame.ReadCsv(projectDirectory + "/Data/USDRateTrain.csv");
            //frame.DropColumn("Date");

            //var data = frame.ToArray2D<float>().Cast<float>().ToArray();
            DataFrame2D df = new DataFrame2D(8);
            df.Load(datas.ToArray());

            var x = df[0, 6];
            var y = df[7];

            return new DataFrameIter(x, y);
        }

        public static DataFrame LoadTest()
        {
            //string projectDirectory = System.IO.Path.GetFullPath(@"..\..\..\");

            //var frame = Frame.ReadCsv(projectDirectory + "/Data/USDRateTest.csv");
            //frame.DropColumn("Date");
            string projectDirectory = System.IO.Path.GetFullPath(@"..\..\..\");
            List<float> datas = new List<float>();
            using (StreamReader readFile = new StreamReader(projectDirectory + "/Data/USDRateTest.csv"))
            {
                string line;
                string[] row;


                while ((line = readFile.ReadLine()) != null)
                {
                    row = line.Split('\t');
                    for (int i = 1; i < row.Length; i++)
                        datas.Add(float.Parse(row[i]));
                }
            }
            //var data = frame.ToArray2D<float>().Cast<float>().ToArray();
            DataFrame2D df = new DataFrame2D(8);
            df.Load(datas.ToArray());
            return df;
        }

        private static Frame<int, string> PreProcesData(Frame<int, string> frame, bool isTest = false)
        {
            // Drop some colmuns which will not help in prediction
            frame.DropColumn("Date");

            return frame;
        }

        public static void SaveToFile(IEnumerable<InputOutputData> inputOutputDatas, string path)
        {
            var builder = new StringBuilder();
            //builder.AppendLine("Date\tRate\tMA5\tMA10\tMA15\tMA20\tMA50\tMACD\tNextDayRate");
            foreach (var inputOutputData in inputOutputDatas)
            {
                builder.AppendLine(inputOutputData.ExchangeRate.Date + "\t" +
                                   inputOutputData.ExchangeRate.Rate + "\t" +
                                   inputOutputData.Ma5 + "\t" +
                                   inputOutputData.Ma10 + "\t" +
                                   inputOutputData.Ma15 + "\t" +
                                   inputOutputData.Ma20 + "\t" +
                                   inputOutputData.Ma50 + "\t" +
                                   inputOutputData.Macd + "\t" +
                                   inputOutputData.NextDayRate);
            }

            using (StreamWriter outputFile = new StreamWriter(path))
                outputFile.Write(builder);
        }

    }
}