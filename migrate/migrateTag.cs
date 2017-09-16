using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;

namespace HistoriskAtlas.Service
{
    public partial class migrate : Page
    {
        List<int> allTagIDs;
        Dictionary<byte, int> tagIDFromLicensID;

        private void MigrateTag()
        {
            StartMigrateTable("Tag", true, true);
            allTagIDs = new List<int>();
            cmd.CommandText = "INSERT INTO Tag (TagID, PlurName, SingName, Category, YearStart, YearEnd, TableID, TextID, Created) VALUES (@TagID, @PlurName, @SingName, @Category, @YearStart, @YearEnd, @TableID, @TextID, @Created)";
            int i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Tag ORDER BY TagID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    allTagIDs.Add((int)dr["TagID"]);

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@TagID", dr["TagID"]);
                    cmd.Parameters.AddWithValue("@PlurName", dr["PlurName"]);
                    cmd.Parameters.AddWithValue("@SingName", dr["SingName"]);
                    cmd.Parameters.AddWithValue("@Category", dr["Category"]);
                    cmd.Parameters.AddWithValue("@YearStart", dr["YearStart"]);
                    cmd.Parameters.AddWithValue("@YearEnd", dr["YearEnd"]);
                    cmd.Parameters.AddWithValue("@TableID", (int)dr["TableID"] == 0 ? DBNull.Value : dr["TableID"]);
                    cmd.Parameters.AddWithValue("@TextID", (int)dr["TextID"] == 0 ? DBNull.Value : dr["TextID"]);
                    cmd.Parameters.AddWithValue("@Created", dr["Created"]);
                    cmd.ExecuteNonQuery();

                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);

            StartMigrateTable("TagHierarki", false, false);
            cmd.CommandText = "INSERT INTO TagHierarki (UpperTagID, TagID) VALUES (@UpperTagID, @TagID)";
            i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT TagID, SubsetTagID FROM TagSubset ORDER BY TagID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (!allTagIDs.Contains((int)dr["TagID"]))
                        continue;

                    if (!allTagIDs.Contains((int)dr["SubsetTagID"]))
                        continue;

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UpperTagID", dr["TagID"]);
                    cmd.Parameters.AddWithValue("@TagID", dr["SubsetTagID"]);
                    cmd.ExecuteNonQuery();

                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);

            StartMigrateTable("Licens", false, false);
            tagIDFromLicensID = new Dictionary<byte, int>();
            cmd.CommandText = "INSERT INTO Licens (LicensID, Text, Url, Ordering, TagID) VALUES (@LicensID, @Text, @Url, @Ordering, @TagID)";
            i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM OBMLicens ORDER BY LicensID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@LicensID", dr["LicensID"]);
                    cmd.Parameters.AddWithValue("@Text", dr["Tekst"]);
                    cmd.Parameters.AddWithValue("@Url", dr["Url"]);
                    cmd.Parameters.AddWithValue("@Ordering", dr["Sortering"]);
                    cmd.Parameters.AddWithValue("@TagID", dr["TagID"]);
                    cmd.ExecuteNonQuery();

                    tagIDFromLicensID.Add((byte)dr["LicensID"], (int)dr["TagID"]);

                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);
        }
    }
}
