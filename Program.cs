using System.Diagnostics;

namespace ParallelRadixSort
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var filePath = "./names.txt";

            Console.WriteLine($"Running sorting for {filePath}: {File.ReadAllLines(filePath).Length} names)");
            RunDefaultSequentialSorting(filePath);
            RunSequentialHeapSort(filePath);
            Console.WriteLine();

            // additionally, I have provided more test data for running sorting on 

            filePath = "./last-names.txt";
            Console.WriteLine($"Running sorting for {filePath}: {File.ReadAllLines(filePath).Length} names)");
            RunDefaultSequentialSorting(filePath);
            RunSequentialHeapSort(filePath);
            Console.WriteLine();

            filePath = "./last-names-duplicated.txt";
            Console.WriteLine($"Running sorting for {filePath}: {File.ReadAllLines(filePath).Length} names)");
            RunDefaultSequentialSorting(filePath);
            RunSequentialHeapSort(filePath);
            Console.WriteLine();


            Console.ReadLine();
        }

        private static void RunDefaultSequentialSorting(string filePath)
        {
            NameSort.Program.RunSorting(filePath);
        }

        private static void RunSequentialHeapSort(string filePath)
        {
            var files = File.ReadAllLines(filePath);
            var stringComparer = StringComparer.OrdinalIgnoreCase;

            var longestNameLength = files.Max(f => f.Length);
            var radixSorter = new RadixSorter(longestNameLength);
            for (var i = 0; i < files.Length; i++)
            {
                files[i] = files[i].PadRight(longestNameLength);
            }

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(files);
            Console.WriteLine($"Sequential Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");

            Directory.CreateDirectory("./outputseq/");
            File.WriteAllText("./outputseq/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
            //foreach (var data in sortedData.GetList)
            //{
            //    Console.WriteLine(data);
            //}
        }

    }
}