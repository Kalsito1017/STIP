import { useMemo, useRef, useEffect, useState, useCallback } from 'react';
import { MapContainer, useMap, ZoomControl } from 'react-leaflet';
import * as L from 'leaflet';
import { Search } from 'lucide-react';
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
import { TransitFilterChips } from '../components/map/TransitFilterChips';
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
  const { data: stops, isLoading: stopsLoading, isError: stopsError, error: stopsErr, refetch: refetchStops } = useStops();
  const { data: liveVehicles, isLoading: vehiclesLoading } = useLiveVehicles();
  const [clusterMode, setClusterMode] = useState(false);
  const [showRoutes, setShowRoutes] = useState(true);
  const [showStops, setShowStops] = useState(true);
  const [showHeatmap, setShowHeatmap] = useState(true);
  const [showVehicles, setShowVehicles] = useState(true);
  const [showCongestion, setShowCongestion] = useState(false);
  const [showNearby, setShowNearby] = useState(false);
  const [activeTransitTypes, setActiveTransitTypes] = useState<Set<number>>(new Set([0, 1, 3, 11]));
  const [userLocation, setUserLocation] = useState<{ lat: number; lon: number } | null>(null);
  const mapRef = useRef<L.Map | null>(null);
  const { data: shapes, isLoading: shapesLoading, isError: shapesError, error: routesShapeErr, refetch: refetchShapes } = useAllRouteShapes(showRoutes);
  const { data: heatmap, isLoading: heatmapLoading } = useDelayHeatmap(showHeatmap);
  const { data: congestion, isLoading: congestionLoading } = useStopCongestionAll(showCongestion);

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

  const handleToggleTransitType = useCallback((type: number) => {
    setActiveTransitTypes((prev) => {
      const next = new Set(prev);
      if (next.has(type)) {
        if (next.size > 1) next.delete(type);
      } else {
        next.add(type);
      }
      return next;
    });
  }, []);

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
    let filtered = routeFilter ? source.filter((v: { routeId: string | null }) => v.routeId === routeFilter) : source;
    if (activeTransitTypes.size < 4) {
      filtered = filtered.filter((v: { routeId: string | null }) => {
        if (!v.routeId) return activeTransitTypes.has(3);
        if (v.routeId.includes('-tram-')) return activeTransitTypes.has(0);
        if (v.routeId.startsWith('r-m')) return activeTransitTypes.has(1);
        if (v.routeId.includes('-trol-')) return activeTransitTypes.has(11);
        return activeTransitTypes.has(3);
      });
    }
    return filtered;
  }, [vehicles, liveVehicles, routeFilter, connectionState, activeTransitTypes]);

  const stopGeojson = useMemo(() => stopsToGeoJSON(stops), [stops]);

  const routeLines = useMemo(() => {
    const all = shapes ?? { type: 'FeatureCollection' as const, features: [] };
    if (activeTransitTypes.size >= 4) return all;
    return {
      ...all,
      features: all.features.filter((f) => {
        const t = f.properties?.routeType?.toLowerCase();
        if (t === 'tram') return activeTransitTypes.has(0);
        if (t === 'metro') return activeTransitTypes.has(1);
        if (t === 'trolley') return activeTransitTypes.has(11);
        return activeTransitTypes.has(3);
      }),
    };
  }, [shapes, activeTransitTypes]);

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
          <ErrorAlert message={routesShapeErr?.message ?? 'Failed to load route shapes'} onRetry={() => refetchShapes()} />
        </div>
      )}
      {stopsError && (
        <div className="absolute top-40 left-4 right-4 z-[1000] max-w-md">
          <ErrorAlert message={stopsErr?.message ?? 'Failed to load stops'} onRetry={() => refetchStops()} />
        </div>
      )}

      <MapContainer
        ref={mapRef}
        center={SOFIA_CENTER}
        zoom={12}
        style={{ width: '100%', height: '100%' }}
        zoomControl={false}
        preferCanvas
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

      <div className="absolute top-14 right-3 sm:right-4 z-[999] pointer-events-auto">
        <div className="bg-card/80 backdrop-blur-md rounded-full shadow-md border border-border/60 px-2 py-1.5">
          <TransitFilterChips activeTypes={activeTransitTypes} onToggle={handleToggleTransitType} />
        </div>
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

      {!mapLoading && displayVehicles.length === 0 && (
        <div className="absolute inset-0 z-[998] pointer-events-none flex items-center justify-center">
          <div className="bg-card/80 backdrop-blur-md border border-border/60 rounded-2xl shadow-lg px-6 py-4 text-center max-w-xs">
            <Search className="w-5 h-5 text-muted-foreground mx-auto mb-2" />
            <p className="text-sm font-medium text-foreground">
              {routeFilter ? 'No vehicles on this route' : 'No vehicles in view'}
            </p>
            <p className="text-xs text-muted-foreground mt-1">
              {routeFilter ? 'Try selecting a different route' : 'Zoom out or change filters to see vehicles'}
            </p>
          </div>
        </div>
      )}
    </div>
  );
}
