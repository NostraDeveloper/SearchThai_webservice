using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
namespace Search_Service_POI
{
    public class _cl_map_hydro
    {
        private EGIS.ShapeFileLib.ShapeFile sf;
        private EGIS.ShapeFileLib.DbfReader dbr;
        private Encoding endcode;

        public _cl_map_hydro(string ShpPath, string DbfPath, Encoding code)
        {
            sf = new EGIS.ShapeFileLib.ShapeFile(ShpPath);
            dbr = new EGIS.ShapeFileLib.DbfReader(DbfPath);
            endcode = code;
        }
        public DataTable polygon(double lat, double lon)
        {
            try
            {
                EGIS.ShapeFileLib.PointD pt = new EGIS.ShapeFileLib.PointD();
                pt.X = lon;
                pt.Y = lat;
                int i = sf.GetShapeIndexContainingPoint(pt, 1);

                dbr.StringEncoding = endcode;

                string[] f = dbr.GetFieldNames();
                string[] data = dbr.GetFields(i);

                DataTable dt = new DataTable();

                for (int j = 0; j < f.Length; j++)
                {
                    dt.Columns.Add(f[j], typeof(string));
                }
                dt.Rows.Add();
                for (int k = 0; k < data.Length; k++)
                {
                    dt.Rows[0][k] = data[k].ToString().Trim();
                }
                dt.TableName = "result";
                return dt;
            }
            catch
            {
                return null;
            }
        }

        public DataTable Iden_Hydro(double lat, double lon)
        {
            try
            {
                DataTable dt = polygon(lat, lon);

                if (dt != null)
                {
                    dt.Columns["NAMETH"].ColumnName = "Name_L";
                    dt.Columns["NAMEEN"].ColumnName = "Name_E";

                    dt.Columns["PROV_CODE"].ColumnName = "AdminLevel1Code";
                    dt.Columns["AMP_CODE"].ColumnName = "AdminLevel2Code";
                    dt.Columns["TAM_CODE"].ColumnName = "AdminLevel3Code";

                    dt.Columns["PROV_NAMT"].ColumnName = "AdminLevel1_L";
                    dt.Columns["AMP_NAMT"].ColumnName = "AdminLevel2_L";
                    dt.Columns["TAM_NAMT"].ColumnName = "AdminLevel3_L";

                    dt.Columns["PROV_NAME"].ColumnName = "AdminLevel1_E";
                    dt.Columns["AMP_NAME"].ColumnName = "AdminLevel2_E";
                    dt.Columns["TAM_NAME"].ColumnName = "AdminLevel3_E";

                    dt.Columns["POSTCODE"].ColumnName = "PostCode";
                    dt.Columns["HYDROTAG"].ColumnName = "NostraId";

                    dt.Columns.Remove("OBJECTID");
                    dt.Columns.Remove("VERSION");


                    dt.Rows[0]["NostraId"] = "HY" +  Math.Ceiling(Convert.ToDouble(dt.Rows[0]["NostraId"].ToString())).ToString() ;

                    dt.Columns.Add("Catcode");
                    dt.Rows[0]["Catcode"] = "HYDROLOGY";
                    dt.Columns.Add("LocalCatCode");
                    dt.Rows[0]["LocalCatCode"] = "HYDROLOGY";

                    dt.Columns.Add("No");
                    dt.Columns.Add("dist");
                    dt.Columns.Add("Branch_E");
                    dt.Columns.Add("Branch_L");
                    dt.Columns.Add("AdminLevel4_E");
                    dt.Columns.Add("AdminLevel4_L");
                    dt.Columns.Add("HouseNo");
                    dt.Columns.Add("Telephone");

                    dt.Rows[0]["Branch_E"] = "";
                    dt.Rows[0]["Branch_L"] = "";
                    dt.Rows[0]["AdminLevel4_E"] = "";
                    dt.Rows[0]["AdminLevel4_L"] = "";
                    dt.Rows[0]["HouseNo"] = "";
                    dt.Rows[0]["Telephone"]= "";
                    dt.Rows[0]["dist"] = "0";

                    dt = CheckName(dt);

                    dt.Columns.Remove("PTYPE");
                    int i = 0;
                    dt.Columns["NostraId"].SetOrdinal(i++);
                    dt.Columns["Name_L"].SetOrdinal(i++);
                    dt.Columns["Name_E"].SetOrdinal(i++);
                    dt.Columns["Branch_L"].SetOrdinal(i++);
                    dt.Columns["Branch_E"].SetOrdinal(i++);
                    dt.Columns["HouseNo"].SetOrdinal(i++);
                    dt.Columns["AdminLevel1_L"].SetOrdinal(i++);
                    dt.Columns["AdminLevel1_E"].SetOrdinal(i++);
                    dt.Columns["AdminLevel2_L"].SetOrdinal(i++);
                    dt.Columns["AdminLevel2_E"].SetOrdinal(i++);
                    dt.Columns["AdminLevel3_L"].SetOrdinal(i++);
                    dt.Columns["AdminLevel3_E"].SetOrdinal(i++);
                    dt.Columns["AdminLevel4_L"].SetOrdinal(i++);
                    dt.Columns["AdminLevel4_E"].SetOrdinal(i++);
                    dt.Columns["Telephone"].SetOrdinal(i++);
                    dt.Columns["PostCode"].SetOrdinal(i++);
                    dt.Columns["Catcode"].SetOrdinal(i++);
                    dt.Columns["LocalCatCode"].SetOrdinal(i++);
                    dt.Columns["AdminLevel1Code"].SetOrdinal(i++);
                    dt.Columns["AdminLevel2Code"].SetOrdinal(i++);
                    dt.Columns["AdminLevel3Code"].SetOrdinal(i++);
                    dt.Columns["No"].SetOrdinal(i++);
                    dt.Columns["dist"].SetOrdinal(i++);


                    dt.TableName = "result";
                    dt.AcceptChanges();
                    return dt;
                }
                else
                    return null;
            }
            catch
            { return null; }

        }

