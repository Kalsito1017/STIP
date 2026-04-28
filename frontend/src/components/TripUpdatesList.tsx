import { useAppStore } from '../store/useAppStore';

const relationshipLabels: Record<number, string> = {
  0: 'Scheduled',
  1: 'Added',
  2: 'Unscheduled',
  3: 'Canceled',
};

function formatDelay(seconds: number | null): string {
  if (seconds === null || seconds === undefined) return '—';
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
  const tripUpdates = useAppStore((s) => s.tripUpdates);

  if (tripUpdates.length === 0) {
    return (
      <div className="bg-white rounded-lg border border-slate-200 p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-slate-700 mb-2">Trip Updates</h3>
        <p className="text-slate-400 text-sm">No trip update data available</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg border border-slate-200 p-5 shadow-sm">
      <h3 className="text-sm font-semibold text-slate-700 mb-4">
        Trip Updates <span className="text-slate-400 font-normal">({tripUpdates.length})</span>
      </h3>
      <div className="space-y-3 max-h-[300px] overflow-y-auto">
        {tripUpdates.slice(0, 20).map((tu) => (
          <div key={tu.tripId} className="border-b border-slate-100 pb-2 last:border-0">
            <div className="flex items-center justify-between text-sm">
              <div className="flex items-center gap-2">
                <span className="font-medium text-slate-800">
                  {tu.routeId ?? '—'}
                </span>
                <span className="text-xs text-slate-400">
                  {relationshipLabels[tu.scheduleRelationship] ?? 'Unknown'}
                </span>
              </div>
              <span className="text-xs text-slate-400">
                Trip {tu.tripId.slice(-6)}
              </span>
            </div>
            {tu.stopTimeUpdates.length > 0 && (
              <div className="mt-1 pl-3 space-y-0.5">
                {tu.stopTimeUpdates.slice(0, 3).map((stu, idx) => (
                  <div key={idx} className="flex items-center gap-2 text-xs">
                    <span className="text-slate-500">
                      Stop {stu.stopSequence ?? stu.stopId ?? '?'}
                    </span>
                    <span className={`font-mono ${delayColor(stu.arrivalDelay)}`}>
                      {formatDelay(stu.arrivalDelay)}
                    </span>
                  </div>
                ))}
                {tu.stopTimeUpdates.length > 3 && (
                  <span className="text-xs text-slate-400">
                    +{tu.stopTimeUpdates.length - 3} more stops
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