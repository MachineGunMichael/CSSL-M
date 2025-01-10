using System;
using System.Collections.Generic;
using System.Text;

namespace CSSL.Utilities.Distributions
{
    public class HalfNormalDistribution : Distribution
    {
        public HalfNormalDistribution(double mu, double sigma, bool type) : base(mu, sigma * sigma)
        {
            Sigma = sigma;
            positive = type;
        }

        public double Sigma { get; }
        private bool positive { get; }

        public override double Next()
        {
            if (positive)
                return Mean + Sigma * Math.Abs(rnd.NextGaussian());
            else
                return Mean + Sigma * -1.0 * Math.Abs(rnd.NextGaussian());
        }
    }
}
