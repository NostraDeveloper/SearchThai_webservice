using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace wordReplace
{
    class _cl_wordRemove
    {
        public string DotAction(string input)
        {
            try
            {
                string tmp = input;

                tmp = addDot(tmp);
                tmp = spaceDot(tmp);
                return tmp;
            }
            catch
            {
                return input;
            }
        }

        public string AllAction(string input)
        {
            string output = input;
            if (string.IsNullOrEmpty(output))
                return "";

            output = DotAction(output);
            output = ReplaceThaiNumber(output);
            output = RemoveSPchar(output);


            return output;
        }

        private string addDot(string input)
        {
            try
            {
                string[] tmp = input.Split(' ');

                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = Regex.Replace(tmp[i], @"[.]([ก-ฮ]|[a-z]|[A-Z])$", m => string.Format(@"{0}.", m.Value));
                }
                string output = "";
                for (int i = 0; i < tmp.Length; i++)
                {
                    output += tmp[i] + " ";
                }

                return output;
            }
            catch
            {
                return input;
            }
        }
        private string spaceDot(string input)
        {
            try {

                //Regex dotP = new Regex(@"([ก-ฮ]|[a-z]|[A-Z])");
                string output = "";
                string[] tmp0 = input.Split(' ');
                for (int a = 0; a < tmp0.Length; a++)
                {
                    string[] tmp = tmp0[a].Split('.');

                    for (int i = 0; i < tmp.Length; i++)
                    {
                        if (tmp[i].Length == 1)
                        {
                            output += tmp[i];
                        }
                        else
                        {
                            output += " " + tmp[i];
                        }
                    }
                    output += " ";
                }
                return output;
            }
            catch{
                return input;
            }
        }

        public string ReplaceThaiNumber(string input)
        {
            string output = input;
            output = output.Replace("๑", "1");
            output = output.Replace("๒", "2");
            output = output.Replace("๓", "3");
            output = output.Replace("๔", "4");
            output = output.Replace("๕", "5");
            output = output.Replace("๖", "6");
            output = output.Replace("๗", "7");
            output = output.Replace("๘", "8");
            output = output.Replace("๙", "9");
            output = output.Replace("๐", "0");
            return output;
        }

        public string RemoveSPchar(string input)
        {
            string output = input;
            output = output.Replace("-", " ");
            output = output.Replace("'", " ");
            output = output.Replace("_", " ");
            output = output.Replace(",", " ");
            output = output.Replace("ฯ", " ");
            return output;
        }

        public string RemoveMoreSpace(string input)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);
            input = regex.Replace(input, " ");

            return input;
        }
    }
}
