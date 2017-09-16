using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;

namespace HistoriskAtlas.Service
{
    public partial class migrate : Page
    {
        List<int> allGeoIDs;
        Dictionary<int, int> latestContentOrderingByGeoID;
        const int maxIntroLength = 300;

        private void MigrateGeo()
        {
            StartMigrateTable("Geo", true, true);
            allGeoIDs = new List<int>();
            latestContentOrderingByGeoID = new Dictionary<int, int>();
            cmd.CommandText = "INSERT INTO GEO (GeoID, Title, Intro, FreeTags, YearStart, YearEnd, Latitude, Longitude, Online, Views, UserID) VALUES (@GeoID, @Title, @Intro, @FreeTags, @YearStart, @YearEnd, @Latitude, @Longitude, @Online, @Views, @UserID)";
            int i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Geo ORDER BY GeoID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    allGeoIDs.Add((int)dr["GeoID"]);

                    string intro = dr["Intro"].ToString(); //TODO: ændret til 300.................
                    if (intro.Length > maxIntroLength)
                    {
                        OutputWarning(dr["Title"] + " (id:" + dr["GeoID"] + ") - Intro longer than " + maxIntroLength + ", truncating.");
                        intro = intro.Substring(0, maxIntroLength);
                    }

                    string freeTags = dr["FreeTags"] is DBNull ? "" : dr["FreeTags"].ToString();

                    LatLng ll = LatLng.FromHACoord(new HACoord((int)dr["GeoX"], (int)dr["GeoY"]));

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@GeoID", dr["GeoID"]);
                    cmd.Parameters.AddWithValue("@Title", dr["Title"]);
                    cmd.Parameters.AddWithValue("@Intro", intro);
                    cmd.Parameters.AddWithValue("@FreeTags", freeTags);
                    cmd.Parameters.AddWithValue("@YearStart", dr["YearStart"]);
                    cmd.Parameters.AddWithValue("@YearEnd", dr["YearEnd"]);
                    cmd.Parameters.AddWithValue("@Latitude", ll.latitude);
                    cmd.Parameters.AddWithValue("@Longitude", ll.longitude);
                    cmd.Parameters.AddWithValue("@Online", dr["Online"]);
                    cmd.Parameters.AddWithValue("@Views", dr["Views"]);
                    cmd.Parameters.AddWithValue("@UserID", 5);
                    cmd.ExecuteNonQuery();

                    if (StepCheck(++i)) break;
                }
            }

            EndMigrateTable(i);


            StartMigrateTable("Content", false, true);
            i = 0;
            int j = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Geo_Text ORDER BY GeoTextID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (!allGeoIDs.Contains((int)dr["GeoID"]))
                        continue;

                    cmd.CommandText = "INSERT INTO Content (GeoID, Ordering, Type, UserID) VALUES (@GeoID, @Ordering, 'Text', " + allUserIDs[0] + "); SELECT SCOPE_IDENTITY()";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@GeoID", dr["GeoID"]);
                    cmd.Parameters.AddWithValue("@Ordering", dr["Ordering"]);
                    int ContentID = Convert.ToInt32(cmd.ExecuteScalar());

                    IncrementOrdering((int)dr["GeoID"], (int)dr["Ordering"]);

