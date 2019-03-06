using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace webservice_search_a_route
{
    public class _prop_Closest
    {
         /// <summary>
        /// 
        /// </summary>
        public _prop_Closest()
        {
            lat = new List<double>();
            lon = new List<double>();
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

        public string routeID { get; set; }

    }
}