using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;

namespace HistoriskAtlas.Service
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            AddRoute("maps", new Maps());
            AddRoute("relevantmaps", new RelevantMaps());
            AddRoute("tags", new Tags());
            AddRoute("tags5", new Tags5());
            AddRoute("tagcategories", new TagCategories());
            AddRoute("geos", new Geos());
            AddRoute("geos5", new Geos5());
            AddRoute("institutions", new Institutions());
            AddRoute("stats", new Stats());
            AddRoute("geocontent/{id}", new GeoContentRoute());
            AddRoute("geocontent5/{id}", new GeoContent5Route());
            AddRoute("geotext/{id}/{ordering}", new GeoTextRoute());
            AddRoute("geotext5/{id}/{ordering}", new GeoText5Route());
            AddRoute("geoliterature/{id}", new GeoLiteratureRoute());
            AddRoute("image/{id}", new ImageRoute());
            AddRoute("imagemeta/{id}", new ImageMetaRoute());
            AddRoute("imagemeta5/{id}", new ImageMeta5Route());
            AddRoute("pdf/{id}", new PDFRoute());
            AddRoute("pdfmeta/{id}", new PDFMetaRoute());
            AddRoute("text/{id}", new TextRoute());
            AddRoute("mapicon/{id}", new MapIconRoute());
            AddRoute("compute/{*function}", new Compute());
            AddRoute("tilescale", new TileScaleRoute());
            AddRoute("harvest/geos", new HarvestGeos());
        }

        private void AddRoute(string baseUrl, IRouteHandler handler)
        {
            if (!baseUrl.Contains("*")) RouteTable.Routes.Add(new Route(baseUrl + ".xml", handler));
            if (!baseUrl.Contains("*")) RouteTable.Routes.Add(new Route(baseUrl + ".json", handler));
            if (!baseUrl.Contains("*")) RouteTable.Routes.Add(new Route(baseUrl + ".html", handler));
            RouteTable.Routes.Add(new Route(baseUrl, handler));
        }
    }
}
