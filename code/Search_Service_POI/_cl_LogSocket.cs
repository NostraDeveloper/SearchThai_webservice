using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GT_Sockets;
using System.Globalization;
namespace Search_Service_POI
{
    public class _cl_LogSocket
    {
        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
        string IP = "";
        int PORT = 0;
        public _cl_LogSocket(int port, string ip)
        {
            IP = ip;
            PORT = port;
        } 

        public bool SendToEngine(string message)
        {
            try
            {
                GT_Socket gt_socket = new GT_Socket();
                gt_socket.Server = IP;// "127.0.0.1";
                gt_socket.Port = PORT;// 9999;
                gt_socket.ProviderID = 112;
                gt_socket.Connect();

                bool stat = gt_socket.SendData(message + " \n", false);
                //string recieve = gt_socket.recieve(10000 * 60);

                //string[] tmp = recieve.Split(new string[] { "#@@#" }, StringSplitOptions.None);
                //int i = tmp[1].Length ;

                gt_socket.Disconnect();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string LogFormat(string ipUser,string input)
        {
            string tmp = "";
            tmp += DateTime.Now.ToString("yyyyMMdd_HH:mm:ss", new CultureInfo("en-US")) + "\t" + ipUser + "\t" + input;
            return tmp;
        }
    }
}