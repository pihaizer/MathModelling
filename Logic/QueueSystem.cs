using System;
using System.Collections.Generic;
using System.Linq;

using MathModelling.Tasks;

namespace MathModelling {
    public class QueueSystem {
        public int WorkersCount { get; set; } = 3;
        public double MaxTime { get; set; } = 5;
        public int MaxQueueSize = 6;
        public bool AddOnlyUsedStates = false;

        public int SuccessfulUsersCount { get; private set; }
        public int FailedUsersCount { get; private set; }
        public int ExpiredUsersCount { get; private set; }

        public double CurrentTime { get; private set; }
        public bool IsRunning { get; private set; }
        
        public double AveragePeopleInQueue => (double) _peopleInQueueSum / _counter;
        public double AverageTimeInQueue => _timeInQueueSum / SuccessfulUsersCount;

        readonly List<User> _queuedUsers = new();
        readonly SortedDictionary<double, Task> _tasksMap = new();
        public readonly SortedDictionary<double, Dictionary<Tuple<int, int>, double>> Analytics =
            new();

        Dictionary<Tuple<int, int>, int> _currentAnalytics = new();

        int _availableWorkersCount;
        long _counter;
        long _addCounter;
        long _addStepValue;
        long _peopleInQueueSum;
        double _timeInQueueSum;

        public void Run() {
            _availableWorkersCount = WorkersCount;

            IsRunning = true;
            _counter = 0;
            _peopleInQueueSum = 0;
            _addStepValue = _tasksMap.Count / 10;
            _addCounter = 1;
            _timeInQueueSum = 0;

            if (!AddOnlyUsedStates) {
                for (int i = 0; i <= WorkersCount; i++) {
                    _currentAnalytics[Tuple.Create(i, 0)] = 0;
                }

                for (int i = 0; i <= MaxQueueSize; i++) {
                    _currentAnalytics[Tuple.Create(WorkersCount, i)] = 0;
                }
            }

            while (_tasksMap.Count > 0 && _tasksMap.First().Value.ExecuteTime < MaxTime) {
                Task task = _tasksMap.First().Value;
                _tasksMap.Remove(task.ExecuteTime);
                ProcessTask(task);
            }
        }

        void ProcessUser(User user) {
            if (user.ExpireTask != null) _tasksMap.Remove(user.ExpireTask.ExecuteTime);

            _timeInQueueSum += CurrentTime - user.QueueEnterTime;

            _availableWorkersCount--;

            var finishTask = new FinishUserTask(
                GetAvailableTime(CurrentTime + user.ProcessTime), user);
            _tasksMap.Add(finishTask.ExecuteTime, finishTask);
        }

        void ProcessTask(Task task) {
            CurrentTime = task.ExecuteTime;
            // Console.WriteLine($"{CurrentTime}: Processing task {task.GetType()}." + 
            //     (task is UserTask userTask1 ? $"User id = {userTask1.User.Id}."  : ""));

            switch (task) {
                case AddUserTask userTask:
                    AddUser(userTask.User);
                    break;
                case ExpireUserTask userTask:
                    ExpireUser(userTask.User);
                    break;
                case FinishUserTask:
                    FinishUser();
                    break;
            }

            _counter++;
            _addCounter--;
            Tuple<int, int> tuple =
                Tuple.Create(WorkersCount - _availableWorkersCount, _queuedUsers.Count);
            
            if (AddOnlyUsedStates && !_currentAnalytics.ContainsKey(tuple)) {
                _currentAnalytics.Add(tuple, 0);
                foreach (KeyValuePair<double,Dictionary<Tuple<int,int>,double>> time in Analytics) {
                    time.Value[tuple] = 0;
                }
            }

            _currentAnalytics[tuple]++;

            if (_addCounter == 0) {
                Dictionary<Tuple<int, int>, double> timestamp = _currentAnalytics.ToDictionary(
                    key => key.Key,
                    pair => (double) pair.Value / _counter);
                Analytics[CurrentTime] = timestamp;
                _addCounter = _addStepValue;
            }

            _peopleInQueueSum += _queuedUsers.Count;
        }

        public void PlanUser(User user, double time) {
            _tasksMap[time] = new AddUserTask(time, user);
            // Console.WriteLine($"User planned. Process time = {user.ProcessTime}, Wait time = {user.WaitTime}");
        }

        void AddUser(User user) {
            if (!IsRunning) throw new Exception("Can't add users while not running.");

            if (HasFreeWorkers()) {
                ProcessUser(user);
                return;
            }

            if (_queuedUsers.Count == MaxQueueSize) {
                FailedUsersCount++;
                return;
            }

            _queuedUsers.Add(user);
            var expireTask = new ExpireUserTask(
                GetAvailableTime(CurrentTime + user.WaitTime), user);
            _tasksMap.Add(expireTask.ExecuteTime, expireTask);
            user.ExpireTask = expireTask;
        }

        double GetAvailableTime(double nearToTime) {
            while (_tasksMap.ContainsKey(nearToTime)) {
                nearToTime += double.Epsilon * 10;
            }

            return nearToTime;
        }

        void FinishUser() {
            _availableWorkersCount++;
            SuccessfulUsersCount++;

            if (_queuedUsers.Count > 0) {
                User user = _queuedUsers[0];
                _queuedUsers.RemoveAt(0);
                ProcessUser(user);
            }
        }

        void ExpireUser(User user) {
            _queuedUsers.Remove(user);
            ExpiredUsersCount++;
        }

        bool HasFreeWorkers() => _availableWorkersCount > 0;
    }
}