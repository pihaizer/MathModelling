namespace MathModelling.Tasks {
    public class UserTask : Task {
        public User User;

        public UserTask(double executeTime, User user) : base(executeTime) {
            User = user;
        }
    }
    
    public class AddUserTask : UserTask {
        public AddUserTask(double executeTime, User user) : base(executeTime, user) { }
    }

    public class FinishUserTask : UserTask {
        public FinishUserTask(double executeTime, User user) : base(executeTime, user) { }
    }

    public class ExpireUserTask : UserTask {
        public ExpireUserTask(double executeTime, User user) : base(executeTime, user) { }
    }
}