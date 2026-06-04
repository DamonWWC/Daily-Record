using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 多线程
{
    internal class ParallelSample
    {
        static void Main1()
        {
            ParallelCancel();
            Console.ReadKey();

        }

        static void ParallelFor()
        {
            int[] nums = Enumerable.Range(0, 1_000_000).ToArray();
            long total = 0;
            Parallel.For<long>(0, nums.Length, () => 0,
                (j, loop, subtotal) =>
                {
                    subtotal += nums[j];
                    return subtotal;
                },
                subtotal => Interlocked.Add(ref total, subtotal));

            Console.WriteLine("The total is {0:N0}", total);
        }

        static void ParallelForeach()
        {
            int[] nums = Enumerable.Range(0, 1000000).ToArray();
            long total = 0;

            // First type parameter is the type of the source elements
            // Second type parameter is the type of the thread-local variable (partition subtotal)
            Parallel.ForEach<int, long>(
                nums, // source collection
                () => 0, // method to initialize the local variable
                (j, loop, subtotal) => // method invoked by the loop on each iteration
                {
                    subtotal += j; //modify local variable
                    return subtotal; // value to be passed to next iteration
                },
                // Method to be executed when each partition has completed.
                // finalResult is the final value of subtotal for a particular partition.
                (finalResult) => Interlocked.Add(ref total, finalResult));

            Console.WriteLine("The total from Parallel.ForEach is {0:N0}", total);
        }

        static void ParallelCancel()
        {
            int[] nums = Enumerable.Range(0, 10_000_000).ToArray();
            CancellationTokenSource cts = new();

            // Use ParallelOptions instance to store the CancellationToken
            ParallelOptions options = new()
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Console.WriteLine("Press any key to start. Press 'c' to cancel.");
            Console.ReadKey();

            // Run a task so that we can cancel from another thread.
            Task.Factory.StartNew(() =>
            {
                if (Console.ReadKey().KeyChar is 'c')
                    cts.Cancel();
                Console.WriteLine("press any key to exit");
            });

            try
            {
                Parallel.ForEach(nums, options, (num) =>
                {
                    double d = Math.Sqrt(num);
                    Console.WriteLine("{0} on {1}", d, Environment.CurrentManagedThreadId);
                });
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                cts.Dispose();
            }
        }
    }
}
