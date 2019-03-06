using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webservice_search_a_route
{
    /// <summary>
    /// Parameter Set For Nearby Option
    /// </summary>
    public class _prop_Nearby
    {
        /// <summary>
        /// 
        /// </summary>
        public _prop_Nearby()
        {
            lat = new List<double>();
            lon = new List<double>();
            Polygon = new List<string>();
            flag = new List<int>();
        }
        /// <summary>
        /// latitude All Node
        /// </summary>
        public List<double> lat { get; set; }
        /// <summary>
        /// Lontitude All Node
        /// </summary>
        public List<double> lon { get; set; }

        public List<int> flag { get; set; }
        /// <summary>
        /// polygon in format lat1,lon1|...
        /// </summary>
        public List<string> Polygon { get; set; }
        public string keyword { get; set; }
        public string category { get; set; }
        public string LocalCatCode { get; set; }
        public string maxReturn { get; set; }
        public string syno { get; set; }
        public string syno_wb { get; set; }

        public string thai_soundex { get; set; }

        /// <summary>
        ///  | is delimiter ระหว่างคำ (แทน space input)
        /// </summary>
        public string thai_soundex_delimeter { get; set; }

        /// <summary>
        /// km.
        /// </summary>
        public double buffer { get; set; }
    }
}