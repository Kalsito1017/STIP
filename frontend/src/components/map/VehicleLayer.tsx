import { useEffect, useRef } from 'react';
import { useMap } from 'react-leaflet';
import * as L from 'leaflet';
import type { Vehicle } from '../../store/useAppStore';
import { TransitTypeRouteColor } from '../../constants/transit';

// --- Trail: last N positions per vehicle ---
const trailStore = new Map<string, { positions: L.LatLng[]; routeType?: number }>();
const TRAIL_LENGTH = 8;

// --- Direction names from bearing ---
function bearingToCardinal(b: number): string {
  const dirs = ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW'];
  return dirs[Math.round(b / 45) % 8];
}

// --- Route type from vehicle ---
function getVehicleRouteType(v: Vehicle): number | undefined {
  // RouteId format: "r-204" → Bus, "r-tram-1" → Tram, "r-m1" → Metro, "r-trol-1" → Trolley
  if (!v.routeId) return undefined;
  if (v.routeId.includes('-tram-')) return 0;
  if (v.routeId.startsWith('r-m')) return 1;
  if (v.routeId.includes('-trol-')) return 11;
  return 3; // default Bus
}

interface Props {
  vehicles: Vehicle[];
  routeNames: Record<string, string>;
}

export function VehicleLayer({ vehicles, routeNames }: Props) {
  const map = useMap();
  const markerRefs = useRef<Map<string, L.Marker>>(new Map());
  const trailGroupRef = useRef<L.LayerGroup | null>(null);

  // Init trail group
  useEffect(() => {
    if (!trailGroupRef.current) {
      trailGroupRef.current = L.layerGroup().addTo(map);
    }
    return () => {
      trailGroupRef.current?.remove();
      trailGroupRef.current = null;
    };
  }, [map]);

  // Sync markers
  useEffect(() => {
    const currentMarkers = new Set(vehicles.map((v) => v.vehicleId));
    const existing = markerRefs.current;

    // Remove stale markers
    for (const [id, marker] of existing) {
      if (!currentMarkers.has(id)) {
        marker.remove();
        existing.delete(id);
        trailStore.delete(id);
      }
    }

    // Add / update markers
    for (const v of vehicles) {
      const routeType = getVehicleRouteType(v);
      const color = routeType != null ? (TransitTypeRouteColor[routeType] ?? '#2563eb') : '#2563eb';
      const displayRoute = v.routeId ? (routeNames[v.routeId] ?? v.routeId) : 'N/A';

      // Build popup HTML
      const popupHtml = `
        <div style="font-size:13px;min-width:180px;line-height:1.5">
          <div style="display:flex;align-items:center;gap:6px;margin-bottom:6px">
            <span style="display:inline-block;width:10px;height:10px;border-radius:50%;background:${color}"></span>
            <strong>${displayRoute}</strong>
          </div>
          <div><span style="color:#64748b">Trip:</span> ${v.tripId ?? '\u2014'}</div>
          <div><span style="color:#64748b">Speed:</span> ${v.speed.toFixed(1)} km/h</div>
          <div><span style="color:#64748b">Heading:</span> ${v.bearing}\u00B0 ${bearingToCardinal(v.bearing)}</div>
          <div><span style="color:#64748b">Updated:</span> ${new Date(v.recordedAt).toLocaleTimeString()}</div>
        </div>
      `;

      // Trail
      const pos = L.latLng(v.lat, v.lon);
      let trailData = trailStore.get(v.vehicleId);
      if (!trailData) {
        trailData = { positions: [], routeType };
        trailStore.set(v.vehicleId, trailData);
      }
      trailData.positions.push(pos);
      if (trailData.positions.length > TRAIL_LENGTH) {
        trailData.positions = trailData.positions.slice(-TRAIL_LENGTH);
      }

      const existingMarker = existing.get(v.vehicleId);
      if (existingMarker) {
        // Smooth move
        const currentPos = existingMarker.getLatLng();
        if (currentPos.distanceTo(pos) > 0.5) {
          existingMarker.setLatLng(pos);
        }
      } else {
        // Create new marker
        const icon = L.divIcon({
          className: 'vehicle-marker',
          html: `
            <div style="
              background:${color};
              color:white;
              border-radius:50%;
              width:30px;
              height:30px;
              display:flex;
              align-items:center;
              justify-content:center;
              font-size:15px;
              border:2px solid white;
              box-shadow:0 2px 6px rgba(0,0,0,0.35);
              cursor:pointer;
              transform:rotate(${v.bearing}deg);
              transition:transform 0.4s ease;
            ">\u{1F68C}</div>
          `,
          iconSize: [30, 30],
          iconAnchor: [15, 15],
        });

        const marker = L.marker(pos, { icon }).addTo(map);
        marker.bindPopup(popupHtml);
        existing.set(v.vehicleId, marker);
      }
    }

    // Update trail polylines
    if (trailGroupRef.current) {
      trailGroupRef.current.clearLayers();
      for (const [id, data] of trailStore) {
        if (!currentMarkers.has(id)) continue;
        if (data.positions.length < 2) continue;
        const polyline = L.polyline(data.positions, {
          color: data.routeType != null ? (TransitTypeRouteColor[data.routeType] ?? '#2563eb') : '#2563eb',
          weight: 2,
          opacity: 0.35,
          dashArray: '4 4',
        });
        trailGroupRef.current.addLayer(polyline);
      }
    }

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [vehicles, routeNames]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      for (const [, marker] of markerRefs.current) {
        marker.remove();
      }
      markerRefs.current.clear();
    };
  }, []);

  return null;
}
