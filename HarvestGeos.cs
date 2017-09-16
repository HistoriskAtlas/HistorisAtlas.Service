using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Xml.Serialization;
using System.Collections;

namespace HistoriskAtlas.Service
{
    public class HarvestGeos : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new HarvestGeosHandler();
        }
    }

    public class HarvestGeosHandler : IHttpHandler
    {
        private Dictionary<int, string> subjects;
        private Dictionary<int, string> periods;
        private int biggestGeoID = 0, requestCount;
        private List<Geo> geos = new List<Geo>();
        private BitArray requestState = null;
        private DateTime callDateTime;
        private DateTime? requestDateTime;

        public void ProcessRequest(HttpContext context)
        {
            requestState = null;
            callDateTime = DateTime.Now;

            requestCount = string.IsNullOrEmpty(context.Request.Params["count"]) ? -1 : Int32.Parse(context.Request.Params["count"]);
            requestDateTime = string.IsNullOrEmpty(context.Request.Params["date"]) ? (DateTime?)null : DateTime.Parse(context.Request.Params["date"]);
            string state = string.IsNullOrEmpty(context.Request.Params["state"]) ? null : context.Request.Params["state"];
            if (state != null)
            {
                requestState = new BitArray(Convert.FromBase64String(state));
                biggestGeoID = requestState.Count - 1;
            }

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                if (requestState == null)
                {
                    GetTags(conn);
                    OutputAllGeos(context, conn, requestCount);
                    return;
                }

                //Deleted
                for (int i = 0; i < requestState.Length; i++)
                {
                    if (requestState[i])
                        if ((int)new SqlCommand("SELECT COUNT(*) FROM Geo WHERE GeoID = " + i, conn).ExecuteScalar() == 0)
                        {
                            geos.Add(new Geo() { ID = i, status = "Deleted" });
                            biggestGeoID = Math.Max(biggestGeoID, i);

                            if (geos.Count == requestCount)
                            {
                                Output(context);
                                return;
                            }
                        }
                }

                GetTags(conn);

                //Changed
                if (requestDateTime != null)
                {
                    var com = new SqlCommand("SELECT Geo.GeoID, GeoX, GeoY, Title, Intro FROM Geo, (SELECT DISTINCT GeoID FROM Geo_Log WHERE Type = 'Saved' AND Date > @RequestDateTime) AS GeoD WHERE Geo.GeoID = GeoD.GeoID ORDER BY GeoID", conn);
                    com.Parameters.AddWithValue("@RequestDateTime", requestDateTime);
                    using (SqlDataReader dr = com.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if ((int)dr["GeoID"] < requestState.Length)
                                if (requestState[(int)dr["GeoID"]])
                                {
                                    GetGeo(dr, conn, "Changed");
                                    if (geos.Count == requestCount)
                                    {
                                        Output(context);
                                        return;
                                    }
                                }
                        }
                    }
                }

                //Added
                using (SqlDataReader dr = new SqlCommand("SELECT GeoID, GeoX, GeoY, Title, Intro FROM Geo WHERE Online = 1 ORDER BY GeoID", conn).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        if ((int)dr["GeoID"] >= requestState.Length)
                            GetGeo(dr, conn, "Added");
                        else
                        {
                            if (!requestState[(int)dr["GeoID"]])
                                GetGeo(dr, conn, "Added");
                        }

                        if (geos.Count == requestCount)
                            break;
                    }
                }

                Output(context);
            }
        }

        private void GetTags(SqlConnection conn)
        {
            subjects = new Dictionary<int, string>();
            periods = new Dictionary<int, string>();

            using (SqlDataReader dr = new SqlCommand("SELECT TagID, Category, Plurname FROM Tag", conn).ExecuteReader())
            {
                while (dr.Read())
                {
                    switch ((byte)dr["Category"])
                    {
                        case 0:
                            subjects.Add((int)dr["TagID"], dr["Plurname"].ToString());
                            break;
                        case 1:
                            periods.Add((int)dr["TagID"], dr["Plurname"].ToString());
                            break;
                    }
                }
            }
        }

        private void OutputAllGeos(HttpContext context, SqlConnection conn, int requestCount)
        {
            using (SqlDataReader dr = new SqlCommand("SELECT " + (requestCount > -1 ? "TOP "+ requestCount + " " : "") + "GeoID, GeoX, GeoY, Title, Intro FROM Geo WHERE Online = 1 ORDER BY GeoID", conn).ExecuteReader())
                while (dr.Read())
                    GetGeo(dr, conn, "Added");

            Output(context);
        }

        private void GetGeo(SqlDataReader dr, SqlConnection conn, string status)
        {
            Geo geo = new Geo() { ID = (int)dr["GeoID"], Title = dr["Title"].ToString(), Intro = dr["Intro"].ToString() };
            LatLng ll = LatLng.FromHACoord(new HACoord((int)dr["GeoX"], (int)dr["GeoY"]));
            geo.Latitude = ll.latitude;
            geo.Longitude = ll.longitude;

            geo.Url = "http://historiskatlas.dk/" + geo.Title.Replace(' ', '_') + "_(" + geo.ID + ")";
            geo.status = status;

            using (SqlDataReader drImage = new SqlCommand("SELECT Image.ImageID, Text FROM Geo_Image, Image WHERE Geo_Image.ImageID = Image.ImageID AND GeoID = " + geo.ID, conn).ExecuteReader())
                if (drImage.Read())
                    geo.Image = new Image() { Url = "http://service.historiskatlas.dk/image/" + (int)drImage["ImageID"], Text = drImage["Text"].ToString() }; //Year = imageInfos[0].Year == 0 ? (int?)null : imageInfos[0].Year };

            using (SqlDataReader drTag = new SqlCommand("SELECT TagID FROM Tag_Geo WHERE GeoID = " + geo.ID, conn).ExecuteReader())
            {
                while (drTag.Read())
                {
                    int tagID = (int)drTag["TagID"];
                    if (subjects.ContainsKey(tagID))
                        geo.Subjects.Add(subjects[tagID]);
                    if (periods.ContainsKey(tagID))
                        geo.Periods.Add(periods[tagID]);
                }
            }

            biggestGeoID = Math.Max(biggestGeoID, geo.ID);
            geos.Add(geo);
        }

        private void Output(HttpContext context)
        {
            Result result = new Result() { Geos = geos };

            var bitA = requestState == null ? new BitArray(biggestGeoID + 1) : requestState;

            if (bitA.Length - 1 < biggestGeoID)
                bitA.Length = biggestGeoID + 1;

            foreach (Geo geo in geos)
                bitA.Set(geo.ID, geo.status != "Deleted");

            var byteA = new byte[bitA.Length / 8 + 1];

            bitA.CopyTo(byteA, 0);

            result.NextCall = "http://service.historiskatlas.dk/harvest/geos?" + (requestCount > -1 ? "count=" + requestCount + "&" : "") + "date=" + HttpUtility.UrlEncode(callDateTime.ToString()) + "&state=" + HttpUtility.UrlEncode(Convert.ToBase64String(byteA));

            context.Response.ContentType = "application/xml";
            XmlSerializer xmlSer = new XmlSerializer(result.GetType());
            xmlSer.Serialize(context.Response.OutputStream, result);
        }

        public bool IsReusable { get { return true; } }

        public class Result
        {
            public List<Geo> Geos { get; set; }
            public string NextCall { get; set; }
        }

        public class Geo
        {
            [XmlAttributeAttribute] public string status { get; set; }
            public int ID { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string Intro { get; set; }
            public Image Image { get; set; }
            public List<string> Subjects = new List<string>();
            public List<string> Periods = new List<string>();

            public bool ShouldSerializeLatitude() { return Latitude.HasValue; }
            public bool ShouldSerializeLongitude() { return Longitude.HasValue; }
            public bool ShouldSerializeSubjects() { return Subjects.Count > 0; }
            public bool ShouldSerializePeriods() { return Periods.Count > 0; }
        }

        public class Image
        {
            public string Url { get; set; }
            public string Text { get; set; }
            //public int? Year { get; set; }
        }
    }
}