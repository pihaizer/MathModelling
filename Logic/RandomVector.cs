using System;
using System.Collections.Generic;
using System.Linq;

namespace MathModelling {
    public static class RandomVector {
        static readonly Random _random = new();
        const double _sumEpsilon = 1e-9;

        static double[,] _inputMatrix;

        static double[] _rowSums;
        static double[] _columnSums;

        public static Output Run(double[,] inputMatrix, int count) {
            _inputMatrix = inputMatrix;
            double sum = 0;

            for (int i = 0; i < _inputMatrix.GetLength(0); i++) {
                for (int j = 0; j < _inputMatrix.GetLength(1); j++) {
                    sum += _inputMatrix[i, j];
                }
            }

            if (Math.Abs(1.0 - sum) > _sumEpsilon) {
                Console.WriteLine($"ERROR: Matrix sum is {sum} != 1");
                return null;
            }

            _rowSums = new double[_inputMatrix.GetLength(0)];
            _columnSums = new double[_inputMatrix.GetLength(1)];

            for (int i = 0; i < _inputMatrix.GetLength(0); i++) {
                for (int j = 0; j < _inputMatrix.GetLength(1); j++) {
                    _rowSums[i] += _inputMatrix[i, j];
                    _columnSums[j] += _inputMatrix[i, j];
                }
            }

            var output = new Output();
            
            List<Tuple<int, int>> randomValues = new();


            for (int n = 0; n < count; ++n) {
                int row = GetRow();
                int column = GetColumn(row);
                randomValues.Add(new Tuple<int, int>(row, column));
            }

            foreach (Tuple<int,int> randomValue in randomValues) {
                int row = randomValue.Item1;
                int column = randomValue.Item2;
                
                //hist
                Tuple<int, int> pos = Tuple.Create(row, column);
                output.Hist[pos] = 1.0 / count + (output.Hist.ContainsKey(pos) ? output.Hist[pos] : 0);
                
                //expected
                output.ExpectedRow += (double) row / count;
                output.ExpectedColumn += (double) column / count;
            }

            foreach (Tuple<int,int> randomValue in randomValues) {
                int row = randomValue.Item1;
                int column = randomValue.Item2;
                
                //dispersion
                output.DispersionRow += Math.Pow(output.ExpectedRow - row, 2) / count;
                output.DispersionColumn += Math.Pow(output.ExpectedColumn - column, 2) / count;
            }
            
            //standard deviation
            output.StandardDeviationRow = Math.Sqrt(output.DispersionRow);
            output.StandardDeviationColumn = Math.Sqrt(output.DispersionColumn);
            
            //interval row
            double u95 = 1.96;
            output.ExpectedInterval95RowMin = output.ExpectedRow - output.StandardDeviationRow * u95;
            output.ExpectedInterval95RowMax = output.ExpectedRow + output.StandardDeviationRow * u95;
            output.ExpectedInterval95ColumnMin = output.ExpectedColumn - output.StandardDeviationColumn * u95;
            output.ExpectedInterval95ColumnMax = output.ExpectedColumn + output.StandardDeviationColumn * u95;

            return output;
        }

        public record Output {
            public Dictionary<Tuple<int, int>, double> Hist = new();
            public double ExpectedRow;
            public double ExpectedColumn;
            public double DispersionRow;
            public double DispersionColumn;
            public double StandardDeviationRow;
            public double StandardDeviationColumn;
            public double ExpectedInterval95RowMin;
            public double ExpectedInterval95RowMax;
            public double ExpectedInterval95ColumnMin;
            public double ExpectedInterval95ColumnMax;
        }

        static int GetRow() {
            double randomValue = _random.NextDouble();
            int randomRow = 0;
            double randomRowSum = 0.0;

            for (int i = 0; i < _inputMatrix.GetLength(0); i++) {
                if (randomValue < randomRowSum + _rowSums[i]) {
                    randomRow = i;
                    break;
                }

                randomRowSum += _rowSums[i];
            }

            return randomRow;
        }

        static int GetColumn(int selectedRow) {
            double randomValue = _random.NextDouble();
            int randomColumn = 0;

            randomValue *= _rowSums[selectedRow];
            double randomColumnSum = 0.0;

            for (int i = 0; i < _inputMatrix.GetLength(1); i++) {
                if (randomValue < randomColumnSum + _inputMatrix[selectedRow, i]) {
                    randomColumn = i;
                    break;
                }

                randomColumnSum += _inputMatrix[selectedRow, i];
            }

            return randomColumn;
        }
    }
}