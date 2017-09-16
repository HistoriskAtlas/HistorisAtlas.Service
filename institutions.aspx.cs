using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HistoriskAtlas.Service
{
    public partial class institutions : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public string GetInstitutions()
        {
            string result = "<TABLE cellpadding=3 cellspacing=0><TR>";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                con.Open();

                result += "<TR><TD><B>Navn</B></TD><TD style='text-align:right'><B>Besøg</B></TD><TD style='text-align:right'><B>Lokaliteter</B></TD><TD style='text-align:right'><B>Gns. Besøg / Lokalitet</B></TD></TR>";

                int count = 0;

                using (SqlDataReader dr = new SqlCommand("SELECT PlurName, SUM([Views]) as summeret, COUNT([Views]) as antal, SUM([Views]) / COUNT([Views]) AS div FROM [hadb3].[dbo].[Geo], [hadb3].[dbo].[Tag_Geo], [hadb3].[dbo].[Tag] WHERE Geo.GeoID = Tag_Geo.GeoID AND Tag_Geo.TagID = Tag.TagID AND Tag.Category = 3 AND Geo.Online = 1	GROUP BY PlurName ORDER BY summeret DESC", con).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result += "<TR>";
                        result += "<TD>" + dr["PlurName"].ToString() + "</TD>";
                        result += "<TD style='text-align:right'>" + Common.PrettyInteger((int)dr["summeret"]) + "</TD>";
                        result += "<TD style='text-align:right'>" + Common.PrettyInteger((int)dr["antal"]) + "</TD>";
                        result += "<TD style='text-align:right'>" + Common.PrettyInteger((int)dr["div"]) + "</TD>";
                        result += "</TR>";
                        count++;
                    }
                }

                result += "</TR></TABLE><BR>";
                result += "<B>Antal institutioner: " + count + "</B>";
                return result;
            }
        }

    }
}
