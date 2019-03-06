using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.SessionState;
using System.Text.RegularExpressions;
using System.IO;
using GT_Sockets;
using System.Diagnostics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;


namespace Search_Service_POI
{
    /// <summary>
    /// Summary description for Search_Landmark
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Search_Landmark : System.Web.Services.WebService
    {
        //static _cl_synonym_replace syn = new _cl_synonym_replace(@"C:\Users\GT_4953\Desktop\synonym\replace.txt", @"C:\Users\GT_4953\Desktop\synonym\add.txt");
        //static _cl_synonym_replace syn = new _cl_synonym_replace(@System.Configuration.ConfigurationSettings.AppSettings["synoFolder"].ToString() + @"\replace.txt", @System.Configuration.ConfigurationSettings.AppSettings["synoFolder"].ToString() + @"\add.txt");
        static _cl_synonym_replace syn = new _cl_synonym_replace(@System.Configuration.ConfigurationSettings.AppSettings["synoFolder"].ToString() + @"\synonym.txt");
        static _cl_map map_public = new _cl_map(@System.Configuration.ConfigurationSettings.AppSettings["shp_public"].ToString(), @System.Configuration.ConfigurationSettings.AppSettings["dbf_public"].ToString(),Encoding.UTF8);
        static _cl_map map_admin = new _cl_map(@System.Configuration.ConfigurationSettings.AppSettings["shp_admin"].ToString(), @System.Configuration.ConfigurationSettings.AppSettings["dbf_admin"].ToString(), Encoding.UTF8);//Encoding.GetEncoding("windows-874"));// 
     
        [WebMethod(Description = "Search_TH_Version3", EnableSession = true)]
        public string Search(string keyword, string AdminLevel3, string AdminLevel2, string AdminLevel1, string PostCode, string AdminLevel4, string category, string LocalCatCode, string tag, string lat, string lon, string radius, string RowsPerPage, string PageNumber, string token)
        {
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("th-TH");
            //Filter parameter
            if (string.IsNullOrEmpty(RowsPerPage))
                RowsPerPage = "20";
            if (string.IsNullOrEmpty(PageNumber))
                PageNumber = "1";

            int indexEnd = Convert.ToInt32(RowsPerPage) * Convert.ToInt32(PageNumber);
            int indexStart = (indexEnd - Convert.ToInt32(RowsPerPage) ) +1;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            //Check Permission
            if (check_authen(token, GetIP(), "1")) { }
            else
            {
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("You don't have permission.");
                connect.TableName = "Fail to Connect";
                return null;
            }
            
            try
            {
                string rs = "";
                if (!string.IsNullOrEmpty(radius))
                {
                    double r = Convert.ToDouble(radius);
                    r = r * 1000;
                    rs = r.ToString();
                }
                GT_Socket gt_socket = new GT_Socket();

                // Initial server information
                //gt_socket.Server = "192.168.1.67";//"globetech.gps.be-mobile.biz"; //
                //gt_socket.Port = 10002;//8085;//
               //gt_socket.Server = "127.0.0.1";
                gt_socket.Server = "127.0.0.1";
                gt_socket.Port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString());
                gt_socket.ProviderID = 112;

                gt_socket.Connect();

                string syno_replace = syn.Dup(keyword.Trim());

                string message = "";
                message += "1;";//type
                message += keyword.Trim() + ";";
                message += syno_replace.Trim() + ";";
                message += ";";//th
                message += ";";//other
                message += ";";//nostraid
                message += AdminLevel4 + ";";//adminlv4
                message += AdminLevel3 + ";";//adminlv3
                message += AdminLevel2 + ";";//adminlv2
                message += AdminLevel1 + ";";//adminlv1

                message += category + ";";//cat
                message += LocalCatCode + ";";//subcode,localCatCode
                message += tag + ";";//tag
                message += PostCode + ";";//PostCode

                message += lat + ";";//lat
                message += lon + ";";//lon
                message += rs + ";";//r
                message += System.Configuration.ConfigurationSettings.AppSettings["MaxReturnSearch"].ToString().Trim();//return

                bool stat = gt_socket.SendData(message + " \n", false);
                string recieve = gt_socket.recieve(10000 * 60);
                int kk = recieve.Length;
                //string recieve2 = gt_socket.recieve(10000 * 60);
                
                gt_socket.Disconnect();

                //recieve += recieve2;
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
                for(int i = 0;i<dt.Rows.Count ;i++)
                {
                    dt.Rows[i]["No"] = indexStart.ToString();
                    indexStart++;
                }
                
                //return recieve;

                long ij = sw.ElapsedMilliseconds;
                dt.TableName = "res" + ij.ToString() + "ms";
                dt.Columns.Remove("hitscore");
                dt.Columns.Remove("NAME_THEN");
                dt.Columns.Remove("NAME_ENTH");
                dt.Columns.Remove("text_search");
                dt.Columns.Remove("Name_E_sort");
                dt.Columns.Remove("Name_L_sort");
                dt.Columns.Remove("Sort_Group");
                dt.Columns.Remove("table");

                //dt.Columns["Catcode"].ColumnName = "CatCode";
                //dt.Columns["dist"].ColumnName = "Dist";
                //dt.Columns["score"].ColumnName = "Score";
                //dt.Columns["subcode"].ColumnName = "LocalCatCode";

                dt.Columns.Remove("HouseNo");
                dt.Columns.Remove("LatLon");
                dt.Columns.Remove("LatLon_Route1");
                dt.Columns.Remove("LatLon_Route2");
                dt.Columns.Remove("LatLon_Route3");
                dt.Columns.Remove("LatLon_Route4");
                

                dt.TableName = "result";
                dt.AcceptChanges();

                
                return csvF(dt);
            }
            catch (Exception e) 
            {
                //DataTable result = new DataTable();
                //result.TableName = "Error";
                //result.Columns.Add("Problem");
                //result.Rows.Add(e);
                //return result;

                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("NotFound");
                connect.TableName = "NotFound";
                return null;
            }
        }



