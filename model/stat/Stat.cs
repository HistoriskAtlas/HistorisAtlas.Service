namespace HistoriskAtlas.Service
{
    public class Stat
    {
        public Contents contents = new Contents();
        public Users users = new Users();

        public class Contents
        {
            public int maps { get; set; }
            public int geos { get; set; }
            public int readyGeos { get; set; }
            public int images { get; set; }
            public int videos { get; set; }
        }

        public class Users
        {
            public int institutions { get; set; }
            public int writers { get; set; }
            public int editors { get; set; }
        }
    }
}