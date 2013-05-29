namespace ZeroMQ.SimpleTests
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal class LatencyBenchmark : ITest
    {
        private const int RoundtripCount = 10000;
        private bool UseInproc = false;

        private static readonly int[] MessageSizes = { 8, 64, 512, 4096, 8192, 16384, 32768 };

        public LatencyBenchmark(bool Inproc = false)
        {
            UseInproc = Inproc;
        }

        public string TestName
        {
            get { return "Latency Benchmark"; }
        }

        public void RunTest()
        {
            var client = new Thread(ClientThread);
            var server = new Thread(ServerThread);

            client.Name = "Client";
            server.Name = "Server";

            server.Start(UseInproc);
            client.Start(UseInproc);

            server.Join(5000);
            client.Join(5000);
        }

        private static void ClientThread(object UseInproc)
        {
            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.REQ))
            {
                socket.Connect((bool)UseInproc ? "inproc://abc" : "tcp://localhost:9000");

                foreach (int messageSize in MessageSizes)
                {
                    var msg = new byte[messageSize];
                    var reply = new byte[messageSize];

                    var watch = new Stopwatch();
                    watch.Start();

                    for (int i = 0; i < RoundtripCount; i++)
                    {
                        SendStatus sendStatus = socket.Send(msg);

                        Debug.Assert(sendStatus == SendStatus.Sent, "Message was not indicated as sent.");

                        int bytesReceived = socket.Receive(reply);

                        Debug.Assert(bytesReceived == messageSize, "Pong message did not have the expected size.");
                    }

                    watch.Stop();
                    long elapsedTime = watch.ElapsedTicks;

                    double latency = (double)elapsedTime / RoundtripCount / 2 * 1000000 / Stopwatch.Frequency;
                    Console.WriteLine("Roundtrips: {0}, MsgSz: {1,5}, Latency: {2,6} us",
                        RoundtripCount, messageSize, latency.ToString("f2"));
                }
            }
        }

        private static void ServerThread(object UseInproc)
        {
            using (var context = ZmqContext.Create())
            using (var socket = context.CreateSocket(SocketType.REP))
            {
                socket.Bind((bool)UseInproc ? "inproc://abc" : "tcp://*:9000");

                foreach (int messageSize in MessageSizes)
                {
                    var message = new byte[messageSize];

                    for (int i = 0; i < RoundtripCount; i++)
                    {
                        int receivedBytes = socket.Receive(message);

                        Debug.Assert(receivedBytes == messageSize, "Ping message length did not match expected value.");

                        SendStatus sendStatus = socket.Send(message);

                        Debug.Assert(sendStatus == SendStatus.Sent, "Message was not indicated as sent.");
                    }
                }
            }
        }
    }
}
