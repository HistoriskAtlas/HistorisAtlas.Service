<%@ Page Language="C#" CodeBehind="tags.aspx.cs" Inherits="HistoriskAtlas.Service.tags" EnableViewState="false"%>
<!doctype html>

<html>
    <head>
        <title>HA Tags</title>
        <style>
            BODY
            {
                font-family: Verdana;
                font-size: 11px;
            }
            IMG
            {
                width:22px;
                height:22px;
                vertical-align:middle;
            }
            TD
            {
                vertical-align:top;
            }
        </style>
    </head>
    <body style="font-family:verdana; font-size: 11px">
        <%=GetTags()%>
    </body>
</html>