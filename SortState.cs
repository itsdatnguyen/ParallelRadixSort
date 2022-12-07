using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelRadixSort;

public class SortState
{
    public string[] Array { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public int Depth { get; set; }
    public int TreeDepth { get; }

    public SortState(string[] array, int startIndex, int endIndex, int depth, int treeDepth)
    {
        Array = array;
        StartIndex = startIndex;
        EndIndex = endIndex;
        Depth = depth;
        TreeDepth = treeDepth;
    }
}
