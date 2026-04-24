(function (window) {
    'use strict';

    function toNumber(value) {
        if (value === null || value === undefined) return null;
        const text = String(value).trim();
        if (!text) return null;
        const parsed = Number(text.replace(',', '.'));
        return Number.isFinite(parsed) ? parsed : null;
    }

    function formatCoord(value) {
        return Number.isFinite(value) ? value.toFixed(6) : '--';
    }

    function resolveAddress(lat, lng) {
        const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${encodeURIComponent(lat)}&lon=${encodeURIComponent(lng)}&zoom=18&addressdetails=1`;

        return fetch(url, {
            headers: {
                'Accept': 'application/json'
            }
        }).then(function (response) {
            if (!response.ok) {
                throw new Error('No se pudo resolver dirección.');
            }
            return response.json();
        }).then(function (data) {
            const displayName = data && data.display_name ? String(data.display_name).trim() : '';
            return displayName;
        });
    }

    function createPointPicker(options) {
        const mapElement = document.getElementById(options.mapId);
        const latInput = document.getElementById(options.latInputId);
        const lngInput = document.getElementById(options.lngInputId);

        if (!mapElement || !latInput || !lngInput || typeof L === 'undefined') {
            return null;
        }

        const infoElement = options.coordinatesInfoId
            ? document.getElementById(options.coordinatesInfoId)
            : null;

        const addressInput = options.addressInputId
            ? document.getElementById(options.addressInputId)
            : null;

        const addressInfoElement = options.addressInfoId
            ? document.getElementById(options.addressInfoId)
            : null;

        const defaultCenter = options.defaultCenter || [-12.046374, -77.042793];
        const defaultZoom = options.defaultZoom || 13;
        const selectedZoom = options.selectedZoom || 16;

        const currentLat = toNumber(latInput.value);
        const currentLng = toNumber(lngInput.value);
        const hasInitial = currentLat !== null && currentLng !== null;

        const map = L.map(mapElement).setView(
            hasInitial ? [currentLat, currentLng] : defaultCenter,
            hasInitial ? selectedZoom : defaultZoom
        );

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        let marker = null;
        let requestCounter = 0;

        function updateInfo(lat, lng) {
            if (!infoElement) return;
            infoElement.textContent = `Latitud: ${formatCoord(lat)} | Longitud: ${formatCoord(lng)}`;
        }

        function updateAddressInfo(message, type) {
            if (!addressInfoElement) return;
            addressInfoElement.textContent = message;
            addressInfoElement.classList.remove('text-muted', 'text-danger', 'text-success');
            addressInfoElement.classList.add(type || 'text-muted');
        }

        function applyFallbackAddress(lat, lng) {
            if (!addressInput) return;
            const fallback = `Referencia aproximada: ${formatCoord(lat)}, ${formatCoord(lng)}`;
            addressInput.value = fallback;
            updateAddressInfo('No se pudo resolver dirección exacta. Se guardó referencia por coordenadas.', 'text-danger');
        }

        function resolveAndSetAddress(lat, lng) {
            if (!addressInput) return;

            requestCounter += 1;
            const currentRequest = requestCounter;
            updateAddressInfo('Resolviendo dirección desde el mapa...', 'text-muted');

            resolveAddress(lat, lng)
                .then(function (address) {
                    if (currentRequest !== requestCounter) return;

                    if (address) {
                        addressInput.value = address;
                        updateAddressInfo('Dirección autocompletada desde coordenadas.', 'text-success');
                        return;
                    }

                    applyFallbackAddress(lat, lng);
                })
                .catch(function () {
                    if (currentRequest !== requestCounter) return;
                    applyFallbackAddress(lat, lng);
                });
        }

        function setLocation(lat, lng, shouldCenter, resolveDireccion) {
            latInput.value = Number(lat).toFixed(6);
            lngInput.value = Number(lng).toFixed(6);

            if (!marker) {
                marker = L.marker([lat, lng], { draggable: true }).addTo(map);
                marker.on('dragend', function (e) {
                    const pos = e.target.getLatLng();
                    setLocation(pos.lat, pos.lng, false, true);
                });
            } else {
                marker.setLatLng([lat, lng]);
            }

            if (shouldCenter) {
                map.setView([lat, lng], selectedZoom);
            }

            updateInfo(lat, lng);

            if (resolveDireccion) {
                resolveAndSetAddress(lat, lng);
            }
        }

        if (hasInitial) {
            setLocation(currentLat, currentLng, false, false);
            if (addressInput && !String(addressInput.value || '').trim()) {
                resolveAndSetAddress(currentLat, currentLng);
            }
        } else {
            updateInfo(NaN, NaN);
        }

        map.on('click', function (e) {
            setLocation(e.latlng.lat, e.latlng.lng, true, true);
        });

        latInput.addEventListener('change', function () {
            const lat = toNumber(latInput.value);
            const lng = toNumber(lngInput.value);
            if (lat === null || lng === null) return;
            setLocation(lat, lng, false, true);
        });

        lngInput.addEventListener('change', function () {
            const lat = toNumber(latInput.value);
            const lng = toNumber(lngInput.value);
            if (lat === null || lng === null) return;
            setLocation(lat, lng, false, true);
        });

        if (options.geolocateButtonId) {
            const geolocateButton = document.getElementById(options.geolocateButtonId);
            if (geolocateButton) {
                geolocateButton.addEventListener('click', function () {
                    if (!navigator.geolocation) {
                        alert('Tu navegador no soporta geolocalización.');
                        return;
                    }

                    navigator.geolocation.getCurrentPosition(
                        function (position) {
                            setLocation(position.coords.latitude, position.coords.longitude, true, true);
                        },
                        function (error) {
                            alert(`No se pudo obtener tu ubicación actual (${error.message}).`);
                        },
                        {
                            enableHighAccuracy: true,
                            timeout: 10000,
                            maximumAge: 0
                        }
                    );
                });
            }
        }

        setTimeout(function () {
            map.invalidateSize();
        }, 150);

        return { map: map };
    }

    function bindImagePreview(fileInputId, imagePreviewId) {
        const fileInput = document.getElementById(fileInputId);
        const imagePreview = document.getElementById(imagePreviewId);

        if (!fileInput || !imagePreview) {
            return;
        }

        fileInput.addEventListener('change', function () {
            const file = fileInput.files && fileInput.files.length > 0
                ? fileInput.files[0]
                : null;

            if (!file) {
                return;
            }

            const reader = new FileReader();
            reader.onload = function (event) {
                imagePreview.src = event.target && event.target.result
                    ? event.target.result
                    : imagePreview.src;
            };

            reader.readAsDataURL(file);
        });
    }

    function createRouteViewer(options) {
        const mapElement = document.getElementById(options.mapId);
        if (!mapElement || typeof L === 'undefined') {
            return null;
        }

        const points = Array.isArray(options.paraderos) ? options.paraderos : [];
        const busLocation = options.busLocation || null;
        const defaultCenter = options.defaultCenter || [-12.046374, -77.042793];

        const map = L.map(mapElement).setView(defaultCenter, 13);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        const layers = [];

        if (points.length > 0) {
            const latLngs = [];

            points.forEach(function (point) {
                const lat = toNumber(point.latitud);
                const lng = toNumber(point.longitud);
                if (lat === null || lng === null) return;

                const marker = L.marker([lat, lng]).addTo(map)
                    .bindPopup(`<strong>${point.nombre || 'Paradero'}</strong><br/>${point.direccion || ''}`);

                marker.bindTooltip(`#${point.ordenParada || '-'}`, { permanent: false, direction: 'top' });
                latLngs.push([lat, lng]);
                layers.push(marker);
            });

            if (latLngs.length > 1) {
                const polyline = L.polyline(latLngs, {
                    color: '#0d6efd',
                    weight: 4,
                    opacity: 0.85
                }).addTo(map);
                layers.push(polyline);
            }
        }

        if (busLocation && toNumber(busLocation.latitud) !== null && toNumber(busLocation.longitud) !== null) {
            const lat = toNumber(busLocation.latitud);
            const lng = toNumber(busLocation.longitud);

            const busIcon = L.divIcon({
                className: 'capi-bus-icon',
                html: '<i class="bi bi-bus-front-fill"></i>',
                iconSize: [28, 28],
                iconAnchor: [14, 14]
            });

            const marker = L.marker([lat, lng], { icon: busIcon }).addTo(map)
                .bindPopup(`<strong>Última ubicación del bus</strong><br/>${busLocation.fechaHora || ''}`);
            layers.push(marker);
        }

        if (layers.length > 0) {
            const group = L.featureGroup(layers);
            map.fitBounds(group.getBounds().pad(0.2));
        }

        setTimeout(function () {
            map.invalidateSize();
        }, 150);

        return { map: map };
    }

    window.CapiMovilMaps = {
        createPointPicker: createPointPicker,
        createRouteViewer: createRouteViewer,
        bindImagePreview: bindImagePreview
    };
})(window);
