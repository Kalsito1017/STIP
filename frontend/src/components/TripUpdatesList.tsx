import { useAppStore } from '../store/useAppStore';
import { Skeleton } from '../components/Skeleton';
import { useTranslation } from 'react-i18next';

function formatDelay(seconds: number | null): string {
  if (seconds === null || seconds === undefined) return '\u2014';
  const sign = seconds >= 0 ? '+' : '';
  const mins = Math.floor(Math.abs(seconds) / 60);
  const secs = Math.abs(seconds) % 60;
  return `${sign}${seconds >= 0 ? '' : '-'}${mins}m ${secs}s`;
}

function delayColor(seconds: number | null): string {
  if (seconds === null || seconds === undefined) return 'text-slate-500';
  if (seconds <= 60) return 'text-green-600';
  if (seconds <= 180) return 'text-yellow-600';
  return 'text-red-600';
}

export function TripUpdatesList() {
  const { t } = useTranslation('dashboard');
  const tripUpdates = useAppStore((s) => s.tripUpdates);
  const connectionState = useAppStore((s) => s.connectionState);

  const loading = tripUpdates.length === 0 && connectionState !== 'connected';

  if (loading) {
    return (
      <div className="bg-card rounded-lg border border-border p-4 sm:p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-foreground mb-4">{t('trip_updates')}</h3>
        <div className="space-y-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="border-b border-border pb-2 last:border-0 space-y-1.5">
              <div className="flex items-center gap-2">
                <Skeleton className="h-3 w-14" />
                <Skeleton className="h-3 w-16" />
              </div>
              <div className="flex items-center gap-2 pl-3">
                <Skeleton className="h-2.5 w-20" />
                <Skeleton className="h-2.5 w-16" />
              </div>
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (tripUpdates.length === 0) {
    return (
      <div className="bg-card rounded-lg border border-border p-4 sm:p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-foreground mb-2">{t('trip_updates')}</h3>
        <p className="text-muted-foreground text-sm">{t('no_trip_updates')}</p>
      </div>
    );
  }

  const relationshipLabels: Record<number, string> = {
    0: t('scheduled'),
    1: t('added'),
    2: t('unscheduled'),
    3: t('canceled'),
  };

  return (
    <div className="bg-card rounded-lg border border-border p-4 sm:p-5 shadow-sm">
      <h3 className="text-sm font-semibold text-foreground mb-4">
        {t('trip_updates')} <span className="text-muted-foreground font-normal">({tripUpdates.length})</span>
      </h3>
      <div className="space-y-3 max-h-[300px] overflow-y-auto">
        {tripUpdates.slice(0, 20).map((tu) => (
          <div key={tu.tripId} className="border-b border-border pb-2 last:border-0">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-1">
              <div className="flex items-center gap-2 flex-wrap">
                <span className="font-medium text-foreground text-sm">
                  {tu.routeId ?? '\u2014'}
                </span>
                <span className="text-xs text-muted-foreground">
                  {relationshipLabels[tu.scheduleRelationship] ?? '\u2014'}
                </span>
              </div>
              <span className="text-xs text-muted-foreground">
                {t('trip')} {tu.tripId.slice(-6)}
              </span>
            </div>
            {tu.stopTimeUpdates.length > 0 && (
              <div className="mt-1 pl-3 space-y-0.5">
                {tu.stopTimeUpdates.slice(0, 3).map((stu, idx) => (
                  <div key={idx} className="flex items-center gap-2 text-xs flex-wrap">
                    <span className="text-muted-foreground">
                      {t('stop')} {stu.stopSequence ?? stu.stopId ?? '?'}
                    </span>
                    <span className={`font-mono ${delayColor(stu.arrivalDelay)}`}>
                      {formatDelay(stu.arrivalDelay)}
                    </span>
                  </div>
                ))}
                {tu.stopTimeUpdates.length > 3 && (
                  <span className="text-xs text-muted-foreground">
                    {t('more_stops', { count: tu.stopTimeUpdates.length - 3 })}
                  </span>
                )}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