        [WebMethod(Description = "Auto Complete")]
        public DataTable AutoComplete(string keyword, string numreturn, string token)
        {

            if (check_authen(token, GetIP(), "1"))
            {

            }
            else
            {
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("You don't have permission.");
                connect.TableName = "Fail to Connect";
                return connect;
            }



            //string select = "";
            string message = "";
            string[] lang = Detect_Language(keyword);
            if (string.IsNullOrEmpty(lang[0]))
            {
                message += "3;";//eng
            }
            else
            {
                message += "2;";//thai
            }


            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                GT_Socket gt_socket = new GT_Socket();

                gt_socket.Server = "127.0.0.1";
                gt_socket.Port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString());
                gt_socket.ProviderID = 112;

                gt_socket.Connect();

                message += keyword.Trim() + ";";
                message += numreturn.ToString();

                bool stat = gt_socket.SendData(message + " \n", false);
                string recieve = gt_socket.recieve(10000 * 60);
                int kk = recieve.Length;

                gt_socket.Disconnect();

                string[] line = recieve.Split('|');

                DataTable dt = new DataTable();
                dt.Columns.Add("name", typeof(string));

                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i].ToUpper().Trim() == "NULL")
                        break;
                    dt.Rows.Add(line[i]);  
                }

                //return recieve;

                long ij = sw.ElapsedMilliseconds;
                dt.TableName = "res" + ij.ToString() + "ms";
                return dt;
            }
            catch (Exception)
            {
                    DataTable connect = new DataTable();
                    connect.Columns.Add("result");
                    connect.Rows.Add("NotFound");
                    connect.TableName = "NotFound";
                    return connect;
            }


        }
        /*
        [WebMethod(Description="Iden")]
        public DataTable Identify(string lat,string lon,string token)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (check_authen(token, GetIP(), "1"))
            {

            }
            else
            {
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("You don't have permission.");
                connect.TableName = "Fail to Connect";
                return connect;
            }

            string message = "4;";
            

            try
            {
                
                
                DataTable dt_public = map_public.polygon(Convert.ToDouble(lat), Convert.ToDouble(lon),);
                DataTable dt_admin = map_admin.polygon(Convert.ToDouble(lat), Convert.ToDouble(lon));

                dt_admin.TableName = "result_" + sw.ElapsedMilliseconds.ToString();

                //return dt_admin;

                GT_Socket gt_socket = new GT_Socket();

                gt_socket.Server = "127.0.0.1";
                gt_socket.Port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString());
                gt_socket.ProviderID = 112;

                gt_socket.Connect();

                message += lat.Trim() + ";";
                message += lon.Trim()+ ";";
                message += System.Configuration.ConfigurationSettings.AppSettings["IdenBuffer"].ToString().Trim() + ";";

                bool stat = gt_socket.SendData(message + " \n", false);
                string recieve = gt_socket.recieve(10000 * 60);
                int kk = recieve.Length;

                gt_socket.Disconnect();

                string[] line = recieve.Split('|');

                DataTable dt = new DataTable();
                string[] header = line[0].Split('^');
                for (int i = 0; i < header.Length; i++)
                {
                    dt.Columns.Add(header[i], typeof(string));
                }

                for (int i = 1; i < line.Length; i++)
                {
                    if (line[i].Trim() == "")
                        break;
                    string[] tmp = line[i].Split('^');
                    dt.Rows.Add(tmp);
                }

                //return recieve;

                long ij = sw.ElapsedMilliseconds;
                dt.TableName = "res" + ij.ToString() + "ms";
                
                if(dt != null)
                {
                    if(dt.Rows.Count ==1)
                    {
                        if (dt_public != null)
                        {
                            if (dt_public.Rows[0]["pid"].ToString().Trim() == dt.Rows[0]["public_id"].ToString().Trim())
                                return IdenFormat(dt,"solr",dt_admin);
                            else
                                return IdenFormat(dt_public, "public", dt_admin);
                        }
                        else
                            return IdenFormat(dt, "solr", dt_admin);
                    }
                }


                if (dt_public != null)
                    return IdenFormat(dt_public, "public", dt_admin);
                else
                    return IdenFormat(dt_admin, "admin", dt_admin); ;

                //ServiceReference1.Search_LandmarkSoapClient s = new ServiceReference1.Search_LandmarkSoapClient("Search_LandmarkSoap");
                //return s.Identify(lat, lon, "F@rT3stS3@rchTh@1");
            }
            catch (Exception e)
            {
                DataTable result = new DataTable();
                result.TableName = "Error";
                result.Columns.Add("Problem");
                result.Rows.Add(e);
                return result;
            }
        }
        */
        [WebMethod(Description="Search Nearby")]
        public string Search_Nearby(string lat,string lon,string distance,string numreturn,string token)
       {
           try
           {
               if (token == "43f9952b429485e270ccf674f")
               {
                   
                   double dist = (double.Parse(distance) / 1000);

                   _c_function fn = new _c_function();

                   string latlon = lat + "," + lon;
                   string URI = "";
                   List<string> li = new List<string>();
                   fn.createSolrQueryString(ref URI, "*", latlon, dist.ToString(), li, li, "", "5", "", "", "", "");
                   URI = URI + "&start=0&rows=" + numreturn;

                   DataTable result = fn.CallSolr_DT(URI);


                   StringBuilder sb = new StringBuilder();

                   try
                   {
                       if (result.Rows.Count==0)
                       {
                           return "NotFound";
                       }
                   }
                   catch (Exception e) { }

                   for (int i = 0; i < result.Rows.Count; i++)
                   {

                       sb.Append(result.Rows[i]["Name_Local"] + "!" + result.Rows[i]["Name_English"] + "!" + (1000*double.Parse(result.Rows[i]["dist"].ToString())).ToString("F2"));
                       sb.Append("|");
                   }

                   return sb.ToString();
               }
               else
               {
                   return "you don't have permission";
               }
           }
           catch (Exception) {
               return "Error in code Please contact developer.";
           }
       }

        [WebMethod(Description = "SumPoiByCat")]
        public DataTable SumPoiByCat(string Catcode, string lat, string lon, string radius, string token)
        {

            if (check_authen(token, GetIP(), "1"))
            {

            }
            else
            {
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("You don't have permission.");
                connect.TableName = "Fail to Connect";
                return connect;
            }


            try
            {
                double r = Convert.ToDouble(radius);
                r = r * 1000;
                int buffer = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["SumBuffer"].ToString());
                if (r > buffer)
                    r = buffer;
                _c_function fn = new _c_function();
                string[] input = Catcode.Split('|');
                DataTable dt = new DataTable();
                dt.TableName = "Result";
                dt.Columns.Add("No", typeof(int));
                dt.Columns.Add("Catcode", typeof(string));
                dt.Columns.Add("sum", typeof(int));

                string[] tmp = Catcode.Split('|');

                for (int i = 0; i < tmp.Length; i++)
                {
                    if (!string.IsNullOrEmpty(tmp[i]))
                    {
                        if (sp_catcode(tmp[i]))
                        {
                            string[] tmp_s = sp_catcode_split(tmp[i]);
                            int sum = 0;
                            for (int j = 0; j < tmp_s.Length; j++)
                            {
                                GT_Socket gt_socket = new GT_Socket();

                                gt_socket.Server = "127.0.0.1";
                                gt_socket.Port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString());
                                gt_socket.ProviderID = 112;

                                gt_socket.Connect();
                                string message = "5;";
                                message += lat.Trim() + ";";
                                message += lon.Trim() + ";";
                                message += r.ToString().Trim() + ";";
                                message += tmp_s[j].ToString().Trim();

                                gt_socket.SendData(message + " \n", false);
                                string recieve = gt_socket.recieve(10000 * 60);
                                int z = Convert.ToInt32(recieve);
                                sum += z;
                            }
                            DataRow dr = dt.NewRow();
                            dr["No"] = i + 1;
                            dr["Catcode"] = tmp[i].ToString();
                            dr["sum"] = sum.ToString();
                            dt.Rows.Add(dr);
                        }
                        else
                        {
                            GT_Socket gt_socket = new GT_Socket();

                            gt_socket.Server = "127.0.0.1";
                            gt_socket.Port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString());
                            gt_socket.ProviderID = 112;

                            gt_socket.Connect();
                            string message = "5;";
                            message += lat.Trim() + ";";
                            message += lon.Trim() + ";";
                            message += r.ToString().Trim() + ";";
                            message += tmp[i].ToString().Trim();

                            gt_socket.SendData(message + " \n", false);
                            string recieve = gt_socket.recieve(10000 * 60);

                            gt_socket.Disconnect();
                            DataRow dr = dt.NewRow();
                            dr["No"] = i + 1;
                            dr["Catcode"] = tmp[i].ToString();
                            dr["sum"] = recieve.ToString().Trim();
                            dt.Rows.Add(dr);
                        }
                    }
                }

                
                return dt;

            }
            catch (Exception e)
            {
                DataTable result = new DataTable();
                result.TableName = "Error";
                result.Columns.Add("Problem");
                result.Rows.Add(e);
                return result;
            }
        }

        [WebMethod(Description = "SumPOIByLocalCat")]
        public DataTable SumPOIByLocalCat(string LocalCatCode, string lat, string lon, string radius, string token)
        {

            if (check_authen(token, GetIP(), "1"))
            {

            }
            else
            {
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("You don't have permission.");
                connect.TableName = "Fail to Connect";
                return connect;
            }


            try
            {
                double r = Convert.ToDouble(radius);
                r = r * 1000;
                int buffer = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["SumBuffer"].ToString());
                if (r > buffer)
                    r = buffer;
                _c_function fn = new _c_function();
                string[] input = LocalCatCode.Split('|');
                DataTable dt = new DataTable();
                dt.TableName = "Result";
                dt.Columns.Add("No", typeof(int));
                dt.Columns.Add("LocalCatCode", typeof(string));
                dt.Columns.Add("sum", typeof(int));

                string[] tmp = LocalCatCode.Split('|');

                for (int i = 0; i < tmp.Length; i++)
                {
                    if (!string.IsNullOrEmpty(tmp[i]))
                    {
                        {
                            GT_Socket gt_socket = new GT_Socket();

                            gt_socket.Server = "127.0.0.1";
                            gt_socket.Port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString());
                            gt_socket.ProviderID = 112;

                            gt_socket.Connect();
                            string message = "6;";
                            message += lat.Trim() + ";";
                            message += lon.Trim() + ";";
                            message += r.ToString().Trim() + ";";
                            message += tmp[i].ToString().Trim();

                            gt_socket.SendData(message + " \n", false);
                            string recieve = gt_socket.recieve(10000 * 60);

                            gt_socket.Disconnect();
                            DataRow dr = dt.NewRow();
                            dr["No"] = i + 1;
                            dr["LocalCatCode"] = tmp[i].ToString();
                            dr["sum"] = recieve.ToString().Trim();
                            dt.Rows.Add(dr);
                        }
                    }
                }


                return dt;

            }
            catch (Exception e)
            {
                DataTable result = new DataTable();
                result.TableName = "Error";
                result.Columns.Add("Problem");
                result.Rows.Add(e);
                return result;
            }
        }

        private bool sp_catcode (string input)
        {
            if (input == "FOOD")
                return true;
            else if (input == "SHOPPING")
                return true;
            else if (input == "FUEL")
                return true;
            else
                return false;
        }

        private string[] sp_catcode_split(string input)
        {
            if (input == "FOOD")
            {
                string[] tmp = new string[] { "FOOD-BAKECOF", "FOOD-FASTFOOD", "FOOD-FOODSHOP", "FOOD-PUB", "FOOD-RESRAURANT" };
                return tmp;
            } 
            else if (input == "SHOPPING")
            {
                string[] tmp = new string[] { "SHOPPING-CONVSTORE", "SHOPPING-DEPTSTORE", "SHOPPING-MARKET", "SHOPPING-STORE" };
                return tmp;
            } 
            else if (input == "FUEL")
            {
                string[] tmp = new string[] { "FUEL-LPG", "FUEL-NGP", "FUEL-OIL", "FUEL-EV" };
                return tmp;
            } 
            else
                return null;
        }
        private int count_category_by_distance (string lat,string lon,string cat,string distance)
        {
            int count = 0;
            try
            {
                StringBuilder URL = new StringBuilder();

                URL.Append(System.Configuration.ConfigurationSettings.AppSettings["URI_address"]);

                if (cat != "") URL.Append("(category :\"" + cat + "\")");
                else URL.Append("(category:*)");


                URL.Append("&sort=geodist()+asc&fq={!geofilt sfield=latlon}&rows=0&fl=nostraid,geodist()&wt=json&indent=true&spatial=true&sfield=latlon");
                URL.Append("&pt=" + lat + "," + lon);
                URL.Append("&d=" + distance);

                String URI = URL.ToString();

                System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
                System.Net.WebResponse response = req.GetResponse();
                System.IO.StreamReader SReader = new System.IO.StreamReader(response.GetResponseStream());

                String Read = SReader.ReadToEnd().Trim();
                int len = Read.IndexOf("numFound\":")+10;
                Read = Read.Remove(0, len);
                len = Read.IndexOf(",");
                Read = Read.Remove(len);
                count = int.Parse(Read);
            }
            catch(Exception)
            {

            }
            return count;
        }

         
        private int count_category_by_boundary(string cat, string sub_district,string district,string province)
        {
            int count = 0;
            try
            {
                StringBuilder URL = new StringBuilder();

                URL.Append(System.Configuration.ConfigurationSettings.AppSettings["URI_address"]);

                if (cat != "") URL.Append("(category :\"" + cat + "\")");
                else URL.Append("(category:*)");

                if (sub_district + district + province != "")
                {
                    URL.Append(" AND (");
                    if (sub_district != "") { URL.Append("(sub_district:" + sub_district + ")"); } else { URL.Append("(sub_district:*)"); }
                    if (district != "") { URL.Append(" AND (district:" + district + ")"); } else { URL.Append(" AND (district:*)"); }
                    if (province != "") { URL.Append(" AND (province:" + province + ")"); } else { URL.Append(" AND (province:*)"); }
                    URL.Append(")");
                  
                }


                URL.Append("&rows=0&fl=nostraid&wt=json&indent=true");
                //URL.Append("&pt=" + lat + "," + lon);
                //URL.Append("&d=" + distance);

                String URI = URL.ToString();

                System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
                System.Net.WebResponse response = req.GetResponse();
                System.IO.StreamReader SReader = new System.IO.StreamReader(response.GetResponseStream());

                String Read = SReader.ReadToEnd().Trim();
                int len = Read.IndexOf("numFound\":") + 10;
                Read = Read.Remove(0, len);
                len = Read.IndexOf(",");
                Read = Read.Remove(len);
                count = int.Parse(Read);
            }
            catch (Exception)
            {

            }
            return count;
        }




        private string[] Detect_Language(string Keyword)
        {
            string[] result = new string[2];

            Regex check_thai = new Regex(@"[ก-ฮฯ-ูเ-ํ๑-๙]+");  //
            Regex check_english = new Regex(@"[A-Za-z]+");
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

           
            return result;
        }

        private bool check_authen(string token, string ip, string type)
        {
            bool permission = false;
            //return true;

            try
            {
                string[] IP_list = System.Configuration.ConfigurationSettings.AppSettings["IP_permission"].ToString().Split('|');
                if( IP_list.Contains(ip))
                {
                    permission = true;
                }
                /*
                string ConnectionString = System.Configuration.ConfigurationSettings.AppSettings["ConStringPermission"].ToString();
                //string ConnectionString = "Server=localhost;UID=gt_apichai;PASSWORD=@dmin123;database=AddressSearch;Max Pool Size=400;Connect Timeout=900;";
               SqlConnection con = new SqlConnection(ConnectionString);
                con.Open();
                SqlCommand SQLCmd = new SqlCommand("SELECT top(1) * FROM [dbo].[authentication] where Authentication = N'" + ip + "' AND Type = N'" + type + "';", con);

                SqlDataReader dt = SQLCmd.ExecuteReader();
                dt.Read();

                if (dt["Authentication"].ToString() == ip && DateTime.Now < Convert.ToDateTime(dt["expireDate"]) && type == dt["type"].ToString())
                {
                    permission = true;
                }

                con.Close();
                */
            }
            catch (Exception e) { }


            //if (token == "@pich@idokm@i123")
            if (token == "F@rT3stS3@rchTh@1")
            {
                permission = true;
            }
           


            return permission;
        }

        [WebMethod(Description = "IP")]
        public string GetIP()
        {
            String ip =
                HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }

        //[WebMethod(Description = "moo")]
        public string Moo(string server,int port)
        {
            
            try
            {
                Socket s = null;
                IPHostEntry hostEntry = null;

                // Get host related information.
                hostEntry = Dns.GetHostEntry(server);

                // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid 
                // an exception that occurs when the host IP Address is not compatible with the address family 
                // (typical in the IPv6 case). 
                foreach (IPAddress address in hostEntry.AddressList)
                {
                    IPEndPoint ipe = new IPEndPoint(address, port);
                    if (ipe.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ipe.Address.ToString();

                        //Socket tempSocket =
                        //    new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                        //tempSocket.Connect(ipe);

                        //if (tempSocket.Connected)
                        //{
                        //    s = tempSocket;
                        //    break;
                        //}
                        //else
                        //{
                        //    continue;
                        //}
                    }
                }
                return "Noip";
            }catch(Exception ex)
            {
                throw new Exception("Error : " + ex.Message);
            }
            
        }

        private DataTable IdenFormat(DataTable dt , string mode,DataTable dt_admin)
        {
            if(mode == "public")
            {
                //dt.Columns.Add("NostraId");

                dt.Columns.Add("Name_L");
                dt.Columns.Add("Name_E");
                dt.Columns.Add("Branch_E");
                dt.Columns.Add("Branch_L");

                dt.Columns.Add("AdminLevel1_E");
                dt.Columns.Add("AdminLevel2_E");
                dt.Columns.Add("AdminLevel3_E");
                dt.Columns.Add("AdminLevel4_E");

                dt.Columns.Add("AdminLevel1_L");
                dt.Columns.Add("AdminLevel2_L");
                dt.Columns.Add("AdminLevel3_L");
                dt.Columns.Add("AdminLevel4_L");

                dt.Columns.Add("HouseNo");
                dt.Columns.Add("Telephone");
                //dt.Columns.Add("Catcode");
                dt.Columns.Add("LocalCatCode");
                dt.Columns.Add("dist");
                dt.Columns.Add("No");


                dt.Rows[0]["dist"] = "0";

                dt.Rows[0]["Name_L"] = dt.Rows[0]["NAMT"];
                dt.Columns.Remove("NAMT");
                dt.Rows[0]["Name_E"] = dt.Rows[0]["NAME"];
                dt.Columns.Remove("NAME");

                dt.Rows[0]["AdminLevel1_E"] = dt_admin.Rows[0]["prov_name"];
                dt.Rows[0]["AdminLevel2_E"] = dt_admin.Rows[0]["amp_name"];
                dt.Rows[0]["AdminLevel3_E"] = dt_admin.Rows[0]["tam_name"];
                dt.Rows[0]["AdminLevel4_E"] = "";

                dt.Rows[0]["AdminLevel1_L"] = dt_admin.Rows[0]["prov_namt"];
                dt.Rows[0]["AdminLevel2_L"] = dt_admin.Rows[0]["amp_namt"];
                dt.Rows[0]["AdminLevel3_L"] = dt_admin.Rows[0]["tam_namt"];
                dt.Rows[0]["AdminLevel4_L"] = "";

                dt.Rows[0]["LocalCatCode"] = dt.Rows[0]["sub_code"];
                
                dt.Columns.Remove("sub_code");

                dt.Columns.Remove("objectid");

                dt.Columns.Remove("LDMTAG");
                dt.Columns.Remove("TYPE");
                dt.Columns.Remove("V22_2");
                dt.Columns.Remove("AREATAG");
                dt.Columns.Remove("POST_RD_T");

                dt.Columns.Remove("PROV_CODE");
                dt.Columns.Remove("AMP_CODE");
                dt.Columns.Remove("TAM_CODE");

                dt.Columns.Remove("PROV_NAME");
                dt.Columns.Remove("AMP_NAME");
                dt.Columns.Remove("TAM_NAME");

                dt.Columns.Remove("PROV_NAMT");
                dt.Columns.Remove("AMP_NAMT");
                dt.Columns.Remove("TAM_NAMT");

                dt.Columns.Remove("VERSION");
                //dt.Columns.Remove("pid");

                dt.Columns["POSTCODE"].ColumnName = "Postcode";
                dt.Columns["catcode"].ColumnName = "CatCode";
                dt.Columns["pid"].ColumnName = "NostraId";

            }
            else if(mode == "admin")
            {
                dt.Columns.Add("NostraId");

                dt.Columns.Add("Name_L");
                dt.Columns.Add("Name_E");
                dt.Columns.Add("Branch_E");
                dt.Columns.Add("Branch_L");

                dt.Columns.Add("AdminLevel1_E");
                dt.Columns.Add("AdminLevel2_E");
                dt.Columns.Add("AdminLevel3_E");
                dt.Columns.Add("AdminLevel4_E");

                dt.Columns.Add("AdminLevel1_L");
                dt.Columns.Add("AdminLevel2_L");
                dt.Columns.Add("AdminLevel3_L");
                dt.Columns.Add("AdminLevel4_L");

                dt.Columns.Add("HouseNo");
                dt.Columns.Add("Telephone");
                dt.Columns.Add("PostCode");
                dt.Columns.Add("Catcode");
                dt.Columns.Add("LocalCatCode");
                dt.Columns.Add("dist");
                dt.Columns.Add("No");


                dt.Rows[0]["dist"] = "0";

                dt.Rows[0]["Name_L"] = "พื้นที่";
                dt.Rows[0]["Name_E"] = "Area";
                dt.Rows[0]["Catcode"] = "";

                dt.Rows[0]["AdminLevel1_E"] = dt_admin.Rows[0]["prov_name"];
                dt.Rows[0]["AdminLevel2_E"] = dt_admin.Rows[0]["amp_name"];
                dt.Rows[0]["AdminLevel3_E"] = dt_admin.Rows[0]["tam_name"];
                dt.Rows[0]["AdminLevel4_E"] = "";

                dt.Rows[0]["AdminLevel1_L"] = dt_admin.Rows[0]["prov_namt"];
                dt.Rows[0]["AdminLevel2_L"] = dt_admin.Rows[0]["amp_namt"];
                dt.Rows[0]["AdminLevel3_L"] = dt_admin.Rows[0]["tam_namt"];
                dt.Rows[0]["AdminLevel4_L"] = "";

                dt.Columns.Remove("objectid");
                dt.Columns.Remove("POLYTYPE");

                dt.Columns.Remove("PROV_NAME");
                dt.Columns.Remove("AMP_NAME");
                dt.Columns.Remove("TAM_NAME");

                dt.Columns.Remove("PROV_NAMT");
                dt.Columns.Remove("AMP_NAMT");
                dt.Columns.Remove("TAM_NAMT");

                dt.Columns.Remove("PROV_CODE");
                dt.Columns.Remove("AMP_CODE");
                dt.Columns.Remove("TAM_CODE");

                dt.Columns.Remove("YEAR");
                dt.Columns.Remove("MALE");
                dt.Columns.Remove("FEMALE");
                dt.Columns.Remove("TOTAL");
                dt.Columns.Remove("HOUSE");
                dt.Columns.Remove("V22_2");

                dt.Columns.Remove("ADMTAG");
                dt.Columns.Remove("VERSION");
                dt.Columns.Remove("AMP_ID");
                dt.Columns.Remove("TAM_ID");
                dt.Columns.Remove("SHAPE_Leng");
                dt.Columns.Remove("SHAPE_Area");
                
            }
            else if(mode == "solr")
            {
                dt.Columns.Remove("public_id");
                dt.Columns.Add("No");

                dt.Columns["Catcode"].ColumnName = "CatCode";
                dt.Columns["dist"].ColumnName = "Dist";
            }

            dt.TableName = "result";
            dt.AcceptChanges();
            return dt;

        }


        

        //[WebMethod(Description = "GenQuery")]
        public string GenQuery(string keyword, string AdminLevel3, string AdminLevel2, string AdminLevel1,string PostCode, string AdminLevel4, string category, string tag, string lat, string lon, string radius, string RowsPerPage, string PageNumber,string token)
        {

            List<string> array = category.Split('|').ToList();  // Split by comma
            List<string> array2 = tag.Split('|').ToList();

            try
            {
              
                _c_function fn = new _c_function();
               
                // Declare Latlon and URI
                string latlon = lat + "," + lon;
                string URI = "";
                string Nearby = ""; // For Search Nearby
                string cutsubnear = "";
                string cutsubre = "";
                // result
                DataTable temp = new DataTable();
                
                

                //Get Synonym
                int synocheck = 0;
                temp = fn.synonymSearch(keyword, latlon,RowsPerPage,PageNumber,ref synocheck,array);

                //Fix &
                keyword = keyword.Trim().Replace("&", "%26");

                //Create Query String
                fn.createSolrQueryString(ref URI, keyword, latlon, radius, array, array2, AdminLevel4, "6", AdminLevel3, AdminLevel2, AdminLevel1, PostCode);
                
                //Fix Bug Page Number
                if (PageNumber == "1") PageNumber = "0";
                else PageNumber = int.Parse(PageNumber) - 1 + "";
                
                //Finally We Got QUERY STRING
                URI = URI + "&start=" + int.Parse(RowsPerPage) * (int.Parse(PageNumber)) + "&rows=" + RowsPerPage;

                return URI;

            }
            catch (Exception e) 
            {
               return "";
            }
        
        }
        //[WebMethod(Description = "CallDT")]
        public DataTable CallDT(string URL)
        {
            try
            {
                _c_function fn = new _c_function();
                DataTable dt = fn.CallSolr_DT(URL);
                dt.TableName = "t";
                return dt;
            }
            catch
            { return null; }

        }
        public string csvF(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(@"D:\nut_search\_version4\webservice_v4[10]\Search_Service_POI\bin\test.csv", sb.ToString(),Encoding.UTF8);

            return sb.ToString();
        }
    }



    class DirAppend
    {

        

        public static void Log(string logMessage)
        {
            StringBuilder sb = new StringBuilder();
        sb.Append(DateTime.Now+"    "+logMessage+"\r\n");
        // flush every 20 seconds as you do it

        DateTime time = DateTime.Now;
        string format = "d-MM-yyyy";
        string str_uploadpath = @"E:\Search Engine\SearchLog\Thailand\";
        string fileName = time.ToString(format) + ".txt";


        File.AppendAllText(Path.Combine(str_uploadpath, fileName), sb.ToString());
        sb.Clear();
        }

       
    
    }
}
