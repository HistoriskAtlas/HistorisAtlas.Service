using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class Stats : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new StatsHandler();
        }
    }

    public class StatsHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            Stat stat = new Stat();

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();
                stat.contents.maps = (int)new SqlCommand("SELECT COUNT(*) AS antal FROM Map WHERE" + (context.Request.Params["publiconly"] != "false" ? " IsPublic = 1" : " 1 = 1") + (context.Request.Params["geo"] == "east" ? " AND BBRight > 184066" : ""), conn).ExecuteScalar();
                stat.contents.images = (int)new SqlCommand("SELECT COUNT(*) AS antal FROM Image" + (context.Request.Params["geo"] == "east" ? " WHERE Image.ImageID IN (SELECT ImageID FROM Geo_Image WHERE GeoID IN (SELECT GeoID FROM Geo WHERE GeoX > 184066))" : ""), conn).ExecuteScalar();
                stat.contents.videos = (int)new SqlCommand("SELECT COUNT(*) AS antal FROM Video" + (context.Request.Params["geo"] == "east" ? " WHERE Video.VideoID IN (SELECT VideoID FROM Geo_Video WHERE GeoID IN (SELECT GeoID FROM Geo WHERE GeoX > 184066))" : ""), conn).ExecuteScalar();
                stat.contents.geos = (int)new SqlCommand("SELECT COUNT(*) AS antal FROM Geo WHERE" + (context.Request.Params["publiconly"] != "false" ? " Online = 1" : " 1 = 1") + (context.Request.Params["geo"] == "east" ? " AND GeoX > 184066" : ""), conn).ExecuteScalar();
                stat.contents.readyGeos = (int)new SqlCommand("SELECT COUNT(*) AS antal FROM Geo WHERE" + (context.Request.Params["publiconly"] != "false" ? " Online = 1" : " 1 = 1") + (context.Request.Params["geo"] == "east" ? " AND GeoX > 184066" : "") + " AND GeoID IN (SELECT GeoID FROM Tag_Geo WHERE TagID = 530)", conn).ExecuteScalar();
                stat.users.institutions = (int)new SqlCommand("SELECT COUNT(*) AS antal FROM Tag WHERE Category = 3" + (context.Request.Params["geo"] == "east" ? " AND TagID IN (SELECT TagID FROM Tag_Geo WHERE GeoID IN (SELECT GeoID FROM Geo WHERE GeoX > 184066))" : ""), conn).ExecuteScalar();
                stat.users.writers = (int)new SqlCommand("SELECT COUNT(*) AS antal FROM [User] WHERE RoleLevel = 1" + (context.Request.Params["geo"] == "east" ? " AND InstitutionID IN (SELECT TagID FROM Tag_Geo WHERE GeoID IN (SELECT GeoID FROM Geo WHERE GeoX > 184066))" : ""), conn).ExecuteScalar();
                stat.users.editors = (int)new SqlCommand("SELECT COUNT(*) AS antal FROM [User] WHERE RoleLevel = 2" + (context.Request.Params["geo"] == "east" ? " AND InstitutionID IN (SELECT TagID FROM Tag_Geo WHERE GeoID IN (SELECT GeoID FROM Geo WHERE GeoX > 184066))" : ""), conn).ExecuteScalar();
             }

            Common.SendStats(context, "stats");
            Common.WriteOutput(stat, context);
        }

        public bool IsReusable { get { return false; } }
    }
}