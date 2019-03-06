using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Data;
using wordReplace;

namespace Search_Service_POI
{
    public class _cl_synonym_replace
    {
        //public Dictionary<string, string> syn_replace = new Dictionary<string, string>();
        public DataTable dt_replace = new DataTable();
        public DataTable dt_add = new DataTable();
        public DataTable dt_dup = new DataTable();
        public _cl_synonym_replace(string path)
        {
            //init_replace(path_r);
            //init_add(path_a);
            init_dup(path);

        }
        private void init_add(string path)
        {
            string[] data = File.ReadAllLines(path, Encoding.UTF8);
            dt_add.Columns.Add("ori", typeof(string));
            dt_add.Columns.Add("syn", typeof(string));

            for (int i = 0; i < data.Length; i++)
            {
                if (string.IsNullOrEmpty(data[i]))
                    continue;
                string[] tmp = data[i].Split('|');
                dt_add.Rows.Add(tmp[0], tmp[1]);
            }
        }

        public string Add(string input)
        {
            string res = "";

            for (int i = 0; i < dt_add.Rows.Count; i++)
            {
                if (input.Contains(dt_add.Rows[i]["ori"].ToString().Trim()))
                //if (dt_add.Rows[i]["ori"].ToString().Trim().Contains(res))
                    res += " " + dt_add.Rows[i]["syn"].ToString().Trim();
            }
            return input + " "+ res.Trim();
        }

        private void init_replace(string path)
        {
            string[] data = File.ReadAllLines(path,Encoding.UTF8);
            dt_replace.Columns.Add("ori", typeof(string));
            dt_replace.Columns.Add("syn", typeof(string));

            for (int i = 0; i < data.Length; i++)
            {
                if (string.IsNullOrEmpty(data[i]))
                    continue;
                string[] tmp = data[i].Split('|');
                dt_replace.Rows.Add(tmp[0], tmp[1]);
            }
        }

        public string Replace(string input)
        {
            string res = input;

            for (int i = 0; i < dt_replace.Rows.Count; i++)
            {
                res = res.Replace(dt_replace.Rows[i]["ori"].ToString().Trim(), dt_replace.Rows[i]["syn"].ToString().Trim());
            }
            return res.Trim();
        }

        public void init_dup(string path)
        {
            string[] data = File.ReadAllLines(path, Encoding.UTF8);
            dt_dup.Columns.Add("ori", typeof(string));
            dt_dup.Columns.Add("syn", typeof(string));

            for (int i = 0; i < data.Length; i++)
            {
                if (string.IsNullOrEmpty(data[i]))
                    continue;
                string[] tmp = data[i].Split('|');
                dt_dup.Rows.Add(tmp[0], tmp[1]);
            }
        }

        public string Dup(string input)
        {
            List<string> tmp = new List<string>();
            _cl_wordRemove wordR = new _cl_wordRemove();
            for (int i = 0; i < dt_dup.Rows.Count; i++)
            {

                if (input.ToUpper().Contains(dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim()))
                {
                    string[] arr = dt_dup.Rows[i]["syn"].ToString().Trim().Split(',');
                    for (int j = 0; j < arr.Length; j++)
                    {
                        string res = input;
                        res = res.ToUpper().Replace(dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim(), arr[j].ToUpper().Trim());
                        tmp.Add(res);
                    }
                }
            }

            //string result = RemoveChar(input);
            string result = wordR.AllAction(input);
            for (int i = 0; i < tmp.Count;i++ )
            {
                //result += "|" + RemoveChar(tmp[i].Trim());
                result += "|" + wordR.AllAction(tmp[i].Trim());
            }
            return result;
        }

        public string RemoveChar(string input)
        {
            input = input.Replace(".", "");
            input = input.Replace("-", "");
            input = input.Replace("ฯ", "");
            return input;
        }


    }
}