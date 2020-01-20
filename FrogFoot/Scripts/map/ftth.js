
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

'use strict';

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
function parsePos(pos, hemisphere)
{
	function toDecimal(pos)
	{
		var sign;
		
		if (pos[0].match(/^[NSWE]$/))
			sign = pos.shift();
		else if (pos[pos.length-1].match(/^[NSWE]$/))
			sign = pos.pop();
		
		sign = sign === 'W' || sign === 'S' ? -1 : 1;
		
		for (var i = 0; i < pos.length; i++)
			if (!parseFloat(pos[i]))
				throw 'Invalid DMS: '+pos.join(' ');
		
		switch (pos.length) {
		case 3:		return sign*(Number(pos[0]) + Number(pos[1])/60 + Number(pos[2])/3600);
		case 2:		return sign*(Number(pos[0]) + Number(pos[1])/60);
		case 1:		return sign*pos[0];
		default:	throw 'Invalid DMS: '+pos.join(' ');
		}
	}
	
	function checkBounds(hemisphere)
	{
		var bounds = [ -90, 90, -180, 180 ];
		
		if (hemisphere !== undefined) {
			if (!hemisphere.match(/^[NS][EW]$/))
				throw 'Invalid Hemisphere: '+hemisphere;
			
			bounds[hemisphere[0] == 'N' ? 0 : 1] = 0;
			bounds[hemisphere[1] == 'E' ? 2 : 3] = 0;
		}
		
		if (lon >= bounds[0] && lon <= bounds[1] &&
			lat >= bounds[2] && lat <= bounds[3])
			var tmp = lat, lat = lon, lon = tmp;
		
		if (lat < bounds[0] || lat > bounds[1])
			throw 'Invalid Latitude: '+lat.toFixed(7);
		
		if (lon < bounds[2] || lon > bounds[3])
			throw 'Invalid Longitude: '+lon.toFixed(7);
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
		throw 'Invalid GPS Coordinates: '+pos;
	
	var lat = toDecimal(parts.slice(0, parts.length / 2));
	var lng = toDecimal(parts.slice(parts.length / 2));
	
	if (parts[0] == 'E' || parts[0] == 'W' ||
		parts[parts.length-1] == 'N' || parts[parts.length-1] == 'S')
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
function ajaxPost(url, callback)
{
	var request = window.ActiveXObject ?
		new ActiveXObject('Microsoft.XMLHTTP') :
		new XMLHttpRequest;
	
	request.onreadystatechange = function() {
		if (request.readyState == 4 && request.status == 200) {
			request.onreadystatechange = function() {};
			
			try {
				callback(JSON.parse(request.responseText));
			} catch(e) {
				console.log(e.message);
			}
		}
	};
	
	request.open('POST', window.location.origin+url, true);
	request.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
	request.send();
}

/**
 * Fetch GeoJSON from server and add to specified map
 * 
 * @param {google.maps.Map} map - Map
 * @param {String} url - Server Url
 * @param {Function} callback - Feature Callback
 * @return void
 */
function addGeoJson(map, url, callback)
{
	ajaxPost(url, function(json) {
		var features = map.data.addGeoJson(json);
		
		if (callback !== undefined)
			callback(features);
		
		map.data.setStyle(function(feature) {
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
function getServerUrl(map, url, pos)
{
	if (pos === undefined) {
		var bounds = map.getBounds();
		var ne = bounds.getNorthEast();
		var sw = bounds.getSouthWest();
		return url+'?bb='+sw.lng()+','+sw.lat()+','+ne.lng()+','+ne.lat();
	}
	
	return url+'?ll='+pos.lat()+','+pos.lng();
}

/**
 * Get Coordinate String
 * 
 * @param {google.maps.LatLng} pos - Latitude/Longitude
 * @return {String}
 */
function getCoordString(pos)
{
	return '('+pos.lat().toFixed(6)+','+pos.lng().toFixed(6)+')';
}

/**
 * Geocode Position and Update its Marker and Info Window with Address
 * 
 * @param {google.maps.LatLng} pos - Latitude/Longitude
 * @param {google.maps.Marker} marker - Marker
 * @param {google.maps.InfoWindow} infowindow - Info Window
 * @return void
 */
function geocodePosition(pos, marker, infowindow)
{
	var geocoder = new google.maps.Geocoder();
	
	geocoder.geocode({
		latLng: pos
	}, function(responses) {
		var address = responses && responses.length > 0 ?
			responses[0].formatted_address : 'cannot determine address';
		
		marker.setTitle(address+"\n"+getCoordString(pos));
		infowindow.setContent('<b>'+address+"</b><br>"+getCoordString(pos));
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
function removeFeatures(map, infowindow, places)
{
	infowindow.close();
	
	map.data.forEach(function(feature) {
		map.data.remove(feature);
	});
	
	places.forEach(function(place) {
		place.infowindow.close();
		google.maps.event.clearInstanceListeners(place.marker);
		place.marker.setMap(null);
	});
	
	places = [];
}

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
function updateFeatures(map, infowindow, places, pos, name)
{
	removeFeatures(map, infowindow, places);
	var bounds = new google.maps.LatLngBounds();
	
	var marker = new google.maps.Marker({
		map: map,
		title: name+"\n"+getCoordString(pos),
		position: pos,
		draggable: true
	});
	
	var markerwindow = new google.maps.InfoWindow({
		content: '<b>'+name+"</b><br>"+getCoordString(pos),
		position: pos
	});
	
	geocodePosition(pos, marker, markerwindow);
	bounds.extend(pos);
	places.push({ marker: marker, infowindow: markerwindow });
	map.fitBounds(bounds);
	
	google.maps.event.addListener(marker, 'dragend', function() {
		updateFeatures(map, infowindow, places, marker.getPosition());
	});
	
	google.maps.event.addListener(marker, 'click', function() {
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
function getBoundsFromString(bbox)
{
	var c = bbox.split(',');
	var sw = new google.maps.LatLng(parseFloat(c[1]), parseFloat(c[0]));
	var ne = new google.maps.LatLng(parseFloat(c[3]), parseFloat(c[2]));
	return new google.maps.LatLngBounds(sw, ne);
}

google.maps.event.addDomListener(window, 'load', function() {
	var map = new google.maps.Map(document.getElementById('map-canvas'), {
		center: new google.maps.LatLng(-34.035289,18.440520),
		zoom: 13,
		mapTypeId: google.maps.MapTypeId.HYBRID,
	});

	var select = document.getElementById('pac-select');
	map.controls[google.maps.ControlPosition.TOP_CENTER].push(select);
	var input = document.getElementById('pac-input');
	map.controls[google.maps.ControlPosition.TOP_CENTER].push(input);
	var searchbox = new google.maps.places.SearchBox(input);
	var infowindow = new google.maps.InfoWindow();
	var places = [];
	var precincts = [];
	
	select.onchange = function() {
		var precinct = precincts[this.selectedIndex-1];
		map.setCenter(precinct.center);
		map.fitBounds(getBoundsFromString(precinct.bounds));
	}
	
	ajaxPost('/ftth/options', function(json) {
		for (var i in json) {
			var pos = new google.maps.LatLng(json[i]['latitude'], json[i]['longitude']);
			
			precincts[i] = {
				name: json[i]['name'],
				center: pos,
				bounds: json[i]['bounds']
			};
			
			var opt = select.appendChild(document.createElement('option'));
			opt.value = i;
			opt.innerHTML = precincts[i].name;
			/*
			var marker = new google.maps.Marker({
				map: map,
				title: precincts[i].name,
				position: pos,
				icon: 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'
			});
			 */
		}
	});
	
	google.maps.event.addListener(searchbox, 'places_changed', function() {
		try {
			updateFeatures(map, infowindow, places, parsePos(input.value, 'SE'));
		} catch(error) {
			var place = searchbox.getPlaces();
			
			if (place.length)
				updateFeatures(map, infowindow, places, place[0].geometry.location, place[0].name);
			else
				alert('Invalid location: '+input.value+'.');
		}
	});
	
	google.maps.event.addListener(map, 'idle', function() {
		addGeoJson(map, getServerUrl(map, '/ftth/features'), function(features) {
			google.maps.event.addListenerOnce(map, 'idle', function() {
				features.forEach(function(feature) {
					map.data.remove(feature);
				});
			});
		});
		
		searchbox.setBounds(map.getBounds());
	});
});