                    string text = Common.GetHTMLFromXAML(dr["Text"].ToString(), true);
                    cmd.CommandText = "INSERT INTO Text (ContentID, Headline, Text) VALUES (@ContentID, @Headline, @Text)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ContentID", ContentID);
                    cmd.Parameters.AddWithValue("@Headline", dr["Headline"]);
                    cmd.Parameters.AddWithValue("@Text", text);
                    cmd.ExecuteNonQuery();

                    //cmd.CommandText = "INSERT INTO Tag_GeoContent (TagID, GeoContentID) VALUES (@TagID, @GeoContentID)";
                    //using (SqlCommand cmdSelectTagGeo = new SqlCommand("SELECT TagID FROM Tag_Geo WHERE GeoTextID = " + dr["GeoTextID"], connHADB3))
                    //using (SqlDataReader drTagGeo = cmdSelectTagGeo.ExecuteReader())
                    //    while (drTagGeo.Read())
                    //    {
                    //        Output("- Inserted TagID " + drTagGeo["TagID"] + "<br>");
                    //        cmd.Parameters.Clear();
                    //        cmd.Parameters.AddWithValue("@TagID", drTagGeo["TagID"]);
                    //        cmd.Parameters.AddWithValue("@GeoContentID", GeoContentID);
                    //        cmd.ExecuteNonQuery();
                    //        j++;
                    //    }

                    if (StepCheck(++i)) break;
                }
            }

            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Geo_Biblio", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (!allGeoIDs.Contains((int)dr["GeoID"]))
                        continue;

                    IncrementOrdering((int)dr["GeoID"]);

                    cmd.CommandText = "INSERT INTO Content (GeoID, Ordering, Type, UserID) VALUES (@GeoID, @Ordering, 'Biblio', " + allUserIDs[0] + "); SELECT SCOPE_IDENTITY()";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@GeoID", dr["GeoID"]);
                    cmd.Parameters.AddWithValue("@Ordering", latestContentOrderingByGeoID[(int)dr["GeoID"]]);
                    int ContentID = Convert.ToInt32(cmd.ExecuteScalar());

                    cmd.CommandText = "INSERT INTO Biblio (ContentID, CQL) VALUES (@ContentID, @CQL)";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ContentID", ContentID);
                    cmd.Parameters.AddWithValue("@CQL", dr["CQL"]);
                    cmd.ExecuteNonQuery();

                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);

            StartMigrateTable("LogType", true, true);
            Dictionary<string, int> LogTypeNameToLogTypeID = new Dictionary<string,int>();
            i = 0;
            foreach (string name in new string[]{"Select", "Insert", "Update", "Delete", "Created", "Saved", "Publish", "ReqPublish", "Unpublish", "Error"})
                LogTypeNameToLogTypeID.Add(name, i++);

            i = 0;
            cmd.CommandText = "INSERT INTO LogType (ID, Name) VALUES (@ID, @Name)";
            foreach (KeyValuePair<string, int> kvp in LogTypeNameToLogTypeID)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", kvp.Value);
                cmd.Parameters.AddWithValue("@Name", kvp.Key);
                cmd.ExecuteNonQuery();
                if (StepCheck(++i)) break;
            }
            EndMigrateTable(i);

            
            StartMigrateTable("LogSource", true, true);
            cmd.CommandText = "INSERT INTO LogSource (ID, Name) VALUES (@ID, @Name)";
            i = 0;
            foreach (string name in new string[]{"HA3client", "service.historiskatlas.dk"})
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@ID", i);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.ExecuteNonQuery();
                if (StepCheck(++i)) break;
            }
            EndMigrateTable(i);


            StartMigrateTable("Log", true, true);
            cmd.CommandText = "INSERT INTO Log (ID, Item, Value, UserID, DateCreated, TypeID, SourceID, Notes) VALUES (@ID, @Item, @Value, @UserID, @DateCreated, @TypeID, @SourceID, @Notes)";
            i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Geo_Log ORDER BY Date", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    int userID = allUserIDs.Contains((int)dr["UserID"]) ? (int)dr["UserID"] : 5;

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ID", i + 1);
                    cmd.Parameters.AddWithValue("@Item", "hadb5.geo|geoid=" + dr["GeoID"].ToString()); //??
                    cmd.Parameters.AddWithValue("@Value", ""); //??
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@DateCreated", dr["Date"]);
                    cmd.Parameters.AddWithValue("@TypeID", LogTypeNameToLogTypeID[dr["Type"].ToString()]);
                    cmd.Parameters.AddWithValue("@SourceID", 0);
                    cmd.Parameters.AddWithValue("@Notes", dr["Comments"] is DBNull ? "" : dr["Comments"].ToString());
                    //TODO: Missing a field for the id of the geo... maybe in the "UpdateValue" field?
                    cmd.ExecuteNonQuery();

                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);
        }

        private void IncrementOrdering(int geoID, int? ordering = null)
        {
            if (latestContentOrderingByGeoID.ContainsKey(geoID))
            {
                if (ordering.HasValue)
                {
                    if (ordering.Value > latestContentOrderingByGeoID[geoID])
                        latestContentOrderingByGeoID[geoID] = ordering.Value;
                }
                else
                    latestContentOrderingByGeoID[geoID]++;
            }
            else
                latestContentOrderingByGeoID.Add(geoID, ordering.HasValue ? ordering.Value : 1);
        }

    }
}
