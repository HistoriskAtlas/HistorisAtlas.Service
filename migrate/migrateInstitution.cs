using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;

namespace HistoriskAtlas.Service
{
    public partial class migrate : Page
    {
        Dictionary<int, int> tagIDToInstitutionID;

        private void MigrateInstitution()
        {
            StartMigrateTable("Institution", false, true);
            tagIDToInstitutionID = new Dictionary<int, int>();

            cmd.CommandText = "INSERT INTO Institution (URL, Email, Type, Deleted, TagID) VALUES (@URL, @Email, @Type, @Deleted, @TagID); SELECT SCOPE_IDENTITY()";
            //cmd.Parameters.Clear();
            //cmd.Parameters.AddWithValue("@URL", "");
            //cmd.Parameters.AddWithValue("@Email", "");
            //cmd.Parameters.AddWithValue("@Type", DBNull.Value);
            //cmd.Parameters.AddWithValue("@Deleted", deletedDateTime);
            //cmd.Parameters.AddWithValue("@TagID", 58);
            //cmd.ExecuteNonQuery();
            
            int i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Institution ORDER BY TagID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@URL", dr["URL"]);
                    cmd.Parameters.AddWithValue("@Email", dr["Email"]);
                    cmd.Parameters.AddWithValue("@Type", dr["Type"]);
                    cmd.Parameters.AddWithValue("@Deleted", dr["Deleted"]);
                    cmd.Parameters.AddWithValue("@TagID", dr["TagID"]);
                    int institutionID = Convert.ToInt32(cmd.ExecuteScalar());

                    tagIDToInstitutionID.Add((int)dr["TagID"], institutionID);

                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);
        }        
    }
}
