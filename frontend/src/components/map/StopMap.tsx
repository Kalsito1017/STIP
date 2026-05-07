import { useEffect } from 'react';
import { MapContainer, useMap, ZoomControl } from 'react-leaflet';
import * as L from 'leaflet';
import { useNavigate } from 'react-router-dom';
import { Map as MapIcon } from 'lucide-react';
import { MapLibreBasemap } from './MapLibreBasemap';
import { useAppStore } from '../../store/useAppStore';

interface Props {
  stopId: string;
  lat: number;
  lon: number;
  stopName: string;
}

function CenterMarker({ lat, lon }: { lat: number; lon: number }) {
  const map = useMap();

  useEffect(() => {
    L.circleMarker([lat, lon], {
      radius: 8,
      fillColor: '#ef4444',
      color: '#ffffff',
      weight: 3,
      fillOpacity: 0.9,
    }).addTo(map);

    map.setView([lat, lon], 16);
  }, [map, lat, lon]);

  return null;
}

function FlyToButton({ lat, lon }: { lat: number; lon: number }) {
  const navigate = useNavigate();
  const setFlyToTarget = useAppStore((s) => s.setFlyToTarget);

  return (
    <button
      onClick={(e) => {
        e.stopPropagation();
        setFlyToTarget({ lat, lon, zoom: 17 });
        navigate('/');
      }}
      className="absolute bottom-2 right-2 z-[1000] bg-card/90 backdrop-blur-md border border-border/60 rounded-lg px-2.5 py-1.5 flex items-center gap-1.5 text-xs font-medium text-foreground shadow-md hover:bg-card transition-colors"
    >
      <MapIcon className="w-3 h-3" />
      Live Map
    </button>
  );
}

export function StopMap({ stopId: _stopId, lat, lon, stopName: _stopName }: Props) {
  const darkMode = useAppStore((s) => s.darkMode);

  return (
    <div className="relative rounded-xl overflow-hidden border border-border shadow-sm">
      <div className="h-48 sm:h-56">
        <MapContainer
          center={[lat, lon]}
          zoom={16}
          style={{ width: '100%', height: '100%' }}
          zoomControl={false}
          scrollWheelZoom={false}
          preferCanvas
          attributionControl={false}
        >
          <ZoomControl position="topright" />
          <MapLibreBasemap
            styleUrl={darkMode ? '/map-styles/dark.json' : '/map-styles/light.json'}
          />
          <CenterMarker lat={lat} lon={lon} />
        </MapContainer>
      </div>
      <FlyToButton lat={lat} lon={lon} />
    </div>
  );
}
