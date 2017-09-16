using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class Institutions : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new InstitutionsHandler();
        }
    }

    public class InstitutionsHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            List<Institution> institutions;

            int type = -1; //default, all except sponsors
            if (context.Request.Params["type"] != null)
                Int32.TryParse(context.Request.Params["type"], out type);

            if (type == 4) //sponsors TODO: make not hardcoded?
            {
                institutions = new List<Institution>();
                institutions.Add(new Institution() { name = "Kulturstyrelsen", url = "http://www.kulturstyrelsen.dk/", email = "post@kulturstyrelsen.dk", logoimageid = 107499 });
                institutions.Add(new Institution() { name = "Odense Kommune", url = "http://odense.dk/", email = "odense@odense.dk", logoimageid = 107500 });
                institutions.Add(new Institution() { name = "Region Syddanmark", url = "http://regionsyddanmark.dk/", email = "kontakt@regionsyddanmark.dk", logoimageid = 107501 });
                institutions.Add(new Institution() { name = "Undervisningsministeriet", url = "http://uvm.dk/", email = "uvm@uvm.dk", logoimageid = 107502 });
                institutions.Add(new Institution() { name = "Kulturregion Fyn", url = "http://kulturregionfyn.dk/", email = "blam@odense.dk", logoimageid = 107503 });
                institutions.Add(new Institution() { name = "Kulturregion Vadehavet", url = "http://www.kulturaftale-vadehavet.dk/", email = "elgjd@esbjergkommune.dk", logoimageid = 107504 });
                institutions.Add(new Institution() { name = "Kulturregion Storstrøm", url = "http://www.kulturregion-storstroem.dk/", email = "klje@vordingborg.dk", logoimageid = 107505 });
                institutions.Add(new Institution() { name = "Ribe 1300", url = "http://ribe1300.dk/", email = "cbj@esbjergkommune.dk", logoimageid = 107506 });
            }
            else
                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT Institution.TagID, URL, Email, Type FROM Institution, Tag WHERE Institution.TagID = Tag.TagID" + (type > -1 ? " AND Type = " + type : "") + " ORDER BY PlurName", conn);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        institutions = new List<Institution>();
                        while (dr.Read())
                            institutions.Add(new Institution() { tagid = (int)dr["TagID"], url = dr["URL"].ToString(), email = dr["Email"].ToString(), type = dr["Type"] is DBNull ? null : (byte?)dr["Type"] });
                    }
                }

            Common.SendStats(context, "institutions");
            Common.WriteOutput(institutions, context);
        }

        public bool IsReusable { get { return false; } }
    }
}