using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;

namespace HistoriskAtlas.Service
{
    public class GeoTextRoute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new GeoTextHandler(context.RouteData);
        }
    }

    public class GeoTextHandler : IHttpHandler
    {
        private RouteData routeData;

        public GeoTextHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            if (Common.IsId1001(Int32.Parse(routeData.Values["id"].ToString())))
            {
                Get1001Text(context);
                return;
            }

            GetHAText(context);
        }

        private void GetHAText(HttpContext context)
        {
            bool returnXaml = context.Request.Params["xaml"] == null ? false : bool.Parse(context.Request.Params["xaml"]);
            bool returnHtml = context.Request.Params["html"] == null ? false : bool.Parse(context.Request.Params["html"]);
            GeoText geoText = new GeoText();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT Headline, Text FROM Geo_Text WHERE GeoID = " + routeData.Values["id"] + " AND Ordering = " + routeData.Values["ordering"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        geoText = new GeoText() { headline = dr["Headline"].ToString() };
                        geoText.text = returnXaml ? dr["Text"].ToString() : Common.GetHTMLFromXAML(dr["Text"].ToString(), returnHtml);
                    }
                }
            }

            Common.SendStats(context, "geotext");
            Common.WriteOutput(geoText, context, routeData);
        }

        private void Get1001Text(HttpContext context)
        {
            GeoText geoText = new GeoText();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT StoryTitle, StoryBody FROM Geo_External_1001 WHERE LocalID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        geoText = new GeoText() { headline = dr["StoryTitle"].ToString(), text = dr["StoryBody"].ToString().Replace("\n", "<br>") };
                    }
                }
            }

            Common.SendStats(context, "geotext1001");
            Common.WriteOutput(geoText, context, routeData);
        }

        public bool IsReusable { get { return false; } }
    }
}