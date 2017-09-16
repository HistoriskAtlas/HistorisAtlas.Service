using System;
using System.Web;
using System.Web.Routing;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Text;
using HistoriskAtlas.Service.ServiceReferenceBiblio;
using System.Xml;

namespace HistoriskAtlas.Service
{
    public class GeoLiteratureRoute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new GeoLiteratureHandler(context.RouteData);
        }
    }

    public class GeoLiteratureHandler : IHttpHandler
    {
        private RouteData routeData;

        public GeoLiteratureHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!Common.RequestIsForHtml(context)) //TODO: only supports html
                return;

            StringBuilder sb = new StringBuilder();
            searchRetrieveRequestType SRRtype = new ServiceReferenceBiblio.searchRetrieveRequestType();
            SRRtype.version = "1.1";
            SRRtype.startRecord = "1";
            SRRtype.maximumRecords = "10";
            SRRtype.recordSchema = "dc";
            SRRtype.recordPacking = "xml";

            using (SqlConnection conn = new SqlConnection(WebConfigurationManager.ConnectionStrings["hadb"].ConnectionString))
            {
                conn.Open();
                using (SqlDataReader dr = new SqlCommand("SELECT CQL FROM Geo_Biblio WHERE GeoID = " + routeData.Values["id"], conn).ExecuteReader())
                    if (dr.Read())
                        SRRtype.query = dr["CQL"].ToString();
            }

            SRWPortClient srw = new ServiceReferenceBiblio.SRWPortClient();
            searchRetrieveResponseType result = srw.SearchRetrieveOperation(SRRtype);

            foreach (ServiceReferenceBiblio.recordType record in result.records)
            {
                XmlNode all = record.recordData.Any[0];
                XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
                manager.AddNamespace("dc", all.NamespaceURI);

                string title = GetFromXML(ref all, ref manager, "title");
                string creator = GetFromXML(ref all, ref manager, "creator");
                string subject = GetFromXML(ref all, ref manager, "subject");
                string date = GetFromXML(ref all, ref manager, "date");
                string format = GetFromXML(ref all, ref manager, "format");
                string description = GetFromXML(ref all, ref manager, "description");
                string url = GetFromXML(ref all, ref manager, "identifier");
                string type = GetFromXML(ref all, ref manager, "type");

                subject = subject != null ? subject.Length > 400 ? subject.Substring(0, 397) + "..." : subject : null;

                sb.AppendLine("<B>" + title + "</B><BR>");
                sb.AppendLine("<I>" + creator + "</I><BR>");
                if (subject != null) sb.AppendLine("Emne: " + subject + "</I><BR>");
                if (description != null) sb.AppendLine(description + "<BR>");
                sb.AppendLine("<BR>");
            }

            Common.SendStats(context, "geoliterature");
            Common.WriteOutput(sb.ToString(), context, routeData);
        }

        private string GetFromXML(ref XmlNode node, ref XmlNamespaceManager manager, string tag)
        {
            return (node.SelectSingleNode("//dc:" + tag, manager) == null) ? null : node.SelectSingleNode("//dc:" + tag, manager).InnerText;
        }

        public bool IsReusable { get { return false; } }
    }
}