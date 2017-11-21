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
            var pool = new ThreadPool(3,3);
            //pool.Start();
            var test = new TaskW(() =>
           {
               Console.WriteLine("test");
           });
            var test1 = new TaskW(() =>
            {
                Console.WriteLine("test11");
            });
            pool.Execute(test);
            pool.Execute(test1);
            //pool.Start();
            Console.ReadLine();
        }
    }
}
