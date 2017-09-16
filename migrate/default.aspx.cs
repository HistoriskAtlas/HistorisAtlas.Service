using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Configuration;
using System.Web.UI;

namespace HistoriskAtlas.Service
{
    public partial class migrate : Page
    {
        DateTime startTimestamp, lastTimestamp;
        SqlConnection connHADB3, connHADB5;
        SqlTransaction trans;
        SqlCommand cmd, cmd2;
        string curTable;
        bool curIdentityInsert;
        bool outputWarnings;
        bool clearOnly;
        List<string> allTables;
        DateTime deletedDateTime;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            outputWarnings = Request.Params["output"] == null ? false : Request.Params["output"].ToLower() == "warnings";
            clearOnly = Request.Params["clearonly"] == null ? false : Request.Params["clearonly"].ToLower() == "true";
        }

        public void Migrate()
        {





            //DISABLED!!!
            return;






            connHADB3 = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString);
            connHADB3.Open();
            connHADB5 = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb5"].ConnectionString);
            connHADB5.Open();

            deletedDateTime = new DateTime(1900, 1, 1);

            using (trans = connHADB5.BeginTransaction(IsolationLevel.Snapshot))
            {
                cmd = new SqlCommand() { Connection = connHADB5, Transaction = trans };
                cmd2 = new SqlCommand() { Connection = connHADB5, Transaction = trans };
                                
                InitMigration();
                try
                {
                    GetTableNames();
                    ClearTables();
                    if (!clearOnly)
                    {
                        MigrateTag();
                        MigrateInstitution();
                        MigrateUser();
                        MigrateGeo();
                        MigrateTag_Geo();
                        MigrateImage();
                        MigrateTag_Media();
                        MigrateMap();

                        //Give HistoriskAtlas.dk institution to locations without any institution
                        cmd.CommandText = "INSERT INTO Tag_Geo(TagID, GeoID) SELECT 492, GeoID FROM Geo WHERE GeoID NOT IN (SELECT[GeoID] FROM[hadb5].[dbo].[Tag_Geo], Tag WHERE Tag_Geo.TagID = Tag.TagID AND Tag.Category = 3)";
                        cmd.ExecuteNonQuery();
                    }
                }
                catch(Exception e)
                {
                    OutputWarning("<br><br><b>ERROR DETECTED: " + e.Message + " - Rolling back transaction.</b>");
                    OutputWarning("Last value of cmd.CommandText: " + cmd.CommandText);
                    OutputWarning("Last value of cmd2.CommandText: " + cmd2.CommandText);
                    trans.Rollback();
                    Context.Response.Flush();
                    throw;
                }

                TimeSpan ts = DateTime.Now - startTimestamp;
                Output("<br><br><h1>ALL DONE in <b>" + ts.ToString(@"mm") + " mins, " + ts.ToString(@"ss") + " secs</b>! - Commiting transaction.</h1><br>", true);
                cmd.Dispose();
                cmd2.Dispose();
                trans.Commit();
            }

