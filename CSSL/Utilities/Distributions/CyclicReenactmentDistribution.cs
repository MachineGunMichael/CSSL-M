﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSSL.Utilities.Distributions
{
    public class CyclicReenactmentDistribution : Distribution
    {
        public CyclicReenactmentDistribution(double[] records) : base(records.Average(), records.Variance())
        {
            Records = records;
            index = 0;
        }

        private int index;

        public double[] Records { get; }

        public override double Next()
        {
            if (index < Records.Length)
            {
                return Records[index++];
            }
            else
            {
                index = 0;
                return Records[index++];
            }
        }
    }
}
