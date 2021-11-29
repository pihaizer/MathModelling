using System.Collections.Generic;

namespace MathModelling {
    public class Worker {
        public bool IsWorking;
        public User CurrentUser;

        public void StartProcessing(User user) {
            CurrentUser = user;
            IsWorking = true;
        }

        public void StopProcessing() {
            CurrentUser = null;
            IsWorking = false;
        }
    }
}