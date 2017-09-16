using System.Runtime.Serialization;

namespace HistoriskAtlas.Service
{
    [DataContract]
    public class HACoord
    {
        [DataMember] internal decimal x;
        [DataMember] internal decimal y;

        public HACoord(decimal x, decimal y)
        {
            this.x = x;
            this.y = y;
        }

        public static HACoord FromLatLng(LatLng ll)
        {
            GPS.GPS.UTM utm = new GPS.GPS.UTM(new GPS.GPS.Coordinate(ll.latitude, GPS.GPS.CoordinateType.Latitude), new GPS.GPS.Coordinate(ll.longitude, GPS.GPS.CoordinateType.Longitude));
            return new HACoord((int)(utm.Easting - 588086) * 5, -(int)(utm.Northing - 6140036) * 5);
        }

        public double doubleX { get { return (double)x; } }
        public double doubleY { get { return (double)y; } }

    }
}
