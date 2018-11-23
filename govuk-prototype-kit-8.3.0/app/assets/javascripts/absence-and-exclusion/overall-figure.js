var overallFigure = {
    "frames": [], 
    "layout": {
        "autosize": true, 
        "yaxis": {
            "ticks": "outside", 
            "tickwidth": 0.5, 
            "title": "Absence rate", 
            "ticklen": 5, 
            "range": [
                0, 
                10.110357403355215
            ], 
            "ticksuffix": "%", 
            "zeroline": true, 
            "showline": true, 
            "type": "linear", 
            "autorange": false
        }, 
        "titlefont": {
            "family": "Arial", 
            "size": 14
        }, 
        "xaxis": {
            "ticks": "outside", 
            "tickwidth": 0.5, 
            "title": "School year", 
            "ticklen": 5, 
            "range": [
                -0.24145891043397963, 
                4.2414589104339795
            ], 
            "zeroline": true, 
            "showline": true, 
            "type": "category", 
            "autorange": true
        }, 
        "hovermode": "closest", 
        "margin": {
            "pad": 0, 
            "r": 30, 
            "b": 50, 
            "l": 60, 
            "t": 20
        }, 
        "legend": {
            "xanchor": "right", 
            "orientation": "h", 
            "borderwidth": 0, 
            "y": 1.1400000000000001, 
            "x": 0.98, 
            "font": {
                "size": 12
            }
        }
    }, 
    "data": [
        {
            "name": "All schools", 
            "ysrc": "martsky:122:66a641", 
            "xsrc": "martsky:122:8ffbcf", 
            "marker": {
                "color": "rgb(0, 94, 165)"
            }, 
            "mode": "markers+lines", 
            "hoverinfo": "y+name", 
            "y": [
                "5.3", 
                "4.5", 
                "4.6", 
                "4.6", 
                "4.7"
            ], 
            "x": [
                "2012/13", 
                "2013/14", 
                "2014/15", 
                "2015/16", 
                "2016/17"
            ], 
            "type": "scatter"
        }, 
        {
            "name": "State primary", 
            "ysrc": "martsky:122:020779", 
            "xsrc": "martsky:122:8ffbcf", 
            "marker": {
                "color": "rgb(145, 43, 136)"
            }, 
            "stackgroup": null, 
            "mode": "markers+lines", 
            "hoverinfo": "y+name", 
            "y": [
                "4.7", 
                "3.9", 
                "4.0", 
                "4.0", 
                "4.0"
            ], 
            "x": [
                "2012/13", 
                "2013/14", 
                "2014/15", 
                "2015/16", 
                "2016/17"
            ], 
            "type": "scatter"
        }, 
        {
            "name": "State secondary", 
            "ysrc": "martsky:122:54b095", 
            "xsrc": "martsky:122:8ffbcf", 
            "marker": {
                "color": "rgb(133, 153, 75)"
            }, 
            "stackgroup": null, 
            "mode": "markers+lines", 
            "hoverinfo": "y+name", 
            "y": [
                "5.9", 
                "5.2", 
                "5.3", 
                "5.2", 
                "5.4"
            ], 
            "x": [
                "2012/13", 
                "2013/14", 
                "2014/15", 
                "2015/16", 
                "2016/17"
            ], 
            "type": "scatter"
        }, 
        {
            "name": "Special", 
            "ysrc": "martsky:122:88131c", 
            "xsrc": "martsky:122:8ffbcf", 
            "marker": {
                "color": "rgb(244, 119, 56)"
            }, 
            "stackgroup": null, 
            "mode": "markers+lines", 
            "hoverinfo": "y+name", 
            "y": [
                "9.6", 
                "9.0", 
                "9.4", 
                "9.1", 
                "9.7"
            ], 
            "x": [
                "2012/13", 
                "2013/14", 
                "2014/15", 
                "2015/16", 
                "2016/17"
            ], 
            "type": "scatter"
        }
    ]
}

window.PLOTLYENV = {
    'BASE_URL': 'https://plot.ly'
};

var gd = document.getElementById('8c843b8c-4ee3-4936-9744-66b739909982')
var resizeDebounce = null;

function resizePlot() {
    var bb = gd.getBoundingClientRect();
    Plotly.relayout(gd, {
        width: bb.width,
        height: bb.height
    });
}


window.addEventListener('resize', function() {
    if (resizeDebounce) {
        window.clearTimeout(resizeDebounce);
    }
    resizeDebounce = window.setTimeout(resizePlot, 100);
});



Plotly.plot(gd, {
    data: overallFigure.data,
    layout: overallFigure.layout,
    frames: overallFigure.frames,
    config: {
        "mapboxAccessToken": "pk.eyJ1IjoiY2hyaWRkeXAiLCJhIjoiY2lxMnVvdm5iMDA4dnhsbTQ5aHJzcGs0MyJ9.X9o_rzNLNesDxdra4neC_A",
        "linkText": "Export to plot.ly",
        "showLink": true
    }
});