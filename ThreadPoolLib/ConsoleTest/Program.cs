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
        static void Main(string[] args)
        {
            ThreadPool pool = null;
            try
            {
                pool = new ThreadPool(1,3);
            }
            catch (Exception)
            {
                Console.ReadLine();
                return;
            }

            var test = new ThreadPoolTask(() =>
           {
               System.Threading.Thread.Sleep(100);
               Console.WriteLine("low priority {0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);
           }, Priority.Low);
            var test1 = new ThreadPoolTask(() =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine("high priority {0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);               
            }, Priority.High);
            var test2 = new ThreadPoolTask(() =>
            {
                System.Threading.Thread.Sleep(100);
                Console.WriteLine("normal priority {0} ", System.Threading.Thread.CurrentThread.ManagedThreadId);                
            }, Priority.Normal);
            pool.Execute(test);
            pool.Execute(test1);
            pool.Execute(test2);

            System.Threading.Thread.Sleep(50);
            Console.WriteLine("new task");
            pool.ExecuteRange(new [] { test, test1, test2 });
            //System.Threading.Thread.Sleep(2000);
            pool.Stop();
            Console.ReadLine();
        }
    }
}
