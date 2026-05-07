import { RefreshCw } from 'lucide-react';
import { usePredictedArrivals, type PredictedArrival } from '../hooks/useArrivals';
import { TransitTypeRouteColor } from '../constants/transit';
import { useTranslation } from 'react-i18next';

interface Props {
  stopId: string;
}

function getRouteTypeFromId(routeId: string): number {
  if (routeId.includes('-tram-')) return 0;
  if (routeId.startsWith('r-m')) return 1;
  if (routeId.includes('-trol-')) return 11;
  return 3;
}

function delayColor(seconds: number | null): string {
  if (seconds === null) return 'text-muted-foreground';
  if (seconds < 120) return 'text-green-600 dark:text-green-400';
  if (seconds < 300) return 'text-amber-600 dark:text-amber-400';
  return 'text-red-600 dark:text-red-400';
}

function delayLabel(seconds: number | null): string {
  if (seconds === null) return '—';
  if (seconds < 60) return `${seconds}s`;
  return `${Math.round(seconds / 60)}m`;
}

export function ArrivalsBoard({ stopId }: Props) {
  const { t } = useTranslation('stops');
  const { data: arrivals, isLoading, refetch, isFetching } = usePredictedArrivals(stopId);

  return (
    <div className="bg-card border border-border rounded-xl shadow-sm overflow-hidden">
      <div className="flex items-center justify-between px-4 py-3 border-b border-border">
        <h3 className="text-sm font-semibold text-foreground">{t('upcoming_arrivals')}</h3>
        <button
          onClick={() => refetch()}
          disabled={isFetching}
          className="p-1.5 rounded-lg hover:bg-accent text-muted-foreground hover:text-foreground transition-colors disabled:opacity-50"
          aria-label="Refresh arrivals"
        >
          <RefreshCw className={`w-3.5 h-3.5 ${isFetching ? 'animate-spin' : ''}`} />
        </button>
      </div>

      <div className="divide-y divide-border">
        {isLoading ? (
          Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="px-4 py-3 flex items-center gap-3">
              <div className="w-10 h-6 rounded-full bg-muted animate-pulse" />
              <div className="flex-1 space-y-1.5">
                <div className="h-3.5 bg-muted rounded animate-pulse w-24" />
                <div className="h-3 bg-muted rounded animate-pulse w-16" />
              </div>
              <div className="h-4 bg-muted rounded animate-pulse w-12" />
            </div>
          ))
        ) : arrivals && arrivals.length > 0 ? (
          arrivals.map((a: PredictedArrival, idx: number) => {
            const routeType = getRouteTypeFromId(a.routeId);
            const color = TransitTypeRouteColor[routeType] ?? '#64748b';
            return (
              <div key={idx} className="px-4 py-3 flex items-center gap-3 hover:bg-accent/50 transition-colors">
                <div
                  className="w-10 h-6 rounded-full flex items-center justify-center text-white text-xs font-bold flex-shrink-0"
                  style={{ backgroundColor: color }}
                >
                  {a.routeShortName}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-foreground truncate">{a.destination}</p>
                  <p className="text-xs text-muted-foreground">
                    {a.scheduledMinutes > 0 ? t('in_minutes', { minutes: a.scheduledMinutes, defaultValue: `in ${a.scheduledMinutes} min` }) : t('arriving_now', { defaultValue: 'Arriving now' })}
                  </p>
                </div>
                <div className="text-right flex-shrink-0">
                  <p className={`text-sm font-semibold tabular-nums ${delayColor(a.predictedDelaySeconds)}`}>
                    {a.predictedDelaySeconds !== null ? `+${delayLabel(a.predictedDelaySeconds)}` : t('on_time', { ns: 'map', defaultValue: 'On time' })}
                  </p>
                  {a.predictedDelaySeconds !== null && (
                    <p className="text-[10px] text-muted-foreground">{t('predicted', { defaultValue: 'predicted' })}</p>
                  )}
                </div>
              </div>
            );
          })
        ) : (
          <div className="px-4 py-6 text-center">
            <p className="text-sm text-muted-foreground">{t('no_arrivals', { defaultValue: 'No upcoming arrivals' })}</p>
          </div>
        )}
      </div>
    </div>
  );
}
