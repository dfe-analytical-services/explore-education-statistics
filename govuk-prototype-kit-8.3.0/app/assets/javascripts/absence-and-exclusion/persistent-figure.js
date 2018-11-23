var persistentFigure = {
    "frames": [], 
    "layout": {
        "autosize": true, 
        "yaxis": {
            "tickwidth": 0.5, 
            "title": "Absence rate", 
            "ticks": "outside", 
            "range": [
                0, 
                30.983108108108105
            ], 
            "ticksuffix": "%", 
            "showline": true, 
            "nticks": 0, 
            "type": "linear", 
            "autorange": false
        }, 
        "titlefont": {
            "size": 14
        }, 
        "xaxis": {
            "tickwidth": 0.5, 
            "title": "School year", 
            "ticks": "outside", 
            "range": [
                -0.24145891043397963, 
                4.2414589104339795
            ], 
            "showline": true, 
            "nticks": 0, 
            "type": "category", 
            "autorange": true
        }, 
        "font": {
            "family": "Arial"
        }, 
        "margin": {
            "pad": 0, 
            "r": 30, 
            "b": 50, 
            "l": 60, 
            "t": 20
        }, 
        "legend": {
            "y": 1.12, 
            "x": 0.98, 
            "font": {
                "size": 12
            }, 
            "xanchor": "right", 
            "orientation": "h"
        }
    }, 
    "data": [
        {
            "name": "All schools", 
            "ysrc": "martsky:124:47fff4", 
            "xsrc": "martsky:124:ba201e", 
            "mode": "markers+lines", 
            "y": [
                "13.6", 
                "10.7", 
                "11.0", 
                "10.5", 
                "10.8"
            ], 
            "x": [
                "2012/13", 
                "2013/14", 
                "2014/15", 
                "2015/16", 
                "2016/17"
            ], 
            "line": {
                "color": "rgb(0, 94, 165)"
            }, 
            "type": "scatter"
        }, 
        {
            "name": "State primary", 
            "ysrc": "martsky:124:5f9c35", 
            "xsrc": "martsky:124:ba201e", 
            "marker": {
                "color": "rgb(145, 43, 136)"
            }, 
            "stackgroup": null, 
            "mode": "markers+lines", 
            "y": [
                "11.0", 
                "8.1", 
                "8.4", 
                "8.2", 
                "8.3"
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
            "ysrc": "martsky:124:cc3f25", 
            "xsrc": "martsky:124:ba201e", 
            "marker": {
                "color": "rgb(133, 153, 75)"
            }, 
            "stackgroup": null, 
            "mode": "markers+lines", 
            "y": [
                "16.5", 
                "13.6", 
                "13.8", 
                "13.1", 
                "13.5"
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
            "ysrc": "martsky:124:c05517", 
            "xsrc": "martsky:124:ba201e", 
            "marker": {
                "color": "rgb(244, 119, 56)"
            }, 
            "stackgroup": null, 
            "mode": "markers+lines", 
            "y": [
                "29.4", 
                "26.5", 
                "27.5", 
                "26.9", 
                "28.5"
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

var gd = document.getElementById('c6a3e1ef-d491-4c64-8096-cdec12565a75')
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
    data: persistentFigure.data,
    layout: persistentFigure.layout,
    frames: persistentFigure.frames,
    config: {
        "mapboxAccessToken": "pk.eyJ1IjoiY2hyaWRkeXAiLCJhIjoiY2lxMnVvdm5iMDA4dnhsbTQ5aHJzcGs0MyJ9.X9o_rzNLNesDxdra4neC_A",
        "linkText": "Export to plot.ly",
        "showLink": true
    }
});