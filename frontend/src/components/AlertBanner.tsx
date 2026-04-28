import { useAppStore } from '../store/useAppStore';

const severityColors: Record<number, string> = {
  1: 'bg-blue-100 border-blue-400 text-blue-800',
  2: 'bg-yellow-100 border-yellow-400 text-yellow-800',
  3: 'bg-orange-100 border-orange-400 text-orange-800',
};

const severityLabels: Record<number, string> = {
  1: 'INFO',
  2: 'WARNING',
  3: 'SEVERE',
};


export function AlertBanner() {
  const alerts = useAppStore((s) => s.alerts);

  if (alerts.length === 0) return null;

  return (
    <div className="space-y-2">
      {alerts.map((alert) => {
        const severity = alert.severity ?? 2;
        const colorClass = severityColors[severity] ?? severityColors[2];

        return (
          <div
            key={alert.alertId}
            className={`border-l-4 px-4 py-3 rounded ${colorClass}`}
          >
            <div className="flex items-center gap-2">
              <span className="text-xs font-bold px-1.5 py-0.5 rounded bg-white/60">
                {severityLabels[severity] ?? 'ALERT'}
              </span>
              <span className="font-semibold">{alert.headerText}</span>
              {alert.informedEntities.some((e) => e.routeId) && (
                <span className="text-xs">
                  Routes: {[...new Set(alert.informedEntities.map((e) => e.routeId).filter(Boolean))].join(', ')}
                </span>
              )}
            </div>
            {alert.descriptionText && (
              <p className="text-sm mt-1">{alert.descriptionText}</p>
            )}
          </div>
        );
      })}
    </div>
  );
}