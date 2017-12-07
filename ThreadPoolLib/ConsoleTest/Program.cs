using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreadPoolLib;

namespace ConsoleTest
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            ThreadPool pool = null;
            try
            {
                pool = new ThreadPool(1,1);
            }
            catch (Exception)
            {
                Console.ReadLine();
                return;
            }

            var test = new ThreadPoolTask(() =>
           {
               System.Threading.Thread.Sleep(1000);
               //Console.WriteLine("low priority {0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);
               logger.Info("Task done low priority ");
           }, Priority.Low);
            var test1 = new ThreadPoolTask(() =>
            {
                System.Threading.Thread.Sleep(1000);
                logger.Info("Task done high priority ");
                //throw new Exception("test");
            }, Priority.High);
            var test2 = new ThreadPoolTask(() =>
            {
                System.Threading.Thread.Sleep(1000);
                logger.Info("Task done normal priority ");
            }, Priority.Normal);
            pool.Execute(test);
            pool.Execute(test1);
            pool.Execute(test2);

            System.Threading.Thread.Sleep(150);
            //Console.WriteLine("new task");
            pool.ExecuteRange(new [] { test, test1, test2 });
            //System.Threading.Thread.Sleep(2000);
            pool.Stop();
            Console.ReadLine();
        }
    }
}
