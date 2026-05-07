import { useState, useMemo, useRef, useEffect } from 'react';
import { Search, Bus, MapPin, Loader2 } from 'lucide-react';
import { useRoutes } from '../hooks/useRoutes';
import { useStops } from '../hooks/useStops';
import { useAppStore } from '../store/useAppStore';
import { Input } from './ui/input';
import { useTranslation } from 'react-i18next';

interface SearchResult {
  type: 'route' | 'stop';
  id: string;
  label: string;
  secondary?: string;
}

export function SearchBar() {
  const { t } = useTranslation('map');
  const [query, setQuery] = useState('');
  const [open, setOpen] = useState(false);
  const { data: routes, isLoading: routesLoading } = useRoutes();
  const { data: stops, isLoading: stopsLoading } = useStops();
  const setFlyToTarget = useAppStore((s) => s.setFlyToTarget);
  const setRouteFilter = useAppStore((s) => s.setRouteFilter);
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const results = useMemo((): SearchResult[] => {
    if (!query || query.length < 1) return [];

    const q = query.toLowerCase();

    const routeResults: SearchResult[] = (routes ?? [])
      .filter((r: { shortName: string }) => r.shortName.toLowerCase().includes(q))
      .slice(0, 5)
      .map((r: { routeId: string; shortName: string }) => ({
        type: 'route' as const,
        id: r.routeId,
        label: r.shortName,
      }));

    const stopResults: SearchResult[] = (stops ?? [])
      .filter((s: { stopName: string }) => s.stopName.toLowerCase().includes(q))
      .slice(0, 5)
      .map((s: { stopId: string; stopName: string }) => ({
        type: 'stop' as const,
        id: s.stopId,
        label: s.stopName,
      }));

    return [...routeResults, ...stopResults].slice(0, 8);
  }, [query, routes, stops]);

  const handleSelect = (r: SearchResult) => {
    setOpen(false);
    setQuery(r.label);

    if (r.type === 'route') {
      setRouteFilter(r.id);
      // Don't fly — just filter vehicles
    } else if (r.type === 'stop') {
      const stop = stops?.find((s: { stopId: string }) => s.stopId === r.id);
      if (stop && 'lat' in stop && 'lon' in stop) {
        setFlyToTarget({ lat: stop.lat as number, lon: stop.lon as number, zoom: 17 });
      }
    }
  };

  return (
    <div className="relative" ref={containerRef}>
      <div className="relative">
        <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground z-10" />
        <Input
          ref={inputRef}
          type="text"
          placeholder={t('search_placeholder')}
          value={query}
          onChange={(e) => { setQuery(e.target.value); setOpen(true); }}
          onFocus={() => { if (results.length > 0) setOpen(true); }}
          className="pl-10 pr-4 h-11 bg-card/80 backdrop-blur-md shadow-md border-border/60 rounded-full w-full focus:ring-2 focus:ring-primary/30 transition-shadow"
        />
      </div>
      {open && query.length >= 1 && (
        <div className="absolute top-full left-0 right-0 mt-1.5 bg-card/95 backdrop-blur-md border border-border/60 rounded-xl shadow-xl overflow-hidden z-50">
          {(routesLoading || stopsLoading) ? (
            <div className="flex items-center gap-2 px-4 py-3 text-sm text-muted-foreground">
              <Loader2 className="w-4 h-4 animate-spin" />
              {t('loading_routes_stops')}
            </div>
          ) : results.length > 0 ? (
            results.map((r) => (
              <button
                key={`${r.type}-${r.id}`}
                onClick={() => handleSelect(r)}
                className="flex items-center gap-3 w-full px-4 py-2.5 text-sm text-left hover:bg-accent transition-colors"
              >
                {r.type === 'route' ? (
                  <Bus className="w-4 h-4 text-primary flex-shrink-0" />
                ) : (
                  <MapPin className="w-4 h-4 text-destructive flex-shrink-0" />
                )}
                <div className="min-w-0">
                  <p className="text-foreground truncate">{r.label}</p>
                  {r.secondary && (
                    <p className="text-xs text-muted-foreground truncate">{r.secondary}</p>
                  )}
                </div>
              </button>
            ))
          ) : (
            <p className="px-4 py-3 text-sm text-muted-foreground">{t('no_results')}</p>
          )}
        </div>
      )}
    </div>
  );
}
