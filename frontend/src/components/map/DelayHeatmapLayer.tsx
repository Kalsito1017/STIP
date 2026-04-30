import { CircleMarker, Popup } from 'react-leaflet';

interface HeatmapPoint {
  lat: number;
  lon: number;
  avgDelaySeconds: number;
  sampleCount: number;
}

interface Props {
  points: HeatmapPoint[];
}

function delayColor(seconds: number): string {
  if (seconds <= 60) return '#22c55e';
  if (seconds <= 180) return '#eab308';
  if (seconds <= 300) return '#f97316';
  return '#ef4444';
}

function delayLabel(seconds: number): string {
  if (seconds <= 60) return 'On time';
  if (seconds <= 180) return 'Slight delay';
  if (seconds <= 300) return 'Moderate delay';
  return 'Severe delay';
}

function pointRadius(sampleCount: number): number {
  return Math.max(8, Math.min(30, 6 * Math.log1p(sampleCount)));
}

export function DelayHeatmapLayer({ points }: Props) {
  return (
    <>
      {points.map((p, i) => (
        <CircleMarker
          key={i}
          center={[p.lat, p.lon]}
          radius={pointRadius(p.sampleCount)}
          pathOptions={{
            fillColor: delayColor(p.avgDelaySeconds),
            color: '#ffffff',
            weight: 1,
            fillOpacity: 0.6,
          }}
        >
          <Popup>
            <div className="text-sm">
              <p><strong>Avg Delay:</strong> {p.avgDelaySeconds.toFixed(0)}s</p>
              <p><strong>Samples:</strong> {p.sampleCount}</p>
              <p><strong>Status:</strong> {delayLabel(p.avgDelaySeconds)}</p>
            </div>
          </Popup>
        </CircleMarker>
      ))}
    </>
  );
}
