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
}

export function FilterPanel({
  routes,
  routeFilter,
  onRouteFilterChange,
  darkMode,
  onToggleDarkMode,
  vehicleCount,
}: Props) {
  return (
    <div className="flex flex-wrap items-center gap-2 sm:gap-3">
      <span className="text-xs sm:text-sm text-slate-500 w-full sm:w-auto">
        {vehicleCount} vehicles tracking
      </span>
      <select
        value={routeFilter}
        onChange={(e) => onRouteFilterChange(e.target.value)}
        className="text-sm border border-slate-300 rounded-md px-2 sm:px-3 py-1.5 bg-white flex-1 sm:flex-none"
      >
        <option value="">All routes</option>
        {routes?.map((r) => (
          <option key={r.routeId} value={r.routeId}>
            {r.shortName}
          </option>
        ))}
      </select>
      <button
        onClick={onToggleDarkMode}
        className="text-sm border border-slate-300 rounded-md px-3 py-1.5 bg-white hover:bg-slate-50 flex-shrink-0"
        title="Toggle dark mode"
      >
        {darkMode ? '\u2600\uFE0F' : '\uD83C\uDF19'}
      </button>
    </div>
  );
}
