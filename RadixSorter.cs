using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelRadixSort;

/// <summary>
/// Class heavily utilizes <see cref="Span{T}"/> which greatly simplifes array handling
/// </summary>
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
        var (partitionMin, partitionMax) = Partition(span, depth, pivot);

        // three way partitioning, partition items less, equal and greater
        PerformSort(span[..partitionMin], depth);
        PerformSort(span[partitionMin..(partitionMax + 1)], depth + 1);
        PerformSort(span[(partitionMax + 1)..], depth);
    }

    private (int min, int max) Partition(Span<string> span, int depth, char pivot)
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

    private char GetPivot(Span<string> span, int depth)
    {
        // the median is selected as the pivot
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