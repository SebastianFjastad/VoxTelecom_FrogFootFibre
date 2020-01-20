var CreatePinMap = function (markers) {
    var zones = [];
    var locations = [];

    function getPortalZones() {
        if (!zones.length) {
            return $.ajax({
                url: '/Home/Home/GetZoneDataForMaps',
                type: "GET",
                dataType: "json",
                success: function (data) {
                    zones = data;
                }
            });
        }
        return true;
    }

    function getPortalLocations() {
        if (!locations.length) {
            return $.ajax({
                url: '/Home/Home/GetLocationDataForMaps',
                type: "GET",
                dataType: "json",
                success: function (data) {
                    locations = data;
                }
            });
        }
        return true;
    }

    function buildLegend() {
        var legendColourStyle = 'width: 12px;height: 12px;margin-left: 3px;display: inline-block;';
        var labelStyle = 'margin-left: 5px; padding-top: -3; font-size: 11px;';
        return $('#map-canvas').append('<div id="legend" style="height: 90px; width: 120px; background: white;margin-left: 10px;padding-top: 5px; ">' +
        '<div style="' + legendColourStyle + 'background: white; border: solid 1px black;"></div><label style="' + labelStyle + '">Potential</label></br>' +
        '<div style="' + legendColourStyle + 'background: DodgerBlue;"></div><label style="' + labelStyle + '">Scheduled</label></br>' +
        '<div style="' + legendColourStyle + 'background: Gold;"></div><label style="' + labelStyle + '">Work In Progress</label></br>' +
        '<div style="' + legendColourStyle + 'background: Chartreuse;"></div><label style="' + labelStyle + '">Completed</label>' +
        '</div>');
    }

    var precinct = {
        "stroke": "white",
        "stroke-width": 2,
        "fill": null,
        "fill-opacity": 0.2,
        "zIndex": 1,
        "title": null,
        "zoneId": null
    }

    var potential = {
        "stroke": "white",
        "stroke-width": 2,
        "fill": null,
        "fill-opacity": 0.2,
        "zIndex": 100
    }

    var scheduled = {
        "stroke": "DodgerBlue",
        "stroke-width": 2,
        "fill": "DodgerBlue",
        "fill-opacity": 0.2,
        "zIndex": 100
    }

    var workInProgress = {
        "stroke": "Gold",
        "stroke-width": 2,
        "fill": "Gold",
        "fill-opacity": 0.1,
        "zIndex": 100
    }

    var completed = {
        "stroke": "Chartreuse",
        "stroke-width": 2,
        "fill": "Chartreuse",
        "fill-opacity": 0.2,
        "zIndex": 100
    }

    var map;

    $.when(getPortalZones(), getPortalLocations()).done(function() {
        setTimeout(function() {
            //google.maps.event.addDomListener(window, 'load', function() {
                map = new google.maps.Map(document.getElementById('map-canvas'), {
                    center: new google.maps.LatLng(-33.993205, 18.472720),
                    zoom: 12,
                    mapTypeId: google.maps.MapTypeId.HYBRID
                });
                buildLegend();
                map.controls[google.maps.ControlPosition.LEFT_BOTTOM].push(document.getElementById('legend'));

                if (typeof $('#User_Latitude').val() == 'undefined' || typeof $('#User_Longitude').val() == 'undefined') {
                    //set default map centre
                    map.setCenter(new google.maps.LatLng(-34.005940, 18.444055));
                } else {
                    map.setCenter({ lat: parseFloat($('#User_Latitude').val()), lng: parseFloat($('#User_Longitude').val()) });
                }

                google.maps.event.addListener(map, 'idle', function() {
                    addGeoJson(map, getServerUrl(map, '/ftth/features'), function(features) {
                        google.maps.event.addListenerOnce(map, 'idle', function() {
                            features.forEach(function(feature) {
                                map.data.remove(feature);
                            });
                        });
                    });
                });

                //check the markers object that needs to be present when using this maps pin func
                if (typeof markers != "undefined") {
                    $.each(markers, function(i, n) {
                        var iconUrl;
                        switch (this.status) {
                        case 0:
                            iconUrl = 'http://maps.google.com/mapfiles/ms/icons/yellow-dot.png'; //no order
                            break;
                        case 1:
                            iconUrl = 'http://maps.google.com/mapfiles/ms/icons/ltblue-dot.png'; //new
                            break;
                        case 2:
                            iconUrl = 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png'; //pending
                            break;
                        case 3:
                            iconUrl = 'http://maps.google.com/mapfiles/ms/icons/purple-dot.png'; //ordered
                            break;
                        case 4:
                            iconUrl = 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'; //accepted
                            break;
                        case 5:
                            iconUrl = 'http://maps.google.com/mapfiles/ms/icons/red-dot.png'; //cancelled
                            break;
                        default:
                            iconUrl = 'http://maps.google.com/mapfiles/ms/icons/yellow-dot.png';
                        }

                        var pos = new google.maps.LatLng(this.lat, this.lng);
                        var marker = new google.maps.Marker({
                            map: map,
                            position: pos,
                            draggable: false,
                            icon: iconUrl
                        });

                        var infowindow = new google.maps.InfoWindow({
                            content: "<div>" + this.firstName + " " + this.lastName +
                                "<br/>" + this.email +
                                "<br/>" + this.address +
                                "</div>"
                        });

                        marker.addListener('click', function() {
                            infowindow.open(map, marker);
                        });
                    });
                }
            });
        }, 20);

    function ajaxPost(url, callback) {
        var request = window.ActiveXObject ?
            new ActiveXObject('Microsoft.XMLHTTP') :
            new XMLHttpRequest;

        request.onreadystatechange = function () {
            if (request.readyState == 4 && request.status == 200) {
                request.onreadystatechange = function () { };

                try {
                    callback(JSON.parse(request.responseText));
                } catch (e) {
                    console.log(e.message);
                }
            }
        };

        request.open('POST', "http://maps.frogfoot.net" + url, true);
        request.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        request.send();
    }

    function getServerUrl(map, url, pos) {
        if (pos === undefined) {
            var bounds = map.getBounds();
            var ne = bounds.getNorthEast();
            var sw = bounds.getSouthWest();
            return url + '?bb=' + sw.lng() + ',' + sw.lat() + ',' + ne.lng() + ',' + ne.lat();
        }

        return url + '?ll=' + pos.lat() + ',' + pos.lng();
    }

    function addGeoJson(map, url, callback) {
        ajaxPost(url, function (json) {
            var items = [];
            //check the location names against features
            $.each(json.features, function (i, x) {
                if ($.inArray(x.properties.title, locations) > -1) {
                    x.properties = precinct; //set the style to precinct
                    items.push(x);
                }
            });

            //check the zone codes against features
            $.each(json.features, function (i, feature) {
                for (var j = 0; j < zones.length; j++) {
                    if (zones[j].Code === feature.properties.title) {
                        switch (zones[j].Status) {
                            case 1:
                                feature.properties = $.extend({}, potential);
                                break;
                            case 2:
                                feature.properties = $.extend({}, scheduled);
                                break;
                            case 3:
                                feature.properties = $.extend({}, workInProgress);
                                break;
                            case 4:
                                feature.properties = $.extend({}, completed);
                                break;
                            default:
                                feature.properties = $.extend({}, potential);
                                break;
                        }
                        items.push(feature);
                    }
                }
            });

            json.features = items;

            var features = map.data.addGeoJson(json);

            if (callback !== undefined) {
                callback(features);
            }

            map.data.setStyle(function (feature) {
                return {
                    strokeColor: feature.getProperty('stroke'),
                    strokeWeight: feature.getProperty('stroke-width'),
                    fillColor: feature.getProperty('fill'),
                    fillOpacity: feature.getProperty('fill-opacity'),
                    icon: feature.getProperty('icon'),
                    zIndex: feature.getProperty('zIndex')
                };
            });
        });
    }
}

