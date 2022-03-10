using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LabZero
{
    class Program
    {
        private const string host = "127.0.0.1";
        private static int port;
        private static int currentPort;
        private static Socket udpSocket;
        private static string currentUser;

        static void Main(string[] args)
        {
            Console.WriteLine("What is your name?");
            currentUser = Console.ReadLine();
            
            InputPorts();

            TryToConnect();

            MakeChat();
        }

        private static void InputPorts()
        {
            while (true)
            {
                Console.WriteLine("Write listening port:");
                if (!int.TryParse(Console.ReadLine(), out int listeningPort) && listeningPort > 5000 && listeningPort < 10000)
                {
                    Console.WriteLine("Incorrect listening port.");
                    continue;
                }

                Console.WriteLine("Write connection port:");
                if (!int.TryParse(Console.ReadLine(), out int connectionPort) && connectionPort > 5000 && connectionPort < 10000)
                {
                    Console.WriteLine("Incorrect connection port.");
                    continue;
                }

                port = listeningPort;
                currentPort = connectionPort;
                break;
            }
        }

        private static void TryToConnect()
        {
            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));

            while (true)
            {
                try
                {
                    tcpSocket.Connect(new IPEndPoint(IPAddress.Parse(host), currentPort));
                    break;
                }
                catch (SocketException)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Waiting for the connection...");
                }
            }
            
            Console.WriteLine("Successfully connected.");
        }

        private static void MakeChat()
        {
            try
            {
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                new Task(ListenMessages).Start();

                while (true)
                {
                    string message = Console.ReadLine();
                    byte[] data = Encoding.Unicode.GetBytes($"{currentUser}: {message}");
                    udpSocket.SendTo(data, new IPEndPoint(IPAddress.Parse(host), currentPort));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection was interrupted.");
            }
            finally
            {
                CloseUdp();
            }
        }

        public static void ListenMessages()
        {
            try
            {
                udpSocket.Bind(new IPEndPoint(IPAddress.Parse(host), port));

                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    EndPoint endPoint = new IPEndPoint(IPAddress.Parse(host), currentPort);

                    do
                    {
                        var data = new byte[64];
                        var count = udpSocket.ReceiveFrom(data, ref endPoint);

                        builder.Append(Encoding.Unicode.GetString(data, 0, count));

                    } while (udpSocket.Available > 0);

                    Console.WriteLine(builder.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection was interrupted.");
            }
            finally
            {
                CloseUdp();
            }
        }

        private static void CloseUdp()
        {
            if (udpSocket != null)
            {
                udpSocket.Shutdown(SocketShutdown.Both);
                udpSocket.Close();
                udpSocket = null;
            }
        }
    }
}
