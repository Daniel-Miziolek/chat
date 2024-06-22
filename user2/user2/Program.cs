using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    static TcpClient client;
    static TcpListener server;
    static NetworkStream stream;

    static void Main(string[] args)
    {
        while (true)
        {
            string command = Console.ReadLine();
            if (command.StartsWith("!connect "))
            {
                string ip = command.Split(' ')[1];
                ConnectToServer(ip);
            }
            else if (command.StartsWith("!create "))
            {
                string ip = command.Split(' ')[1];
                CreateServer(ip);
            }
            else
            {
                Console.WriteLine("Unknown command -> " + command);
            }
        }
    }

    static void ConnectToServer(string ip)
    {
        try
        {
            client = new TcpClient();
            client.Connect(IPAddress.Parse(ip), 8888);
            Console.WriteLine("Connected to server...");

            var receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            while (true)
            {
                string message = Console.ReadLine();
                SendMessage(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void CreateServer(string ip)
    {
        try
        {
            server = new TcpListener(IPAddress.Parse(ip), 8888);
            server.Start();
            Console.WriteLine("Server started... Waiting for a client to connect...");

            client = server.AcceptTcpClient();
            Console.WriteLine("Client connected...");

            var receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            while (true)
            {
                string message = Console.ReadLine();
                SendMessage(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void SendMessage(string message)
    {
        try
        {
            stream = client.GetStream();
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    static void ReceiveMessages()
    {
        try
        {
            stream = client.GetStream();
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: " + message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
