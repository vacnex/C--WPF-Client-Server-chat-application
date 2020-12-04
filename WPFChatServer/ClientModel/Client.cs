using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WPFChatServer.ClientModel
{
    public class Client
    {
        String hostname;
        String ip;

        public Client(){}
        public Client(string hostname, String ip)
        {
            Hostname = hostname;
            Ip = ip;
        }
        public Client(string hostname)
        {
            Hostname = hostname;
        }

        public string Hostname { get => hostname; set => hostname = value; }
        public String Ip { get => ip; set => ip = value; }

        public override string ToString()
        {
            return this.Hostname + " " + this.Ip;
        }
    }
}
