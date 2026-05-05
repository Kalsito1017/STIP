import { useEffect, useRef } from 'react';
import { useMap } from 'react-leaflet';
import * as L from 'leaflet';
import type { Vehicle } from '../../store/useAppStore';
import { useAppStore } from '../../store/useAppStore';
import { TransitTypeRouteColor } from '../../constants/transit';
import i18n from '../../i18n';

const CLUSTER_RADIUS_PX = 60;

interface ClusterGroup {
  lat: number;
  lon: number;
  count: number;
  routeTypes: Set<number>;
}

interface Props {
  vehicles: Vehicle[];
  routeNames: Record<string, string>;
}

function getRouteType(v: Vehicle): number {
  if (!v.routeId) return 3;
  if (v.routeId.includes('-tram-')) return 0;
  if (v.routeId.startsWith('r-m')) return 1;
  if (v.routeId.includes('-trol-')) return 11;
  return 3;
}

export function VehicleClusterLayer({ vehicles, routeNames }: Props) {
  const map = useMap();
  const layerRef = useRef<L.LayerGroup | null>(null);
  const zoomRef = useRef(map.getZoom());

  useEffect(() => {
    if (!layerRef.current) {
      layerRef.current = L.layerGroup().addTo(map);
    }
    return () => {
      layerRef.current?.remove();
      layerRef.current = null;
    };
  }, [map]);

  useEffect(() => {
    const onZoom = () => {
      zoomRef.current = map.getZoom();
    };
    map.on('zoomend', onZoom);
    return () => { map.off('zoomend', onZoom); };
  }, [map]);

  useEffect(() => {
    const layer = layerRef.current;
    if (!layer) return;
    layer.clearLayers();

    if (vehicles.length === 0) return;

    const zoom = zoomRef.current;

    if (zoom >= 15 || vehicles.length < 20) {
      for (const v of vehicles) {
        const routeType = getRouteType(v);
        const color = TransitTypeRouteColor[routeType] ?? '#2563eb';
        const displayRoute = v.routeId ? (routeNames[v.routeId] ?? v.routeId) : 'N/A';

        const icon = L.divIcon({
          className: 'vehicle-marker',
          html: `
            <div style="
              background:${color};
              color:white;
              border-radius:50%;
              width:26px;
              height:26px;
              display:flex;
              align-items:center;
              justify-content:center;
              font-size:13px;
              border:2px solid white;
              box-shadow:0 1px 4px rgba(0,0,0,.3);
              cursor:pointer;
              transform:rotate(${v.bearing}deg);
              transition:transform 0.4s ease;
            ">${String.fromCodePoint(0x1F68C)}</div>
          `,
          iconSize: [26, 26],
          iconAnchor: [13, 13],
        });

        L.marker([v.lat, v.lon], { icon })
          .bindTooltip(`<b>${displayRoute}</b><br/>${v.speed.toFixed(0)} ${i18n.t('common:km_h')}`, {
            direction: 'top',
            offset: [0, -16],
            opacity: 1,
          })
          .on('click', () => {
            useAppStore.getState().setSelectedVehicle(v);
          })
          .addTo(layer);
      }
      return;
    }

    const clusters = new Map<string, ClusterGroup>();
    const cellSize = CLUSTER_RADIUS_PX * 2;

    for (const v of vehicles) {
      const point = map.latLngToContainerPoint(L.latLng(v.lat, v.lon));
      const cellX = Math.round(point.x / cellSize);
      const cellY = Math.round(point.y / cellSize);
      const key = `${cellX},${cellY}`;

      const existing = clusters.get(key);
      if (existing) {
        existing.count++;
        existing.lat += v.lat;
        existing.lon += v.lon;
        existing.routeTypes.add(getRouteType(v));
      } else {
        clusters.set(key, {
          lat: v.lat,
          lon: v.lon,
          count: 1,
          routeTypes: new Set([getRouteType(v)]),
        });
      }
    }

    for (const [, cluster] of clusters) {
      const avgLat = cluster.lat / cluster.count;
      const avgLon = cluster.lon / cluster.count;
      const primaryColor = TransitTypeRouteColor[[...cluster.routeTypes][0]] ?? '#2563eb';
      const size = Math.min(28 + cluster.count * 3, 52);

      const icon = L.divIcon({
        className: 'vehicle-cluster-marker',
        html: `
          <div style="
            background:${primaryColor};
            color:white;
            border-radius:50%;
            width:${size}px;
            height:${size}px;
            display:flex;
            align-items:center;
            justify-content:center;
            font-size:${size > 42 ? '13px' : '11px'};
            font-weight:700;
            border:3px solid white;
            box-shadow:0 2px 8px rgba(0,0,0,.35);
            cursor:pointer;
          ">${cluster.count}</div>
        `,
        iconSize: [size, size],
        iconAnchor: [size / 2, size / 2],
      });

      L.marker([avgLat, avgLon], { icon })
        .bindTooltip(`<b>${i18n.t('map:vehicles_count', { count: cluster.count })}</b>`, {
          direction: 'top',
          offset: [0, -10],
          opacity: 1,
        })
        .addTo(layer);
    }
  }, [vehicles, map, routeNames]);

  return null;
}
