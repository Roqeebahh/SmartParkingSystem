const Maps = (() => {
  'use strict';

  const LASU_CENTER = { lat: 6.4670, lng: 3.1830 };
  const LASU_ZOOM   = 16;
  const CAMPUS_ZOOM = 17;

  let map                = null;
  let directionsService  = null;
  let directionsRenderer = null;
  let userMarker         = null;
  let destMarker         = null;
  let userLocation       = null;
  let isInitialised      = false;
  let initRetryTimer     = null;

  function initMap() {
    if (isInitialised) {
      refreshMap();
      return;
    }

    const mapElement = document.getElementById('googleMap');
    if (!mapElement || !window.google?.maps) return;
    if (!isMapReadyToRender(mapElement)) {
      scheduleInitRetry();
      return;
    }

    clearInitRetry();

    map = new google.maps.Map(mapElement, {
      center: LASU_CENTER,
      zoom:   LASU_ZOOM,
      mapTypeId:        'roadmap',
      mapTypeControl:   false,
      streetViewControl: false,
      fullscreenControl: true,
      zoomControlOptions: { position: google.maps.ControlPosition.RIGHT_BOTTOM },
      styles: mapStyles()
    });

    directionsService  = new google.maps.DirectionsService();
    directionsRenderer = new google.maps.DirectionsRenderer({
      map,
      panel: document.getElementById('directionsPanel'),
      suppressMarkers: false,
      polylineOptions: { strokeColor: '#006400', strokeWeight: 5 }
    });

    locateUser();
    isInitialised = true;
    hideOverlay();
  }

  function ensureInitialised() {
    if (!isInitialised && window.google?.maps) {
      initMap();
      return;
    }

    refreshMap();
  }

  function refreshMap() {
    if (!map || !window.google?.maps) return;

    setTimeout(() => {
      google.maps.event.trigger(map, 'resize');
      map.setCenter(userLocation || LASU_CENTER);
    }, 0);
  }

  function isMapReadyToRender(mapElement) {
    const mapView = document.getElementById('mapView');
    return !!(
      mapView?.classList.contains('active') &&
      mapElement.offsetWidth > 0 &&
      mapElement.offsetHeight > 0
    );
  }

  function scheduleInitRetry() {
    if (initRetryTimer) return;
    initRetryTimer = setTimeout(() => {
      initRetryTimer = null;
      initMap();
    }, 50);
  }

  function clearInitRetry() {
    if (!initRetryTimer) return;
    clearTimeout(initRetryTimer);
    initRetryTimer = null;
  }

  // ── User Location ────────────────────────────────────────────────────────
  function locateUser() {
    if (!navigator.geolocation) { fallbackToLASU(); return; }
    navigator.geolocation.getCurrentPosition(
      pos => {
        userLocation = { lat: pos.coords.latitude, lng: pos.coords.longitude };
        placeUserMarker(userLocation);
        map.setCenter(userLocation);
      },
      err => { console.warn('[Maps] Geolocation:', err.message); fallbackToLASU(); },
      { enableHighAccuracy: true, timeout: 8000, maximumAge: 30000 }
    );
  }

  function fallbackToLASU() {
    userLocation = LASU_CENTER;
    placeUserMarker(userLocation);
    if (window.App) App.showToast('Using LASU campus as your location.', 'info', 3000);
  }

  function placeUserMarker(pos) {
    if (userMarker) userMarker.setMap(null);
    userMarker = new google.maps.Marker({
      position:  pos,
      map,
      title:     'Your Location',
      icon: {
        path:         google.maps.SymbolPath.CIRCLE,
        scale:        10,
        fillColor:    '#006400',
        fillOpacity:  1,
        strokeColor:  '#ffffff',
        strokeWeight: 3
      },
      zIndex:    100,
      animation: google.maps.Animation.DROP
    });
  }

  // ── Navigate ─────────────────────────────────────────────────────────────
  function navigateTo(facultyGroup) {
    if (!map) ensureInitialised();
    if (!map) {
      if (window.App) App.showToast('Map is still loading. Please try again.', 'info');
      return;
    }

    fetch(`http://192.168.197.170:5062/api/faculty/${facultyGroup.facultyId}`)
    //fetch(`https://localhost:7240/api/faculty/${facultyGroup.facultyId}`)
      .then(r => r.json())
      .then(f => resolveRoute(f, facultyGroup))
      .catch(() => {
        if (window.App) App.showToast('Could not load faculty location.', 'error');
      });
  }

  function resolveRoute(faculty, group) {
    const lat = Number(faculty.latitude);
    const lng = Number(faculty.longitude);

    if (!Number.isFinite(lat) || !Number.isFinite(lng)) {
      if (window.App) App.showToast('Faculty location coordinates are missing or invalid.', 'error');
      console.error('[Maps] Invalid faculty coordinates:', faculty);
      return;
    }

    const dest = { lat, lng };

    if (destMarker) destMarker.setMap(null);
    destMarker = new google.maps.Marker({
      position:  dest,
      map,
      title:     faculty.name,
      label:     { text: 'P', color: '#ffffff', fontWeight: 'bold', fontSize: '14px' },
      animation: google.maps.Animation.DROP
    });

    const infoHtml = `
      <div style="padding:.4rem;max-width:200px;font-family:sans-serif">
        <strong style="color:#006400">${faculty.name}</strong><br>
        <small>${faculty.description}</small><br>
        ${group.monitoringEnabled
          ? '<span style="color:#16a34a;font-size:.75rem">● Live Parking Available</span>'
          : '<span style="color:#6b7280;font-size:.75rem">Monitoring coming soon</span>'}
      </div>`;
    const infoWindow = new google.maps.InfoWindow({ content: infoHtml });
    destMarker.addListener('click', () => infoWindow.open(map, destMarker));
    infoWindow.open(map, destMarker);

    const origin = userLocation || LASU_CENTER;
    directionsService.route(
      {
        origin,
        destination: dest,
        travelMode:  google.maps.TravelMode.DRIVING
      },
      (result, status) => {
        if (status === 'OK') {
          directionsRenderer.setDirections(result);
          updateRoutePanel(faculty.name, result);
        } else {
          map.panTo(dest);
          map.setZoom(CAMPUS_ZOOM);
          updateRoutePanelError(faculty.name);
          if (window.App) App.showToast(`Directions unavailable (${status})`, 'error');
        }
      }
    );

    updateRoutePanel(faculty.name, null);
  }

  function updateRoutePanel(name, result) {
    const panel = document.getElementById('routePanel');
    panel.style.display = 'block';
    document.getElementById('routeDestTitle').textContent = name;
    if (result) {
      const leg = result.routes[0]?.legs[0];
      document.getElementById('routeDist').textContent = leg?.distance?.text ?? '–';
      document.getElementById('routeETA').textContent  = leg?.duration?.text ?? '–';
    } else {
      document.getElementById('routeDist').textContent = '…';
      document.getElementById('routeETA').textContent  = '…';
    }
  }

  function updateRoutePanelError(name) {
    document.getElementById('routePanel').style.display = 'block';
    document.getElementById('routeDestTitle').textContent = name;
    document.getElementById('routeDist').textContent = 'N/A';
    document.getElementById('routeETA').textContent  = 'N/A';
  }

  function hideOverlay() {
    const overlay = document.getElementById('mapOverlay');
    if (overlay) overlay.classList.add('hidden');
  }

  function showOverlayError(message) {
    const overlay = document.getElementById('mapOverlay');
    if (!overlay) return;

    overlay.classList.remove('hidden');
    overlay.innerHTML = `<p style="color:var(--color-occupied);font-weight:600;text-align:center;padding:1rem">${message}</p>`;
  }

  function mapStyles() {
    return [
      { featureType: 'poi.school', elementType: 'geometry', stylers: [{ color: '#d4edda' }] },
      { featureType: 'poi.park',   elementType: 'geometry', stylers: [{ color: '#c8e6c9' }] },
      { featureType: 'road.highway', elementType: 'geometry', stylers: [{ color: '#e0e0e0' }] },
      { featureType: 'water', elementType: 'geometry', stylers: [{ color: '#b3e5fc' }] }
    ];
  }

  return { initMap, ensureInitialised, navigateTo, refreshMap, showOverlayError };
})();

// Google Maps JS API callback
window.Maps = Maps;
window.initMap = () => Maps.initMap();
window.gm_authFailure = () => {
  Maps.showOverlayError('Google Maps API key error. Check API key restrictions, billing, and Maps JavaScript API access.');
  console.error('[Maps] Google Maps authentication failed.');
};
