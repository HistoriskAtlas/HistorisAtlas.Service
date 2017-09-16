using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace HistoriskAtlas.Service
{
    [DataContract]
    public class Geo
    {
        [DataMember] public int id { get; set; }
        [DataMember] public string title { get; set; }
        [DataMember] public decimal lat { get; set; }
        [DataMember] public decimal lng { get; set; }
        [DataMember] public List<int> tagids = new List<int>();
        [XmlIgnore] public double distFromCenter;
    }

    public class GeoByDistFromCenter : IComparer<Geo>
    {
        public int Compare(Geo x, Geo y)
        {
            return x.distFromCenter.CompareTo(y.distFromCenter);
        }
    }

    public class Cluster : List<Geo>
    {
        public LatLng latLng { get; set; }
    }
}
