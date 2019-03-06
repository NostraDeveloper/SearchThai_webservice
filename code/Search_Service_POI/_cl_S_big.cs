using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Service_POI
{
    public class _cl_S_big
    {
        private List<string> BIGDATA = new List<string>();
        public _cl_S_big(string[] input)
        {
            BIGDATA = input.ToList<string>();
        }

        public bool check(string input)
        {
            string tmp = input.ToUpper().Trim();
            
            for(int i = 0;i<BIGDATA.Count;i++)
            {
                if (tmp.Contains(BIGDATA[i].ToUpper().Trim()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}