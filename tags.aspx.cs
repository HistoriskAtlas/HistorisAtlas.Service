using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HistoriskAtlas.Service
{
    public partial class tags : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public string GetTags()
        {
            //string result = "<TABLE cellpadding=1 cellspacing=0>";
            string result = "<DIV style='column-count: auto; -webkit-column-count:auto; -moz-column-count:auto; column-width: 200px; -webkit-column-width:200px; -moz-column-width:200px'>";

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();

                //result += "<TR><TD style='text-align:right'><B>ID</B></TD><TD><B>Navn</B></TD><TD><B>Publiceret</B></TD><TD style='text-align:right'><B>(Startår)</B></TD><TD style='text-align:right'><B>År</B></TD><TD><B></B></TD></TR>";

                using (SqlDataReader dr = new SqlCommand("SELECT PlurName, TagID FROM Tag WHERE TagID NOT IN (SELECT SubsetTagID FROM TagSubset) AND Category = 0 ORDER BY PlurName", conn).ExecuteReader())
                {
                    //result += "<TR>";
                    while (dr.Read())
                    {
                        //result += "<TD>"
                        result += "<div style='display: inline-block'>";
                        result += "<img src='http://historiskatlas.dk/images/dots/dot" + dr["TagID"].ToString() + ".png' onerror=\"this.src='http://historiskatlas.dk/images/dots/dotDefault.png'\"><B>" + dr["PlurName"].ToString() + "</B><BR>";
                        result += "<TABLE cellpadding=1 cellspacing=0>" + GetTags((int)dr["TagID"], 1, conn) + "</TABLE><BR>";
                        result += "</div><br>";
                        //result += "</TD>";
                    }
                    //result += "</TR>";
                }
            }
            result += "</DIV>";
            //result += "</TABLE>";

            return result;
        }

        public string GetTags(int upperTagID, int level, SqlConnection conn)
        {
            string result = "";
            using (SqlDataReader dr = new SqlCommand("SELECT Tag.TagID, PlurName, SubsetTagID FROM Tag, TagSubset WHERE Tag.TagID = TagSubset.SubsetTagID AND TagSubset.TagID = " + upperTagID + " ORDER BY PlurName", conn).ExecuteReader())
            {
                while (dr.Read())
                {
                    result += "<TR>";
                    result += "<TD>";
                    for (int i = 0; i < level; i++)
                        result += "&nbsp;&nbsp; ";
                    result += "<img src='http://historiskatlas.dk/images/dots/dot" + dr["TagID"].ToString() + ".png' onerror=\"this.src='http://historiskatlas.dk/images/dots/dotDefault.png'\"> " + dr["PlurName"].ToString() + "</TD>";
                    result += "</TR>";
                    result += GetTags((int)dr["SubsetTagID"], level + 1, conn);
                }
            }

            return result;
        }

    }
}
