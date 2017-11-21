using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private int countMaxThread;
        private int countThread;
        private int countActiveThread;

        private ManualResetEvent stopTaskEvent;

        private Dictionary<int?, ManualResetEvent> threadsEvent;

        private List<Task> threadList;

        private List<ThreadPoolTask> taskList;

        private object lockObj;

        private bool isStop;

        private void AddTask(ThreadPoolTask task)
        {
            lock (taskList)
            {
                taskList.Add(task);
            }
        }

        private void DeleteTask(ThreadPoolTask task)
        {
            lock (taskList)
            {
                taskList.Remove(task);
            }
        }

        public bool Execute(ThreadPoolTask task)
        {
            bool result = true;
            lock (lockObj)
            {
                if (!isStop)
                {
                    AddTask(task);
                    logger.Info("Add new task");
                    StartTaskOnFreeThread();
                }
                else
                {
                    string strE = "Task execution failed.";
                    logger.Error(strE);
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
                string strE = "The initial number of finished streams should not be more than the total number.";
                logger.Error(strE);
                throw new ArgumentException(strE);
            }

            if ((countMaxThread <= 0) || (countStartThread <= 0))
            {
                string strE = "The initial values of the number of threads must be positive and nonzero.";
                logger.Error(strE);
                throw new ArgumentException(strE);
            }

            this.countMaxThread = countMaxThread;
            countThread = 0;
            countActiveThread = 0;

            lockObj = new object();

            stopTaskEvent = new ManualResetEvent(false);

            threadsEvent = new Dictionary<int?, ManualResetEvent>();
            threadList = new List<Task>();
            taskList = new List<ThreadPoolTask>();

            for (int i = 0; i < countStartThread; i++)
            {
                createThread();
            }
            logger.Info("Added {0} threads",countStartThread);


        }
        private void createThread()
        {
            countThread++;
            if (countThread > countMaxThread)
            {
                logger.Warn("The allowed number of threads is exceeded, an additional thread is allocated");
                countMaxThread++;

            }
            Task thread = new Task(ThreadWork, TaskCreationOptions.LongRunning);
            threadsEvent.Add(thread.Id, new ManualResetEvent(false));
            thread.Start();
            threadList.Add(thread);
        }

        private void ThreadWork()
        {
            while (true)
            {
                threadsEvent[Task.CurrentId].WaitOne();
                ThreadPoolTask task = SelectTask();
                if (task != null)
                {
                    try
                    {
                        task.Execute();
                    }
                    finally
                    {
                        if (isStop)
                        {
                            stopTaskEvent.Set();
                        }
                        threadsEvent[Task.CurrentId].Reset();
                    }
                }
            }

        }

        private ThreadPoolTask SelectTask()
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
        }
    }
}
