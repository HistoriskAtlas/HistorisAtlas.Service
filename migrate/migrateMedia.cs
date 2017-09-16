using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;

namespace HistoriskAtlas.Service
{
    public partial class migrate : Page
    {
        private void MigrateTag_Media()
        {
            MigrateTagMedia("Image", allImageIDs);
        }

        private void MigrateTagMedia(string mediatype, List<int> allIDs)
        {
            StartMigrateTable("Tag_" + mediatype, false, false);

            cmd.CommandText = "INSERT INTO Tag_" + mediatype + " (TagID, " + mediatype + "ID) VALUES (@TagID, @" + mediatype + "ID)";
            cmd2.CommandText = "SELECT COUNT(*) FROM Tag_" + mediatype + " WHERE TagID = @TagID AND " + mediatype + "ID = @" + mediatype + "ID";
            int i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT TagID, " + mediatype + "ID FROM Tag_" + mediatype + " ORDER BY TagID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (!allTagIDs.Contains((int)dr["TagID"]))
                        continue;

                    if (!allIDs.Contains((int)dr[mediatype + "ID"]))
                        continue;

                    cmd2.Parameters.Clear();
                    cmd2.Parameters.AddWithValue("@TagID", dr["TagID"]);
                    cmd2.Parameters.AddWithValue("@" + mediatype + "ID", dr[mediatype + "ID"]);
                    if ((int)cmd2.ExecuteScalar() > 0)
                        continue;

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@TagID", dr["TagID"]);
                    cmd.Parameters.AddWithValue("@" + mediatype + "ID", dr[mediatype + "ID"]);
                    cmd.ExecuteNonQuery();

                    if (StepCheck(++i)) break;
                }
            }

            EndMigrateTable(i);
        }

    }
}
