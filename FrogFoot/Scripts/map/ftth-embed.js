
/**
 * -*- mode: C; eval: c_set_style("bsd"); -*-
 * 
 * Render FTTH Coverage Maps
 * 
 * @author Abraham van der Merwe <abz@frogfoot.com>
 * @copyright 2015 Frogfoot Networks (Pty) Ltd
 * @license protected/docs/LICENSE
 * @see https://developers.google.com/maps/documentation/javascript/reference
 */

//'use strict';

/**
 * Parse GPS coordinates (Lat, Lon) in any format.
 * 
 * This is very flexible on formats, allowing deg, deg+min, deg+min+sec,
 * signed or unsigned, direction before or after, and pretty much any
 * seperator between numbers.
 * 
 * If a hemisphere is specified, latitude and longitude will automatically
 * be swapped if necessary (i.e. both Lat,Lon and Lon,Lat format supported).
 * 
 * @param {String} pos - Latitude/Longitude String (in any format)
 * @param {String} hemisphere - Expected Hemisphere ('NW', 'NE', 'SW', 'SE')
 * @return {google.maps.LatLng} - parsed Latitude/Longitude
 * @throw {Error} - if GPS coordinates are incorrect or hemisphere format incorrect
 * 
 * @example
 *   console.log(parsePos(" S26° 3.4182', E028° 5.373'"));
 */
