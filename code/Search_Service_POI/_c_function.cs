using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.IO;

namespace Search_Service_POI
{
    public class _c_function
    {
        public string addConStr = System.Configuration.ConfigurationSettings.AppSettings["ConStringMaster"].ToString();
        public string addDB = System.Configuration.ConfigurationSettings.AppSettings["SearchDatabase"].ToString();

        // OUTPUT  (Column:word) 
        private string insert(string column, string word, Boolean fix, Boolean and)
        {
            string result = "";
            word = Regex.Replace(word, @"\b(and|AND)\b", "\"and\"");
            word = Regex.Replace(word, @"\b(or|OR)\b", "\"or\"");
            
            if (and)
            {
                word = word.Replace(" ", " AND ");
            }

            if (!fix)
            {
                result = column + ":(" + word + ")";
            }
            else
            {
                result = column + @":(""" + word + @""")";
            }

            return result;
        }


        public string sortType(string sortype)
        {
            string sort = "";
            switch (sortype)
            {
                case "0":
                    {
                        sort = "&sort=score desc";
                        return sort;
                    }
                case "1":
                    {
                        sort = "&sort=name_l asc";
                        return sort;
                    }
                case "2":
                    {
                        sort = "&sort=name_l desc";
                        return sort;
                    }
                case "3":
                    {
                        sort = "&sort=name_e asc";
                        return sort;
                    }
                case "4":
                    {
                        sort = "&sort=name_e desc";
                        return sort;
                    }
                case "5":
                    {
                        sort = "&sort=geodist() asc,score desc";
                        return sort;
                    }
                case "6":
                    {
                        sort = "&sort= score desc,geodist() asc";
                        return sort;
                    }
                 
                default: { sort = "&sort=score desc,geodist() asc"; return sort; }
            }
        }

        /// <summary>
        /// Query From Database with master id in table from solr
        /// </summary>
        /// <param name="table"></param>
        /// <param name="dt"></param>
        public void QueryResult(DataTable table, ref DataTable dt)
        {
            string addConStr = System.Configuration.ConfigurationSettings.AppSettings["ConStringMaster"].ToString();
            SqlConnection sqlConn = new SqlConnection(addConStr);
            sqlConn.Open();

            foreach (DataRow row in table.Rows)
            {
                string query = "";   // SELECT * FROM Master_Table WHERE Master_ID = '" + row["Master_ID"] + "'    ###########OLD COMMAND######
                // Order Column here at *
                query = "SELECT  Master_Table.Master_ID,LANDMARK.HOUSE_NO,Master_Table.Name_L,Master_Table.LOCATION_L,ADMIN_AREA.NAME1,ADMIN_AREA.NAME2,ADMIN_AREA.NAME3,";
                query = query + "Master_Table.Name_E,Master_Table.LOCATION_E,ADMIN_AREA.NAME1_ENG,ADMIN_AREA.NAME2_ENG,ADMIN_AREA.NAME3_ENG,Master_Table.POST_CODE,TELEPHONE.Telephone,Master_Table.Lat,";
                query = query + "Master_Table.Lon";
                query = query + " FROM  Master_Table LEFT OUTER JOIN ";
                query = query + " ADMIN_AREA ON Master_Table.ADMIN_AREA_ID = ADMIN_AREA.ADMIN_AREA_ID LEFT OUTER JOIN ";
                query = query + " TELEPHONE ON Master_Table.Master_ID = TELEPHONE.Master_ID LEFT OUTER JOIN ";
                query = query + " LANDMARK ON Master_Table.Master_ID = LANDMARK.Master_ID ";
                query = query + " Where Master_table.master_id = " + row["nostraid"];
                //

                SqlCommand cmd = new SqlCommand(query, sqlConn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            sqlConn.Close();

            try
            {
                dt.Columns["NAME1"].ColumnName = "admin_divis_1_local";
                dt.Columns["NAME2"].ColumnName = "admin_divis_2_local";
                dt.Columns["NAME3"].ColumnName = "admin_divis_3_local";
                dt.Columns["NAME1_ENG"].ColumnName = "admin_divis_1_english";
                dt.Columns["NAME2_ENG"].ColumnName = "admin_divis_2_english";
                dt.Columns["NAME3_ENG"].ColumnName = "admin_divis_3_english";
            }
            catch (Exception) { }
            dt.TableName = "Result";
        }

      
        public void createSolrQueryString(ref string URI, string keyword, string latlon, string radius, List<string> category, List<string> tag, string location, string sort, string sub_district, string district, string province, string postcode)
        {
            string URIAddress = System.Configuration.ConfigurationSettings.AppSettings["URI_address"].ToString();
            StringBuilder SB = new StringBuilder();


            //////////////////////////////Detect Language///////////////////////////////////

            string[] lang = Detect_Language(keyword);


            ////////////////////////////////////////////////////////////////////////////////

            //Keyword And Name  Split by Language
            SB.Append("(");

            if (keyword != "" & keyword != "*")
            {
                ///////////////////////////////// NAME //////////////////////////////////////////////
                SB.Append("(");
                if (string.IsNullOrEmpty(lang[0]))
                {
                    SB.Append(insert("Name_English", lang[1].Trim(),true, true)+"^20000  ");
                }
                else if (string.IsNullOrEmpty(lang[1]))
                {
                    SB.Append(insert("Name_Local", lang[0].Trim(), true, true) + "^2000  ");
                }
                else
                {
                    SB.Append(insert("Name_English", lang[1].Trim(), true, true)+"^20 AND " + insert("Name_Local", lang[0].Trim(), true, true)+"^20 ");
                }
                SB.Append(")");
                ///////////////////////////////// NAME //////////////////////////////////////////////

                //SYN

                SB.Append(@" OR (synonym:"""+keyword.Replace("&","%26")+@""")^10000 ");

                //SYN
                
                ///////////////////////////////// Description //////////////////////////////////////////////
                SB.Append("OR (");
                if (string.IsNullOrEmpty(lang[0]) && string.IsNullOrEmpty(lang[1]))
                {
                    SB.Append(insert("Description_English", lang[2].Trim(), false, true));  //AND Description_English:(" + lang[1].Trim() + ")  ภาษษอังกฤษเพี้ยน
                    SB.Append("AND " + insert("Description_Local", lang[2].Trim(), false, true));
                }
                else if (string.IsNullOrEmpty(lang[0]))
                {
                    SB.Append( insert("Description_English", lang[1].Trim(), false, true));
                }
                else if (string.IsNullOrEmpty(lang[1]))
                {
                    SB.Append(insert("Description_Local", lang[0].Trim(), false, true));
                }
                else
                {
                    SB.Append(insert("Description_English", lang[1].Trim(), false, true));  //AND Description_English:(" + lang[1].Trim() + ")  ภาษษอังกฤษเพี้ยน
                    SB.Append("AND " + insert("Description_Local", lang[0].Trim(), false, true));
                }
                SB.Append(")");
                ///////////////////////////////// Description //////////////////////////////////////////////

            }
            else SB.Append("*");


            //SB.Append(" AND (");
            //SB.Append("Layer:LANDMARK");
            //SB.Append(")");


            SB.Append(")");

            // PerfectMatch With and
            //if (sub_district + district + province + postcode != "")
            //{
            //    SB.Append(" AND (");
            //    if (sub_district != "") { SB.Append(SolrQueryString(sub_district, "Admin_3")); } else { SB.Append(SolrQueryString("*", "Admin_3")); }
            //    if (district != "") { SB.Append(" AND " + SolrQueryString(district, "Admin_2")); } else { SB.Append(" AND " + SolrQueryString("*", "Admin_2")); }
            //    if (province != "") { SB.Append(" AND " + SolrQueryString(province, "Admin_1")); } else { SB.Append(" AND " + SolrQueryString("*", "Admin_1")); }
            //    if (postcode != "") { SB.Append(" AND " + SolrQueryString(postcode, "PostCode")); } else { SB.Append(" AND " + SolrQueryString("*", "PostCode")); }
            //    SB.Append(")");
            //}


            if (sub_district + district + province != "")
            {
                string admincode = province + district + sub_district;
                SB.Append(" AND (");
                if (sub_district == "") { SB.Append("Admin_code:" + admincode + "*)"); }
                else
                {
                    SB.Append("Admin_code:" + admincode + ")");
                }

            }


            if (!string.IsNullOrEmpty(postcode))
            {
                SB.Append(" AND (PostCode:" + postcode + ") ");
            }


            if (!string.IsNullOrEmpty(location))
            {
                SB.Append(" AND " + SolrQueryString(location, "Admin_4"));
            }
            else { }


            //category
            //if (category != "") { SB.Append("AND " + SolrQueryString(category, "category")); } else { SB.Append("AND " + SolrQueryString("*", "category")); }
            if (category.Count() > 0)
            {
                if (category.Count() == 1 && category[0] == "") { }
                else
                {
                    SB.Append(" AND (");
                    for (int i = 0; i < category.Count(); i++)
                    {

                        SB.Append(SolrQueryString(category[i], "Category"));
                        if (i + 1 < category.Count()) SB.Append(" OR ");

                    }
                    SB.Append(")");
                }


            }


            //tag
            //if (tag != "") { SB.Append("AND " + SolrQueryString(tag, "tag")); } else { SB.Append("AND " + SolrQueryString("*", "tag")); }
            if (tag.Count() > 0)
            {
                if (string.IsNullOrEmpty(tag[0])) { }
                else
                {
                    SB.Append(" AND (");
                    for (int i = 0; i < tag.Count(); i++)
                    {
                        SB.Append(SolrQueryString(tag[i], "Tag"));
                        if (i + 1 < tag.Count()) SB.Append(" OR ");


                    }
                    SB.Append(")");
                }
            }

            //location






            //other parameter
            SB.Append("&defType=edismax");
            SB.Append("&wt=json&indent=true");
            SB.Append("&mm=3<-1 5<50%25");
           
            SB.Append("&tie=0");
          
            SB.Append("&fl=NostraID,house_number,Name_Local,Branch_L,Admin_1_Local,Admin_2_Local,Admin_3_Local,Admin_4_Local,Name_English,Branch_E,Admin_1_English,Admin_2_English,Admin_3_English,Admin_4_English,PostCode,telephone,LatLon,LatLon_Route_1,LatLon_Route_2,LatLon_Route_3,LatLon_Route_4,CategoryReturn,Popularity,score");

            /*  
            SB.Append("&pf2=Description_Local^20");
            SB.Append("&pf3=Description_Local^30 ");
              SB.Append("&ps2=3&ps3=5");
            
            SB.Append("&qf=synonym Name_Local Name_English ");
            SB.Append("&pf=Name_Local");
             */

            //latlon
            if (latlon != ",")
            {
                if (!string.IsNullOrEmpty(radius)) SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=true&pt=" + latlon + "&sfield=LatLon&d=" + radius);  //&spatial=true&pt=" + latlon + "&sfield=latlon&d=50"
                else SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=true&pt=" + latlon + "&sfield=LatLon&d=1000");

                //sort

            }




            //Sort
            if (latlon != ",")
            {
                if (sort == "")
                {
                    if (string.IsNullOrEmpty(keyword) & category.Count > 0)
                    {
                        SB.Append(sortType("5"));  // Near by First
                    }
                    else
                    {
                        SB.Append(sortType("6")); // Relevance First

                    }
                }
                else
                {
                    SB.Append(sortType(sort));
                }
            }
            else
            {
                SB.Append(sortType("0"));
            }
            // Sort Normal score,dist  have Lat,Lon
            // Sort Without Lat,Lon
            // Sort For Category Search








            //defaultsearchfield
            //SB.Append("&df=name,field_l,field_e,sub_district,district,province,category,tag");




            URI = URIAddress + SB.ToString();

        }

        public void createSolrQueryString_KMmini(ref string URI, string keyword, string latlon, string radius, List<string> category, List<string> tag, string location, string sort, string sub_district, string district, string province, string postcode)
        {
            string URIAddress = System.Configuration.ConfigurationSettings.AppSettings["URI_address"].ToString();
            StringBuilder SB = new StringBuilder();


            //////////////////////////////Detect Language///////////////////////////////////

            string[] lang = Detect_Language(keyword);


            ////////////////////////////////////////////////////////////////////////////////

            //Keyword And Name  Split by Language
            SB.Append("(");

            if (keyword != "" & keyword != "*")
            {
                ///////////////////////////////// NAME //////////////////////////////////////////////
                SB.Append("(");
                if (string.IsNullOrEmpty(lang[0]))
                {
                    SB.Append(insert("Name_English", lang[1].Trim(), true, true) + "^20000  ");
                }
                else if (string.IsNullOrEmpty(lang[1]))
                {
                    SB.Append(insert("Name_Local", lang[0].Trim(), true, true) + "^2000  ");
                }
                else
                {
                    SB.Append(insert("Name_English", lang[1].Trim(), true, true) + "^20 AND " + insert("Name_Local", lang[0].Trim(), true, true) + "^20 ");
                }
                SB.Append(")");
                ///////////////////////////////// NAME //////////////////////////////////////////////

                //SYN

                //SB.Append(@" OR (synonym:""" + keyword.Replace("&", "%26") + @""")^10000 ");

                //SYN

                ///////////////////////////////// Description //////////////////////////////////////////////
                SB.Append("OR (");
                if (string.IsNullOrEmpty(lang[0]) && string.IsNullOrEmpty(lang[1]))
                {
                    SB.Append(insert("Description_English", lang[2].Trim(), false, true));  //AND Description_English:(" + lang[1].Trim() + ")  ภาษษอังกฤษเพี้ยน
                    SB.Append("AND " + insert("Description_Local", lang[2].Trim(), false, true));
                }
                else if (string.IsNullOrEmpty(lang[0]))
                {
                    SB.Append(insert("Description_English", lang[1].Trim(), false, true));
                }
                else if (string.IsNullOrEmpty(lang[1]))
                {
                    SB.Append(insert("Description_Local", lang[0].Trim(), false, true));
                }
                else
                {
                    SB.Append(insert("Description_English", lang[1].Trim(), false, true));  //AND Description_English:(" + lang[1].Trim() + ")  ภาษษอังกฤษเพี้ยน
                    SB.Append("AND " + insert("Description_Local", lang[0].Trim(), false, true));
                }
                SB.Append(")");
                ///////////////////////////////// Description //////////////////////////////////////////////

            }
            else SB.Append("*");



            SB.Append(")");

            // PerfectMatch With and

            if (sub_district + district + province != "")
            {
                string admincode = province + district + sub_district;
                SB.Append(" AND (");
                if (sub_district == "") { SB.Append("Admin_code:" + admincode + "*)"); }
                else
                {
                    SB.Append("Admin_code:" + admincode + ")");
                }

            }


            if (!string.IsNullOrEmpty(postcode))
            {
                SB.Append(" AND (PostCode:" + postcode + ") ");
            }


            if (!string.IsNullOrEmpty(location))
            {
                SB.Append(" AND " + SolrQueryString(location, "Admin_4"));
            }
            else { }


            //category
           
            if (category.Count() > 0)
            {
                if (category.Count() == 1 && category[0] == "") { }
                else
                {
                    SB.Append(" AND (");
                    for (int i = 0; i < category.Count(); i++)
                    {

                        SB.Append(SolrQueryString(category[i], "Category"));
                        if (i + 1 < category.Count()) SB.Append(" OR ");

                    }
                    SB.Append(")");
                }


            }


            //tag
            if (tag.Count() > 0)
            {
                if (string.IsNullOrEmpty(tag[0])) { }
                else
                {
                    SB.Append(" AND (");
                    for (int i = 0; i < tag.Count(); i++)
                    {
                        SB.Append(SolrQueryString(tag[i], "Tag"));
                        if (i + 1 < tag.Count()) SB.Append(" OR ");


                    }
                    SB.Append(")");
                }
            }

            //location

            //other parameter
            SB.Append("&defType=edismax");
            SB.Append("&wt=json&indent=true");
            SB.Append("&mm=1<-1 3<50%25");

            SB.Append("&tie=0");

            SB.Append("&fl=NostraID,house_number,Name_Local,Branch_L,Admin_1_Local,Admin_2_Local,Admin_3_Local,Admin_4_Local,Name_English,Branch_E,Admin_1_English,Admin_2_English,Admin_3_English,Admin_4_English,PostCode,telephone,LatLon,LatLon_Route_1,LatLon_Route_2,LatLon_Route_3,LatLon_Route_4,CategoryReturn,Popularity");


            //latlon
            if (latlon != ",")
            {
                if (!string.IsNullOrEmpty(radius)) SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=true&pt=" + latlon + "&sfield=LatLon&d=" + radius);  //&spatial=true&pt=" + latlon + "&sfield=latlon&d=50"
                else SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=true&pt=" + latlon + "&sfield=LatLon&d=1000");

                //sort

            }

            //Sort
            if (latlon != ",")
            {
                if (sort == "")
                {
                    if (string.IsNullOrEmpty(keyword) & category.Count > 0)
                    {
                        SB.Append(sortType("5"));  // Near by First
                    }
                    else
                    {
                        SB.Append(sortType("6")); // Relevance First

                    }
                }
                else
                {
                    SB.Append(sortType(sort));
                }
            }
            else
            {
                SB.Append(sortType("0"));
            }

            URI = URIAddress + SB.ToString();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="URI"></param>
        /// <param name="keyword">
        /// 0 = ชื่อหมู่
        /// 1 = จังหวัด
        /// 2 = อำเภอ
        /// 3 = ตำบล
        /// </param>
        /// <param name="latlon"></param>
        /// <param name="radius"></param>
        /// <param name="category"></param>
        /// <param name="tag"></param>
        /// <param name="location"></param>
        /// <param name="sort"></param>
        /// <param name="sub_district"></param>
        /// <param name="district"></param>
        /// <param name="province"></param>
        /// <param name="postcode"></param>
        public void createSolrQueryString_moo(ref string URI, string[] keyword, string latlon, string radius, List<string> category, List<string> tag, string location, string sort, string sub_district, string district, string province, string postcode)
        {
            
            string URIAddress = System.Configuration.ConfigurationSettings.AppSettings["URI_address"].ToString();
            StringBuilder SB = new StringBuilder();


            //////////////////////////////Detect Language///////////////////////////////////

            //string[] lang = Detect_Language(keyword);


            ////////////////////////////////////////////////////////////////////////////////

            //Keyword And Name  Split by Language
            SB.Append("(");

            //if (keyword != "" & keyword != "*")
            {
                ///////////////////////////////// NAME //////////////////////////////////////////////
                SB.Append("(");
                //SB.Append(insert("Name_Local", keyword[0].Trim(), true, true) + "^2000  ");
                SB.Append(insert("moo", keyword[0].Trim(), true, true) + "^2000  ");
                if(!string.IsNullOrEmpty( keyword[1]))
                    SB.Append(insert("Admin_1_Local", keyword[1].Trim(), true, true) + "^3000  ");
                if (!string.IsNullOrEmpty(keyword[2]))
                    SB.Append(insert("Admin_2_Local", keyword[2].Trim(), true, true) + "^3000  ");
                if (!string.IsNullOrEmpty(keyword[3]))
                    SB.Append(insert("Admin_3_Local", keyword[3].Trim(), true, true) + "^3000  ");
                SB.Append(")");

                SB = SB.Replace("\"", "\\\"");
                ///////////////////////////////// NAME //////////////////////////////////////////////

                //SYN

                //SB.Append(@" OR (synonym:""" + keyword.Replace("&", "%26") + @""")^10000 ");

                //SYN

                ///////////////////////////////// Description //////////////////////////////////////////////
                /*SB.Append("OR (");
                if (string.IsNullOrEmpty(lang[0]) && string.IsNullOrEmpty(lang[1]))
                {
                    SB.Append(insert("Description_English", lang[2].Trim(), false, true));  //AND Description_English:(" + lang[1].Trim() + ")  ภาษษอังกฤษเพี้ยน
                    SB.Append("AND " + insert("Description_Local", lang[2].Trim(), false, true));
                }
                else if (string.IsNullOrEmpty(lang[0]))
                {
                    SB.Append(insert("Description_English", lang[1].Trim(), false, true));
                }
                else if (string.IsNullOrEmpty(lang[1]))
                {
                    SB.Append(insert("Description_Local", lang[0].Trim(), false, true));
                }
                else
                {
                    SB.Append(insert("Description_English", lang[1].Trim(), false, true));  //AND Description_English:(" + lang[1].Trim() + ")  ภาษษอังกฤษเพี้ยน
                    SB.Append("AND " + insert("Description_Local", lang[0].Trim(), false, true));
                }
                SB.Append(")");*/
                ///////////////////////////////// Description //////////////////////////////////////////////

            }
            //else SB.Append("*");



            SB.Append(")");

            // PerfectMatch With and
            //if (sub_district + district + province + postcode != "")
            //{
            //    SB.Append(" AND (");
            //    if (sub_district != "") { SB.Append(SolrQueryString(sub_district, "Admin_3")); } else { SB.Append(SolrQueryString("*", "Admin_3")); }
            //    if (district != "") { SB.Append(" AND " + SolrQueryString(district, "Admin_2")); } else { SB.Append(" AND " + SolrQueryString("*", "Admin_2")); }
            //    if (province != "") { SB.Append(" AND " + SolrQueryString(province, "Admin_1")); } else { SB.Append(" AND " + SolrQueryString("*", "Admin_1")); }
            //    if (postcode != "") { SB.Append(" AND " + SolrQueryString(postcode, "PostCode")); } else { SB.Append(" AND " + SolrQueryString("*", "PostCode")); }
            //    SB.Append(")");
            //}


            if (sub_district + district + province != "")
            {
                string admincode = province + district + sub_district;
                SB.Append(" AND (");
                if (sub_district == "") { SB.Append("Admin_code:" + admincode + "*)"); }
                else
                {
                    SB.Append("Admin_code:" + admincode + ")");
                }

            }


            if (!string.IsNullOrEmpty(postcode))
            {
                SB.Append(" AND (PostCode:" + postcode + ") ");
            }


            if (!string.IsNullOrEmpty(location))
            {
                SB.Append(" AND " + SolrQueryString(location, "Admin_4"));
            }
            else { }


            //category
            //if (category != "") { SB.Append("AND " + SolrQueryString(category, "category")); } else { SB.Append("AND " + SolrQueryString("*", "category")); }

            SB.Append(" AND (");
            //SB.Append("Category:(other)");
            SB.Append("Layer:VILLAGE");
            SB.Append(")");


            //tag
           

            //location

            //other parameter
            SB.Append("&defType=edismax");
            SB.Append("&wt=json&indent=true");
            //SB.Append("&mm=3<-1 5<50%25");

            SB.Append("&tie=0");

            SB.Append("&fl=NostraID,house_number,Name_Local,Branch_L,Admin_1_Local,Admin_2_Local,Admin_3_Local,Admin_4_Local,Name_English,Branch_E,Admin_1_English,Admin_2_English,Admin_3_English,Admin_4_English,PostCode,telephone,LatLon,LatLon_Route_1,LatLon_Route_2,LatLon_Route_3,LatLon_Route_4,CategoryReturn,Popularity");

            /*  
            SB.Append("&pf2=Description_Local^20");
            SB.Append("&pf3=Description_Local^30 ");
              SB.Append("&ps2=3&ps3=5");
            
            SB.Append("&qf=synonym Name_Local Name_English ");
            SB.Append("&pf=Name_Local");
             */

            //latlon
            if (latlon != ",")
            {
                if (!string.IsNullOrEmpty(radius)) SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=false&pt=" + latlon + "&sfield=LatLon&d=" + radius);  //&spatial=true&pt=" + latlon + "&sfield=latlon&d=50"
                else SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=false&pt=" + latlon + "&sfield=LatLon&d=1000000");

                //sort

            }




            //Sort

            SB.Append(sortType("0"));
            
            // Sort Normal score,dist  have Lat,Lon
            // Sort Without Lat,Lon
            // Sort For Category Search








            //defaultsearchfield
            //SB.Append("&df=name,field_l,field_e,sub_district,district,province,category,tag");




            URI = URIAddress + SB.ToString();

        }

        public void createSolrQueryString_admin(ref string URI, string keyword, string latlon, string radius, List<string> category, List<string> tag, string location, string sort, string sub_district, string district, string province, string postcode)
        {

            string URIAddress = System.Configuration.ConfigurationSettings.AppSettings["URI_address"].ToString();
            StringBuilder SB = new StringBuilder();


            //////////////////////////////Detect Language///////////////////////////////////

            //string[] lang = Detect_Language(keyword);
            Regex check_thai = new Regex(@"[ก-ฮฯ-ูเ-ํ๑-๙]+");
            Regex check_english = new Regex(@"[A-Za-z&%]+");

            ////////////////////////////////////////////////////////////////////////////////

            //Keyword And Name  Split by Language
            SB.Append("(");

            //if (keyword != "" & keyword != "*")
            {
                ///////////////////////////////// NAME //////////////////////////////////////////////
                SB.Append("(");
                if (check_english.IsMatch(keyword))
                {
                    keyword = keyword.ToLower().Replace("province", " ");
                    keyword = keyword.ToLower().Replace("subdistrict", " sub district ");
                    SB.Append(insert("Name_English", keyword.Trim(), true, true) + "^2000  ");
                }
                else
                    SB.Append(insert("Name_Local", keyword.Trim(), true, true) + "^2000  ");
                SB.Append(")");

                SB = SB.Replace("\"", "\\\"");
               

            }
            //else SB.Append("*");



            SB.Append(")");

            // PerfectMatch With and
            //if (sub_district + district + province + postcode != "")
            //{
            //    SB.Append(" AND (");
            //    if (sub_district != "") { SB.Append(SolrQueryString(sub_district, "Admin_3")); } else { SB.Append(SolrQueryString("*", "Admin_3")); }
            //    if (district != "") { SB.Append(" AND " + SolrQueryString(district, "Admin_2")); } else { SB.Append(" AND " + SolrQueryString("*", "Admin_2")); }
            //    if (province != "") { SB.Append(" AND " + SolrQueryString(province, "Admin_1")); } else { SB.Append(" AND " + SolrQueryString("*", "Admin_1")); }
            //    if (postcode != "") { SB.Append(" AND " + SolrQueryString(postcode, "PostCode")); } else { SB.Append(" AND " + SolrQueryString("*", "PostCode")); }
            //    SB.Append(")");
            //}


            if (sub_district + district + province != "")
            {
                string admincode = province + district + sub_district;
                SB.Append(" AND (");
                if (sub_district == "") { SB.Append("Admin_code:" + admincode + "*)"); }
                else
                {
                    SB.Append("Admin_code:" + admincode + ")");
                }

            }


            if (!string.IsNullOrEmpty(postcode))
            {
                SB.Append(" AND (PostCode:" + postcode + ") ");
            }


            if (!string.IsNullOrEmpty(location))
            {
                SB.Append(" AND " + SolrQueryString(location, "Admin_4"));
            }
            else { }

            //category
            //if (category != "") { SB.Append("AND " + SolrQueryString(category, "category")); } else { SB.Append("AND " + SolrQueryString("*", "category")); }

            SB.Append(" AND (");
            SB.Append("Layer:ADMIN_POINT");
            SB.Append(")");


            //tag


            //location

            //other parameter
            SB.Append("&defType=edismax");
            SB.Append("&wt=json&indent=true");
            //SB.Append("&mm=3<-1 5<50%25");

            SB.Append("&tie=0");

            SB.Append("&fl=NostraID,house_number,Name_Local,Branch_L,Admin_1_Local,Admin_2_Local,Admin_3_Local,Admin_4_Local,Name_English,Branch_E,Admin_1_English,Admin_2_English,Admin_3_English,Admin_4_English,PostCode,telephone,LatLon,LatLon_Route_1,LatLon_Route_2,LatLon_Route_3,LatLon_Route_4,CategoryReturn,Popularity");

            //latlon
            if (latlon != ",")
            {
                if (!string.IsNullOrEmpty(radius)) SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=false&pt=" + latlon + "&sfield=LatLon&d=" + radius);  //&spatial=true&pt=" + latlon + "&sfield=latlon&d=50"
                else SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=false&pt=" + latlon + "&sfield=LatLon&d=1000000");

            }

            //Sort

            SB.Append(sortType("0"));

            URI = URIAddress + SB.ToString();

        }

        public void createSolrQueryString_intersection(ref string URI, string keyword, string latlon, string radius, List<string> category, List<string> tag, string location, string sort, string sub_district, string district, string province, string postcode)
        {

            string URIAddress = System.Configuration.ConfigurationSettings.AppSettings["URI_address"].ToString();
            StringBuilder SB = new StringBuilder();


            //////////////////////////////Detect Language///////////////////////////////////

            //string[] lang = Detect_Language(keyword);
            Regex check_thai = new Regex(@"[ก-ฮฯ-ูเ-ํ๑-๙]+");
            Regex check_english = new Regex(@"[A-Za-z&%]+");

            ////////////////////////////////////////////////////////////////////////////////

            //Keyword And Name  Split by Language
            SB.Append("(");

            //if (keyword != "" & keyword != "*")
            {
                ///////////////////////////////// NAME //////////////////////////////////////////////
                SB.Append("(");
                if (check_english.IsMatch(keyword))
                {
                    keyword = keyword.ToLower().Replace("province", " ");
                    keyword = keyword.ToLower().Replace("subdistrict", " sub district ");
                    SB.Append(insert("Name_English", keyword.Trim(), true, true) + "^2000  ");
                }
                else
                    SB.Append(insert("Name_Local", keyword.Trim(), true, true) + "^2000  ");
                SB.Append(")");

                SB = SB.Replace("\"", "\\\"");


            }



            SB.Append(")");


            if (sub_district + district + province != "")
            {
                string admincode = province + district + sub_district;
                SB.Append(" AND (");
                if (sub_district == "") { SB.Append("Admin_code:" + admincode + "*)"); }
                else
                {
                    SB.Append("Admin_code:" + admincode + ")");
                }

            }


            if (!string.IsNullOrEmpty(postcode))
            {
                SB.Append(" AND (PostCode:" + postcode + ") ");
            }


            if (!string.IsNullOrEmpty(location))
            {
                SB.Append(" AND " + SolrQueryString(location, "Admin_4"));
            }
            else { }

            //category
            //if (category != "") { SB.Append("AND " + SolrQueryString(category, "category")); } else { SB.Append("AND " + SolrQueryString("*", "category")); }

            SB.Append(" AND (");
            SB.Append("Layer:INTERSECTION");
            SB.Append(")");


            //tag


            //location

            //other parameter
            SB.Append("&defType=edismax");
            SB.Append("&wt=json&indent=true");
            //SB.Append("&mm=3<-1 5<50%25");

            SB.Append("&tie=0");

            SB.Append("&fl=NostraID,house_number,Name_Local,Branch_L,Admin_1_Local,Admin_2_Local,Admin_3_Local,Admin_4_Local,Name_English,Branch_E,Admin_1_English,Admin_2_English,Admin_3_English,Admin_4_English,PostCode,telephone,LatLon,LatLon_Route_1,LatLon_Route_2,LatLon_Route_3,LatLon_Route_4,CategoryReturn,Popularity");

            //latlon
            if (latlon != ",")
            {
                if (!string.IsNullOrEmpty(radius)) SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=false&pt=" + latlon + "&sfield=LatLon&d=" + radius);  //&spatial=true&pt=" + latlon + "&sfield=latlon&d=50"
                else SB.Append(",dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=false&pt=" + latlon + "&sfield=LatLon&d=1000000");

            }

            //Sort

            SB.Append(sortType("0"));

            URI = URIAddress + SB.ToString();

        }

        public DataTable CallSolr_DT(string URIAddress)
        {
           
            System.Net.WebRequest req = System.Net.WebRequest.Create(URIAddress);
            System.Net.WebResponse response = req.GetResponse();
            System.IO.StreamReader SReader = new System.IO.StreamReader(response.GetResponseStream());

            String Read = SReader.ReadToEnd().Trim();


            int len = Read.IndexOf("[");
            int cut = Read.LastIndexOf("]");
            Read = Read.Remove(0, len);
            Read = Read.Remove(Read.Length - 2, 2);
            
            DataTable res_DT = JsonConvert.DeserializeObject<DataTable>(Read);

            try 
            { 
                for(int i=0 ;res_DT.Rows.Count > i ;i++)
                {
                    res_DT.Rows[i]["CategoryReturn"] = res_DT.Rows[i]["CategoryReturn"].ToString().ToUpper();
                }
            }
            catch
            { }


            return res_DT;
        }

        public string SolrQueryString(string input, string type)
        {
            switch (type)
            {
                case "Admin_3":
                    {
                        return "(Admin_3:" + input + ")";

                    }
                case "Admin_2":
                    {
                        return "(Admin_2:" + input + ")";
                    }
                case "Admin_1":
                    {
                        return "(Admin_1:" + input + ")";
                    }
                case "Category":
                    {
                        return "(Category:" + input + ")";
                    }
                case "Tag":
                    {
                        return "(Name_Local:\"" + input + "\")";
                    }
                case "Name_Local":
                    {
                        return "(Name_Local:(" + input + "))";
                    }
                case "Name_English":
                    {
                        return "(Name_English:(" + input + "))";
                    }
                case "keyword":
                    {
                        return "(Description_Local:(" + input + "))^1.05 OR (Description_English:(" + input + ")^1.05 )";
                    }
                case "Admin_4":
                    {
                        return "(Admin_4:(" + input + "))";
                    }
                case "PostCode":
                    {
                        return "(PostCode:" + input + ")";
                    }

                default:
                    {
                        return "";
                    }
            }
        }

        // For Chose when to sort by distance or relevance   if numfound > 1000  must be distance;
        public int CheckNumFound(string URIAddress)
        {
            int found = 0;
            try
            {
                System.Net.WebRequest req = System.Net.WebRequest.Create(@URIAddress);
                System.Net.WebResponse response = req.GetResponse();
                System.IO.StreamReader SReader = new System.IO.StreamReader(response.GetResponseStream());

                String Read = SReader.ReadToEnd().Trim();
               
                try
                {
                    var sz = (JObject)JsonConvert.DeserializeObject(Read);
                    found = (int)sz["response"]["numFound"];
                }
                catch (Exception e) { }
            }
            catch (Exception e) { }
            


            return found;
        }

        //search with synonym only
        public DataTable synonymSearch(string keyword, string latlon, string row, string page, ref int check,List<string> category)
        {
            string URIAddress = System.Configuration.ConfigurationSettings.AppSettings["URI_address"].ToString();
            StringBuilder SB = new StringBuilder();
            //Fix Bug Page Number
            
            if (page == "1") page = "0";
            else page = int.Parse(page) - 1 + "";

    


            SB.Append(@"synonym:" + keyword.Trim().Replace("&","%26"));

            string catfix = "";

            //category
            //if (category != "") { SB.Append("AND " + SolrQueryString(category, "category")); } else { SB.Append("AND " + SolrQueryString("*", "category")); }
            if (category.Count() > 0)
            {
                if (category.Count() == 1 && category[0] == "") { }
                else
                {
                    SB.Append(" AND (");
                    catfix = " AND (";
                    for (int i = 0; i < category.Count(); i++)
                    {

                        SB.Append(SolrQueryString(category[i], "Category"));
                        catfix += "Category:" + category[i];
                        if (i + 1 < category.Count()) { 
                            SB.Append(" OR ");
                            catfix += " OR ";
                        }

                    }
                    SB.Append(")");
                    catfix += ")";
                }


            }




            //other parameter
            SB.Append("&wt=json&indent=true");
            SB.Append("&fl=NostraID,house_number,Name_Local,Branch_L,Admin_1_Local,Admin_2_Local,Admin_3_Local,Admin_4_Local,Name_English,Branch_E,Admin_1_English,Admin_2_English,Admin_3_English,Admin_4_English,PostCode,telephone,LatLon,LatLon_Route_1,LatLon_Route_2,LatLon_Route_3,LatLon_Route_4,CategoryReturn");
             string URI = URIAddress + SB.ToString();

             check = CheckNumFound(URI);
             
             DataTable temp = new DataTable();

             try
             {
                 if (check > 20 & latlon != ",") // Sort by distance
                 {
                     temp = CallSolr_DT(URIAddress + "(synonym:" + keyword.Trim().Replace("&", "%26") + ")"+catfix+"&fl=NostraID,house_number,Name_Local,Branch_L,Admin_1_Local,Admin_2_Local,Admin_3_Local,Admin_4_Local,Name_English,Branch_E,Admin_1_English,Admin_2_English,Admin_3_English,Admin_4_English,PostCode,telephone,LatLon,LatLon_Route_1,LatLon_Route_2,LatLon_Route_3,LatLon_Route_4,CategoryReturn,dist:geodist()&fq={!geofilt sfield=LatLon}&spatial=true&pt=" + latlon + "&sfield=LatLon&d=20&start=" + int.Parse(row) * (int.Parse(page)) + "&rows=" + row + "&wt=json&indent=true&sort=geodist() asc");
                 }
                 else // Sort by relevance
                 {
                     temp = CallSolr_DT(URIAddress + "(synonym:" + keyword.Trim().Replace("&", "%26") + ")" + catfix + "&fl=NostraID,house_number,Name_Local,Branch_L,Admin_1_Local,Admin_2_Local,Admin_3_Local,Admin_4_Local,Name_English,Branch_E,Admin_1_English,Admin_2_English,Admin_3_English,Admin_4_English,PostCode,telephone,LatLon,LatLon_Route_1,LatLon_Route_2,LatLon_Route_3,LatLon_Route_4,CategoryReturn&start=" + int.Parse(row) * (int.Parse(page)) + "&rows=" + row + "&wt=json&indent=true");
                 }
             }
             catch (Exception e) { }
             return temp;
        }


        private string[] Detect_Language(string Keyword)
        {
            string[] result = new string[3];

            Regex check_thai = new Regex(@"[ก-ฮฯ-ูเ-ํ๑-๙]+");  //
            Regex check_english = new Regex(@"[A-Za-z&%]+");
            Regex check_number = new Regex(@"[0-9]+");


            List<string> list = Keyword.Split(' ').ToList();

            for (int i = 0; i < list.Count; i++)
            {

                while (list[i].Length != 0)
                {
                    if (check_thai.IsMatch(list[i]))
                    {
                        result[0] += " " + check_thai.Match(list[i]);
                        list[i] = list[i].Replace(check_thai.Match(list[i]).ToString(), " ");
                        continue;
                    }
                    if (check_english.IsMatch(list[i]))
                    {
                        result[1] += " " + check_english.Match(list[i]);
                        list[i] = list[i].Replace(check_english.Match(list[i]).ToString(), " ");
                        continue;
                    }
                    if (check_number.IsMatch(list[i]))
                    {
                        result[0] += " " + check_number.Match(list[i]);
                        result[1] += " " + check_number.Match(list[i]);
                        result[2] += " " + check_number.Match(list[i]);
                        list[i] = list[i].Replace(check_number.Match(list[i]).ToString(), "");
                        continue;
                    }
                    list[i] = "";
                }
            }
            try { result[0] = result[0].Trim(); }
            catch (Exception) { }
            try { result[1] = result[1].Trim(); }
            catch (Exception) { }
            try { result[2] = result[1].Trim(); }
            catch (Exception) { }

            try
            {
                if (!check_thai.IsMatch(result[0]))
                {
                    result[0] = "";
                }
            }
            catch (Exception e) { }
            try
            {
                if (!check_english.IsMatch(result[1]))
                {
                    result[1] = "";
                }
            }
            catch (Exception e) { }


            /////////FIX NUMBER JUMP JUMP
            //try
            //{

            //    if (!check_english.IsMatch(result[1]))
            //    {
            //        result[1] = "";
            //    }
            //}
            //catch (Exception e) { }

            try
            {
                result[0] = result[0].Replace("&", "%26");
            }
            catch (Exception e) { }


            try
            {
                result[1] = result[1].Replace("&", "%26");
            }
            catch (Exception e) { }
            /*
            try
            {
                if (check_number.IsMatch(result[1]))
                {
                    if (check_english.IsMatch(result[1]) != true)
                    {
                        result[1] = "";
                    }
                }
            }
            catch (Exception e) { }
            */

            return result;
        }


        private string DecryptRijndael(string cipherText, string salt)
        {

            if (string.IsNullOrEmpty(cipherText))
                throw new ArgumentNullException("cipherText");

            cipherText = cipherText.Replace("-", "+");
            string text;

            var aesAlg = NewRijndaelManaged(salt);
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var cipher = Convert.FromBase64String(cipherText);

            using (var msDecrypt = new MemoryStream(cipher))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        text = srDecrypt.ReadToEnd();
                    }
                }
            }
            return text;

        }

        private static RijndaelManaged NewRijndaelManaged(string salt)
        {
            if (salt == null) throw new ArgumentNullException("salt");
            var saltBytes = Encoding.ASCII.GetBytes(salt);
            var key = new Rfc2898DeriveBytes("NostraGT", saltBytes);

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

            return aesAlg;
        }

        private static string EncryptRijndael(string text, string salt)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            var aesAlg = NewRijndaelManaged(salt);

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(text);
            }

            return Convert.ToBase64String(msEncrypt.ToArray()).Replace("+", "-");
        }



    }
}