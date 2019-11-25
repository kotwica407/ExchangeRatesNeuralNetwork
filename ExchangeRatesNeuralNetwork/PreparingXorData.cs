using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiaNet.Data;

namespace ExchangeRatesNeuralNetwork
{
    public static class PreparingXorData
    {
        public static (DataFrame2D, DataFrame2D) PrepareDataSet()
        {
            DataFrame2D x = new DataFrame2D(2);
            x.Load(new float[] { 0, 0, 0, 1, 1, 0, 1, 1 });
            DataFrame2D y = new DataFrame2D(1);
            y.Load(new float[] { 0, 1, 1, 0 });

            return (x, y);
        }
    }
}