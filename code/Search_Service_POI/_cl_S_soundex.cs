using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using wordReplace;
using System.Threading;

namespace Search_Service_POI
{
    public class _cl_S_soundex
    {
        int WAIT_TRY = System.Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["waitTry"].ToString());
        int TRY_AGAIN = System.Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["tryAgain"].ToString());

        public bool Convert_(string input, out string withDeli, out string output, out string wordB)
        {
            _cl_S_WordBreak wb = new _cl_S_WordBreak();
            _cl_wordRemove wr = new _cl_wordRemove();
            ServiceReference_soundex.SoundexSoapClient sound = new ServiceReference_soundex.SoundexSoapClient("SoundexSoap");
            withDeli = "";
            output = "";
            wordB = "";
            try
            {
                string tmp = wr.AllAction(input);
                tmp = wr.RemoveMoreSpace(tmp.Trim());
                tmp = tmp.Replace(" ", "|");

                withDeli = "";
                output = "";

                string tmp_wb = wb.wb_action(tmp);
                tmp_wb = wr.RemoveMoreSpace(tmp_wb).Trim();

                string[] tmpWb_arr = tmp_wb.Split('|');

                // wb to sound
                for (int i = 0; i < tmpWb_arr.Length; i++)
                {
                    string tmp_sound = sound.Thai_soundex(tmpWb_arr[i]).Replace("^", " ");

                    output += tmp_sound + " ";
                    withDeli += tmp_sound + "|";
                }

                withDeli = withDeli.Substring(0, withDeli.Length - 1);

                output = wr.RemoveMoreSpace(output).Trim();
                withDeli = wr.RemoveMoreSpace(output).Trim();
                wordB = wr.RemoveMoreSpace(tmp_wb.Replace("|", " ")).Trim();

                return true;

            }
            catch
            {
                return false;
            }
        }

        public bool Convert(string input, out string withDeli, out string output, out string wordB)
        {
            int tryAgain = TRY_AGAIN;
            _cl_S_WordBreak wb = new _cl_S_WordBreak();
            _cl_wordRemove wr = new _cl_wordRemove();
            ServiceReference_soundex.SoundexSoapClient sound = new ServiceReference_soundex.SoundexSoapClient("SoundexSoap");
            withDeli = "";
            output = "";
            wordB = "";

            if (string.IsNullOrEmpty(wr.RemoveMoreSpace(wr.AllAction(input)).Trim()))
                return true;

            //while (tryAgain >= 0)
            {
                withDeli = "";
                output = "";
                wordB = "";
                try
                {
                    string[] input_arr = input.Split('|');
                    List<string> list_output = new List<string>();
                    List<string> list_withDeli = new List<string>();
                    List<string> list_wordB = new List<string>();

                    for (int z = 0; z < input_arr.Length; z++)
                    {
                        string tmp = wr.AllAction(input_arr[z]);
                        tmp = wr.RemoveMoreSpace(tmp.Trim()).Trim();

                        withDeli = "";
                        output = "";

                        string tmp_wb = "";
                        while (tryAgain >= 0)
                        {
                            try
                            {
                                tmp_wb = wb.wb_action(tmp);
                                tmp_wb = wr.RemoveMoreSpace(tmp_wb).Trim();
                                if (!string.IsNullOrEmpty(tmp_wb))
                                    break;
                                else
                                {
                                    tryAgain--;
                                    Thread.Sleep(WAIT_TRY);
                                }
                            }
                            catch
                            {
                                tryAgain--;
                                Thread.Sleep(WAIT_TRY);
                            }
                        }
                        // wb to sound
                        string tmp_sound = "";
                        while (tryAgain >= 0)
                        {
                            try
                            {
                                tmp_sound = sound.Thai_soundex(tmp_wb).Replace("^", " ");
                                if (!string.IsNullOrEmpty(tmp_sound))
                                    break;
                                else
                                {
                                    tryAgain--;
                                    Thread.Sleep(WAIT_TRY);
                                }
                            }
                            catch
                            {
                                tryAgain--;
                                Thread.Sleep(WAIT_TRY);
                            }
                        }

                        output += tmp_sound + " ";
                        withDeli += tmp_sound + "|";

                        withDeli = withDeli.Substring(0, withDeli.Length - 1);

                        list_output.Add(wr.RemoveMoreSpace(output).Trim());
                        list_withDeli.Add(wr.RemoveMoreSpace(output).Trim());
                        list_wordB.Add(wr.RemoveMoreSpace(tmp_wb.Replace("|", " ")).Trim());
                    }

                    output = ListTOString(list_output);
                    withDeli = ListTOString(list_withDeli);
                    wordB = ListTOString(list_wordB);

                    output = fix_urgen(output);
                    withDeli = fix_urgen(withDeli);
                    //if (string.IsNullOrEmpty(wr.RemoveMoreSpace(output).Trim()))
                    //    continue;
                    //else
                    //    return true;
                }
                catch
                {
                    //tryAgain--;
                    //Thread.Sleep(WAIT_TRY);
                }
            }
            return false;
        }

        private string ListTOString(List<string> input)
        {
            if (input == null)
                return "";
            if (input.Count == 0)
                return "";

            string output = "";
            for (int i = 0; i < input.Count; i++)
            {
                output += input[i] + "|";
            }
            output = output.Substring(0, output.Length - 1);
            return output;
        }

        private string fix_urgen(string input)
        {
            string output = input.Replace("ke:_1 sO:n_5 ?@:_1 b@:n_3", "ke:_1 sO:n_5 ?e:อ_1 b@:n_1");
            return output;
        }


    }
}