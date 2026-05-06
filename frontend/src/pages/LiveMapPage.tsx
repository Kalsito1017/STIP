import { useMemo, useRef, useEffect, useState, useCallback } from 'react';
import { MapContainer, useMap, ZoomControl } from 'react-leaflet';
import * as L from 'leaflet';
import { useAppStore } from '../store/useAppStore';
import { useRealtime } from '../hooks/useRealtime';
import { useRoutes } from '../hooks/useRoutes';
import { useLiveVehicles } from '../hooks/useVehicles';
import { useStops } from '../hooks/useStops';
import { useAllRouteShapes } from '../hooks/useRouteShapes';
import { useDelayHeatmap } from '../hooks/useHeatmap';
import { useStopCongestionAll, useNearbyStops } from '../hooks/useAnalytics';
import { AlertBanner } from '../components/AlertBanner';
import { ErrorAlert } from '../components/ErrorAlert';
import { MapLoadingOverlay } from '../components/MapLoadingOverlay';
import { MapLibreBasemap } from '../components/map/MapLibreBasemap';
import { RouteShapeLayer } from '../components/map/RouteShapeLayer';
import { StopLayer } from '../components/map/StopLayer';
import { VehicleLayer } from '../components/map/VehicleLayer';
import { VehicleClusterLayer } from '../components/map/VehicleClusterLayer';
import { DelayHeatmapLayer } from '../components/map/DelayHeatmapLayer';
import { StopCongestionLayer } from '../components/map/StopCongestionLayer';
import { NearbyStopsLayer } from '../components/map/NearbyStopsLayer';
import { VehicleDetailSheet } from '../components/map/VehicleDetailSheet';
import { MapControls } from '../components/map/MapControls';
import { VehicleStatsBar } from '../components/map/VehicleStatsBar';
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

function FlyToTarget() {
  const map = useMap();
  const flyToTarget = useAppStore((s) => s.flyToTarget);
  const setFlyToTarget = useAppStore((s) => s.setFlyToTarget);

  useEffect(() => {
    if (!flyToTarget) return;
    map.flyTo([flyToTarget.lat, flyToTarget.lon], flyToTarget.zoom, { duration: 1 });
    setFlyToTarget(null);
  }, [flyToTarget, map, setFlyToTarget]);

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
  const connectionState = useAppStore((s) => s.connectionState);
  const darkMode = useAppStore((s) => s.darkMode);
  const routeFilter = useAppStore((s) => s.routeFilter);
  const { data: routes, isLoading: routesLoading, isError: routesError, error: routesErr, refetch: refetchRoutes } = useRoutes();
  const { data: stops, isLoading: stopsLoading } = useStops();
  const { data: liveVehicles, isLoading: vehiclesLoading } = useLiveVehicles();
  const { data: shapes, isLoading: shapesLoading, isError: shapesError, error: shapesErr, refetch: refetchShapes } = useAllRouteShapes();
  const { data: heatmap, isLoading: heatmapLoading } = useDelayHeatmap();
  const { data: congestion, isLoading: congestionLoading } = useStopCongestionAll();
  const [clusterMode, setClusterMode] = useState(false);
  const [showRoutes, setShowRoutes] = useState(true);
  const [showStops, setShowStops] = useState(true);
  const [showHeatmap, setShowHeatmap] = useState(true);
  const [showVehicles, setShowVehicles] = useState(true);
  const [showCongestion, setShowCongestion] = useState(false);
  const [showNearby, setShowNearby] = useState(false);
  const [userLocation, setUserLocation] = useState<{ lat: number; lon: number } | null>(null);
  const mapRef = useRef<L.Map | null>(null);

  const handleLocate = useCallback(() => {
    mapRef.current?.locate({ setView: true, maxZoom: 16, enableHighAccuracy: true });
  }, []);

  const handleToggleNearby = useCallback(() => {
    if (!showNearby && !userLocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => setUserLocation({ lat: pos.coords.latitude, lon: pos.coords.longitude }),
        () => {},
        { enableHighAccuracy: true, timeout: 5000 }
      );
    }
    setShowNearby((v) => !v);
  }, [showNearby, userLocation]);

  const { data: nearbyStops } = useNearbyStops(
    userLocation?.lat ?? null,
    userLocation?.lon ?? null,
    0.5
  );

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

  const mapLoading = routesLoading || stopsLoading || vehiclesLoading || shapesLoading || heatmapLoading || congestionLoading;
  const loadingLayers = [
    { label: 'Routes', loaded: !routesLoading },
    { label: 'Stops', loaded: !stopsLoading },
    { label: 'Vehicles', loaded: !vehiclesLoading },
    { label: 'Route shapes', loaded: !shapesLoading },
    { label: 'Delay heatmap', loaded: !heatmapLoading },
    { label: 'Stop congestion', loaded: !congestionLoading },
  ];

  return (
    <div className="h-screen w-screen relative">
      <MapLoadingOverlay visible={mapLoading} layers={loadingLayers} />

      {routesError && (
        <div className="absolute top-16 left-4 right-4 z-[1000] max-w-md">
          <ErrorAlert message={routesErr.message} onRetry={() => refetchRoutes()} />
        </div>
      )}
      {shapesError && (
        <div className="absolute top-28 left-4 right-4 z-[1000] max-w-md">
          <ErrorAlert message={shapesErr?.message ?? 'Failed to load route shapes'} onRetry={() => refetchShapes()} />
        </div>
      )}

      <MapContainer
        ref={mapRef}
        center={SOFIA_CENTER}
        zoom={12}
        style={{ width: '100%', height: '100%' }}
        zoomControl={false}
      >
        <ZoomControl position="topright" />
        <FitBoundsOnShapes />
        <FlyToTarget />

        <MapLibreBasemap
          styleUrl={darkMode ? '/map-styles/dark.json' : '/map-styles/light.json'}
        />

        {showRoutes && <RouteShapeLayer data={routeLines} />}
        {showStops && <StopLayer data={stopGeojson} />}
        {showHeatmap && heatmap && heatmap.length > 0 && <DelayHeatmapLayer points={heatmap} />}
        {showCongestion && congestion && congestion.length > 0 && <StopCongestionLayer points={congestion} />}
        {showNearby && nearbyStops && nearbyStops.length > 0 && <NearbyStopsLayer stops={nearbyStops} />}
        {showVehicles && (clusterMode ? (
          <VehicleClusterLayer vehicles={displayVehicles} routeNames={routeNames} />
        ) : (
          <VehicleLayer vehicles={displayVehicles} routeNames={routeNames} />
        ))}
      </MapContainer>

      <div className="absolute top-14 left-3 right-3 sm:left-4 sm:right-4 z-[999] max-w-md">
        <AlertBanner />
      </div>

      <VehicleStatsBar vehicles={displayVehicles} />

      <MapControls
        clusterMode={clusterMode}
        onToggleCluster={() => setClusterMode((v) => !v)}
        showRoutes={showRoutes}
        showStops={showStops}
        showHeatmap={showHeatmap}
        showVehicles={showVehicles}
        showCongestion={showCongestion}
        showNearby={showNearby}
        onToggleRoutes={() => setShowRoutes((v) => !v)}
        onToggleStops={() => setShowStops((v) => !v)}
        onToggleHeatmap={() => setShowHeatmap((v) => !v)}
        onToggleVehicles={() => setShowVehicles((v) => !v)}
        onToggleCongestion={() => setShowCongestion((v) => !v)}
        onToggleNearby={handleToggleNearby}
        onLocate={handleLocate}
      />

      <VehicleDetailSheet routeNames={routeNames} />
    </div>
  );
}
