import { useAppStore } from '../../store/useAppStore';
import { useTranslation } from 'react-i18next';

const stateConfig = {
  connected: { color: 'bg-green-500', ring: 'ring-green-500/30' },
  reconnecting: { color: 'bg-yellow-500', ring: 'ring-yellow-500/30' },
  disconnected: { color: 'bg-red-500', ring: 'ring-red-500/30' },
} as const;

export function ConnectionIndicator() {
  const { t } = useTranslation('layout');
  const connectionState = useAppStore((s) => s.connectionState);
  const cfg = stateConfig[connectionState];
  const label = t(connectionState);

  return (
    <div className="flex items-center gap-2 px-2.5 py-1.5 rounded-full bg-card/80 backdrop-blur-md shadow-sm border border-border/60 text-xs text-muted-foreground select-none">
      <span className="relative flex h-2.5 w-2.5">
        <span className={`absolute inline-flex h-full w-full rounded-full ${cfg.color} opacity-40 ${connectionState === 'reconnecting' ? 'animate-ping' : ''}`} />
        <span className={`relative inline-flex h-2.5 w-2.5 rounded-full ${cfg.color} ring-2 ${cfg.ring}`} />
      </span>
      <span className="hidden sm:inline">{label}</span>
    </div>
  );
}
