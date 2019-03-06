using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using webservice_search_a_route;

namespace Search_Service_POI
{
    public class _cl_S_EngineFormat
    {
        public string GetSearchFormat(_prop_SearchInput input)
        {
            string message ="";


                message += "1;";//type
                message += input.keyword.Trim() + ";";
                message += input.syno.Trim() + ";";
                message += input.syno_wb.Trim() + ";";
                message += ";";//th
                message += ";";//other
                message += ";";//nostraid
                message += input.AdminLevel4 + ";";//adminlv4
                message += input.AdminLevel3 + ";";//adminlv3
                message += input.AdminLevel2 + ";";//adminlv2
                message += input.AdminLevel1 + ";";//adminlv1

                message += input.houseNumber + ";";
                message += input.telephone + ";";

                message += input.category + ";";//cat
                message += input.LocalCatCode + ";";//subcode,localCatCode
                message += input.tag + ";";//tag
                message += input.PostCode + ";";//PostCode

                message += input.lat + ";";//lat
                message += input.lon + ";";//lon
                message += input.radius + ";";//r
                message += input.maxReturn;//return

            return message;
        }

        public string GetTotalSearchFormat(_prop_SearchInput input)
        {
            string message = "";


            message += "7;";//type
            message += input.keyword.Trim() + ";";
            message += input.syno.Trim() + ";";
            message += input.syno_wb.Trim() + ";";
            message += ";";//th
            message += ";";//other
            message += ";";//nostraid
            message += input.AdminLevel4 + ";";//adminlv4
            message += input.AdminLevel3 + ";";//adminlv3
            message += input.AdminLevel2 + ";";//adminlv2
            message += input.AdminLevel1 + ";";//adminlv1

            message += input.houseNumber + ";";
            message += input.telephone + ";";

            message += input.category + ";";//cat
            message += input.LocalCatCode + ";";//subcode,localCatCode
            message += input.tag + ";";//tag
            message += input.PostCode + ";";//PostCode

            message += input.lat + ";";//lat
            message += input.lon + ";";//lon
            message += input.radius + ";";//r
            message += input.maxReturn;//return

            return message;
        }

        public string FormatNearby(_prop_Nearby input)
        {
            string message = "";


            message += "8;";//type
            message += input.keyword.Trim() + ";";
            message += input.syno.Trim() + ";";
            message += input.syno_wb.Trim() + ";";

            message += input.category + ";";//cat
            message += input.LocalCatCode + ";";//subcode,localCatCode

            message += input.lat[0].ToString() + ";";//Lat Start
            message += input.lon[0].ToString() + ";";//Lon Start

            for (int i = 0; i < input.Polygon.Count; i++)
            {
                message += input.Polygon[i] + "?";
            }

            message = message.Substring(0, message.Length - 1) + ";";
            message += input.maxReturn;//return

            return message;
        }

        public string FormatClosest(_prop_Closest input)
        {
            string message = "";


            message += "9;";//type
            message += input.keyword.Trim() + ";";
            message += input.syno.Trim() + ";";
            message += input.syno_wb.Trim() + ";";

            message += input.category + ";";//cat
            message += input.LocalCatCode + ";";//subcode,localCatCode

            message += input.lat[0].ToString() + ";";//Lat Start
            message += input.lon[0].ToString() + ";";//Lon Start

            message += input.routeID + ";";//routeID all
            message += input.maxReturn;//return

            return message;
        }