        private DataTable CheckName(DataTable dt)
        {
            try
            {
                if (string.IsNullOrEmpty(dt.Rows[0]["Name_L"].ToString().Trim()))
                {
                    if (dt.Rows[0]["PTYPE"].ToString().Trim() == "1")
                        dt.Rows[0]["Name_L"] = "แม่น้ำ";
                    else if (dt.Rows[0]["PTYPE"].ToString().Trim() == "2")
                        dt.Rows[0]["Name_L"] = "คลอง";
                    else if (dt.Rows[0]["PTYPE"].ToString().Trim() == "3")
                        dt.Rows[0]["Name_L"] = "หนอง,บึง";
                    else if (dt.Rows[0]["PTYPE"].ToString().Trim() == "4")
                        dt.Rows[0]["Name_L"] = "ทะเล";
                    else if (dt.Rows[0]["PTYPE"].ToString().Trim() == "5")
                        dt.Rows[0]["Name_L"] = "บ่อน้ำ";
                }

                if (string.IsNullOrEmpty(dt.Rows[0]["Name_E"].ToString().Trim()))
                {
                    if (dt.Rows[0]["PTYPE"].ToString().Trim() == "1")
                        dt.Rows[0]["Name_E"] = "River";
                    else if (dt.Rows[0]["PTYPE"].ToString().Trim() == "2")
                        dt.Rows[0]["Name_E"] = "Canal";
                    else if (dt.Rows[0]["PTYPE"].ToString().Trim() == "3")
                        dt.Rows[0]["Name_E"] = "Nong, Bung";
                    else if (dt.Rows[0]["PTYPE"].ToString().Trim() == "4")
                        dt.Rows[0]["Name_E"] = "Sea";
                    else if (dt.Rows[0]["PTYPE"].ToString().Trim() == "5")
                        dt.Rows[0]["Name_E"] = "Pond";
                }

                return dt;
            }
            catch
            {
                return dt;
            }
        }
    }
}