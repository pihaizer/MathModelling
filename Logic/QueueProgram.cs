using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MathModelling {
    public static class QueueProgram {
        static int _workersCount = 10;
        static int _queueSize = 10;
        static double _maxTime = 1000; // in hours

        static double _incomeIntensity = 20;
        static double _processIntensity = 1;
        static double Load => _incomeIntensity / _processIntensity;

        static double _averageWaitTime = 0.5;
        static double WaitTimeIntensity => 1 / _averageWaitTime;

        public record Parameters {
            public int WorkersCount = 10;
            public int QueueSize = 10;
            public double IncomeIntensity = 20;
            public double ProcessIntensity = 1;
            public double AverageWaitTime = 0.5;
            public double MaxTime = 10;
            public bool OnlyUsedStates = false;
            public Func<double, double> ProcessDistributionFunc = Utility.GetRandomExponentialValue;
        }

        public static Output Run(Parameters parameters) {
            _workersCount = parameters.WorkersCount;
            _queueSize = parameters.QueueSize;
            _incomeIntensity = parameters.IncomeIntensity;
            _processIntensity = parameters.ProcessIntensity;
            _averageWaitTime = parameters.AverageWaitTime;
            _maxTime = parameters.MaxTime;
            
            var queueSystem = new QueueSystem {
                WorkersCount = _workersCount,
                MaxQueueSize = _queueSize,
                MaxTime = _maxTime,
                AddOnlyUsedStates = parameters.OnlyUsedStates
            };

            double time = 0;

            while (time < _maxTime) {
                time += Utility.GetRandomExponentialValue(_incomeIntensity);
                queueSystem.PlanUser(
                    new User {
                        QueueEnterTime = time,
                        ProcessTime = parameters.ProcessDistributionFunc(_processIntensity),
                        WaitTime = Utility.GetRandomExponentialValue(WaitTimeIntensity),
                    },
                    time);
            }

            queueSystem.Run();
            Console.WriteLine(
                $"Successful: {queueSystem.SuccessfulUsersCount}\n" +
                $"Failed: {queueSystem.FailedUsersCount}");
            
            double denyChance = (double) queueSystem.FailedUsersCount /
                (queueSystem.FailedUsersCount + queueSystem.SuccessfulUsersCount + queueSystem.ExpiredUsersCount);
            Console.WriteLine($"Deny chance: {denyChance}");
            
            double p0 = 1;

            p0 += Sum(1, _workersCount, i => DividedByFactorial(Math.Pow(Load, i), i)); // sum (i - n) ρ^i / i!

            double B = WaitTimeIntensity / _processIntensity;
            p0 += DividedByFactorial(Math.Pow(Load, _workersCount), _workersCount) // ρ^n / n!
                * Sum(1, _workersCount,
                    i => Math.Pow(Load, i) / Product(1, i, l => _workersCount + l * B)); // sum (i - m) (ρ^i / prod (n+lB)

            p0 = 1 / p0;

            // Console.WriteLine($"p0 = {p0}");
            
            double pn = DividedByFactorial(Math.Pow(Load, _workersCount), _workersCount) * p0;
            
            // Console.WriteLine($"pn = {pn}");

            double pnm = pn * Math.Pow(Load, _queueSize) /
                Product(1, _queueSize, l => _workersCount + l * B);
            
            // Console.WriteLine($"pnm = {pnm}");
            Console.WriteLine($"Expected deny chance: {pnm}");

            var output = new Output {
                Analytics = queueSystem.Analytics,
                DenyChance = denyChance,
                ExpectedDenyChance = pnm,
                SuccessfulUsersCount = queueSystem.SuccessfulUsersCount,
                FailedUsersCount = queueSystem.FailedUsersCount,
                AverageTimeInQueue = queueSystem.AverageTimeInQueue,
                AverageUsersInQueue = queueSystem.AveragePeopleInQueue
            };

            return output;
        }

        public record Output {
            public int SuccessfulUsersCount;
            public int FailedUsersCount;
            public double DenyChance;
            public double ExpectedDenyChance;
            public double AverageUsersInQueue;
            public double AverageTimeInQueue;
            public SortedDictionary<double, Dictionary<Tuple<int, int>, double>> Analytics;
        }

        static double DividedByFactorial(double value, int factorial) {
            for (int j = 1; j <= factorial; j++) {
                value /= j;
            }

            return value;
        }

        static double Sum(int from, int to, Func<int, double> func) {
            double sum = 0;
            
            for (int i = from; i <= to; i++) {
                sum += func(i);
            }

            return sum;
        }

        static double Product(int from, int to, Func<int, double> func) {
            double product = 1;
            
            for (int i = from; i <= to; i++) {
                product *= func(i);
            }

            return product;
        }
    }
}