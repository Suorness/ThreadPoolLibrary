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

        private Task[] threads;

        private List<TaskW> taskList;

        private object lockObj;

        private bool isStop;

        private void AddTask(TaskW task)
        {
            lock (taskList)
            {
                taskList.Add(task);
            }

            addTaskEvent.Set();
        }

        private void DeleteTask(TaskW task)
        {
            lock (taskList)
            {
                taskList.Remove(task);
            }

        }

        public bool Execute(TaskW task)
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
                threads[i] = new Task(ThreadWork(), TaskCreationOptions.LongRunning);

            }
            taskList = new List<TaskW>();

        }
        public void Start()
        {

        }
        //wtf

        private  Action ThreadWork()
        {
            while (true)
            {
                //threadsEvent[Thread.CurrentThread.ManagedThreadId].WaitOne();
                // не будет работать вероятно 
                TaskW task = null;
                //task = SelectTask();
                if (task != null)
                {
                    try
                    {
                        task.Execute();
                    }
                    finally
                    {
                        DeleteTask(task);
                        // проверка на завершение
                    }
                }
            }

        }
    }
}
