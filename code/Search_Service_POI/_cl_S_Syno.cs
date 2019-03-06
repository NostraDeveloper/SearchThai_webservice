using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using wordReplace;
namespace Search_Service_POI
{
    public class _cl_S_Syno
    {
        private DataTable dt_dup = new DataTable();
        //private DataTable dt_short = new DataTable();

        _cl_wordRemove cl_wordR = new _cl_wordRemove();
        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("th-TH");
        public _cl_S_Syno(string[] FileString)
        {
            init_dt(FileString);
        }

        private void init_dt(string[] data)
        {
            dt_dup.Columns.Add("ori", typeof(string));
            dt_dup.Columns.Add("syn", typeof(string));

            for (int i = 0; i < data.Length; i++)
            {
                if (string.IsNullOrEmpty(data[i]))
                    continue;
                string[] tmp = data[i].Split('|');
                dt_dup.Rows.Add(tmp[0], tmp[1]);
            }
        }

        public string Dup(List<string> input)
        {
            List<string> SynoList = syno(input);

            string result = "";

            if (SynoList != null)
            {
                SynoList = SynoListDelDup(SynoList);
                for (int i = 0; i < SynoList.Count; i++)
                {
                    result += cl_wordR.AllAction(SynoList[i].Trim()).ToUpper() + "|";
                }
            }

            if (string.IsNullOrEmpty(result.Trim()))
                return "";
            else
                return result.Substring(0, result.Length - 1);
        }

