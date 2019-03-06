using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace Search_Service_POI
{
    public class _cl_S_StringToDT
    {
        public DataTable ToDataTable(string recieve, int RowsPerPage, int PageNumber)
        {
            string[] tmp_recive = recieve.Split(new string[] { "@##@" }, StringSplitOptions.None);
            string[] line = tmp_recive[1].Split('|');
            DataTable dt = new DataTable();
            dt.TableName = tmp_recive[0].ToString().Trim();
            int indexEnd = Convert.ToInt32(RowsPerPage) * Convert.ToInt32(PageNumber);
            int indexStart = (indexEnd - Convert.ToInt32(RowsPerPage)) + 1;

            string[] header = line[0].Split('^');
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] == "dist" || header[i] == "score")
                {
                    dt.Columns.Add(header[i], typeof(double));
                }
                else
                    dt.Columns.Add(header[i], typeof(string));

            }

            for (int i = indexStart; i < line.Length; i++)
            {
                if (line[i].Trim() == "")
                    break;
                string[] tmp = line[i].Split('^');
                dt.Rows.Add(tmp);
                if (i == indexEnd)
                    break;
            }

            dt.Columns.Add("No", typeof(string));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["No"] = indexStart.ToString();
                indexStart++;
            }

            return dt;
        }

        public DataTable ToDataTable(string recieve)
        {
            string[] line = recieve.Split('|');
            DataTable dt = new DataTable();

            string[] header = line[0].Split('^');
            for (int i = 0; i < header.Length; i++)
            {
                if (header[i] == "dist" || header[i] == "score")
                {
                    dt.Columns.Add(header[i], typeof(double));
                }
                else
                    dt.Columns.Add(header[i], typeof(string));

            }

            for (int i = 1; i < line.Length; i++)
            {
                if (line[i].Trim() == "")
                    break;
                string[] tmp = line[i].Split('^');
                dt.Rows.Add(tmp);
            }

            return dt;
        }
    }
}