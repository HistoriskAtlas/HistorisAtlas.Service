using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization.Json;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Xml.Serialization;
using System.Web.Routing;
using System.Text.RegularExpressions;

namespace HistoriskAtlas.Service
{
    public class Common
    {
        public static void WriteOutput(object obj, HttpContext context, RouteData routeData = null)
        {
            if (RequestIsForXml(context)) //XML
            {
                context.Response.ContentType = "application/xml; charset=UTF-8";
                XmlSerializer xmlSer = new XmlSerializer(obj.GetType());
                xmlSer.Serialize(context.Response.OutputStream, obj);
                return;
            }

            if (RequestIsForHtml(context)) //HTML
            {
                context.Response.ContentType = "text/html";
                context.Response.Write(obj as string);
                return;
            }

            DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
            if (context.Request.Params["callback"] == null) //JSON
            {
                context.Response.ContentType = "application/json; charset=UTF-8";
                ser.WriteObject(context.Response.OutputStream, obj);
                return;
            }

            //JSONP
            context.Response.ContentType = "application/javascript; charset=UTF-8";
            context.Response.Write(context.Request.Params["callback"] + "(");
            ser.WriteObject(context.Response.OutputStream, obj);
            context.Response.OutputStream.Flush();
            context.Response.Write(")");
        }

        public static bool RequestIsForXml(HttpContext context)
        {
            return context.Request.Url.GetLeftPart(UriPartial.Path).EndsWith(".xml");
        }

        public static bool RequestIsForHtml(HttpContext context)
        {
            return context.Request.Url.GetLeftPart(UriPartial.Path).EndsWith(".html");
        }

        public static T ReadJson<T>(HttpContext context)
        {
            DataContractJsonSerializer serLatLng = new DataContractJsonSerializer(typeof(T));
            return (T)serLatLng.ReadObject(context.Request.InputStream);
        }

        public static SortedDictionary<string, string> GetParams(HttpContext context, string defaultKey = "id")
        {
            SortedDictionary<string, string> param = new SortedDictionary<string, string>();
            char[] splitSlash = { '/' };
            char[] split = { '=' };
            List<string> ids = new List<string>();
            foreach (string str in context.Request.Path.TrimStart(splitSlash).Split(splitSlash))
            {
                if (str.Split(split).Length == 1)
                    ids.Add(str);
                else
                {
                    if (str.Split(split)[0] == defaultKey)
                    {
                        ids.Add(str.Split(split)[1]);
                        continue;
                    }
                    if (str.Split(split)[0] == "cachebreak")
                        continue;
                    param.Add(str.Split(split)[0], str.Split(split)[1]);
                }
            }

            param.Add(defaultKey, string.Join("/", ids));
            return param;
        }

        public static void SendStats(HttpContext context, string eventName)
        {
            using (SqlConnection connStats = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadbStats"].ConnectionString))
            {
                connStats.Open();
                SendStat(connStats, eventName, "UrlReferrer", context.Request.UrlReferrer == null ? "" : Left(context.Request.UrlReferrer.ToString(), 255));
                SendStat(connStats, eventName, "UserAgent", context.Request.UserAgent == null ? "" : Left(context.Request.UserAgent, 255));
            }
        }

        private static void SendStat(SqlConnection conn, string eventName, string statName, string stat)
        {
            DateTime today = DateTime.Today;
            SqlCommand cmdGet = new SqlCommand("SELECT [Count] FROM Stats" + statName + " WHERE Date = @Date AND Event = @Event AND " + statName + " = @" + statName, conn);
            SqlCommand cmdInsert = new SqlCommand("INSERT INTO Stats" + statName + " (Date, Event, " + statName + ", [Count]) VALUES (@Date, @Event, @" + statName + ", 1)", conn);
            SqlCommand cmdUpdate = new SqlCommand("UPDATE Stats" + statName + " SET [Count] = [Count] + 1 WHERE Date = @Date AND Event = @Event AND " + statName + " = @" + statName, conn);
            cmdGet.Parameters.AddWithValue("@Date", today);
            cmdGet.Parameters.AddWithValue("@Event", eventName);
            cmdGet.Parameters.AddWithValue("@" + statName, stat);
            object result = cmdGet.ExecuteScalar();
            SqlCommand cmd = result == null ? cmdInsert : cmdUpdate;
            cmd.Parameters.AddWithValue("@Date", today);
            cmd.Parameters.AddWithValue("@Event", eventName);
            cmd.Parameters.AddWithValue("@" + statName, stat);
            cmd.ExecuteNonQuery();
        }

        private static string Left(string str, int length)
        {
            return str.Substring(0, Math.Min(str.Length, length));
        }

        public static string GetHTMLFromXAML(string xaml, bool returnHtml)
        {
            string html;

            html = Regex.Replace(xaml, @"<Paragraph\b[^>]*>(.*?)</Paragraph>", @"$1<br />", RegexOptions.Multiline | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<Paragraph>", @"<br />", RegexOptions.Multiline | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<Run FontWeight=""Bold"" FontStyle=""Italic"" Text=""(.*?)"" />", returnHtml ? @"<b><i>$1</i></b>" : @"$1", RegexOptions.Multiline | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<Run FontWeight=""Bold"" Text=""(.*?)"" />", returnHtml ? @"<b>$1</b>" : @"$1", RegexOptions.Multiline | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<Run FontStyle=""Italic"" Text=""(.*?)"" />", returnHtml ? @"<i>$1</i>" : @"$1", RegexOptions.Multiline | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<Run Text=""(.*?)"" />", @"$1", RegexOptions.Multiline | RegexOptions.Singleline);

            if (returnHtml)
            {
                html = Regex.Replace(html, @"<Hyperlink\b[^>]*NavigateUri=" + "\"(.*?)\"" + @"[^>]*>(.*?)</Hyperlink>", @"<a href='$1' target='_blank'>$2</a>", RegexOptions.Multiline | RegexOptions.Singleline);
                html = Regex.Replace(html, @"<Hyperlink\b[^>]*TargetName=" + "\"geolink\\?id=(.*?)\"" + @"[^>]*>(.*?)</Hyperlink>", @"<a href='http://historiskatlas.dk/_($1)'>$2</a>", RegexOptions.Multiline | RegexOptions.Singleline);
            }
            html = Regex.Replace(html, @"<Hyperlink\b[^>]*>(.*?)</Hyperlink>", @"$1", RegexOptions.Multiline | RegexOptions.Singleline);

            //new maj 2013:
            html = Regex.Replace(html, @"<Run\b[^>]* />", @"", RegexOptions.Multiline | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<Run\b[^>]*FontStyle=""Italic""[^>]*>(.*?)</Run>", returnHtml ? @"<i>$1</i>" : @"$1", RegexOptions.Multiline | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<Run\b[^>]*FontWeight=""Bold""[^>]*>(.*?)</Run>", returnHtml ? @"<b>$1</b>" : @"$1", RegexOptions.Multiline | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<Run\b[^>]*>(.*?)</Run>", @"$1", RegexOptions.Multiline | RegexOptions.Singleline);

            //jan 2015: added "RegexOptions.Singleline"

            return html;
        }

        public static bool IsId1001(int id)
        {
            return id >= 100000000 && id < 200000000;
        }

        public static string PrettyInteger(string input)
        {
            int i;
            if (!Int32.TryParse(input, out i))
                return "0";

            return PrettyInteger(i);
        }

        public static string PrettyInteger(int i)
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