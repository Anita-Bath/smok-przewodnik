import { KrakowPlace } from '@/services/krakowData';

export function generateLeafletHtml(
  places: KrakowPlace[],
  selectedPlaceId?: string,
  userLocation?: { latitude: number; longitude: number } | null,
  isHighContrast?: boolean
): string {
  const placesJson = JSON.stringify(places);
  const selectedIdJson = JSON.stringify(selectedPlaceId || null);
  const userLocJson = JSON.stringify(userLocation || null);

  return `<!DOCTYPE html>
<html lang="pl">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
  <title>Smok Przewodnik Mapa</title>
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=" crossorigin="" />
  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js" integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=" crossorigin=""></script>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; -webkit-tap-highlight-color: transparent; }
    html, body, #map { width: 100%; height: 100%; overflow: hidden; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; }
    
    /* High contrast tile overlay */
    body.high-contrast .leaflet-tile-pane {
      filter: contrast(135%) saturate(120%) brightness(95%);
    }

    /* Custom Marker Styles */
    .poi-marker-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      cursor: pointer;
      transform: translate(-50%, -100%);
    }

    .poi-icon-bubble {
      width: 38px;
      height: 38px;
      border-radius: 50%;
      background: #008779;
      border: 2.5px solid #FFFFFF;
      box-shadow: 0 3px 8px rgba(0,0,0,0.3);
      display: flex;
      align-items: center;
      justify-content: center;
      color: #FFFFFF;
      font-size: 18px;
      transition: transform 0.2s cubic-bezier(0.34, 1.56, 0.64, 1), background-color 0.2s;
    }

    .poi-marker-container.selected .poi-icon-bubble {
      background: #004F9E;
      transform: scale(1.22);
      border-color: #FFFFFF;
      box-shadow: 0 4px 12px rgba(0,79,158,0.45);
    }

    body.high-contrast .poi-icon-bubble {
      background: #FFFFFF;
      color: #000000;
      border: 3px solid #000000;
      font-weight: 900;
    }

    body.high-contrast .poi-marker-container.selected .poi-icon-bubble {
      background: #000000;
      color: #FFFFFF;
      border: 3px solid #FFFFFF;
      outline: 2px solid #000000;
    }

    .poi-badge {
      background: #FFFFFF;
      border: 1px solid #CBD5E1;
      border-radius: 8px;
      padding: 3px 8px;
      margin-top: 4px;
      font-size: 11px;
      font-weight: 700;
      color: #0F172A;
      white-space: nowrap;
      box-shadow: 0 2px 4px rgba(0,0,0,0.15);
      pointer-events: none;
    }

    .poi-marker-container.selected .poi-badge {
      border: 2px solid #004F9E;
      color: #004F9E;
      font-weight: 800;
    }

    body.high-contrast .poi-badge {
      background: #FFFFFF;
      border: 2px solid #000000;
      color: #000000;
      font-weight: 900;
    }

    body.high-contrast .poi-marker-container.selected .poi-badge {
      background: #000000;
      color: #FFFFFF;
      border: 2px solid #FFFFFF;
      outline: 2px solid #000000;
    }

    /* User location marker */
    .user-location-marker {
      width: 22px;
      height: 22px;
      position: relative;
    }
    .user-location-pulse {
      position: absolute;
      width: 36px;
      height: 36px;
      left: -7px;
      top: -7px;
      background: rgba(0, 135, 121, 0.35);
      border-radius: 50%;
      animation: pulse 1.8s infinite;
    }
    .user-location-dot {
      position: absolute;
      width: 18px;
      height: 18px;
      left: 2px;
      top: 2px;
      background: #004F9E;
      border: 3px solid #FFFFFF;
      border-radius: 50%;
      box-shadow: 0 2px 6px rgba(0,0,0,0.4);
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

    /* Attribution style tweak */
    .leaflet-control-attribution {
      font-size: 9px !important;
      background: rgba(255, 255, 255, 0.85) !important;
    }
  </style>
</head>
<body class="${isHighContrast ? 'high-contrast' : ''}">
  <div id="map"></div>

  <script>
    var places = ${placesJson};
    var selectedId = ${selectedIdJson};
    var userLocation = ${userLocJson};
    var isHc = ${isHighContrast ? 'true' : 'false'};

    // Initialize Leaflet Map centered on Kraków Old Town
    var map = L.map('map', {
      center: [50.0617, 19.9373],
      zoom: 15,
      zoomControl: false,
      attributionControl: true
    });

    // Add OpenStreetMap tiles
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(map);

    L.control.zoom({ position: 'bottomright' }).addTo(map);

    var markersMap = {};
    var userMarker = null;

    function getIconGlyph(cat) {
      if (cat === 'monument') return '🏰';
      if (cat === 'transit') return '🚆';
      if (cat === 'restroom') return '🚻';
      if (cat === 'park') return '🌳';
      if (cat === 'culture') return '🏛️';
      return '📍';
    }

    function renderPlaces(items, activeId) {
      // Clear existing markers
      Object.keys(markersMap).forEach(function(k) {
        map.removeLayer(markersMap[k]);
      });
      markersMap = {};

      items.forEach(function(place) {
        var isSelected = place.id === activeId;
        var iconHtml = '<div class="poi-marker-container ' + (isSelected ? 'selected' : '') + '">' +
          '<div class="poi-icon-bubble">' + getIconGlyph(place.category) + '</div>' +
          '<div class="poi-badge">' + place.name + '</div>' +
        '</div>';

        var divIcon = L.divIcon({
          className: '',
          html: iconHtml,
          iconSize: [120, 50],
          iconAnchor: [60, 50]
        });

        var marker = L.marker([place.coordinates.latitude, place.coordinates.longitude], {
          icon: divIcon,
          zIndexOffset: isSelected ? 1000 : 100
        }).addTo(map);

        marker.on('click', function() {
          selectPlace(place.id, true);
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
        className: '',
        html: '<div class="user-location-marker">' +
          '<div class="user-location-pulse"></div>' +
          '<div class="user-location-dot"></div>' +
        '</div>',
        iconSize: [22, 22],
        iconAnchor: [11, 11]
      });

      userMarker = L.marker([coords.latitude, coords.longitude], {
        icon: userIcon,
        zIndexOffset: 2000
      }).addTo(map);

      if (shouldPan) {
        map.flyTo([coords.latitude, coords.longitude], 16, { animate: true, duration: 1.0 });
      }
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
        if (window.ReactNativeWebView && window.ReactNativeWebView.postMessage) {
          window.ReactNativeWebView.postMessage(msg);
        } else if (window.parent) {
          window.parent.postMessage(msg, '*');
        }
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

    // Bridge listeners
    window.addEventListener('message', function(event) {
      try {
        var data = typeof event.data === 'string' ? JSON.parse(event.data) : event.data;
        if (!data || !data.type) return;

        if (data.type === 'SET_SELECTED_PLACE') {
          selectPlace(data.placeId, false);
        } else if (data.type === 'SET_USER_LOCATION') {
          userLocation = data.coords;
          updateUserMarker(data.coords, data.panToUser === true);
        } else if (data.type === 'SET_HIGH_CONTRAST') {
          if (data.enabled) {
            document.body.classList.add('high-contrast');
          } else {
            document.body.classList.remove('high-contrast');
          }
        } else if (data.type === 'UPDATE_PLACES') {
          places = data.places;
          renderPlaces(places, selectedId);
        }
      } catch (err) {}
    });

    document.addEventListener('message', function(event) {
      try {
        var data = JSON.parse(event.data);
        if (!data || !data.type) return;

        if (data.type === 'SET_SELECTED_PLACE') {
          selectPlace(data.placeId, false);
        } else if (data.type === 'SET_USER_LOCATION') {
          userLocation = data.coords;
          updateUserMarker(data.coords, data.panToUser === true);
        } else if (data.type === 'SET_HIGH_CONTRAST') {
          if (data.enabled) {
            document.body.classList.add('high-contrast');
          } else {
            document.body.classList.remove('high-contrast');
          }
        } else if (data.type === 'UPDATE_PLACES') {
          places = data.places;
          renderPlaces(places, selectedId);
        }
      } catch (err) {}
    });
  </script>
</body>
</html>`;
}
