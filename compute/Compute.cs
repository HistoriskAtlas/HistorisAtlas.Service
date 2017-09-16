using System;
using System.Collections.Generic;
using System.Web.Routing;
using System.Web;
using System.Globalization;
using System.Web.Configuration;
using System.Data.SqlClient;

namespace HistoriskAtlas.Service
{
    public class Compute : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext context)
        {
            return new ComputeHandler(context.RouteData);
        }
    }

    public class ComputeHandler : IHttpHandler
    {
        private RouteData routeData;

        public ComputeHandler(RouteData routeData)
        {
            this.routeData = routeData;
        }

        public void ProcessRequest(HttpContext context)
        {
            string function = routeData.Values["function"].ToString();

            if (function == "" || function.Contains("/"))
            {
                context.Response.ContentType = "text/html";
                return;
            }

            Common.SendStats(context, "compute/" + function);
            switch (function)
            {
                case "getHACoord":
                    decimal lat = decimal.Parse(context.Request.Params["latitude"], CultureInfo.InvariantCulture);
                    decimal lng = decimal.Parse(context.Request.Params["longitude"], CultureInfo.InvariantCulture);
                    Common.WriteOutput(HACoord.FromLatLng(new LatLng(lat, lng)), context);
                    break;
            }
        }

        public bool IsReusable { get { return false; } }
    }
}