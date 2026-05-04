import { useEffect, useState, useCallback } from 'react';
import { useMap } from 'react-leaflet';
import * as L from 'leaflet';
import { LocateFixed, Crosshair } from 'lucide-react';

export function LocateControl() {
  const map = useMap();
  const [locating, setLocating] = useState(false);
  const circleRef = useState<L.Circle | null>(null)[1];

  const handleLocate = useCallback(() => {
    setLocating(true);
    map.locate({ setView: true, maxZoom: 16, enableHighAccuracy: true });
  }, [map]);

  useEffect(() => {
    const onLocationFound = (e: L.LocationEvent) => {
      setLocating(false);
      L.circle(e.latlng, {
        radius: e.accuracy / 2,
        color: '#2563eb',
        fillColor: '#2563eb',
        fillOpacity: 0.15,
        weight: 1,
      }).addTo(map);
    };

    const onLocationError = () => {
      setLocating(false);
    };

    map.on('locationfound', onLocationFound);
    map.on('locationerror', onLocationError);

    return () => {
      map.off('locationfound', onLocationFound);
      map.off('locationerror', onLocationError);
    };
  }, [map, circleRef]);

  return (
    <div className="leaflet-top leaflet-right" style={{ marginTop: '80px' }}>
      <div className="leaflet-control">
        <button
          onClick={handleLocate}
          disabled={locating}
          className="bg-white border border-slate-300 rounded-md px-2.5 py-2 shadow-sm hover:bg-slate-50 disabled:opacity-50 cursor-pointer flex items-center gap-1.5 text-sm"
          aria-label="Locate me"
          title="Show my location"
          style={{ pointerEvents: 'auto' }}
        >
          {locating ? (
            <Crosshair className="w-4 h-4 text-blue-500 animate-spin" />
          ) : (
            <LocateFixed className="w-4 h-4 text-slate-600" />
          )}
        </button>
      </div>
    </div>
  );
}
