using System.Collections.Generic;

namespace HistoriskAtlas.Service
{
    public class GeoContent : Geo
    {
        public string intro { get; set; }
        public List<GeoContentText> texts = new List<GeoContentText>();
        public List<int> imageids = new List<int>();
        public List<int> videoids = new List<int>();
        public List<int> pdfids = new List<int>();
        public List<GeoContentExt> exts = new List<GeoContentExt>();
        public string license { get; set; } //TODO: set for HA Geos
    }

    public class GeoContentText
    {
        public int ordering { get; set; }
        public string headline { get; set; }
    }

    public class GeoContentExt
    {
        public byte source { get; set; }
        public string link { get; set; }
    }
}
