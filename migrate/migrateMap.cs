using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;

namespace HistoriskAtlas.Service
{
    public partial class migrate : Page
    {

        private void MigrateMap()
        {
            StartMigrateTable("Map", false, false);
            cmd.CommandText = "INSERT INTO Map (MapID, Name, Comment, OrgProductionStartYear, OrgProductionEndYear, IsPublic, ReplacedBy, TextID, MinLat, MaxLat, MinLon, MaxLon, MinZ, MaxZ, IconCoords) VALUES (@MapID, @Name, @Comment, @OrgProductionStartYear, @OrgProductionEndYear, @IsPublic, @ReplacedBy, @TextID, @MinLat, @MaxLat, @MinLon, @MaxLon, @MinZ, @MaxZ, @IconCoords)";

            int i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM Map ORDER BY MapID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    int mapID = (int)dr["MapID"];
                    bool isPublic = (bool)dr["IsPublic"];
                    if (mapID == 161)
                        isPublic = true;

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@MapID", mapID);
                    cmd.Parameters.AddWithValue("@Name", dr["Name"].ToString());
                    cmd.Parameters.AddWithValue("@Comment", dr["Kommentar"].ToString());
                    cmd.Parameters.AddWithValue("@OrgProductionStartYear", dr["OrgStartYear"]);
                    cmd.Parameters.AddWithValue("@OrgProductionEndYear", dr["OrgYear"]);
                    cmd.Parameters.AddWithValue("@IsPublic", isPublic);
                    cmd.Parameters.AddWithValue("@ReplacedBy", dr["ReplacedBy"]);
                    cmd.Parameters.AddWithValue("@TextID", dr["TextID"]);

                    if (mapID == 161)
                        SetLatLons(cmd.Parameters, -85.05115, 85.05115, -180, 180);

                    if (dr["MinLat"] is DBNull)
                    {
                        if (!(dr["BBLeft"] is DBNull))
                        {
                            if ((int)dr["BBLeft"] != 0)
                            {
                                LatLng ll1 = LatLng.FromHACoord(new HACoord((int)dr["BBLeft"], (int)dr["BBBottom"]));
                                LatLng ll2 = LatLng.FromHACoord(new HACoord((int)dr["BBRight"], (int)dr["BBTop"]));
                                SetLatLons(cmd.Parameters, ll1.latitude, ll2.latitude, ll1.longitude, ll2.longitude);
                            }
                        }
                    }
                    else
                        SetLatLons(cmd.Parameters, dr["MinLat"], dr["MaxLat"], dr["MinLon"], dr["MaxLon"]);

                    if (!cmd.Parameters.Contains("@MinLat"))
                        SetLatLons(cmd.Parameters, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

                    if (dr["MinZ"] is DBNull)
                    {
                        if (!(dr["BBMinZ"] is DBNull))
                            SetZs(cmd.Parameters, ZfromZ((byte)dr["BBMaxZ"]), ZfromZ((byte)dr["BBMinZ"]));
                    }
                    else
                        SetZs(cmd.Parameters, dr["MinZ"], dr["MaxZ"]);

                    if (!cmd.Parameters.Contains("@MinZ"))
                        SetZs(cmd.Parameters, DBNull.Value, DBNull.Value);

                    cmd.Parameters.AddWithValue("@IconCoords", dr["IconCoords"].ToString());

                    cmd.ExecuteNonQuery();

                    if (StepCheck(++i)) break;
                }
            }
            EndMigrateTable(i);
        }

        private void SetLatLons(SqlParameterCollection spc, object MinLat, object MaxLat, object MinLon, object MaxLon)
        {
            if (!spc.Contains("@MinLat"))
                spc.AddWithValue("@MinLat", MinLat);
            if (!spc.Contains("@MaxLat"))
                spc.AddWithValue("@MaxLat", MaxLat);
            if (!spc.Contains("@MinLon"))
                spc.AddWithValue("@MinLon", MinLon);
            if (!spc.Contains("@MaxLon"))
                spc.AddWithValue("@MaxLon", MaxLon);
        }

        private void SetZs(SqlParameterCollection spc, object MinZ, object MaxZ)
        {
            spc.AddWithValue("@MinZ", MinZ);
            spc.AddWithValue("@MaxZ", MaxZ);
        }

        private byte ZfromZ(byte z)
        {
            //return (byte)(19 - (z * ((double)19 / 13)));
            return (byte)(19 - z);
        }
    }
}
