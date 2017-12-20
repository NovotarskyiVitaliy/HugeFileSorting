using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sinbadsoft.Lib.Collections;

namespace OrderFile
{
    internal static class Program
    {
        static void Main()
        {
            var configPieceSize = ConfigurationManager.AppSettings["pieceSize"];
            if (configPieceSize == null) throw new ArgumentNullException(nameof(configPieceSize));
            var pieceSize = int.Parse(configPieceSize);
            var inputFilePath = ConfigurationManager.AppSettings["inputFilePath"];
            if (inputFilePath == null) throw new ArgumentNullException(nameof(inputFilePath));
            var outputFilePath = ConfigurationManager.AppSettings["outputFilePath"];
            if (outputFilePath == null) throw new ArgumentNullException(nameof(outputFilePath));
            Console.WriteLine("It has being processing...");

            var sw = Stopwatch.StartNew();

            var hugeFileSorting = new HugeFileSorting(pieceSize, inputFilePath, outputFilePath);
            hugeFileSorting.MergePieces(hugeFileSorting.SplitIntoOrderedPiece());

            sw.Stop();

            Console.WriteLine("Time taken: {0} sec", sw.Elapsed.TotalSeconds);
            Console.Read();
        }
    }

    public class HugeFileSorting
    {
        private const char LineDelimeter = '.';
        private int PieceSize { get; }
        private string InputFilePath { get; }
        private string OutputFilePath { get; }

        public HugeFileSorting(int pieceSize, string inputFilePath, string outputFilePath)
        {
            PieceSize = pieceSize;
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
        }

        public IEnumerable<string> SplitIntoOrderedPiece()
        {
            var pieceConteiner = new List<string>();
            var size = 0L;

            using (var reader = new StreamReader(InputFilePath))
                for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    if (size + line.Length + 2 >= PieceSize)
                    {
                        size = 0L;
                        yield return GetPieceConteiner(pieceConteiner);
                    }

                    size += line.Length + 2;
                    pieceConteiner.Add(line);
                }

            if (pieceConteiner.Any())
            {
                yield return GetPieceConteiner(pieceConteiner);
            }
        }

        private static string GetPieceConteiner(List<string> pieceConteiner)
        {
            var pieceFilePath = Path.GetTempFileName();

            pieceConteiner.Sort((x, y) => -CompareLines(x, y, -1));

            File.WriteAllLines(pieceFilePath, pieceConteiner);
            pieceConteiner.Clear();

            Console.WriteLine($"{Path.GetTempFileName()} is splited");
            return pieceFilePath;
        }

        public void MergePieces(IEnumerable<string> listFilePathesPiece)
        {
            var pieceStreamReader = listFilePathesPiece
                .Select(path => new StreamReader(path))
                .Where(chunkReader => !chunkReader.EndOfStream)
                .ToList();

            var queue = new PriorityQueue<string, TextReader>((x, y) => -CompareLines(x, y));

            Console.WriteLine("It has been merging. A bit time left...");

            pieceStreamReader.ForEach(chunkReader => queue.Enqueue(chunkReader.ReadLine(), chunkReader));

            using (var resultWriter = new StreamWriter(OutputFilePath, false))
                while (queue.Count > 0)
                {
                    var topItem = queue.Dequeue();

                    var pieceReader = topItem.Value;
                    var resultLine = topItem.Key;

                    resultWriter.WriteLine(resultLine);

                    var nextLine = pieceReader.ReadLine();

                    if (nextLine != null)
                    {
                        queue.Enqueue(nextLine, pieceReader);
                    }
                    else
                    {
                        pieceReader.Dispose();
                    }
                }
        }

        private static int CompareLines(string strA, string strB, int compareFlag = 1)
        {
            var arrA = strA.Split(LineDelimeter);
            var arrB = strB.Split(LineDelimeter);

            var strcomp = string.CompareOrdinal(arrA[1], arrB[1]);

            if (strcomp != 0)
            {
                return strcomp * compareFlag;
            }

            int nA, nB;
            try
            {
                nA = Convert.ToInt32(arrA[0]);
                nB = Convert.ToInt32(arrB[0]);
            }
            catch
            {
                Console.WriteLine("Input file is corrupt");
                throw;
            }

            if (compareFlag == 1)
            {
                return nA == nB ? 0 : (nA > nB ? 1 : -1);
            }
            return nA == nB ? 0 : (nA > nB ? -1 : 1);
        }
    }
}