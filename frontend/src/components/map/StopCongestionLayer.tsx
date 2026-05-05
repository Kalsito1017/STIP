import { CircleMarker, Popup } from 'react-leaflet';
import { useTranslation } from 'react-i18next';

interface CongestionPoint {
  stopId: string;
  stopName: string;
  lat: number;
  lon: number;
  avgDelaySeconds: number;
  sampleCount: number;
  severity: string;
}

interface Props {
  points: CongestionPoint[];
}

function severityColor(severity: string): string {
  switch (severity) {
    case 'low': return '#22c55e';
    case 'medium': return '#eab308';
    case 'high': return '#f97316';
    case 'severe': return '#ef4444';
    default: return '#6b7280';
  }
}

function pointRadius(sampleCount: number): number {
  return Math.max(6, Math.min(20, 4 * Math.log1p(sampleCount)));
}

export function StopCongestionLayer({ points }: Props) {
  const { t } = useTranslation('map');

  return (
    <>
      {points.map((p) => (
        <CircleMarker
          key={p.stopId}
          center={[p.lat, p.lon]}
          radius={pointRadius(p.sampleCount)}
          pathOptions={{
            fillColor: severityColor(p.severity),
            color: '#ffffff',
            weight: 2,
            fillOpacity: 0.7,
          }}
        >
          <Popup>
            <div className="text-sm">
              <p className="font-semibold">{p.stopName}</p>
              <p><strong>{t('avg_delay_label')}</strong> {p.avgDelaySeconds.toFixed(0)}s</p>
              <p><strong>{t('trips_today')}</strong> {p.sampleCount}</p>
              <p><strong>{t('severity_label')}</strong> {p.severity}</p>
            </div>
          </Popup>
        </CircleMarker>
      ))}
    </>
  );
}