            connHADB5.Close();
            connHADB5.Dispose();
            connHADB3.Close();
            connHADB3.Dispose();
        }

        private void InitMigration()
        {
            startTimestamp = DateTime.Now;
            lastTimestamp = startTimestamp;
            Output("Migration started at " + startTimestamp.ToLocalTime() + "<br><br>");
            Context.Response.Flush();
        }

        private void StartMigrateTable(string table, bool identityInsert, bool reseed)
        {
            curTable = table;
            curIdentityInsert = identityInsert;
            Output("<hr><H2>Migrating to " + table + "</H2><hr>", true);
            
            if (identityInsert)
                SetCmd("SET IDENTITY_INSERT [" + table + "] ON", true);

            if (reseed)
                SetCmd("DBCC CHECKIDENT('" + table + "', RESEED, 0)", true);
        }

        private void EndMigrateTable(int rowCount)
        {
            TimeSpan ts = DateTime.Now - lastTimestamp;
            Output("<hr>Migrated to " + curTable + " <b>" + String.Format(CultureInfo.InvariantCulture, "{0:n0}", rowCount) + " rows</b> in <b>" + ts.ToString(@"mm") + " mins, " + ts.ToString(@"ss") + " secs</b><hr><br><br>", true);
            lastTimestamp = DateTime.Now;

            if (curIdentityInsert)
                SetCmd("SET IDENTITY_INSERT [" + curTable + "] OFF", true);

            Context.Response.Flush();
        }

        private void GetTableNames()
        {
            allTables = new List<string>();
            string[] excludeTables = { "'sysdiagrams'", "'Region'", "'Region_Region'", "'Region_RegionSource'", "'RegionSource'", "'RegionType'", "'RegionTypeCategory'" };
            using (SqlCommand cmdSelect = new SqlCommand("SELECT TABLE_NAME FROM information_schema.tables WHERE TABLE_NAME NOT IN (" + string.Join(", ", excludeTables) + ") ORDER BY TABLE_NAME", connHADB5, trans))
                using (SqlDataReader dr = cmdSelect.ExecuteReader())
                    while (dr.Read())
                        allTables.Add(dr["TABLE_NAME"].ToString());

            //tableInfos = new Dictionary<string, List<TableInfo>>();
            //using (SqlCommand cmdSelect = new SqlCommand("SELECT tab1.name AS [table], col1.name AS [column], tab2.name AS [referenced_table], col2.name AS [referenced_column], col2.system_type_id AS [referenced_column_type] FROM sys.foreign_key_columns fkc INNER JOIN sys.tables tab1 ON tab1.object_id = fkc.parent_object_id INNER JOIN sys.columns col1 ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id INNER JOIN sys.tables tab2 ON tab2.object_id = fkc.referenced_object_id INNER JOIN sys.columns col2 ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id", connHADB5, trans))
            //    using (SqlDataReader dr = cmdSelect.ExecuteReader())
            //        while (dr.Read())
            //        {
            //            string key = dr["referenced_table"].ToString();
            //            if (!tableInfos.ContainsKey(key))
            //                tableInfos.Add(key, new List<TableInfo>());

            //            tableInfos[key].Add(new TableInfo() { ReferencingTable = dr["table"].ToString(), ReferencingColumn = dr["column"].ToString(), ReferencedTable = dr["referenced_table"].ToString(), ReferencedColumn = dr["referenced_column"].ToString(), ReferencedColumnType = (byte)dr["referenced_column_type"] });
            //        }
        }

        private void ClearTables()
        {
            Output("<hr><H2>Truncating all tables</H2><hr>", true);
            
            //COULD BE DONE, but would lose transaction...
            //cmd.Parameters.Clear();
            //cmd.CommandText = "EXEC sp_msforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT all\"";
            //cmd.ExecuteNonQuery();
            //cmd.CommandText = "EXEC sp_MSForEachTable \"DELETE FROM ?\"";
            //cmd.ExecuteNonQuery();
            //cmd.CommandText = "EXEC sp_msforeachtable \"ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all\"";
            //cmd.ExecuteNonQuery();

            int totalRowCount;
            cmd.Parameters.Clear();
            cmd2.Parameters.Clear();
            do
            {
                totalRowCount = 0;
                foreach (string table in allTables)
                {
                    Output("Starting on " + table);
                    
                    cmd.CommandText = "SELECT COUNT(*) FROM [" + table + "]";
                    if ((int)cmd.ExecuteScalar() == 0)
                    {
                        Output(" - Done - 0 rows left<br>");
                        continue;
                    }

                    List<string> selectSQLs = new List<string>();
                    string deleteSQL = "";

                    cmd.CommandText = "SELECT tab1.name AS [table], col1.name AS [column], col2.name AS [referenced_column] FROM sys.foreign_key_columns fkc INNER JOIN sys.tables tab1 ON tab1.object_id = fkc.parent_object_id INNER JOIN sys.columns col1 ON col1.column_id = parent_column_id AND col1.object_id = tab1.object_id INNER JOIN sys.tables tab2 ON tab2.object_id = fkc.referenced_object_id INNER JOIN sys.columns col2 ON col2.column_id = referenced_column_id AND col2.object_id = tab2.object_id WHERE tab2.name = '" + table + "'";
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.HasRows)
                        {
                            SetCmd2("DELETE FROM [" + table + "]", true);
                            Output(" - Done - 0 rows left<br>");
                            continue;
                        }

                        while (dr.Read())
                        {
                            if (deleteSQL == "") 
                                deleteSQL = "DELETE FROM [" + table + "] WHERE " + dr["referenced_column"].ToString() + " NOT IN ";

                            selectSQLs.Add("(SELECT " + dr["column"].ToString() + " FROM [" + dr["table"].ToString() + "] WHERE " + dr["column"].ToString() + " IN (SELECT " + dr["referenced_column"].ToString() + " FROM [" + table + "]))");
                        }
                    }

                    SetCmd(deleteSQL + "(" + string.Join(" UNION ", selectSQLs) + ")", true);
                    cmd.CommandText = "SELECT COUNT(*) FROM [" + table + "]";
                    int count = (int)cmd.ExecuteScalar();
                    totalRowCount += (int)cmd.ExecuteScalar();
                    Output(" - Done - " + count + " rows left<br>");
                }
                Output("<b>Total rows left to delete: " + totalRowCount + "</b><br>");
            } while (totalRowCount > 0);

            Output("<hr>All deleted!<hr><br><br>", true);
        }
        
        private void ClearTable(string table)
        {
            SetCmd("DELETE FROM [" + table + "]", true);
        }

        private void SetCmd(string sql, bool execute = false)
        {
            _SetCmd(cmd, sql, execute);
        }

        private void SetCmd2(string sql, bool execute = false)
        {
            _SetCmd(cmd2, sql, execute);
        }

        private void _SetCmd(SqlCommand cmd, string sql, bool execute)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = sql;
            if (execute)
                cmd.ExecuteNonQuery();
        }

        private bool StepCheck(int i)
        {
            if (i % 100 != 0)
                return false;
            
            Output(". ");
            Context.Response.Flush();
            return false; //true
        }

        private void Output(string text, bool important = false)
        {
            if (outputWarnings && !important)
                Context.Response.Write(". ");
            else
                Context.Response.Write(text);
        }

        private void OutputWarning(string text)
        {
            Output("<br><span class='warning'>WARNING: " + text + "</span>", true);
        }
    }

    public class TableInfo
    {
        public string ReferencingTable;
        public string ReferencingColumn;
        public string ReferencedTable;
        public string ReferencedColumn;
        public byte ReferencedColumnType;
    }
}
