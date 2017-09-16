using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;

namespace HistoriskAtlas.Service
{
    public partial class migrate : Page
    {
        List<int> allImageIDs;

        private void MigrateImage()
        {
            StartMigrateTable("Image", true, true);
            allImageIDs = new List<int>();
            Dictionary<int, OBMImage> OBMImages = new Dictionary<int, OBMImage>();

            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM OBMtblBilled ORDER BY BilledID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
                while (dr.Read())
                {
                    OBMImage OBMImage = new OBMImage();
                    OBMImage.Ophavsmand = dr["Ophavsmand"] is DBNull ? null : (string)dr["Ophavsmand"];
                    OBMImage.OphavsmandUsikker = dr["OphavsmandUsikker"] is DBNull ? (bool?)null : (bool?)dr["OphavsmandUsikker"];
                    OBMImage.Ophavsmand2 = dr["Ophavsmand2"] is DBNull ? null : (string)dr["Ophavsmand2"];
                    OBMImage.Sorteringsaarstal = dr["Sorteringsaarstal"] is DBNull ? (int?)null : (int?)dr["Sorteringsaarstal"];
                    OBMImage.LicensID = dr["LicensID"] is DBNull ? (byte?)null : (byte?)dr["LicensID"];
                    OBMImage.Rettighedshaver = dr["Rettighedshaver"].ToString();
                    OBMImages.Add((int)dr["BilledID"], OBMImage);
                }

            cmd.CommandText = "INSERT INTO [Image] (ImageID, Text, Year, YearIsApprox, Photographer, Licensee, Created, Deleted) VALUES (@ImageID, @Text, @Year, @YearIsApprox, @Photographer, @Licensee, @Created, @Deleted)";
            cmd2.CommandText = "INSERT INTO [Tag_Image] (TagID, ImageID) VALUES (@TagID, @ImageID)";
            int i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Image ORDER BY ImageID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    allImageIDs.Add((int)dr["ImageID"]);

                    int? year = dr["Year"] is DBNull ? (int?)null : (int?)dr["Year"];
                    bool yearIsApprox = false;
                    string photographer = null;
                    string licensee = null;

                    OBMImage OBMImage = OBMImages.ContainsKey((int)dr["ImageID"]) ? OBMImages[(int)dr["ImageID"]] : null;
                    if (OBMImage != null)
                    {
                        if (OBMImage.Sorteringsaarstal.HasValue && !year.HasValue)
                        {
                            year = OBMImage.Sorteringsaarstal.Value;
                            yearIsApprox = true;
                        }
                        if (OBMImage.OphavsmandUsikker.HasValue)
                            if (!OBMImage.OphavsmandUsikker.Value)
                            {
                                photographer = OBMImage.Ophavsmand2 == null ? OBMImage.Ophavsmand : OBMImage.Ophavsmand2;
                                photographer = photographer == "Ukendt" || photographer == "" ? null : photographer;
                            }

                        licensee = OBMImage.Rettighedshaver;
                    }

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@ImageID", dr["ImageID"]);
                    cmd.Parameters.AddWithValue("@Text", dr["Text"]);
                    cmd.Parameters.AddWithValue("@Year", year.HasValue ? year.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@YearIsApprox", yearIsApprox);
                    cmd.Parameters.AddWithValue("@Photographer", photographer == null ? (object)DBNull.Value : photographer);
                    cmd.Parameters.AddWithValue("@Licensee", licensee == null ? (object)DBNull.Value : licensee);
                    cmd.Parameters.AddWithValue("@Created", dr["Created"]);
                    cmd.Parameters.AddWithValue("@Deleted", (bool)dr["Deleted"] ? deletedDateTime : (object)DBNull.Value);
                    cmd.ExecuteNonQuery();

                    if (OBMImage != null)
                    {
                        if (OBMImage.LicensID.HasValue)
                        {
                            cmd2.Parameters.Clear();
                            cmd2.Parameters.AddWithValue("@TagID", tagIDFromLicensID[OBMImage.LicensID.Value]);
                            cmd2.Parameters.AddWithValue("@ImageID", (int)dr["ImageID"]);
                            cmd2.ExecuteNonQuery();
                        }
                    }

                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);

            StartMigrateTable("Geo_Image", false, false);
            cmd.CommandText = "INSERT INTO Geo_Image (GeoID, ImageID, Ordering) VALUES (@GeoID, @ImageID, @Ordering)";
            i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT GeoID, ImageID, Ordering FROM Geo_Image ORDER BY GeoID, Ordering", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (!allGeoIDs.Contains((int)dr["GeoID"]))
                        continue;

                    if (!allImageIDs.Contains((int)dr["ImageID"]))
                        continue;

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@GeoID", dr["GeoID"]);
                    cmd.Parameters.AddWithValue("@ImageID", dr["ImageID"]);
                    cmd.Parameters.AddWithValue("@Ordering", dr["Ordering"]);
                    cmd.ExecuteNonQuery();

                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);

        }

        public class OBMImage
        {
            public string Ophavsmand;
            public bool? OphavsmandUsikker;
            public string Ophavsmand2;
            public int? Sorteringsaarstal;
            public byte? LicensID;
            public string Rettighedshaver;
        }

    }
}
