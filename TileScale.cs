using System;
using System.Web;
using System.Web.Routing;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace HistoriskAtlas.Service
{
    public class TileScaleRoute : IRouteHandler
    {
        public static ImageCodecInfo jpgEncoder;
        public static EncoderParameters myEncoderParameters;

        public TileScaleRoute()
        {
            jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            Encoder myEncoder = Encoder.Quality;
            myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 85L);
            myEncoderParameters.Param[0] = myEncoderParameter;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
                if (codec.FormatID == format.Guid)
                    return codec;

            return null;
        }

        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new TileScaleHandler();
        }
    }

    public class TileScaleHandler : IHttpHandler
    {
        const int pixelOversize = 2;

        public void ProcessRequest(HttpContext context)
        {
            using (Image img = Image.FromStream(context.Request.InputStream))
            {
                using (Bitmap bmp = new Bitmap(256, 256))
                using (Graphics gra = Graphics.FromImage(bmp))
                {
                    gra.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gra.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    gra.CompositingQuality = CompositingQuality.HighQuality;
                    gra.PixelOffsetMode = PixelOffsetMode.Half;
                    gra.CompositingMode = CompositingMode.SourceCopy;
                    gra.DrawImage(img, -pixelOversize, -pixelOversize, 256 + pixelOversize * 2, 256 + pixelOversize * 2);

                    context.Response.ContentType = "image/jpeg";
                    bmp.Save(context.Response.OutputStream, TileScaleRoute.jpgEncoder, TileScaleRoute.myEncoderParameters);
                }
            }
        }

        public bool IsReusable { get { return false; } }
    }
}