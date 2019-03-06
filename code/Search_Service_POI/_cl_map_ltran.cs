using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
/////////////////////
using EGIS.ShapeFileLib;
using System.Collections.ObjectModel;

namespace Search_Service_POI
{
    public class _cl_map_ltran
    {
        private string DBFFileName;
        private DbfReader dbr;
        private Encoding endcode = Encoding.UTF8;
        
        private double Radius = 0.1;
        private ShapeFile sf;
        private string ShapeFileName;
        private const int INDEX_WGS_84 = 23;
        public _cl_map_ltran()
        {

        }

        public _cl_map_ltran(string path_shapeFile,string path_dbFile)
        {
            SetshapeFile(path_shapeFile);
            SetdbfFile(path_dbFile);
            loadShp();

            SetEncode(Encoding.GetEncoding("windows-874"));
        }

        public _cl_map_ltran(string path_shapeFile, string path_dbFile, Encoding enc)
        {
            SetshapeFile(path_shapeFile);
            SetdbfFile(path_dbFile);
            loadShp();

            SetEncode(enc);
        }
        public string[] getdata(double lat, double lon)
        {
            try
            {
                PointD pt = new PointD();
                pt.X = lon;
                pt.Y = lat;
                int closestShape = this.sf.GetClosestShape(pt, this.Radius);
                return this.dbr.GetFields(closestShape);
            }
            catch
            {
                return null;
            }
        }

        public void loadShp()
        {
            this.sf = new ShapeFile(this.ShapeFileName);
            this.dbr = new DbfReader(this.DBFFileName);
            this.dbr.StringEncoding = this.endcode;
        }

        public void SetdbfFile(string path)
        {
            this.DBFFileName = path;
        }

        public void SetEncode(Encoding data)
        {
            this.endcode = data;
        }

        public void SetRadius(double r)
        {
            this.Radius = r;
        }

        public void SetshapeFile(string path)
        {
            this.ShapeFileName = path;
        }

        public double[] SnapToLine(double lat, double lon)
        {
            try
            {
                PointD pt = new PointD();
                pt.X = lon;
                pt.Y = lat;
                PolylineDistanceInfo polylineDistanceInfo = new PolylineDistanceInfo();
                this.sf.GetClosestShape(pt, this.Radius, out polylineDistanceInfo);
                PointD polylinePoint = polylineDistanceInfo.PolylinePoint;
                return new double[] { polylinePoint.X, polylinePoint.Y };
            }
            catch
            {
                return null;
            }
        }

        static public double DistanceBetweenLatLongPoints(double lat1,double lon1,double lat2,double lon2)
        {
            return ConversionFunctions.DistanceBetweenLatLongPoints(INDEX_WGS_84, lat1, lon1, lat2, lon2);
        }

        public _prop_SnapLtrans getAllData(double lat, double lon, double radius)
        {
            _prop_SnapLtrans data_gis = null;
            PolylineDistanceInfo polylineDistanceInfo;
            PointD pt = new PointD();
            
            pt.X = lon;
            pt.Y = lat;
            if (!(sf.ShapeType == ShapeType.PolyLine || sf.ShapeType == ShapeType.PolyLineM)) return null;
            int index;
            EGIS.ShapeFileLib.ConversionFunctions.RefEllipse = 23;
            index = sf.GetClosestShape(pt, radius, out polylineDistanceInfo);
            if (index >= 0)
            {
                Console.Out.WriteLine("LineSegmentSide:" + polylineDistanceInfo.LineSegmentSide);
                if (sf.ShapeType == ShapeType.PolyLine)
                {
                    data_gis = new _prop_SnapLtrans();
                    double total = 0;
                    PointD[] data = sf.GetShapeDataD(index)[0];
                    for (int i = 0; i < data.Length - 1; i++)
                    {
                        total = total + ConversionFunctions.DistanceBetweenLatLongPoints(INDEX_WGS_84, data[i].Y, data[i].X, data[i + 1].Y, data[i + 1].X);
                    }

                    double distance_to_end = 0;
                    for (int i = polylineDistanceInfo.PointIndex; i < data.Length - 1; i++)
                    {
                        distance_to_end = distance_to_end + ConversionFunctions.DistanceBetweenLatLongPoints(INDEX_WGS_84, data[i].Y, data[i].X, data[i + 1].Y, data[i + 1].X);
                    }

                    double distance_snap = ConversionFunctions.DistanceBetweenLatLongPoints(INDEX_WGS_84, lat, lon, polylineDistanceInfo.PolylinePoint.Y, polylineDistanceInfo.PolylinePoint.X);
                    string side_of_road = GetLineSegmentSideDescription(polylineDistanceInfo.LineSegmentSide);
                    double distance_to_start = total - distance_to_end;
                    data_gis.lattitude_on_line = polylineDistanceInfo.PolylinePoint.Y;
                    data_gis.lontitude_on_line = polylineDistanceInfo.PolylinePoint.X;
                    data_gis.side_of_line = side_of_road;
                    data_gis.snap_distance = distance_snap;
                    data_gis.index_shape = index;
                    data_gis.tVal = polylineDistanceInfo.TVal;
                    data_gis.calculate_length_line = total;
                    data_gis.distance_snap_to_start = distance_to_start;
                    data_gis.distance_snap_to_end = distance_to_end;
                    data_gis.data_db = this.dbr.GetFields(index);
                    data_gis.dt = DbfToTable(dbr.GetFieldNames(), data_gis.data_db);
                    if(data.Length > 0 )
                    {
                        data_gis.lat_start = data[0].Y;
                        data_gis.lon_start = data[0].X;
                        data_gis.lat_end = data[data.Length - 1].Y;
                        data_gis.lon_end = data[data.Length - 1].X;
                    }
                }

            }
            return data_gis;
        }

        private DataTable DbfToTable(string[] f , string[] data)
        {
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
            return dt;
        }

        private string GetLineSegmentSideDescription(LineSegmentSide lineSegmentSide)
        {
            switch (lineSegmentSide)
            {
                case LineSegmentSide.LeftOfSegment:
                    return "left of road center";
                case LineSegmentSide.RightOfSegment:
                    return "right of road center";
                case LineSegmentSide.StartOfSegment:
                    return "from start of road";
                case LineSegmentSide.EndOfSegment:
                    return "after end of road";
                default:
                    return "";
            }
        }
    }
}