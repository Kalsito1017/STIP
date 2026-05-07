import { useState } from 'react';
import { ChevronDown, ChevronUp, X, AlertTriangle, Info, AlertOctagon } from 'lucide-react';
import { useAppStore } from '../store/useAppStore';
import { useTranslation } from 'react-i18next';

const severityStyles: Record<number, { bg: string; badge: string; icon: React.ComponentType<{ className?: string }> }> = {
  1: { bg: 'bg-blue-50/90 border-blue-400', badge: 'bg-blue-100 text-blue-700', icon: Info },
  2: { bg: 'bg-yellow-50/90 border-yellow-400', badge: 'bg-yellow-100 text-yellow-700', icon: AlertTriangle },
  3: { bg: 'bg-orange-50/90 border-orange-400', badge: 'bg-orange-100 text-orange-700', icon: AlertOctagon },
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
  const [dismissed, setDismissed] = useState<Set<string>>(new Set());

  if (alerts.length === 0) return null;

  const severityLabels: Record<number, string> = {
    1: t('info'),
    2: t('warning'),
    3: t('severe'),
  };

  const validAlerts = alerts.filter(
    (a) => isValidAlertText(a.headerText) || isValidAlertText(a.descriptionText)
  );
  const visibleAlerts = expanded ? validAlerts : validAlerts.slice(0, 2);

  return (
    <div className="max-h-[35vh] overflow-y-auto space-y-2 rounded-xl pr-1 scrollbar-thin">
      {visibleAlerts.map((alert) => {
        const severity = alert.severity ?? 2;
        const style = severityStyles[severity] ?? severityStyles[2];
        const label = severityLabels[severity] ?? t('alert');
        const SevIcon = style.icon;

        if (dismissed.has(alert.alertId)) return null;

        return (
          <div
            key={alert.alertId}
            className={`${style.bg} backdrop-blur-md border-l-4 rounded-xl px-4 py-3 shadow-sm`}
          >
            <div className="flex items-start gap-3">
              <SevIcon className="w-4 h-4 mt-0.5 flex-shrink-0 opacity-70" />
              <div className="flex-1 min-w-0">
                <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
                  <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded-full ${style.badge} flex-shrink-0`}>
                    {label}
                  </span>
                  {isValidAlertText(alert.headerText) && (
                    <span className="font-semibold text-sm break-words">{alert.headerText}</span>
                  )}
                </div>
                {alert.informedEntities.some((e) => e.routeId) && (
                  <span className="text-[11px] text-muted-foreground mt-1 inline-block">
                    {t('routes_prefix')} {[...new Set(alert.informedEntities.map((e) => e.routeId).filter(Boolean))].join(', ')}
                  </span>
                )}
                {isValidAlertText(alert.descriptionText) && (
                  <p className="text-xs text-muted-foreground mt-1.5 break-words leading-relaxed">{alert.descriptionText}</p>
                )}
              </div>
              <button
                onClick={() => setDismissed((prev) => new Set(prev).add(alert.alertId))}
                className="p-1 rounded-lg hover:bg-black/5 text-muted-foreground/60 hover:text-foreground transition-colors flex-shrink-0"
                aria-label="Dismiss alert"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        );
      })}

      {validAlerts.length > 2 && (
        <button
          onClick={() => setExpanded(!expanded)}
          className="w-full flex items-center justify-center gap-1.5 text-xs text-muted-foreground hover:text-foreground py-1.5 rounded-lg hover:bg-card/80 transition-colors"
        >
          {expanded ? (
            <>{t('show_less', { ns: 'common' })} <ChevronUp className="w-3 h-3" /></>
          ) : (
            <>{validAlerts.length - 2} {t('more_alerts', { defaultValue: 'more alerts' })} <ChevronDown className="w-3 h-3" /></>
          )}
        </button>
      )}
    </div>
  );
}