        /*
        public List<string> syno(string input)
        {
            try
            {
                List<string> tmp = new List<string>();
                for (int i = 0; i < dt_dup.Rows.Count; i++)
                {

                    if (input.ToUpper().Contains(dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim()))
                    {
                        string[] arr = dt_dup.Rows[i]["syn"].ToString().Trim().Split(',');
                        for (int j = 0; j < arr.Length; j++)
                        {
                            string res = input;
                            res = res.ToUpper().Replace(dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim(), arr[j].ToUpper().Trim());
                            tmp.Add(res);
                        }
                    }
                }
                return tmp;
            }
            catch
            {
                return null;
            }
        }
        */
        public List<string> syno(List<string> input)
        {
            try
            {
                for (int i = 0; i < dt_dup.Rows.Count; i++)
                {
                    int z = input.Count;
                    for (int y = 0; y < z; y++)
                    {
                        if (dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim()[0] == '+'
                            ||
                            dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim()[0] == '_'
                            )
                        {
                            string tmp = dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim();
                            if (input[y].ToUpper().Trim().Contains(tmp.Substring(1, tmp.Length - 1)))
                            {
                                string[] arr = dt_dup.Rows[i]["syn"].ToString().Trim().Split(',');
                                for (int j = 0; j < arr.Length; j++)
                                {
                                    string res = input[y];

                                    if (tmp[0] == '+')
                                    {
                                        res = PlusTo(tmp, arr[j], res);
                                        if (!string.IsNullOrEmpty(res))
                                        {
                                            input.Add(res.Trim());
                                        }
                                    }
                                    else if (tmp[0] == '_')
                                    {
                                        res = UnderTo(tmp, arr[j], res);
                                        if (!string.IsNullOrEmpty(res))
                                        {
                                            input.Add(res.Trim());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (CheckSynoIsEnglish(dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim()))
                            {
                                string tmp_engInput = "";
                                tmp_engInput = " " + input[y].ToUpper().Trim() + " ";

                                string tmp_syno = "";
                                tmp_syno = " " + dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim() + " ";
                                if (tmp_engInput.Contains(tmp_syno))
                                {
                                    string[] arr = dt_dup.Rows[i]["syn"].ToString().Trim().Split(',');
                                    for (int j = 0; j < arr.Length; j++)
                                    {
                                        string res = tmp_engInput;
                                        string tmp_replace = "";
                                        tmp_replace = " " + arr[j].ToUpper().Trim() + " ";
                                        res = res.ToUpper().Replace(tmp_syno, tmp_replace);
                                        if (!input.Contains(res.Trim()))
                                            input.Add(res.Trim());
                                    }
                                }


                            }
                            else if (input[y].ToUpper().Trim().Contains(dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim()))
                            {
                                string[] arr = dt_dup.Rows[i]["syn"].ToString().Trim().Split(',');
                                for (int j = 0; j < arr.Length; j++)
                                {
                                    string res = input[y];
                                    res = res.ToUpper().Replace(dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim(), arr[j].ToUpper().Trim());
                                    if (!input.Contains(res.Trim()))
                                        input.Add(res.Trim());
                                }
                            }
                        }

                    }
                }

                return input;
            }
            catch { return null; }


            /*
            try
            {
                List<string> tmp = new List<string>();
                for (int i = 0; i < dt_dup.Rows.Count; i++)
                {

                    if (input.ToUpper().Contains(dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim()))
                    {
                        string[] arr = dt_dup.Rows[i]["syn"].ToString().Trim().Split(',');
                        for (int j = 0; j < arr.Length; j++)
                        {
                            string res = input;
                            res = res.ToUpper().Replace(dt_dup.Rows[i]["ori"].ToString().ToUpper().Trim(), arr[j].ToUpper().Trim());
                            tmp.Add(res);
                        }
                    }
                }
                return tmp;
            }
            catch
            {
                return null;
            }*/
        }
        private string RemoveChar(string input)
        {
            input = input.Replace(".", "");
            input = input.Replace("-", "");
            input = input.Replace("ฯ", "");

            input = input.Replace("๐", "0");
            input = input.Replace("๑", "1");
            input = input.Replace("๒", "2");
            input = input.Replace("๓", "3");
            input = input.Replace("๔", "4");
            input = input.Replace("๕", "5");
            input = input.Replace("๖", "6");
            input = input.Replace("๗", "7");
            input = input.Replace("๘", "8");
            input = input.Replace("๙", "9");
            return input;
        }
        private bool CheckShortWord(string shortWord, string mainWord, ref string pa)
        {
            try
            {
                pa = @"(^|\s)" + shortWord + @"($|\s)";
                Regex s = new Regex(pa);
                Match match = s.Match(mainWord);
                return match.Success;
            }
            catch
            {
                return false;
            }
        }

        private bool CheckDottWord(string shortWord, string mainWord, ref string pa)
        {
            try
            {
                pa = @"(^|\s)" + shortWord + @"($|\s|[.])";
                Regex s = new Regex(pa);
                Match match = s.Match(mainWord);
                return match.Success;
            }
            catch
            {
                return false;
            }
        }

        public string RemoveMoreSpace(string input)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            input = regex.Replace(input, " ");

            return input;
        }

        private string PlusTo(string ori, string syno, string data)
        {
            try
            {
                string tmp0 = data.Replace(" ", "");
                tmp0.Replace(ori.Substring(1, ori.Length - 1), "|");
                if (tmp0.Contains("||"))
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }


            try
            {
                string[] tmpData = RemoveMoreSpace(data).Trim().ToUpper().Split(' ');
                string compare = ori.Substring(1, ori.Length - 1);
                for (int i = 0; i < tmpData.Length; i++)
                {
                    //check

                    string tmp_arr = tmpData[i].Replace(compare, "|").Trim();
                    if (tmp_arr.Contains("|"))
                    {
                        //if (tmp_arr.Count(f => f == '|') > 1) //เจอมากกว่า 1 คำติดกันไม่ต้องทำ
                        if (tmp_arr.Contains("||"))
                        {
                            return "";
                        }
                        else
                        {
                            //ตรวจสอบว่าเป็นคำแรก หรือไม่
                            if (tmp_arr[0] == '|')
                            {
                                //case ถ้า + -> -
                                if (syno[0] == '_')
                                {
                                    //เช่น 
                                    //ตึก|ทาวเวอร์
                                    //ตึก เอ = เอทาวเวอร์
                                    if (tmp_arr.Length == 1)
                                    {
                                        tmpData[i] = "";
                                        tmpData[i + 1] = tmpData[i + 1] + " " + syno.Substring(1, syno.Length - 1);

                                        string output = "";
                                        for (int j = 0; j < tmpData.Length; j++)
                                        {
                                            output += tmpData[j] + " ";
                                        }
                                        return output.Trim();
                                    }
                                    else
                                    {
                                        tmpData[i] = tmp_arr.Substring(1, tmp_arr.Length - 1) + syno.Substring(1, syno.Length - 1);

                                        string output = "";
                                        for (int j = 0; j < tmpData.Length; j++)
                                        {
                                            output += tmpData[j] + " ";
                                        }
                                        return output.Trim();
                                    }
                                }
                                else if (syno[0] == '+')
                                {
                                    tmpData[i] = tmp_arr.Replace("|", syno.Substring(1, syno.Length - 1));

                                    string output = "";
                                    for (int j = 0; j < tmpData.Length; j++)
                                    {
                                        output += tmpData[j] + " ";
                                    }
                                    return output.Trim();
                                }
                                else
                                {
                                    tmpData[i] = tmp_arr.Replace("|", syno.Trim());

                                    string output = "";
                                    for (int j = 0; j < tmpData.Length; j++)
                                    {
                                        output += tmpData[j] + " ";
                                    }
                                    return output.Trim();

                                }
                            }
                        }
                    }

                }
                return "";

            }
            catch
            {
                return "";
            }


        }

        private string UnderTo(string ori, string syno, string data)
        {
            try
            {
                string tmp0 = data.Replace(" ", "");
                tmp0.Replace(ori.Substring(1, ori.Length - 1), "|");
                if (tmp0.Contains("||"))
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }

            try
            {
                string[] tmpData = RemoveMoreSpace(data).Trim().ToUpper().Split(' ');
                string compare = ori.Substring(1, ori.Length - 1);
                for (int i = tmpData.Length - 1; i >= 0; i--)
                {
                    //check

                    string tmp_arr = tmpData[i].Replace(compare, "|").Trim();
                    if (tmp_arr.Contains("|"))
                    {
                        //if (tmp_arr.Count(f => f == '|') > 1) //เจอมากกว่า 1 คำติดกันไม่ต้องทำ
                        if (tmp_arr.Contains("||"))
                        {
                            return "";
                        }
                        else
                        {
                            //ตรวจสอบว่าเป็นคำสุดท้าย หรือไม่
                            if (tmp_arr[tmp_arr.Length - 1] == '|')
                            {
                                //case ถ้า - -> +
                                if (syno[0] == '+')
                                {
                                    //เช่น 
                                    //ทาวเวอร์ ->
                                    //ตึก เอ = เอทาวเวอร์
                                    if (tmp_arr.Length == 1)
                                    {
                                        tmpData[i] = "";
                                        tmpData[i - 1] = syno.Substring(1, syno.Length - 1) + tmpData[i - 1];

                                        string output = "";
                                        for (int j = 0; j < tmpData.Length; j++)
                                        {
                                            output += tmpData[j] + " ";
                                        }
                                        return output.Trim();
                                    }
                                    else
                                    {
                                        tmpData[i] = syno.Substring(1, syno.Length - 1) + tmp_arr.Substring(0, tmp_arr.Length - 1);

                                        string output = "";
                                        for (int j = 0; j < tmpData.Length; j++)
                                        {
                                            output += tmpData[j] + " ";
                                        }
                                        return output.Trim();
                                    }
                                }
                                else if (syno[0] == '_')
                                {
                                    tmpData[i] = tmp_arr.Replace("|", syno.Substring(1, syno.Length - 1));

                                    string output = "";
                                    for (int j = 0; j < tmpData.Length; j++)
                                    {
                                        output += tmpData[j] + " ";
                                    }
                                    return output.Trim();
                                }
                                else
                                {
                                    tmpData[i] = tmp_arr.Replace("|", syno.Trim());

                                    string output = "";
                                    for (int j = 0; j < tmpData.Length; j++)
                                    {
                                        output += tmpData[j] + " ";
                                    }
                                    return output.Trim();

                                }
                            }
                        }
                    }

                }
                return "";

            }
            catch
            {
                return "";
            }


        }

        private string NoneTo(string ori, string syno, string data)
        {
            try
            {
                string tmp0 = data.Replace(" ", "");
                tmp0.Replace(ori, "|");
                if (tmp0.Contains("||"))
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }

            try
            {
                string[] tmpData = RemoveMoreSpace(data).Trim().ToUpper().Split(' ');
                string compare = ori.Substring(1, ori.Length - 1);
                for (int i = tmpData.Length - 1; i >= 0; i--)
                {
                    //check

                    string tmp_arr = tmpData[i].Replace("compare", "|").Trim();
                    if (tmp_arr.Contains("|"))
                    {
                        //if (tmp_arr.Count(f => f == '|') > 1) //เจอมากกว่า 1 คำติดกันไม่ต้องทำ
                        if (tmp_arr.Contains("||"))
                        {
                            return "";
                        }
                        else
                        {
                            //ตรวจสอบว่าเป็นคำสุดท้าย หรือไม่
                            if (tmp_arr[tmp_arr.Length - 1] == '|')
                            {
                                //case ถ้า - -> +
                                if (syno[0] == '+')
                                {
                                    //เช่น 
                                    //ทาวเวอร์ ->
                                    //ตึก เอ = เอทาวเวอร์
                                    if (tmp_arr.Length == 1)
                                    {
                                        tmpData[i] = "";
                                        tmpData[i - 1] = syno.Substring(1, syno.Length - 1) + tmpData[i - 1];

                                        string output = "";
                                        for (int j = 0; j < tmpData.Length; j++)
                                        {
                                            output += tmpData[j] + " ";
                                        }
                                        return output.Trim();
                                    }
                                    else
                                    {
                                        tmpData[i] = syno.Substring(1, syno.Length - 1) + tmp_arr.Substring(1, tmp_arr.Length - 1);

                                        string output = "";
                                        for (int j = 0; j < tmpData.Length; j++)
                                        {
                                            output += tmpData[j] + " ";
                                        }
                                        return output.Trim();
                                    }
                                }
                                else if (syno[0] == '_')
                                {
                                    tmpData[i] = tmp_arr.Replace("|", syno.Substring(1, syno.Length - 1));

                                    string output = "";
                                    for (int j = 0; j < tmpData.Length; j++)
                                    {
                                        output += tmpData[j] + " ";
                                    }
                                    return output.Trim();
                                }
                                else
                                {
                                    tmpData[i] = tmp_arr.Replace("|", syno.Trim());

                                    string output = "";
                                    for (int j = 0; j < tmpData.Length; j++)
                                    {
                                        output += tmpData[j] + " ";
                                    }
                                    return output.Trim();

                                }
                            }
                        }
                    }

                }
                return "";

            }
            catch
            {
                return "";
            }


        }

        public List<string> SynoListDelDup(List<string> input)
        {
            try
            {
                List<string> output = new List<string>();
                for (int i = 0; i < input.Count; i++)
                {
                    if (!output.Contains(input[i].Trim().ToUpper()))
                    {
                        output.Add(input[i].Trim().ToUpper());
                    }
                }

                return output;
            }
            catch
            {
                return null;
            }
        }

        public bool WordAdd_space(string input, ref string output)
        {
            string key = "SIAM";
            string tmp_input = RemoveMoreSpace(input.Trim()).ToUpper();
            string tmp_output = " " + tmp_input;
            tmp_output = tmp_output.Replace(" " + key, " " + key + " ").Trim();
            tmp_output = RemoveMoreSpace(tmp_output);

            if (tmp_output == tmp_input)
                return false;
            else
            {
                output = tmp_output;
                return true;
            }
        }

        public bool WordDEL_space(string input, ref string output)
        {
            string key = "SIAM";
            string tmp_input = RemoveMoreSpace(input.Trim()).ToUpper();
            string tmp_output = " " + tmp_input;
            tmp_output = tmp_output.Replace(" " + key + " ", " " + key).Trim();
            tmp_output = RemoveMoreSpace(tmp_output);

            if (tmp_output == tmp_input)
                return false;
            else
            {
                output = tmp_output;
                return true;
            }
        }

        public bool CheckSynoIsEnglish(string input)
        {
            Regex check_thai = new Regex(@"[ก-ฮฯ-ูเ-ํ๑-๙]+");
            Regex check_english = new Regex(@"[A-Za-z]+");

            if (!check_thai.IsMatch(input) && check_english.IsMatch(input))
                return true;
            else
                return false;
        }


    }
}