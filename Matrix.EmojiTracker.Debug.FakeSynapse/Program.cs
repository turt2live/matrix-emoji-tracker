using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Matrix.EmojiTracker.Debug.FakeSynapse
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] welcome = Encoding.UTF8.GetBytes("SERVER localhost\n");

            var lines = File.ReadAllLines("raw-rdata.txt");
            byte[][] messages = lines.Select(k => Encoding.UTF8.GetBytes(k + "\n")).ToArray();

            TcpListener server = new TcpListener(IPAddress.Loopback, 9092);
            server.Start();

            int c = 0;
            object sync = new object();
            Task.Factory.StartNew(async () =>
            {
                var lastStart = DateTime.Now;
                while (true)
                {
                    lock (sync)
                    {
                        var end = DateTime.Now;
                        var ms = (end - lastStart).TotalMilliseconds;
                        Console.WriteLine("Sent {0} events in {1}ms", c, ms);
                        lastStart = end;
                        c = 0;
                    }

                    await Task.Delay(1000);
                }
            });

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    var stream = client.GetStream();

                    Console.WriteLine("Sending hello");
                    stream.Write(welcome, 0, welcome.Length);

                    int i = 0;
                    while (client.Connected)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            var msg = messages[i];
                            stream.Write(msg, 0, msg.Length);
                            i++;
                            if (i >= messages.Length) i = 0;
                            lock (sync) c++;
                        }

                        await Task.Delay(1);
                    }
                }
            });

            Console.ReadKey(true);
        }
    }
}
