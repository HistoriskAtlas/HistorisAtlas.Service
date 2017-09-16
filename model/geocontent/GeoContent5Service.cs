using System;
using System.Web;
using System.Web.Routing;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class GeoContent5Route : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new GeoContent5Handler(context.RouteData);
        }
    }

    public class GeoContent5Handler : IHttpHandler
    {
        private RouteData routeData;

        public GeoContent5Handler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            GetGeoContent(context);
        }

        private void GetGeoContent(HttpContext context)
        {
            GeoContent geoContent = new GeoContent();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb5"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT GeoID, Title, Intro, Latitude, Longitude FROM Geo WHERE GeoID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        geoContent = new GeoContent() { 
                            id = (int)dr["GeoID"], 
                            title = dr["Title"].ToString(), 
                            lat = (decimal)dr["Latitude"], 
                            lng = (decimal)dr["Longitude"], 
                            intro = dr["Intro"].ToString() 
                        };

                        using (SqlCommand cmdTags = new SqlCommand("SELECT TagID FROM Tag_Geo WHERE GeoID = " + geoContent.id, conn))
                        using (SqlDataReader drTags = cmdTags.ExecuteReader())
                            while (drTags.Read())
                                geoContent.tagids.Add((int)drTags["TagID"]);

                        using (SqlCommand cmdImgs = new SqlCommand("SELECT ImageID FROM Geo_Image WHERE GeoID = " + geoContent.id, conn))
                        using (SqlDataReader drImgs = cmdImgs.ExecuteReader())
                            while (drImgs.Read())
                                geoContent.imageids.Add((int)drImgs["ImageID"]);

                        //using (SqlCommand cmdVids = new SqlCommand("SELECT VideoID FROM Geo_Video WHERE GeoID = " + geoContent.id, conn))
                        //using (SqlDataReader drVids = cmdVids.ExecuteReader())
                        //    while (drVids.Read())
                        //        geoContent.videoids.Add((int)drVids["VideoID"]);

                        //using (SqlCommand cmdPDFs = new SqlCommand("SELECT PDFID FROM Geo_PDF WHERE GeoID = " + geoContent.id, conn))
                        //using (SqlDataReader drPDFs = cmdPDFs.ExecuteReader())
                        //    while (drPDFs.Read())
                        //        geoContent.pdfids.Add((int)drPDFs["PDFID"]);

                        //using (SqlCommand cmdExts = new SqlCommand("SELECT Source, Link FROM Geo_Ext WHERE GeoID = " + geoContent.id, conn))
                        //using (SqlDataReader drExts = cmdExts.ExecuteReader())
                        //    while (drExts.Read())
                        //        geoContent.exts.Add(new GeoContentExt() { source = (byte)drExts["Source"], link = drExts["Link"].ToString() }); //"http://www.youtube.com/watch?v="

                        using (SqlCommand cmdTexts = new SqlCommand("SELECT Content.Ordering, Headline FROM Content, Text WHERE Content.ContentID = Text.ContentID AND GeoID = " + geoContent.id + " ORDER BY Content.Ordering", conn))
                        using (SqlDataReader drTexts = cmdTexts.ExecuteReader())
                            while (drTexts.Read())
                                geoContent.texts.Add(new GeoContentText() { ordering = (int)((Int16)drTexts["Ordering"]), headline = drTexts["Headline"].ToString() });
                    }
                }
            }

            Common.SendStats(context, "geocontent5");
            Common.WriteOutput(geoContent, context, routeData);
        }

        public bool IsReusable { get { return false; } }
    }
}