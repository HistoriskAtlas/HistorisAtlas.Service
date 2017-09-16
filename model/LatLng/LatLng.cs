using System.Runtime.Serialization;
using System.Globalization;
using System;
using System.Drawing;

namespace HistoriskAtlas.Service
{
    [DataContract]
    public class LatLng
    {
        private const double OFFSET = 268435456;
        private const double RADIUS = 85445659.4471;

        [DataMember] internal decimal latitude;
        [DataMember] internal decimal longitude;

        public LatLng(decimal latitude, decimal longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public double Distance(LatLng ll)
        {
            LatLng diff = this - ll;
            return Math.Sqrt(Math.Pow((double)diff.latitude, 2) + Math.Pow((double)diff.longitude, 2));
        }

        public Point ToPixelCoord()
        {
            return new Point(
                (int)(OFFSET - RADIUS * Math.Log((1 + Math.Sin((double)latitude * Math.PI / 180)) / (1 - Math.Sin((double)latitude * Math.PI / 180))) / 2),
                (int)(OFFSET + RADIUS * (double)longitude * Math.PI / 180)
                );
        }
        
        public double PixelDistance(LatLng ll, int z)
        {
            Point p1 = this.ToPixelCoord();
            Point p2 = ll.ToPixelCoord();
            return (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) >> (21 - z);
        }

        public static LatLng operator -(LatLng ll1, LatLng ll2) { return new LatLng(ll1.latitude - ll2.latitude, ll1.longitude - ll2.longitude); }
        public static LatLng operator +(LatLng ll1, LatLng ll2) { return new LatLng(ll1.latitude + ll2.latitude, ll1.longitude + ll2.longitude); }
        public static LatLng operator /(LatLng ll, decimal div) { return new LatLng(ll.latitude / div, ll.longitude / div); }
        public static LatLng operator *(LatLng ll, decimal div) { return new LatLng(ll.latitude * div, ll.longitude * div); }

        public static LatLng FromString(string latLng)
        {
            string[] latLngArray = latLng.Split(new char[] { ',' });
            return new LatLng(decimal.Parse(latLngArray[0], CultureInfo.InvariantCulture), decimal.Parse(latLngArray[1], CultureInfo.InvariantCulture));
        }

        public static LatLng FromHACoord(HACoord HACoord)
        {
            double x = HACoord.doubleX / 5.0 + 588086.0;
            double y = -HACoord.doubleY / 5.0 + 6140036.0;
            GPS.GPS.UTM utm = new GPS.GPS.UTM(y, x, 32);
            return new LatLng(utm.Latitude.GetDecimalCoordinate(), utm.Longitude.GetDecimalCoordinate());
        }


    }
}
