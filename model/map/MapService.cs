using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class Maps : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new MapsHandler(context.RouteData);
        }
    }

    public class MapsHandler : IHttpHandler
    {
        RouteData routeData;

        public MapsHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            //TODO: if (verb = GET)?!
            List<Map> maps;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();
                bool publicAccess = (context.Request.Params["public"] != "false"); //default true
                bool includeStandard = (context.Request.Params["standard"] == "true"); //default false

                SqlCommand cmd = new SqlCommand("SELECT * FROM Map WHERE ReplacedBy = 0" + (publicAccess ? " AND IsPublic = 1 " : " ") + "ORDER BY OrgYear", conn);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    maps = new List<Map>();
                    while (dr.Read())
                    {
                        if (!includeStandard && Map.stdMapIds.Contains((int)dr["MapID"]))
                            continue;

                        Map map = new Map() { id = (int)dr["MapID"], title = dr["Name"].ToString(), year = (int)dr["OrgYear"], textID = (int)dr["TextID"] };
                        if (!(dr["BBMaxZ"] is DBNull)) map.BBMaxZ = (byte)dr["BBMaxZ"];
                        if (!(dr["BBMinZ"] is DBNull)) map.BBMinZ = (byte)dr["BBMinZ"];
                        if (!(dr["BBLeft"] is DBNull)) map.BBLeft = (int)dr["BBLeft"];
                        if (!(dr["BBRight"] is DBNull)) map.BBRight = (int)dr["BBRight"];
                        if (!(dr["BBTop"] is DBNull)) map.BBTop = (int)dr["BBTop"];
                        if (!(dr["BBBottom"] is DBNull)) map.BBBottom = (int)dr["BBBottom"];
                        if (!(dr["MinLat"] is DBNull)) map.boundryBox = new LatLngBox(new LatLng((decimal)dr["MinLat"], (decimal)dr["MinLon"]), new LatLng((decimal)dr["MaxLat"], (decimal)dr["MaxLon"]), false);

                        //if (dr["IconXYZ"] is DBNull)
                        //{
                        //    int z = Math.Min(map.BBMinZ + 2, 12);
                        //    int x = map.BBMaxZ > 0 ? (int)(((map.BBRight + map.BBLeft) / 2.0) / Math.Pow(2.0, z)) / 256 : 0;
                        //    int y = map.BBMaxZ > 0 ? (int)(((map.BBBottom + map.BBTop) / 2.0) / Math.Pow(2.0, z)) / 256 : 0;
                        //    map.iconXYZ = x + "|" + y + "|" + z;
                        //}
                        //else
                        //    map.iconXYZ = dr["IconXYZ"].ToString();

                        maps.Add(map);
                    }
                }
            }

            Common.SendStats(context, "maps");
            Common.WriteOutput(maps, context, routeData);
        }

        public bool IsReusable { get { return false; } }
    }
}

//using System;
//using System.ServiceModel;
//using System.ServiceModel.Activation;
//using System.ServiceModel.Web;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Web.Configuration;

//namespace HistoriskAtlas.Service
//{
//    [ServiceContract]
//    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
//    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]

//    public class MapService
//    {

        
        
        
//        //TOOD: Better way to make a REST proxy..... with JsonP support!?




//        [WebGet(UriTemplate = "")]
//        public List<Map> GetCollection()
//        {
//            List<Map> maps;

//            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
//            {
//                conn.Open();
//                SqlCommand cmd = new SqlCommand("SELECT * FROM Map WHERE IsPublic = 1 ORDER BY OrgYear", conn);
//                using (SqlDataReader dr = cmd.ExecuteReader())
//                {
//                    maps = new List<Map>();
//                    while (dr.Read())
//                    {
//                        Map map = new Map() { id = (int)dr["MapID"], title = dr["Name"].ToString(), year = (int)dr["OrgYear"] };
//                        if (!(dr["BBMaxZ"] is DBNull)) map.BBMaxZ = (byte)dr["BBMaxZ"];
//                        if (!(dr["BBMinZ"] is DBNull)) map.BBMinZ = (byte)dr["BBMinZ"];
//                        if (!(dr["BBLeft"] is DBNull)) map.BBLeft = (int)dr["BBLeft"];
//                        if (!(dr["BBRight"] is DBNull)) map.BBRight = (int)dr["BBRight"];
//                        if (!(dr["BBTop"] is DBNull)) map.BBTop = (int)dr["BBTop"];
//                        if (!(dr["BBBottom"] is DBNull)) map.BBBottom = (int)dr["BBBottom"];
                        
//                        if (dr["IconXYZ"] is DBNull)
//                        {
//                            int z = Math.Min(map.BBMinZ + 2, 12);
//                            int x = map.BBMaxZ > 0 ? (int)(((map.BBRight + map.BBLeft) / 2.0) / Math.Pow(2.0, z)) / 256 : 0;
//                            int y = map.BBMaxZ > 0 ? (int)(((map.BBBottom + map.BBTop) / 2.0) / Math.Pow(2.0, z)) / 256 : 0;
//                            map.iconXYZ = x + "|" + y + "|" + z;
//                        }
//                        else
//                            map.iconXYZ = dr["IconXYZ"].ToString();

//                        maps.Add(map);
//                    }
//                }
//            }

//            return maps;
//        }

//        [WebInvoke(UriTemplate = "", Method = "POST")]
//        public Map Create(Map instance)
//        {
//            // TODO: Add the new instance of Map to the collection
//            throw new NotImplementedException();
//        }

//        [WebGet(UriTemplate = "{id}")]
//        public Map Get(string id)
//        {
//            // TODO: Return the instance of Map with the given id
//            throw new NotImplementedException();
//        }

//        [WebInvoke(UriTemplate = "{id}", Method = "PUT")]
//        public Map Update(string id, Map instance)
//        {
//            // TODO: Update the given instance of Map in the collection
//            throw new NotImplementedException();
//        }

//        [WebInvoke(UriTemplate = "{id}", Method = "DELETE")]
//        public void Delete(string id)
//        {
//            // TODO: Remove the instance of Map with the given id from the collection
//            throw new NotImplementedException();
//        }

//    }
//}
