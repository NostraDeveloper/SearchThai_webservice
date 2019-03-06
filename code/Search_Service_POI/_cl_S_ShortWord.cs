using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

namespace Search_Service_POI
{
    public class _cl_S_ShortWord
    {
        private DataTable dt_short = new DataTable();
        
        System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("th-TH");
        public _cl_S_ShortWord(string[] data)
        {
            init_dt(data);
        }
        private void init_dt(string[] dataS)
        {
            dt_short.Columns.Add("ori", typeof(string));
            dt_short.Columns.Add("syn", typeof(string));

            for (int i = 0; i < dataS.Length; i++)
            {
                if (string.IsNullOrEmpty(dataS[i]))
                    continue;
                string[] tmp = dataS[i].Split('|');
                dt_short.Rows.Add(tmp[0], tmp[1]);
            }
        }

        public bool CheckWord(string keyword , ref List<string> output)
        {
            bool flag = false;
            string tmp_keyword = spaceLastDot(keyword);
            for(int i=0;i<dt_short.Rows.Count ;i++)
            {
                string tmp = dt_short.Rows[i]["ori"].ToString().Trim();

                //if(tmp.Substring(tmp.Length-2,1) == ".")
                if(tmp[tmp.Length-1] == '.')
                {
                    checkDot(tmp_keyword, spaceLastDot(dt_short.Rows[i]["ori"].ToString().Trim()), dt_short.Rows[i]["syn"].ToString().Trim(),ref output);
                    flag = true;
                }
                else
                {
                    checkShort(keyword, dt_short.Rows[i]["ori"].ToString().Trim(), dt_short.Rows[i]["syn"].ToString().Trim(),ref output);
                    flag = true;
                }
            }

            return flag;
        }

        public string CheckWord(string keyword)
        {
            string output = "";
            string tmp_keyword = spaceLastDot(keyword);
            output = tmp_keyword;
            for (int i = 0; i < dt_short.Rows.Count; i++)
            {
                string tmp = dt_short.Rows[i]["ori"].ToString().Trim();

                if (tmp == "มร.นม.")
                {
                    int o = 0;
                }
                //if(tmp.Substring(tmp.Length-2,1) == ".")
                if (tmp[tmp.Length - 1] == '.')
                {
                    tmp_keyword = checkDot_big(tmp_keyword, spaceLastDot(dt_short.Rows[i]["ori"].ToString().Trim()), dt_short.Rows[i]["syn"].ToString().Trim());
                }
                else
                {
                    tmp_keyword = checkShort_big(tmp_keyword, dt_short.Rows[i]["ori"].ToString().Trim(), dt_short.Rows[i]["syn"].ToString().Trim());
                }
            }

            if (tmp_keyword.Trim() == output.Trim())
                return "";
            else
                return tmp_keyword.Trim();
        }

        public void checkDot(string input, string table, string fullWord,ref List<string> output)
        {
            //if(SkipCheckDot(table,input))
            //{
            //    return;
            //}
            //else 
            //{
            //    string tmpRegex = "";
            //    if (CheckDotWord(table, input.Trim().ToUpper(), ref tmpRegex))
            //    {
            //        Regex tmpR = new Regex(tmpRegex);

            //        string res = tmpR.Replace(input.Trim().ToUpper(),fullWord);
            //        output.Add(res);
            //    }

            //}

            string tmpRegex = "";
            if (CheckShortWord(table.Replace(".","\\."), input.Trim().ToUpper(), ref tmpRegex))
            {
                Regex tmpR = new Regex(tmpRegex);

                string res = tmpR.Replace(input.Trim().ToUpper(), " " + fullWord);
                output.Add(res.Trim());
            }
        }

