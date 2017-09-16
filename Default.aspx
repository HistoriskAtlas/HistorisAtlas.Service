<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HistoriskAtlas.Service.Default" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>service.historiskatlas.dk</title>
    <style>
        A {text-decoration:none; color:blue}
        TABLE {margin:10px}
        TD {padding-right:20px}
        .label {font-weight:bold; width:100px}
    </style>
</head>
<body style="font-family:Verdana; font-size:12px; margin:30px">
    <h1><a href="<%=GetLink(null, "")%>">service.historiskatlas.dk</a></h1>
    Internet media type output format: 
        <a href="<%=GetLink("json")%>"<%=Bold("json")%>>application/json</a> | 
        <a href="<%=GetLink("xml")%>"<%=Bold("xml")%>>application/xml</a> |
        <a href="<%=GetLink("jpeg")%>"<%=Bold("jpeg")%>>image/jpeg</a> |
        <a href="<%=GetLink("pdf")%>"<%=Bold("pdf")%>>application/pdf</a> |
        <a href="<%=GetLink("html")%>"<%=Bold("html")%>>text/html</a>
        <hr>

    <% if (service == "") { %>
    <table>
        <% foreach (string s in services[format]) { %>
            <tr><td><h2><a href="<%=GetLink(null, s)%>"><%=(s + "." + format)%></a></h2></td><td><%=GetDescription(s)%></td></tr>
        <% } %>
    </table>
    <% } else { %>
    <h2><%=(service + "." + format)%></h2>
    <%=GetDescription()%><br>
    <br>
    <b>Parametre</b>
    <table>
        <% if (GetParameters().Count == 0) { %>
            <tr><td><i>ingen</i></td></tr>
        <% } else { %>
            <% foreach (Variable v in GetParameters()) { %>
                <tr>
                    <td class="label"><%=v.title%></td><td><i><%=v.type%></i></td><td><%=v.description%></td>
                </tr>
            <% } %>
        <% } %>
    </table>
    <br>
    <b>Returnerede værdier</b>
     <table>
        <% if (GetReturned().Count == 0) { %>
            <tr><td><i>ingen</i></td></tr>
        <% } else { %>
            <% foreach (Variable v in GetReturned()) { %>
                <tr>
                    <td class="label"><%=v.title%></td><td<% if (v.type == "DEPRECATED") { %> style="color:red"<% } %>><i><%=v.type%></i></td><td><%=v.description%></td>
                </tr>
            <% } %>
        <% } %>
    </table>
    <br>
    <b>Eksempel</b>
    <table>
        <% if (GetExamples().Count == 0) { %>
            <tr><td><i>ingen</i></td></tr>
        <% } else { %>
            <% foreach (Example e in GetExamples()) { %>
                <tr>
                    <td><a href="<%=e.url%>"><%=e.url%></a></td><td><i><%=e.description%></i></td>
                </tr>
            <% } %>
        <% } %>
    </table>
    <br>

    <% } %>

<%--    <h2>test</h2>
    <ul>
        <li><h3><a href="historiskatlas-v1://test.htm">custom protocol test</a></h3></li>
    </ul>--%>
</body>
</html>
