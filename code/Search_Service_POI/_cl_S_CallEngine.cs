using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GT_Sockets;
namespace Search_Service_POI
{
    public class _cl_S_CallEngine
    {
        int TIMEOUT = 0;
        string IP = "";
        int PORT = 0;
        public _cl_S_CallEngine(int Port,string ip,int TimeOut)
        {
            TIMEOUT = TimeOut;
            PORT = Port;
            IP = ip;
        }
        public string SendToEngine(string message)
        {
            try
            {
                GT_Socket gt_socket = new GT_Socket();
                gt_socket.Server = IP;
                gt_socket.Port = PORT;
                gt_socket.ProviderID = 112;
                gt_socket.Connect();

                bool stat = gt_socket.SendData(message + " \n", false);
                string recieve = gt_socket.recieve(10000 * 60);

                //string[] tmp = recieve.Split(new string[] { "#@@#" }, StringSplitOptions.None);
                //int i = tmp[1].Length ;

                gt_socket.Disconnect();
                return recieve;
            }
            catch
            {
                return "";
            }
        }
            
    }
}