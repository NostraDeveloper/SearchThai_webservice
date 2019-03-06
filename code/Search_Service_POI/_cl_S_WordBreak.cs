using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;

namespace Search_Service_POI
{
    public class _cl_S_WordBreak
    {
        int WAIT_TRY = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["waitTry"].ToString());
        int TRY_AGAIN = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["tryAgain"].ToString());
        ServiceReference2.WB_ServiceSoapClient WBservice = new ServiceReference2.WB_ServiceSoapClient("WB_ServiceSoap");

        public string wb_action(string input_wb)
        {
            int tryAgain = TRY_AGAIN;
            //while (tryAgain >= 0)
            {
                try
                {
                    if (string.IsNullOrEmpty(input_wb))
                        return "";

                    string[] tmp_ = input_wb.Split('|');
                    List<string> tmp_out = new List<string>();

                    for (int i = 0; i < tmp_.Length; i++)
                    {
                        string tmp_wb = "";
                        while (tryAgain >= 0)
                        {
                            if (string.IsNullOrEmpty(tmp_[i].Trim()))
                                break;
                            try
                            {
                                tmp_wb = WBservice.Do_WordsBK(tmp_[i]);
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

                        tmp_wb = tmp_wb.Trim().Replace("^", " ").ToUpper();
                        tmp_out.Add(tmp_wb);
                    }

                    string output = "";
                    for (int i = 0; i < tmp_out.Count; i++)
                    {
                        output += tmp_out[i].ToString().Trim() + "|";
                    }
                    output = output.Substring(0, output.Length - 1);
                    output = output.ToUpper();
                }
                catch
                {
                    //tryAgain--;
                    //Thread.Sleep(WAIT_TRY);
                }
            }
            return "";
        }
    }
}