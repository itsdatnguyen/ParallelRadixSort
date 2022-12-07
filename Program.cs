using System.Diagnostics;

namespace ParallelRadixSort
{
    public class Program
    {
        static void Main(string[] args)
        {
            RunTestOn("./names.txt");
            RunTestOn("./last-names.txt");
            RunTestOn("./last-names-duplicated.txt");
            RunTestOn("./international-names.txt");
        }

        private static void RunTestOn(string filePath)
        {
            Console.WriteLine($"Running sorting for {filePath}: {File.ReadAllLines(filePath).Length} names)");
            RunDefaultSequentialSorting(filePath);
            RunSequentialSpanRadixSort(filePath);
            RunSequentialArrayRadixSort(filePath);
            RunParallelArrayRadixSort(filePath);
            Console.WriteLine();
        }

        private static void RunDefaultSequentialSorting(string filePath)
        {
            NameSort.Program.RunSorting(filePath);
        }

        private static void RunSequentialSpanRadixSort(string filePath)
        {
            var files = File.ReadAllLines(filePath);
            var longestNameLength = files.Max(f => f.Length);
            var radixSorter = new SequentialSpanRadixSorter(longestNameLength);

            for (var i = 0; i < files.Length; i++)
            {
                files[i] = files[i].PadRight(longestNameLength);
            }

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(files);
            Console.WriteLine($"Sequential Span Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");

            Directory.CreateDirectory("./outputspan/");
            File.WriteAllText("./outputspan/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
        }

        private static void RunSequentialArrayRadixSort(string filePath)
        {
            var files = File.ReadAllLines(filePath);
            var longestNameLength = files.Max(f => f.Length);
            var radixSorter = new SequentialArrayRadixSorter(longestNameLength);

            for (var i = 0; i < files.Length; i++)
            {
                files[i] = files[i].PadRight(longestNameLength);
            }

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(files);
            Console.WriteLine($"Sequential Array Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");

            Directory.CreateDirectory("./outputarray/");
            File.WriteAllText("./outputarray/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
        }

        private static void RunParallelArrayRadixSort(string filePath)
        {
            var files = File.ReadAllLines(filePath);
            var longestNameLength = files.Max(f => f.Length);
            var radixSorter = new ParallelArrayRadixSorter(longestNameLength);

            for (var i = 0; i < files.Length; i++)
            {
                files[i] = files[i].PadRight(longestNameLength);
            }

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(files);
            Console.WriteLine($"Parallel Array Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");

            Directory.CreateDirectory("./outputparallel/");
            File.WriteAllText("./outputparallel/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
        }
    }
}