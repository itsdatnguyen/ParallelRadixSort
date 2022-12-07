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
            var data = NameSort.Program.RunSorting(filePath);

            Directory.CreateDirectory("./outputnormal/");
            File.WriteAllText("./outputnormal/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
        }

        private static void RunSequentialSpanRadixSort(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var longestNameLength = lines.Max(f => f.Length);
            var radixSorter = new SequentialSpanRadixSorter(longestNameLength);
            SwapLastAndFirstNames(lines);

            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].PadRight(longestNameLength);
            }

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(lines);
            Console.WriteLine($"Sequential Span Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");

            SwapLastAndFirstNames(lines);
            Directory.CreateDirectory("./outputspan/");
            File.WriteAllText("./outputspan/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
        }

        private static void RunSequentialArrayRadixSort(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var longestNameLength = lines.Max(f => f.Length);
            var radixSorter = new SequentialArrayRadixSorter(longestNameLength);
            SwapLastAndFirstNames(lines);

            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].PadRight(longestNameLength);
            }

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(lines);
            Console.WriteLine($"Sequential Array Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");

            SwapLastAndFirstNames(lines);
            Directory.CreateDirectory("./outputarray/");
            File.WriteAllText("./outputarray/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
        }

        private static void RunParallelArrayRadixSort(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var longestNameLength = lines.Max(f => f.Length);
            var radixSorter = new ParallelArrayRadixSorter(longestNameLength);
            SwapLastAndFirstNames(lines);

            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].PadRight(longestNameLength);
            }

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(lines);
            Console.WriteLine($"Parallel Array Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms");

            SwapLastAndFirstNames(lines);
            Directory.CreateDirectory("./outputparallel/");
            File.WriteAllText("./outputparallel/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
        }

        /// <summary>
        /// Swap last and first names for ease of use by sorting by last name
        /// </summary>
        private static void SwapLastAndFirstNames(string[] array)
        {
            if (array[0].Trim().Split(' ').Length > 1)
            {
                for (var i = 0; i < array.Length; i++)
                {
                    var names = array[i].Trim().Split(" ");
                    array[i] = names[1] + " " + names[0];
                }
            }
        }
    }
}