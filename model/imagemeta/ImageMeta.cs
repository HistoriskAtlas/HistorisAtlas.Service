using System.Collections.Generic;

namespace HistoriskAtlas.Service
{
    public class ImageMeta
    {
        public int id { get; set; }
        public string text { get; set; }
        public string credit { get; set; }
        public string photographer { get; set; }
        public string licensee { get; set; }
        public int? year { get; set; }
        public List<int> tagids = new List<int>();
    }
}
