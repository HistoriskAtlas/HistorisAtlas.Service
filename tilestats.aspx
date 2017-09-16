<%@ Page Language="C#" CodeBehind="tilestats.aspx.cs" Inherits="HistoriskAtlas.Service.tilestats" EnableViewState="false"%>
<!doctype html>

<html>
    <head>
        <title>HA Tile stats</title>
        <style>
            /*DIV {font-size: 0; line-height: 0; width:1px}
            TD { vertical-align:bottom }
            TD.high {color:Green}
            TD.low {color:Orange}
            TD.unknown {color:Red}*/
        </style>
        <script type="text/javascript" src="https://www.google.com/jsapi"></script>
        <script language="javascript">

            function scroll() {
                document.getElementById('out').style.top = (window.pageYOffset + 3) + 'px'
            }

            function over(text) {
                document.getElementById('out').innerHTML = text;
            }

            function out() {
                document.getElementById('out').innerHTML = '';
            }

            window.onscroll = scroll;

            //google.load("visualization", "1", {packages:["corechart"]});
            google.load('visualization', '1.0', { 'packages': ['corechart', 'table', 'gauge', 'controls'] });
            google.setOnLoadCallback(drawChart);
            var dashboard, chart, control, viewStd, viewStdPercent, viewTime, viewTimePercent, data;
            var percent = false;
            var time = false;

            function drawChart() {

                dashboard = new google.visualization.Dashboard(document.getElementById('chart_div'));

                data = new google.visualization.DataTable(
                   {
                       cols: [{ id: 'date', label: 'Dato', type: 'date' },
                            { id: 'error', label: 'Error', type: 'number' },
                            { id: 'generate', label: 'Generate', type: 'number' },
                            { id: 'cache', label: 'Cache', type: 'number' },
                            { id: 'notmodified', label: 'Not Modified', type: 'number' },
                            { id: 'time', label: 'Load time', type: 'number' },
                            { id: 'generatetime', label: 'Generate load time', type: 'number' },
                            { id: 'cachetime', label: 'Cache load time', type: 'number' },
                            { id: 'notmodifiedtime', label: 'Not Modified load time', type: 'number' }                            
                        ],
                       rows: [<%=GetChartData()%>]
                   }
                )

                var viewPercent = new google.visualization.DataView(data);
                makeViewPercent(viewPercent, data);

                viewStd = new google.visualization.DataView(data);
                viewStd.setColumns([0, 1, 2, 3, 4]);

                viewStdPercent = new google.visualization.DataView(viewPercent);
                viewStdPercent.setColumns([0, 1, 2, 3, 4]);

                viewTime = new google.visualization.DataView(data);
                viewTime.setColumns([0, 5, 6, 7, 8]);

                viewTimePercent = new google.visualization.DataView(viewPercent);
                viewTimePercent.setColumns([0, 5, 6, 7, 8]);

                chart = new google.visualization.ChartWrapper({
                    'chartType': 'AreaChart',
                    'containerId': 'chart_div',
                    'options': {
                        width: '100%',
                        height: 600,
                        //legend: { position: 'top' },
                        //bar: { groupWidth: '100%' },
                        isStacked: true,
                        areaOpacity: 1.0,
                        lineWidth: 0,
                        colors: ['black', 'red', 'orange', 'green'],
                        chartArea: { 'width': '80%' },
                        hAxis: { 'textPosition': 'none' },
                        chartArea: { 'top': 50, 'height': 550, 'left': '10%', 'width': '80%' },
                    }
                });

                control = new google.visualization.ControlWrapper({
                    'controlType': 'ChartRangeFilter',
                    'containerId': 'control_div',
                    'options': {
                        'filterColumnIndex': 0,
                        'ui': {
                            'chartType': 'LineChart',
                            'chartOptions': {
                                'chartArea': { 'top': 0, 'height': 50, 'left': '10%', 'width': '80%' },
                                'hAxis': { 'baselineColor': 'none' },
                                'height': 50
                            },
                            'chartView': {
                                'columns': [0, 3]
                            },
                            'minRangeSize': 86400000 * 30
                        }
                    }
                });

                dashboard.bind(control, chart);
                draw();
            }

            function togglePercent() {
                percent = !percent;
                draw();
                return false;
            }

            function toggleMode() {
                time = !time;
                draw();
                return false;
            }

            function draw() {
                chart.setOption('title', time ? 'Average time in ms to serve tile' : 'Number of tiles served');
                chart.setOption('isStacked', !time);
                chart.setOption('areaOpacity', time ? 0.5 : 1.0);
                control.setOption('ui.chartView.columns', [0, time ? 1 : 3])
                dashboard.draw(percent ? (time ? viewTimePercent : viewStdPercent) : (time ? viewTime : viewStd));
            }

            function makeViewPercent(view, data) {
                view.setColumns([0, {
                    type: 'number',
                    label: data.getColumnLabel(1),
                    calc: function (dt, row) {
                        var val = dt.getValue(row, 1);
                        for (var i = 1, total = 0, cols = 5 ; i < cols; i++) {
                            total += dt.getValue(row, i);
                        }
                        var percent = val / total;
                        return { v: percent, f: (percent * 100).toFixed(2) + '%' };
                    }
                }, {
                    type: 'number',
                    label: data.getColumnLabel(2),
                    calc: function (dt, row) {
                        var val = dt.getValue(row, 2);
                        for (var i = 1, total = 0, cols = 5 ; i < cols; i++) {
                            total += dt.getValue(row, i);
                        }
                        var percent = val / total;
                        return { v: percent, f: (percent * 100).toFixed(2) + '%' };
                    }
                }, {
                    type: 'number',
                    label: data.getColumnLabel(3),
                    calc: function (dt, row) {
                        var val = dt.getValue(row, 3);
                        for (var i = 1, total = 0, cols = 5 ; i < cols; i++) {
                            total += dt.getValue(row, i);
                        }
                        var percent = val / total;
                        return { v: percent, f: (percent * 100).toFixed(2) + '%' };
                    }
                }, {
                    type: 'number',
                    label: data.getColumnLabel(4),
                    calc: function (dt, row) {
                        var val = dt.getValue(row, 4);
                        for (var i = 1, total = 0, cols = 5 ; i < cols; i++) {
                            total += dt.getValue(row, i);
                        }
                        var percent = val / total;
                        return { v: percent, f: (percent * 100).toFixed(2) + '%' };
                    }
                }, {
                    type: 'number',
                    label: data.getColumnLabel(5),
                    calc: function (dt, row) {
                        var val = dt.getValue(row, 5);
                        for (var i = 5, total = 0, cols = 9 ; i < cols; i++) {
                            total += dt.getValue(row, i);
                        }
                        var percent = val / total;
                        return { v: percent, f: (percent * 100).toFixed(2) + '%' };
                    }
                }, {
                    type: 'number',
                    label: data.getColumnLabel(6),
                    calc: function (dt, row) {
                        var val = dt.getValue(row, 6);
                        for (var i = 5, total = 0, cols = 9 ; i < cols; i++) {
                            total += dt.getValue(row, i);
                        }
                        var percent = val / total;
                        return { v: percent, f: (percent * 100).toFixed(2) + '%' };
                    }
                }, {
                    type: 'number',
                    label: data.getColumnLabel(7),
                    calc: function (dt, row) {
                        var val = dt.getValue(row, 7);
                        for (var i = 5, total = 0, cols = 9 ; i < cols; i++) {
                            total += dt.getValue(row, i);
                        }
                        var percent = val / total;
                        return { v: percent, f: (percent * 100).toFixed(2) + '%' };
                    }
                }, {
                    type: 'number',
                    label: data.getColumnLabel(8),
                    calc: function (dt, row) {
                        var val = dt.getValue(row, 8);
                        for (var i = 5, total = 0, cols = 9 ; i < cols; i++) {
                            total += dt.getValue(row, i);
                        }
                        var percent = val / total;
                        return { v: percent, f: (percent * 100).toFixed(2) + '%' };
                    }
                }]);
            }

        </script>
    </head>
    <body style="font-family:verdana; font-size: 11px">
        <div style="width:100%"><a href="#" onclick="togglePercent();">toggle percent</a> <a href="#" onclick="toggleMode();">toggle mode</a></div>
        <div id="chart_div" style="width: 100%; height: 600px;"></div>
        <div id="control_div" style="width: 100%; height:50px"></div>
        <%--<SPAN id='out' style="position:absolute; right:3px; top:3px"></SPAN><BR>--%>
        <% if (Request.Params["graphonly"] != "true") { %>
            <BR>
            <%=GetStats("UrlReferrer")%>
            <BR>
            <%=GetStats("UserAgent")%>
            <BR>
            <%=GetErrors()%>
        <% } %>
    </body>
</html>