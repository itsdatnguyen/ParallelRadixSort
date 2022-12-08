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
    /// This can be around 4X faster, but it appears to run slower the first time it's run. 
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
        /// Ran the published .exe file in release mode on computer with a Ryzen 5800X3D
        /// 
        /// Running sorting for ./names.txt: 10000 names)
        /// Default LINQ Sort Elapsed Time: 15ms | 157986 ticks
        /// Sequential Span Radix Elapsed Time: 9ms | 90792 ticks
        /// Sequential Array Radix Elapsed Time: 1ms | 14889 ticks
        /// Parallel Array Radix Elapsed Time: 4ms | 40144 ticks
        /// 
        /// Running sorting for ./names.txt: 10000 names)
        /// Default LINQ Sort Elapsed Time: 7ms | 74046 ticks
        /// Sequential Span Radix Elapsed Time: 9ms | 90663 ticks
        /// Sequential Array Radix Elapsed Time: 1ms | 11841 ticks
        /// Parallel Array Radix Elapsed Time: 0ms | 4645 ticks
        /// 
        /// Running sorting for ./last-names.txt: 88799 names)
        /// Default LINQ Sort Elapsed Time: 84ms | 842277 ticks
        /// Sequential Span Radix Elapsed Time: 27ms | 278048 ticks
        /// Sequential Array Radix Elapsed Time: 8ms | 88984 ticks
        /// Parallel Array Radix Elapsed Time: 3ms | 39321 ticks
        /// 
        /// Running sorting for ./last-names.txt: 88799 names)
        /// Default LINQ Sort Elapsed Time: 86ms | 862788 ticks
        /// Sequential Span Radix Elapsed Time: 17ms | 178738 ticks
        /// Sequential Array Radix Elapsed Time: 9ms | 94229 ticks
        /// Parallel Array Radix Elapsed Time: 3ms | 39555 ticks
        /// 
        /// Running sorting for ./last-names-duplicated.txt: 887990 names)
        /// Default LINQ Sort Elapsed Time: 945ms | 9451318 ticks
        /// Sequential Span Radix Elapsed Time: 96ms | 969351 ticks
        /// Sequential Array Radix Elapsed Time: 91ms | 911375 ticks
        /// Parallel Array Radix Elapsed Time: 22ms | 224468 ticks
        /// 
        /// Running sorting for ./last-names-duplicated.txt: 887990 names)
        /// Default LINQ Sort Elapsed Time: 951ms | 9516193 ticks
        /// Sequential Span Radix Elapsed Time: 102ms | 1020203 ticks
        /// Sequential Array Radix Elapsed Time: 87ms | 872642 ticks
        /// Parallel Array Radix Elapsed Time: 23ms | 232995 ticks
        /// 
        /// Running sorting for ./international-names.txt: 19948 names)
        /// Default LINQ Sort Elapsed Time: 14ms | 146277 ticks
        /// Sequential Span Radix Elapsed Time: 4ms | 48655 ticks
        /// Sequential Array Radix Elapsed Time: 2ms | 20413 ticks
        /// Parallel Array Radix Elapsed Time: 0ms | 9762 ticks
        /// 
        /// Running sorting for ./international-names.txt: 19948 names)
        /// Default LINQ Sort Elapsed Time: 14ms | 144155 ticks
        /// Sequential Span Radix Elapsed Time: 4ms | 48620 ticks
        /// Sequential Array Radix Elapsed Time: 2ms | 20414 ticks
        /// Parallel Array Radix Elapsed Time: 0ms | 9791 ticks
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