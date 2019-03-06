using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Data;
using GT_Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Diagnostics;
using webservice_search_a_route;
using wordReplace;
using System.Threading;
namespace Search_Service_POI
{
    /// <summary>
    /// Summary description for search
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]

    public class search : System.Web.Services.WebService
    {
        static string FILE_SYNO = @System.Configuration.ConfigurationSettings.AppSettings["SynoFile"].ToString();
        static string FILE_SHORT = @System.Configuration.ConfigurationSettings.AppSettings["ShortFile"].ToString();
        static string FILE_IP = @System.Configuration.ConfigurationSettings.AppSettings["IPFile"].ToString();
        static string FILE_TOKEN = @System.Configuration.ConfigurationSettings.AppSettings["TokenFile"].ToString();

        //static string FILE_DOT = @System.Configuration.ConfigurationSettings.AppSettings["DotFile"].ToString();
        static string FILE_CAT = @System.Configuration.ConfigurationSettings.AppSettings["CatFile"].ToString();
        static string FILE_BIG = @System.Configuration.ConfigurationSettings.AppSettings["BigFile"].ToString();

        static string LOG_PATH = @System.Configuration.ConfigurationSettings.AppSettings["LogPath"].ToString();

        static int MAX = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["MaxReturnSearch"].ToString());
        static int MAX_AUTO = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["auto_MaxReturn"].ToString());
        static int PORT = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString());

        //webservice try again
        static int WAIT_TRY = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["waitTry"].ToString());
        static int TRY_AGAIN = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["tryAgain"].ToString());

        _cl_S_INIT_service I_SERVICE = new _cl_S_INIT_service(FILE_SYNO, FILE_SHORT, MAX, FILE_TOKEN, FILE_IP, FILE_CAT, FILE_BIG);

        static _cl_map map_public = new _cl_map(@System.Configuration.ConfigurationSettings.AppSettings["shp_public"].ToString(), @System.Configuration.ConfigurationSettings.AppSettings["dbf_public"].ToString(), Encoding.UTF8);
        static _cl_map map_admin = new _cl_map(@System.Configuration.ConfigurationSettings.AppSettings["shp_admin"].ToString(), @System.Configuration.ConfigurationSettings.AppSettings["dbf_admin"].ToString(), Encoding.UTF8);//Encoding.GetEncoding("windows-874"));// 
        static object obj_public = new object();
        static object obj_admin = new object();

        static _cl_map_hydro HYDRO_IDENT = new _cl_map_hydro(@System.Configuration.ConfigurationSettings.AppSettings["shp_hydro_ident"].ToString(), @System.Configuration.ConfigurationSettings.AppSettings["dbf_hydro_ident"].ToString(), Encoding.UTF8);
        static object OBJ_LOCK_HYDRO_IDENT = new object();


        ServiceReference3.identSoapClient WS_IDENT_LTRANS = new ServiceReference3.identSoapClient("identSoap");

        //static _cl_map_ltran map_ltran_km = new _cl_map_ltran(@System.Configuration.ConfigurationSettings.AppSettings["shp_public"].ToString(), @System.Configuration.ConfigurationSettings.AppSettings["dbf_public"].ToString(), Encoding.UTF8);


        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("th-TH");


        /************A long route parameter******************/
        private const int TYPE_CLOSEST = 1;
        private const int TYPE_NEARBY = 2;
        static string USER_ID_DEBUG = System.Configuration.ConfigurationSettings.AppSettings["USER_ID_DEBUG"].ToString();

        static _cl_LogSocket logS = new _cl_LogSocket(Convert.ToInt32(@System.Configuration.ConfigurationSettings.AppSettings["PORT_LOG"].ToString()), "127.0.0.1");

        [WebMethod(Description = "Search_TH_Version4.4.5", EnableSession = true)]
        public DataTable Search(string keyword, string AdminLevel3, string AdminLevel2, string AdminLevel1, string PostCode, string AdminLevel4, string HouseNo, string Telephone, string category, string LocalCatCode, string tag, string lat, string lon, string radius, string RowsPerPage, string PageNumber, string token, string userKey, string ipClient, string option)
        {
            try
            {
                Stopwatch timeProcess = new Stopwatch();
                timeProcess.Start();

                _cl_S_Parameter para = new _cl_S_Parameter(I_SERVICE.MAXRETURN);

                bool InputFormatChecker = para.CheckInputFormat(keyword, AdminLevel3, AdminLevel2, AdminLevel1, PostCode, AdminLevel4, HouseNo, Telephone, category, LocalCatCode, tag, lat, lon, radius, RowsPerPage, PageNumber, token, userKey);

                if (InputFormatChecker == false)
                {
                    Context.Response.Status = "400 Bad Request";
                    Context.Response.StatusCode = 400;
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.End();

                    return DT_BadRequest();
                }

                _prop_SearchInput input = para.Check(keyword, AdminLevel3, AdminLevel2, AdminLevel1, PostCode, AdminLevel4, HouseNo, Telephone, category.ToUpper().Trim(), LocalCatCode, tag, lat, lon, radius, RowsPerPage, PageNumber, token);

                _cl_S_authen authen = new _cl_S_authen(I_SERVICE.IP_LIST, I_SERVICE.TOKEN_LIST);
                bool permission = authen.check(GetIP(), token);

                if (permission)
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
                string message = "";
                bool cantextToCat_flag = false;
                bool convertSoundex_flag = true;

                #region check admin
                int tryAgain_checkAdmin = TRY_AGAIN;
                while (tryAgain_checkAdmin >= 0)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(input.AdminLevel1)
                            && string.IsNullOrEmpty(input.AdminLevel2)
                            && string.IsNullOrEmpty(input.AdminLevel3))
                        {
                            ServiceReference_textAnalytic.WebService_searchSoapClient textAnalytic = new ServiceReference_textAnalytic.WebService_searchSoapClient("WebService_searchSoap");
                            DataTable dt_admin1 = textAnalytic.CheckAdmin1Thai(input.keyword);
                            if (dt_admin1 != null)
                            {
                                input.keyword = dt_admin1.Rows[0]["keyword"].ToString();
                                input.AdminLevel1 = dt_admin1.Rows[0]["admin1"].ToString();
                                break;
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }
                    catch
                    {
                        tryAgain_checkAdmin--;
                        Thread.Sleep(WAIT_TRY);
                    }
                }
                #endregion
                
                //check text to cat
                if (!string.IsNullOrEmpty(input.keyword)
                    && string.IsNullOrEmpty(input.category)
                    && string.IsNullOrEmpty(input.LocalCatCode)
                    )
                {
                    _prop_SearchInput tmp_input = new _prop_SearchInput();
                    tmp_input.keyword = input.keyword;
                    tmp_input.AdminLevel4 = input.AdminLevel4;
                    tmp_input.AdminLevel3 = input.AdminLevel3;
                    tmp_input.AdminLevel2 = input.AdminLevel2;
                    tmp_input.AdminLevel1 = input.AdminLevel1;
                    tmp_input.PostCode = input.PostCode;
                    tmp_input.telephone = input.telephone;
                    tmp_input.houseNumber = input.houseNumber;
                    tmp_input.category = input.category;
                    tmp_input.LocalCatCode = input.LocalCatCode;
                    tmp_input.tag = input.tag;
                    tmp_input.lat = input.lat;
                    tmp_input.lon = input.lon;
                    tmp_input.radius = input.radius;
                    tmp_input.RowsPerPage = input.RowsPerPage;
                    tmp_input.PageNumber = input.PageNumber;
                    tmp_input.token = input.token;
                    tmp_input.maxReturn = input.maxReturn;
                    tmp_input.syno = "";
                    tmp_input.syno_wb = "";

                    _cl_wordRemove wr = new _cl_wordRemove();
                    tmp_input.keyword = wr.RemoveMoreSpace(wr.DotAction(tmp_input.keyword)).Trim();
                    _cl_S_TextToCat tToCat = new _cl_S_TextToCat(I_SERVICE.FILE_CAT);
                    string tmp_localcat = "";

                    if (tToCat.CheckWord(tmp_input.keyword, ref tmp_localcat))
                    {
                        tmp_input.keyword = "";
                        tmp_input.LocalCatCode = tmp_localcat;

                        input = tmp_input;
                        cantextToCat_flag = true;
                    }


                }
                bool big_flag = false;
                //ถ้าไม่เข้าเงื่อนไข textTocat ต้องทำ
                if (cantextToCat_flag == false)
                {
                    _cl_wordRemove wr = new _cl_wordRemove();

                    string tmp_input = input.keyword.Trim();
                    tmp_input = wr.ReplaceThaiNumber(wr.RemoveSPchar(tmp_input));


                    _cl_S_big bigW = new _cl_S_big(I_SERVICE.FILE_BIG);
                    if (bigW.check(tmp_input))
                    {
                        convertSoundex_flag = false;
                        big_flag = true;
                    }

                    _cl_S_ShortWord shortW = new _cl_S_ShortWord(I_SERVICE.FILE_SHORT);
                    string haveShortW = shortW.CheckWord(tmp_input);
                    _cl_S_Syno syno = new _cl_S_Syno(I_SERVICE.FILE_SYNO);

                    List<string> tmp_shortANDsyno = new List<string>();
                    tmp_shortANDsyno.Add(tmp_input);

                    if (!string.IsNullOrEmpty(haveShortW))
                    {
                        tmp_shortANDsyno.Add(haveShortW);
                        convertSoundex_flag = false;
                        big_flag = false;
                    }
                    input.syno = syno.Dup(tmp_shortANDsyno);
                    input.syno = wr.RemoveMoreSpace(input.syno).Trim();

                    if (!CheckThis_Thai(input.syno))
                    {
                        convertSoundex_flag = false;
                    }

                }

                // จัด Format ที่ส่งเข้า Engine
                _cl_S_EngineFormat EngineFormat = new _cl_S_EngineFormat();

                if (convertSoundex_flag)
                {
                    _cl_S_soundex sound = new _cl_S_soundex();

                    string tmp_1 = "";
                    string tmp_2 = "";
                    string tmp_3 = "";

                    bool canConvert = sound.Convert(input.syno, out tmp_1, out tmp_2, out tmp_3);

                    input.thai_soundex_delimeter = tmp_1;
                    input.thai_soundex = tmp_2;
                    input.syno_wb = tmp_3;

                    message = EngineFormat.GetSearch_Soundex_Format(input);
                }
                else if (big_flag)
                {
                    input.syno_wb = "";
                    message = EngineFormat.GetSearchFormat_big(input);
                }
                else
                {
                    _cl_S_WordBreak wbService = new _cl_S_WordBreak();
                    _cl_wordRemove wr = new _cl_wordRemove();
                    string[] tmp_wb = input.syno.Split('|');
                    input.syno_wb = "";

                    for (int i = 0; i < tmp_wb.Length; i++)
                        input.syno_wb += wr.RemoveMoreSpace(wbService.wb_action(tmp_wb[i])).Trim() + "|";

                    input.syno_wb = input.syno_wb.Substring(0, input.syno_wb.Length - 1);

                    message = EngineFormat.GetSearchFormat(input);
                }
                //ส่ง เข้า Engine
                _cl_S_CallEngine call = new _cl_S_CallEngine(Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString())
                                                            , "127.0.0.1"
                                                            , Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["timeout"].ToString()));
                string res = call.SendToEngine(message);
                if (string.IsNullOrEmpty(res) || res == "\r\n")
                {
                    DataTable connect = new DataTable();
                    connect.Columns.Add("result");
                    connect.Rows.Add("NotFound");
                    connect.TableName = "result";
                    return connect;
                }
                else
                {

                    DataTable dt = new DataTable();
                    _cl_S_StringToDT toDT = new _cl_S_StringToDT();
                    dt = toDT.ToDataTable(res, Convert.ToInt32(input.RowsPerPage), Convert.ToInt32(input.PageNumber));

                    string totalS = dt.TableName;

                    _cl_S_Format_Result result_dt = new _cl_S_Format_Result();
                    dt = result_dt.SearchResultFormat(dt);
                    dt.TableName = "result";
                    dt.AcceptChanges();

                    try
                    {
                        if (System.Configuration.ConfigurationSettings.AppSettings["KeepLog"].ToString().ToUpper().Trim() == "TRUE")
                        {

                            string Optional = "";
                            if (System.Configuration.ConfigurationSettings.AppSettings["KeepLog_LatLonR"].ToString().ToUpper().Trim() == "TRUE")
                                Optional += "\t" + lat + "\t" + lon + "\t" + radius;
                            if (System.Configuration.ConfigurationSettings.AppSettings["KeepLog_Admin"].ToString().ToUpper().Trim() == "TRUE")
                                Optional += "\t" + AdminLevel1 + "\t" + AdminLevel2 + "\t" + AdminLevel3 + "\t" + AdminLevel4 + "\t" + PostCode + "\t" + HouseNo + "\t" + Telephone;
                            if (System.Configuration.ConfigurationSettings.AppSettings["KeepLog_Cat"].ToString().ToUpper().Trim() == "TRUE")
                                Optional += "\t" + LocalCatCode + "\t" + category;
                            if (System.Configuration.ConfigurationSettings.AppSettings["KeepLog_Num"].ToString().ToUpper().Trim() == "TRUE")
                                Optional += "\t" + PageNumber + "\t" + RowsPerPage;
                            WriteLog(DateTime.Now.ToString()
                                + "\t" + GetIP()
                                + "\t" + userKey
                                + "\t" + ipClient
                                + "\t" + keyword
                                + "\t" + input.syno
                                + "\t" + input.syno_wb
                                + "\t" + totalS
                                + "\t" + timeProcess.ElapsedMilliseconds.ToString()
                                + Optional
                                );
                        }
                    }
                    catch
                    { }

                    return dt;

                }
            }
            catch
            {
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("NotFound");
                connect.TableName = "result";
                return connect;
            }

        }

        [WebMethod(Description = "Auto Complete")]
        public DataTable AutoComplete(string keyword, string numreturn, string token, string userKey, string ipClient)
        {
            bool isThai = false;
            try
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["KeepLogAuto"].ToString().ToUpper().Trim() == "TRUE")
                {
                    string tmp_log = "AUTOCOMPLETE_SEARCH_TH" + "\t"
                                + DateTime.Now.ToString() + "\t"
                                + GetIP() + "\t"
                                + userKey + "\t"
                                + ipClient + "\t"
                                + keyword;
                    logS.SendToEngine(tmp_log);
                }
            }
            catch
            { }
            _cl_S_authen authen = new _cl_S_authen(I_SERVICE.IP_LIST, I_SERVICE.TOKEN_LIST);
            bool permission = authen.check(GetIP(), token);
            if (permission)
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

            //Bad request
            //if (string.IsNullOrEmpty(lat) && string.IsNullOrEmpty(lon))
            //{
            string lat = @System.Configuration.ConfigurationSettings.AppSettings["auto_lat"].ToString();
            string lon = @System.Configuration.ConfigurationSettings.AppSettings["auto_lon"].ToString();
            //}
            try
            {
                _cl_S_Parameter checkFormat = new _cl_S_Parameter(20);
                if (string.IsNullOrEmpty(keyword) || string.IsNullOrEmpty(userKey))//|| !checkFormat.CheckLatLon(lat, lon))
                {
                    Context.Response.Status = "400 Bad Request";
                    Context.Response.StatusCode = 400;
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.End();

                    return DT_BadRequest();
                }
                if (!string.IsNullOrEmpty(numreturn))
                {
                    int tmp_int = Convert.ToInt32(numreturn);
                }
            }
            catch
            {
                Context.Response.Status = "400 Bad Request";
                Context.Response.StatusCode = 400;
                Context.ApplicationInstance.CompleteRequest();
                Context.Response.End();

                return DT_BadRequest();
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
                isThai = true;
            }

            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("name", typeof(string));

                GT_Socket gt_socket = new GT_Socket();

                if (string.IsNullOrEmpty(numreturn))
                    numreturn = "20";

                int NumReTurn = Convert.ToInt32(numreturn);
                if (NumReTurn > MAX_AUTO)
                    NumReTurn = MAX_AUTO;
                else if (NumReTurn <= 0)
                {
                    DataTable connect = new DataTable();
                    connect.Columns.Add("result");
                    connect.Rows.Add("NotFound");
                    connect.TableName = "NotFound";
                    return connect;
                }

                gt_socket.Server = "127.0.0.1";
                gt_socket.Port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString());
                gt_socket.ProviderID = 112;

                _cl_wordRemove wr = new _cl_wordRemove();
                _cl_S_TextToCat tToCat = new _cl_S_TextToCat(I_SERVICE.FILE_CAT);

                #region autocomplete
                List<string> autocomplete_list = new List<string>();

                string auto_limit_result = "";

                if (CheckAutoCompleteMaxLimit(dt.Rows.Count))
                {

                    if (keyword.Trim().Length <= 10)
                    {
                        gt_socket.Connect();
                        message += keyword.Trim() + ";";
                        message += NumReTurn.ToString() + ";";//@System.Configuration.ConfigurationSettings.AppSettings["auto_limit_result1"].ToString() + ";";
                        message += lat.Trim() + ";";
                        message += lon.Trim() + ";";
                        message += "prefix";

                        bool stat = gt_socket.SendData(message + " \n", false);
                        string recieve = gt_socket.recieve(10000 * 60);

                        if (!string.IsNullOrEmpty(recieve))
                        {
                            string[] line = recieve.Split('|');
                            for (int i = 0; i < line.Length; i++)
                            {
                                if (line[i].ToUpper().Trim() == "NULL" || string.IsNullOrEmpty(line[i].Trim()))
                                    break;
                                autocomplete_list.Add(line[i].ToString().Trim());// + "[1]");
                            }
                        }
                        gt_socket.Disconnect();
                        auto_limit_result = "auto_limit_result1";
                    }

                    //if (keyword.Trim().Length > 10 || autocomplete_list.Count == 0)
                    if (keyword.Trim().Length > 10 || autocomplete_list.Count < NumReTurn)
                    {
                        gt_socket.Connect();
                        message += keyword.Trim() + ";";
                        message += NumReTurn.ToString() + ";";//@System.Configuration.ConfigurationSettings.AppSettings["auto_limit_result2"].ToString() + ";";
                        message += lat.Trim() + ";";
                        message += lon.Trim() + ";";
                        message += "normal";

                        bool stat = gt_socket.SendData(message + " \n", false);
                        string recieve = gt_socket.recieve(10000 * 60);

                        if (!string.IsNullOrEmpty(recieve))
                        {
                            string[] line = recieve.Split('|');
                            for (int i = 0; i < line.Length; i++)
                            {
                                if (line[i].ToUpper().Trim() == "NULL" || string.IsNullOrEmpty(line[i].Trim()))
                                    break;
                                if(!autocomplete_list.Contains(line[i].ToString().Trim()))
                                    autocomplete_list.Add(line[i].ToString().Trim());// + "[2]");
                                //dt.Rows.Add(line[i].ToString().Trim());
                            }
                        }
                        gt_socket.Disconnect();
                        auto_limit_result = "auto_limit_result2";
                    }

                    if (autocomplete_list.Count != 0)
                    {
                        int j = 0;
                        for (int i = 0; i < autocomplete_list.Count; i++)
                        {
                            if (!CheckAutoCompleteMaxLimit(dt.Rows.Count))
                                break;
                            dt.Rows.Add(autocomplete_list[i]);
                            j++;
                            if (j == NumReTurn)
                            //if (j.ToString() == @System.Configuration.ConfigurationSettings.AppSettings[auto_limit_result].ToString())
                            {
                                break;
                            }
                        }
                    }
                }
                #endregion

                if (dt.Rows.Count <= 0)
                {
                    DataTable connect = new DataTable();
                    connect.Columns.Add("result");
                    connect.Rows.Add("NotFound");
                    connect.TableName = "NotFound";
                    return connect;
                }

                //return recieve;

                dt.TableName = "res";
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

        [WebMethod(Description = "Iden")]
        public DataTable Identify(string lat, string lon, string token, string userKey, string ipClient)
        {
            try
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["KeepLogIdent"].ToString().ToUpper().Trim() == "TRUE")
                {
                    string tmp_log = "Identify_SEARCH_TH" + "\t"
                                + DateTime.Now.ToString() + "\t"
                                + GetIP() + "\t"
                                + userKey + "\t"
                                + ipClient + "\t"
                                + lat + "\t" + lon;
                    logS.SendToEngine(tmp_log);
                }
            }
            catch
            { }

            _cl_S_authen authen = new _cl_S_authen(I_SERVICE.IP_LIST, I_SERVICE.TOKEN_LIST);
            bool permission = authen.check(GetIP(), token);
            if (permission)
            { }
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
                //Bad request
                try
                {
                    if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lon) || string.IsNullOrEmpty(userKey))
                    {
                        Context.Response.Status = "400 Bad Request";
                        Context.Response.StatusCode = 400;
                        Context.ApplicationInstance.CompleteRequest();
                        Context.Response.End();

                        return DT_BadRequest();
                    }
                    double tmp_lat = Convert.ToDouble(lat);
                    double tmp_lon = Convert.ToDouble(lon);

                }
                catch
                {
                    Context.Response.Status = "400 Bad Request";
                    Context.Response.StatusCode = 400;
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.End();

                    return DT_BadRequest();
                }

                GT_Socket gt_socket = new GT_Socket();

                gt_socket.Server = "127.0.0.1";
                gt_socket.Port = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString());
                gt_socket.ProviderID = 112;

                gt_socket.Connect();

                message += lat.Trim() + ";";
                message += lon.Trim() + ";";
                message += System.Configuration.ConfigurationSettings.AppSettings["MaxIdenBuffer"].ToString().Trim() + ";";

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

                dt.TableName = "res";
                if (dt != null)
                {
                    if (dt.Rows.Count == 1)
                    {
                        if (Convert.ToDouble(dt.Rows[0]["dist"].ToString()) * 1000 <= Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["IdenBuffer_NearPoint"].ToString()))
                        {
                            //return IdenFormat(dt, "solr", dt_admin);
                            return IdenFormat(dt, "solr", null);
                        }
                    }
                }


                DataTable dt_hydro = new DataTable();
                lock (OBJ_LOCK_HYDRO_IDENT)
                {
                    dt_hydro = HYDRO_IDENT.Iden_Hydro(Convert.ToDouble(lat), Convert.ToDouble(lon));
                }

                if (dt_hydro != null)
                {

                    DataTable dt_ltran = new DataTable();
                    dt_ltran = WS_IDENT_LTRANS.Search_SnapLtrans(lat, lon, "0.1");
                    if (dt_ltran != null)
                    {
                        if (dt_ltran.Rows.Count == 1)
                        {
                            int buf_ = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["IdenBuffer_HydroLtrans"].ToString());
                            double L_dist = Convert.ToDouble(dt_ltran.Rows[0]["DIS"].ToString());

                            if (buf_ >= L_dist)
                            {
                                DataTable dt_admin_l = new DataTable();
                                lock (obj_admin)
                                {
                                    dt_admin_l = map_admin.polygon(Convert.ToDouble(lat), Convert.ToDouble(lon));
                                }
                                dt_admin_l.TableName = "result";

                                return IdenFormat(dt_ltran, "LTRANS", dt_admin_l);
                            }

                        }
                    }

                    return dt_hydro;
                }


                DataTable dt_public = new DataTable();
                DataTable dt_admin = new DataTable();

                lock (obj_admin)
                {
                    dt_admin = map_admin.polygon(Convert.ToDouble(lat), Convert.ToDouble(lon));
                }
                if (dt_admin != null)
                {

                    lock (obj_public)
                    {
                        dt_public = map_public.polygon(Convert.ToDouble(lat), Convert.ToDouble(lon));
                    }


                    dt_admin.TableName = "result";

                    if (dt != null)
                    {
                        if (dt.Rows.Count == 1)
                        {
                            if (Convert.ToDouble(dt.Rows[0]["dist"].ToString()) * 1000 <= Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["IdenBuffer"].ToString()))
                            {
                                if (dt_public != null)
                                {
                                    if (dt_public.Rows[0]["pid"].ToString().Trim() == dt.Rows[0]["public_id"].ToString().Trim())
                                        return IdenFormat(dt, "solr", null);
                                    else
                                        return IdenFormat(dt_public, "public", dt_admin);
                                }
                                else
                                    return IdenFormat(dt, "solr", null);
                            }
                        }
                    }


                    if (dt_public != null)
                        return IdenFormat(dt_public, "public", dt_admin);

                    DataTable dt_ltrans;
                    try
                    {
                        dt_ltrans = WS_IDENT_LTRANS.Search_SnapLtrans(lat, lon, "0.1");
                    }
                    catch { dt_ltrans = null; }

                    DataTable dt_KM;
                    try
                    {
                        dt_KM = WS_IDENT_LTRANS.Search_SnapLtrans_KM(lat, lon, "0.005");
                    }
                    catch
                    {
                        dt_KM = null;
                    }

                    if (dt_KM != null)
                    {
                        if (Convert.ToDouble(dt_KM.Rows[0]["DIS"]) <= Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["IdenBuffer_Ltrans"].ToString()))
                        {
                            return IdenFormat(dt_KM, "LTRANS_KM", dt_admin);
                        }
                    }

                    if (dt_ltrans != null)
                    {
                        if (Convert.ToDouble(dt_ltrans.Rows[0]["DIS"]) <= Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["IdenBuffer_Ltrans"].ToString()))
                        {
                            return IdenFormat(dt_ltrans, "LTRANS", dt_admin);
                        }
                    }

                    return IdenFormat(dt_admin, "admin", dt_admin);
                }
                else
                {
                    if (dt != null)
                    {
                        if (dt.Rows.Count == 1)
                        {
                            if (Convert.ToDouble(dt.Rows[0]["dist"].ToString()) * 1000 <= Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["IdenBuffer_NotInAdmin"].ToString()))
                            {
                                return IdenFormat(dt, "solr2", null);
                            }

                        }
                    }
                }

                DataTable notFound = new DataTable();
                notFound.Columns.Add("result");
                notFound.Rows.Add("NotFound");
                notFound.TableName = "NotFound";
                return notFound;


            }
            catch (Exception e)
            {
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("NotFound");
                connect.TableName = "NotFound";
                return connect;
            }
        }

        [WebMethod(Description = "Search Nearby")]
        public string Search_Nearby(string lat, string lon, string distance, string numreturn, string token)
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
                        if (result.Rows.Count == 0)
                        {
                            return "NotFound";
                        }
                    }
                    catch (Exception e) { }

                    for (int i = 0; i < result.Rows.Count; i++)
                    {

                        sb.Append(result.Rows[i]["Name_Local"] + "!" + result.Rows[i]["Name_English"] + "!" + (1000 * double.Parse(result.Rows[i]["dist"].ToString())).ToString("F2"));
                        sb.Append("|");
                    }

                    return sb.ToString();
                }
                else
                {
                    return "you don't have permission";
                }
            }
            catch (Exception)
            {
                return "Error in code Please contact developer.";
            }
        }

        [WebMethod(Description = "SumPoiByCat")]
        public DataTable SumPoiByCat(string Catcode, string lat, string lon, string radius, string token, string userKey, string ipClient)
        {
            try
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["KeepLogSumPoiByCat"].ToString().ToUpper().Trim() == "TRUE")
                {
                    string tmp_log = "SumPoiByCat_SEARCH_TH" + "\t"
                                + DateTime.Now.ToString() + "\t"
                                + GetIP() + "\t"
                                + userKey + "\t"
                                + ipClient + "\t"
                                + Catcode + "\t"
                                + lat + "\t"
                                + lon + "\t"
                                + radius;
                    logS.SendToEngine(tmp_log);
                }
            }
            catch
            { }

            _cl_S_authen authen = new _cl_S_authen(I_SERVICE.IP_LIST, I_SERVICE.TOKEN_LIST);
            bool permission = authen.check(GetIP(), token);
            if (permission)
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
                //Bad request
                try
                {
                    if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lon) || string.IsNullOrEmpty(Catcode) || string.IsNullOrEmpty(userKey))
                    {
                        Context.Response.Status = "400 Bad Request";
                        Context.Response.StatusCode = 400;
                        Context.ApplicationInstance.CompleteRequest();
                        Context.Response.End();

                        return DT_BadRequest();
                    }
                    double tmp_lat = Convert.ToDouble(lat);
                    double tmp_lon = Convert.ToDouble(lon);
                    if (string.IsNullOrEmpty(radius))
                    {
                        radius = (Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["SumBuffer"].ToString()) / 1000).ToString();
                    }
                    else
                    {
                        double tmp_radius = Convert.ToDouble(radius);
                    }

                }
                catch
                {
                    Context.Response.Status = "400 Bad Request";
                    Context.Response.StatusCode = 400;
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.End();

                    return DT_BadRequest();
                }


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
            {/*
                DataTable result = new DataTable();
                result.TableName = "Error";
                result.Columns.Add("Problem");
                result.Rows.Add(e);
                return result;
                */
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("NotFound");
                connect.TableName = "result";
                return connect;
            }
        }

        [WebMethod(Description = "SumPOIByLocalCat")]
        public DataTable SumPOIByLocalCat(string LocalCatCode, string lat, string lon, string radius, string token, string userKey, string ipClient)
        {
            try
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["KeepLogSumPOIByLocalCat"].ToString().ToUpper().Trim() == "TRUE")
                {
                    string tmp_log = "SumPoiByCat_SEARCH_TH" + "\t"
                                + DateTime.Now.ToString() + "\t"
                                + GetIP() + "\t"
                                + userKey + "\t"
                                + ipClient + "\t"
                                + LocalCatCode + "\t"
                                + lat + "\t"
                                + lon + "\t"
                                + radius;
                    logS.SendToEngine(tmp_log);
                }
            }
            catch
            { }

            _cl_S_authen authen = new _cl_S_authen(I_SERVICE.IP_LIST, I_SERVICE.TOKEN_LIST);
            bool permission = authen.check(GetIP(), token);
            if (permission)
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
                //Bad request
                try
                {
                    if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lon) || string.IsNullOrEmpty(LocalCatCode) || string.IsNullOrEmpty(userKey))
                    {
                        Context.Response.Status = "400 Bad Request";
                        Context.Response.StatusCode = 400;
                        Context.ApplicationInstance.CompleteRequest();
                        Context.Response.End();

                        return DT_BadRequest();
                    }
                    double tmp_lat = Convert.ToDouble(lat);
                    double tmp_lon = Convert.ToDouble(lon);
                    if (string.IsNullOrEmpty(radius))
                    {
                        radius = (Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["SumBuffer"].ToString()) / 1000).ToString();
                    }
                    else
                    {
                        double tmp_radius = Convert.ToDouble(radius);
                    }

                }
                catch
                {
                    Context.Response.Status = "400 Bad Request";
                    Context.Response.StatusCode = 400;
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.End();

                    return DT_BadRequest();
                }

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
            {/*
                DataTable result = new DataTable();
                result.TableName = "Error";
                result.Columns.Add("Problem");
                result.Rows.Add(e);
                return result;
              */
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("NotFound");
                connect.TableName = "result";
                return connect;
            }
        }
        //[WebMethod(Description = "IP")]
        public string GetIP()
        {
            String ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }

        [WebMethod(Description = "TotalSearchResult")]
        public DataTable TotalSearchResult(string keyword, string AdminLevel3, string AdminLevel2, string AdminLevel1, string PostCode, string AdminLevel4, string HouseNo, string Telephone, string category, string LocalCatCode, string tag, string lat, string lon, string radius, string RowsPerPage, string PageNumber, string token, string userKey, string ipClient, string option)
        {

            try
            {
                _cl_S_Parameter para = new _cl_S_Parameter(I_SERVICE.MAXRETURN);
                bool InputFormatChecker = para.CheckInputFormat(keyword, AdminLevel3, AdminLevel2, AdminLevel1, PostCode, AdminLevel4, HouseNo, Telephone, category, LocalCatCode, tag, lat, lon, radius, RowsPerPage, PageNumber, token, userKey);

                if (InputFormatChecker == false)
                {
                    Context.Response.Status = "400 Bad Request";
                    Context.Response.StatusCode = 400;
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.End();

                    return DT_BadRequest();
                }

                _prop_SearchInput input = para.Check(keyword, AdminLevel3, AdminLevel2, AdminLevel1, PostCode, AdminLevel4, HouseNo, Telephone, category, LocalCatCode, tag, lat, lon, radius, RowsPerPage, PageNumber, token);

                _cl_S_authen authen = new _cl_S_authen(I_SERVICE.IP_LIST, I_SERVICE.TOKEN_LIST);
                bool permission = authen.check(GetIP(), token);
                if (permission)
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

                bool cantextToCat_flag = false;

                if (!string.IsNullOrEmpty(input.keyword)
                    && string.IsNullOrEmpty(input.category)
                    && string.IsNullOrEmpty(input.LocalCatCode)
                    )
                {
                    _prop_SearchInput tmp_input = new _prop_SearchInput();
                    tmp_input.keyword = input.keyword;
                    tmp_input.AdminLevel4 = input.AdminLevel4;
                    tmp_input.AdminLevel3 = input.AdminLevel3;
                    tmp_input.AdminLevel2 = input.AdminLevel2;
                    tmp_input.AdminLevel1 = input.AdminLevel1;
                    tmp_input.PostCode = input.PostCode;
                    tmp_input.telephone = input.telephone;
                    tmp_input.houseNumber = input.houseNumber;
                    tmp_input.category = input.category;
                    tmp_input.LocalCatCode = input.LocalCatCode;
                    tmp_input.tag = input.tag;
                    tmp_input.lat = input.lat;
                    tmp_input.lon = input.lon;
                    tmp_input.radius = input.radius;
                    tmp_input.RowsPerPage = input.RowsPerPage;
                    tmp_input.PageNumber = input.PageNumber;
                    tmp_input.token = input.token;
                    tmp_input.maxReturn = input.maxReturn;
                    tmp_input.syno = "";
                    tmp_input.syno_wb = "";

                    _cl_wordRemove wr = new _cl_wordRemove();
                    tmp_input.keyword = wr.RemoveMoreSpace(wr.DotAction(tmp_input.keyword)).Trim();
                    _cl_S_TextToCat tToCat = new _cl_S_TextToCat(I_SERVICE.FILE_CAT);
                    string tmp_localcat = "";
                    if (tToCat.CheckWord(tmp_input.keyword, ref tmp_localcat))
                    {
                        tmp_input.keyword = "";
                        tmp_input.LocalCatCode = tmp_localcat;

                        input = tmp_input;
                        cantextToCat_flag = true;
                    }


                }

                if (cantextToCat_flag == false)
                {
                    _cl_wordRemove wr = new _cl_wordRemove();

                    string tmp_input = input.keyword.Trim();
                    tmp_input = wr.ReplaceThaiNumber(wr.RemoveSPchar(tmp_input));

                    _cl_S_ShortWord shortW = new _cl_S_ShortWord(I_SERVICE.FILE_SHORT);
                    string haveShortW = shortW.CheckWord(tmp_input);
                    _cl_S_Syno syno = new _cl_S_Syno(I_SERVICE.FILE_SYNO);

                    List<string> tmp_shortANDsyno = new List<string>();
                    tmp_shortANDsyno.Add(tmp_input);

                    if (!string.IsNullOrEmpty(haveShortW))
                        tmp_shortANDsyno.Add(haveShortW);

                    input.syno = syno.Dup(tmp_shortANDsyno);
                    input.syno = wr.RemoveMoreSpace(input.syno).Trim();

                }

                bool soundex_flag = false;
                //check ว่า ถูกทำ synnonyme หรือ คำย่อหรือไม่
                if (!string.IsNullOrEmpty(input.syno))
                {
                    if (!input.syno.Contains("|"))
                    { soundex_flag = true; }
                }
                else
                {
                    soundex_flag = true;
                }

                if (!soundex_flag)
                {
                    _cl_wordRemove wr = new _cl_wordRemove();
                    _cl_S_WordBreak wbService = new _cl_S_WordBreak();

                    string[] tmp_wb = input.syno.Split('|');
                    input.syno_wb = "";
                    for (int i = 0; i < tmp_wb.Length; i++)
                        input.syno_wb += wr.RemoveMoreSpace(wbService.wb_action(tmp_wb[i])).Trim() + "|";

                    input.syno_wb = input.syno_wb.Substring(0, input.syno_wb.Length - 1);
                }
                else
                {
                    //set thai Soundex mode
                    input.mode = 1;

                    _cl_S_soundex sound = new _cl_S_soundex();

                    string tmp_1 = "";
                    string tmp_2 = "";
                    string tmp_3 = "";

                    bool canConvert = sound.Convert(input.syno, out tmp_1, out tmp_2, out tmp_3);

                    input.thai_soundex_delimeter = tmp_1;
                    input.thai_soundex = tmp_2;
                    input.syno_wb = tmp_3;
                }

                _cl_S_EngineFormat EngineFormat = new _cl_S_EngineFormat();
                string message = "";
                if (input.mode == 1)
                    message = EngineFormat.GetTotalSearchFormat_Sound(input);
                else
                    message = EngineFormat.GetTotalSearchFormat(input);

                //string message = EngineFormat.GetTotalSearchFormat(input);

                _cl_S_CallEngine call = new _cl_S_CallEngine(Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString())
                                                            , "127.0.0.1"
                                                            , Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["timeout"].ToString()));
                string res = call.SendToEngine(message);

                res = res.Trim();
                DataTable dt = new DataTable();
                dt.Columns.Add("total");
                dt.Rows.Add(res);


                dt.TableName = "result";

                try
                {
                    if (System.Configuration.ConfigurationSettings.AppSettings["KeepLogTotalSearchResult"].ToString().ToUpper().Trim() == "TRUE")
                    {

                        string Optional = "";
                        if (System.Configuration.ConfigurationSettings.AppSettings["KeepLog_LatLonR"].ToString().ToUpper().Trim() == "TRUE")
                            Optional += "\t" + lat + "\t" + lon + "\t" + radius;
                        if (System.Configuration.ConfigurationSettings.AppSettings["KeepLog_Admin"].ToString().ToUpper().Trim() == "TRUE")
                            Optional += "\t" + AdminLevel1 + "\t" + AdminLevel2 + "\t" + AdminLevel3 + "\t" + AdminLevel4 + "\t" + PostCode;
                        if (System.Configuration.ConfigurationSettings.AppSettings["KeepLog_Cat"].ToString().ToUpper().Trim() == "TRUE")
                            Optional += "\t" + LocalCatCode + "\t" + category;
                        if (System.Configuration.ConfigurationSettings.AppSettings["KeepLog_Num"].ToString().ToUpper().Trim() == "TRUE")
                            Optional += "\t" + PageNumber + "\t" + RowsPerPage;
                        WriteLogTotal(DateTime.Now.ToString()
                            + "\t" + GetIP()
                            + "\t" + userKey
                            + "\t" + ipClient
                            + "\t" + keyword
                            + "\t" + input.syno
                            + "\t" + input.syno_wb
                            + "\t" + res
                            + Optional
                            );
                    }
                }
                catch
                { }


                return dt;
            }
            catch
            {
                Context.Response.Status = "400 Bad Request";
                Context.Response.StatusCode = 400;
                Context.ApplicationInstance.CompleteRequest();
                Context.Response.End();

                return DT_BadRequest();
            }

        }

        [WebMethod(Description = "searchAlongRoute", EnableSession = true)]
        public DataTable searchAlongRoute(string keyword, string catCode, string localCatCode, string turnNode, int type, double buffer, string userKey, string ipClient)
        {
            try
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["KeepLogAlongRoute"].ToString().ToUpper().Trim() == "TRUE")
                {
                    string tmp_log = //"AlongRoute_SEARCH_TH" + "\t"
                                DateTime.Now.ToString() + "\t"
                                + GetIP() + "\t"
                                + userKey + "\t"
                                + ipClient + "\t"
                                + keyword + "\t"
                                + catCode + "\t"
                                + localCatCode + "\t"
                                + turnNode + "\t"
                                + type + "\t"
                                + buffer;
                    //logS.SendToEngine(tmp_log);
                    WriteLogAroute(tmp_log);
                }
            }
            catch
            { }

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                _cl_S_authen authen = new _cl_S_authen(I_SERVICE.IP_LIST, I_SERVICE.TOKEN_LIST);
                bool permission = authen.check(GetIP(), "");

                if (permission || userKey == USER_ID_DEBUG)
                {

                }
                else
                {

                    Context.Response.Status = "403 Forbidden";
                    Context.Response.StatusCode = 403;
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.End();

                    return DT_permission();
                }

                string ignore_closest = System.Configuration.ConfigurationSettings.AppSettings["_AROUTE_IGNORE_CLOSESET"].ToString().ToUpper().Trim();

                if (type != TYPE_CLOSEST && type != TYPE_NEARBY)
                {
                    Context.Response.Status = "400 Bad Request";
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.StatusCode = 400;
                    Context.Response.End();

                    return DT_BadRequest();
                }

                if (string.IsNullOrEmpty(userKey))
                {
                    Context.Response.Status = "400 Bad Request";
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.StatusCode = 400;
                    Context.Response.End();

                    return DT_BadRequest();
                }

                if (type == TYPE_CLOSEST && ignore_closest == "YES")
                {
                    type = TYPE_NEARBY;
                }

                /*********************search keyword**********************/
                string tmp_keyword = "";
                string tmp_syno = "";
                string tmp_syno_wb = "";
                string tmp_localCatCode = "";
                bool cantextToCat_flag = false;
                bool convertSoundex_flag = true;

                string tmp_thai_soundex_delimeter = "";
                string tmp_thai_soundex = "";
                int input_mode = 0;

                //soundex param


                if (!string.IsNullOrEmpty(keyword)
                   && string.IsNullOrEmpty(catCode)
                   && string.IsNullOrEmpty(localCatCode)
                   )
                {
                    tmp_keyword = keyword;

                    _cl_wordRemove wr = new _cl_wordRemove();
                    tmp_keyword = wr.RemoveMoreSpace(wr.DotAction(tmp_keyword)).Trim();
                    _cl_S_TextToCat tToCat = new _cl_S_TextToCat(I_SERVICE.FILE_CAT);
                    string tmp_localcat = "";
                    if (tToCat.CheckWord(tmp_keyword, ref tmp_localcat))
                    {
                        tmp_keyword = "";
                        tmp_localCatCode = tmp_localcat;
                        cantextToCat_flag = true;
                    }


                }


                if (cantextToCat_flag == false)
                {
                    tmp_keyword = keyword.Trim();
                    _cl_wordRemove wr = new _cl_wordRemove();

                    tmp_keyword = wr.ReplaceThaiNumber(wr.RemoveSPchar(tmp_keyword));

                    _cl_S_big bigW = new _cl_S_big(I_SERVICE.FILE_BIG);
                    if (bigW.check(tmp_keyword))
                    {
                        convertSoundex_flag = false;
                    }

                    _cl_S_ShortWord shortW = new _cl_S_ShortWord(I_SERVICE.FILE_SHORT);
                    string haveShortW = shortW.CheckWord(tmp_keyword);

                    _cl_S_Syno syno = new _cl_S_Syno(I_SERVICE.FILE_SYNO);
                    List<string> tmp_shortANDsyno = new List<string>();

                    tmp_shortANDsyno.Add(tmp_keyword);

                    if (!string.IsNullOrEmpty(haveShortW))
                    {
                        tmp_shortANDsyno.Add(haveShortW);
                        convertSoundex_flag = false;
                    }
                    tmp_syno = syno.Dup(tmp_shortANDsyno);
                    tmp_syno = wr.RemoveMoreSpace(tmp_syno).Trim();

                    if (!CheckThis_Thai(tmp_syno))
                        convertSoundex_flag = false;

                }

                _cl_S_soundex sound = new _cl_S_soundex();

                string tmp_1 = "";
                string tmp_2 = "";
                string tmp_3 = "";

                bool canConvert = sound.Convert(tmp_syno, out tmp_1, out tmp_2, out tmp_3);

                tmp_thai_soundex_delimeter = tmp_1;
                tmp_thai_soundex = tmp_2;
                tmp_syno_wb = tmp_3;


                if (type == TYPE_NEARBY)
                {
                    _prop_Nearby input = new _prop_Nearby();
                    input.keyword = tmp_keyword;
                    input.syno = tmp_syno;
                    input.syno_wb = tmp_syno_wb;
                    input.thai_soundex = tmp_thai_soundex;
                    input.thai_soundex_delimeter = tmp_thai_soundex_delimeter;

                    input.buffer = buffer;
                    _cl_Create_Polygon poly = new _cl_Create_Polygon();
                    input = poly.Format(input, turnNode);

                    if (input == null)
                    {
                        Context.Response.Status = "400 Bad Request";
                        Context.Response.StatusCode = 400;
                        Context.ApplicationInstance.CompleteRequest();
                        Context.Response.End();

                        return DT_BadRequest();
                    }
                    else if (input.lat.Count < 2)
                    {
                        Context.Response.Status = "400 Bad Request";
                        Context.Response.StatusCode = 400;
                        Context.ApplicationInstance.CompleteRequest();
                        Context.Response.End();

                        return DT_BadRequest();
                    }

                    input = poly.CreateAllPolygon(input);

                    input.category = catCode;

                    if (cantextToCat_flag)
                    {
                        input.LocalCatCode = tmp_localCatCode;
                    }
                    else
                    {
                        input.LocalCatCode = localCatCode;
                    }


                    _cl_S_CallEngine call = new _cl_S_CallEngine(PORT
                                                            , "127.0.0.1"
                                                            , Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["timeout"].ToString()));

                    _cl_S_EngineFormat es = new _cl_S_EngineFormat();
                    string message = "";

                    if (convertSoundex_flag)
                    {
                        message = es.FormatNearby_Sound(input);
                    }
                    else
                    {
                        message = es.FormatNearby(input);
                    }

                    string res = call.SendToEngine(message);

                    DataTable dt = new DataTable();
                    _cl_S_Format_Result format = new _cl_S_Format_Result();
                    dt = format.ToDataTable(res);
                    dt = format.SearchResultFormat(dt);
                    dt = format.SortDis(dt);
                    dt = format.AlongRouteFormat(dt);

                    if (userKey == USER_ID_DEBUG)
                    {
                        dt.TableName = "result_" + sw.ElapsedMilliseconds + "_";
                        dt.Columns.Add("polygon");

                        string mm = "";
                        for (int i = 0; i < input.Polygon.Count; i++)
                        {
                            mm += input.Polygon[i] + "?";
                        }
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dt.Rows[i]["polygon"] = mm;
                        }
                    }
                    else
                        dt.TableName = "result";

                    dt.AcceptChanges();

                    //DTtoCSV(dt);

                    return dt;

                }
                else if (type == TYPE_CLOSEST)
                {
                    _prop_Closest input = new _prop_Closest();
                    input.keyword = tmp_keyword;
                    input.syno = tmp_syno;
                    input.syno_wb = tmp_syno_wb;
                    input.thai_soundex = tmp_thai_soundex;
                    input.thai_soundex_delimeter = tmp_thai_soundex_delimeter;

                    _cl_Find_Route aClose = new _cl_Find_Route();
                    input = aClose.Format(input, turnNode);

                    if (input == null)
                    {
                        Context.Response.Status = "400 Bad Request";
                        Context.Response.StatusCode = 400;
                        Context.ApplicationInstance.CompleteRequest();
                        Context.Response.End();

                        return DT_BadRequest();
                    }
                    else if (input.lat.Count < 2)
                    {
                        Context.Response.Status = "400 Bad Request";
                        Context.Response.StatusCode = 400;
                        Context.ApplicationInstance.CompleteRequest();
                        Context.Response.End();

                        return DT_BadRequest();
                    }

                    input = aClose.FindRouteID(input);
                    input.category = catCode;
                    if (cantextToCat_flag)
                    {
                        input.LocalCatCode = tmp_localCatCode;
                    }
                    else
                    {
                        input.LocalCatCode = localCatCode;
                    }
                    _cl_S_CallEngine call = new _cl_S_CallEngine(PORT
                                                           , "127.0.0.1"
                                                           , Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["timeout"].ToString()));

                    _cl_S_EngineFormat es = new _cl_S_EngineFormat();
                    string message = "";

                    if (convertSoundex_flag)
                    {
                        message = es.FormatClosest_Sound(input);
                    }
                    else { message = es.FormatClosest(input); }

                    string res = call.SendToEngine(message);

                    DataTable dt = new DataTable();
                    _cl_S_Format_Result format = new _cl_S_Format_Result();
                    dt = format.ToDataTable(res);
                    dt = format.SearchResultFormat(dt);
                    dt = format.SortDis(dt);

                    if (userKey == USER_ID_DEBUG)
                    {

                        dt.TableName = "result_" + sw.ElapsedMilliseconds + "_";
                        dt.Columns.Add("route");

                        string mm = "";

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dt.Rows[i]["route"] = input.routeID;
                        }

                    }
                    else
                        dt.TableName = "result";

                    dt.AcceptChanges();
                    return dt;
                }
                else
                {
                    Context.Response.Status = "400 Bad Request";
                    Context.Response.StatusCode = 400;
                    Context.ApplicationInstance.CompleteRequest();
                    Context.Response.End();

                    return DT_BadRequest();
                }
            }

            catch (Exception ex)
            {
                //DataTable dt = new DataTable();

                //dt.Columns.Add("error");
                //dt.Rows.Add(ex.ToString());
                //dt.TableName = "error";
                //return dt;

                //Context.Response.Status = "500 Internal Server Error";
                //Context.Response.StatusCode = 500;
                //Context.ApplicationInstance.CompleteRequest();
                //Context.Response.End();

                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("NotFound");
                connect.TableName = "result";
                return connect;
            }
        }

        [WebMethod(Description = "searchNostraID", EnableSession = true)]
        public DataTable searchNostraID(string nostraID, string userKey, string ipClient)
        {
            _cl_S_authen authen = new _cl_S_authen(I_SERVICE.IP_LIST, I_SERVICE.TOKEN_LIST);
            bool permission = authen.check(GetIP(), "");

            if (permission)
            {

            }
            else
            {

                Context.Response.Status = "403 Forbidden";
                Context.Response.StatusCode = 403;
                Context.ApplicationInstance.CompleteRequest();
                Context.Response.End();

                return DT_permission();
            }

            if (string.IsNullOrEmpty(userKey) || string.IsNullOrEmpty(nostraID))
            {
                Context.Response.Status = "400 Bad Request";
                Context.ApplicationInstance.CompleteRequest();
                Context.Response.StatusCode = 400;
                Context.Response.End();

                return DT_BadRequest();
            }

            try
            {
                _cl_S_CallEngine call = new _cl_S_CallEngine(Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["port"].ToString())
                                                           , "127.0.0.1"
                                                           , Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["timeout"].ToString()));

                string message = "14;" + nostraID.Trim();

                string res = call.SendToEngine(message);
                if (string.IsNullOrEmpty(res) || res == "\r\n")
                {
                    DataTable connect = new DataTable();
                    connect.Columns.Add("result");
                    connect.Rows.Add("NotFound");
                    connect.TableName = "result";
                    return connect;
                }
                else
                {

                    DataTable dt = new DataTable();
                    _cl_S_StringToDT toDT = new _cl_S_StringToDT();
                    dt = toDT.ToDataTable(res, 1, 1);

                    string totalS = dt.TableName;

                    _cl_S_Format_Result result_dt = new _cl_S_Format_Result();
                    dt = result_dt.SearchResultFormat(dt);
                    dt.Rows[0]["dist"] = DBNull.Value;
                    dt.TableName = "result";
                    dt.AcceptChanges();

                    try
                    {
                        if (System.Configuration.ConfigurationSettings.AppSettings["KeepLogNid"].ToString().ToUpper().Trim() == "TRUE")
                        {
                            WriteLogNostraID(DateTime.Now.ToString()
                                            + "\t" + GetIP()
                                            + "\t" + userKey
                                            + "\t" + ipClient
                                            + "\t" + nostraID
                                            );

                        }
                    }
                    catch
                    { }

                    return dt;

                }
            }
            catch
            {
                DataTable connect = new DataTable();
                connect.Columns.Add("result");
                connect.Rows.Add("NotFound");
                connect.TableName = "result";
                return connect;
            }


        }
        //********** FUNCTION SP **********/
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
                    //if (check_number.IsMatch(list[i]))
                    //{
                    //    result[0] += " " + check_number.Match(list[i]);
                    //    result[1] += " " + check_number.Match(list[i]);
                    //    list[i] = list[i].Replace(check_number.Match(list[i]).ToString(), "");
                    //    continue;
                    //}
                    list[i] = "";
                }
            }
            try { result[0] = result[0].Trim(); }
            catch (Exception) { }
            try { result[1] = result[1].Trim(); }
            catch (Exception) { }


            return result;
        }

        private bool CheckThis_Thai(string input)
        {
            Regex check_thai = new Regex(@"[ก-ฮฯ-ูเ-ํ๑-๙]+");
            if (check_thai.IsMatch(input))
                return true;
            else
                return false;
        }
        private DataTable IdenFormat(DataTable dt, string mode, DataTable dt_admin)
        {
            _cl_S_Format_Result fr = new _cl_S_Format_Result();
            if (mode == "public")
            {
                try
                {
                    dt.Columns.Remove("OBJECTID_1");
                }
                catch
                {

                }
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

                dt.Rows[0]["Name_L"] = dt.Rows[0]["NAMETH"];
                dt.Columns.Remove("NAMT");
                dt.Columns.Remove("NAMETH");
                dt.Rows[0]["Name_E"] = dt.Rows[0]["NAME"];
                dt.Columns.Remove("NAME");
                dt.Columns.Remove("NAMEEN");

                dt.Rows[0]["AdminLevel1_E"] = dt.Rows[0]["PROV_NAME"];
                dt.Rows[0]["AdminLevel2_E"] = dt.Rows[0]["AMP_NAME"];
                dt.Rows[0]["AdminLevel3_E"] = dt.Rows[0]["TAM_NAME"];
                dt.Rows[0]["AdminLevel4_E"] = "";

                dt.Rows[0]["AdminLevel1_L"] = dt.Rows[0]["PROV_NAMT"];
                dt.Rows[0]["AdminLevel2_L"] = dt.Rows[0]["AMP_NAMT"];
                dt.Rows[0]["AdminLevel3_L"] = dt.Rows[0]["TAM_NAMT"];
                dt.Rows[0]["AdminLevel4_L"] = "";

                dt.Rows[0]["LocalCatCode"] = dt.Rows[0]["sub_code"];

                dt.Columns.Remove("sub_code");

                try
                {
                    dt.Columns.Remove("objectid");
                }
                catch { }

                try
                {
                    dt.Columns.Remove("LDMTAG");
                }
                catch { }
                try
                {
                    dt.Columns.Remove("TYPE");
                }
                catch { }
                try
                {
                    dt.Columns.Remove("V22_2");
                }
                catch { }
                try
                {
                    dt.Columns.Remove("AREATAG");
                }
                catch { }
                try
                {
                    dt.Columns.Remove("POST_RD_T");
                }
                catch { }

                //dt.Columns.Remove("PROV_CODE");
                //dt.Columns.Remove("AMP_CODE");
                //dt.Columns.Remove("TAM_CODE");

                dt.Columns["PROV_CODE"].ColumnName = "AdminLevel1Code";
                dt.Columns["AMP_CODE"].ColumnName = "AdminLevel2Code";
                dt.Columns["TAM_CODE"].ColumnName = "AdminLevel3Code";

                dt.Columns.Remove("PROV_NAME");
                dt.Columns.Remove("AMP_NAME");
                dt.Columns.Remove("TAM_NAME");

                dt.Columns.Remove("PROV_NAMT");
                dt.Columns.Remove("AMP_NAMT");
                dt.Columns.Remove("TAM_NAMT");

                try
                {
                    dt.Columns.Remove("VERSION");
                }
                catch { }

                //dt.Columns.Remove("pid");

                dt.Columns["POSTCODE"].ColumnName = "PostCode";
                dt.Columns["catcode"].ColumnName = "Catcode";
                dt.Columns["pid"].ColumnName = "NostraId";

            }
            else if (mode == "admin")
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
                dt.Columns["POSTCODE"].ColumnName = "PostCode";
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

                //dt.Columns.Remove("PROV_CODE");
                //dt.Columns.Remove("AMP_CODE");
                //dt.Columns.Remove("TAM_CODE");

                dt.Columns["PROV_CODE"].ColumnName = "AdminLevel1Code";
                dt.Columns["AMP_CODE"].ColumnName = "AdminLevel2Code";
                dt.Columns["TAM_CODE"].ColumnName = "AdminLevel3Code";

                //dt.Columns.Remove("YEAR");
                //dt.Columns.Remove("MALE");
                //dt.Columns.Remove("FEMALE");
                //dt.Columns.Remove("TOTAL");
                //dt.Columns.Remove("HOUSE");
                //dt.Columns.Remove("V22_2");

                //dt.Columns.Remove("ADMTAG");
                dt.Columns.Remove("VERSION");
                dt.Columns.Remove("AMP_ID");
                dt.Columns.Remove("TAM_ID");
                //dt.Columns.Remove("SHAPE_Leng");
                //dt.Columns.Remove("OBJECTID_1");

            }
            else if (mode == "admin_s")
            {
                dt_admin.Columns.Add("NostraId");

                dt_admin.Columns.Add("Name_L");
                dt_admin.Columns.Add("Name_E");
                dt_admin.Columns.Add("Branch_E");
                dt_admin.Columns.Add("Branch_L");

                dt_admin.Columns.Add("AdminLevel1_E");
                dt_admin.Columns.Add("AdminLevel2_E");
                dt_admin.Columns.Add("AdminLevel3_E");
                dt_admin.Columns.Add("AdminLevel4_E");

                dt_admin.Columns.Add("AdminLevel1_L");
                dt_admin.Columns.Add("AdminLevel2_L");
                dt_admin.Columns.Add("AdminLevel3_L");
                dt_admin.Columns.Add("AdminLevel4_L");

                dt_admin.Columns.Add("HouseNo");
                dt_admin.Columns.Add("Telephone");
                dt_admin.Columns["POSTCODE"].ColumnName = "PostCode";
                dt_admin.Columns.Add("Catcode");
                dt_admin.Columns.Add("LocalCatCode");
                dt_admin.Columns.Add("dist");
                dt_admin.Columns.Add("No");


                dt_admin.Rows[0]["dist"] = "0";

                dt_admin.Rows[0]["Name_L"] = "พื้นที่";
                dt_admin.Rows[0]["Name_E"] = "Area";
                dt_admin.Rows[0]["Catcode"] = "";

                dt_admin.Rows[0]["AdminLevel1_E"] = dt_admin.Rows[0]["prov_name"];
                dt_admin.Rows[0]["AdminLevel2_E"] = dt_admin.Rows[0]["amp_name"];
                dt_admin.Rows[0]["AdminLevel3_E"] = dt_admin.Rows[0]["tam_name"];

                dt_admin.Rows[0]["AdminLevel1_L"] = dt_admin.Rows[0]["prov_namt"];
                dt_admin.Rows[0]["AdminLevel2_L"] = dt_admin.Rows[0]["amp_namt"];
                dt_admin.Rows[0]["AdminLevel3_L"] = dt_admin.Rows[0]["tam_namt"];

                if (dt.Rows[0]["Catcode"].ToString().Trim() == "STREET")
                {
                    dt_admin.Rows[0]["AdminLevel4_E"] = dt.Rows[0]["Name_E"];
                    dt_admin.Rows[0]["AdminLevel4_L"] = dt.Rows[0]["Name_L"];
                }
                else
                {
                    dt_admin.Rows[0]["AdminLevel4_E"] = dt.Rows[0]["AdminLevel4_E"];
                    dt_admin.Rows[0]["AdminLevel4_L"] = dt.Rows[0]["AdminLevel4_L"];
                }

                dt_admin.Columns.Remove("objectid");
                dt_admin.Columns.Remove("POLYTYPE");

                dt_admin.Columns.Remove("PROV_NAME");
                dt_admin.Columns.Remove("AMP_NAME");
                dt_admin.Columns.Remove("TAM_NAME");

                dt_admin.Columns.Remove("PROV_NAMT");
                dt_admin.Columns.Remove("AMP_NAMT");
                dt_admin.Columns.Remove("TAM_NAMT");

                dt_admin.Columns["PROV_CODE"].ColumnName = "AdminLevel1Code";
                dt_admin.Columns["AMP_CODE"].ColumnName = "AdminLevel2Code";
                dt_admin.Columns["TAM_CODE"].ColumnName = "AdminLevel3Code";

                //dt_admin.Columns.Remove("YEAR");
                //dt_admin.Columns.Remove("MALE");
                //dt_admin.Columns.Remove("FEMALE");
                //dt_admin.Columns.Remove("TOTAL");
                //dt_admin.Columns.Remove("HOUSE");
                //dt_admin.Columns.Remove("V22_2");

                //dt_admin.Columns.Remove("ADMTAG");
                dt_admin.Columns.Remove("VERSION");
                dt_admin.Columns.Remove("AMP_ID");
                dt_admin.Columns.Remove("TAM_ID");
                //dt_admin.Columns.Remove("SHAPE_Leng");
                //dt_admin.Columns.Remove("OBJECTID_1");

                dt_admin.TableName = "result";
                dt_admin.AcceptChanges();
                return fr.IdenFormatSort(dt_admin);

            }
            else if (mode == "solr")
            {
                dt.Columns.Remove("public_id");
                dt.Columns.Add("No");

                dt.Columns["Admin_id1"].ColumnName = "AdminLevel1Code";
                dt.Columns["Admin_id2"].ColumnName = "AdminLevel2Code";
                dt.Columns["Admin_id3"].ColumnName = "AdminLevel3Code";
                //dt.Columns["Catcode"].ColumnName = "CatCode";
                //dt.Columns["dist"].ColumnName = "Dist";

                //dt.Columns.Add("AdminLevel1Code");
                //dt.Columns.Add("AdminLevel2Code");
                //dt.Columns.Add("AdminLevel3Code");


                //dt.Rows[0]["AdminLevel1Code"] = dt_admin.Rows[0]["PROV_CODE"];
                //dt.Rows[0]["AdminLevel2Code"] = dt_admin.Rows[0]["AMP_CODE"];
                //dt.Rows[0]["AdminLevel3Code"] = dt_admin.Rows[0]["TAM_CODE"];

                dt.Rows[0]["AdminLevel1Code"] = dt.Rows[0]["Admin_Code"].ToString().Substring(0, 2);
                dt.Rows[0]["AdminLevel2Code"] = dt.Rows[0]["Admin_Code"].ToString().Substring(2, 2);
                dt.Rows[0]["AdminLevel3Code"] = dt.Rows[0]["Admin_Code"].ToString().Substring(4, 2);

                dt.Columns.Remove("Admin_Code");
            }
            else if (mode == "solr2")
            {
                dt.Columns.Remove("public_id");
                dt.Columns.Add("No");

                dt.Columns["Admin_id1"].ColumnName = "AdminLevel1Code";
                dt.Columns["Admin_id2"].ColumnName = "AdminLevel2Code";
                dt.Columns["Admin_id3"].ColumnName = "AdminLevel3Code"; ;

                dt.Rows[0]["AdminLevel1Code"] = dt.Rows[0]["Admin_Code"].ToString().Substring(0, 2);
                dt.Rows[0]["AdminLevel2Code"] = dt.Rows[0]["Admin_Code"].ToString().Substring(2, 2);
                dt.Rows[0]["AdminLevel3Code"] = dt.Rows[0]["Admin_Code"].ToString().Substring(4, 2);

                dt.Columns.Remove("Admin_Code");


                dt.Rows[0]["NostraId"] = null;

                dt.Rows[0]["Name_L"] = "พื้นที่";
                dt.Rows[0]["Name_E"] = "Area";

                dt.Rows[0]["Branch_E"] = null;
                dt.Rows[0]["Branch_L"] = null;

                dt.Rows[0]["AdminLevel4_E"] = "";
                dt.Rows[0]["AdminLevel4_L"] = "";

                dt.Rows[0]["HouseNo"] = null;
                dt.Rows[0]["Telephone"] = null;
                //dt.Rows[0]["PostCode"] = "";
                dt.Rows[0]["Catcode"] = "";
                dt.Rows[0]["LocalCatCode"] = null;

                dt.Rows[0]["dist"] = "0";

            }
            else if (mode == "LTRANS")
            {
                dt_admin.Columns.Add("NostraId");

                dt_admin.Columns.Add("Name_L");
                dt_admin.Columns.Add("Name_E");
                dt_admin.Columns.Add("Branch_E");
                dt_admin.Columns.Add("Branch_L");

                dt_admin.Columns.Add("AdminLevel1_E");
                dt_admin.Columns.Add("AdminLevel2_E");
                dt_admin.Columns.Add("AdminLevel3_E");
                dt_admin.Columns.Add("AdminLevel4_E");

                dt_admin.Columns.Add("AdminLevel1_L");
                dt_admin.Columns.Add("AdminLevel2_L");
                dt_admin.Columns.Add("AdminLevel3_L");
                dt_admin.Columns.Add("AdminLevel4_L");

                dt_admin.Columns.Add("HouseNo");
                dt_admin.Columns.Add("Telephone");
                dt_admin.Columns["POSTCODE"].ColumnName = "PostCode";
                dt_admin.Columns.Add("Catcode");
                dt_admin.Columns.Add("LocalCatCode");
                dt_admin.Columns.Add("dist");
                dt_admin.Columns.Add("No");


                dt_admin.Rows[0]["dist"] = "0";

                dt_admin.Rows[0]["Name_L"] = dt.Rows[0]["NAMETH"].ToString().Trim();
                dt_admin.Rows[0]["Name_E"] = dt.Rows[0]["NAMEEN"].ToString().Trim();
                dt_admin.Rows[0]["Branch_E"] = "";
                dt_admin.Rows[0]["Branch_L"] = "";
                dt_admin.Rows[0]["LocalCatCode"] = "STREET";
                dt_admin.Rows[0]["Catcode"] = "STREET";
                double idL = 0;
                try
                {
                    idL = Convert.ToDouble((dt.Rows[0]["LAT"].ToString().Trim().Replace(".", "") + (dt.Rows[0]["LON"].ToString().Trim().Replace(".", ""))));
                    idL = idL / 7;
                }
                catch
                {

                }
                dt_admin.Rows[0]["NostraId"] = "LT" + idL.ToString("F0");
                dt_admin.Rows[0]["AdminLevel4_L"] = "";
                dt_admin.Rows[0]["AdminLevel4_E"] = "";
                dt_admin.Rows[0]["HouseNo"] = "";
                dt_admin.Rows[0]["Telephone"] = "";

                dt_admin.Rows[0]["AdminLevel1_E"] = dt_admin.Rows[0]["prov_name"];
                dt_admin.Rows[0]["AdminLevel2_E"] = dt_admin.Rows[0]["amp_name"];
                dt_admin.Rows[0]["AdminLevel3_E"] = dt_admin.Rows[0]["tam_name"];

                dt_admin.Rows[0]["AdminLevel1_L"] = dt_admin.Rows[0]["prov_namt"];
                dt_admin.Rows[0]["AdminLevel2_L"] = dt_admin.Rows[0]["amp_namt"];
                dt_admin.Rows[0]["AdminLevel3_L"] = dt_admin.Rows[0]["tam_namt"];


                dt_admin.Columns.Remove("objectid");
                dt_admin.Columns.Remove("POLYTYPE");

                dt_admin.Columns.Remove("PROV_NAME");
                dt_admin.Columns.Remove("AMP_NAME");
                dt_admin.Columns.Remove("TAM_NAME");

                dt_admin.Columns.Remove("PROV_NAMT");
                dt_admin.Columns.Remove("AMP_NAMT");
                dt_admin.Columns.Remove("TAM_NAMT");

                dt_admin.Columns["PROV_CODE"].ColumnName = "AdminLevel1Code";
                dt_admin.Columns["AMP_CODE"].ColumnName = "AdminLevel2Code";
                dt_admin.Columns["TAM_CODE"].ColumnName = "AdminLevel3Code";

                //dt_admin.Columns.Remove("YEAR");
                //dt_admin.Columns.Remove("MALE");
                //dt_admin.Columns.Remove("FEMALE");
                //dt_admin.Columns.Remove("TOTAL");
                //dt_admin.Columns.Remove("HOUSE");
                //dt_admin.Columns.Remove("V22_2");

                //dt_admin.Columns.Remove("ADMTAG");
                dt_admin.Columns.Remove("VERSION");
                dt_admin.Columns.Remove("AMP_ID");
                dt_admin.Columns.Remove("TAM_ID");
                //dt_admin.Columns.Remove("SHAPE_Leng");
                //dt_admin.Columns.Remove("OBJECTID_1");

                dt_admin.TableName = "result";
                dt_admin.AcceptChanges();
                return fr.IdenFormatSort(dt_admin);
            }
            else if (mode == "LTRANS_KM")
            {
                dt_admin.Columns.Add("NostraId");

                dt_admin.Columns.Add("Name_L");
                dt_admin.Columns.Add("Name_E");
                dt_admin.Columns.Add("Branch_E");
                dt_admin.Columns.Add("Branch_L");

                dt_admin.Columns.Add("AdminLevel1_E");
                dt_admin.Columns.Add("AdminLevel2_E");
                dt_admin.Columns.Add("AdminLevel3_E");
                dt_admin.Columns.Add("AdminLevel4_E");

                dt_admin.Columns.Add("AdminLevel1_L");
                dt_admin.Columns.Add("AdminLevel2_L");
                dt_admin.Columns.Add("AdminLevel3_L");
                dt_admin.Columns.Add("AdminLevel4_L");

                dt_admin.Columns.Add("HouseNo");
                dt_admin.Columns.Add("Telephone");
                dt_admin.Columns["POSTCODE"].ColumnName = "PostCode";
                dt_admin.Columns.Add("Catcode");
                dt_admin.Columns.Add("LocalCatCode");
                dt_admin.Columns.Add("dist");
                dt_admin.Columns.Add("No");


                dt_admin.Rows[0]["dist"] = "0";


                int KM_num = 0;
                try
                {
                    double kmm = 0;
                    kmm = Convert.ToDouble(dt.Rows[0]["M_PLUS"].ToString());
                    kmm = kmm * 0.01;
                    kmm = Math.Round(kmm);
                    kmm = kmm * 100;

                    KM_num = Convert.ToInt32(kmm);
                }
                catch
                { }
                if (KM_num > 0)
                {
                    dt_admin.Rows[0]["Name_L"] = dt.Rows[0]["NAMETH"].ToString().Trim() + " (กม." + dt.Rows[0]["KMNO"].ToString().Trim() + " +" + KM_num.ToString() + ")";
                    dt_admin.Rows[0]["Name_E"] = dt.Rows[0]["NAMEEN"].ToString().Trim() + " (KM." + dt.Rows[0]["KMNO"].ToString().Trim() + " +" + KM_num.ToString() + ")";
                }
                else
                {
                    dt_admin.Rows[0]["Name_L"] = dt.Rows[0]["NAMETH"].ToString().Trim() + " (กม." + dt.Rows[0]["KMNO"].ToString().Trim() + ")";
                    dt_admin.Rows[0]["Name_E"] = dt.Rows[0]["NAMEEN"].ToString().Trim() + " (KM." + dt.Rows[0]["KMNO"].ToString().Trim() + ")";
                }

                dt_admin.Rows[0]["Branch_E"] = "";
                dt_admin.Rows[0]["Branch_L"] = "";
                dt_admin.Rows[0]["LocalCatCode"] = "STREET";
                dt_admin.Rows[0]["Catcode"] = "STREET";
                double idL = 0;
                try
                {
                    idL = Convert.ToDouble((dt.Rows[0]["LAT"].ToString().Trim().Replace(".", "") + dt.Rows[0]["LON"].ToString().Trim().Replace(".", "")));
                    idL = idL / 7;
                }
                catch
                {

                }
                dt_admin.Rows[0]["NostraId"] = "LT" + idL.ToString("F0");
                dt_admin.Rows[0]["AdminLevel4_L"] = "";
                dt_admin.Rows[0]["AdminLevel4_E"] = "";
                dt_admin.Rows[0]["HouseNo"] = "";
                dt_admin.Rows[0]["Telephone"] = "";

                dt_admin.Rows[0]["AdminLevel1_E"] = dt_admin.Rows[0]["prov_name"];
                dt_admin.Rows[0]["AdminLevel2_E"] = dt_admin.Rows[0]["amp_name"];
                dt_admin.Rows[0]["AdminLevel3_E"] = dt_admin.Rows[0]["tam_name"];

                dt_admin.Rows[0]["AdminLevel1_L"] = dt_admin.Rows[0]["prov_namt"];
                dt_admin.Rows[0]["AdminLevel2_L"] = dt_admin.Rows[0]["amp_namt"];
                dt_admin.Rows[0]["AdminLevel3_L"] = dt_admin.Rows[0]["tam_namt"];


                dt_admin.Columns.Remove("objectid");
                dt_admin.Columns.Remove("POLYTYPE");

                dt_admin.Columns.Remove("PROV_NAME");
                dt_admin.Columns.Remove("AMP_NAME");
                dt_admin.Columns.Remove("TAM_NAME");

                dt_admin.Columns.Remove("PROV_NAMT");
                dt_admin.Columns.Remove("AMP_NAMT");
                dt_admin.Columns.Remove("TAM_NAMT");

                dt_admin.Columns["PROV_CODE"].ColumnName = "AdminLevel1Code";
                dt_admin.Columns["AMP_CODE"].ColumnName = "AdminLevel2Code";
                dt_admin.Columns["TAM_CODE"].ColumnName = "AdminLevel3Code";

                //dt_admin.Columns.Remove("YEAR");
                //dt_admin.Columns.Remove("MALE");
                //dt_admin.Columns.Remove("FEMALE");
                //dt_admin.Columns.Remove("TOTAL");
                //dt_admin.Columns.Remove("HOUSE");
                //dt_admin.Columns.Remove("V22_2");

                //dt_admin.Columns.Remove("ADMTAG");
                dt_admin.Columns.Remove("VERSION");
                dt_admin.Columns.Remove("AMP_ID");
                dt_admin.Columns.Remove("TAM_ID");
                //dt_admin.Columns.Remove("SHAPE_Leng");
                //dt_admin.Columns.Remove("OBJECTID_1");

                dt_admin.TableName = "result";
                dt_admin.AcceptChanges();
                return fr.IdenFormatSort(dt_admin);
            }

            dt.TableName = "result";
            dt.AcceptChanges();
            return fr.IdenFormatSort(dt);

        }
        private bool sp_catcode(string input)
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

        private bool WriteLog(string log)
        {
            try
            {
                string FilePath = LOG_PATH + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                StreamWriter F = File.AppendText(FilePath);
                F.WriteLine(log);
                F.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool WriteLogTotal(string log)
        {
            try
            {
                string FilePath = LOG_PATH + "Total" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                StreamWriter F = File.AppendText(FilePath);
                F.WriteLine(log);
                F.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool WriteLogIden(string log)
        {
            try
            {
                string FilePath = LOG_PATH + "Iden" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                StreamWriter F = File.AppendText(FilePath);
                F.WriteLine(log);
                F.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool WriteLogAroute(string log)
        {
            try
            {
                string FilePath = LOG_PATH + "Aroute" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                StreamWriter F = File.AppendText(FilePath);
                F.WriteLine(log);
                F.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool WriteLogNostraID(string log)
        {
            try
            {
                string FilePath = LOG_PATH + "NostraID" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                StreamWriter F = File.AppendText(FilePath);
                F.WriteLine(log);
                F.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private DataTable DT_BadRequest()
        {
            DataTable connect = new DataTable();
            connect.Columns.Add("result");
            connect.Rows.Add("Bad Request");
            connect.TableName = "result";

            return connect;
        }
        private DataTable DT_permission()
        {
            DataTable connect = new DataTable();
            connect.Columns.Add("result");
            connect.Rows.Add("You don't have permission.");
            connect.TableName = "Fail to Connect";
            return connect;
        }

        private bool CheckAutoCompleteMaxLimit(int input)
        {
            if (input >= MAX_AUTO)
                return false;
            else
                return true;
        }


    }
}
