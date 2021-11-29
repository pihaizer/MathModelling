using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

using MathModelling;

namespace Visualisation {
    public partial class Lab1 : UserControl {
        static double[,] _inputMatrix = {
            {0.08, 0.03, 0.09, 0.05, 0.08}, //0.33
            {0.05, 0.03, 0.07, 0.04, 0.06}, //0.25
            {0.01, 0.02, 0.01, 0.02, 0.04}, //0.10
            {0.02, 0.03, 0.02, 0.04, 0.01}, //0.12
            {0.01, 0.04, 0.07, 0.03, 0.05} //0.20
        };

        public ChartValues<HeatPoint> Values { get; set; } = new ChartValues<HeatPoint>();
        public List<string> X { get; set; } = new List<string>();
        public List<string> Y { get; set; } = new List<string>();

        public SeriesCollection XSeriesCollection { get; set; } = new SeriesCollection();
        public SeriesCollection YSeriesCollection { get; set; } = new SeriesCollection();

        public int Count { get; set; } = 100000;

        TextBox _textBox;

        public Lab1() {
            InitializeComponent();

            DataContext = this;
            _textBox = FindName("ExactValuesTextBox") as TextBox;
        }

        void Randomize(object sender, RoutedEventArgs routedEventArgs) {
            RandomVector.Output output = RandomVector.Run(_inputMatrix, Count);
            Dictionary<Tuple<int, int>, double> hist = output.Hist;

            Values.Clear();
            X.Clear();
            Y.Clear();

            foreach (KeyValuePair<Tuple<int, int>, double> keyValuePair in hist) {
                int row = keyValuePair.Key.Item1;
                int column = keyValuePair.Key.Item2;

                if (!Y.Contains(row.ToString())) Y.Add(row.ToString());
                if (!X.Contains(column.ToString())) X.Add(column.ToString());

                Values.Add(new HeatPoint(column, row, Math.Round(keyValuePair.Value, 3)));
            }

            X.Sort();
            Y.Sort();

            var xProbabilities = new SortedDictionary<int, double>();
            var yProbabilities = new SortedDictionary<int, double>();

            foreach (KeyValuePair<Tuple<int, int>, double> pair in hist) {
                int x = pair.Key.Item1;
                int y = pair.Key.Item2;

                if (!xProbabilities.ContainsKey(x))
                    xProbabilities.Add(x, pair.Value);
                else
                    xProbabilities[x] += pair.Value;

                if (!yProbabilities.ContainsKey(y))
                    yProbabilities.Add(y, pair.Value);
                else
                    yProbabilities[y] += pair.Value;
            }

            XSeriesCollection.Add(new ColumnSeries {
                Values = new ChartValues<double>(xProbabilities.Values)
            });

            YSeriesCollection.Add(new ColumnSeries {
                Values = new ChartValues<double>(yProbabilities.Values)
            });

            _textBox.Text = $"Expected row {output.ExpectedRow}\n" +
                $"Expected column {output.ExpectedColumn}\n" +
                $"Dispersion row {output.DispersionRow}\n" +
                $"Dispersion column {output.DispersionColumn}\n" +
                $"Standard deviation row {output.StandardDeviationRow}\n" +
                $"Standard deviation column {output.StandardDeviationColumn}\n" +
                $"Expected interval 95% row min {output.ExpectedInterval95RowMin}\n" +
                $"Expected interval 95% row max {output.ExpectedInterval95RowMax}\n" +
                $"Expected interval 95% column min {output.ExpectedInterval95ColumnMin}\n" +
                $"Expected interval 95% column max {output.ExpectedInterval95ColumnMax}\n";
        }
    }
}