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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HistoriskAtlas.Service
{
    public class ImageRoute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new ImageHandler(context.RouteData);
        }
    }

    public class ImageHandler : IHttpHandler
    {
        private RouteData routeData;

        public ImageHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            Common.SendStats(context, "image");
            
            Image img;
            int w, h, a, squareCrop, nW, nH, imageID;
            bool smartCrop;
            Bitmap bmp;
            Graphics gra;

            if (!int.TryParse(routeData.Values["id"].ToString(), out imageID))
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Error! Could not parse image id: " + routeData.Values["id"].ToString());
                return;
            }
            int.TryParse(context.Request.QueryString.Get("width"), out w);
            int.TryParse(context.Request.QueryString.Get("height"), out h);
            int.TryParse(context.Request.QueryString.Get("any"), out a);
            bool.TryParse(context.Request.QueryString.Get("smartCrop"), out smartCrop);
            if (context.Request.QueryString.Get("squareCrop") == null)
                squareCrop = -1;
            else
                int.TryParse(context.Request.QueryString.Get("squareCrop"), out squareCrop);
            string cachePath = WebConfigurationManager.AppSettings["ImagePath"] + "Cache\\" + (imageID % 100);
            string cacheFile = imageID + "_" + w + "x" + h + (squareCrop > -1 ? "_sq" + squareCrop : "") + (a > 0 ? "_any" + a : "") + ".jpg";
            context.Response.ContentType = "image/jpeg";
            if (context.Request.QueryString["download"] != null)
                context.Response.AddHeader("Content-disposition", "attachment; filename=HA" + imageID + ".jpg");

            try
            {
                using (img = Image.FromFile(cachePath + "\\" + cacheFile))
                    img.Save(context.Response.OutputStream, ImageFormat.Jpeg);
                return;
            }
            catch
            { }

            //try
            //{
                using (img = GetImage(imageID))
                {
                    if (h == 0 && w == 0 && a == 0 && squareCrop == -1)
                    {
                        img.Save(context.Response.OutputStream, ImageFormat.Jpeg);
                        return;
                    }

                    if (a > 0)
                    {
                        if (img.Height < img.Width)
                            h = a;
                        else
                            w = a;
                    }

                    if (h == 0)
                        h = (int)(((double)img.Height / (double)img.Width) * (double)w);

                    if (w == 0)
                        w = (int)(((double)img.Width / (double)img.Height) * (double)h);

                    nW = squareCrop == -1 ? w : w > h ? h : w;
                    nH = squareCrop == -1 ? h : nW;

                    using (bmp = new Bitmap(nW, nH, PixelFormat.Format32bppRgb))
                    {
                        if (smartCrop)
                        {
                            SmartCrop(nW, nH, img);
                        }
                        else
                            using (gra = Graphics.FromImage(bmp))
                            {
                                gra.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                gra.SmoothingMode = SmoothingMode.HighQuality;
                                gra.CompositingQuality = CompositingQuality.HighQuality;
                                if (squareCrop == -1)
                                    gra.DrawImage(img, 0, 0, w, h);
                                else
                                {
                                    if (w > h)
                                        gra.DrawImage(img, (int)(-((double)w / (double)img.Width) * squareCrop), 0, w, h);
                                    else
                                        gra.DrawImage(img, 0, (int)(-((double)h / (double)img.Height) * squareCrop), w, h);
                                }
                            }
                        bmp.Save(context.Response.OutputStream, ImageFormat.Jpeg);

                        if (!Directory.Exists(cachePath))
                            Directory.CreateDirectory(cachePath);

                        try
                        {
                            bmp.Save(cachePath + "\\" + cacheFile, ImageFormat.Jpeg);
                        }
                        catch
                        {
                        }
                    }
                }
            //}
            //catch
            //{
            //    context.Response.ContentType = "text/plain";
            //    context.Response.Write("Error! Could not find image with id: " + imageID);
            //    return;
            //}
        }

        private Image GetImage(int imageID)
        {
            if (Common.IsId1001(imageID))
            {
                string url;

                using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT Uri FROM Image_External_1001 WHERE ImageID = " + imageID, conn);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                           url = dr["Uri"].ToString();
                        else
                            return null;
                    }
                }

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    return Image.FromStream(response.GetResponseStream());
            }

            return Image.FromFile(WebConfigurationManager.AppSettings["ImagePath"] + (imageID % 100) + "\\" + imageID + ".jpg");
        }

        private Bitmap SmartCrop(int destWidth, int destHeight, Image srcImg)
        {
            bool horz = destWidth / destHeight > srcImg.Width / srcImg.Height;
            List<int> lowestPath = new List<int>();
            long lowestDiff = long.MaxValue;

            //using (Bitmap srcBmp = new Bitmap(srcImg.Width, srcImg.Height, PixelFormat.Format24bppRgb))
            using (Bitmap srcBmp = new Bitmap(srcImg))
            {
                //using (Graphics gra = Graphics.FromImage(srcBmp))
                //    gra.DrawImageUnscaled(srcImg, 0, 0);

                BitmapData data = srcBmp.LockBits(new Rectangle(0, 0, srcBmp.Width, srcBmp.Height), ImageLockMode.ReadWrite, srcBmp.PixelFormat);

                int numBytes = data.Stride * srcBmp.Height;
                byte[] rgbValues = new byte[numBytes];
                Marshal.Copy(data.Scan0, rgbValues, 0, numBytes);
                int bytesPerPixel = Image.GetPixelFormatSize(srcBmp.PixelFormat) / 8;
                int maxI = horz ? srcImg.Width : srcImg.Height;

                while (maxI > 1000)
                {

                    for (int i = 0; i < maxI; i++)
                    {
                        int x = horz ? i : 0;
                        int y = horz ? 0 : i;
                        List<int> path = new List<int>() { i };
                        long diff = 0;


                        for (int j = 0; j < (horz ? srcImg.Height : srcImg.Width) - 1; j++)
                        {
                            int displace;
                            int tempDiff;
                            int diff1 = PixelDiff(rgbValues, bytesPerPixel, maxI, horz, srcBmp, x, y, horz ? x - 1 : x + 1, horz ? y + 1 : y - 1);
                            int diff2 = PixelDiff(rgbValues, bytesPerPixel, maxI, horz, srcBmp, x, y, horz ? x : x + 1, horz ? y + 1 : y);

                            displace = diff1 < diff2 ? -1 : 0;
                            tempDiff = diff1 < diff2 ? diff1 : diff2;

                            int diff3 = PixelDiff(rgbValues, bytesPerPixel, maxI, horz, srcBmp, x, y, horz ? x + 1 : x + 1, horz ? y + 1 : y + 1);

                            displace = diff3 < tempDiff ? 1 : displace;
                            tempDiff = diff3 < tempDiff ? diff3 : tempDiff;

                            diff += tempDiff;
                            if (diff > lowestDiff)
                                break;

                            x += horz ? displace : 1; 
                            y += horz ? 1 : displace;

                            path.Add(horz ? x : y);
                        }

                        if (diff < lowestDiff)
                        {
                            lowestDiff = diff;
                            lowestPath = path;
                        }
                    }

                    //Shrink
                    for (int i = 0; i < (horz ? srcImg.Width : srcImg.Height) - 1; i++)
                    {
                        int x = horz ? i : 0;
                        int y = horz ? 0 : i;

                        for (int j = 0; j < (horz ? srcImg.Height : srcImg.Width); j++)
                        {
                            //TODO: not taking horz into account:
                            if (x >= lowestPath[y])
                            {
                                int index = (x + y * srcBmp.Width) * bytesPerPixel;
                                rgbValues[index] = rgbValues[index + bytesPerPixel];
                                rgbValues[index + 1] = rgbValues[index + bytesPerPixel + 1];
                                rgbValues[index + 2] = rgbValues[index + bytesPerPixel + 2];
                            }

                            if (horz)
                                y++;
                            else
                                x++;
                        }
                    }
                    maxI--;
                }

                Marshal.Copy(rgbValues, 0, data.Scan0, numBytes);
                srcBmp.UnlockBits(data);


                //TODO: TEST
                srcBmp.Save(@"c:\obm\8\test.jpg", ImageFormat.Jpeg);


                return srcBmp;
            }

        }

        private int PixelDiff(byte[] rgbValues, int bytesPerPixel, int maxI, bool horz, Bitmap bmp, int x1, int y1, int x2, int y2)
        {
            if (horz ? x2 > maxI - 1 : y2 > maxI - 1)
                return int.MaxValue;

            if (x2 < 0 || x2 > bmp.Width - 1)
                return int.MaxValue;

            if (y2 < 0 || y2 > bmp.Height - 1)
                return int.MaxValue;

            
            
            //TODO: Reading wrong?!


            //byte r1 = Marshal.ReadByte(data.Scan0, x1 * 4 + y1 * data.Stride);
            //byte g1 = Marshal.ReadByte(data.Scan0, x1 * 4 + y1 * data.Stride + 1);
            //byte b1 = Marshal.ReadByte(data.Scan0, x1 * 4 + y1 * data.Stride + 2);
            //byte r2 = Marshal.ReadByte(data.Scan0, x2 * 4 + y2 * data.Stride);
            //byte g2 = Marshal.ReadByte(data.Scan0, x2 * 4 + y2 * data.Stride + 1);
            //byte b2 = Marshal.ReadByte(data.Scan0, x2 * 4 + y2 * data.Stride + 2);
            int i = (x1 + y1 * bmp.Width) * bytesPerPixel;
            byte r1 = rgbValues[i];
            byte g1 = rgbValues[i + 1];
            byte b1 = rgbValues[i + 2];
            i = (x2 + y2 * bmp.Width) * bytesPerPixel;
            byte r2 = rgbValues[i];
            byte g2 = rgbValues[i + 1];
            byte b2 = rgbValues[i + 2];
            return Math.Abs(r1 - r2) + Math.Abs(g1 - g2) + Math.Abs(b1 - b2);
        }

        public bool IsReusable { get { return false; } }
    }
}