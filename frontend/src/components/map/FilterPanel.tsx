import { Sun, Moon, Layers } from 'lucide-react';
import { Button } from '../ui/button';
import { useTranslation } from 'react-i18next';

interface RouteOption {
  routeId: string;
  shortName: string;
}

interface Props {
  routes: RouteOption[] | undefined;
  routeFilter: string;
  onRouteFilterChange: (value: string) => void;
  darkMode: boolean;
  onToggleDarkMode: () => void;
  vehicleCount: number;
  clusterMode: boolean;
  onToggleCluster: () => void;
}

export function FilterPanel({
  routes,
  routeFilter,
  onRouteFilterChange,
  darkMode,
  onToggleDarkMode,
  vehicleCount,
  clusterMode,
  onToggleCluster,
}: Props) {
  const { t } = useTranslation('map');

  return (
    <div className="flex flex-wrap items-center gap-2 sm:gap-3">
      <span className="text-xs sm:text-sm text-slate-500 w-full sm:w-auto">
        {t('vehicles_tracking', { count: vehicleCount })}
      </span>
      <select
        value={routeFilter}
        onChange={(e) => onRouteFilterChange(e.target.value)}
        className="text-sm border border-slate-300 rounded-md px-2 sm:px-3 py-1.5 bg-white flex-1 sm:flex-none"
        aria-label={t('filter_aria')}
      >
        <option value="">{t('all_routes')}</option>
        {routes?.map((r) => (
          <option key={r.routeId} value={r.routeId}>
            {r.shortName}
          </option>
        ))}
      </select>
      <Button
        variant="outline"
        size="sm"
        onClick={onToggleCluster}
        aria-label={clusterMode ? t('show_individual') : t('show_clusters')}
        title={clusterMode ? t('individual_markers') : t('cluster_markers')}
      >
        <Layers className="w-4 h-4" />
        <span className="hidden sm:inline">{clusterMode ? t('clusters') : t('individual')}</span>
      </Button>
      <Button
        variant="outline"
        size="icon"
        onClick={onToggleDarkMode}
        aria-label={darkMode ? t('switch_light') : t('switch_dark')}
        title={t('toggle_dark_mode')}
      >
        {darkMode ? <Sun className="w-4 h-4" /> : <Moon className="w-4 h-4" />}
      </Button>
    </div>
  );
}
