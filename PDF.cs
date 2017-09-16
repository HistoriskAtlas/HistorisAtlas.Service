using System;
using System.Web;
using System.Web.Routing;
using System.Web.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Data.SqlClient;
using System.Net;

namespace HistoriskAtlas.Service
{
    public class PDFRoute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new PDFHandler(context.RouteData);
        }
    }

    public class PDFHandler : IHttpHandler
    {
        private RouteData routeData;

        public PDFHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            Common.SendStats(context, "pdf");
            int pdfID;

            if (!int.TryParse(routeData.Values["id"].ToString(), out pdfID))
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error! Could not parse pdf id: " + routeData.Values["id"].ToString());
                return;
            }

            string filename = "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT Filename FROM PDF WHERE PDFID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        filename = dr["Filename"].ToString();
                    }
                }
            }

            if (filename == "")
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error! Could not find PDF with id: " + routeData.Values["id"]);
                return;
            }

            string url = @"http://historiskatlas.dk/pdf/" + filename;
            context.Response.Redirect(url, true);
        }

        public bool IsReusable { get { return false; } }
    }
}