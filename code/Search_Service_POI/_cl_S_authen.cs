using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Service_POI
{
    public class _cl_S_authen
    {
        private List<string> IP;
        private List<string> TOKEN;

        public _cl_S_authen ( List<string> IP_List , List<string> Token_List)
        {
            IP = IP_List;
            TOKEN = Token_List; 
        }

        public bool check(string ip,string token)
        {
            string bypass = System.Configuration.ConfigurationSettings.AppSettings["noIP"].ToString();
            if (bypass.ToUpper().Trim() == "TRUE")
            { return true; }
            if (TOKEN.Contains(token))
                return true;

            if (IP.Contains(ip))
                return true;

            return false;
        }
    }
}