using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using Search_Service_POI;

namespace webservice_search_a_route
{
    public class _cl_Find_Route
    {
        private _cl_S_CallEngine EngineRouting = new _cl_S_CallEngine(Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["_AROUTE_Routing_port"].ToString())
                                                            , "127.0.0.1"
                                                            , Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["timeout"].ToString()));
        private double LIMIT_DIS = Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["_AROUTE_SUM_OF_DISTANCE"].ToString());        
        public _prop_Closest FindRouteID(_prop_Closest input)
        {
            double sumOfDis = 0;
            string routeID = "";
            int countRouteID = 0;
            
            for (int i = 0; i < input.lat.Count-1;i++ )
            {
                double tmp_dis = 0;
                
                if (input.flag[i] != 0)
                {
                    tmp_dis = Distance(input.lat[i], input.lon[i], input.lat[i + 1], input.lon[i + 1]);
                }
                else
                {
                    string tmpEngine = Routing(input.lat[i].ToString(), input.lon[i].ToString(), input.lat[i + 1].ToString(), input.lon[i + 1].ToString());

                    
                    string tmp_1Route = "";

                    if (!string.IsNullOrEmpty(tmpEngine.Trim()))
                    {
                        string[] tmpRes_arr = tmpEngine.Split(';');

                        // tmpDis (Km.) / Engine return (m.)
                        tmp_dis = Convert.ToDouble(tmpRes_arr[0]) / 1000.0;


                        for (int j = 1; j < tmpRes_arr.Length; j++)
                        {
                            tmp_1Route += tmpRes_arr[j].ToString().Trim() + " ";
                            countRouteID++;
                        }
                        routeID += " " + tmp_1Route;
                        //routeID = ClearSpace(routeID);
                        //if( routeID.Split(' ').Length >1000)
                        //{
                        //    break;
                        //}
                        if(countRouteID >1000)
                        { break; }
                    }
                    else
                    {
                        tmp_dis = Distance(input.lat[i], input.lon[i], input.lat[i + 1], input.lon[i + 1]);
                    }
                }
                sumOfDis += tmp_dis;
                if (sumOfDis >= LIMIT_DIS)
                    break;
            }

            input.routeID = routeID;
            return input;
        }

        public string Routing(string lat1, string lon1, string lat2, string lon2)
        {
            try
            {
                string message = lat1 + ";" + lon1 + ";" + lat2 + ";" + lon2;
                string output =  EngineRouting.SendToEngine(message);
                output = output.Replace(",", "");
                output = output.Replace("\0", "");
                return output;
            }
            catch
            { return ""; }
        }

        public _prop_Closest Format(_prop_Closest input, string turnNode)
        {
            try
            {
                string[] tmpAll = turnNode.Split('|');

                for (int i = 0; i < tmpAll.Length;i++ )
                {
                    string[] tmpnode = tmpAll[i].Split(',');

                    input.lat.Add(Convert.ToDouble(tmpnode[0]));
                    input.lon.Add(Convert.ToDouble(tmpnode[1]));

                    input.flag.Add(Convert.ToInt32(tmpnode[2]));
                }

                return input;
            }
            catch
            { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        /// <returns> distance in KM. Units</returns>
        public double Distance(double lat1, double lon1, double lat2, double lon2)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;

            return dist * 1.609344; ;
        }

        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private double rad2deg(double rad)
        {
            return (rad * 180 / Math.PI);
        }

        private string ClearSpace (string input)
        {
            try
            {
                string output = input;
                RegexOptions options = RegexOptions.None;
                Regex regex = new Regex("[ ]{2,}", options);
                output = output.Replace(input, " ");
                return output;
            }
            catch
            {
                return input;
            }
        }

    }
}