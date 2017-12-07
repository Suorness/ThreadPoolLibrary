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
        CancellationTokenSource tokenSource;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private int countMaxThread;
        private int countThread;
        private int countActiveThread;

        private Dictionary<int?, ManualResetEvent> threadsEvent;
        private List<Task> threadList;
        private List<ThreadPoolTask> taskList;

        private object lockObj;
        private object lockCount;

        private bool isStop;
        private bool isDispose;

        private void AddTask(ThreadPoolTask task)
        {
            lock (taskList)
            {
                taskList.Add(task);
            }
        }
        private void AddTaskRange(IEnumerable<ThreadPoolTask> tasks)
        {
            lock (taskList)
            {
                taskList.AddRange(tasks);
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
                    StartTaskOnThread();
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

        public bool ExecuteRange(IEnumerable<ThreadPoolTask> tasks)
        {
            bool result = true;
            lock (lockObj)
            {
                if (!isStop)
                {
                    AddTaskRange(tasks);
                    logger.Info("Add new task, count:{0}", tasks.Count());
                    for (int i = 0; i < tasks.Count(); i++)
                    {
                        StartTaskOnThread();
                    }
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

            int temp = 0;
            do
            {
                lock (lockCount)
                {
                    temp = countActiveThread;
                }
            }
            while (temp > 0);
            //logger.Info("Вышло {0}", temp);
            Dispose();
        }
        private void Dispose()
        {
            if (!isDispose)
            {
                tokenSource.Cancel();
                foreach (var thread in threadList)
                {
                    threadsEvent[thread.Id].Set();
                    threadsEvent[thread.Id].Dispose();
                }

                try
                {
                    Task.WaitAll(threadList.ToArray());
                }
                catch (AggregateException e)
                {
                    logger.Error("Dispose error.");
                }
                finally
                {
                    tokenSource.Dispose();
                }
                //logger.Info("Остановлено");
                isDispose = true;
            }
        }
        public ThreadPool() : this(System.Environment.ProcessorCount, System.Environment.ProcessorCount) { }

        public ThreadPool(int countStartThread, int countMaxThread)
        {
            if (countStartThread > countMaxThread)
            {
                string strE = "The initial number of finished thread should not be more than the total number.";
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
            lockCount = new object();

            tokenSource = new CancellationTokenSource();

            threadsEvent = new Dictionary<int?, ManualResetEvent>();
            threadList = new List<Task>();
            taskList = new List<ThreadPoolTask>();

            for (int i = 0; i < countStartThread; i++)
            {
                CreateThread();
            }
            logger.Info("Added {0} threads", countStartThread);
        }

        private int CreateThread()
        {
            countThread++;
            if (countThread > countMaxThread)
            {
                logger.Warn("The allowed number of threads is exceeded, an additional thread is allocated");
                countMaxThread++;

            }
            Task thread = new Task(ThreadWork, tokenSource.Token, TaskCreationOptions.LongRunning);
            threadsEvent.Add(thread.Id, new ManualResetEvent(false));
            thread.Start();
            threadList.Add(thread);
            return thread.Id;
        }

        private void ThreadWork()
        {
            while (!tokenSource.IsCancellationRequested)
            {
                threadsEvent[Task.CurrentId].WaitOne();
                ThreadPoolTask task = SelectTask();
                if (task != null)
                {
                    try
                    {
                        logger.Info("Running Task number {0}", Task.CurrentId);
                        task.Execute();
                        logger.Info("End Task number {0}", Task.CurrentId);
                    }
                    catch (Exception)
                    {
                        logger.Error("Execution error");
                    }
                    finally
                    {
                        threadsEvent[Task.CurrentId].Reset();
                        lock (lockCount)
                        {
                            countActiveThread--;
                        }
                    }
                }
            }

        }

        private ThreadPoolTask SelectTask()
        {
            ThreadPoolTask task = null;
            lock (taskList)
            {
                var waitingTask = taskList.Where(t => !t.IsRun);

                if (waitingTask.Count() > 0)
                {
                    var priorities = (Priority[])Enum.GetValues(typeof(Priority));
                    var ordered = priorities.OrderByDescending(x => x);
                    foreach (var priority in ordered)
                    {
                        var maxPriority = waitingTask.Any(t => t.GetPriority == priority);
                        if (maxPriority)
                        {
                            task = waitingTask.Where(t => t.GetPriority == priority).ToArray().First();
                            DeleteTask(task);
                            break;
                        }
                    }

                }
            }
            return task;
        }

        private void StartTaskOnThread()
        {
            lock (threadList)
            {
                var availableThread = false;
                foreach (var thread in threadList)
                {
                    if (threadsEvent[thread.Id].WaitOne(0) == false)
                    {
                        availableThread = true;
                        threadsEvent[thread.Id].Set();
                        lock (lockCount)
                        {
                            countActiveThread++;
                        }
                        break;
                    }
                }
                if (!availableThread)
                {
                    lock (lockCount)
                    {
                        countActiveThread++;
                    }

                    threadsEvent[CreateThread()].Set();
                }

            }
        }
    }
}
