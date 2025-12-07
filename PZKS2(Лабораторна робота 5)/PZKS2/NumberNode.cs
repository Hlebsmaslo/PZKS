using System;
using System.Collections.Generic;

namespace PZKS2
{
    public class NumberNode : Node
    {
        public double Value;
        public NumberNode(double v) { Value = v; }
        public override string Label => Value.ToString();
    }
}