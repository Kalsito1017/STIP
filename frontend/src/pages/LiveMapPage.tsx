import { useEffect, useMemo, useState } from 'react';
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import L from 'leaflet';
import { useAppStore } from '../store/useAppStore';
import { useRealtime } from '../hooks/useRealtime';
import { useRoutes } from '../hooks/useRoutes';
import { useLiveVehicles } from '../hooks/useVehicles';
import { useStops } from '../hooks/useStops';
import { AlertBanner } from '../components/AlertBanner';
import type { Vehicle } from '../store/useAppStore';

const busIcon = L.divIcon({
  className: 'custom-marker',
  html: '<div style="background:#2563eb;color:white;border-radius:50%;width:24px;height:24px;display:flex;align-items:center;justify-content:center;font-size:10px;font-weight:bold;border:2px solid white;box-shadow:0 1px 3px rgba(0,0,0,.3)">🚌</div>',
  iconSize: [24, 24],
  iconAnchor: [12, 12],
});

const stopIcon = L.divIcon({
  className: 'stop-marker',
  html: '<div style="background:#ef4444;color:white;border-radius:50%;width:12px;height:12px;border:2px solid white;box-shadow:0 1px 2px rgba(0,0,0,.3)"></div>',
  iconSize: [12, 12],
  iconAnchor: [6, 6],
});

function FitBounds() {
  const map = useMap();
  useEffect(() => {
    map.setView([42.6977, 23.3219], 13);
  }, [map]);
  return null;
}

export function LiveMapPage() {
  useRealtime();
  const vehicles = useAppStore((s) => s.vehicles);
  const [routeFilter, setRouteFilter] = useState('');
  const { data: routes } = useRoutes();
  const { data: stops } = useStops();
  const { data: liveVehicles } = useLiveVehicles(routeFilter || undefined);

  const displayVehicles = useMemo(() => {
    if (routeFilter && liveVehicles) return liveVehicles;
    if (vehicles.length > 0) return vehicles;
    return liveVehicles ?? [];
  }, [vehicles, liveVehicles, routeFilter]);

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
        </div>
      </div>
      <div className="bg-white rounded-lg border border-slate-200 overflow-hidden shadow-sm" style={{ height: 'calc(100vh - 160px)' }}>
        <MapContainer
          center={[42.6977, 23.3219]}
          zoom={13}
          style={{ height: '100%', width: '100%' }}
          zoomControl={false}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a>'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <FitBounds />
          {stops?.map((stop: { stopId: string; stopName: string; lat: number; lon: number }) => (
            <Marker key={stop.stopId} position={[stop.lat, stop.lon]} icon={stopIcon}>
              <Popup>{stop.stopName}</Popup>
            </Marker>
          ))}
          {displayVehicles.map((v: Vehicle) => (
            <Marker key={v.vehicleId} position={[v.lat, v.lon]} icon={busIcon}>
              <Popup>
                <div className="text-sm">
                  <p><strong>Route:</strong> {v.routeId ?? 'N/A'}</p>
                  <p><strong>Speed:</strong> {v.speed} km/h</p>
                  <p><strong>Bearing:</strong> {v.bearing}°</p>
                </div>
              </Popup>
            </Marker>
          ))}
        </MapContainer>
      </div>
    </div>
  );
}
