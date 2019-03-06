using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Service_POI
{
    public class _prop_SearchInput
    {
        public string keyword { get; set; }
        public string AdminLevel3 { get; set; }
        public string AdminLevel2 { get; set; }
        public string AdminLevel1 { get; set; }
        public string PostCode { get; set; }
        public string AdminLevel4 { get; set; }
        public string houseNumber { get; set; }
        public string telephone { get; set; }
        public string category { get; set; }
        public string LocalCatCode { get; set; }
        public string tag { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string radius { get; set; }
        public string RowsPerPage { get; set; }
        public string PageNumber { get; set; }
        public string token { get; set; }

        public string maxReturn { get; set; }

        public string syno { get; set; }

        public string syno_wb { get; set; }

        public string thai_soundex { get; set; }

        /// <summary>
        ///  | is delimiter ระหว่างคำ (แทน space input)
        /// </summary>
        public string thai_soundex_delimeter { get; set; }

        /// <summary>
        /// 0 = default
        /// 1 = thai soundex
        /// </summary>
        public int mode { get; set; }

        public _prop_SearchInput()
        {
            mode = 0;
        }
    }
}