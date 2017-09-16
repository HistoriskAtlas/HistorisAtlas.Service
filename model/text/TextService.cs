using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class TextRoute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new TextHandler(context.RouteData);
        }
    }

    public class TextHandler : IHttpHandler
    {
        private RouteData routeData;

        public TextHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            Text text = new Text();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM Text WHERE TextID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        text = new Text() { id = (int)dr["TextID"], text = Common.GetHTMLFromXAML(dr["Text"].ToString(), false)};
                    }
                }
            }
            
            Common.SendStats(context, "text");
            Common.WriteOutput(text, context);
        }

        public bool IsReusable { get { return false; } }
    }
}