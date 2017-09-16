using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;

namespace HistoriskAtlas.Service
{
    public class GeoContentRoute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new GeoContentHandler(context.RouteData);
        }
    }

    public class GeoContentHandler : IHttpHandler
    {
        private RouteData routeData;

        public GeoContentHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            if (Common.IsId1001(Int32.Parse(routeData.Values["id"].ToString())))
            {
                GetGeoContent1001(context);
                return;
            }

            GetGeoContent(context);
        }

        private void GetGeoContent(HttpContext context)
        {
            GeoContent geoContent = new GeoContent();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                //TODO: Should only do this, if its a non editor who is accessing the info
                //new SqlCommand("UPDATE Geo SET Views = Views + 1 WHERE GeoID = " + routeData.Values["id"], conn).ExecuteNonQuery();

                SqlCommand cmd = new SqlCommand("SELECT GeoID, Title, Intro, GeoX, GeoY FROM Geo WHERE GeoID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        LatLng ll = LatLng.FromHACoord(new HACoord((int)dr["GeoX"], (int)dr["GeoY"]));
                        geoContent = new GeoContent() { 
                            id = (int)dr["GeoID"], 
                            title = dr["Title"].ToString(), 
                            lat = ll.latitude, 
                            lng = ll.longitude, 
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

                        using (SqlCommand cmdVids = new SqlCommand("SELECT VideoID FROM Geo_Video WHERE GeoID = " + geoContent.id, conn))
                        using (SqlDataReader drVids = cmdVids.ExecuteReader())
                            while (drVids.Read())
                                geoContent.videoids.Add((int)drVids["VideoID"]);

                        using (SqlCommand cmdPDFs = new SqlCommand("SELECT PDFID FROM Geo_PDF WHERE GeoID = " + geoContent.id, conn))
                        using (SqlDataReader drPDFs = cmdPDFs.ExecuteReader())
                            while (drPDFs.Read())
                                geoContent.pdfids.Add((int)drPDFs["PDFID"]);

                        using (SqlCommand cmdExts = new SqlCommand("SELECT Source, Link FROM Geo_Ext WHERE GeoID = " + geoContent.id, conn))
                        using (SqlDataReader drExts = cmdExts.ExecuteReader())
                            while (drExts.Read())
                                geoContent.exts.Add(new GeoContentExt() { source = (byte)drExts["Source"], link = drExts["Link"].ToString() }); //"http://www.youtube.com/watch?v="

                        using (SqlCommand cmdTexts = new SqlCommand("SELECT Ordering, Headline FROM Geo_Text WHERE GeoID = " + geoContent.id + " ORDER BY Ordering", conn))
                        using (SqlDataReader drTexts = cmdTexts.ExecuteReader())
                            while (drTexts.Read())
                                geoContent.texts.Add(new GeoContentText() { ordering = (int)drTexts["Ordering"], headline = drTexts["Headline"].ToString() });
                    }
                }
            }

            Common.SendStats(context, "geocontent");
            Common.WriteOutput(geoContent, context, routeData);
        }

        private void GetGeoContent1001(HttpContext context)
        {
            GeoContent1001 geoContent1001 = new GeoContent1001();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM Geo_External_1001 WHERE LocalID = " + routeData.Values["id"], conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        geoContent1001 = new GeoContent1001() { 
                            id = (int)dr["LocalID"], 
                            title = dr["Title"].ToString(), 
                            subtitle = dr["Subtitle"].ToString(),
                            lat = (decimal)dr["Lat"],
                            lng = (decimal)dr["Lng"], 
                            uri = dr["Uri"].ToString(),
                            intro = dr["Description"].ToString(),
                            license = dr["License"].ToString()
                        };

                        geoContent1001.texts.Add(new GeoContentText() { ordering = 0, headline = dr["StoryTitle"].ToString() });
                    }
                }

                using (SqlCommand cmdImgs = new SqlCommand("SELECT ImageID FROM Image_External_1001 WHERE LocalGeoID = " + geoContent1001.id, conn))
                using (SqlDataReader drImgs = cmdImgs.ExecuteReader())
                    while (drImgs.Read())
                        geoContent1001.imageids.Add((int)drImgs["ImageID"]);

            }

            Common.SendStats(context, "geocontent1001");
            Common.WriteOutput(geoContent1001, context, routeData);
        }

        public bool IsReusable { get { return false; } }
    }
}