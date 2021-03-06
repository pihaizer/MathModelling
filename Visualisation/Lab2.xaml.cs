using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using LiveCharts;
using LiveCharts.Wpf;

using MathModelling;

namespace Visualisation {
    public partial class Lab2 : UserControl {
        public int WorkersCount { get; set; } = 3;
        public int QueueSize { get; set; } = 3;
        public double IncomeIntensity { get; set; } = 5;
        public double ProcessIntensity { get; set; } = 2;
        public double AverageWaitTime { get; set; } = 1;
        public double SimulationTime { get; set; } = 10;

        TextBlock _resultsTextBox;

        public Lab2() {
            InitializeComponent();

            DataContext = this;
            _resultsTextBox = FindName("Results") as TextBlock;
        }

        void Simulate(object sender, RoutedEventArgs routedEventArgs) {
            var parameters = new QueueProgram.Parameters() {
                WorkersCount = WorkersCount,
                QueueSize = QueueSize,
                IncomeIntensity = IncomeIntensity,
                ProcessIntensity = ProcessIntensity,
                AverageWaitTime = AverageWaitTime,
                MaxTime = SimulationTime,
                OnlyUsedStates = true
            };
            QueueProgram.Output results = QueueProgram.Run(parameters);
            _resultsTextBox.Text =
                $"Successful users: {results.SuccessfulUsersCount}\n" +
                $"Failed users: {results.FailedUsersCount}\n" +
                $"Deny chance: {results.DenyChance}\n" +
                $"Expected deny chance: {results.ExpectedDenyChance}\n";

            SeriesCollection.Clear();
            Labels.Clear();
            
            SortedDictionary<double, Dictionary<Tuple<int, int>, double>> analytics =
                results.Analytics;
            
            var series = new Dictionary<Tuple<int, int>, Dictionary<double, double>>();
            
            foreach (Tuple<int, int> tuple in analytics.Last().Value.Keys) {
                series.Add(tuple, new Dictionary<double, double>());
            }

            foreach (KeyValuePair<double, Dictionary<Tuple<int, int>, double>> timeTuple in
                analytics) {
                Labels.Add(Math.Round(timeTuple.Key, 3).ToString(CultureInfo.InvariantCulture));
                foreach (KeyValuePair<Tuple<int, int>, double> tupleValue in timeTuple.Value) {
                    series[tupleValue.Key].Add(timeTuple.Key, tupleValue.Value);
                }
            }

            foreach (KeyValuePair<Tuple<int, int>, Dictionary<double, double>> s in series) {
                SeriesCollection.Add(new LineSeries() {
                    Title = s.Key.ToString(),
                    Values = new ChartValues<double>(s.Value.Values),
                });
            }
        }

        public SeriesCollection SeriesCollection { get; set; } = new SeriesCollection();
        public List<string> Labels { get; set; } = new List<string>();
    }
}