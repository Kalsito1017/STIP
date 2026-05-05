import { CircleMarker, Popup } from 'react-leaflet';

interface NearbyStop {
  stopId: string;
  stopName: string;
  lat: number;
  lon: number;
}

interface Props {
  stops: NearbyStop[];
}

export function NearbyStopsLayer({ stops }: Props) {
  return (
    <>
      {stops.map((s) => (
        <CircleMarker
          key={s.stopId}
          center={[s.lat, s.lon]}
          radius={10}
          pathOptions={{
            fillColor: '#3b82f6',
            color: '#1d4ed8',
            weight: 3,
            fillOpacity: 0.8,
            dashArray: '4 4',
          }}
        >
          <Popup>
            <div className="text-sm">
              <p className="font-semibold">{s.stopName}</p>
              <p className="text-slate-500">{s.stopId}</p>
            </div>
          </Popup>
        </CircleMarker>
      ))}
    </>
  );
}
