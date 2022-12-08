using System.Diagnostics;

namespace ParallelRadixSort
{
    /// <summary>
    /// This project implements sequential and parallel sorting implementations to time sort data.
    /// Additionally, this project will write back the sorted list into text files.
    /// There are 3 implementations of sorting:
    /// 
    /// <see cref="SequentialSpanRadixSorter"/> was the first implementation that I wrote that uses <see cref="Span{T}"/>
    /// I didn't really know it at the time, but it seems like using Span is a bit slower than a normal array
    /// 
    /// <see cref="SequentialArrayRadixSorter"/> was the second implementation that uses a normal array. As you can see by the results, it is noticably faster.
    /// 
    /// <see cref="ParallelArrayRadixSorter"/> was my parallel implementation that uses a ThreadPool behind the scenes to run threads. 
    /// This can be around 4X faster, but it appears to run slow the first time it's run. 
    /// I assume that's due to some initial overhead with warming up the ThreadPool. 
    /// In my tests, I run each dataset twice so you can see that this parallel sort is only slower for the first run.
    /// Otherwise it beats every other algorithm afterward.
    /// With this class, I'm running the recursive <see cref="ParallelArrayRadixSorter.PerformSortForThread(object)"/>  function in a thread 
    /// if the current tree depth is under a certain number. I've given it a number of 8 since that seems the most effective.
    /// </summary>
    /// 
    /// <remarks>
    /// I originally tried tackling this project by doing parallel heapsort by running multiple heapsorts on sections of an array...
    /// but there is still the issue of how to recombine the data. Overall, the performance was terrible and I had to scrap that idea.
    /// 
    /// After that, I researched radix sort, including LSD and MSD radix sort. 
    /// The idea of a quicksort tree-based algorithm sounded like a practical idea to parallelize, so that's the main focus for this project.
    /// Overall, I'm pretty happy with the performance. MSD radix sort is very fast as is, and making it parallel seems to be great for large datasets.
    /// </remarks>
    public class Program
    {
        /// <example>
        /// Sample generated output:
        /// Ran on a computer with a Ryzen 5800X3D
        /// 
        /// Running sorting for ./names.txt: 10000 names)
        /// Default LINQ Sort Elapsed Time: 19ms | 190598 ticks
        /// Sequential Span Radix Elapsed Time: 12ms | 125059 ticks
        /// Sequential Array Radix Elapsed Time: 2ms | 25533 ticks
        /// Parallel Array Radix Elapsed Time: 30ms | 305147 ticks
        /// 
        /// Running sorting for ./names.txt: 10000 names)
        /// Default LINQ Sort Elapsed Time: 13ms | 133831 ticks
        /// Sequential Span Radix Elapsed Time: 11ms | 117640 ticks
        /// Sequential Array Radix Elapsed Time: 2ms | 22795 ticks
        /// Parallel Array Radix Elapsed Time: 0ms | 6338 ticks
        /// 
        /// Running sorting for ./last-names.txt: 88799 names)
        /// Default LINQ Sort Elapsed Time: 153ms | 1535361 ticks
        /// Sequential Span Radix Elapsed Time: 80ms | 801429 ticks
        /// Sequential Array Radix Elapsed Time: 21ms | 214732 ticks
        /// Parallel Array Radix Elapsed Time: 5ms | 56095 ticks
        /// 
        /// Running sorting for ./last-names.txt: 88799 names)
        /// Default LINQ Sort Elapsed Time: 158ms | 1589730 ticks
        /// Sequential Span Radix Elapsed Time: 74ms | 742404 ticks
        /// Sequential Array Radix Elapsed Time: 19ms | 196879 ticks
        /// Parallel Array Radix Elapsed Time: 5ms | 50898 ticks
        /// 
        /// Running sorting for ./last-names-duplicated.txt: 887990 names)
        /// Default LINQ Sort Elapsed Time: 1687ms | 16879867 ticks
        /// Sequential Span Radix Elapsed Time: 356ms | 3564332 ticks
        /// Sequential Array Radix Elapsed Time: 226ms | 2266956 ticks
        /// Parallel Array Radix Elapsed Time: 52ms | 528030 ticks
        /// 
        /// Running sorting for ./last-names-duplicated.txt: 887990 names)
        /// Default LINQ Sort Elapsed Time: 1661ms | 16613146 ticks
        /// Sequential Span Radix Elapsed Time: 345ms | 3453274 ticks
        /// Sequential Array Radix Elapsed Time: 229ms | 2291630 ticks
        /// Parallel Array Radix Elapsed Time: 53ms | 534017 ticks
        /// 
        /// Running sorting for ./international-names.txt: 19948 names)
        /// Default LINQ Sort Elapsed Time: 25ms | 254073 ticks
        /// Sequential Span Radix Elapsed Time: 19ms | 191314 ticks
        /// Sequential Array Radix Elapsed Time: 4ms | 49259 ticks
        /// Parallel Array Radix Elapsed Time: 1ms | 18206 ticks
        /// 
        /// Running sorting for ./international-names.txt: 19948 names)
        /// Default LINQ Sort Elapsed Time: 25ms | 257052 ticks
        /// Sequential Span Radix Elapsed Time: 18ms | 189034 ticks
        /// Sequential Array Radix Elapsed Time: 4ms | 45182 ticks
        /// Parallel Array Radix Elapsed Time: 1ms | 16821 ticks
        /// </example>
        static void Main(string[] args)
        {
            // running the tests twice on each dataset because it seems like the parallel implementation is always a bit slower on the first run
            // probably the thread pool needs to warm up somehow

            // I've additionally provided more files to run sorting on
            RunTestOn("./names.txt");
            RunTestOn("./names.txt");
            RunTestOn("./last-names.txt");
            RunTestOn("./last-names.txt");
            RunTestOn("./last-names-duplicated.txt");
            RunTestOn("./last-names-duplicated.txt");
            RunTestOn("./international-names.txt");
            RunTestOn("./international-names.txt");
            Console.ReadLine();
        }

        private static void RunTestOn(string filePath)
        {
            Console.WriteLine($"Running sorting for {filePath}: {File.ReadAllLines(filePath).Length} names)");
            RunDefaultSequentialLinqSorting(filePath);
            RunSequentialSpanRadixSort(filePath);
            RunSequentialArrayRadixSort(filePath);
            RunParallelArrayRadixSort(filePath);
            Console.WriteLine();
        }

        private static void RunDefaultSequentialLinqSorting(string filePath)
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

            // 8 seems like the most effective number to use for performance
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