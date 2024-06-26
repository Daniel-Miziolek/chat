using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System.Windows.Input;

namespace chat1
{
    public partial class MainWindow : Window
    {
        static TcpClient client;
        static TcpListener server;
        static NetworkStream stream;
        public static MainWindow Instance { get; private set; }
        public ObservableCollection<ChatMessage> Messages { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            if (Instance == null)
            {
                Instance = this;
            }

            Messages = new ObservableCollection<ChatMessage>();
            listBox1.ItemsSource = Messages;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string ip = "127.0.0.1";
            ConnectToServer(ip);
        }

        private void ConnectToServer(string ip)
        {
            try
            {
                client = new TcpClient();
                client.Connect(IPAddress.Parse(ip), 8888);
                MessageBox.Show("Connected to server", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                var receiveThread = new Thread(ReceiveMessages);
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendMessage(string message)
        {
            if (client == null)
            {
                MessageBox.Show("Not connected to any server.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                stream = client.GetStream();
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);

                Dispatcher.Invoke(() =>
                {
                    Messages.Add(new ChatMessage { Message = message, IsUserMessage = true });
                    textBox2.Clear();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sending message failed: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                stream = client.GetStream();
                while (client != null && client.Connected)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    Dispatcher.Invoke(() =>
                    {
                        Messages.Add(new ChatMessage { Message = message, IsUserMessage = message.StartsWith(user.Content.ToString()) });
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Receiving message failed: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            string ip = "127.0.0.1";
            CreateServer(ip);
        }

        private void CreateServer(string ip)
        {
            try
            {
                server = new TcpListener(IPAddress.Parse(ip), 8888);
                server.Start();

                MessageBox.Show("Server started... Waiting for a client to connect...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

                var acceptClientThread = new Thread(() =>
                {
                    client = server.AcceptTcpClient();
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Client connected...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    });

                    var receiveThread = new Thread(ReceiveMessages);
                    receiveThread.Start();
                });

                acceptClientThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Server creation failed: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        private void set_Click(object sender, RoutedEventArgs e)
        {
            settings seti = new settings();
            seti.Show();
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            Messages.Clear();
        }

        private void dis_Click(object sender, RoutedEventArgs e)
        {
            DisconnectClient();
        }

        private void DisconnectClient()
        {
            try
            {
                if (client != null)
                {
                    stream.Close();
                    client.Close();
                    client = null;

                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Disconnected from server", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Disconnection failed: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string message = user.Content + " " + DateTime.Now.ToString("HH:mm") + "\n" + textBox2.Text;
                SendMessage(message);
            }
        }
    }

    public class ChatMessage
    {
        public string Message { get; set; }
        public bool IsUserMessage { get; set; }
    }
}
