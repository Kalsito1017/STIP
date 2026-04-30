import { Marker, Popup } from 'react-leaflet';
import * as L from 'leaflet';
import type { Vehicle } from '../../store/useAppStore';

interface Props {
  vehicles: Vehicle[];
}

export function VehicleLayer({ vehicles }: Props) {
  return (
    <>
      {vehicles.map((v) => {
        const icon = L.divIcon({
          className: 'vehicle-marker',
          html: `<div style="transform:rotate(${v.bearing}deg);background:#2563eb;color:white;border-radius:50%;width:28px;height:28px;display:flex;align-items:center;justify-content:center;font-size:14px;font-weight:bold;border:2px solid white;box-shadow:0 2px 4px rgba(0,0,0,.3);cursor:pointer">\u{1F68C}</div>`,
          iconSize: [28, 28],
          iconAnchor: [14, 14],
        });

        return (
          <Marker key={v.vehicleId} position={[v.lat, v.lon]} icon={icon}>
            <Popup>
              <div className="text-sm">
                <p><strong>Route:</strong> {v.routeId ?? 'N/A'}</p>
                <p><strong>Speed:</strong> {v.speed} km/h</p>
                <p><strong>Bearing:</strong> {v.bearing}\u00B0</p>
              </div>
            </Popup>
          </Marker>
        );
      })}
    </>
  );
}
