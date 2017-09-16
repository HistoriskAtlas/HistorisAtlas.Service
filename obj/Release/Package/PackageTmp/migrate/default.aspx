<%@ Page Language="C#" CodeBehind="default.aspx.cs" Inherits="HistoriskAtlas.Service.migrate" EnableViewState="false"%>
<!doctype html>

<html>
    <head>
        <title>HA DB migration from hadb3 to hadb5</title>
        <style>
            .warning {
                color: red;
            }
        </style>
    </head>
    <body style="font-family:verdana; font-size: 11px">
        <H1>HA DB migration from hadb3 to hadb5</H1>
        <%Migrate();%>
    </body>
</html>