import { KrakowPlace } from '@/services/krakowData';

export function generateLeafletHtml(
  places: KrakowPlace[],
  selectedPlaceId?: string,
  userLocation?: { latitude: number; longitude: number } | null,
  isHighContrast?: boolean,
  isDark?: boolean,
  activePresetId?: string | null
): string {
  const placesJson = JSON.stringify(places);
  const selectedIdJson = JSON.stringify(selectedPlaceId || null);
  const userLocJson = JSON.stringify(userLocation || null);
  const activePresetIdJson = JSON.stringify(activePresetId || null);

  return `<!DOCTYPE html>
<html lang="pl">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
  <title>Smok Przewodnik Mapa</title>
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=" crossorigin="" />
  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js" integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=" crossorigin=""></script>
  <style>
    /* Clean reset for map container only */
    html, body, #map {
      width: 100%;
      height: 100%;
      margin: 0;
      padding: 0;
      overflow: hidden;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
    }

    body.dark-mode, body.dark-mode #map {
      background: #0F172A;
    }

    /* High contrast tile overlay */
    body.high-contrast .leaflet-tile-pane {
      filter: contrast(135%) saturate(120%) brightness(95%);
    }

    /* Transparent marker containers */
    .poi-pin-icon, .search-pin-icon, .user-loc-icon {
      background: transparent !important;
      border: none !important;
      cursor: pointer;
    }

    /* Custom Leaflet Tooltip for labels positioned cleanly above pin */
    .poi-map-tooltip {
      background: #FFFFFF !important;
      color: #0F172A !important;
      font-weight: 700 !important;
      font-size: 11px !important;
      border: 1px solid #CBD5E1 !important;
      border-radius: 6px !important;
      box-shadow: 0 2px 6px rgba(0,0,0,0.15) !important;
      padding: 3px 8px !important;
      white-space: nowrap !important;
      pointer-events: auto !important;
      cursor: pointer !important;
      transition: border-color 0.2s, box-shadow 0.2s;
    }
    .poi-map-tooltip::before {
      border-top-color: #CBD5E1 !important;
    }
    .poi-map-tooltip.selected {
      border: 2px solid #004F9E !important;
      color: #004F9E !important;
      font-weight: 800 !important;
      box-shadow: 0 3px 10px rgba(0,79,158,0.3) !important;
    }
    .poi-map-tooltip.selected::before {
      border-top-color: #004F9E !important;
    }

    /* Dark Mode Tooltips */
    body.dark-mode .poi-map-tooltip {
      background: #1E293B !important;
      color: #F8FAFC !important;
      border: 1px solid #334155 !important;
      box-shadow: 0 2px 8px rgba(0,0,0,0.5) !important;
    }
    body.dark-mode .poi-map-tooltip::before {
      border-top-color: #334155 !important;
    }
    body.dark-mode .poi-map-tooltip.selected {
      border: 2px solid #38BDF8 !important;
      color: #38BDF8 !important;
      box-shadow: 0 3px 12px rgba(56,189,248,0.4) !important;
    }
    body.dark-mode .poi-map-tooltip.selected::before {
      border-top-color: #38BDF8 !important;
    }

    /* High Contrast Tooltips */
    body.high-contrast .poi-map-tooltip {
      background: #000000 !important;
      color: #FFFFFF !important;
      border: 2px solid #FFFFFF !important;
      font-weight: 900 !important;
    }
    body.high-contrast .poi-map-tooltip::before {
      border-top-color: #FFFFFF !important;
    }
    body.high-contrast .poi-map-tooltip.selected {
      background: #FFFFFF !important;
      color: #000000 !important;
      border: 3px solid #000000 !important;
      outline: 2px solid #FFFFFF !important;
    }
    body.high-contrast .poi-map-tooltip.selected::before {
      border-top-color: #000000 !important;
    }

    /* User location marker with pulsing animation */
    .user-location-marker {
      width: 24px;
      height: 24px;
      position: relative;
    }
    .user-location-pulse {
      position: absolute;
      width: 36px;
      height: 36px;
      left: -6px;
      top: -6px;
      background: rgba(0, 135, 121, 0.35);
      border-radius: 50%;
      animation: pulse 1.8s infinite;
    }
    body.dark-mode .user-location-pulse {
      background: rgba(56, 189, 248, 0.4);
    }
    .user-location-dot {
      position: absolute;
      width: 18px;
      height: 18px;
      left: 3px;
      top: 3px;
      background: #004F9E;
      border: 3px solid #FFFFFF;
      border-radius: 50%;
      box-shadow: 0 2px 6px rgba(0,0,0,0.4);
    }
    body.dark-mode .user-location-dot {
      background: #38BDF8;
      border-color: #0F172A;
    }
    @keyframes pulse {
      0% { transform: scale(0.8); opacity: 0.8; }
      70% { transform: scale(1.6); opacity: 0; }
      100% { transform: scale(1.6); opacity: 0; }
    }
    body.high-contrast .user-location-dot {
      background: #000000;
      border-color: #FFFFFF;
      outline: 2px solid #000000;
    }

    .leaflet-control-attribution {
      font-size: 9px !important;
      background: rgba(255, 255, 255, 0.85) !important;
    }
    body.dark-mode .leaflet-control-attribution {
      background: rgba(15, 23, 42, 0.85) !important;
      color: #94A3B8 !important;
    }
    body.dark-mode .leaflet-control-attribution a {
      color: #38BDF8 !important;
    }
    body.dark-mode .leaflet-bar a {
      background-color: #1E293B !important;
      color: #F8FAFC !important;
      border-bottom-color: #334155 !important;
    }
  </style>
</head>
<body class="${isHighContrast ? 'high-contrast' : ''} ${isDark ? 'dark-mode' : ''}">
  <div id="map"></div>

  <script>
    var places = ${placesJson};
    var selectedId = ${selectedIdJson};
    var userLocation = ${userLocJson};
    var isHc = ${isHighContrast ? 'true' : 'false'};
    var isDarkMode = ${isDark ? 'true' : 'false'};
    var activePreset = ${activePresetIdJson};

    var tileUrl = 'https://tile.openstreetmap.org/{z}/{x}/{y}.png';

    // Initialize Leaflet Map centered on Kraków Old Town
    var map = L.map('map', {
      center: [50.0617, 19.9373],
      zoom: 15,
      zoomControl: false,
      attributionControl: false,
      markerZoomAnimation: false,
      zoomAnimation: true,
      fadeAnimation: true
    });

    var currentTileLayer = L.tileLayer(tileUrl, {
      maxZoom: 19,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    function setDarkMode(enabled) {
      isDarkMode = !!enabled;
      if (isDarkMode) {
        document.body.classList.add('dark-mode');
      } else {
        document.body.classList.remove('dark-mode');
      }
      renderPlaces(places, selectedId);
      if (lastRouteCoords) {
        drawRoute(lastRouteCoords, lastRouteProfile);
      }
    }

    L.control.zoom({ position: 'bottomright' }).addTo(map);

    var markersMap = {};
    var userMarker = null;
    var searchMarker = null;
    var routePolyline = null;
    var routeOutline = null;
    var routeOriginMarker = null;
    var lastRouteCoords = null;
    var lastRouteProfile = null;

    function drawRoute(coords, profileType) {
      clearRoute();
      if (!coords || !coords.length) return;
      lastRouteCoords = coords;
      lastRouteProfile = profileType;

      var mainColor = '#008779';
      if (profileType === 'fastest') mainColor = isDarkMode ? '#38BDF8' : '#004F9E';
      if (profileType === 'quietest') mainColor = isDarkMode ? '#4ADE80' : '#15803D';
      if (profileType === 'best_supported') mainColor = isDarkMode ? '#A78BFA' : '#7C3AED';
      if (profileType === 'easiest') mainColor = isDarkMode ? '#2DD4BF' : '#008779';
      if (isHc) mainColor = isDarkMode ? '#FFFFFF' : '#000000';

      // Outer contrasting casing line
      var outlineColor = isHc ? (isDarkMode ? '#000000' : '#FFFFFF') : (isDarkMode ? '#0F172A' : '#FFFFFF');
      routeOutline = L.polyline(coords, {
        color: outlineColor,
        weight: isHc ? 10 : 8,
        opacity: 0.95,
        lineCap: 'round',
        lineJoin: 'round'
      }).addTo(map);

      // Main high-visibility route stroke
      routePolyline = L.polyline(coords, {
        color: mainColor,
        weight: isHc ? 6 : 5,
        opacity: 1.0,
        lineCap: 'round',
        lineJoin: 'round'
      }).addTo(map);

      // Start origin pin/dot (Point A)
      var startCoord = coords[0];
      var startIcon = L.divIcon({
        className: 'user-loc-icon',
        html: '<div style="width:22px;height:22px;border-radius:50%;background:#004F9E;border:3px solid #FFF;box-shadow:0 2px 6px rgba(0,0,0,0.4);display:flex;align-items:center;justify-content:center;color:#FFF;font-size:11px;font-weight:900;">A</div>',
        iconSize: [22, 22],
        iconAnchor: [11, 11]
      });
      routeOriginMarker = L.marker(startCoord, { icon: startIcon, zIndexOffset: 2500 }).addTo(map);

      // Fit map view to encompass the entire route
      try {
        var bounds = routePolyline.getBounds();
        if (bounds.isValid()) {
          map.fitBounds(bounds, { padding: [55, 55], animate: true, duration: 0.8 });
        }
      } catch (err) {}
    }

    function clearRoute() {
      lastRouteCoords = null;
      lastRouteProfile = null;
      if (routePolyline) {
        map.removeLayer(routePolyline);
        routePolyline = null;
      }
      if (routeOutline) {
        map.removeLayer(routeOutline);
        routeOutline = null;
      }
      if (routeOriginMarker) {
        map.removeLayer(routeOriginMarker);
        routeOriginMarker = null;
      }
    }

    // Map background tap listener to collapse or dismiss bottom sheet
    map.on('click', function(e) {
      var msg = JSON.stringify({ type: 'MAP_CLICKED', lat: e.latlng.lat, lng: e.latlng.lng });
      sendToParent(msg);
    });

    function sendToParent(msg) {
      if (window.ReactNativeWebView && window.ReactNativeWebView.postMessage) {
        window.ReactNativeWebView.postMessage(msg);
      } else if (window.parent && window.parent !== window) {
        window.parent.postMessage(msg, '*');
      }
    }

    map.on('moveend', function() {
      var c = map.getCenter();
      sendToParent(JSON.stringify({ type: 'MAP_MOVED', lat: c.lat, lng: c.lng }));
    });

    function getIconGlyph(cat) {
      if (cat === 'monument') return '🏰';
      if (cat === 'transit') return '🚆';
      if (cat === 'restroom') return '🚻';
      if (cat === 'park') return '🌳';
      if (cat === 'culture') return '🏛️';
      return '📍';
    }

    // Creates an SVG teardrop pin where the bottom point is mathematically locked to the exact GPS coordinate
    function createPinIcon(place, isSelected, highContrast) {
      var glyph = getIconGlyph(place.category);
      var w = isSelected ? 38 : 32;
      var h = isSelected ? 48 : 42;
      var ax = isSelected ? 19 : 16;
      var ay = h; // Exact bottom tip of the pin
      var pinColor = isSelected ? (highContrast ? '#000000' : '#004F9E') : (highContrast ? '#005A4E' : '#008779');
      
      if (activePreset === 'wheelchair') {
        if (place.wheelchairAccess === 'full') pinColor = '#16A34A';
        else if (place.wheelchairAccess === 'limited') pinColor = '#EAB308';
        else if (place.wheelchairAccess === 'none') pinColor = '#DC2626';
        else pinColor = '#757575'; // Gray if unknown/no data
      }
      
      var strokeColor = '#FFFFFF';
      var strokeW = isSelected ? 2.5 : 2;
      var cr = isSelected ? 13 : 11;
      var cy = isSelected ? 18 : 16;
      var gSize = isSelected ? 15 : 13;

      var pathD = isSelected
        ? 'M19 48C19 48 36 30 36 18C36 8.6 28.4 1 19 1C9.6 1 2 8.6 2 18C2 30 19 48 19 48Z'
        : 'M16 42C16 42 30 26 30 16C30 7.7 23.7 1 16 1C8.3 1 2 7.7 2 16C2 26 16 42 16 42Z';

      var svg = '<svg width="' + w + '" height="' + h + '" viewBox="0 0 ' + w + ' ' + h + '" fill="none" xmlns="http://www.w3.org/2000/svg" style="display:block;filter:drop-shadow(0px 3px 6px rgba(0,0,0,0.35));">' +
        '<path d="' + pathD + '" fill="' + pinColor + '" stroke="' + strokeColor + '" stroke-width="' + strokeW + '"/>' +
        '<circle cx="' + ax + '" cy="' + cy + '" r="' + cr + '" fill="#FFFFFF"/>' +
        '<text x="' + ax + '" y="' + (cy + 1) + '" font-size="' + gSize + '" text-anchor="middle" dominant-baseline="central">' + glyph + '</text>' +
      '</svg>';

      return L.divIcon({
        className: 'poi-pin-icon' + (isSelected ? ' selected' : ''),
        html: svg,
        iconSize: [w, h],
        iconAnchor: [ax, ay],
        tooltipAnchor: [0, -h]
      });
    }

    function renderPlaces(items, activeId) {
      // Clear existing markers
      Object.keys(markersMap).forEach(function(k) {
        map.removeLayer(markersMap[k]);
      });
      markersMap = {};

      items.forEach(function(place) {
        var isSelected = place.id === activeId;
        var icon = createPinIcon(place, isSelected, isHc);

        var marker = L.marker([place.coordinates.latitude, place.coordinates.longitude], {
          icon: icon,
          zIndexOffset: isSelected ? 1000 : 100
        }).addTo(map);

        // Native Leaflet tooltip positioned strictly above the pin tip, avoiding anchor drift
        marker.bindTooltip(place.name, {
          permanent: true,
          direction: 'top',
          offset: [0, -4],
          className: 'poi-map-tooltip' + (isSelected ? ' selected' : '')
        });

        marker.on('click', function(ev) {
          L.DomEvent.stopPropagation(ev);
          selectPlace(place.id, true);
        });

        marker.on('tooltipopen', function(e) {
          if (e.tooltip && e.tooltip._container) {
            e.tooltip._container.onclick = function(ev) {
              ev.stopPropagation();
              selectPlace(place.id, true);
            };
          }
        });

        markersMap[place.id] = marker;
      });
    }

    function updateUserMarker(coords, shouldPan) {
      if (!coords) return;
      if (userMarker) {
        map.removeLayer(userMarker);
      }

      var userIcon = L.divIcon({
        className: 'user-loc-icon',
        html: '<div class="user-location-marker">' +
          '<div class="user-location-pulse"></div>' +
          '<div class="user-location-dot"></div>' +
        '</div>',
        iconSize: [24, 24],
        iconAnchor: [12, 12]
      });

      userMarker = L.marker([coords.latitude, coords.longitude], {
        icon: userIcon,
        zIndexOffset: 2000
      }).addTo(map);

      if (shouldPan) {
        map.flyTo([coords.latitude, coords.longitude], 16, { animate: true, duration: 1.0 });
      }
    }

    function setSearchPin(coords, label) {
      if (searchMarker) {
        map.removeLayer(searchMarker);
      }

      var searchSvg = '<svg width="36" height="46" viewBox="0 0 36 46" fill="none" xmlns="http://www.w3.org/2000/svg" style="display:block;filter:drop-shadow(0px 3px 6px rgba(0,0,0,0.4));">' +
        '<path d="M18 46C18 46 34 29 34 17C34 8.2 26.8 1 18 1C9.2 1 2 8.2 2 17C2 29 18 46 18 46Z" fill="#DC2626" stroke="#FFFFFF" stroke-width="2.5"/>' +
        '<circle cx="18" cy="17" r="12" fill="#FFFFFF"/>' +
        '<text x="18" y="18" font-size="14" text-anchor="middle" dominant-baseline="central">📍</text>' +
      '</svg>';

      var searchIcon = L.divIcon({
        className: 'search-pin-icon',
        html: searchSvg,
        iconSize: [36, 46],
        iconAnchor: [18, 46],
        tooltipAnchor: [0, -46]
      });

      searchMarker = L.marker([coords.latitude, coords.longitude], {
        icon: searchIcon,
        zIndexOffset: 3000
      }).addTo(map);

      searchMarker.bindTooltip(label || 'Wyszukany adres', {
        permanent: true,
        direction: 'top',
        offset: [0, -4],
        className: 'poi-map-tooltip selected'
      });

      map.flyTo([coords.latitude, coords.longitude], 17, { animate: true, duration: 1.0 });
    }

    function selectPlace(placeId, notifyParent) {
      selectedId = placeId;
      renderPlaces(places, selectedId);

      var p = places.find(function(x) { return x.id === placeId; });
      if (p) {
        map.flyTo([p.coordinates.latitude, p.coordinates.longitude], 16, { animate: true, duration: 0.8 });
      }

      if (notifyParent) {
        var msg = JSON.stringify({ type: 'SELECT_PLACE', placeId: placeId });
        sendToParent(msg);
      }
    }

    // Initial render
    renderPlaces(places, selectedId);
    if (userLocation) {
      updateUserMarker(userLocation, false);
    }
    if (selectedId) {
      var initialPlace = places.find(function(x) { return x.id === selectedId; });
      if (initialPlace) {
        map.setView([initialPlace.coordinates.latitude, initialPlace.coordinates.longitude], 15);
      }
    }

    function handleIncomingData(data) {
      if (!data || !data.type) return;

      if (data.type === 'SET_SELECTED_PLACE') {
        selectPlace(data.placeId, false);
      } else if (data.type === 'SET_USER_LOCATION') {
        userLocation = data.coords;
        updateUserMarker(data.coords, data.panToUser === true);
      } else if (data.type === 'SET_SEARCH_PIN') {
        setSearchPin(data.coords, data.label);
      } else if (data.type === 'SET_HIGH_CONTRAST') {
        isHc = !!data.enabled;
        if (isHc) {
          document.body.classList.add('high-contrast');
        } else {
          document.body.classList.remove('high-contrast');
        }
        renderPlaces(places, selectedId);
      } else if (data.type === 'SET_DARK_MODE') {
        setDarkMode(data.enabled);
      } else if (data.type === 'UPDATE_PLACES') {
        places = data.places;
        renderPlaces(places, selectedId);
      } else if (data.type === 'DRAW_ROUTE') {
        drawRoute(data.coordinates, data.profileType);
      } else if (data.type === 'CLEAR_ROUTE') {
        clearRoute();
      } else if (data.type === 'FOCUS_MANEUVER') {
        if (data.location) {
          map.flyTo([data.location[0], data.location[1]], 17, { animate: true, duration: 0.6 });
        }
      }
    }

    // Bridge listeners
    window.addEventListener('message', function(event) {
      try {
        var data = typeof event.data === 'string' ? JSON.parse(event.data) : event.data;
        handleIncomingData(data);
      } catch (err) {}
    });

    document.addEventListener('message', function(event) {
      try {
        var data = JSON.parse(event.data);
        handleIncomingData(data);
      } catch (err) {}
    });

    // Notify parent that map is ready
    sendToParent(JSON.stringify({ type: 'MAP_READY' }));
  </script>
</body>
</html>`;
}
