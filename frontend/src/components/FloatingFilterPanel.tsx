import { useState } from 'react';
import { SlidersHorizontal, Layers, Bus, MapPin, Thermometer, Car } from 'lucide-react';
import { Button } from './ui/button';

interface RouteOption {
  routeId: string;
  shortName: string;
}

interface Props {
  routes: RouteOption[] | undefined;
  routesLoading?: boolean;
  routeFilter: string;
  onRouteFilterChange: (value: string) => void;
  vehicleCount: number;
  clusterMode: boolean;
  onToggleCluster: () => void;
  showRoutes: boolean;
  showStops: boolean;
  showHeatmap: boolean;
  showVehicles: boolean;
  onToggleRoutes: () => void;
  onToggleStops: () => void;
  onToggleHeatmap: () => void;
  onToggleVehicles: () => void;
}

export function FloatingFilterPanel({
  routes,
  routesLoading,
  routeFilter,
  onRouteFilterChange,
  vehicleCount,
  clusterMode,
  onToggleCluster,
  showRoutes,
  showStops,
  showHeatmap,
  showVehicles,
  onToggleRoutes,
  onToggleStops,
  onToggleHeatmap,
  onToggleVehicles,
}: Props) {
  const [expanded, setExpanded] = useState(false);

  return (
    <div className="absolute bottom-6 left-3 sm:left-4 z-[1000] pointer-events-auto">
      {expanded && (
        <div className="mb-2 bg-white border border-slate-200 rounded-lg shadow-lg p-3 w-56">
          <div className="space-y-2">
            <div className="text-xs font-semibold text-slate-500 uppercase tracking-wide">
              {vehicleCount} vehicles tracking
            </div>

            <select
              value={routeFilter}
              onChange={(e) => onRouteFilterChange(e.target.value)}
              disabled={routesLoading || !routes}
              className="w-full text-sm border border-slate-300 rounded-md px-2 py-1.5 bg-white disabled:opacity-50 disabled:cursor-not-allowed"
              aria-label="Filter vehicles by route"
            >
              <option value="">{routesLoading ? 'Loading routes...' : 'All routes'}</option>
              {routes?.map((r) => (
                <option key={r.routeId} value={r.routeId}>
                  {r.shortName}
                </option>
              ))}
            </select>

            <div className="space-y-1 pt-1 border-t border-slate-100">
              <ToggleRow icon={Bus} label="Route Shapes" checked={showRoutes} onChange={onToggleRoutes} />
              <ToggleRow icon={MapPin} label="Stops" checked={showStops} onChange={onToggleStops} />
              <ToggleRow icon={Thermometer} label="Delay Heatmap" checked={showHeatmap} onChange={onToggleHeatmap} />
              <ToggleRow icon={Car} label="Vehicles" checked={showVehicles} onChange={onToggleVehicles} />
            </div>

            <div className="pt-1 border-t border-slate-100">
              <ToggleRow
                icon={Layers}
                label={clusterMode ? 'Cluster markers' : 'Individual markers'}
                checked={clusterMode}
                onChange={onToggleCluster}
              />
            </div>
          </div>
        </div>
      )}

      <Button
        variant="outline"
        size="sm"
        onClick={() => setExpanded((v) => !v)}
        className={`bg-white shadow-sm border-slate-200 gap-2 h-10 ${expanded ? 'ring-2 ring-blue-400' : ''}`}
        aria-label="Toggle filter panel"
      >
        <SlidersHorizontal className="w-4 h-4" />
        <span className="hidden sm:inline">Filters</span>
      </Button>
    </div>
  );
}

function ToggleRow({ icon: Icon, label, checked, onChange }: {
  icon: React.ComponentType<{ className?: string }>;
  label: string;
  checked: boolean;
  onChange: () => void;
}) {
  return (
    <label className="flex items-center gap-2 py-1 cursor-pointer text-sm text-slate-700 hover:text-slate-900">
      <input
        type="checkbox"
        checked={checked}
        onChange={onChange}
        className="rounded border-slate-300 text-blue-600 focus:ring-blue-500"
      />
      <Icon className="w-3.5 h-3.5 text-slate-400 flex-shrink-0" />
      <span>{label}</span>
    </label>
  );
}
