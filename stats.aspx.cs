using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HistoriskAtlas.Service
{
    public partial class stats : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SqlDataAdapter ada;
            DataTable DT;

            string whereUserAgent = Request.QueryString["UserAgent"] != null ? " AND UserAgent = '" + Request.QueryString["UserAgent"] + "'" : "";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["hadbStats"].ConnectionString))
            {
                con.Open();

                using (ada = new SqlDataAdapter("SELECT SUM(CacheCount + NotModifiedCount + GenerateCount) AS antal FROM StatsTile", con))
                {
                    DT = new DataTable();
                    ada.Fill(DT);
                    (FindControl("TilesLabel") as Label).Text = Common.PrettyInteger(DT.Rows[0][0].ToString());
                }

                using (ada = new SqlDataAdapter("SELECT SUM(Count) AS antal FROM StatsUserAgent WHERE Event = 'image'" + whereUserAgent, con))
                {
                    DT = new DataTable();
                    ada.Fill(DT);
                    (FindControl("ImagesLabel") as Label).Text = Common.PrettyInteger(DT.Rows[0][0].ToString());
                }

                using (ada = new SqlDataAdapter("SELECT SUM(Count) AS antal FROM StatsUserAgent WHERE Event = 'imagemeta'" + whereUserAgent, con))
                {
                    DT = new DataTable();
                    ada.Fill(DT);
                    (FindControl("ImagesMetaLabel") as Label).Text = Common.PrettyInteger(DT.Rows[0][0].ToString());
                }

                using (ada = new SqlDataAdapter("SELECT SUM(Count) AS antal FROM StatsUserAgent WHERE Event = 'geos'" + whereUserAgent, con))
                {
                    DT = new DataTable();
                    ada.Fill(DT);
                    (FindControl("GeosLabel") as Label).Text = Common.PrettyInteger(DT.Rows[0][0].ToString());
                }

                using (ada = new SqlDataAdapter("SELECT SUM(Count) AS antal FROM StatsUserAgent WHERE Event = 'geocontent'" + whereUserAgent, con))
                {
                    DT = new DataTable();
                    ada.Fill(DT);
                    (FindControl("GeoContentLabel") as Label).Text = Common.PrettyInteger(DT.Rows[0][0].ToString());
                }

                using (ada = new SqlDataAdapter("SELECT SUM(Count) AS antal FROM StatsUserAgent WHERE Event = 'maps'" + whereUserAgent, con))
                {
                    DT = new DataTable();
                    ada.Fill(DT);
                    (FindControl("MapsLabel") as Label).Text = Common.PrettyInteger(DT.Rows[0][0].ToString());
                }

                using (ada = new SqlDataAdapter("SELECT SUM(Count) AS antal FROM StatsUserAgent WHERE Event = 'tags'" + whereUserAgent, con))
                {
                    DT = new DataTable();
                    ada.Fill(DT);
                    (FindControl("TagsLabel") as Label).Text = Common.PrettyInteger(DT.Rows[0][0].ToString());
                }
            }
        }

        public string GetForeground()
        {
            return Context.Request.Params["foreground"] == null ? "000000" : Context.Request.Params["foreground"];
        }

        public string GetBackground()
        {
            return Context.Request.Params["background"] == null ? "ffffff" : Context.Request.Params["background"];
        }

        public string GetFontSize()
        {
            return Context.Request.Params["fontsize"] == null ? "11" : Context.Request.Params["fontsize"];
        }

        public string GetLineHeight()
        {
            return Context.Request.Params["lineheight"] == null ? "13" : Context.Request.Params["lineheight"];
        }

        public string GetFontFamily()
        {
            return Context.Request.Params["fontfamily"] == null ? "Verdana" : Context.Request.Params["fontfamily"];
        }
    }
}
