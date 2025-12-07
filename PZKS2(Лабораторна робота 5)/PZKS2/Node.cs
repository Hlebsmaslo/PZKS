using System;
using System.Collections.Generic;

namespace PZKS2
{
    public abstract class Node
    {
        public abstract string Label { get; }
        public int FinishTime { get; set; } = 0;
    }
}