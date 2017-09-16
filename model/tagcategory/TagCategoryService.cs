using System;
using System.Web;
using System.Web.Routing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace HistoriskAtlas.Service
{
    public class TagCategories : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new TagCategoriesHandler();
        }
    }

    public class TagCategoriesHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            List<TagCategory> tagCategories = new List<TagCategory>();

            tagCategories.Add(new TagCategory() { id = 0, plurName = "Emner", singName = "Emne" });
            tagCategories.Add(new TagCategory() { id = 1, plurName = "Perioder", singName = "Periode" });
            tagCategories.Add(new TagCategory() { id = 2, plurName = "Geografi", singName = "Geografi" });
            tagCategories.Add(new TagCategory() { id = 3, plurName = "Institutioner", singName = "Institution" });
            tagCategories.Add(new TagCategory() { id = 4, plurName = "Licenser", singName = "Licens" });
            tagCategories.Add(new TagCategory() { id = 5, plurName = "Interne", singName = "Intern" });

            Common.SendStats(context, "tagcategories");
            Common.WriteOutput(tagCategories, context);
        }

        public bool IsReusable { get { return false; } }
    }
}