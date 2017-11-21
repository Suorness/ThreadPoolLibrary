using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPoolLib
{
    public class ThreadPoolTask
    {
        private Action action;

        private Priority priority;

        private bool isRun;


        public ThreadPoolTask(Action action, Priority priority)
        {
            this.action = action;
            this.priority = priority;
            isRun = false;
        }
        public ThreadPoolTask(Action action) : this(action, Priority.Normal) { }

        public void Execute()
        {
            lock (this)
            {
                isRun = true;
            }
            action();
        }

        public bool IsRun { get; }

        public Priority GetPriority { get { return priority; } }
    }
}