        private bool SkipCheckDot(string shortWord, string mainWord)
        {
            try
            {
                string pa = @"(^|\s)" + shortWord + @"([ก-ฮ]{2}[.]|[a-z]{2}[.]|[A-Z]{2}[.]|[ก-ฮ][.]|[a-z][.]|[A-Z][.])";
                Regex s = new Regex(pa);
                Match match = s.Match(mainWord);
                return match.Success;
            }
            catch
            {
                return false;
            }
        }
        private bool CheckDotWord(string shortWord, string mainWord, ref string pa)
        {
            try
            {
                pa = @"(^|\s)" + shortWord;
                Regex s = new Regex(pa);
                Match match = s.Match(mainWord);
                return match.Success;
            }
            catch
            {
                return false;
            }
        }
        private string spaceLastDot(string input)
        {
            try
            {
                string[] tmp_spaceInput = input.Split(' ');
                string output = "";
                string tmp = "";
                for (int z = 0; z < tmp_spaceInput.Length; z++)
                {
                    int index = tmp_spaceInput[z].Length - 1;
                    if (tmp_spaceInput[z][index] == '.')
                    {
                        output += tmp_spaceInput[z] + " ";
                        continue;
                    }
                    else if (!tmp_spaceInput[z].Contains("."))
                    {
                        output += tmp_spaceInput[z] + " ";
                        continue;
                    }
                    else 
                    {
                        for(int i = tmp_spaceInput[z].Length-1;i>=0;i-- )
                        {
                            if(tmp_spaceInput[z][i] =='.')
                            {
                                output += tmp_spaceInput[z].Substring(0, i + 1);
                                output += " ";
                                output += tmp_spaceInput[z].Substring(i + 1, tmp_spaceInput[z].Length - i + 1 -2);
                                output += " ";
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(tmp))
                { output += tmp; }
                return output.Trim();
            }
            catch
            {
                return input;
            }
        }

        private string spaceLastDot_old(string input)
        {
            try
            {
                string output = "";
                string tmp = "";
                int index = input.Length - 1;
                for (int i = 0; i < input.Length; i++)
                {
                    tmp += input[i];
                    if (input[i] == '.')
                    {
                        if (i + 4 < index)
                        {
                            if (input[i + 4] != '.' && input[i + 3] != '.' && input[i + 2] != '.')
                            {
                                output += tmp + " ";
                                tmp = "";
                            }
                        }
                        else if (i + 3 < index)
                        {
                            if (input[i + 3] != '.' && input[i + 2] != '.')
                            {
                                output += tmp + " ";
                                tmp = "";
                            }
                        }
                        else if (i + 2 < index)
                        {
                            if (input[i + 2] != '.')
                            {
                                output += tmp + " ";
                                tmp = "";
                            }
                        }
                        else
                        {
                            output += tmp;
                            tmp = "";
                        }

                    }
                }

                if (!string.IsNullOrEmpty(tmp))
                { output += tmp; }
                return output;
            }
            catch
            {
                return input;
            }
        }
        private void checkShort(string input , string table , string fullWord,ref List<string> output)
        {
            string tmpRegex = "";
            if (CheckShortWord(table, input.Trim().ToUpper(), ref tmpRegex))
            {
                Regex tmpR = new Regex(tmpRegex);

                string res = tmpR.Replace(input.Trim().ToUpper(), " "+ fullWord +" ");
                output.Add(res.Trim());
            }
        }

        private bool CheckShortWord(string shortWord, string mainWord, ref string pa)
        {
            try
            {
                pa = @"(^|\s)" + shortWord.Trim() + @"($|\s)";
                Regex s = new Regex(pa);
                Match match = s.Match(mainWord.Trim());
                return match.Success;
            }
            catch
            {
                return false;
            }
        }



        //short to big all
        public string checkDot_big(string input, string table, string fullWord)
        {

            string tmpRegex = "";
            if (CheckShortWord(table.Replace(".", "\\."), input.Trim().ToUpper(), ref tmpRegex))
            {
                Regex tmpR = new Regex(tmpRegex);

                string res = tmpR.Replace(input.Trim().ToUpper(), " " + fullWord + " " );
                return res.Trim();
            }
            else
            {
                return input;
            }
        }

        private string checkShort_big(string input, string table, string fullWord)
        {
            string tmpRegex = "";
            if (CheckShortWord(table, input.Trim().ToUpper(), ref tmpRegex))
            {
                Regex tmpR = new Regex(tmpRegex);

                string res = tmpR.Replace(input.Trim().ToUpper(), " " + fullWord + " ");
                 return (res.Trim());
            }
            else
            {
                return input;
            }
        }

    }
}