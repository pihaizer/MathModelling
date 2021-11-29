using MathModelling.Tasks;

namespace MathModelling {
    public record User {
        public double QueueEnterTime;
        public double WaitTime;
        public double ProcessTime;
        public ExpireUserTask ExpireTask;
        public int Id = _idCounter++;
        
        static int _idCounter = 0;
    }
}