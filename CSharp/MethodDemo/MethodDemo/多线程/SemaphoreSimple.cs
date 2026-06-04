using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 多线程
{
    internal class SemaphoreSimple
    {
        static Semaphore semaphore = new Semaphore(2, 2);//第一个参数是初始信号量，第二个参数是最大信号量

        static void Main1()
        { 
            // 创建并启动三个任务
            Task[] tasks = new Task[]
            {
            Task.Run(() => PerformWork(1)),
            Task.Run(() => PerformWork(2)),
            Task.Run(() => PerformWork(3)),
             Task.Run(() => PerformWork(4)),
            };

            // 等待所有任务完成
            Task.WaitAll(tasks);

            Console.WriteLine("All phases completed.");

        }

        static void PerformWork(int participantId)
        {
            semaphore.WaitOne();//如果信号量大于0 ，则计数器减1，并允许线程继续执行，若计数器为0，则线程被堵塞并等待其他线程释放信号量。
            try
            {
                Console.WriteLine($"Participant {participantId} ");
                Thread.Sleep(new Random().Next(1000, 3000));
            }
            finally
            {
                semaphore.Release();//增加信号量的计数值，从而允许一个活多个等待中的线程获取信号量并继续执行。
            }
        }
    }
}
