import { useEffect, useRef } from 'react';
import { useMap } from 'react-leaflet';
import * as L from 'leaflet';
import type { Vehicle } from '../../store/useAppStore';
import { useAppStore } from '../../store/useAppStore';
import { TransitTypeRouteColor } from '../../constants/transit';

const trailStore = new Map<string, { positions: L.LatLng[]; routeType?: number }>();
const TRAIL_LENGTH = 8;

function getVehicleRouteType(v: Vehicle): number | undefined {
  if (!v.routeId) return undefined;
  if (v.routeId.includes('-tram-')) return 0;
  if (v.routeId.startsWith('r-m')) return 1;
  if (v.routeId.includes('-trol-')) return 11;
  return 3;
}

interface Props {
  vehicles: Vehicle[];
  routeNames: Record<string, string>;
}

export function VehicleLayer({ vehicles, routeNames }: Props) {
  const map = useMap();
  const markerRefs = useRef<Map<string, L.Marker>>(new Map());
  const trailGroupRef = useRef<L.LayerGroup | null>(null);

  useEffect(() => {
    if (!trailGroupRef.current) {
      trailGroupRef.current = L.layerGroup().addTo(map);
    }
    return () => {
      trailGroupRef.current?.remove();
      trailGroupRef.current = null;
    };
  }, [map]);

  useEffect(() => {
    const currentMarkers = new Set(vehicles.map((v) => v.vehicleId));
    const existing = markerRefs.current;

    for (const [id, marker] of existing) {
      if (!currentMarkers.has(id)) {
        marker.remove();
        existing.delete(id);
        trailStore.delete(id);
      }
    }

    for (const v of vehicles) {
      const routeType = getVehicleRouteType(v);
      const color = routeType != null ? (TransitTypeRouteColor[routeType] ?? '#2563eb') : '#2563eb';

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
        const currentPos = existingMarker.getLatLng();
        if (currentPos.distanceTo(pos) > 0.5) {
          existingMarker.setLatLng(pos);
          const el = existingMarker.getElement();
          if (el) {
            const inner = el.querySelector('div');
            if (inner) {
              inner.classList.add('animate-marker-pulse');
              setTimeout(() => inner.classList.remove('animate-marker-pulse'), 600);
            }
          }
        }
        existingMarker.setTooltipContent(getTooltip(v, color, routeNames));
      } else {
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
            ">${String.fromCodePoint(0x1F68C)}</div>
          `,
          iconSize: [30, 30],
          iconAnchor: [15, 15],
        });

        const marker = L.marker(pos, { icon }).addTo(map);
        marker.bindTooltip(getTooltip(v, color, routeNames), {
          direction: 'top',
          offset: [0, -20],
          opacity: 1,
        });
        marker.on('click', () => {
          useAppStore.getState().setSelectedVehicle(v);
        });
        existing.set(v.vehicleId, marker);
      }
    }

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

function getTooltip(v: Vehicle, color: string, routeNames: Record<string, string>): string {
  const displayRoute = v.routeId ? (routeNames[v.routeId] ?? v.routeId) : 'N/A';
  return `
    <div style="font-size:12px;line-height:1.4">
      <div style="display:flex;align-items:center;gap:4px;margin-bottom:2px">
        <span style="display:inline-block;width:8px;height:8px;border-radius:50%;background:${color}"></span>
        <strong>${displayRoute}</strong>
      </div>
      <span style="color:#64748b">${v.speed.toFixed(0)} km/h</span>
    </div>
  `;
}
