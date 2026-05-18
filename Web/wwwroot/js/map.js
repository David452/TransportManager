window.mapInstances = {};

window.initMap = (elementId, interactive = true) => {
    const map = L.map(elementId, {
        zoomControl: interactive,
        dragging: interactive,
        scrollWheelZoom: interactive,
        doubleClickZoom: interactive,
        touchZoom: interactive,
        keyboard: interactive,
    }).setView([48.7, 19.5], 7);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);
    window.mapInstances[elementId] = map;

}

window.showRoute = (elementId, polylinePoints, originLabel, destinationLabel) => {
    const map = window.mapInstances[elementId];
    if (!map) return;

    map.eachLayer(layer => {
        if (layer instanceof L.Polyline || layer instanceof L.Marker) {
            map.removeLayer(layer);
        }
    });

    if (!polylinePoints || polylinePoints.length === 0) return;

    const line = L.polyline(polylinePoints, { color: '#3b82f6', weight: 4 }).addTo(map);

    L.marker(polylinePoints[0]).bindPopup(originLabel).addTo(map);
    L.marker(polylinePoints[polylinePoints.length - 1]).bindPopup(destinationLabel).addTo(map);

    map.fitBounds(line.getBounds(), { padding: [32, 32] });
};
