using HandyControl.Controls;
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

namespace WPFChatClientServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        MemoryStream ms;
        Socket sClient;
        IPEndPoint IP;
        IPHostEntry IPHost;

        public MainWindow()
        {
            InitializeComponent();
            Message.Items.Add("Chat Client");

        }
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
                Connect(IPAddress.Parse(ipbox.Text));
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
                Send();
                AddMessage(IPHost.HostName.ToString() + ": " + MesssageSend.Text);
                //Message.Items.MoveCurrentToLast();
                //Message.ScrollIntoView(Message.Items.CurrentItem);
            }
            
        }

        private void Connect(IPAddress ipaddress)
        {
            IP = new IPEndPoint(ipaddress, 9999);
            sClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            IPHost = Dns.GetHostEntry(Dns.GetHostName());
            try
            {
                sClient.Connect(IP);
                Growl.SuccessGlobal("Ket Noi thanh cong");
                sClient.Send(Serialize("Client " + IPHost.HostName.ToString() + " da vao"));
            }
            catch (Exception e )
            {
                Growl.ErrorGlobal("Ket Noi khong thanh cong");
                return;
            }
            Thread t = new Thread(Recieve);
            t.IsBackground = true;
            t.Start();
        }

        private void Close()
        {
            sClient.Close();
        }

        private void Send()
        {
            sClient.Send(Serialize(IPHost.HostName.ToString() + ": " + MesssageSend.Text));
        }

        private void Recieve()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    sClient.Receive(data);
                    String message = (String)Deserialize(data);
                    if (!message.Contains("\0")) ;
                    {
                        AddMessage(message);
                    }
                    
                }
            }
            catch (Exception e)
            {
                Close();
            }
        }

        private void AddMessage(String message)
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
