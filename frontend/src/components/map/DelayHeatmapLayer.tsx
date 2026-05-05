import { CircleMarker, Popup } from 'react-leaflet';
import { useTranslation } from 'react-i18next';

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

function pointRadius(sampleCount: number): number {
  return Math.max(8, Math.min(30, 6 * Math.log1p(sampleCount)));
}

export function DelayHeatmapLayer({ points }: Props) {
  const { t } = useTranslation('map');

  const delayLabel = (seconds: number): string => {
    if (seconds <= 60) return t('on_time');
    if (seconds <= 180) return t('slight_delay');
    if (seconds <= 300) return t('moderate_delay');
    return t('severe_delay');
  };

  return (
    <>
      {points.map((p) => (
        <CircleMarker
          key={`${p.lat.toFixed(6)}-${p.lon.toFixed(6)}`}
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
              <p><strong>{t('avg_delay_label')}</strong> {p.avgDelaySeconds.toFixed(0)}s</p>
              <p><strong>{t('samples_label')}</strong> {p.sampleCount}</p>
              <p><strong>{t('status_label')}</strong> {delayLabel(p.avgDelaySeconds)}</p>
            </div>
          </Popup>
        </CircleMarker>
      ))}
    </>
  );
}
