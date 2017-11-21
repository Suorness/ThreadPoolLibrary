using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPoolLib
{
    public class ThreadPool
    {
        private int countMaxThread;

        private int countThread;

        private ManualResetEvent addTaskEvent;

        private ManualResetEvent stopTaskEvent;

        private Dictionary<int, ManualResetEvent> threadsEvent;

        private List<Task> taskList;

        private object lockObj;

        private bool isStop;

        private void AddTask(Task task)
        {
            lock (taskList)
            {
                taskList.Add(task);
            }

            addTaskEvent.Set();
        }

        private void DeleteTask(Task task)
        {
            lock (taskList)
            {
                taskList.Remove(task);
            }

        }

        public bool Execute(Task task)
        {
            bool result = true;
            lock (lockObj)
            {
                if (!isStop)
                {
                    AddTask(task);
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        public void Stop()
        {
            lock (lockObj)
            {
                isStop = true;
            }
            while (taskList.Count > 0)
            {
                //TODO STOP
            }
            // TODO DISPOSE
        }

        public ThreadPool() : this(System.Environment.ProcessorCount, System.Environment.ProcessorCount) { }

        public ThreadPool(int countStartThread, int countMaxThread)
        {
            if (countStartThread > countMaxThread)
            {
                //TODO LOG Exeption
            }

            if ((countMaxThread <= 0) || (countStartThread <= 0))
            {
                //TODO LOG Exeption 
            }

            this.countMaxThread = countMaxThread;

            lockObj = new object();

            stopTaskEvent = new ManualResetEvent(false);
            addTaskEvent = new ManualResetEvent(false);

            threadsEvent = new Dictionary<int, ManualResetEvent>(countMaxThread);

            for (int i = 0; i < countStartThread; i++)
            {
                // TODO CREATE thread 

            }
            taskList = new List<Task>();

        }
        public void Start()
        {

        }

        private void ThreadWork()
        {
            while (true)
            {
                //threadsEvent[Thread.CurrentThread.ManagedThreadId].WaitOne();
                // не будет работать вероятно 

            }

        }
    }
}
