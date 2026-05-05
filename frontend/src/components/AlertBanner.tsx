import { useAppStore } from '../store/useAppStore';
import { useTranslation } from 'react-i18next';

const severityColors: Record<number, string> = {
  1: 'bg-blue-100 border-blue-400 text-blue-800',
  2: 'bg-yellow-100 border-yellow-400 text-yellow-800',
  3: 'bg-orange-100 border-orange-400 text-orange-800',
};

export function AlertBanner() {
  const { t } = useTranslation('alerts');
  const alerts = useAppStore((s) => s.alerts);

  if (alerts.length === 0) return null;

  const severityLabels: Record<number, string> = {
    1: t('info'),
    2: t('warning'),
    3: t('severe'),
  };

  return (
    <div className="space-y-2">
      {alerts.map((alert) => {
        const severity = alert.severity ?? 2;
        const colorClass = severityColors[severity] ?? severityColors[2];
        const label = severityLabels[severity] ?? t('alert');

        return (
          <div
            key={alert.alertId}
            className={`border-l-4 px-3 sm:px-4 py-2 sm:py-3 rounded ${colorClass}`}
          >
            <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
              <span className="text-xs font-bold px-1.5 py-0.5 rounded bg-white/60 flex-shrink-0">
                {label}
              </span>
              <span className="font-semibold text-sm sm:text-base break-words">{alert.headerText}</span>
              {alert.informedEntities.some((e) => e.routeId) && (
                <span className="text-xs flex-shrink-0">
                  {t('routes_prefix')} {[...new Set(alert.informedEntities.map((e) => e.routeId).filter(Boolean))].join(', ')}
                </span>
              )}
            </div>
            {alert.descriptionText && (
              <p className="text-xs sm:text-sm mt-1 break-words">{alert.descriptionText}</p>
            )}
          </div>
        );
      })}
    </div>
  );
}
