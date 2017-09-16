<%@ Page Language="C#" CodeBehind="stats.aspx.cs" Inherits="HistoriskAtlas.Service.stats" EnableViewState="false"%>
<html>
    <head>
    <style>
        .name { font-size: <%=GetFontSize()%>px; line-height:<%=GetLineHeight()%>px }
        .value { font-size: <%=GetFontSize()%>px; text-align:right; font-weight:bold; line-height:<%=GetLineHeight()%>px}
    </style>
    </head>
    <body style="color: <%=GetForeground()%>; background-color: <%=GetBackground()%>; font-family:<%=GetFontFamily()%>; font-size: <%=GetFontSize()%>px; margin:0px; overflow:hidden">
        <table width="100%" cellpadding=0 cellspacing=0>
            <tr><td class="name">Tiles</td><td class="value"><asp:Label ID="TilesLabel" runat="server" /></td></tr>
            <tr><td class="name">Billeder</td><td class="value"><asp:Label ID="ImagesLabel" runat="server" /></td></tr>
            <tr><td class="name">Billeder (meta)</td><td class="value"><asp:Label ID="ImagesMetaLabel" runat="server" /></td></tr>
            <tr><td class="name">Lokalitetsliste</td><td class="value"><asp:Label ID="GeosLabel" runat="server" /></td></tr>
            <tr><td class="name">Lokalitetsindhold</td><td class="value"><asp:Label ID="GeoContentLabel" runat="server" /></td></tr>
            <tr><td class="name">Kortliste</td><td class="value"><asp:Label ID="MapsLabel" runat="server" /></td></tr>
            <tr><td class="name">Tagliste</td><td class="value"><asp:Label ID="TagsLabel" runat="server" /></td></tr>
        </table>
    </body>
</html>