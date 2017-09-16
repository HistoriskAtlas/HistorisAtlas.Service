namespace HistoriskAtlas.Service
{
    public class Tag
    {
        public int id { get; set; }
        public string plurName { get; set; }
        public string singName { get; set; }
        public byte category { get; set; }
        public int? yearStart { get; set; }
        public int? yearEnd { get; set; }
    }
}
