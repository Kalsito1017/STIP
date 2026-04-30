import { useMemo, useRef, useEffect, useState } from 'react';
import { MapContainer, TileLayer, useMap, ZoomControl } from 'react-leaflet';
import * as L from 'leaflet';
import { useAppStore } from '../store/useAppStore';
import { useRealtime } from '../hooks/useRealtime';
import { useRoutes } from '../hooks/useRoutes';
import { useLiveVehicles } from '../hooks/useVehicles';
import { useStops } from '../hooks/useStops';
import { useAllRouteShapes } from '../hooks/useRouteShapes';
import { useDelayHeatmap } from '../hooks/useHeatmap';
import { AlertBanner } from '../components/AlertBanner';
import { ErrorAlert } from '../components/ErrorAlert';
import { RouteShapeLayer } from '../components/map/RouteShapeLayer';
import { StopLayer } from '../components/map/StopLayer';
import { VehicleLayer } from '../components/map/VehicleLayer';
import { DelayHeatmapLayer } from '../components/map/DelayHeatmapLayer';
import { FilterPanel } from '../components/map/FilterPanel';
import type { StopFeatureCollection } from '../types/map';

const SOFIA_CENTER: L.LatLngTuple = [42.6977, 23.3219];

function FitBoundsOnShapes() {
  const map = useMap();
  const { data: shapes } = useAllRouteShapes();
  const fittedRef = useRef(false);

  useEffect(() => {
    if (!shapes?.features?.length || fittedRef.current) return;

    const allCoords = shapes.features.flatMap((f) => f.geometry.coordinates);
    if (allCoords.length > 0) {
      const bounds = L.latLngBounds(
        allCoords.map((c: number[]) => L.latLng(c[1], c[0]))
      );
      map.fitBounds(bounds, { padding: [50, 50], maxZoom: 14 });
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

function formatTimeAgo(isoString: string | null): string {
  if (!isoString) return '';
  const diff = Date.now() - new Date(isoString).getTime();
  const seconds = Math.floor(diff / 1000);
  if (seconds < 5) return 'Just now';
  if (seconds < 60) return `${seconds}s ago`;
  const minutes = Math.floor(seconds / 60);
  return `${minutes}m ago`;
}

export function LiveMapPage() {
  useRealtime();
  const vehicles = useAppStore((s) => s.vehicles);
  const connectionState = useAppStore((s) => s.connectionState);
  const lastUpdatedAt = useAppStore((s) => s.lastUpdatedAt);
  const darkMode = useAppStore((s) => s.darkMode);
  const toggleDarkMode = useAppStore((s) => s.toggleDarkMode);
  const [routeFilter, setRouteFilter] = useState('');
  const { data: routes, isError: routesError, error: routesErr, refetch: refetchRoutes } = useRoutes();
  const { data: stops } = useStops();
  const { data: liveVehicles } = useLiveVehicles();
  const { data: shapes } = useAllRouteShapes();
  const { data: heatmap } = useDelayHeatmap();

  const routeNames = useMemo(() => {
    const map: Record<string, string> = {};
    routes?.forEach((r: { routeId: string; shortName: string }) => {
      map[r.routeId] = r.shortName;
    });
    return map;
  }, [routes]);

  const displayVehicles = useMemo(() => {
    const source = connectionState === 'connected' && vehicles.length > 0 ? vehicles : (liveVehicles ?? []);
    return routeFilter ? source.filter((v: { routeId: string | null }) => v.routeId === routeFilter) : source;
  }, [vehicles, liveVehicles, routeFilter, connectionState]);

  const stopGeojson = useMemo(() => stopsToGeoJSON(stops), [stops]);

  const routeLines = shapes ?? { type: 'FeatureCollection' as const, features: [] };

  return (
    <div className="space-y-3 sm:space-y-4">
      <AlertBanner />
      {routesError && (
        <ErrorAlert message={routesErr.message} onRetry={() => refetchRoutes()} />
      )}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div className="flex items-center gap-3">
          <h1 className="text-xl sm:text-2xl font-bold text-slate-900">Live Map</h1>
          {lastUpdatedAt && (
            <span className="text-xs text-slate-400">
              Updated {formatTimeAgo(lastUpdatedAt)}
            </span>
          )}
        </div>
        <FilterPanel
          routes={routes}
          routeFilter={routeFilter}
          onRouteFilterChange={setRouteFilter}
          darkMode={darkMode}
          onToggleDarkMode={toggleDarkMode}
          vehicleCount={displayVehicles.length}
        />
      </div>
      <div className="bg-white rounded-lg border border-slate-200 overflow-hidden shadow-sm" style={{ height: 'calc(100vh - 200px)' }}>
        <MapContainer
          center={SOFIA_CENTER}
          zoom={12}
          style={{ width: '100%', height: '100%' }}
          zoomControl={false}
        >
          <ZoomControl position="topright" />
          <FitBoundsOnShapes />

          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            opacity={darkMode ? 0 : 1}
          />
          {darkMode && (
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a>, &copy; <a href="https://carto.com/">CARTO</a>'
              url="https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png"
            />
          )}

          <RouteShapeLayer data={routeLines} />
          <StopLayer data={stopGeojson} />
          {heatmap && heatmap.length > 0 && <DelayHeatmapLayer points={heatmap} />}
          <VehicleLayer vehicles={displayVehicles} routeNames={routeNames} />
        </MapContainer>
      </div>
    </div>
  );
}
