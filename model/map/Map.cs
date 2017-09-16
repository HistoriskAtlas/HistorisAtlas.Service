using System.Runtime.Serialization;
using System.Globalization;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace HistoriskAtlas.Service
{
    [DataContract]
    public class Map
    {
        public static List<int> stdMapIds = new List<int> { 13, 33, 15, 34, 37, 51, 61 }; //85

        [DataMember] public int id { get; set; }
        [DataMember] public string title { get; set; }
        [DataMember] public int textID { get; set; }
        [DataMember] public int year { get; set; }
        //public string iconXYZ { get; set; }
        [DataMember] public int BBMinZ { get; set; } //mobile dosnt use
        [DataMember] public int BBMaxZ { get; set; } //mobile dosnt use
        [DataMember] public int BBLeft { get; set; } //mobile dosnt use
        [DataMember] public int BBRight { get; set; } //mobile dosnt use
        [DataMember] public int BBTop { get; set; } //mobile dosnt use
        [DataMember] public int BBBottom { get; set; } //mobile dosnt use
        [XmlIgnore] public LatLngBox boundryBox{ get; set; }
        [DataMember] public decimal? centerLat { get {
            return boundryBox == null ? (decimal?)null : (boundryBox.llLatLng.latitude + boundryBox.urLatLng.latitude) / 2;
        } set{} }
        [DataMember] public decimal? centerLng { get {
            return boundryBox == null ? (decimal?)null : (boundryBox.llLatLng.longitude + boundryBox.urLatLng.longitude) / 2;
        } set{} }
        [DataMember] public decimal? spanLat { get {
            return boundryBox == null ? (decimal?)null : boundryBox.urLatLng.latitude - boundryBox.llLatLng.latitude;
        } set{} }
        [DataMember] public decimal? spanLng { get {
            return boundryBox == null ? (decimal?)null : boundryBox.urLatLng.longitude - boundryBox.llLatLng.longitude;
        } set{} }
    }
}
