using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HistoriskAtlas.Service
{
    public partial class tilestats : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public string GetChartData()
        {
            StringBuilder sb = new StringBuilder();
            DateTime dt;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["hadbStats"].ConnectionString))
            {
                con.Open();

                //int max = (int)new SqlCommand("SELECT MAX(CacheCount + NotModifiedCount + GenerateCount + ErrorCount) AS count FROM StatsTile", con).ExecuteScalar();
                using (SqlDataReader dr = new SqlCommand("SELECT [Date], CacheCount, NotModifiedCount, GenerateCount, ErrorCount, AvgLoadTimeCache, AvgLoadTimeNotModified, AvgLoadTimeGenerate, AvgLoadTime FROM StatsTile ORDER BY Date", con).ExecuteReader())
                {
                    bool morerows = dr.Read();
                    while (morerows)
                    {
                        //sb.Append("['" + (DateTime)dr["Date"] + "'");
                        //sb.Append(", " + (int)dr["ErrorCount"]);
                        //sb.Append(", " + (int)dr["GenerateCount"]);
                        //sb.Append(", " + (int)dr["CacheCount"]);
                        //sb.Append(", " + (int)dr["NotModifiedCount"]);
                        //sb.Append(", '']");

                        dt = (DateTime)dr["Date"];
                        sb.Append("{c:[{v:new Date(" + dt.Year + ", " + dt.Month + ", " + dt.Day + ")},{v:" + (int)dr["ErrorCount"] + "},{v:" + (int)dr["GenerateCount"] + "},{v:" + (int)dr["CacheCount"] + "},{v:" + (int)dr["NotModifiedCount"] + "},{v:" + (int)dr["AvgLoadTime"] + "},{v:" + (int)dr["AvgLoadTimeGenerate"] + "},{v:" + (int)dr["AvgLoadTimeCache"] + "},{v:" + (int)dr["AvgLoadTimeNotModified"] + "}]}");
                        
                        morerows = dr.Read();
                        if (morerows)
                            sb.Append(",");
                    }
                }
            }

            return sb.ToString();
        }

        public string GetGraph()
        {
            string result = "<TABLE cellpadding=0 cellspacing=0><TR>";
            int graphHeight = 200;

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["hadbStats"].ConnectionString))
            {
                con.Open();

                int max = (int)new SqlCommand("SELECT MAX(CacheCount + NotModifiedCount + GenerateCount + ErrorCount) AS count FROM StatsTile", con).ExecuteScalar();
                using (SqlDataReader dr = new SqlCommand("SELECT [Date], CacheCount, NotModifiedCount, GenerateCount, ErrorCount FROM StatsTile ORDER BY Date", con).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        string total = PrettyInteger((int)dr["NotModifiedCount"] + (int)dr["CacheCount"] + (int)dr["GenerateCount"] + (int)dr["ErrorCount"]);
                        result += "<TD>";
                        result += createBar((int)dr["NotModifiedCount"] * graphHeight / max, "green", ((DateTime)dr["Date"]).ToShortDateString() + " - not modified: " + PrettyInteger((int)dr["NotModifiedCount"]) + " - total: " + total);
                        result += createBar((int)dr["CacheCount"] * graphHeight / max, "orange", ((DateTime)dr["Date"]).ToShortDateString() + " - cache: " + PrettyInteger((int)dr["CacheCount"]) + " - total: " + total);
                        result += createBar((int)dr["GenerateCount"] * graphHeight / max, "red", ((DateTime)dr["Date"]).ToShortDateString() + " - generate: " + PrettyInteger((int)dr["GenerateCount"]) + " - total: " + total);
                        result += createBar((int)dr["ErrorCount"] * graphHeight / max, "black", ((DateTime)dr["Date"]).ToShortDateString() + " - error: " + PrettyInteger((int)dr["ErrorCount"]) + " - total: " + total);
                        result += "</TD>";
                    }
                }

                result += "</TR></TABLE><BR>";
                //result += "<TABLE cellpadding=0 cellspacing=0><TR>";

                //max = (int)new SqlCommand("SELECT MAX(CacheCount + NotModifiedCount + GenerateCount) AS count FROM StatsTile", con).ExecuteScalar();
                //using (SqlDataReader dr = new SqlCommand("SELECT [Date], CacheCount, NotModifiedCount, GenerateCount FROM StatsTile ORDER BY Date", con).ExecuteReader())
                //{
                //    while (dr.Read())
                //    {
                //        string total = PrettyInteger((int)dr["NotModifiedCount"] + (int)dr["CacheCount"] + (int)dr["GenerateCount"]);
                //        result += "<TD valign=bottom>";
                //        result += createBar((int)dr["NotModifiedCount"] * graphHeight / max, "green", ((DateTime)dr["Date"]).ToShortDateString() + " - not modified: " + PrettyInteger((int)dr["NotModifiedCount"]) + " - total: " + total);
                //        result += createBar((int)dr["CacheCount"] * graphHeight / max, "orange", ((DateTime)dr["Date"]).ToShortDateString() + " - cache: " + PrettyInteger((int)dr["CacheCount"]) + " - total: " + total);
                //        result += createBar((int)dr["GenerateCount"] * graphHeight / max, "red", ((DateTime)dr["Date"]).ToShortDateString() + " - generate: " + PrettyInteger((int)dr["GenerateCount"]) + " - total: " + total);
                //        result += "</TD>";
                //    }
                //}

                //result += "</TR></TABLE><BR>";
                result += "<TABLE cellpadding=0 cellspacing=0><TR>";

                int i = 0;
                int CacheCount = 0, NotModifiedCount = 0, GenerateCount = 0, ErrorCount = 0;

                using (SqlDataReader dr = new SqlCommand("SELECT [Date], CacheCount, NotModifiedCount, GenerateCount, ErrorCount FROM StatsTile ORDER BY Date", con).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        CacheCount += (int)dr["CacheCount"];
                        NotModifiedCount += (int)dr["NotModifiedCount"];
                        GenerateCount += (int)dr["GenerateCount"];
                        ErrorCount += (int)dr["ErrorCount"];
                        i++;

                        //if (i % 2 == 0)
                        //{
                            max = (int)dr["NotModifiedCount"] + (int)dr["CacheCount"] + (int)dr["GenerateCount"] + (int)dr["ErrorCount"];
                            result += "<TD>";
                            result += createBar(NotModifiedCount * graphHeight / max, "green", ((DateTime)dr["Date"]).ToShortDateString() + " - not modified: " + (NotModifiedCount * 100 / max) + "%");
                            result += createBar(CacheCount * graphHeight / max, "orange", ((DateTime)dr["Date"]).ToShortDateString() + " - cache: " + (CacheCount * 100 / max) + "%");
                            result += createBar(GenerateCount * graphHeight / max, "red", ((DateTime)dr["Date"]).ToShortDateString() + " - generate: " + (GenerateCount * 100 / max) + "%");
                            result += createBar(ErrorCount * graphHeight / max, "black", ((DateTime)dr["Date"]).ToShortDateString() + " - error: " + (ErrorCount * 100 / max) + "%");
                            result += "</TD>";

                            CacheCount = 0; NotModifiedCount = 0; GenerateCount = 0; ErrorCount = 0;
                        //}
                    }
                }

                result += "</TR></TABLE><BR>";
                //result += "<TABLE cellpadding=0 cellspacing=0><TR>";

                //using (SqlDataReader dr = new SqlCommand("SELECT [Date], CacheCount, NotModifiedCount, GenerateCount FROM StatsTile ORDER BY Date", con).ExecuteReader())
                //{
                //    while (dr.Read())
                //    {
                //        max = (int)dr["NotModifiedCount"] + (int)dr["CacheCount"] + (int)dr["GenerateCount"];
                //        result += "<TD valign=bottom>";
                //        result += createBar((int)dr["NotModifiedCount"] * graphHeight / max, "green", ((DateTime)dr["Date"]).ToShortDateString() + " - not modified: " + ((int)dr["NotModifiedCount"] * 100 / max) + "%");
                //        result += createBar((int)dr["CacheCount"] * graphHeight / max, "orange", ((DateTime)dr["Date"]).ToShortDateString() + " - cache: " + ((int)dr["CacheCount"] * 100 / max) + "%");
                //        result += createBar((int)dr["GenerateCount"] * graphHeight / max, "red", ((DateTime)dr["Date"]).ToShortDateString() + " - generate: " + ((int)dr["GenerateCount"] * 100 / max) + "%");
                //        result += "</TD>";
                //    }
                //}

                //result += "</TR></TABLE><BR>";
                result += "<TABLE cellpadding=0 cellspacing=0><TR>";

                //max = (int)new SqlCommand("SELECT MAX(AvgLoadTime) AS count FROM StatsTile", con).ExecuteScalar();
                max = 60000; // 1 min
                using (SqlDataReader dr = new SqlCommand("SELECT [Date], AvgLoadTime FROM StatsTile ORDER BY Date", con).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result += "<TD>";
                        result += createBar(Math.Min((int)dr["AvgLoadTime"], 60000) * graphHeight / max, "red", ((DateTime)dr["Date"]).ToShortDateString() + " - avg. load time: " + ((decimal)((int)dr["AvgLoadTime"]) / 1000));
                        result += "</TD>";
                    }
                }
            }

            result += "</TR></TABLE>";

            return result;
        }

        public string createBar(int height, string color, string mouseover)
        {
            return "<DIV style='height:" + height + "px; background-color:" + color + "' onmouseover='over(\"" + mouseover + "\")' onmouseout='out()'></div>";
        }

        public string GetStats(string type)
        {
            string[] highlight = { "Historisk Atlas", "HistoriskAtlas", "Apache-HttpClient/UNAVAILABLE (java 1.4)" };
            string[] lowlight = { "Mozilla/" };
            string className = "high";
            
            StringBuilder sb = new StringBuilder();
            sb.Append("<TABLE cellpadding=0 cellspacing=0>");
            sb.Append("<TR><TD style='font-weight:bold'>" + type + "</TD><TD style='text-align:right; padding-left:10px; font-weight:bold'>Count</TD></TR>");

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["hadbStats"].ConnectionString))
            {
                con.Open();
                using (SqlDataReader dr = new SqlCommand("SELECT [" + type + "], SUM(Count) AS Count FROM [hadb5].[dbo].[Stats" + type + "] WHERE Event = 'tile' GROUP BY " + type + " ORDER BY Count DESC", con).ExecuteReader()) //AND [" + type + "] NOT LIKE 'http://localhost%'
                {
                    while (dr.Read())
                    {
                        string t = dr[type].ToString();
                        if (type == "UserAgent")
                        {
                            className = "unknown";
                            foreach (string h in highlight)
                                if (t.Contains(h))
                                    className = "high";
                            //t = t.Replace(h, "<b>" + h + "</b>");

                            foreach (string l in lowlight)
                                if (t.Contains(l))
                                    className = "low";
                        }

                        sb.Append("<TR><TD class='" + className + "'>" + t + "</TD><TD style='text-align:right; padding-left:10px; vertical-align:top'>" + PrettyInteger((int)dr["Count"]) + "</TD></TR>");
                    }
                }
            }

            sb.Append("</TABLE>");
            return sb.ToString();
        }

        public string GetErrors()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<TABLE cellpadding=0 cellspacing=0 style='border-spacing:10px 0px'>");
            sb.Append("<TR><TD style='font-weight:bold'>Error</TD><TD style='font-weight:bold'>UrlReferrer</TD><TD style='font-weight:bold'>UserAgent</TD><TD style='font-weight:bold; text-align:right'>Count</TD></TR>");

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["hadbStats"].ConnectionString))
            {
                con.Open();
                using (SqlDataReader dr = new SqlCommand("SELECT TOP 1000 Error, UrlReferrer, UserAgent, COUNT(*) AS count FROM [hadb5].[dbo].[StatsTileError] GROUP BY Error, UrlReferrer, UserAgent ORDER BY count DESC", con).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        sb.Append("<TR>");
                        sb.Append("<TD style='vertical-align:top'>" + dr["Error"].ToString() + "</TD>");
                        sb.Append("<TD style='vertical-align:top'>" + dr["UrlReferrer"].ToString() + "</TD>");
                        sb.Append("<TD style='vertical-align:top'>" + dr["UserAgent"].ToString() + "</TD>");
                        //result += "<TD style='vertical-align:top'>" + dr["Request"].ToString() + "</TD>";
                        sb.Append("<TD style='vertical-align:top; text-align:right'>" + PrettyInteger((int)dr["count"]) + "</TD>");
                        sb.Append("</TR>");
                    }
                }
            }

            sb.Append("</TABLE>");
            return sb.ToString();
        }

        public string PrettyInteger(int i)
        {
            string s = "";
            string nul = "0000";
            while (i >= 1000)
            {
                s = "." + nul.Remove(3 - ("" + i % 1000).Length) + (i % 1000) + s;
                i = i / 1000;
            }
            s = i + s;
            return s;
        }
    }
}
