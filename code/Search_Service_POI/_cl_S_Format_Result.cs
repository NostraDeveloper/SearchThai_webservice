using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace Search_Service_POI
{
    public class _cl_S_Format_Result
    {

        public DataTable SearchResultFormat(DataTable dt)
        {
            try
            {
                dt.Columns.Remove("Group_Name");
            }catch{}

            try
            {
                dt.Columns.Remove("Group_All");
            }
            catch { }
            try
            {
                dt.Columns.Remove("NAME_ENTH");
            }
            catch { }
            try
            {
                dt.Columns.Remove("NAME_THEN");
            }
            catch { }
            try
            {
                dt.Columns.Remove("NAME_ENTH_wb");
            }
            catch { }
            try
            {
                dt.Columns.Remove("NAME_THEN_wb");
            }
            catch { }

            /////////////
            try
            {
                dt.Columns.Remove("Name_E_sort");
            }
            catch { }
            try
            {
                dt.Columns.Remove("Name_L_sort");
            }
            catch { }
            try
            {
                dt.Columns.Remove("Name_L_sort_WB");
            }
            catch { }
            try
            {
                dt.Columns.Remove("Name_E_sort_WB");
            }
            catch { }
            try
            {
                dt.Columns.Remove("des_eng");
            }
            catch { }
            try
            {
                dt.Columns.Remove("des_local");
            }
            catch { }
            try
            {
                dt.Columns.Remove("text_search");
            }
            catch { }
            try
            {
                dt.Columns.Remove("hitscore");
            }
            catch { }
            
            try
            {
                /*dt.Columns.Add("AdminLevel1Code");
                dt.Columns.Add("AdminLevel2Code");
                dt.Columns.Add("AdminLevel3Code");
                */
                for(int i = 0;i<dt.Rows.Count;i++)
                {
                    //Admin_Code
                    dt.Rows[i]["AdminLevel1Code"] = dt.Rows[i]["Admin_Code"].ToString().Substring(0, 2);
                    dt.Rows[i]["AdminLevel2Code"] = dt.Rows[i]["Admin_Code"].ToString().Substring(2, 2);
                    dt.Rows[i]["AdminLevel3Code"] = dt.Rows[i]["Admin_Code"].ToString().Substring(4, 2);
                }
                dt.Columns.Remove("Admin_Code");
            }
            catch
            {

            }
            
            try
            {
                dt.Columns.Remove("SOUND_NAME_LOCAL");
            }
            catch{}
            try
            {
                dt.Columns.Remove("SOUND_NAME_THEN");
            }
            catch { }
            try
            {
                dt.Columns.Remove("SOUND_NAME_ENTH");
            }
            catch { }
            try
            {
                dt.Columns.Remove("HouseNumber_sound");
            }
            catch { }
            try
            {
                dt.Columns.Remove("Telephone_sound");
            }
            catch { }
            try
            {
                dt.Columns.Remove("PostCode_sound");
            }
            catch { }
            try
            {
                dt.Columns.Remove("local_soundex");
            }
            catch { }
           
            return SearchFormatSort(dt);
        }

        public DataTable IdenFormatSort(DataTable dt)
        {
            int i = 0;
            dt.Columns["No"].SetOrdinal(i++);
            dt.Columns["NostraId"].SetOrdinal(i++);
            dt.Columns["Name_E"].SetOrdinal(i++);
            dt.Columns["Name_L"].SetOrdinal(i++);
            dt.Columns["Branch_E"].SetOrdinal(i++);
            dt.Columns["Branch_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel1_E"].SetOrdinal(i++);
            dt.Columns["AdminLevel1_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel2_E"].SetOrdinal(i++);
            dt.Columns["AdminLevel2_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel3_E"].SetOrdinal(i++);
            dt.Columns["AdminLevel3_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel4_E"].SetOrdinal(i++);
            dt.Columns["AdminLevel4_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel1Code"].SetOrdinal(i++);
            dt.Columns["AdminLevel2Code"].SetOrdinal(i++);
            dt.Columns["AdminLevel3Code"].SetOrdinal(i++);
            dt.Columns["Catcode"].SetOrdinal(i++);
            dt.Columns["LocalCatCode"].SetOrdinal(i++);
            dt.Columns["HouseNo"].SetOrdinal(i++);
            dt.Columns["Telephone"].SetOrdinal(i++);
            dt.Columns["PostCode"].SetOrdinal(i++);
            dt.Columns["dist"].SetOrdinal(i++);

            dt.AcceptChanges();
            return dt;

        }

        public DataTable SearchFormatSort(DataTable dt)
        {
            int i = 0;
            dt.Columns["No"].SetOrdinal(i++);
            dt.Columns["NostraId"].SetOrdinal(i++);
            dt.Columns["Name_E"].SetOrdinal(i++);
            dt.Columns["Name_L"].SetOrdinal(i++);
            dt.Columns["Branch_E"].SetOrdinal(i++);
            dt.Columns["Branch_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel1_E"].SetOrdinal(i++);
            dt.Columns["AdminLevel1_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel2_E"].SetOrdinal(i++);
            dt.Columns["AdminLevel2_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel3_E"].SetOrdinal(i++);
            dt.Columns["AdminLevel3_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel4_E"].SetOrdinal(i++);
            dt.Columns["AdminLevel4_L"].SetOrdinal(i++);
            dt.Columns["AdminLevel1Code"].SetOrdinal(i++);
            dt.Columns["AdminLevel2Code"].SetOrdinal(i++);
            dt.Columns["AdminLevel3Code"].SetOrdinal(i++);
            dt.Columns["Catcode"].SetOrdinal(i++);
            dt.Columns["LocalCatCode"].SetOrdinal(i++);
            dt.Columns["HouseNo"].SetOrdinal(i++);
            dt.Columns["Telephone"].SetOrdinal(i++);
            dt.Columns["PostCode"].SetOrdinal(i++);
            dt.Columns["dist"].SetOrdinal(i++);

            dt.Columns["LatLon"].SetOrdinal(i++);
            dt.Columns["LatLon_Route1"].SetOrdinal(i++);
            dt.Columns["LatLon_Route2"].SetOrdinal(i++);
            dt.Columns["LatLon_Route3"].SetOrdinal(i++);
            dt.Columns["LatLon_Route4"].SetOrdinal(i++);
            dt.Columns["score"].SetOrdinal(i++);
            dt.Columns["Popularity"].SetOrdinal(i++);
            //dt.Columns["Admin_Code"].SetOrdinal(i++);

            dt.AcceptChanges();
            return dt;

        }

        public DataTable SortDis(DataTable dt)
        {
            DataView dv = dt.DefaultView;
            dv.Sort = "dist asc";
            DataTable sortedDT = dv.ToTable();

            for (int i = 0; i < sortedDT.Rows.Count; i++)
            {
                sortedDT.Rows[i]["No"] = (i + 1).ToString();
            }

            return sortedDT;
        }

        public DataTable ToDataTable(string recieve)
        {
            string[] tmp_recive = recieve.Split(new string[] { "@##@" }, StringSplitOptions.None);
            string[] line = tmp_recive[1].Split('|');
            DataTable dt = new DataTable();
            dt.TableName = tmp_recive[0].ToString().Trim();
            //int indexEnd = Convert.ToInt32(RowsPerPage) * Convert.ToInt32(PageNumber);
            //int indexStart = (indexEnd - Convert.ToInt32(RowsPerPage)) + 1;

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

            //for (int i = indexStart; i < line.Length; i++)
            for (int i = 1; i < line.Length; i++)
            {
                if (line[i].Trim() == "")
                    break;
                string[] tmp = line[i].Split('^');
                dt.Rows.Add(tmp);
                //if (i == indexEnd)
                //    break;
            }

            dt.Columns.Add("No", typeof(string));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["No"] = (i + 1).ToString();
                //indexStart++;
            }

            return dt;
        }

        public DataTable AlongRouteFormat(DataTable dt)
        {
            DataTable dt_tmp = new DataTable();
            dt_tmp = dt.Copy();
            try 
            {
                for (int i=0;i<dt_tmp.Rows.Count;i++)
                {
                    double dis_tmp = Convert.ToDouble( dt_tmp.Rows[i]["dist"].ToString().Trim()); 

                    if(dis_tmp !=0)
                    {
                        dt_tmp.Rows[i]["dist"] = dis_tmp.ToString("N3"); 
                    }

                    string[] LatLon = dt_tmp.Rows[i]["LatLon"].ToString().Trim().Split(',');
                    string[] LatLon_Route1 = dt_tmp.Rows[i]["LatLon_Route1"].ToString().Trim().Split(',');
                    string[] LatLon_Route2 = dt_tmp.Rows[i]["LatLon_Route2"].ToString().Trim().Split(',');
                    string[] LatLon_Route3 = dt_tmp.Rows[i]["LatLon_Route3"].ToString().Trim().Split(',');
                    string[] LatLon_Route4 = dt_tmp.Rows[i]["LatLon_Route4"].ToString().Trim().Split(',');

                    dt_tmp.Rows[i]["LatLon"] = Convert.ToDouble(LatLon[0]).ToString("N6") + "," + Convert.ToDouble(LatLon[1]).ToString("N6");
                    dt_tmp.Rows[i]["LatLon_Route1"] = Convert.ToDouble(LatLon_Route1[0]).ToString("N6") + "," + Convert.ToDouble(LatLon_Route1[1]).ToString("N6");
                    dt_tmp.Rows[i]["LatLon_Route2"] = Convert.ToDouble(LatLon_Route2[0]).ToString("N6") + "," + Convert.ToDouble(LatLon_Route2[1]).ToString("N6");
                    dt_tmp.Rows[i]["LatLon_Route3"] = Convert.ToDouble(LatLon_Route3[0]).ToString("N6") + "," + Convert.ToDouble(LatLon_Route3[1]).ToString("N6");
                    dt_tmp.Rows[i]["LatLon_Route4"] = Convert.ToDouble(LatLon_Route4[0]).ToString("N6") + "," + Convert.ToDouble(LatLon_Route4[1]).ToString("N6");
                }

                return dt_tmp;

            }
            catch
            { return dt; }
        }
    }
}