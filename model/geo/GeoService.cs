using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Globalization;

namespace HistoriskAtlas.Service
{
    public class Geos : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new GeoHandler();
        }
    }

    public class GeoHandler : IHttpHandler
    {
        private LatLng center = null;
        private int count = -1; //all
        private int z = -1;
        private bool clustering = false;
        private LatLngBox llBox = null; //all
        private List<string> sources;
        private List<Geo> geos = new List<Geo>();

        public void ProcessRequest(HttpContext context)
        {
            GetParams(context);

            if (sources.Contains("HA"))
                GetHAGeos(context, sources.Count > 1);

            if (sources.Contains("1001"))
                GetExternalGeos("1001", 388, sources.Count > 1);

            if (sources.Contains("DSFL"))
                GetExternalGeos("DSFL", 390, sources.Count > 1);

            if (sources.Count > 1)
            {
                geos.Sort(new GeoByDistFromCenter());

                if (count > -1)
                    if (geos.Count > count)
                        geos = geos.GetRange(0, count);
            }

            if (clustering)
                Clustering();

            Common.SendStats(context, "geos");
            Common.WriteOutput(geos, context);
        }

        private void GetParams(HttpContext context)
        {
            if (context.Request.Params["center"] != null)
                center = LatLng.FromString(context.Request.Params["center"]);

            if (context.Request.Params["count"] != null)
                Int32.TryParse(context.Request.Params["count"], out count);

            if (context.Request.Params["z"] != null)
                Int32.TryParse(context.Request.Params["z"], out z);

            if (context.Request.Params["clustering"] != null)
                bool.TryParse(context.Request.Params["clustering"], out clustering);

            if (context.Request.Params["span"] != null && center != null)
                llBox = new LatLngBox(center, LatLng.FromString(context.Request.Params["span"]));

            if (!string.IsNullOrEmpty(context.Request.Params["source"]))
                sources = new List<string>(context.Request.Params["source"].ToUpper().Split(new char[] { ',' }));
            else
                sources = new List<string>() { "HA" };
        }

        private void GetHAGeos(HttpContext context, bool needDistance)
        {
            int tagscategory = -1; //all
            if (context.Request.Params["tagscategory"] != null)
                Int32.TryParse(context.Request.Params["tagscategory"], out tagscategory);

            string sqlTagSearch = "";
            if (!string.IsNullOrEmpty(context.Request.Params["includetags"]))
            {
                List<string> includeTags = new List<string>(context.Request.Params["includetags"].Split(new char[] { ',' }));
                sqlTagSearch = " AND GeoID IN (SELECT GeoID FROM Tag_Geo WHERE TagID IN (" + string.Join(",", includeTags) + "))";
            }

            string sqlOrderBy = "";
            if (center != null && llBox == null)
            {
                HACoord coord = HACoord.FromLatLng(center);
                sqlOrderBy = " ORDER BY POWER((GeoX - " + coord.x + ") / 10000.0, 2) + POWER((GeoY - " + coord.y + ") / 10000.0, 2)";
            }

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                var cmd = new SqlCommand("SELECT" + (count > -1 ? " TOP " + count : "") + " GeoID, Title, GeoX, GeoY FROM Geo WHERE Online = 1" + (llBox == null ? "" : " AND GeoX > " + HACoord.FromLatLng(llBox.llLatLng).x + " AND GeoX < " + HACoord.FromLatLng(llBox.urLatLng).x + " AND GeoY > " + HACoord.FromLatLng(llBox.urLatLng).y + " AND GeoY < " + HACoord.FromLatLng(llBox.llLatLng).y) + sqlTagSearch + sqlOrderBy, conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        LatLng ll = LatLng.FromHACoord(new HACoord((int)dr["GeoX"], (int)dr["GeoY"]));
                        Geo geo = new Geo() { id = (int)dr["GeoID"], title = dr["Title"].ToString(), lat = ll.latitude, lng = ll.longitude };

                        if (center != null && needDistance)
                            geo.distFromCenter = ll.Distance(center);

                        SqlCommand cmdTags;
                        if (tagscategory == -1)
                            cmdTags = new SqlCommand("SELECT TagID FROM Tag_Geo WHERE GeoID = " + geo.id, conn);
                        else
                            cmdTags = new SqlCommand("SELECT Tag_Geo.TagID FROM Tag_Geo, Tag WHERE Tag_Geo.TagID = Tag.TagID AND Tag.Category = " + tagscategory + " AND GeoID = " + geo.id, conn);

                        using (SqlDataReader drTags = cmdTags.ExecuteReader())
                            while (drTags.Read())
                                geo.tagids.Add((int)drTags["TagID"]);

                        geos.Add(geo);
                    }
                }
            }
        }

        private void GetExternalGeos(string source, int tagid, bool needDistance)
        {
            string sqlOrderBy = (center != null && llBox == null) ? " ORDER BY POWER((Lat - " + center.latitude.ToString(CultureInfo.InvariantCulture) + ") / 10000.0, 2) + POWER((Lng - " + center.longitude.ToString(CultureInfo.InvariantCulture) + ") / 10000.0, 2)" : "";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();
                
                var cmd = new SqlCommand("SELECT" + (count > -1 ? " TOP " + count : "") + " LocalID, Title, Lat, Lng FROM Geo_External_" + source + (llBox == null ? "" : " WHERE Lat > " + llBox.llLatLng.latitude.ToString(CultureInfo.InvariantCulture) + " AND Lat < " + llBox.urLatLng.latitude.ToString(CultureInfo.InvariantCulture) + " AND Lng > " + llBox.llLatLng.longitude.ToString(CultureInfo.InvariantCulture) + " AND Lng < " + llBox.urLatLng.longitude.ToString(CultureInfo.InvariantCulture)) + sqlOrderBy, conn);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        LatLng ll = new LatLng((decimal)dr["Lat"], (decimal)dr["Lng"]);
                        Geo geo = new Geo() { id = (int)dr["LocalID"], title = dr["Title"].ToString(), lat = ll.latitude, lng = ll.longitude };
                        
                        if (center != null && needDistance)
                            geo.distFromCenter = ll.Distance(center);

                        geo.tagids.Add(tagid);
                        
                        geos.Add(geo);
                    }
                }
            }
        }


        private void Clustering()
        {
            List<Cluster> clusters = new List<Cluster>();
            while (geos.Count > 0)
            {
                Geo geo = geos[0];
                geos.RemoveAt(0);
                Cluster closesetCluster = null;
                double closesetDistance = double.MaxValue;
                foreach (Cluster cluster in clusters)
                {
                    double distance = cluster.latLng.PixelDistance(new LatLng(geo.lat, geo.lng), z);
                    if (distance < closesetDistance)
                    {
                        closesetCluster = cluster;
                        closesetDistance = distance;
                    }
                }

                if (closesetDistance > 20)
                {
                    Cluster cluster = new Cluster();
                    cluster.latLng = new LatLng(geo.lat, geo.lng);
                    clusters.Add(cluster);
                    cluster.Add(geo);
                }
                else
                {
                    closesetCluster.Add(geo);
                    closesetCluster.latLng = new LatLng(0, 0);
                    foreach (Geo cGeo in closesetCluster)
                        closesetCluster.latLng += new LatLng(cGeo.lat, cGeo.lng);
                    closesetCluster.latLng /= closesetCluster.Count;
                }
            }

            foreach (Cluster cluster in clusters)
            {
                if (cluster.Count == 1)
                    geos.Add(cluster[0]);
                else
                {
                    Geo geo = new Geo() { id = -cluster.Count };
                    LatLng cLatLng = new LatLng(0, 0);
                    foreach (Geo cGeo in cluster)
                    {
                        cLatLng += new LatLng(cGeo.lat, cGeo.lng);
                        foreach (int tagid in cGeo.tagids)
                            if (!geo.tagids.Contains(tagid))
                                geo.tagids.Add(tagid);
                    }
                    cLatLng /= cluster.Count;
                    geo.lat = cLatLng.latitude;
                    geo.lng = cLatLng.longitude;
                    geos.Add(geo);
                }
            }
        }

        public bool IsReusable { get { return false; } }
    }
}