import { useEffect, useMemo, useRef, useState } from 'react';
import Map, { Source, Layer, Marker, Popup, NavigationControl, ScaleControl, useMap } from 'react-map-gl';
import { useAppStore } from '../store/useAppStore';
import { useRealtime } from '../hooks/useRealtime';
import { useRoutes } from '../hooks/useRoutes';
import { useLiveVehicles } from '../hooks/useVehicles';
import { useStops } from '../hooks/useStops';
import { useAllRouteShapes } from '../hooks/useRouteShapes';
import { AlertBanner } from '../components/AlertBanner';
import type { Vehicle } from '../store/useAppStore';
import type { StopFeatureCollection } from '../types/map';

const MAPBOX_TOKEN = import.meta.env.MAPBOX_TOKEN;

const SOFIA_CENTER: [number, number] = [23.3219, 42.6977];
const LIGHT_STYLE = 'mapbox://styles/mapbox/streets-v12';
const DARK_STYLE = 'mapbox://styles/mapbox/dark-v11';

function FitBoundsOnShapes() {
  const { current: map } = useMap();
  const { data: shapes } = useAllRouteShapes();
  const fittedRef = useRef(false);

  useEffect(() => {
    if (!map || !shapes?.features?.length || fittedRef.current) return;

    const allCoords = shapes.features.flatMap((f) => f.geometry.coordinates);
    if (allCoords.length > 0) {
      map.fitBounds(allCoords as [number, number][], { padding: 50, maxZoom: 14, duration: 0 });
      fittedRef.current = true;
    }
  }, [map, shapes]);

  return null;
}

function stopsToGeoJSON(stops: { stopId: string; stopName: string; lat: number; lon: number }[] | undefined): StopFeatureCollection {
  return {
    type: 'FeatureCollection',
    features: (stops ?? []).map((s) => ({
      type: 'Feature' as const,
      geometry: { type: 'Point' as const, coordinates: [s.lon, s.lat] },
      properties: { stopId: s.stopId, stopName: s.stopName },
    })),
  };
}

export function LiveMapPage() {
  useRealtime();
  const vehicles = useAppStore((s) => s.vehicles);
  const darkMode = useAppStore((s) => s.darkMode);
  const toggleDarkMode = useAppStore((s) => s.toggleDarkMode);
  const [routeFilter, setRouteFilter] = useState('');
  const [popupInfo, setPopupInfo] = useState<Vehicle | null>(null);
  const { data: routes } = useRoutes();
  const { data: stops } = useStops();
  const { data: liveVehicles } = useLiveVehicles(routeFilter || undefined);
  const { data: shapes } = useAllRouteShapes();

  const displayVehicles = useMemo(() => {
    if (routeFilter && liveVehicles) return liveVehicles;
    if (vehicles.length > 0) return vehicles;
    return liveVehicles ?? [];
  }, [vehicles, liveVehicles, routeFilter]);

  const stopGeojson = useMemo(() => stopsToGeoJSON(stops), [stops]);

  const routeLines = useMemo(() => {
    if (!shapes) return { type: 'FeatureCollection' as const, features: [] };
    return shapes;
  }, [shapes]);

  return (
    <div className="space-y-4">
      <AlertBanner />
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Live Map</h1>
        <div className="flex items-center gap-3">
          <span className="text-sm text-slate-500">
            {displayVehicles.length} vehicles tracking
          </span>
          <select
            value={routeFilter}
            onChange={(e) => setRouteFilter(e.target.value)}
            className="text-sm border border-slate-300 rounded-md px-3 py-1.5 bg-white"
          >
            <option value="">All routes</option>
            {routes?.map((r: { routeId: string; shortName: string }) => (
              <option key={r.routeId} value={r.routeId}>
                {r.shortName}
              </option>
            ))}
          </select>
          <button
            onClick={toggleDarkMode}
            className="text-sm border border-slate-300 rounded-md px-3 py-1.5 bg-white hover:bg-slate-50"
            title="Toggle dark mode"
          >
            {darkMode ? '☀️' : '🌙'}
          </button>
        </div>
      </div>
      <div className="bg-white rounded-lg border border-slate-200 overflow-hidden shadow-sm" style={{ height: 'calc(100vh - 160px)' }}>
        <Map
          mapboxAccessToken={MAPBOX_TOKEN}
          mapStyle={darkMode ? DARK_STYLE : LIGHT_STYLE}
          initialViewState={{ longitude: SOFIA_CENTER[0], latitude: SOFIA_CENTER[1], zoom: 12 }}
          style={{ width: '100%', height: '100%' }}
          reuseMaps
        >
          <NavigationControl position="top-right" />
          <ScaleControl position="bottom-left" />
          <FitBoundsOnShapes />

          <Source id="route-shapes" type="geojson" data={routeLines}>
            <Layer
              id="route-lines"
              type="line"
              paint={{
                'line-color': ['get', 'color'],
                'line-width': 3,
                'line-opacity': 0.7,
              }}
              layout={{ 'line-cap': 'round', 'line-join': 'round' }}
            />
          </Source>

          <Source id="stops" type="geojson" data={stopGeojson}>
            <Layer
              id="stop-circles"
              type="circle"
              paint={{
                'circle-radius': 5,
                'circle-color': '#ef4444',
                'circle-stroke-color': '#ffffff',
                'circle-stroke-width': 2,
              }}
            />
          </Source>

          {displayVehicles.map((v) => (
            <Marker
              key={v.vehicleId}
              longitude={v.lon}
              latitude={v.lat}
              onClick={(e) => {
                e.originalEvent.stopPropagation();
                setPopupInfo(v);
              }}
            >
              <div
                style={{
                  transform: `rotate(${v.bearing}deg)`,
                  background: '#2563eb',
                  color: 'white',
                  borderRadius: '50%',
                  width: '28px',
                  height: '28px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '14px',
                  fontWeight: 'bold',
                  border: '2px solid white',
                  boxShadow: '0 2px 4px rgba(0,0,0,.3)',
                  cursor: 'pointer',
                }}
              >
                🚌
              </div>
            </Marker>
          ))}

          {popupInfo && (
            <Popup
              longitude={popupInfo.lon}
              latitude={popupInfo.lat}
              onClose={() => setPopupInfo(null)}
              closeButton
              closeOnClick={false}
              offset={16}
            >
              <div className="text-sm">
                <p><strong>Route:</strong> {popupInfo.routeId ?? 'N/A'}</p>
                <p><strong>Speed:</strong> {popupInfo.speed} km/h</p>
                <p><strong>Bearing:</strong> {popupInfo.bearing}°</p>
              </div>
            </Popup>
          )}
        </Map>
      </div>
    </div>
  );
}
