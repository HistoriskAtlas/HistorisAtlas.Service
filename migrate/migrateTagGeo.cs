using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;

namespace HistoriskAtlas.Service
{
    public partial class migrate : Page
    {
        private void MigrateTag_Geo()
        {
            StartMigrateTable("Tag_Geo", false, false);
            cmd.CommandText = "INSERT INTO Tag_Geo (TagID, GeoID, YearStart, YearEnd) VALUES (@TagID, @GeoID, @YearStart, @YearEnd)";
            SqlCommand cmdUpdateGeoYearStart = new SqlCommand("UPDATE Geo SET YearStart = CASE WHEN YearStart < @YearStart THEN YearStart ELSE @YearStart END WHERE GeoID = @GeoID", connHADB5, trans);
            SqlCommand cmdUpdateGeoYearEnd = new SqlCommand("UPDATE Geo SET YearEnd = CASE WHEN YearEnd > @YearEnd THEN YearEnd ELSE @YearEnd END WHERE GeoID = @GeoID", connHADB5, trans);
            int i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT TagID, GeoID, YearStart, YearEnd FROM Tag_Geo WHERE GeoTextID = 0 ORDER BY TagID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (!allTagIDs.Contains((int)dr["TagID"]))
                        continue;

                    if (!allGeoIDs.Contains((int)dr["GeoID"]))
                        continue;

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@TagID", dr["TagID"]);
                    cmd.Parameters.AddWithValue("@GeoID", dr["GeoID"]);
                    cmd.Parameters.AddWithValue("@YearStart", dr["YearStart"]);
                    cmd.Parameters.AddWithValue("@YearEnd", dr["YearEnd"]);
                    cmd.ExecuteNonQuery();



                    if (dr["YearStart"] != DBNull.Value)
                    {
                        cmdUpdateGeoYearStart.Parameters.Clear();
                        cmdUpdateGeoYearStart.Parameters.AddWithValue("@YearStart", dr["YearStart"]);
                        cmdUpdateGeoYearStart.Parameters.AddWithValue("@GeoID", dr["GeoID"]);
                        cmdUpdateGeoYearStart.ExecuteNonQuery();
                    }

                    if (dr["YearEnd"] != DBNull.Value)
                    {
                        cmdUpdateGeoYearEnd.Parameters.Clear();
                        cmdUpdateGeoYearEnd.Parameters.AddWithValue("@YearEnd", dr["YearEnd"]);
                        cmdUpdateGeoYearEnd.Parameters.AddWithValue("@GeoID", dr["GeoID"]);
                        cmdUpdateGeoYearEnd.ExecuteNonQuery();
                    }


                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);
        }
    }
}
