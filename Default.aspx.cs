using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HistoriskAtlas.Service
{
    public partial class Default : System.Web.UI.Page
    {
        public string format;
        public string service;
        public static Dictionary<string, List<string>> services; 

        protected void Page_Load(object sender, EventArgs e)
        {
            services = new Dictionary<string, List<string>>();
            List<string> std = new List<string>(new string[] { "maps", "relevantmaps", "tags", "tagcategories", "geos", "institutions", "stats", "geocontent/[geoid]", "geotext/[geoid]/[ordering]", "imagemeta/[imageid]", "pdfmeta/[pdfid]", "text/[textid]" });
            services.Add("json", std);
            services.Add("xml", std);
            services.Add("jpeg", new List<string>(new string[] { "image/[imageid]", "mapicon/[mapid]", "http://tile.historiskatlas.dk/[mapid]/[zoom]/[x]/[y]" }));
            services.Add("pdf", new List<string>(new string[] { "pdf/[pdfid]" }));
            services.Add("html", new List<string>(new string[] { "geoliterature/[geoid]" }));

            format = GetFormat();
            service = string.IsNullOrEmpty(Request.Params["service"]) ? "" : Request.Params["service"];

            if (service != "")
                if (!services[format].Contains(service))
                    service = "";
        }

        private string GetFormat()
        {
            if (string.IsNullOrEmpty(Request.Params["format"]))
                return "json";

            switch (Request.Params["format"].ToLower())
            {
                case "json":
                    return "json";
                case "xml":
                    return "xml";
                case "jpeg":
                    return "jpeg";
                case "pdf":
                    return "pdf";
                case "html":
                    return "html";
                default:
                    return "json";
            }
        }

        public string Bold(string f) { return format == f ? " style='font-weight:bold'" : ""; }

        public string GetLink(string newFormat = null, string newService = null)
        {
            return "?format=" + (newFormat == null ? format : newFormat) + "&service=" + (newService == null ? service : newService); 
        }

        public string GetDescription(string s = null)
        {
            switch (s == null ? service : s)
            {
                case "maps": return "Returnerer en liste af data om kort, der er publiceret på Historisk Atlas.";
                case "relevantmaps": return "Returnerer en liste af ID'er på kort, der dækker det område og zoom-niveau der er angivet af parametrerne.";
                default: return "";
            }
        }

        public List<Variable> GetParameters()
        {
            List<Variable> result = new List<Variable>();
            switch (service)
            {
                case "relevantmaps":
                    result.Add(new Variable() { title = "center", type = "real,real", description = "Koordinatsæt (lat,lng) for centrum af det angivne kortudsnit." });
                    result.Add(new Variable() { title = "span", type = "real,real", description = "Bredde- og længdegradsudbredelse (lat,lng) af det angivne kortudsnit." });
                    result.Add(new Variable() { title = "z", type = "integer", description = "Zoomniveau for det angivne kortudsnit. Hvert kort har en minimums- og maksimumsgrænse, hvor indenfor de bedst kan ses." });
                    break;
                case "tags": case "tagcategories": case "geos": case "institutions": case "stats": case "geocontent/[geoid]": case "geotext/[geoid]/[ordering]": 
                case "geoliterature/[geoid]": case "imagemeta/[imageid]": case "pdfmeta/[pdfid]": case "text/[textid]": case "image/[imageid]": case "mapicon/[mapid]":
                case "http://tile.historiskatlas.dk/[mapid]/[zoom]/[x]/[y]": case "pdf/[pdfid]":
                    result.Add(new Variable() { title = "Awaiting documentation" });
                    break;
            }
            return result;
        }
 
        public List<Variable> GetReturned()
        {
            List<Variable> result = new List<Variable>();
            switch (service)
            {
                case "maps":
                    result.Add(new Variable() { title = "id", type = "integer", description = "Unikt ID for kortet. Kan bruges ved kald til <a href=\"" + GetLink("jpeg", "http://tile.historiskatlas.dk") + "\">tile-servicen</a>." });
                    result.Add(new Variable() { title = "title", type = "string", description = "Overskrift for lokaliteten." });
                    result.Add(new Variable() { title = "textid", type = "integer", description = "ID på en beskrivende tekst, der kan hentes ved kald til <a href=\"" + GetLink(null, "text") + "\">text-servicen</a>." });
                    result.Add(new Variable() { title = "year", type = "integer", description = "Årstal for opmåling / udgivelse af kortet." });
                    result.Add(new Variable() { title = "BBMinZ", type = "DEPRECATED", description = "Bruges pt. kun internt." });
                    result.Add(new Variable() { title = "BBMaxZ", type = "DEPRECATED" });
                    result.Add(new Variable() { title = "BBLeft", type = "DEPRECATED" });
                    result.Add(new Variable() { title = "BBRight", type = "DEPRECATED" });
                    result.Add(new Variable() { title = "BBTop", type = "DEPRECATED" });
                    result.Add(new Variable() { title = "BBBottom", type = "DEPRECATED" });
                    result.Add(new Variable() { title = "centerLat", type = "real", description = "Breddegradskoordinat (latitude) for centrum af kortet." });
                    result.Add(new Variable() { title = "centerLng", type = "real", description = "Længdegradskoordinat (longitude) for centrum af kortet." });
                    result.Add(new Variable() { title = "centerLng", type = "real", description = "Breddegradsudbredelsen af kortet." });
                    result.Add(new Variable() { title = "centerLng", type = "real", description = "Længdegradsudbredelsen af kortet." });
                    break;
                case "relevantmaps":
                    result.Add(new Variable() { title = "id", type = "integer", description = "Unikt ID for kortet. Se <a href=\"" + GetLink(null, "maps") + "\">maps-servicen</a>." });
                    break;
                case "tags": case "tagcategories": case "geos":case "institutions": case "stats": case "geocontent/[geoid]": case "geotext/[geoid]/[ordering]": 
                case "geoliterature/[geoid]": case "imagemeta/[imageid]": case "pdfmeta/[pdfid]": case "text/[textid]": case "image/[imageid]": case "mapicon/[mapid]":
                case "http://tile.historiskatlas.dk/[mapid]/[zoom]/[x]/[y]": case "pdf/[pdfid]":
                    result.Add(new Variable() { title = "Awaiting documentation" });
                    break;
            }
            return result;
        }

        public List<Example> GetExamples()
        {
            List<Example> result = new List<Example>();
            switch (service)
            {
                case "maps":
                    result.Add(new Example() { url = "maps." + format });
                    break;
                case "relevantmaps":
                    result.Add(new Example() { url = "relevantmaps." + format + "?center=55.40095,10.40405&span=0.3153,1.03821&z=13", description = "Svarende til et område dækkende Fyn." });
                    break;
                case "tags":
                    result.Add(new Example() { url = "tags." + format + "?category=0&parentid=0", description = "" });
                    break;
                case "tagcategories":
                    result.Add(new Example() { url = "tagcategories." + format, description = "" });
                    break;
                case "geos":
                    result.Add(new Example() { url = "geos." + format + "?center=55.060091,10.60524&count=50&tagscategory=0", description = "Svendborg, only subject tags returned" });
                    result.Add(new Example() { url = "geos." + format + "?center=55.40095,10.40405&span=0.3153,1.03821", description = "Fyn" });
                    result.Add(new Example() { url = "geos." + format + "?center=55.40095,10.40405&span=0.3153,1.03821&includetags=1,4,5", description = "Fyn, only includes geos wich have tag with id 1, 4 or 5" });
                    result.Add(new Example() { url = "geos." + format + "?center=55.40095,10.40405&span=0.3153,1.03821&z=8&clustering=true", description = "Fyn /w clustering. clustering=true returns clusters and/or regular geos according to zoomlevel (z). Clusters have the same structure as geos, but with title=null, tagids from the underlaying geos and id=-[count], where count is the number of underlaying geos." });
                    result.Add(new Example() { url = "geos." + format + "?center=55.060091,10.60524&count=10&source=HA,1001", description = "Source parameter defaults to \"HA\", but also \"1001\" (1001 fortællinger) can be specified. Combinations is also possible, as shown in the example. \"1001\" geos have a tag with id 388, and id's between 100.000.000 and 200.000.000." });
                    break;
                case "institutions":
                    result.Add(new Example() { url = "institutions." + format, description = "All institutions." });
                    result.Add(new Example() { url = "institutions." + format + "?type=1", description = "Only museums. type: 1=archives, 2=libraries, 3=museums, 4=sponsors" });
                    break;
                case "stats":
                    result.Add(new Example() { url = "stats." + format, description = "" });
                    break;
                case "geocontent/[geoid]":
                    result.Add(new Example() { url = "geocontent/42." + format, description = "" });
                    break;
                case "geotext/[geoid]/[ordering]":
                    result.Add(new Example() { url = "geotext/43/0." + format, description = "" });
                    break;
                case "geoliterature/[geoid]":
                    result.Add(new Example() { url = "geoliterature/43." + format, description = "" });
                    break;
                case "imagemeta/[imageid]":
                    result.Add(new Example() { url = "imagemeta/131." + format, description = "" });
                    break;
                case "pdfmeta/[pdfid]":
                    result.Add(new Example() { url = "pdfmeta/756." + format, description = "" });
                    break;
                case "text/[textid]":
                    result.Add(new Example() { url = "text/43." + format, description = "" });
                    break;
                case "image/[imageid]":
                    result.Add(new Example() { url = "image/131?width=300", description = "" });
                    break;
                case "mapicon/[mapid]":
                    result.Add(new Example() { url = "mapicon/55?width=580&height=120", description = "" });
                    break;
                case "http://tile.historiskatlas.dk/[mapid]/[zoom]/[x]/[y]":
                    result.Add(new Example() { url = "http://tile.historiskatlas.dk/55/8/135/80.jpg", description = "" });
                    break;
                case "pdf/[pdfid]":
                    result.Add(new Example() { url = "pdf/756", description = "" });
                    break;
            }
            return result;
        }

        public class Variable
        {
            public string title;
            public string type;
            public string description;
        }

        public class Example
        {
            public string url;
            public string description;
        }
    }
}