using System.Diagnostics;

namespace ParallelRadixSort
{
    public class Program
    {
        /// <summary>
        /// This project contains 3 implementations of sortings
        /// </summary>
        /// 
        /// <example>
        /// Sample generated output:
        /// Ran on a computer with a Ryzen 5800X3D
        /// 
        /// Running sorting for ./names.txt: 10000 names)
        /// Default LINQ Sort Elapsed Time: 18ms | 183196 ticks
        /// Sequential Span Radix Elapsed Time: 13ms | 130819 ticks
        /// Sequential Array Radix Elapsed Time: 3ms | 39259 ticks
        /// Parallel Array Radix Elapsed Time: 23ms | 239624 ticks
        /// 
        /// Running sorting for ./names.txt: 10000 names)
        /// Default LINQ Sort Elapsed Time: 8ms | 86918 ticks
        /// Sequential Span Radix Elapsed Time: 11ms | 117960 ticks
        /// Sequential Array Radix Elapsed Time: 2ms | 22573 ticks
        /// Parallel Array Radix Elapsed Time: 1ms | 19772 ticks
        /// 
        /// Running sorting for ./last-names.txt: 88799 names)
        /// Default LINQ Sort Elapsed Time: 147ms | 1472124 ticks
        /// Sequential Span Radix Elapsed Time: 76ms | 762798 ticks
        /// Sequential Array Radix Elapsed Time: 19ms | 194986 ticks
        /// Parallel Array Radix Elapsed Time: 5ms | 51003 ticks
        /// 
        /// Running sorting for ./last-names.txt: 88799 names)
        /// Default LINQ Sort Elapsed Time: 148ms | 1488598 ticks
        /// Sequential Span Radix Elapsed Time: 76ms | 762126 ticks
        /// Sequential Array Radix Elapsed Time: 18ms | 188101 ticks
        /// Parallel Array Radix Elapsed Time: 4ms | 48538 ticks
        /// 
        /// Running sorting for ./last-names-duplicated.txt: 887990 names)
        /// Default LINQ Sort Elapsed Time: 1716ms | 17168225 ticks
        /// Sequential Span Radix Elapsed Time: 350ms | 3509172 ticks
        /// Sequential Array Radix Elapsed Time: 228ms | 2289632 ticks
        /// Parallel Array Radix Elapsed Time: 55ms | 554862 ticks
        /// 
        /// Running sorting for ./last-names-duplicated.txt: 887990 names)
        /// Default LINQ Sort Elapsed Time: 1692ms | 16928478 ticks
        /// Sequential Span Radix Elapsed Time: 342ms | 3424093 ticks
        /// Sequential Array Radix Elapsed Time: 230ms | 2305974 ticks
        /// Parallel Array Radix Elapsed Time: 52ms | 522922 ticks
        /// 
        /// Running sorting for ./international-names.txt: 19948 names)
        /// Default LINQ Sort Elapsed Time: 25ms | 254314 ticks
        /// Sequential Span Radix Elapsed Time: 18ms | 189775 ticks
        /// Sequential Array Radix Elapsed Time: 4ms | 45985 ticks
        /// Parallel Array Radix Elapsed Time: 1ms | 17601 ticks
        /// 
        /// Running sorting for ./international-names.txt: 19948 names)
        /// Default LINQ Sort Elapsed Time: 25ms | 257972 ticks
        /// Sequential Span Radix Elapsed Time: 18ms | 188949 ticks
        /// Sequential Array Radix Elapsed Time: 4ms | 45708 ticks
        /// Parallel Array Radix Elapsed Time: 1ms | 18158 ticks
        /// </example>
        static void Main(string[] args)
        {
            // running the tests twice on each dataset because it seems like the parallel implementation is always a bit slower on the first run
            // probably the thread pool needs to warm up somehow
            RunTestOn("./names.txt");
            RunTestOn("./names.txt");
            RunTestOn("./last-names.txt");
            RunTestOn("./last-names.txt");
            RunTestOn("./last-names-duplicated.txt");
            RunTestOn("./last-names-duplicated.txt");
            RunTestOn("./international-names.txt");
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
            PadDataToSameLength(lines, longestNameLength);

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(lines);
            Console.WriteLine($"Sequential Span Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms | {stopwatch.ElapsedTicks} ticks");

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
            PadDataToSameLength(lines, longestNameLength);

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(lines);
            Console.WriteLine($"Sequential Array Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms | {stopwatch.ElapsedTicks} ticks");

            SwapLastAndFirstNames(lines);
            Directory.CreateDirectory("./outputarray/");
            File.WriteAllText("./outputarray/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
        }

        private static void RunParallelArrayRadixSort(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var longestNameLength = lines.Max(f => f.Length);
            var radixSorter = new ParallelArrayRadixSorter(longestNameLength, 8);
            SwapLastAndFirstNames(lines);
            PadDataToSameLength(lines, longestNameLength);

            var stopwatch = Stopwatch.StartNew();
            var data = radixSorter.Sort(lines);
            Console.WriteLine($"Parallel Array Radix Elapsed Time: {stopwatch.ElapsedMilliseconds}ms | {stopwatch.ElapsedTicks} ticks");

            SwapLastAndFirstNames(lines);
            Directory.CreateDirectory("./outputparallel/");
            File.WriteAllText("./outputparallel/" + filePath, string.Join(Environment.NewLine, string.Join(Environment.NewLine, data)));
        }

        /// <summary>
        /// Swap last and first names for ease of use by sorting by last name.
        /// This allows radix sort to work properly
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

        /// <summary>
        /// Essential for MSD radix sort to work in this implementation
        /// </summary>
        private static void PadDataToSameLength(string[] lines, int longestNameLength)
        {
            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].PadRight(longestNameLength);
            }
        }
    }
}