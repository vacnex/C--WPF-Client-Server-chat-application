﻿using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
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
using MessageBox = HandyControl.Controls.MessageBox;

namespace WPFChatServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
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
            //List<Client> customerClientList = new List<Client>();
            Client client = (Client)ListClients.SelectedItem;
            List<Client> SelectedClientList = ListClients.SelectedItems.Cast<Client>().ToList();

            if (!String.IsNullOrWhiteSpace(MesssageSend.Text) && ListClients.SelectedItem !=null)
            {
                //if (client != null && SelectedClientList.Count == 1)
                //{
                int i =0;
                foreach (Socket item in listclient)
                {

                    if (SelectedClientList[i].Ip.Equals(item.RemoteEndPoint.ToString().Substring(0, item.RemoteEndPoint.ToString().IndexOf(':'))))
                        {
                            Send(item, MesssageSend.Text);
                        }
                    i++;
                    //}
                    
                   
                }
                //if (ListClients.SelectedItems.Count > 1)
                //{
                //    for (int i = 0; i < ListClients.SelectedItems.Count; i++)
                //    {
                //        customerClientList.Add((Client)ListClients.SelectedItems[i]);
                //    }
                //    foreach (Socket item in listclient)
                //    {

                //    }
                //}
                //foreach (Socket item in listclient)
                //{
                //    if (ListClients.SelectedItems.Count > 1)
                //    {
                //        Client itemclient = new Client();
                //        if (itemclient.Ip.Equals(item.RemoteEndPoint.ToString().Substring(0, item.RemoteEndPoint.ToString().IndexOf(':'))))
                //        {

                //        }
                //        //foreach (Client itemclient in customerClientList)
                //        //{
                //        //    if (itemclient.Ip.Equals(item.RemoteEndPoint.ToString().Substring(0, item.RemoteEndPoint.ToString().IndexOf(':'))))
                //        //    {
                //        //        Send(item);
                //        //    }
                //        //}
                //    }
                //    else
                //    {
                //        if (client.Ip.Equals(item.RemoteEndPoint.ToString().Substring(0, item.RemoteEndPoint.ToString().IndexOf(':'))))
                //        {
                //            Send(item);
                //        }
                //    }

                //if (client.Ip.Equals(item.RemoteEndPoint.ToString().Substring(0, item.RemoteEndPoint.ToString().IndexOf(':'))))
                //{
                //    Send(item);
                //}
            }else
            {
                Growl.WarningGlobal("chưa nhập tin nhắn hoặc chưa chọn người cần gửi");
            }

        }
        /// <summary>
        /// Hàm khởi tạo kết nối
        /// </summary>
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
                    IP = new IPEndPoint(IPAddress.Any, 9999);
                    sServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
                
            });
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// Hàm đóng kết nối
        /// </summary>
        private void Close()
        {
            sServer.Close();
        }

        /// <summary>
        /// Hàm gửi message
        /// </summary>
        /// <param name="client">truyền vào Socket cần gửi</param>
        private void Send(Socket client, String message)
        {
            if (!MesssageSend.Equals(null))
            {
                client.Send(Serialize(IPHost.HostName.ToString() + ": " + message));
                AddMessage(IPHost.HostName.ToString() + ": " + message);
            }
        }

        /// <summary>
        /// Hạm nhận
        /// </summary>
        /// <param name="obj">Truyền vào Object nhận được</param>
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
                    //AddMessage(message);
                    if (isIpAddress(message))
                        AddClient(message);
                    else
                        AddMessage(message);
                }
            }
            catch (Exception)
            {
                listclient.Remove(client);
                client.Close();
            }
        }
        
        /// <summary>
        /// Hàm lấy hostname từ ip
        /// </summary>
        /// <param name="ip">Truyền vào địa chỉ ip</param>
        /// <returns>Trả về Hostname của ip</returns>
        private String GetHostnameFromIP(String ip)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(ip);
            return hostEntry.HostName;
        }

        /// <summary>
        /// Hàm kiểm tra IP
        /// </summary>
        /// <param name="iphost">Truyền vào IP</param>
        /// <returns>Trả về true/false</returns>
        private bool isIpAddress (String iphost)
        {
            IPAddress ip;
            return IPAddress.TryParse(iphost, out ip);
        }

        /// <summary>
        /// Hàm thêm client 
        /// </summary>
        /// <param name="message">truyền vào massage chứa client</param>
        private void AddClient(String message)
        {
            this.Dispatcher.Invoke(() => {
                var client = new Client(GetHostnameFromIP(message), message);
                ListClients.Items.Add(client);
            });
        }

        /// <summary>
        /// Hàm thêm message vào listbox
        /// </summary>
        /// <param name="message">Truyền vào message</param>
        private void AddMessage(string message)
        {
            this.Dispatcher.Invoke(() => {
                Message.Items.Add(message);
                //MesssageSend.Clear();
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
        /// <returns>Trả về giá trị của mảng byte truyền vào</returns>
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
