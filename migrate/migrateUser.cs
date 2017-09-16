using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

namespace HistoriskAtlas.Service
{
    public partial class migrate : Page
    {
        int roleCount;
        List<int> allUserIDs;

        private void MigrateUser()
        {
            StartMigrateTable("Role", true, true);
            InsertRole(0, "READ", "Reader");
            InsertRole(1, "WRITE", "Writer");
            InsertRole(2, "EDIT", "Editor (remove-disabled)");
            InsertRole(3, "DELETE", "Editor (remove-enabled)");
            InsertRole(4, "ADMIN", "Administrator");
            InsertRole(5, "DEV", "Developer");
            EndMigrateTable(roleCount);
            
            StartMigrateTable("User", true, true);

            allUserIDs = new List<int>();
            MD5 md5 = MD5.Create();
            StringBuilder sb = new StringBuilder();

            cmd.CommandText = "INSERT INTO [User] (UserID, Login, Password, FirstName, LastName, Email, Location, About, Created, Deleted, DefaultLicenseID, LicenseName, DefaultURLParameters, GeoID, RoleID, PasswordValidTo, UpdatePassRequired, IsActive) VALUES (@UserID, @Login, @Password, @FirstName, @LastName, @Email, @Location, @About, @Created, @Deleted, @DefaultLicenseID, @LicenseName, @DefaultURLParameters, @GeoID, @RoleID, @PasswordValidTo, @UpdatePassRequired, @IsActive)";
            cmd2.CommandText = "INSERT INTO [User_Institution] (UserID, InstitutionID) VALUES (@UserID, @InstitutionID)";
            int i = 0;
            using (SqlCommand cmdSelect = new SqlCommand("SELECT * FROM [User] ORDER BY UserID", connHADB3))
            using (SqlDataReader dr = cmdSelect.ExecuteReader())
            {
                while (dr.Read())
                {
                    string password = dr["Password"].ToString();
                    byte[] retVal = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                    sb.Clear();
                    for (int s = 0; s < retVal.Length; s++)
                        sb.Append(retVal[s].ToString("x2"));
                    password = sb.ToString();

                    allUserIDs.Add((int)dr["UserID"]);

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@UserID", dr["UserID"]);
                    cmd.Parameters.AddWithValue("@Login", dr["Login"]);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@FirstName", dr["FirstName"]);
                    cmd.Parameters.AddWithValue("@LastName", dr["LastName"]);
                    cmd.Parameters.AddWithValue("@Email", dr["Email"]);
                    cmd.Parameters.AddWithValue("@Location", DBNull.Value);
                    cmd.Parameters.AddWithValue("@About", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Created", dr["Created"]);
                    cmd.Parameters.AddWithValue("@Deleted", (bool)dr["Deleted"] ? deletedDateTime : (object)DBNull.Value);
                    //cmd.Parameters.AddWithValue("@InstitutionID", institutionID);
                    cmd.Parameters.AddWithValue("@DefaultLicenseID", DBNull.Value);
                    cmd.Parameters.AddWithValue("@LicenseName", DBNull.Value); //TODO delete from DB?
                    cmd.Parameters.AddWithValue("@DefaultURLParameters", DBNull.Value);
                    cmd.Parameters.AddWithValue("@GeoID", dr["GeoID"]);
                    cmd.Parameters.AddWithValue("@RoleID", dr["RoleLevel"]);
                    cmd.Parameters.AddWithValue("@PasswordValidTo", startTimestamp.AddYears(10));
                    cmd.Parameters.AddWithValue("@UpdatePassRequired", false);
                    cmd.Parameters.AddWithValue("@IsActive", DBNull.Value);
                    cmd.ExecuteNonQuery();

                    if (tagIDToInstitutionID.ContainsKey((int)dr["InstitutionID"]))
                    {
                        cmd2.Parameters.Clear();
                        cmd2.Parameters.AddWithValue("@UserID", dr["UserID"]);
                        cmd2.Parameters.AddWithValue("@InstitutionID", tagIDToInstitutionID[(int)dr["InstitutionID"]]);
                        cmd2.ExecuteNonQuery();
                    }


                    if (StepCheck(++i)) break;
                }
            }

            md5.Dispose();

            EndMigrateTable(i);
        }

        private void InsertRole(int id, string code, string name, string description = null)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "INSERT INTO Role (ID, Code, Name, Description) VALUES (@ID, @Code, @Name, @Description)";
            cmd.Parameters.AddWithValue("@ID", id);
            cmd.Parameters.AddWithValue("@Code", code);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@Description", description == null ? (object)DBNull.Value : description);
            cmd.ExecuteNonQuery();
            roleCount++;
        }

    }
}
