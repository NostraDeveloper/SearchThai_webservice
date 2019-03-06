using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;

namespace Search_Service_POI
{
    public class _cl_map
    {
        private EGIS.ShapeFileLib.ShapeFile sf;
        private EGIS.ShapeFileLib.DbfReader dbr;
        private Encoding endcode;

        public _cl_map(string ShpPath, string DbfPath, Encoding code)
        {
            sf = new EGIS.ShapeFileLib.ShapeFile(ShpPath);
            dbr = new EGIS.ShapeFileLib.DbfReader(DbfPath);
            endcode = code;
        }

        //public DataTable polygon(double lat, double lon,EGIS.ShapeFileLib.ShapeFile shp ,EGIS.ShapeFileLib.DbfReader dbf)
        public DataTable polygon(double lat, double lon)
        {
            try
            {
                EGIS.ShapeFileLib.PointD pt = new EGIS.ShapeFileLib.PointD();
                pt.X = lon;
                pt.Y = lat;
                int i = sf.GetShapeIndexContainingPoint(pt, 0.1);
                //int i = shp.GetShapeIndexContainingPoint(pt, 1);

                dbr.StringEncoding = endcode;
                //dbf.StringEncoding = endcode;
                string[] f = dbr.GetFieldNames();
                string[] data = dbr.GetFields(i);

                //string[] f = dbf.GetFieldNames();
                //string[] data = dbf.GetFields(i);

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
    }
}