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
using WPFChatServer.ClientModel;

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
            }
        }
        
        /// <summary>
        /// Hàm khởi tạo kết nối
        /// </summary>
        /// <param name="ipaddress">Truyền vào địa chỉ IP</param>
        private void Connect(IPAddress ipaddress)
        {
            IP = new IPEndPoint(ipaddress, 9999);
            sClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            IPHost = Dns.GetHostEntry(Dns.GetHostName());
            try
            {
                sClient.Connect(IP);
                Growl.SuccessGlobal("Ket Noi thanh cong");
                foreach (var item in IPHost.AddressList)
                {
                    if (item.AddressFamily == AddressFamily.InterNetwork)
                        sClient.Send(Serialize(item.ToString()));
                }
                sClient.Send(Serialize(IPHost.HostName.ToString()+" da ket noi"));
            }
            catch (Exception)
            {
                return;
            }
            Thread t = new Thread(Recieve);
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// Hàm đóng kết nối
        /// </summary>
        private void Close()
        {
            sClient.Close();
        }


        /// <summary>
        /// Hàm gửi
        /// </summary>
        private void Send()
        {
            sClient.Send(Serialize(IPHost.HostName.ToString() + ": " + MesssageSend.Text));
        }


        /// <summary>
        /// Hàm nhận
        /// </summary>
        private void Recieve()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    sClient.Receive(data);
                    String message = (String)Deserialize(data);
                    if (!message.Contains("\0"))
                    {
                        if (isListIpAddress(message))
                            UpdateClient(message);
                        else
                            AddMessage(message);
                    }
                }
            }
            catch (Exception)
            {
                Close();
            }
        }

        private void UpdateClient(string message)
        {
            this.Dispatcher.Invoke(() => {
                List<String> ListIpClient = message.Split(',').ToList();
                ListIpClient.RemoveAll(x => string.IsNullOrEmpty(x));
                ListClients.Items.Clear();
                for (int i = 0; i < ListIpClient.Count; i++)
                {
                    var client = new Client(GetHostnameFromIP(ListIpClient[i]), ListIpClient[i]);
                    if (client.Ip != sClient.LocalEndPoint.ToString().Substring(0, sClient.LocalEndPoint.ToString().IndexOf(':')))
                        ListClients.Items.Add(client);
                }
                
            });
        }

        /// <summary>
        /// Kiểm tra message gửi về có phải là chuỗi các ip của client đã kết nối
        /// </summary>
        /// <param name="message">Message gửi về</param>
        /// <returns>Trả về truê nếu message đó là danh sách ip, ngược lài là false</returns>
        private bool isListIpAddress(string message)
        {
            return message.Contains(",");
        }

        /// <summary>
        /// Hàm lấy hostname từ ip
        /// </summary>
        /// <param name="ip">Truyền vào địa chỉ ip</param>
        /// <returns>Trả về Hostname của ip</returns>
        private string GetHostnameFromIP(string ip)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(ip);
            return hostEntry.HostName;
        }


        /// <summary>
        /// Hàm thêm message vào listbox
        /// </summary>
        /// <param name="message">Truyền vào massage</param>
        private void AddMessage(String message)
        {
            this.Dispatcher.Invoke(() => {
                Message.Items.Add(message);
                MesssageSend.Clear();
                Message.Items.MoveCurrentToLast();
                Message.ScrollIntoView(Message.Items.CurrentItem);
            });
            
        }

        /// <summary>
        /// Hàm chuyển đổi đối tượng thành mảng byte
        /// </summary>
        /// <param name="obj">Truyền vào object</param>
        /// <returns>Trả về mảng byte của object</returns>
        private byte[] Serialize(Object obj)
        {
            ms = new MemoryStream();
            BinaryFormatter fn = new BinaryFormatter();
            fn.Serialize(ms, obj);
            return ms.ToArray();
        }

        /// <summary>
        /// Hàm chuyển đổi mảng byte về string
        /// </summary>
        /// <param name="data">Truyền mảng byte</param>
        /// <returns></returns>
        private Object Deserialize(byte[] data)
        {
            ms = new MemoryStream(data);
            BinaryFormatter fn = new BinaryFormatter();
            return fn.Deserialize(ms);
        }
        /// <summary>
        /// Hàm đóng kết nối khi cửa sổ đóng.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
        }
    }
}
