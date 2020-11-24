using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFChatServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MemoryStream ms;
        Socket sServer;
        IPEndPoint IP;
        List<Socket> listclient;
        IPHostEntry IPHost;
        public MainWindow()
        {
            InitializeComponent();
            Message.Items.Add("Chat Server");
            Connect();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(MesssageSend.Text))
            {
                foreach (Socket item in listclient)
                {
                    Send(item);
                }
                //Message.Items.MoveCurrentToLast();
                //Message.ScrollIntoView(Message.Items.CurrentItem);

            }
            
        }
        private void Connect()
        {
            listclient = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, 9999);
            sServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            IPHost = Dns.GetHostEntry(Dns.GetHostName());



            sServer.Bind(IP);
            Thread t = new Thread(() => {
                try
                {
                    while (true)
                    {
                        sServer.Listen(10);
                        Socket client = sServer.Accept();
                        listclient.Add(client);

                        Thread r = new Thread(Recieve);
                        r.IsBackground = true;
                        r.Start(client);
                    }
                }
                catch (Exception)
                {
                    IP = new IPEndPoint(IPAddress.Any, 4354);
                    sServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
                
            });
            t.IsBackground = true;
            t.Start();
            
        }

        private void Close()
        {
            sServer.Close();
        }

        private void Send(Socket client)
        {
            if (!MesssageSend.Equals(null))
            {
                client.Send(Serialize(IPHost.HostName.ToString() + ": " + MesssageSend.Text));
                AddMessage(IPHost.HostName.ToString() + ": " + MesssageSend.Text);
            }

        }

        private void Recieve(Object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    client.Receive(data);
                    String message = (String)Deserialize(data);
                    AddMessage(message);
                }
            }
            catch (Exception)
            {
                listclient.Remove(client);
                client.Close();
            }
        }

        private void AddMessage(string message)
        {
            this.Dispatcher.Invoke(() => {
                Message.Items.Add(message);
                MesssageSend.Clear();
                Message.Items.MoveCurrentToLast();
                Message.ScrollIntoView(Message.Items.CurrentItem);
            });
        }

        private byte[] Serialize(Object obj)
        {
            ms = new MemoryStream();
            BinaryFormatter fn = new BinaryFormatter();
            fn.Serialize(ms, obj);
            return ms.ToArray();
        }

        private Object Deserialize(byte[] data)
        {
            ms = new MemoryStream(data);
            BinaryFormatter fn = new BinaryFormatter();
            return fn.Deserialize(ms);
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
        }
    }
}
