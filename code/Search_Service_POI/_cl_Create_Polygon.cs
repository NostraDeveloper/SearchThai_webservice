using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EGIS.ShapeFileLib;

namespace webservice_search_a_route
{
    public class _cl_Create_Polygon
    {
        //private double R_EARTH = 6378.1;
        private double BUFFER = Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["_AROUTE_NEAR_BUFFER"].ToString());
        private double BUFFER_MIN = Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["_AROUTE_NEAR_BUFFER_MIN"].ToString());
        private double BUFFER_MAX = Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["_AROUTE_NEAR_BUFFER_MAX"].ToString());
        private double LIMIT_DIS = Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["_AROUTE_SUM_OF_DISTANCE"].ToString());
        /// <summary>
        /// Calculate Bearing
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        /// <returns></returns>
        public double Bearing(double lat1, double lon1, double lat2, double lon2)
        {
            double x = Math.Sin(lon2 - lon1) * Math.Cos(lat2);
            double y = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1);
            double b = Math.Atan2(x, y);
            return b * 180 / Math.PI;
        }

        /// <summary>
        /// Plus Angle right
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double BearingRight(double input)
        {
            return input + 90;
        }
        /// <summary>
        /// Plus Angle left
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double BearingLeft(double input)
        {
            return input - 90;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="bearing">องศา</param>
        /// <param name="dis">เมตร</param>
        /// <returns>[0] = lat, [1] = lon</returns>
        public double[] AnotherPoint(double lat, double lon, double bearing, double dis)
        {
            UtmCoordinate utm = ConversionFunctions.LLToUtm(23, lat, lon);
            UtmCoordinate utm_out = ConversionFunctions.LocationFromRangeBearing(utm, dis, bearing);

            LatLongCoordinate ll = ConversionFunctions.UtmToLL(23, utm_out);

            double[] output = new double[] { ll.Latitude, ll.Longitude };
            return output;
        }

        /// <summary>
        /// Check turnNode Format and input to _prop_Nearby
        /// </summary>
        /// <param name="input"></param>
        /// <returns>IF Wrong Format return null</returns>
        public _prop_Nearby CheckFormat(string input)
        {
            try
            {
                _prop_Nearby _par = new _prop_Nearby();

                string[] node = input.Split('|');

                for (int i = 0; i < node.Length; i++)
                {
                    string[] tmp_node = node[i].Split(',');
                    if (tmp_node.Length != 2)
                        return null;

                    double tmp_lat = Convert.ToDouble(tmp_node[0]);
                    double tmp_lon = Convert.ToDouble(tmp_node[1]);
                    int tmp_flag = Convert.ToInt32(tmp_node[2]);

                    _par.lat.Add(tmp_lat);
                    _par.lon.Add(tmp_lon);
                    _par.flag.Add(tmp_flag);
                }

                return _par;
            }
            catch {return null; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public _prop_Nearby CreateAllPolygon(_prop_Nearby input)
        {
            double sumOfDis = 0;
            double disPoint = BUFFER * 1000;


            for (int i = 0; i < input.lat.Count-1;i++ )
            {
                if(input.flag[i] !=0)
                {
                    continue;
                }

                double tmp_dis = Distance(input.lat[i], input.lon[i], input.lat[i + 1], input.lon[i + 1]);
                sumOfDis += tmp_dis;

                double tmp_bearing = Bearing(input.lat[i], input.lon[i], input.lat[i + 1], input.lon[i + 1]);

                double tmp_BR = BearingRight(tmp_bearing);
                double tmp_BL = BearingLeft(tmp_bearing);

                double[] pointStartLeft = AnotherPoint(input.lat[i], input.lon[i], tmp_BL, disPoint);
                double[] pointStartRight = AnotherPoint(input.lat[i], input.lon[i], tmp_BR, disPoint);

                double[] pointEndLeft = AnotherPoint(input.lat[i + 1], input.lon[i + 1], tmp_BL, disPoint);
                double[] pointEndRight = AnotherPoint(input.lat[i + 1], input.lon[i + 1], tmp_BR, disPoint);
                if(i+1 != input.lat.Count)
                {
                    pointEndLeft = AnotherPoint(pointEndLeft[0], pointEndLeft[1], tmp_bearing, disPoint);
                    pointEndRight = AnotherPoint(pointEndRight[0], pointEndRight[1], tmp_bearing, disPoint);
                }

                string tmp = PolygonFormat(pointStartLeft, pointStartRight, pointEndLeft, pointEndRight);
                input.Polygon.Add(tmp);

                if (sumOfDis >= LIMIT_DIS)
                    break;
            }


            return input;
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

        private  double rad2deg(double rad)
        {
            return (rad * 180 / Math.PI);
        }

        private string PolygonFormat(double[] pointStartLeft,double[] pointStartRight,double[] pointEndLeft,double[] pointEndRight)
        {
            string output = "";
            output += pointStartLeft[0].ToString() + "," + pointStartLeft[1].ToString() + "|";
            output += pointEndLeft[0].ToString() + "," + pointEndLeft[1].ToString() + "|";
            output += pointEndRight[0].ToString() + "," + pointEndRight[1].ToString() + "|";
            output += pointStartRight[0].ToString() + "," + pointStartRight[1].ToString() + "|";

            output += pointStartLeft[0].ToString() + "," + pointStartLeft[1].ToString();

            return output;
        }

        public _prop_Nearby Format(_prop_Nearby input, string turnNode)
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

                if (input.buffer < 0)
                    return null;
                else if (input.buffer > BUFFER_MAX)
                    BUFFER = BUFFER_MAX;
                else if (input.buffer < BUFFER_MIN)
                    BUFFER = BUFFER_MIN;
                else
                    BUFFER = input.buffer;
                
                return input;
            }
            catch
            { return null; }
        }
    }
}