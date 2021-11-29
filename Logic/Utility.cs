using System;

namespace MathModelling {
    public static class Utility {
        static readonly Random _random = new();

        public static double GetRandomExponentialValue(double lambda = 1f) =>
            -1f / lambda * Math.Log(1 - _random.NextDouble());
    }
}