using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Service_POI
{
    public class _cl_S_Parameter
    {
        int MAXRETURN = 50;
        public _cl_S_Parameter(int MAX)
        {
            MAXRETURN = MAX;
        }

        public _prop_SearchInput Check(string keyword, string AdminLevel3, string AdminLevel2, string AdminLevel1, string PostCode, string AdminLevel4,string houseNumber,string telephone, string category, string LocalCatCode, string tag, string lat, string lon, string radius, string RowsPerPage, string PageNumber, string token)
        {
            _prop_SearchInput input = new _prop_SearchInput();

            if (string.IsNullOrEmpty(RowsPerPage))
                RowsPerPage = "20";
            
            if (string.IsNullOrEmpty(PageNumber))
                PageNumber = "1";

            //(input > 0) ? "positive" : "negative";
            input.keyword = (string.IsNullOrEmpty(keyword)) ? "" : keyword;
            
            input.AdminLevel1 = (string.IsNullOrEmpty(AdminLevel1)) ? "" : AdminLevel1;
            input.AdminLevel2 = (string.IsNullOrEmpty(AdminLevel2)) ? "" : AdminLevel2;
            input.AdminLevel3 = (string.IsNullOrEmpty(AdminLevel3)) ? "" : AdminLevel3;
            input.AdminLevel4 = (string.IsNullOrEmpty(AdminLevel4)) ? "" : AdminLevel4;
            input.PostCode = (string.IsNullOrEmpty(PostCode)) ? "" : PostCode;

            input.houseNumber = (string.IsNullOrEmpty(houseNumber)) ? "" : houseNumber;
            input.telephone = (string.IsNullOrEmpty(telephone)) ? "" : telephone;

            input.category = (string.IsNullOrEmpty(category)) ? "" : category;
            input.LocalCatCode = (string.IsNullOrEmpty(LocalCatCode)) ? "" : LocalCatCode;
            input.tag = "";

            try
            {
                double tmp = 0;
                if (double.TryParse(lat, out tmp))
                    input.lat = lat;
                else
                    input.lat = "";

                if (double.TryParse(lon, out tmp))
                    input.lon = lon;
                else
                    input.lon = "";

                if (double.TryParse(radius, out tmp))
                {
                    tmp = tmp * 1000;
                    input.radius = tmp.ToString();
                }
                else
                    input.radius = "";

                if(string.IsNullOrEmpty(input.lat) || string.IsNullOrEmpty(input.lon))
                {
                    input.lat = "";
                    input.lon = "";
                    input.radius = "";
                }
                else if (!string.IsNullOrEmpty(input.lat) && !string.IsNullOrEmpty(input.lon) && string.IsNullOrEmpty(input.radius))
                {
                    input.radius = "1000000";
                }
            }
            catch
            {
                input.lat = "";
                input.lon = "";
                input.radius = "";
            }

            input.RowsPerPage = (string.IsNullOrEmpty(RowsPerPage)) ? "" : RowsPerPage;
            input.PageNumber = (string.IsNullOrEmpty(PageNumber)) ? "" : PageNumber;
            input.maxReturn = MAXRETURN.ToString();

            return input;
        }

        public bool CheckInputFormat(string keyword, string AdminLevel3, string AdminLevel2, string AdminLevel1, string PostCode, string AdminLevel4, string category, string LocalCatCode, string tag, string lat, string lon, string radius, string RowsPerPage, string PageNumber, string token)
        {
            try
            {
                //** USE catch **//
                // lat lon double only
                // Error to catch Return
                if (!string.IsNullOrEmpty(lat) || !string.IsNullOrEmpty(lon))
                {
                    double latd = Convert.ToDouble(lat);
                    double lond = Convert.ToDouble(lon);
                }
                if (!string.IsNullOrEmpty(radius))
                {
                    double rd = Convert.ToDouble(radius);
                }
                if (!string.IsNullOrEmpty(RowsPerPage) || !string.IsNullOrEmpty(PageNumber))
                {
                    int r_int = Convert.ToInt32(RowsPerPage);
                    int p_int = Convert.ToInt32(PageNumber);
                }


                //** Return Error **//
                // lat with lon 
                // If not [Duo] input return error
                if (
                    (!string.IsNullOrEmpty(lat) && string.IsNullOrEmpty(lon))
                    ||
                    (string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
                    )
                {

                    return false;
                }

                // Can input Radius When input Lat Lon
                if (!string.IsNullOrEmpty(radius) &&
                    ((string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lon)))
                    )
                {
                    return false;
                }
                // Input Row & Page [Duo] Only
                if (
                    (!string.IsNullOrEmpty(RowsPerPage) && string.IsNullOrEmpty(PageNumber))
                    ||
                    (string.IsNullOrEmpty(RowsPerPage) && !string.IsNullOrEmpty(PageNumber))
                    )
                {
                    return false;
                }

                //Case AdminLV
                if (string.IsNullOrEmpty(AdminLevel1) && !string.IsNullOrEmpty(AdminLevel2)
                    ||
                    string.IsNullOrEmpty(AdminLevel1) && !string.IsNullOrEmpty(AdminLevel3)
                    ||
                    string.IsNullOrEmpty(AdminLevel2) && !string.IsNullOrEmpty(AdminLevel3)
                    )
                {
                    return false;
                }

                //case Empty input
                if (
                    string.IsNullOrEmpty(keyword)
                    && string.IsNullOrEmpty(AdminLevel3)
                    && string.IsNullOrEmpty(AdminLevel2)
                    && string.IsNullOrEmpty(AdminLevel1)
                    && string.IsNullOrEmpty(PostCode)
                    && string.IsNullOrEmpty(AdminLevel4)
                    && string.IsNullOrEmpty(category)
                    && string.IsNullOrEmpty(LocalCatCode)
                    && string.IsNullOrEmpty(tag)
                    && string.IsNullOrEmpty(lat)
                    && string.IsNullOrEmpty(lon)
                    && string.IsNullOrEmpty(radius)
                    && string.IsNullOrEmpty(RowsPerPage)
                    && string.IsNullOrEmpty(PageNumber)
                    )
                {
                    return false;
                }

                if (
                    string.IsNullOrEmpty(keyword)

                    && string.IsNullOrEmpty(AdminLevel3)
                    && string.IsNullOrEmpty(AdminLevel2)
                    && string.IsNullOrEmpty(AdminLevel1)

                    && string.IsNullOrEmpty(category)
                    && string.IsNullOrEmpty(LocalCatCode)

                    && string.IsNullOrEmpty(lat)
                    && string.IsNullOrEmpty(lon)
                    )
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CheckInputFormat(string keyword, string AdminLevel3, string AdminLevel2, string AdminLevel1, string PostCode, string AdminLevel4, string houseNumber, string telephone, string category, string LocalCatCode, string tag, string lat, string lon, string radius, string RowsPerPage, string PageNumber, string token, string userKey)
        {
            try
            {
                if (string.IsNullOrEmpty(userKey))
                {
                    return false;
                }
                //** USE catch **//
                // lat lon double only
                // Error to catch Return
                if (!string.IsNullOrEmpty(lat) || !string.IsNullOrEmpty(lon))
                {
                    double latd = Convert.ToDouble(lat);
                    double lond = Convert.ToDouble(lon);
                }
                if (!string.IsNullOrEmpty(radius))
                {
                    double rd = Convert.ToDouble(radius);
                }
                if (!string.IsNullOrEmpty(RowsPerPage) || !string.IsNullOrEmpty(PageNumber))
                {
                    int r_int = Convert.ToInt32(RowsPerPage);
                    int p_int = Convert.ToInt32(PageNumber);
                }


                //** Return Error **//
                // lat with lon 
                // If not [Duo] input return error
                if (
                    (!string.IsNullOrEmpty(lat) && string.IsNullOrEmpty(lon))
                    ||
                    (string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
                    )
                {

                    return false;
                }

                // Can input Radius When input Lat Lon
                if (!string.IsNullOrEmpty(radius) &&
                    ((string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lon)))
                    )
                {
                    return false;
                }
                // Input Row & Page [Duo] Only
                if (
                    (!string.IsNullOrEmpty(RowsPerPage) && string.IsNullOrEmpty(PageNumber))
                    ||
                    (string.IsNullOrEmpty(RowsPerPage) && !string.IsNullOrEmpty(PageNumber))
                    )
                {
                    return false;
                }

                //Case AdminLV
                /*
                if(string.IsNullOrEmpty(AdminLevel1) && !string.IsNullOrEmpty(AdminLevel2)
                    ||
                    string.IsNullOrEmpty(AdminLevel1) && !string.IsNullOrEmpty(AdminLevel3)
                    ||
                    string.IsNullOrEmpty(AdminLevel2) && !string.IsNullOrEmpty(AdminLevel3)
                    )
                {
                    return false;
                }*/

                //case Empty input
                if(
                    string.IsNullOrEmpty( keyword)
                    && string.IsNullOrEmpty( AdminLevel3)
                    && string.IsNullOrEmpty( AdminLevel2)
                    && string.IsNullOrEmpty( AdminLevel1)
                    && string.IsNullOrEmpty( PostCode)
                    && string.IsNullOrEmpty( AdminLevel4)
                    && string.IsNullOrEmpty(houseNumber)
                    && string.IsNullOrEmpty(telephone)
                    && string.IsNullOrEmpty( category)
                    && string.IsNullOrEmpty( LocalCatCode)
                    && string.IsNullOrEmpty( tag)
                    && string.IsNullOrEmpty( lat)
                    && string.IsNullOrEmpty( lon)
                    && string.IsNullOrEmpty( radius)
                    && string.IsNullOrEmpty( RowsPerPage)
                    && string.IsNullOrEmpty( PageNumber)
                    )
                {
                    return false;
                }

                if (
                    string.IsNullOrEmpty(keyword)

                    && string.IsNullOrEmpty(AdminLevel3)
                    && string.IsNullOrEmpty(AdminLevel2)
                    && string.IsNullOrEmpty(AdminLevel1)

                    && string.IsNullOrEmpty(PostCode)
                    && string.IsNullOrEmpty(AdminLevel4)

                    && string.IsNullOrEmpty(houseNumber)
                    && string.IsNullOrEmpty(telephone)

                    && string.IsNullOrEmpty(category)
                    && string.IsNullOrEmpty(LocalCatCode)

                    && string.IsNullOrEmpty(lat)
                    && string.IsNullOrEmpty(lon)
                    )
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}