        public string GetSearch_Soundex_Format(_prop_SearchInput input)
        {
            string message = "";


            message += "10;";//type
            message += input.syno.Trim() + ";";
            message += input.thai_soundex_delimeter.Trim() + ";";
            message += input.thai_soundex.Trim() + ";";
            message += ";";//th
            message += ";";//other
            message += ";";//nostraid
            message += input.AdminLevel4 + ";";//adminlv4
            message += input.AdminLevel3 + ";";//adminlv3
            message += input.AdminLevel2 + ";";//adminlv2
            message += input.AdminLevel1 + ";";//adminlv1
            
            message += input.houseNumber + ";";
            message += input.telephone + ";";

            message += input.category + ";";//cat
            message += input.LocalCatCode + ";";//subcode,localCatCode
            message += input.tag + ";";//tag
            message += input.PostCode + ";";//PostCode

            message += input.lat + ";";//lat
            message += input.lon + ";";//lon
            message += input.radius + ";";//r
            message += input.maxReturn;//return

            return message;
        }

        public string GetTotalSearchFormat_Sound(_prop_SearchInput input)
        {
            string message = "";


            message += "11;";//type
            message += input.keyword.Trim() + ";";
            message += input.thai_soundex_delimeter.Trim() + ";";
            message += input.thai_soundex.Trim() + ";";
            message += ";";//th
            message += ";";//other
            message += ";";//nostraid
            message += input.AdminLevel4 + ";";//adminlv4
            message += input.AdminLevel3 + ";";//adminlv3
            message += input.AdminLevel2 + ";";//adminlv2
            message += input.AdminLevel1 + ";";//adminlv1

            message += input.houseNumber + ";";
            message += input.telephone + ";";

            message += input.category + ";";//cat
            message += input.LocalCatCode + ";";//subcode,localCatCode
            message += input.tag + ";";//tag
            message += input.PostCode + ";";//PostCode

            message += input.lat + ";";//lat
            message += input.lon + ";";//lon
            message += input.radius + ";";//r
            message += input.maxReturn;//return

            return message;
        }

        public string FormatNearby_Sound(_prop_Nearby input)
        {
            string message = "";


            message += "12;";//type
            message += input.keyword.Trim() + ";";
            message += input.thai_soundex_delimeter.Trim() + ";";
            message += input.thai_soundex.Trim() + ";";

            message += input.category + ";";//cat
            message += input.LocalCatCode + ";";//subcode,localCatCode

            message += input.lat[0].ToString() + ";";//Lat Start
            message += input.lon[0].ToString() + ";";//Lon Start

            for (int i = 0; i < input.Polygon.Count; i++)
            {
                message += input.Polygon[i] + "?";
            }

            message = message.Substring(0, message.Length - 1) + ";";
            message += input.maxReturn;//return

            return message;
        }

        public string FormatClosest_Sound(_prop_Closest input)
        {
            string message = "";


            message += "13;";//type
            message += input.keyword.Trim() + ";";
            message += input.thai_soundex_delimeter.Trim() + ";";
            message += input.thai_soundex.Trim() + ";";

            message += input.category + ";";//cat
            message += input.LocalCatCode + ";";//subcode,localCatCode

            message += input.lat[0].ToString() + ";";//Lat Start
            message += input.lon[0].ToString() + ";";//Lon Start

            message += input.routeID + ";";//routeID all
            message += input.maxReturn;//return

            return message;
        }

        public string GetSearchFormat_big(_prop_SearchInput input)
        {
            string message = "";


            message += "15;";//type
            message += input.keyword.Trim() + ";";
            message += input.syno.Trim() + ";";
            message += ";";
            message += ";";//th
            message += ";";//other
            message += ";";//nostraid
            message += input.AdminLevel4 + ";";//adminlv4
            message += input.AdminLevel3 + ";";//adminlv3
            message += input.AdminLevel2 + ";";//adminlv2
            message += input.AdminLevel1 + ";";//adminlv1

            message += input.houseNumber + ";";
            message += input.telephone + ";";

            message += input.category + ";";//cat
            message += input.LocalCatCode + ";";//subcode,localCatCode
            message += input.tag + ";";//tag
            message += input.PostCode + ";";//PostCode

            message += input.lat + ";";//lat
            message += input.lon + ";";//lon
            message += input.radius + ";";//r
            message += input.maxReturn;//return

            return message;
        }
    }
}