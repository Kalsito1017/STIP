import { useState } from 'react';
import { ChevronDown, ChevronUp } from 'lucide-react';
import { useAppStore } from '../store/useAppStore';
import { useTranslation } from 'react-i18next';

const severityColors: Record<number, string> = {
  1: 'bg-blue-100 border-blue-400 text-blue-800',
  2: 'bg-yellow-100 border-yellow-400 text-yellow-800',
  3: 'bg-orange-100 border-orange-400 text-orange-800',
};

const LANGUAGE_CODES = new Set(['en', 'bg', 'de', 'fr', 'es', 'it', 'ru', 'tr', 'ja', 'zh', 'ko', 'ar']);

function isValidAlertText(text: string | null | undefined): boolean {
  if (!text) return false;
  const trimmed = text.trim();
  if (trimmed.length === 0) return false;
  if (LANGUAGE_CODES.has(trimmed.toLowerCase())) return false;
  return true;
}

export function AlertBanner() {
  const { t } = useTranslation('alerts');
  const alerts = useAppStore((s) => s.alerts);
  const [expanded, setExpanded] = useState(false);

  if (alerts.length === 0) return null;

  const severityLabels: Record<number, string> = {
    1: t('info'),
    2: t('warning'),
    3: t('severe'),
  };

  const visibleAlerts = expanded ? alerts : alerts.slice(0, 3);

  return (
    <div className="max-h-[40vh] overflow-y-auto space-y-2 rounded-lg pr-1">
      {visibleAlerts.map((alert) => {
        const severity = alert.severity ?? 2;
        const colorClass = severityColors[severity] ?? severityColors[2];
        const label = severityLabels[severity] ?? t('alert');

        if (!isValidAlertText(alert.headerText) && !isValidAlertText(alert.descriptionText)) {
          return null;
        }

        return (
          <div
            key={alert.alertId}
            className={`border-l-4 px-3 sm:px-4 py-2 sm:py-3 rounded ${colorClass}`}
          >
            <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
              <span className="text-xs font-bold px-1.5 py-0.5 rounded bg-white/60 flex-shrink-0">
                {label}
              </span>
              {isValidAlertText(alert.headerText) && (
                <span className="font-semibold text-sm sm:text-base break-words">{alert.headerText}</span>
              )}
              {alert.informedEntities.some((e) => e.routeId) && (
                <span className="text-xs flex-shrink-0">
                  {t('routes_prefix')} {[...new Set(alert.informedEntities.map((e) => e.routeId).filter(Boolean))].join(', ')}
                </span>
              )}
            </div>
            {isValidAlertText(alert.descriptionText) && (
              <p className="text-xs sm:text-sm mt-1 break-words">{alert.descriptionText}</p>
            )}
          </div>
        );
      })}

      {alerts.length > 3 && (
        <button
          onClick={() => setExpanded(!expanded)}
          className="w-full flex items-center justify-center gap-1 text-xs text-slate-500 hover:text-slate-700 py-1"
        >
          {expanded ? (
            <>{t('show_less', { ns: 'common' })} <ChevronUp className="w-3 h-3" /></>
          ) : (
            <>{alerts.length - 3} {t('more_alerts', { defaultValue: 'more alerts' })} <ChevronDown className="w-3 h-3" /></>
          )}
        </button>
      )}
    </div>
  );
}
