using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Search_Service_POI
{
    public class _cl_S_TextToCat
    {
        DataTable dt_localCat = new DataTable();
        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("th-TH");

        public bool CheckWord(string keyword,ref string localCat)
        {
            try
            {
                for (int i = 0; i < dt_localCat.Rows.Count; i++)
                {
                    if (dt_localCat.Rows[i]["keyword"].ToString() == keyword.Trim().ToUpper())
                    {
                        localCat = dt_localCat.Rows[i]["localcatcode"].ToString();
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private void init_dt(string[] dataCat)
        {


            dt_localCat.Columns.Add("keyword", typeof(string));
            dt_localCat.Columns.Add("localcatcode", typeof(string));

            for (int i = 0; i < dataCat.Length; i++)
            {
                if (string.IsNullOrEmpty(dataCat[i]))
                    continue;
                string[] tmp = dataCat[i].Split(';');
                dt_localCat.Rows.Add(tmp[0], tmp[1]);
            }
        }

        public _cl_S_TextToCat(string[] dataCat)
        {
            init_dt(dataCat);
        }
    }
}