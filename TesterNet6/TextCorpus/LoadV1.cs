﻿using DBreeze.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TesterNet6.TextCorpus.Clustering;

namespace TesterNet6.TextCorpus
{
    internal static  class Load
    {
        static string tblLoadV1 = "LoadV1";

        public static void SelectV1()
        {
            DBreeze.Utils.FastRandom frnd = new DBreeze.Utils.FastRandom();
            int vectorSize = 1536; //OpenAI 1536
            double[] queryVector=new double[vectorSize];
            for (int pp = 0; pp < vectorSize; pp++)
                queryVector[pp] = frnd.NextDouble();


            using (var tran = Program.DBEngine.GetTransaction())
            {
                tran.ValuesLazyLoadingIsOn = false; //to read key already with value

                var res = tran.VectorsSearchSimilar(tblLoadV1, queryVector, 3);

                foreach (var el in res)
                {
                    //var rowDoc = tran.Select<byte[], string>(tblDotblLoadV1csFurniture, 2.ToIndex(el));
                   // var dbFurniture = JsonSerializer.Deserialize<FurnitureItem>(rowDoc.Value);
                    Console.WriteLine($"{el.ToHexFromByteArray()}");
                   // Console.WriteLine($"\tDescription: {dbFurniture.Description}");

                }
            }
        }

        static double[] GetRandomEmbedding()
        {
            int size = 1536;
            Random random = new Random();
            double[] randomArray = new double[size];

            for (int i = 0; i < size; i++)
            {
                randomArray[i] = random.NextDouble();
            }
            return randomArray;
            // Now you can use the randomArray
        }

        static int idCnt = 0;
        static string tableVector = "tableVector"; //Vector Table
        public static void Insert01()
        {
            //Debug.Log("Store_String_Vectors Start " + Time.time);
            Console.WriteLine("Store_String_Vectors Start ");

            //-such format will be inserted into VectorTable, Key is exernal documentID, value is vector itself
            Dictionary<byte[], double[]> vectorsToInsert = new Dictionary<byte[], double[]>();
            using (var tran = Program.DBEngine.GetTransaction())
            {
                //-sync of Doctable and vector table for searching docs
                tran.SynchronizeTables(tableVector);

                //Loop and insert 10 times the same data
                for (int i = 0; i < 50; i++)
                {
                    idCnt++;

                    vectorsToInsert.Add(idCnt.To_4_bytes_array_BigEndian(), GetRandomEmbedding());
                }

                //-storing documents as vectors (with/without deferred indexing) 
                if (vectorsToInsert.Count > 0)
                {
                    //-in case of big quantity of vectors, use deferredIndexing: true (to run computation in the background)
                    tran.VectorsInsert(tableVector, vectorsToInsert, deferredIndexing: false);
                }

                tran.Commit();

                //Debug.Log("Vector Count == " + tran.Count(tableVector));
            }

            Console.WriteLine("Store_String_Vectors End ");
        }

        public static void LoadV1()
        {
            //for(int jhz=0;jhz<10;jhz++)
            //    SelectV1();

            //return;
            DBreeze.Utils.FastRandom frnd = new DBreeze.Utils.FastRandom();
            var rnd = new Random();
            int batchSize = 1000; //100 (batchSize) documents per round
            int vectorSize = 1536; //OpenAI 1536
            double[][] bt = new double[batchSize][];
            //for (int i = 0; i < 100; i++)
            //{
            //    bt[i]=new byte[500];
            //    rnd.NextBytes(bt[i]);
            //}

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch sw1 = new System.Diagnostics.Stopwatch();
            sw.Start();
            DateTime now = DateTime.Now;
            for (int j = 0; j < 100; j++)//100 times we insert batch
            {
                Console.Write($"{(j * batchSize + batchSize)} - "); //quantity of documents inside

                for (int i = 0; i < batchSize; i++)//inserting 100 vectors of size 500 double
                {

                    bt[i] = new double[vectorSize];
                    for (int pp = 0; pp < vectorSize; pp++)
                        bt[i][pp] = frnd.NextDouble();
                    //rnd.NextBytes(bt[i]);
                    //rnd.NextBytes(bt[i]);
                }
                sw1.Reset();
                sw1.Start();
                using (var tran = Program.DBEngine.GetTransaction())
                {
                    var x = bt.Select((k, v) =>
                    new KeyValuePair<byte[], double[]>(
                        (v + j * batchSize).ToBytes(),
                        k.Select(Convert.ToDouble).ToArray()))
                    .ToDictionary(k => k.Key, v => v.Value);

                    //world.AddVectors(x);
                    tran.VectorsInsert(tblLoadV1, x);

                    tran.Commit();
                }
                sw1.Stop();
                Console.WriteLine($"roundMS: {sw1.ElapsedMilliseconds}");
            }
            sw.Stop();
            Console.WriteLine($"MS: {sw.ElapsedMilliseconds}; - {(DateTime.Now - now).TotalMilliseconds}");
        }



        ///// <summary>
        ///// 
        ///// </summary>
        //public static void LoadV2()
        //{
        //    var rnd=new Random();
        //    int batchSize = 1000; //100 (batchSize) documents per round
        //    byte[][] bt = new byte[batchSize][];
        //    //for (int i = 0; i < 100; i++)
        //    //{
        //    //    bt[i]=new byte[500];
        //    //    rnd.NextBytes(bt[i]);
        //    //}

        //    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //    System.Diagnostics.Stopwatch sw1 = new System.Diagnostics.Stopwatch();
        //    sw.Start();
        //    DateTime now = DateTime.Now;
        //    for (int j = 0; j < 100; j++)//100 times we insert batch
        //    {
        //        Console.Write($"{(j* batchSize + batchSize)} - "); //quantity of documents inside
                
        //        for (int i = 0; i < batchSize; i++)//inserting 100 vectors of size 500 double
        //        {
        //            bt[i] = new byte[500];
        //            //rnd.NextBytes(bt[i]);
        //            rnd.NextBytes(bt[i]);
        //        }
        //        sw1.Reset();
        //        sw1.Start();
        //        using (var tran = Program.DBEngine.GetTransaction())
        //        {
        //            var x = bt.Select((k, v) => 
        //            new KeyValuePair<byte[], double[]>(
        //                (v+ j*batchSize).ToBytes(), 
        //                k.Select(Convert.ToDouble).ToArray()))
        //            .ToDictionary(k=>k.Key,v=>v.Value);

        //            tran.VectorsInsert(tblLoadV1, x);

        //            tran.Commit();
        //        }
        //        sw1.Stop();
        //        Console.WriteLine($"roundMS: {sw1.ElapsedMilliseconds}");
        //    }
        //    sw.Stop();
        //    Console.WriteLine($"MS: {sw.ElapsedMilliseconds}; - {(DateTime.Now - now).TotalMilliseconds}");
                

                



        //}





    }
}
