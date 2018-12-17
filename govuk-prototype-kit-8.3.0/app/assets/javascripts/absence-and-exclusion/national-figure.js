var nationalFigure = {
    "frames": [], 
    "layout": {
        "autosize": true, 
        "yaxis": {
            "tickwidth": 0.5, 
            "title": "Absence rate", 
            "ticks": "outside", 
            "range": [
                0, 
                5.533333333333333
            ], 
            "ticksuffix": "%", 
            "showline": true, 
            "autorange": false
        }, 
        "showlegend": true, 
        "xaxis": {
            "showspikes": false, 
            "automargin": false, 
            "showticklabels": true, 
            "tickwidth": 0.5, 
            "title": "School year", 
            "ticks": "outside", 
            "range": [
                4.2414589104339795, 
                -0.24145891043397963
            ], 
            "mirror": false, 
            "zeroline": true, 
            "showline": true, 
            "type": "category", 
            "autorange": true
        }, 
        "font": {
            "family": "Arial"
        }, 
        "margin": {
            "r": 30, 
            "b": 50, 
            "l": 60, 
            "t": 20
        }, 
        "legend": {
            "traceorder": "normal", 
            "xanchor": "right", 
            "orientation": "h", 
            "y": 1.1, 
            "x": 0.96, 
            "font": {
                "size": 12
            }
        }
    }, 
    "data": [
        {
            "name": "Overall", 
            "ysrc": "martsky:120:dbc68d", 
            "xsrc": "martsky:120:c849e4", 
            "marker": {
                "color": "rgb(0, 94, 165)"
            }, 
            "mode": "markers+lines", 
            "y": [
                "4.7%", 
                "4.6%", 
                "4.6%", 
                "4.5%", 
                "5.3%"
            ], 
            "x": [
                "2016/17", 
                "2015/16", 
                "2014/15", 
                "2013/14", 
                "2012/13"
            ], 
            "type": "scatter"
        }, 
        {
            "name": "Authorised", 
            "ysrc": "martsky:120:acaf0d", 
            "xsrc": "martsky:120:c849e4", 
            "marker": {
                "color": "rgb(145, 43, 136)"
            }, 
            "mode": "markers+lines", 
            "y": [
                "3.4%", 
                "3.4%", 
                "3.5%", 
                "3.5%", 
                "4.2%"
            ], 
            "x": [
                "2016/17", 
                "2015/16", 
                "2014/15", 
                "2013/14", 
                "2012/13"
            ], 
            "line": {
                "color": "rgb(145, 43, 136)"
            }, 
            "type": "scatter"
        }, 
        {
            "name": "Unauthorised", 
            "ysrc": "martsky:120:9d1f03", 
            "xsrc": "martsky:120:c849e4", 
            "marker": {
                "color": "rgb(133, 153, 75)"
            }, 
            "mode": "markers+lines", 
            "y": [
                "1.3%", 
                "1.1%", 
                "1.1%", 
                "1.1%", 
                "1.1%"
            ], 
            "x": [
                "2016/17", 
                "2015/16", 
                "2014/15", 
                "2013/14", 
                "2012/13"
            ], 
            "type": "scatter"
        }
    ]
}


window.PLOTLYENV = {
    'BASE_URL': 'https://plot.ly'
};

var gd = document.getElementById('da7c10ac-eab3-4a0d-b7ee-2dc75bf614ba')
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
    data: nationalFigure.data,
    layout: nationalFigure.layout,
    frames: nationalFigure.frames,
    config: {
        "mapboxAccessToken": "pk.eyJ1IjoiY2hyaWRkeXAiLCJhIjoiY2lxMnVvdm5iMDA4dnhsbTQ5aHJzcGs0MyJ9.X9o_rzNLNesDxdra4neC_A",
        "linkText": "Export to plot.ly",
        "showLink": true
    }
});