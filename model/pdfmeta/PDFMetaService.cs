using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class PDFMetaRoute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new PDFMetaHandler(context.RouteData);
        }
    }

    public class PDFMetaHandler : IHttpHandler
    {
        private RouteData routeData;

        public PDFMetaHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            GetPDFMeta(context);
        }

        private void GetPDFMeta(HttpContext context)
        {
            PDFMeta pdfMeta = new PDFMeta();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT Title FROM PDF WHERE PDFID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        pdfMeta = new PDFMeta() { title = dr["Title"].ToString() };
                    }
                }
            }

            Common.SendStats(context, "pdfmeta");
            Common.WriteOutput(pdfMeta, context);
        }

        public bool IsReusable { get { return false; } }
    }
}