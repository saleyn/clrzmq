namespace ZeroMQ.SimpleTests
{
    using System;
    using System.Collections.Generic;

    internal class Program
    {
        public static void Main(string[] args)
        {
            List<string> par = new List<string>(args);
            bool inproc = par.Find(x => x == "-inproc") != null;
            bool wait   = par.Find(x => x == "-wait") != null;

            RunTests(
                new HelloWorld(),
                new LatencyBenchmark(inproc),
                new ThroughputBenchmark());

            Console.WriteLine("Finished running tests.");
            if (wait) Console.ReadLine();
        }

        private static void RunTests(params ITest[] tests)
        {
            foreach (ITest test in tests)
            {
                Console.WriteLine("Running test {0}...", test.TestName);
                Console.WriteLine();
                test.RunTest();
                Console.WriteLine();
            }
        }
    }
}
