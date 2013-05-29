namespace ZeroMQ.Perf.LatRemote
{
    using System;
    using System.Diagnostics;

    using ZeroMQ;

    internal class Program
    {
        internal static int Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.Out.WriteLine("usage: remote_lat <address> <message-size> <roundtrip-count>\n");
                return 1;
            }

            string address = args[0];
            int messageSize = Convert.ToInt32(args[1]);
            int roundtripCount = Convert.ToInt32(args[2]);

            if (messageSize <= 0 || roundtripCount <= 0)
            {
                Console.Error.WriteLine("message-size and roundtrip-count must be positive values.");
                return 1;
            }

            // Initialize 0MQ infrastructure
            using (ZmqContext ctx = ZmqContext.Create())
            using (ZmqSocket skt = ctx.CreateSocket(SocketType.REQ))
            {
                skt.Connect(address);

                // Create a message to send.
                var msg = new byte[messageSize];

                Stopwatch watch = Stopwatch.StartNew();

                // Start sending messages.
                for (int i = 0; i < roundtripCount; i++)
                {
                    skt.Send(msg);

                    int receivedBytes;
                    msg = skt.Receive(msg, out receivedBytes);

                    Debug.Assert(receivedBytes == messageSize, "Received message did not have the expected length.");
                }

                watch.Stop();
                long elapsedUsec = watch.ElapsedTicks * 1000000 / Stopwatch.Frequency;

                double latency = (double)elapsedUsec / roundtripCount / 2;
                long   trans   = roundtripCount * 1000000 / elapsedUsec;

                Console.WriteLine("Roundtrips: {0}, MsgSz: {1,5}, Latency: {2,6} us, Trans/s: {3,6}",
                    roundtripCount, messageSize, latency.ToString("f2"), trans);
            }

            return 0;
        }
    }
}
