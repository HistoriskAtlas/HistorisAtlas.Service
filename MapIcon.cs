using System;
using System.Web;
using System.Web.Routing;
using System.Web.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Data.SqlClient;
using System.Net;

namespace HistoriskAtlas.Service
{
    public class MapIconRoute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new MapIconHandler(context.RouteData);
        }
    }

    public class MapIconHandler : IHttpHandler
    {
        private RouteData routeData;

        public MapIconHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            Common.SendStats(context, "mapicon");

            int w, h, x = 0, y = 0, z = 0, mapID;
            bool clearCache = false;
            Image img;
            Bitmap bmp;
            Graphics gra;

            if (!int.TryParse(routeData.Values["id"].ToString(), out mapID))
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error! Could not parse map id: " + routeData.Values["id"].ToString());
                return;
            }

            bool.TryParse(context.Request.QueryString.Get("clearcache"), out clearCache);
            int.TryParse(context.Request.QueryString.Get("width"), out w);
            int.TryParse(context.Request.QueryString.Get("height"), out h);
            w = (w == 0 ? 256 : w);
            h = (h == 0 ? 256 : h);
            string cachePath = WebConfigurationManager.AppSettings["ImagePath"] + "Cache\\mapicons\\";
            string cacheFile = mapID +"_" + w + "x" + h + ".jpg";
            context.Response.ContentType = "image/jpeg";

            if (clearCache)
                File.Delete(cachePath + "\\" + cacheFile);
            else
            {
                try
                {
                    using (img = Image.FromFile(cachePath + "\\" + cacheFile))
                        img.Save(context.Response.OutputStream, ImageFormat.Jpeg);
                    return;
                }
                catch
                { }
            }

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT IconCoords FROM Map WHERE MapID = " + mapID, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        string iconCoords;
                        try
                        {
                            dr.Read();
                            iconCoords = dr["IconCoords"].ToString();
                        }
                        catch
                        { 
                            context.Response.ContentType = "text/plain";
                            context.Response.Write("Error! Could not find map with id: " + mapID);
                            return;
                        }
                        char[] split = { '|' };
                        try
                        {
                            int.TryParse(iconCoords.Split(split)[0], out x);
                            int.TryParse(iconCoords.Split(split)[1], out y);
                            int.TryParse(iconCoords.Split(split)[2], out z);
                        }
                        catch
                        {
                        }
                        x = (x == 0 ? 135 : x);
                        y = (y == 0 ? 80 : y);
                        z = (z == 0 ? 8 : z);
                    }
                }
            }

            using (bmp = new Bitmap(w, h, PixelFormat.Format32bppRgb))
            {
                for (int dx = 0; dx < w / 256 + 1; dx++)
                {
                    for (int dy = 0; dy < h / 256 + 1; dy++)
                    {
                        WebRequest request = WebRequest.Create("http://tile.historiskatlas.dk/" + mapID + "/" + z + "/" + (x + dx) + "/" + (y + dy) + ".jpg");
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        using (img = Image.FromStream(response.GetResponseStream()))
                        {
                            using (gra = Graphics.FromImage(bmp))
                            {
                                gra.DrawImageUnscaled(img, dx * 256, dy * 256);
                            }
                        }
                    }
                }
                bmp.Save(context.Response.OutputStream, ImageFormat.Jpeg);
                if (!Directory.Exists(cachePath))
                    Directory.CreateDirectory(cachePath);

                bmp.Save(cachePath + "\\" + cacheFile, ImageFormat.Jpeg);
            }
        }

        public bool IsReusable { get { return false; } }
    }
}