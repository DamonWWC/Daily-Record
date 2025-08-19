using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 多线程
{
    internal class BarrierSimple
    {
        static Barrier barrier = new Barrier(3, (b) =>
        {
            Console.WriteLine($"Phase {b.CurrentPhaseNumber} completed.");
        });

        public static void Main1()
        {
            // 创建并启动三个任务
            Task[] tasks = new Task[]
            {
            Task.Run(() => PerformWork(1)),
            Task.Run(() => PerformWork(2)),
            Task.Run(() => PerformWork(3))
            };

            // 等待所有任务完成
            Task.WaitAll(tasks);

            Console.WriteLine("All phases completed.");
        }

        static void PerformWork(int participantId)
        {
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine($"Participant {participantId} is working on phase {i}.");
                Thread.Sleep(new Random().Next(1000, 3000)); // 模拟工作

                barrier.SignalAndWait(); // 信号并等待其他参与者
            }

            Console.WriteLine($"Participant {participantId} has completed all phases.");
        }
    }
}