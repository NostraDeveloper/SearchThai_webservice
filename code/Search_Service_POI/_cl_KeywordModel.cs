using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_Service_POI
{
    public class _cl_KeywordModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="keyOut">
        /// index
        /// 0 = ชื่อหมู่
        /// 1 = จังหวัด
        /// 2 = อำเภอ
        /// 3 = ตำบล</param>
        /// <returns></returns>
        public bool moo(string keyword ,out string[] keyOut)
        {
            keyOut = new string[4];
            bool IsMoo = false;
            
            //Regex A2_gbkk = new Regex(@"\bเขต");
            //Regex A1_gbkk = new Regex(@"\bแขวง");

            Regex moo = new Regex(@"\bหมู่ที่\s\d+");
            Regex moo1 = new Regex(@"\bหมู่ที่\d+");
            Regex moo2 = new Regex(@"\bหมู่\s\d+");
            Regex moo3 = new Regex(@"\bหมู่\d+");

            Regex m = new Regex(@"\bม.\d+");
            Regex m1 = new Regex(@"\bม\d+");
            Regex m2 = new Regex(@"\bม.\s\d+");
            Regex m3 = new Regex(@"\bม\s\d+");



            Regex A3 = new Regex(@"\bตำบล\w+($|\s)");
            Regex A3_1 = new Regex(@"\bตำบล\s\w+($|\s)");
            Regex A3_2 = new Regex(@"\bต\s\w+($|\s)");
            Regex A3_3 = new Regex(@"\bต.\s\w+($|\s)");
            Regex A3_4 = new Regex(@"\bต.\w+($|\s)");

            Regex A2 = new Regex(@"\bอำเภอ\w+($|\s)");
            Regex A2_1 = new Regex(@"\bอำเภอ\s\w+($|\s)");
            Regex A2_2 = new Regex(@"\bอ\s\w+($|\s)");
            Regex A2_3 = new Regex(@"\bอ.\s\w+($|\s)");
            Regex A2_4 = new Regex(@"\bอ.\w+($|\s)");


            Regex A1 = new Regex(@"\bจังหวัด\w+($|\s)");
            Regex A1_1 = new Regex(@"\bจังหวัด\s\w+($|\s)");
            Regex A1_2 = new Regex(@"\bจ\s\w+($|\s)");
            Regex A1_3 = new Regex(@"\bจ.\s\w+($|\s)");
            Regex A1_4 = new Regex(@"\bจ.\w+($|\s)");


            if (
                   moo.IsMatch(keyword)
                || moo1.IsMatch(keyword)
                || moo2.IsMatch(keyword)
                || moo3.IsMatch(keyword)

                || m.IsMatch(keyword)
                || m1.IsMatch(keyword)
                || m2.IsMatch(keyword)
                || m3.IsMatch(keyword)

                )
            {
                if (
                    A3.IsMatch(keyword)
                    || A3_1.IsMatch(keyword)
                    || A3_2.IsMatch(keyword)
                    || A3_3.IsMatch(keyword)
                    || A3_4.IsMatch(keyword)
                    )
                {
                    IsMoo = true;
                }

                if (
                   A2.IsMatch(keyword)
                   || A2_1.IsMatch(keyword)
                   || A2_2.IsMatch(keyword)
                   || A2_3.IsMatch(keyword)
                   || A2_4.IsMatch(keyword)
                   )
                {
                    IsMoo = true;
                }

                if (
                   A1.IsMatch(keyword)
                   || A1_1.IsMatch(keyword)
                   || A1_2.IsMatch(keyword)
                   || A1_3.IsMatch(keyword)
                   || A1_4.IsMatch(keyword)
                   )
                {
                    IsMoo = true;
                }
            }

            //if (keyword.Contains("หมู่ที่") || keyword.Contains("หมู่"))
            //{
            //    if (keyword.Contains("ตำบล"))
            //        IsMoo = true;
            //    if (keyword.Contains("อำเภอ"))
            //        IsMoo = true;
            //    if (keyword.Contains("จังหวัด"))
            //        IsMoo = true;
            //}

            if (IsMoo)
            {
                if (moo.IsMatch(keyword))
                    keyOut[0] = moo.Match(keyword).Value;       
                else if (moo1.IsMatch(keyword))
                    keyOut[0] = moo1.Match(keyword).Value;       
                else if (moo2.IsMatch(keyword))
                    keyOut[0] = moo2.Match(keyword).Value;
                else if (moo3.IsMatch(keyword))
                    keyOut[0] = moo3.Match(keyword).Value;
                else if (m.IsMatch(keyword))
                    keyOut[0] = m.Match(keyword).Value;
                else if (m1.IsMatch(keyword))
                    keyOut[0] = m1.Match(keyword).Value;
                else if (m2.IsMatch(keyword))
                    keyOut[0] = m2.Match(keyword).Value;
                else if (m3.IsMatch(keyword))
                    keyOut[0] = m3.Match(keyword).Value;
                else
                    return false;

                if (A1.IsMatch(keyword))
                    keyOut[1] = A1.Match(keyword).Value;
                else if (A1_1.IsMatch(keyword))
                    keyOut[1] = A1_1.Match(keyword).Value;
                else if (A1_2.IsMatch(keyword))
                    keyOut[1] = A1_2.Match(keyword).Value;
                else if (A1_3.IsMatch(keyword))
                    keyOut[1] = A1_3.Match(keyword).Value;
                else if (A1_4.IsMatch(keyword))
                    keyOut[1] = A1_4.Match(keyword).Value;
                else
                    keyOut[1] = "";

                if (A2.IsMatch(keyword))
                    keyOut[2] = A2.Match(keyword).Value;
                else if (A2_1.IsMatch(keyword))
                    keyOut[2] = A2_1.Match(keyword).Value;
                else if (A2_2.IsMatch(keyword))
                    keyOut[2] = A2_2.Match(keyword).Value;
                else if (A2_3.IsMatch(keyword))
                    keyOut[2] = A2_3.Match(keyword).Value;
                else if (A2_4.IsMatch(keyword))
                    keyOut[2] = A2_4.Match(keyword).Value;
                else
                    keyOut[2] = "";

                if (A3.IsMatch(keyword))
                    keyOut[3] = A3.Match(keyword).Value;
                else if (A3_1.IsMatch(keyword))
                    keyOut[3] = A3_1.Match(keyword).Value;
                else if (A3_2.IsMatch(keyword))
                    keyOut[3] = A3_2.Match(keyword).Value;
                else if (A3_3.IsMatch(keyword))
                    keyOut[3] = A3_3.Match(keyword).Value;
                else if (A3_4.IsMatch(keyword))
                    keyOut[3] = A3_4.Match(keyword).Value;
                else
                    keyOut[3] = ""; 
            }

            if (IsMoo)
            {
                keyOut[0] = keyOut[0].Replace("หมู่ ", "หมู่ที่");
                keyOut[0] = keyOut[0].Replace("หมู่ที่ ", "หมู่ที่");
                keyOut[0] = keyOut[0].Replace("ม.", "หมู่ที่");
                keyOut[0] = keyOut[0].Replace("ม ", "หมู่ที่");

                if (!string.IsNullOrEmpty(keyOut[1]))
                {
                    keyOut[1] = keyOut[1].Replace("จังหวัด", "");
                    keyOut[1] = keyOut[1].Replace("จ.", "");

                    if (keyOut[1].Substring(0, 2) == "จ ")
                        keyOut[1] = keyOut[1].Substring(2, keyOut[1].Length-2);
                }
                if (!string.IsNullOrEmpty(keyOut[2]))
                {
                    keyOut[2] = keyOut[2].Replace("อำเภอ", "");
                    keyOut[2] = keyOut[2].Replace("อ.", "");

                    if (keyOut[2].Substring(0, 2) == "อ ")
                        keyOut[2] = keyOut[2].Substring(2, keyOut[2].Length - 2);
                }
                if (!string.IsNullOrEmpty(keyOut[3]))
                {
                    keyOut[3] = keyOut[3].Replace("ตำบล", "");
                    keyOut[3] = keyOut[3].Replace("ต.", "");

                    if (keyOut[3].Substring(0, 2) == "ต ")
                        keyOut[3] = keyOut[3].Substring(2, keyOut[3].Length - 2);
                }
            }
            return IsMoo;
        }

        public bool Km (string keyword)
        {
            bool Iskm = false;

            string k = keyword.Replace(".", "");
            k = keyword.Replace("หมายเลข", "");
            
            Regex ทล = new Regex(@"\bทล\d+");
            Regex ทล_ = new Regex(@"\bทล\s\d+");
            Regex ทลdot_ = new Regex(@"\bทล.\s\d+");
            Regex ทลdot = new Regex(@"\bทล.\d+");

            Regex กม = new Regex(@"\bกม\d+");
            Regex กม_ = new Regex(@"\bกม\s\d+");
            Regex กมdot_ = new Regex(@"\bกม.\s\d+");
            Regex กมdot = new Regex(@"\bกม.\d+");

            Regex กิโล = new Regex(@"\bกิโล\d+");
            Regex กิโลฯ = new Regex(@"\bกิโลฯ\d+");
            Regex กิโลเมตร = new Regex(@"\bกิโลเมตร\d+");
            Regex กิโลเมตรที่ = new Regex(@"\bกิโลเมตรที่\d+");

            Regex กิโล_ = new Regex(@"\bกิโล\s\d+");
            Regex กิโลฯ_ = new Regex(@"\bกิโลฯ\s\d+");
            Regex กิโลเมตร_ = new Regex(@"\bกิโลเมตร\s\d+");
            Regex กิโลเมตรที่_ = new Regex(@"\bกิโลเมตรที่\s\d+");

            Regex หลักกิโลเมตร = new Regex(@"\bหลักกิโลเมตร\d+");
            Regex หลักกิโลเมตรที่ = new Regex(@"\bหลักกิโลเมตรที่\d+");
            Regex หลักกิโลเมตร_ = new Regex(@"\bหลักกิโลเมตร\s\d+");
            Regex หลักกิโลเมตรที่_ = new Regex(@"\bหลักกิโลเมตรที่\s\d+");


            if (ทล.IsMatch(k) || ทล_.IsMatch(k) || ทลdot_.IsMatch(k) || ทลdot.IsMatch(k))
                if (กม.IsMatch(k) || กม_.IsMatch(k) || กมdot_.IsMatch(k) || กมdot.IsMatch(k)
                    || กิโล.IsMatch(k) || กิโลฯ.IsMatch(k) || กิโลเมตร.IsMatch(k) || กิโลเมตรที่.IsMatch(k)
                    || กิโล_.IsMatch(k) || กิโลฯ_.IsMatch(k) || กิโลเมตร_.IsMatch(k) || กิโลเมตรที่_.IsMatch(k)
                    || หลักกิโลเมตรที่_.IsMatch(k) || หลักกิโลเมตรที่.IsMatch(k)||หลักกิโลเมตร_.IsMatch(k)||หลักกิโลเมตร.IsMatch(k))
                    Iskm = true;
            

            return Iskm;
        }

        public bool BTS(string keyword , out string kout)
        {
            bool Iskm = false;
            kout = keyword;

            Regex bts = new Regex(@"\bbts\s");
            Regex BTS = new Regex(@"\bBTS\s");
            Regex Bts = new Regex(@"\bBts\s");

            if (bts.IsMatch(keyword))
            {
                kout = keyword.Replace("bts ", "บีทีเอส");
                Iskm = true;
            }
            else if (BTS.IsMatch(keyword))
            {
                kout = keyword.Replace("BTS ", "บีทีเอส");
                Iskm = true;
            }
            else if (Bts.IsMatch(keyword))
            {
                kout = keyword.Replace("Bts ", "บีทีเอส");
                Iskm = true;
            }


            return Iskm;
        }

        public bool admin(string keyword)
        {

            bool IsAdmin = false;

            keyword = keyword.ToLower();

            Regex check_thai = new Regex(@"[ก-ฮฯ-ูเ-ํ๑-๙]+");
            Regex check_english = new Regex(@"[A-Za-z&%]+");

            if(check_thai.IsMatch(keyword) && check_english.IsMatch(keyword))
                return false;

            if(check_thai.IsMatch(keyword))
            {
                if(keyword.Contains(' '))
                {
                    return false;
                }
            }


            Regex PRO = new Regex(@"^จังหวัด");
            Regex AMP = new Regex(@"^อำเภอ");
            Regex AMP2 = new Regex(@"^เขต");

            Regex TAM = new Regex(@"^ตำบล");
            Regex TAM2 = new Regex(@"^แขวง");

            Regex e_PRO = new Regex(@"province$");
            Regex e_AMP = new Regex(@"district$");
            Regex e_TAM = new Regex(@"subdistrict$");
            


            if (PRO.IsMatch(keyword) || AMP.IsMatch(keyword) || AMP2.IsMatch(keyword) 
                || TAM.IsMatch(keyword) || TAM2.IsMatch(keyword)
                || e_PRO.IsMatch(keyword) || e_AMP.IsMatch(keyword) || e_TAM.IsMatch(keyword)
                )
            {
                return true;
            }


            return IsAdmin;
        }

        public bool intersection(string keyword)
        {

            bool IsInter = false;

            keyword = keyword.ToLower();

            Regex check_thai = new Regex(@"[ก-ฮฯ-ูเ-ํ๑-๙]+");
            Regex check_english = new Regex(@"[A-Za-z&%]+");

            if (check_thai.IsMatch(keyword) && check_english.IsMatch(keyword))
                return false;

            Regex th_ok = new Regex(@"^แยก\w+");
            Regex th_not = new Regex(@"^แยก\s");


            if (th_ok.IsMatch(keyword) && !th_not.IsMatch(keyword)
                )
            {
                return true;
            }


            return IsInter;
        }
    }
}