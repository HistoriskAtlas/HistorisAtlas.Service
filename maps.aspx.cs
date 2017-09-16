using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HistoriskAtlas.Service
{
    public partial class maps : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public string GetMaps()
        {
            string result = "<TABLE cellpadding=3 cellspacing=0><TR>";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                con.Open();

                result += "<TR><TD style='text-align:right'><B>ID</B></TD><TD><B>Navn</B></TD><TD><B>Publiceret</B></TD><TD style='text-align:right'><B>(Startår)</B></TD><TD style='text-align:right'><B>År</B></TD><TD><B></B></TD></TR>";

                int count = 0;
                string where = "";

                if (Request.Params["search"] != "")
                    where = " WHERE Kommentar LIKE '%" + Request.Params["search"] + "%'";

                using (SqlDataReader dr = new SqlCommand("SELECT * FROM Map" + where + " ORDER BY MapID", con).ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result += "<TR>";
                        result += "<TD style='text-align:right'>" + dr["MapID"].ToString() + "</TD>";
                        result += "<TD><A href=\"http://historiskatlas.dk/?map=" + dr["MapID"].ToString() + "\">" + dr["Name"].ToString() + "</A></TD>";
                        result += "<TD>" + dr["IsPublic"].ToString() + "</TD>";
                        result += "<TD style='text-align:right'>" + dr["OrgStartYear"].ToString() + "</TD>";
                        result += "<TD style='text-align:right'>" + dr["OrgYear"].ToString() + "</TD>";
                        result += "<TD>" + ((int)dr["ReplacedBy"] == 0 ? "" : "Erstattet med " + dr["ReplacedBy"].ToString()) + "</TD>";
                        result += "</TR>";
                        count++;
                    }
                }

                result += "</TR></TABLE><BR>";
                result += "<B>Antal kort: " + count + "</B>";
                return result;
            }
        }

    }
}
