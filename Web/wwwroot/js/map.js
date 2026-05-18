window.mapInstances = {};
window.tileLayers = {};

const LIGHT_TILES = 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png';
const DARK_TILES = 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png';

function currentTileUrl() {
    return document.documentElement.getAttribute('data-theme') === 'dark' ? DARK_TILES : LIGHT_TILES;
}

window.initMap = (elementId, interactive = true) => {
    const map = L.map(elementId, {
        zoomControl: interactive,
        dragging: interactive,
        scrollWheelZoom: interactive,
        doubleClickZoom: interactive,
        touchZoom: interactive,
        keyboard: interactive,
    }).setView([48.7, 19.5], 7);

    const tiles = L.tileLayer(currentTileUrl(), {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/attributions">CARTO</a>'
    }).addTo(map);

    window.mapInstances[elementId] = map;
    window.tileLayers[elementId] = tiles;
};

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

window.disposeMap = (elementId) => {
    const map = window.mapInstances[elementId];
    if (map) {
        map.remove();
        delete window.mapInstances[elementId];
    }
    delete window.tileLayers[elementId];
};

new MutationObserver(() => {
    const url = currentTileUrl();
    for (const id in window.tileLayers) {
        window.tileLayers[id].setUrl(url);
    }
}).observe(document.documentElement, { attributes: true, attributeFilter: ['data-theme'] });