function parsePos(pos, hemisphere) {
    function toDecimal(pos) {
        var sign;

        if (pos[0].match(/^[NSWE]$/))
            sign = pos.shift();
        else if (pos[pos.length - 1].match(/^[NSWE]$/))
            sign = pos.pop();

        sign = sign === 'W' || sign === 'S' ? -1 : 1;

        for (var i = 0; i < pos.length; i++)
            if (!parseFloat(pos[i]))
                throw 'Invalid DMS: ' + pos.join(' ');

        switch (pos.length) {
            case 3: return sign * (Number(pos[0]) + Number(pos[1]) / 60 + Number(pos[2]) / 3600);
            case 2: return sign * (Number(pos[0]) + Number(pos[1]) / 60);
            case 1: return sign * pos[0];
            default: throw 'Invalid DMS: ' + pos.join(' ');
        }
    }

    function checkBounds(hemisphere) {
        var bounds = [-90, 90, -180, 180];

        if (hemisphere !== undefined) {
            if (!hemisphere.match(/^[NS][EW]$/))
                throw 'Invalid Hemisphere: ' + hemisphere;

            bounds[hemisphere[0] == 'N' ? 0 : 1] = 0;
            bounds[hemisphere[1] == 'E' ? 2 : 3] = 0;
        }

        if (lon >= bounds[0] && lon <= bounds[1] &&
            lat >= bounds[2] && lat <= bounds[3])
            var tmp = lat, lat = lon, lon = tmp;

        if (lat < bounds[0] || lat > bounds[1])
            throw 'Invalid Latitude: ' + lat.toFixed(7);

        if (lon < bounds[2] || lon > bounds[3])
            throw 'Invalid Longitude: ' + lon.toFixed(7);
    }

    var parts = pos.
        trim().
        toUpperCase().
        replace(/[NSWE]/g, ' $& ').
        replace(/[\s,;?°\'’′\"″]+/g, ' ').
        replace(/ +/g, ' ').
        trim().
        split(' ');

    if ((parts.length % 2) || parts.length < 2 || parts.length > 8)
        throw 'Invalid GPS Coordinates: ' + pos;

    var lat = toDecimal(parts.slice(0, parts.length / 2));
    var lng = toDecimal(parts.slice(parts.length / 2));

    if (parts[0] == 'E' || parts[0] == 'W' ||
        parts[parts.length - 1] == 'N' || parts[parts.length - 1] == 'S')
        var tmp = lat, lat = lng, lng = tmp;

    checkBounds(hemisphere);

    return new google.maps.LatLng(lat, lng);
}

/**
 * Fetch JSON from server
 * 
 * @param {String} url - Server Url (relative to window.location.origin)
 * @param {String} callback - Callback Function
 * @return void
 */
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

    //request.open('POST', window.location.origin+url, true);
    request.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
    request.send();
}

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

/**
 * Fetch GeoJSON from server and add to specified map
 * 
 * @param {google.maps.Map} map - Map
 * @param {String} url - Server Url
 * @param {Function} callback - Feature Callback
 * @return void
 */
function addGeoJson(map, url, callback) {
    ajaxPost(url, function (json) {
        var items = [];
        //check the location names against features
        $.each(json.features, function (i, x) {
            if ($.inArray(x.properties.title, locations) > -1 && x.properties.fill !== '#000000') {
                x.properties = precinct; //set the style to precinct
                items.push(x);
            }
        });

        //check the zone codes against features
        $.each(json.features, function (i, _feature) {
            for (var j = 0; j < zones.length; j++) {
                var _zone = zones[j];

                if (_zone.Code === _feature.properties.title) {

                    switch (_zone.Status) {
                        case 1:
                            _feature.properties = $.extend({}, potential);
                            break;
                        case 2:
                            _feature.properties = $.extend({}, scheduled);
                            break;
                        case 3:
                            _feature.properties = $.extend({}, workInProgress);
                            break;
                        case 4:
                            _feature.properties = $.extend({}, completed);
                            break;
                        default:
                            feature.properties = $.extend({}, potential);
                            break;
                    }

                    if ($('#useZoneInfoClick').length) { //check if the map must show the Zone Info
                        _feature.properties.zoneId = _zone.ZoneId;
                    }
                    items.push(_feature);
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

/**
 * Get Server Url
 * 
 * @param {google.maps.Map} map - Map
 * @param {String} url - Server Url
 * @param {google.maps.LatLng} pos - Lat/Lng Attribute (optional)
 * @return string
 */
function getServerUrl(map, url, pos) {
    if (pos === undefined) {
        var bounds = map.getBounds();
        var ne = bounds.getNorthEast();
        var sw = bounds.getSouthWest();
        return url + '?bb=' + sw.lng() + ',' + sw.lat() + ',' + ne.lng() + ',' + ne.lat();
    }

    return url + '?ll=' + pos.lat() + ',' + pos.lng();
}

/**
 * Get Coordinate String
 * 
 * @param {google.maps.LatLng} pos - Latitude/Longitude
 * @return {String}
 */
function getCoordString(pos) {
    return '(' + pos.lat().toFixed(6) + ',' + pos.lng().toFixed(6) + ')';
}

/**
 * Geocode Position and Update its Marker and Info Window with Address
 * 
 * @param {google.maps.LatLng} pos - Latitude/Longitude
 * @param {google.maps.Marker} marker - Marker
 * @param {google.maps.InfoWindow} infowindow - Info Window
 * @return void
 */
function geocodePosition(pos, marker, infowindow) {
    var geocoder = new google.maps.Geocoder();

    geocoder.geocode({
        latLng: pos
    }, function (responses) {
        var address = responses && responses.length > 0 ?
            responses[0].formatted_address : 'cannot determine address';

        //set co-ords to inputs for saving
        if ($('#User_Id').length) {
            //for logged in user
            $('#User_Latitude').val(pos.lat().toFixed(6));
            $('#User_Longitude').val(pos.lng().toFixed(6));
        } else {
            //for register page
            $('#Latitude').val(pos.lat().toFixed(6));
            $('#Longitude').val(pos.lng().toFixed(6));
        }

        marker.setTitle(address + "\n" + getCoordString(pos));
        infowindow.setContent('<b>' + address + "</b><br>" + getCoordString(pos));
    });
}

/**
 * Remove Features
 * 
 * @param {google.maps.Map} map - Map
 * @param {google.maps.InfoWindow} infowindow - Info Window
 * @param {Array} places - Places
 * @return void
 */
function removeFeatures(map, infowindow, places) {
    infowindow.close();

    map.data.forEach(function (feature) {
        map.data.remove(feature);
    });

    places.forEach(function (place) {
        place.infowindow.close();
        google.maps.event.clearInstanceListeners(place.marker);
        place.marker.setMap(null);
    });

    places = [];
}

/**
 * Display Survey Results
 * 
 * @param {google.maps.LatLng} pos - Latitude/Longitude
 * @return void
 */
function displaySurveyResults(pos) {
    var url = '/ftth/check?ll=' + pos.lat() + ',' + pos.lng();
    var html = document.getElementById('map-results');

    ajaxPost(url, function (json) {
        var inPrecinct = json['possible'];
        var results =  inPrecinct ?
            'You are located in ' + json['precinct-name'] :
            'Right now, your address is not included in a Frogfoot FTTH precinct, however we do acknowledge areas where interest is high. The more neighbours you can persuade to register interest, the higher the chance that your address could become part of a Frogfoot FTTH precinct!';

        html.style.display = 'block';
        html.innerHTML = '<label class=\"control-label\" data-inprecinct=\"' + inPrecinct + '\">Results:</label>' + '<span> ' + results + '</span>';
    });
}

//marker variable to keep track of the map marker
var marker;

/**
 * Update Features
 * 
 * @param {google.maps.Map} map - Map
 * @param {google.maps.InfoWindow} infowindow - Info Window
 * @param {Array} places - Places
 * @param {google.maps.LatLng} pos - Latitude/Longitude
 * @param {String} name - Place Name (if any)
 * @return void
 */
function updateFeatures(map, infowindow, places, pos, name) {
    removeFeatures(map, infowindow, places);
    var bounds = new google.maps.LatLngBounds();

    if (marker != null) {
        marker.setMap(null);
    }

    marker = new google.maps.Marker({
        map: map,
        title: name + "\n" + getCoordString(pos),
        position: pos,
        draggable: true
    });

    var markerwindow = new google.maps.InfoWindow({
        content: '<b>' + name + "</b><br>" + getCoordString(pos),
        position: pos
    });

    geocodePosition(pos, marker, markerwindow);
    bounds.extend(pos);
    places.push({ marker: marker, infowindow: markerwindow });
    map.fitBounds(bounds);
    displaySurveyResults(pos);

    google.maps.event.addListener(marker, 'dragend', function (evt) {
        updateFeatures(map, infowindow, places, marker.getPosition());

        //check if user is logged in
        if ($('#User_Id').length) {
            $('#User_Latitude').val(evt.latLng.lat().toFixed(6));
            $('#User_Longitude').val(evt.latLng.lng().toFixed(6));
        } else {
            $('#Latitude').val(evt.latLng.lat().toFixed(6));
            $('#Longitude').val(evt.latLng.lng().toFixed(6));
        }
    });

    google.maps.event.addListener(marker, 'click', function () {
        infowindow.close();
        markerwindow.open(map);
    });

    if (map.getZoom() > 18)
        map.setZoom(18);
}

/**
 * Convert Bounding Box from string value to LatLngBounds class
 * 
 * @param {String} bbox - Bounding Box (W,S,E,N)
 * @return {google.maps.LatLngBounds}
 */
function getBoundsFromString(bbox) {
    var c = bbox.split(',');
    var sw = new google.maps.LatLng(parseFloat(c[1]), parseFloat(c[0]));
    var ne = new google.maps.LatLng(parseFloat(c[3]), parseFloat(c[2]));
    return new google.maps.LatLngBounds(sw, ne);
}

//create map legend
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

$.when(getPortalZones(), getPortalLocations()).done(function () {
    setTimeout(function () {
        var map = new google.maps.Map(document.getElementById('map-canvas'), {
            center: new google.maps.LatLng(-34.035289, 18.440520),
            zoom: 13,
            mapTypeId: google.maps.MapTypeId.HYBRID
        });
        buildLegend();
        map.controls[google.maps.ControlPosition.LEFT_BOTTOM].push(document.getElementById('legend'));
        var select = document.getElementById('pac-select');
        var input = document.getElementById('pac-input');
        var searchbox = new google.maps.places.SearchBox(input);
        var infowindow = new google.maps.InfoWindow();
        var places = [];
        var precincts = [];

        if (select != null) {
            select.onchange = function () {
                var precinct = precincts[this.selectedIndex - 1];
                if (precinct != null) {
                    map.setCenter(precinct.center);
                    map.fitBounds(getBoundsFromString(precinct.bounds));
                }
            }
        }

        var lat = $('#Latitude').val();
        var lng = $("#Longitude").val();

        //for logged in user
        if ($('#User_Id').length) {
            lat = $('#User_Latitude').val();
            lng = $("#User_Longitude").val();
        }

        //if lat long exists create marker
        if ((typeof lat !== "undefined" && lat !== "0" && lat.length) && (typeof lng !== "undefined" && lng.length && lng !== "0")) {
            var pos = new google.maps.LatLng(lat, lng);
            map.setCenter(pos);
            map.setZoom(17);

            if (marker == null) {
                marker = new google.maps.Marker({
                    map: map,
                    position: pos,
                    draggable: true
                });
            }
            if ($('#User_Id').length) {
                //attach drag event for logged in user
                google.maps.event.addListener(marker, 'dragend', function (evt) {
                    $('#User_Latitude').val(evt.latLng.lat().toFixed(6));
                    $('#User_Longitude').val(evt.latLng.lng().toFixed(6));
                });
            }
        }

        ajaxPost('/ftth/options', function (json) {
            for (var i in json) {
                var pos = new google.maps.LatLng(json[i]['latitude'], json[i]['longitude']);

                precincts[i] = {
                    name: json[i]['name'],
                    center: pos,
                    bounds: json[i]['bounds'],
                    id: json[i]['id']
                };

                if (select != null) {
                    var opt = select.appendChild(document.createElement('option'));
                    opt.value = i;
                    opt.innerHTML = precincts[i].name;
                    opt.setAttribute('data-id', precincts[i].id);
                }
            }
        });

        google.maps.event.addListener(searchbox, 'places_changed', function () {
            try {
                updateFeatures(map, infowindow, places, parsePos(input.value, 'SE'));
            } catch (error) {
                var place = searchbox.getPlaces();

                if (place.length)
                    updateFeatures(map, infowindow, places, place[0].geometry.location, place[0].name);
                else
                    alert('Invalid location: ' + input.value + '.');
            }
        });

        //if user doesn't select an address from the google places dropdown, try find the address 
        //input.addEventListener('change', function () {
        //    var geo = new google.maps.Geocoder;
        //    geo.geocode({ componentRestrictions: { country: 'ZA' }, 'address': this.value },
        //        function (results, status) {
        //            if (status == google.maps.GeocoderStatus.OK) {
        //                if (results.length) {
        //                    updateFeatures(map, infowindow, places, results[0].geometry.location, results[0].name);
        //                    $('#pac-input').val(results[0].formatted_address);
        //                    $('#Latitude').val(results[0].geometry.location.lat());
        //                    $('#Longitude').val(results[0].geometry.location.lng());
        //                } else {
        //                    alert('Invalid location: ' + input.value + '. Please enter your address and select it from the dropdown.');
        //                }
        //            } else {
        //                alert("Finding your location was not successful for the following reason: " + status);
        //            }
        //        });
        //});

        google.maps.event.addListener(map, 'idle', function () {
            addGeoJson(map, getServerUrl(map, '/ftth/features'), function (features) {
                google.maps.event.addListenerOnce(map, 'idle', function () {
                    features.forEach(function (feature) {
                        map.data.remove(feature);
                    });
                });
            });

            if ($('#useZoneInfoClick').length) {
                map.data.addListener('click', function (e) {
                    var inMemoryZone = $.grep(zones, function (z) {
                        var mapZoneId = e.feature.getProperty('zoneId');
                        return z.ZoneId == mapZoneId;
                    });

                    if (inMemoryZone.length) {
                        var fdof, ldof;

                        if (inMemoryZone[0].FirstDateOfFibre != null && inMemoryZone[0].LastDateOfFibre != null) {
                            fdof = parseInt(inMemoryZone[0].FirstDateOfFibre.substr(6));
                            ldof = parseInt(inMemoryZone[0].LastDateOfFibre.substr(6));
                        }

                        $('#zoneInfoName').text(inMemoryZone[0].Code);
                        $('#zoneInfoStatus').text(inMemoryZone[0].FibreStatusName);
                        if (fdof != null && ldof != null) {
                            $('#zoneInfoFDOF').text(moment(fdof).format('DD/MM/YYYY'));
                            $('#zoneInfoLDOF').text(moment(ldof).format('DD/MM/YYYY'));
                        }
                        $('#zoneInfoContainer').show();
                    } else {
                        $('#zoneInfoContainer').hide();

                    }
                });
            }

            searchbox.setBounds(map.getBounds());
        });
    }, 20);
});