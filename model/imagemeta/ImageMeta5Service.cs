using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class ImageMeta5Route : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new ImageMeta5Handler(context.RouteData);
        }
    }

    public class ImageMeta5Handler : IHttpHandler
    {
        private RouteData routeData;

        public ImageMeta5Handler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            GetImageMeta(context);
        }

        private void GetImageMeta(HttpContext context)
        {
            ImageMeta imageMeta = new ImageMeta();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb5"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT ImageID, Text, Year, Photographer, Licensee FROM Image WHERE ImageID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        imageMeta = new ImageMeta() { id = (int)dr["ImageID"], text = dr["Text"].ToString(), year = dr["Year"] is DBNull ? null : (int?)((Int16)dr["Year"]), photographer = dr["Photographer"] is DBNull ? null : dr["Photographer"].ToString(), licensee = dr["Licensee"] is DBNull ? null : dr["Licensee"].ToString() };

                        using (SqlCommand cmdTags = new SqlCommand("SELECT TagID FROM Tag_Image WHERE ImageID = " + imageMeta.id, conn))
                        using (SqlDataReader drTags = cmdTags.ExecuteReader())
                            while (drTags.Read())
                                imageMeta.tagids.Add((int)drTags["TagID"]);
                    }
                }
            }

            Common.SendStats(context, "imagemeta5");
            Common.WriteOutput(imageMeta, context);
        }

        public bool IsReusable { get { return false; } }
    }
}