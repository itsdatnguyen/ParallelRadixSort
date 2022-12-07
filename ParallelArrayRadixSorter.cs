using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelRadixSort;

/// <summary>
/// <para>
/// MSD Radix Sort implementation that utilizes 3-Way quicksort.
/// The MSD (Most Significant Digit) algorithm starts with the most significant digit (the left-most digit) and works its way to the less significant digits.
/// As data is partitioned, the data is split into items: less than, equal to, and greater than the pivot.
/// </para>
/// <para>
/// Interestingly, this class is faster than <see cref="SequentialSpanRadixSorter"/>. I assume there is more overhead with using <see cref="Span{T}"/>
/// </para>
/// </summary>
public class ParallelArrayRadixSorter : IDisposable
{
    private readonly int _maxStringLength;
    private readonly int _threadTreeDepth;
    private CountdownEvent _countdown;

    public ParallelArrayRadixSorter(int maxStringLength, int threadTreeDepth)
    {
        _maxStringLength = maxStringLength;
        _threadTreeDepth = threadTreeDepth;
        _countdown = new CountdownEvent(1);
    }

    public string[] Sort(string[] array)
    {
        PerformSort(array, 0, array.Length - 1, 0, 1);

        // block until all threads are done
        _countdown.Signal();
        _countdown.Wait();
        return array;
    }

    /// <summary>
    /// Entry point for threads 
    /// </summary>
    public void PerformSort(object data)
    {
        var sortData = data as SortState;
        PerformSort(sortData.Array, sortData.StartIndex, sortData.EndIndex, sortData.Depth, sortData.TreeDepth);
        _countdown.Signal();
    }

    public void PerformSort(string[] array, int startIndex, int endIndex, int radixDepth, int treeDepth)
    {
        if (endIndex - startIndex < 1 || radixDepth >= _maxStringLength)
        {
            return;
        }

        var pivot = GetPivot(array, startIndex, endIndex, radixDepth);
        var (minEqualBoundary, maxEqualBoundary) = Partition(array, startIndex, endIndex, radixDepth, pivot);

        // three way partitioning, partition items less, equal and greater than the pivot
        // this is useful because for items where the radix is the same as the pivot, we need to go deeper in the radix to sort further

        // only really queue threads for first depth
        // creating your own threads is very slow, so I'm using thread pool since that seems a lot faster
        if (treeDepth <= _threadTreeDepth)
        {
            _countdown.AddCount(3);
            ThreadPool.QueueUserWorkItem(PerformSort, new SortState(array, startIndex, minEqualBoundary - 1, radixDepth, treeDepth + 1));
            ThreadPool.QueueUserWorkItem(PerformSort, new SortState(array, minEqualBoundary, maxEqualBoundary, radixDepth + 1, treeDepth + 1));
            ThreadPool.QueueUserWorkItem(PerformSort, new SortState(array, maxEqualBoundary + 1, endIndex, radixDepth, treeDepth + 1));
        }
        else
        {
            PerformSort(array, startIndex, minEqualBoundary - 1, radixDepth, treeDepth + 1);
            PerformSort(array, minEqualBoundary, maxEqualBoundary, radixDepth + 1, treeDepth + 1);
            PerformSort(array, maxEqualBoundary + 1, endIndex, radixDepth, treeDepth + 1);
        }
    }

    /// <summary>
    /// Three way quicksort partitioning of the data into 3 sections: less than, equal, and greater than the pivot
    /// </summary>
    private (int minEqualBoundary, int maxEqualBoundary) Partition(string[] array, int startIndex, int endIndex, int depth, char pivot)
    {
        int min = startIndex, max = endIndex, i = startIndex;

        while (i <= max)
        {
            if (array[i][depth] < pivot)
            {
                Swap(array, i, min);
                min++;
                i++;
            }
            else if (array[i][depth] > pivot)
            {
                Swap(array, i, max);
                max--;
            }
            else
            {
                i++;
            }
        }

        return (min, max);
    }

    /// <summary>
    /// The median value of the span is selected as the pivot
    /// </summary>
    private char GetPivot(string[] array, int startIndex, int endIndex, int depth)
    {
        var medianIndex = startIndex + (endIndex - startIndex) / 2;
        return array[medianIndex][depth];
    }

    private void Swap(string[] array, int index1, int index2)
    {
        var temp = array[index1];
        array[index1] = array[index2];
        array[index2] = temp;
    }

    public void Dispose()
    {
        _countdown.Dispose();
    }
}