using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.FSharp;
using Microsoft.FSharp.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using static test1.Program;

namespace test1
{
     static class Program
    {

        static void Main(string[] args)
        {

            var windowsize = 4;
            var client = new MongoClient("mongodb://10.8.0.1:27017/testdb");
            var database = client.GetDatabase("testdb");
            var _pressures = database.GetCollection<Pressures>("Pressure");
            IList<Pressures> Read() => _pressures.Find(pres => true).ToList();
            List<double> pressure = new List<double> { };
            foreach (Pressures item in Read())
            {
                pressure.Add(Convert.ToDouble(item.Pressure));
            }
            var windowed = (SeqModule.Windowed(4, pressure));
            List<double[]> lista = new List<double[]> { };
            foreach (var item in windowed)
            {
                lista.Add(item);
            }
            double[][] trainData = null;
            double[][] testData = null;
            var lis = lista.ToArray();
            ShowMatrix(lis, 6, 1, true);
            Sampledata(out trainData, out testData, lis);

            const int numInput = 4;
            const int numHidden = 7;
            const int numOutput = 1;
            NeuralNetwork nn = new NeuralNetwork(numInput, numHidden, numOutput);
            nn.InitializeWeights();

            int maxEpochs = 2000;
            double learnRate = 0.05;
            double momentum = 0.01;
            double weightDecay = 0.0001;
            
            Console.WriteLine("Setting maxEpochs = 2000, learnRate = 0.05, momentum = 0.01, weightDecay = 0.0001");
            Console.WriteLine("Training has hard-coded mean squared error < 0.020 stopping condition");

            Console.WriteLine("\nBeginning training using incremental back-propagation\n");
            nn.Train(trainData, maxEpochs, learnRate, momentum, weightDecay);
            Console.WriteLine("Training complete");

            Console.WriteLine(windowed.ToString());





        }

        private static void Sampledata(out double[][] trainData, out double[][] testData, double[][] lis)
        {
            // split allData into 80% trainData and 20% testData
            Random rnd = new Random(0);
            int totRows = lis.Length;
            int numCols = lis[0].Length;

            int trainRows = (int)(totRows * 0.80); // hard-coded 80-20 split
            int testRows = totRows - trainRows;

            trainData = new double[trainRows][];
            testData = new double[testRows][];

            int[] sequence = new int[totRows]; // create a random sequence of indexes
            for (int i = 0; i < sequence.Length; ++i)
                sequence[i] = i;

            for (int i = 0; i < sequence.Length; ++i)
            {
                int r = rnd.Next(i, sequence.Length);
                int tmp = sequence[r];
                sequence[r] = sequence[i];
                sequence[i] = tmp;
            }

            int si = 0; // index into sequence[]
            int j = 0; // index into trainData or testData

            for (; si < trainRows; ++si) // first rows to train data
            {
                trainData[j] = new double[numCols];
                int idx = sequence[si];
                Array.Copy(lis[idx], trainData[j], numCols);
                ++j;
            }

            j = 0; // reset to start of test data
            for (; si < totRows; ++si) // remainder to test data
            {
                testData[j] = new double[numCols];
                int idx = sequence[si];
                Array.Copy(lis[idx], testData[j], numCols);
                ++j;
            }
        }

        static void ShowMatrix(double [][]matrix, int numRows, int decimals, bool newLine)
        {
            for (int i = 0; i < numRows; ++i)
            {
                Console.Write(i.ToString().PadLeft(3) + ": ");
                for (int j = 0; j < matrix[i].Length; ++j)
                {
                    if (matrix[i][j] >= 0.0) Console.Write(" "); else Console.Write("-");
                    Console.Write(Math.Abs(matrix[i][j]).ToString("F" + decimals) + " ");
                }
                Console.WriteLine("");
            }
            if (newLine == true) Console.WriteLine("");
        }

        public static IEnumerable<T[]> Windowed<T>(this IEnumerable<T> list, int windowSize)
        {
            //Checks elided
            var arr = new T[windowSize];
            int r = windowSize - 1, i = 0;
            using (var e = list.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    arr[i] = e.Current;
                    i = (i + 1) % windowSize;
                    if (r == 0)
                        yield return ArrayInit<T>(windowSize, j => arr[(i + j) % windowSize]);
                    else
                        r = r - 1;
                }
            }
        }
        public static T[] ArrayInit<T>(int size, Func<int, T> func)
        {
            var output = new T[size];
            for (var i = 0; i < size; i++) output[i] = func(i);
            return output;
        }
    }
        
       public class Pressures
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public ObjectId _id { get; set; }
            public string station { get; set; }
            public string Pressure { get; set; }
            public DateTime observationDate { get; set; }
            public DateTime CreatedOn
            {
                get { return _id.CreationTime; }
            }
        }
        
        

    
}

