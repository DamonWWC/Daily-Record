using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace 多线程
{
    internal class ContinuationStateExample
    {
        public static void TaskException()
        {
            // This should throw an UnauthorizedAccessException.
            try
            {
                if (GetAllFiles(@"C:\") is { Length: > 0 } files)
                {
                    foreach (var file in files)
                    {
                        Console.WriteLine(file);
                    }
                }
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    Console.WriteLine(
                        "{0}: {1}", ex.GetType().Name, ex.Message);
                }
            }
            Console.WriteLine();

            // This should throw an ArgumentException.
            try
            {
                foreach (var s in GetAllFiles(""))
                {
                    Console.WriteLine(s);
                }
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                    Console.WriteLine(
                        "{0}: {1}", ex.GetType().Name, ex.Message);
            }
        }

        private static string[] GetAllFiles(string path)
        {
            var task1 =
                Task.Run(() => Directory.GetFiles(
                    path, "*.txt",
                    SearchOption.AllDirectories));

            try
            {
                return task1.Result;
            }
            catch (AggregateException ae)
            {
                ae.Handle(x =>
                {
                    // Handle an UnauthorizedAccessException
                    if (x is UnauthorizedAccessException)
                    {
                        Console.WriteLine(
                            "You do not have permission to access all folders in this path.");
                        Console.WriteLine(
                            "See your network administrator or try another path.");
                    }
                    return x is UnauthorizedAccessException;
                });
                return Array.Empty<string>();
            }
        }


        private static async Task Main()
        {


            var writeOnceBlock = new WriteOnceBlock<string>(null);
          
            //var bufferBlock = new BufferBlock<int>();
            //var post01 = Task.Run(() =>
            //{
            //    bufferBlock.Post(0);
            //    bufferBlock.Post(1);
            //});

            //var receiv = Task.Run(() =>
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        Console.WriteLine(bufferBlock.Receive());
            //    }
            //});
            //var post2 = Task.Run(() =>
            //{
            //    bufferBlock.Post(2);
            //});

            //await Task.WhenAll(post01,  receiv, post2);

            Console.ReadLine();
        }

        private static void Main1()
        {
            byte threshold = 0x40;

            // data is a Task<byte[]>
            var data = Task<byte[]>.Factory.StartNew(() =>
            {
                return GetData();
            });
            var aa = Task.Run(() => { return 1; });
            var bb = aa.ContinueWith(x => { return Task.Run(() => { return 1; }); });
            // We want to return a task so that we can
            // continue from it later in the program.
            // Without Unwrap: stepTwo is a Task<Task<byte[]>>
            // With Unwrap: stepTwo is a Task<byte[]>
            var stepTwo = data.ContinueWith((antecedent) =>
            {
                return Task<byte>.Factory.StartNew(() => Compute(antecedent.Result));
            })
                .Unwrap();

            // Without Unwrap: antecedent.Result = Task<byte>
            // and the following method will not compile.
            // With Unwrap: antecedent.Result = byte and
            // we can work directly with the result of the Compute method.
            var lastStep = stepTwo.ContinueWith((antecedent) =>
            {
                if (antecedent.Result >= threshold)
                {
                    return Task.Factory.StartNew(() => Console.WriteLine("Program complete. Final = 0x{0:x} threshold = 0x{1:x}", stepTwo.Result, threshold));
                }
                else
                {
                    return DoSomeOtherAsynchronousWork(stepTwo.Result, threshold);
                }
            });

            lastStep.Wait();
            Console.WriteLine("Press any key");

            Console.ReadLine();
        }

        #region Dummy_Methods

        private static byte[] GetData()
        {
            Random rand = new Random();
            byte[] bytes = new byte[64];
            rand.NextBytes(bytes);
            return bytes;
        }

        private static Task DoSomeOtherAsynchronousWork(int i, byte b2)
        {
            return Task.Factory.StartNew(() =>
            {
                Thread.SpinWait(500000);
                Console.WriteLine("Doing more work. Value was <= threshold");
            });
        }

        private static byte Compute(byte[] data)
        {
            byte final = 0;
            foreach (byte item in data)
            {
                final ^= item;
                Console.WriteLine("{0:x}", final);
            }
            Console.WriteLine("Done computing");
            return final;
        }

        #endregion Dummy_Methods

        private static DateTime DoWork()
        {
            Thread.Sleep(2000);

            return DateTime.Now;
        }

        private static async Task ContinuationState()
        {
            Task<DateTime> task = Task.Run(() => DoWork());

            var continuations = new List<Task<DateTime>>();
            for (int i = 0; i < 5; i++)
            {
                task = task.ContinueWith((antecedent, _) => DoWork(), DateTime.Now);
                continuations.Add(task);
            }

            await task;

            foreach (Task<DateTime> continuation in continuations)
            {
                DateTime start = (DateTime)continuation.AsyncState!;
                DateTime end = continuation.Result;

                Console.WriteLine($"Task was created at {start.TimeOfDay} and finished at {end.TimeOfDay}.");
            }
        }
    }
}