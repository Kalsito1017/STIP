import { useMemo } from 'react';
import { Bus, TramFront, Train, Zap } from 'lucide-react';
import type { Vehicle } from '../../store/useAppStore';
import { TransitTypeRouteColor } from '../../constants/transit';
import { useTranslation } from 'react-i18next';

interface Props {
  vehicles: Vehicle[];
}

interface TypeCount {
  type: number;
  label: string;
  color: string;
  icon: React.ComponentType<{ className?: string; color?: string }>;
  count: number;
}

function getRouteType(routeId: string | null): number {
  if (!routeId) return 3;
  if (routeId.includes('-tram-')) return 0;
  if (routeId.startsWith('r-m')) return 1;
  if (routeId.includes('-trol-')) return 11;
  return 3;
}

export function VehicleStatsBar({ vehicles }: Props) {
  const { t: tMap } = useTranslation('map');
  const { t: tTransit } = useTranslation('transit');
  const typeMeta: Record<number, { label: string; icon: React.ComponentType<{ className?: string; color?: string }> }> = {
    0: { label: tTransit('tram'), icon: TramFront },
    1: { label: tTransit('metro'), icon: Train },
    3: { label: tTransit('bus'), icon: Bus },
    11: { label: tTransit('trolley'), icon: Zap },
  };

  const typeCounts = useMemo((): TypeCount[] => {
    const counts = new Map<number, number>();
    for (const v of vehicles) {
      const rt = getRouteType(v.routeId);
      counts.set(rt, (counts.get(rt) ?? 0) + 1);
    }

    return Array.from(counts.entries())
      .sort((a, b) => b[1] - a[1])
      .map(([type, count]) => ({
        type,
        label: typeMeta[type]?.label ?? '',
        icon: typeMeta[type]?.icon ?? Bus,
        color: TransitTypeRouteColor[type] ?? '#64748b',
        count,
      }));
  }, [vehicles, tTransit]);

  const total = vehicles.length;

  if (total === 0) return null;

  return (
    <div className="absolute bottom-20 lg:bottom-6 left-3 sm:left-4 z-[1000] pointer-events-none">
      <div className="flex flex-wrap gap-1.5">
        {typeCounts.map((tc) => (
          <div
            key={tc.type}
            className="pointer-events-auto bg-card/90 backdrop-blur-sm border border-border rounded-full px-3 py-1.5 flex items-center gap-1.5 shadow-lg text-xs font-medium"
          >
            <tc.icon className="w-3 h-3" color={tc.color} />
            <span className="text-foreground">{tc.count}</span>
          </div>
        ))}
        <div className="pointer-events-auto bg-card/90 backdrop-blur-sm border border-border rounded-full px-3 py-1.5 flex items-center gap-1 shadow-lg text-xs font-medium">
          <span className="text-muted-foreground">{tMap('vehicles')}</span>
          <span className="text-foreground font-bold">{total}</span>
        </div>
      </div>
    </div>
  );
}
