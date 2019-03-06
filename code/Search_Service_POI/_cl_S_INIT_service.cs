using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;

namespace Search_Service_POI
{
    public class _cl_S_INIT_service
    {

        public string[] FILE_SYNO;
        public string[] FILE_SHORT;
        public string[] FILE_CAT;
        public string[] FILE_BIG;

        public List<string> IP_LIST;
        public List<string> TOKEN_LIST;
        public int MAXRETURN;

        private string PATH_SYNO;
        private string PATH_SHORT;
        //private string PATH_DOT;
        private string PATH_CAT;
        private string PATH_BIG;
        private string PATH_TOKEN;
        private string PATH_IP;

        public _cl_S_INIT_service(string PathSyno, string PathShort, int MaxReturn, string PathToken, string PathIP, string PathCat, string PathBig)
        {
            try
            {
                PATH_SYNO = PathSyno;
                PATH_SHORT = PathShort;
                PATH_TOKEN = PathToken;
                PATH_IP = PathIP;
                MAXRETURN = MaxReturn;

                //PATH_DOT = PathDot;
                PATH_CAT = PathCat;
                PATH_BIG = PathBig;

                Set_IP();
                Set_Syno();
                Set_Short();
                Set_Token();
                //Set_Dot();
                Set_Cat();
                Set_Big();
            }
            catch
            {
            }
        }

        //public bool Set_Dot()
        //{
        //    try
        //    {
        //        FILE_DOT = null;
        //        FILE_DOT = File.ReadAllLines(PATH_DOT, Encoding.UTF8);

        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public bool Set_Big()
        {
            try
            {
                FILE_BIG = null;
                FILE_BIG = File.ReadAllLines(PATH_BIG, Encoding.UTF8);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Set_Cat()
        {
            try
            {
                FILE_CAT = null;
                FILE_CAT = File.ReadAllLines(PATH_CAT, Encoding.UTF8);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Set_Syno()
        {
            try
            {
                FILE_SYNO = null;
                FILE_SYNO = File.ReadAllLines(PATH_SYNO, Encoding.UTF8);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Set_IP()
        {
            try
            {
                IP_LIST = null;

                string[] tmp = File.ReadAllLines(PATH_IP, Encoding.UTF8);
                IP_LIST = tmp.ToList<string>();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Set_Token()
        {
            try
            {
                TOKEN_LIST = null;

                string[] tmp = File.ReadAllLines(PATH_TOKEN, Encoding.UTF8);
                TOKEN_LIST = tmp.ToList<string>();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Set_Short()
        {
            try
            {
                FILE_SHORT = null;
                FILE_SHORT = File.ReadAllLines(PATH_SHORT, Encoding.UTF8);

                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}