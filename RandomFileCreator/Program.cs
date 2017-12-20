using System;
using System.Configuration;
using System.IO;

namespace RandomFileCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            var outputFilePath = ConfigurationManager.AppSettings["outputFilePath"];
            if (outputFilePath == null) throw new ArgumentNullException(nameof(outputFilePath));
            var configLineQuan = ConfigurationManager.AppSettings["linesQuantity"];
            if (configLineQuan == null) throw new ArgumentNullException(nameof(configLineQuan));

            var lineQuan = int.Parse(configLineQuan);

            var randomTextContent = new RandomTextContent(lineQuan, outputFilePath);

            Console.WriteLine("It has been processing...");
            randomTextContent.GenerateFile();
            Console.WriteLine($"{lineQuan} lines has been genereted into file {outputFilePath} ...");
            Console.Read();
        }

        private class RandomTextContent
        {
            private const int MaxNumber = 1000;
            private const char LineDelimeter = '.';
            private string OutputFilePath { get; }
            private int LinesQuantity { get; }

            private readonly string[] _article = {"The", "A", "One", "Some", "Any"};
            private readonly string[] _noun = {"boy", "girl", "dog", "town", "car"};
            private readonly string[] _verb = {"drove", "jumped", "ran", "walked", "skipped"};
            private readonly string[] _preposition = {"to", "from", "over", "under", "on"};

            public RandomTextContent(int linesQuantity, string outputFilePath)
            {
                LinesQuantity = linesQuantity;
                OutputFilePath = outputFilePath;
            }

            public void GenerateFile()
            {
                using (var resultWriter = new StreamWriter(OutputFilePath, false))
                {
                    for (var i = 0; i < LinesQuantity; i++)
                    {
                        resultWriter.WriteLine(GetRandomLine(i));
                    }
                }
            }

            private string GetRandomLine(int seed)
            {
                var rndarticle = new Random(seed);
                var rndnoun = new Random(seed);
                var rndverb = new Random(seed);
                var rndpreposition = new Random(seed);
                var rndNumber = new Random(seed);

                var randomarticle = rndarticle.Next(0, _article.Length);
                var randomnoun = rndnoun.Next(0, _noun.Length);
                var randomverb = rndverb.Next(0, _verb.Length);
                var randompreposition = rndpreposition.Next(0, _preposition.Length);
                var rndNumberString = rndNumber.Next(1, MaxNumber).ToString();

                return
                    $"{rndNumberString}{LineDelimeter}{_article[randomarticle]} {_noun[randomnoun]} {_verb[randomverb]} {_preposition[randompreposition]}";
            }
        }
    }
}