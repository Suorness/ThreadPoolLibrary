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

        private Dictionary<int?, ManualResetEvent> threadsEvent;

        private List<Task> threadList;

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
                    StartTaskOnFreeThread();
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

            threadsEvent = new Dictionary<int?, ManualResetEvent>(countMaxThread);

            threadList = new List<Task>();
            for (int i = 0; i < countStartThread; i++)
            {
                // TODO CHEACK CREATE
                Task thread = new Task(ThreadWork, TaskCreationOptions.LongRunning);
                threadsEvent.Add(thread.Id, new ManualResetEvent(false));
                thread.Start();
                threadList.Add(thread);
            }
            taskList = new List<TaskW>();

        }
        public void Start()
        {
            StartTaskOnFreeThread();
        }

        private void ThreadWork()
        {
            while (true)
            {
                threadsEvent[Task.CurrentId].WaitOne();
                TaskW task = null;
                task = SelectTask();
                if (task != null)
                {
                    try
                    {
                        task.Execute();
                    }
                    finally
                    {
                       
                        if (isStop)
                            stopTaskEvent.Set();
                        threadsEvent[Task.CurrentId].Reset();
                    }
                }
            }

        }

        private TaskW SelectTask()
        {
            lock (taskList)
            {
                if (taskList.Count == 0)
                {
                    throw new ArgumentException();
                    //TODO Log Exeption
                }
                var waitingTask = taskList.Where(t => !t.IsRun);
                //TODO разграничать приоритеты
                // Для проверки не будем учитывать приоритеты
                if (waitingTask.Count() > 0)
                {
                    var task = waitingTask.ToArray().First();
                    DeleteTask(task);
                    return task;
                }
                else
                {
                    //TEST
                    throw new Exception();
                }
            }
        }

        private void StartTaskOnFreeThread()
        {
            //while (true)
            //{
            //    addTaskEvent.WaitOne();
                lock (threadList)
                {
                    foreach (var thread in threadList)
                    {
                        if (threadsEvent[thread.Id].WaitOne(0) == false)
                        {
                            threadsEvent[thread.Id].Set();
                            break;
                        }
                    }
                }
            //    addTaskEvent.Reset();
            //}
        }
    }
}
