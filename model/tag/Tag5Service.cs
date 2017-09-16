using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class Tags5 : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new Tags5Handler();
        }
    }

    public class Tags5Handler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            List<Tag> tags;

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb5"].ConnectionString))
            {
                conn.Open();
                int category = -1; //all
                if (context.Request.Params["category"] != null)
                    Int32.TryParse(context.Request.Params["category"], out category);

                int parentID = -1; //all
                if (context.Request.Params["parentid"] != null)
                    Int32.TryParse(context.Request.Params["parentid"], out parentID);

                List<string> wheres = new List<string>();
                if (category > -1) 
                    wheres.Add("Category = " + category);
                if (parentID == 0) 
                    wheres.Add("TagID NOT IN (SELECT TagID From TagHierarki)");
                if (parentID > 0) 
                    wheres.Add("TagID IN (SELECT TagID From TagHierarki WHERE UpperTagID = " + parentID + ")");

                SqlCommand cmd = new SqlCommand("SELECT * FROM Tag" + (wheres.Count > 0 ? " WHERE " + string.Join(" AND ", wheres) : ""), conn);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    tags = new List<Tag>();
                    while (dr.Read())
                    {
                        Tag tag = new Tag() { id = (int)dr["TagID"], plurName = dr["PlurName"].ToString(), singName = dr["SingName"].ToString(), category = (byte)dr["Category"], yearStart = dr["YearStart"] is DBNull ? null : (int?)dr["YearStart"], yearEnd = dr["YearEnd"] is DBNull ? null : (int?)dr["YearEnd"] };
                        tags.Add(tag);
                    }
                }
            }

            Common.SendStats(context, "tags5");
            Common.WriteOutput(tags, context);
        }

        public bool IsReusable { get { return false; } }
    }
}