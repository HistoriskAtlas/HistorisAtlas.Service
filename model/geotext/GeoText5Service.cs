using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;

namespace HistoriskAtlas.Service
{
    public class GeoText5Route : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new GeoText5Handler(context.RouteData);
        }
    }

    public class GeoText5Handler : IHttpHandler
    {
        private RouteData routeData;

        public GeoText5Handler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            GetHAText(context);
        }

        private void GetHAText(HttpContext context)
        {
            GeoText geoText = new GeoText();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb5"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT Headline, Text FROM Content, Text WHERE Content.ContentID = Text.ContentID AND GeoID = " + routeData.Values["id"] + " AND Content.Ordering = " + routeData.Values["ordering"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        geoText = new GeoText() { headline = dr["Headline"].ToString() };
                        geoText.text = dr["Text"].ToString();
                    }
                }
            }

            Common.SendStats(context, "geotext5");
            Common.WriteOutput(geoText, context, routeData);
        }

        public bool IsReusable { get { return false; } }
    }
}