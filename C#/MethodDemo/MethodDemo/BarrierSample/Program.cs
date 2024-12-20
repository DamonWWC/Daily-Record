using System.Text;

namespace BarrierSample
{
    internal class Program
    {
        static string[] words1 = new string[] { "brown", "jumps", "the", "fox", "quick" };
        static string[] words2 = new string[] { "dog", "lazy", "the", "over" };
        static string solution = "the quick brown fox jumps over the lazy dog.";

        static bool success = false;
        static Barrier barrier = new Barrier(2, (b) =>
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < words1.Length; i++)
            {
                sb.Append(words1[i]);
                sb.Append(" ");
            }
            for (int i = 0; i < words2.Length; i++)
            {
                sb.Append(words2[i]);

                if (i < words2.Length - 1)
                    sb.Append(" ");
            }
            sb.Append(".");
#if TRACE
            System.Diagnostics.Trace.WriteLine(sb.ToString());
#endif
            Console.CursorLeft = 0;
            Console.Write("Current phase: {0}", barrier.CurrentPhaseNumber);
            if (String.CompareOrdinal(solution, sb.ToString()) == 0)
            {
                success = true;
                Console.WriteLine("\r\nThe solution was found in {0} attempts", barrier.CurrentPhaseNumber);
            }
        });
        static void Solve(string[] wordArray)
        {
            while (success == false)
            {
                Random random = new Random();
                for (int i = wordArray.Length - 1; i > 0; i--)
                {
                    int swapIndex = random.Next(i + 1);
                    string temp = wordArray[i];
                    wordArray[i] = wordArray[swapIndex];
                    wordArray[swapIndex] = temp;
                }

                // We need to stop here to examine results
                // of all thread activity. This is done in the post-phase
                // delegate that is defined in the Barrier constructor.
                barrier.SignalAndWait();
            }
        }
        private static void Main(string[] args)
        {
            Thread t1 = new Thread(() => Solve(words1));
            Thread t2 = new Thread(() => Solve(words2));
            t1.Start();
            t2.Start();

            // Keep the console window open.
            Console.ReadLine();


            //Barrier barrier = new Barrier(3, b => Console.WriteLine($"All parts have reached the barrier at phase: {b.CurrentPhaseNumber + 1}"));

            //Task[] tasks = new Task[3];

            //for (int i = 0; i < tasks.Length; i++)
            //{
            //    int taskId = i;
            //    tasks[i] = Task.Run(() =>
            //    {
            //        Console.WriteLine($"Task {taskId} is starting...");
            //        Thread.Sleep(1000); // Simulate work
            //        barrier.SignalAndWait(); // Wait for all tasks to reach the barrier
            //        Console.WriteLine($"Task {taskId} is continuing...");
            //        Thread.Sleep(1000); // Simulate more work
            //    });
            //}

            //Task.WaitAll(tasks);

            //const int numberTasks = 2;
            //const int partitonSize = 1000;
            //const int loops = 5;
            //var taskResults = new Dictionary<int, int[][]>();
            //var data = new List<string>[loops];
            //for (int i = 0; i < loops; i++)
            //{
            //    data[i] = new List<string>(FillData(partitonSize * numberTasks));
            //}
            //var barrier = new Barrier(numberTasks + 1);
            //LogBarrierInformation("initial participants in barrier", barrier);
            //for (int i = 0; i < numberTasks; i++)
            //{
            //    barrier.AddParticipant();
            //    int jobNumber = i;
            //    taskResults.Add(i, new int[loops][]);
            //    for (int loop = 0; loop < loops; loop++)
            //    {
            //        taskResults[i][loop] = new int[26];
            //    }
            //    Console.WriteLine($"Main - starting task job {jobNumber}");
            //    Task.Run(() =>
            //    {
            //        CalculationInTask(jobNumber, partitonSize, barrier, data, loops, taskResults[jobNumber]);
            //    });
            //}

            //for (int loop = 0; loop < 5; loop++)
            //{
            //    LogBarrierInformation("main task, start signaling and wait", barrier);
            //    barrier.SignalAndWait();
            //    LogBarrierInformation("main task waiting completed", barrier);
            //    int[][] resultCollection1 = taskResults[0];
            //    int[][] resultCollection2 = taskResults[1];
            //    var resultCollection = resultCollection1[loop].Zip(
            //        resultCollection2[loop], (c1, c2) => c1 + c2);
            //    char ch = 'a';
            //    int sum = 0;
            //    foreach (var x in resultCollection)
            //    {
            //        Console.WriteLine($"{ch++},count: {x}");
            //        sum += x;
            //    }
            //    LogBarrierInformation($"main task finished loop {loop}, num:{sum}", barrier);
            //}

            //Console.WriteLine("finished all iterations");
            Console.ReadLine();
        }

        private static void CalculationInTask(int jobNumber, int partitionSize, Barrier barrier, IList<string>[] coll, int loops, int[][] results)
        {
            LogBarrierInformation("CalculationInTask started", barrier);
            for (int i = 0; i < loops; i++)
            {
                var data = new List<string>(coll[i]);
                int start = jobNumber * partitionSize;
                int end = start * partitionSize;
                Console.WriteLine($"Task {Task.CurrentId} in loop {i} :partition " +
                    $"from {start} to {end}");
                for (int j = start; j < end; j++)
                {
                    char c = data[j][0];
                    results[i][c - 97]++;
                }
                Console.WriteLine($"Calculation completed from task {Task.CurrentId} " +
                    $"in loop {i}. {results[i][0]} times a, {results[i][1]} times z");
                LogBarrierInformation("send signal and wait for all", barrier);
                barrier.SignalAndWait();
                LogBarrierInformation("waiting completed", barrier);
            }
            barrier.RemoveParticipant();
            LogBarrierInformation("finished task,, removed participant", barrier);
        }

        public static IEnumerable<string> FillData(int size)
        {
            var r = new Random();
            return Enumerable.Range(0, size).Select(x => GetString(r));
        }

        private static string GetString(Random r)
        {
            var sb = new StringBuilder(6);
            for (int i = 0; i < 6; i++)
            {
                sb.Append((char)(r.Next(26) + 97));
            }
            return sb.ToString();
        }

        private static void LogBarrierInformation(string info, Barrier barrier)
        {
            Console.WriteLine($"Task {Task.CurrentId}:{info}. " +
                $"{barrier.ParticipantCount} current and " +
                $"{barrier.ParticipantsRemaining} remaining participants, " +
                $"phase {barrier.CurrentPhaseNumber}");
        }
    }
}