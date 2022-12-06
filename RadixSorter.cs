using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelRadixSort;

/// <summary>
/// <para>
/// 3-Way MSD Radix Sort implementation that utilizes a quicksort with 3 partitioned sections.
/// The MSD (Most Signification Digit) algorithm starts with the most significant digit (the left-most digit) and works its way to the less significant digits.
/// </para>
/// </summary>
/// <remarks>
/// Class heavily utilizes <see cref="Span{T}"/> which greatly simplifes array handling.
/// </remarks>
public class RadixSorter
{
    private readonly int _maxStringLength;

    public RadixSorter(int maxStringLength)
    {
        _maxStringLength = maxStringLength;
    }

    public string[] Sort(string[] array)
    {
        PerformSort(array, 0);
        return array;
    }

    public void PerformSort(Span<string> span, int depth)
    {
        if (span.Length < 2 || depth >= _maxStringLength)
        {
            return;
        }

        var pivot = GetPivot(span, depth);
        var (minEqualBoundary, maxEqualBoundary) = Partition(span, depth, pivot);

        // three way partitioning, partition items less, equal and greater than the pivot
        // this is useful because for items where the radix is the same as the pivot, we need to go deeper in the radix to sort further

        // this is using a cool C# feature called ranges https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes
        // so this syntactic sugar is really easy to read combined with Span<T>
        PerformSort(span[..minEqualBoundary], depth);
        PerformSort(span[minEqualBoundary..(maxEqualBoundary + 1)], depth + 1);
        PerformSort(span[(maxEqualBoundary + 1)..], depth);
    }

    /// <summary>
    /// Three way quicksort partitioning of the data into 3 sections: less than, equal, greater than the pivot
    /// </summary>
    private (int minEqualBoundary, int maxEqualBoundary) Partition(Span<string> span, int depth, char pivot)
    {
        int min = 0, max = span.Length - 1, i = 0;

        while (i <= max)
        {
            if (span[i][depth] < pivot)
            {
                Swap(span, i, min);
                min++;
                i++;
            }
            else if (span[i][depth] > pivot)
            {
                Swap(span, i, max);
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
    private char GetPivot(Span<string> span, int depth)
    {
        var medianIndex = (span.Length - 1) / 2;
        return span[medianIndex][depth];
    }

    private void Swap(Span<string> span, int index1, int index2)
    {
        var temp = span[index1];
        span[index1] = span[index2];
        span[index2] = temp;
    }
}