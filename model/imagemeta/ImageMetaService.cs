using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class ImageMetaRoute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new ImageMetaHandler(context.RouteData);
        }
    }

    public class ImageMetaHandler : IHttpHandler
    {
        private RouteData routeData;

        public ImageMetaHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            if (Common.IsId1001(Int32.Parse(routeData.Values["id"].ToString())))
            {
                GetImageMeta1001(context);
                return;
            }

            GetImageMeta(context);
        }

        private void GetImageMeta(HttpContext context)
        {
            ImageMeta imageMeta = new ImageMeta();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT ImageID, Text, Year FROM Image WHERE ImageID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        imageMeta = new ImageMeta() { id = (int)dr["ImageID"], text = dr["Text"].ToString(), year = dr["Year"] is DBNull ? null : (int?)dr["Year"] };

                        using (SqlCommand cmdTags = new SqlCommand("SELECT TagID FROM Tag_Image WHERE ImageID = " + imageMeta.id, conn))
                        using (SqlDataReader drTags = cmdTags.ExecuteReader())
                            while (drTags.Read())
                                imageMeta.tagids.Add((int)drTags["TagID"]);
                    }
                }
            }

            Common.SendStats(context, "imagemeta");
            Common.WriteOutput(imageMeta, context);
        }

        private void GetImageMeta1001(HttpContext context)
        {
            ImageMeta imageMeta = new ImageMeta();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT ImageID, Text, Credit FROM Image_External_1001 WHERE ImageID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        imageMeta = new ImageMeta() { id = (int)dr["ImageID"], text = dr["Text"].ToString(), credit = dr["Credit"].ToString() };
                        imageMeta.tagids.Add(388);
                    }
                }
            }

            Common.SendStats(context, "imagemeta1001");
            Common.WriteOutput(imageMeta, context);
        }
            

        public bool IsReusable { get { return false; } }
    }
}