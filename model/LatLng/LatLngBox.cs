using System.Runtime.Serialization;

namespace HistoriskAtlas.Service
{
    [DataContract]
    public class LatLngBox
    {
        [DataMember] internal LatLng llLatLng;
        [DataMember] internal LatLng urLatLng;

        public LatLngBox(LatLng ll1, LatLng ll2, bool centerSpan = true)
        {
            if (centerSpan)
            {
                this.llLatLng = ll1 - ll2 / 2;
                this.urLatLng = ll1 + ll2 / 2;
            }
            else
            {
                this.llLatLng = ll1;
                this.urLatLng = ll2;
            }
        }
    }
}
