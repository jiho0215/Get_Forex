using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        private string[] countries = new string[] { "usd", "gbp", "krw" };  
        private string rate = "";
        private string fxcode = "";
        private string from = "";
        private string to = "";
        private string ip = "127.0.0.1"; //localhost
        private string dbName = "soodas";

        static void Main(string[] args)
        {
            ConsoleApplication1.Program ob1 = new ConsoleApplication1.Program();
            ob1.run();
        }

        private void run()
        {
            while (true)
            {
                for (int i = 0; i < (countries.Length); i++)
                {
                    for (int j = i + 1; j < countries.Length; j++)  
                    {
                        this.from =countries[i];
                        this.to =countries[j];

                        if (this.from == this.to) continue;
                        else this.fxcode = this.from + this.to;

                        getRate();

                        if (this.rate == "") errorlog(fxcode+" Rate is NULL");
                        else setRate();

                        this.fxcode = "";
                        rate = "";
                    }
                }
                Console.WriteLine("---------------------------------");
                Thread.Sleep(14000);
            }//end while
        }

        private void getRate()
        {
            this.rate = "";
            //Parsing part example: 1&nbsp;USD&nbsp;=&nbsp;0.637624&nbsp;GBP</td>
            string webaddress = "http://www.xe.com/currencyconverter/convert/?Amount=1&From=" + this.from + "&To=" + this.to;
            try
            {
                WebClient web = new WebClient();
                String html = web.DownloadString(webaddress);
                MatchCollection m1 = Regex.Matches(html, @"1&nbsp;" + this.from.ToUpper() + "&nbsp;=&nbsp;(.+?)&nbsp;" + this.to.ToUpper() + "</td>", RegexOptions.Singleline);

                foreach (Match m in m1)
                {
                    try
                    {
                        this.rate = m.Groups[1].Value;
                        if (this.fxcode == (from + "krw") || this.fxcode == ("krw"+to))
                        {
                            this.rate = this.rate.Replace(",","");
                        }
                        Console.WriteLine(this.fxcode+"\t"+this.rate+" \t"+DateTime.Now);
                    }
                    catch(Exception e)
                    {
                        errorlog("Error in the getRate Method. Read Rate from Retrieved String.");
                    }
                }
            }
            catch (Exception e)
            {
                errorlog("Error in the getRate Method. Maybe Web Connection Fail");
            }
        }

        private void setRate()
        {
            try
            {
                //Set SQL
                SqlConnection conn = new SqlConnection("Data Source=" + ip + ";Initial Catalog=" + dbName + ";Integrated Security=true;");
                conn.Open();
                Console.WriteLine("Saved on "+fxcode+" table.");
                SqlCommand cmd = new SqlCommand("insert into " + this.fxcode + "(fxcode, rate) values('" + this.fxcode + "'," + this.rate + ")", conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                //Set SQL END
            }
            catch (Exception e)
            {
                errorlog("setRate Method '"+fxcode + "' SQL Connection Error.");
            }
        }
        public int exitCode { get; set; }
        
        public void errorlog(string msg)
        {
            try
            {
                //Set SQL
                SqlConnection conn = new SqlConnection("Data Source=" + ip + ";Initial Catalog=" + dbName + ";User ID=sa;Password=;");
                conn.Open();
                SqlCommand cmd = new SqlCommand("insert into error_log (msg) values('"+msg+"')", conn);
                cmd.ExecuteNonQuery();
                conn.Close();
                //Set SQL END
            }
            catch (Exception e)
            {
                Console.WriteLine("Error_log Method Error");
            }
        }
    }
}