using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Globalization;

namespace HistoriskAtlas.Service
{
    public class RelevantMaps : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new RelevantMapsHandler();
        }
    }

    public class RelevantMapsHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            List<int> mapIDs = new List<int>();
            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();
                bool publicAccess = (context.Request.Params["public"] != "false"); //default true
                LatLng center = LatLng.FromString(context.Request.Params["center"]);
                LatLng span = LatLng.FromString(context.Request.Params["span"]);
                int z = int.Parse(context.Request.Params["z"]);
                bool includeStandard = (context.Request.Params["standard"] == "true"); //default false

                string sql = "SELECT MapID FROM Map WHERE ReplacedBy = 0" + (publicAccess ? " AND IsPublic = 1 " : " ") + "AND MinLat < " + (center.latitude + span.latitude / 2).ToString(CultureInfo.InvariantCulture) + " AND MaxLat > " + (center.latitude - span.latitude / 2).ToString(CultureInfo.InvariantCulture) + " AND MinLon < " + (center.longitude + span.longitude / 2).ToString(CultureInfo.InvariantCulture) + " AND MaxLon > " + (center.longitude - span.longitude / 2).ToString(CultureInfo.InvariantCulture) + " AND MinZ <= " + z + " AND MaxZ >= " + z + " ORDER BY OrgYear";

                SqlCommand cmd = new SqlCommand(sql, conn);
                using (SqlDataReader dr = cmd.ExecuteReader())
                    while (dr.Read())
                        if (!includeStandard && !Map.stdMapIds.Contains((int)dr["MapID"]))
                            mapIDs.Add((int)dr["MapID"]);
            }

            Common.SendStats(context, "relevantmaps");
            Common.WriteOutput(mapIDs, context);
        }

        public bool IsReusable { get { return false; } }
    }
}