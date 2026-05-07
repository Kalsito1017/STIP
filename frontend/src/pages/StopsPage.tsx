import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useStops } from '../hooks/useStops';
import { SkeletonCard, SkeletonTable } from '../components/Skeleton';
import { MapPin, ArrowUpDown, ArrowUp, ArrowDown, Map } from 'lucide-react';
import { FavoriteButton } from '../components/FavoriteButton';
import { useAppStore } from '../store/useAppStore';
import { motion } from 'motion/react';
import { EmptyState } from '../components/EmptyState';
import { useTranslation } from 'react-i18next';

interface Stop {
  stopId: string;
  stopName: string;
  lat: number;
  lon: number;
  routeCount?: number;
}

type SortKey = 'name' | 'id';
type SortDir = 'asc' | 'desc';

function SortIcon({ active, dir }: { active: boolean; dir: SortDir }) {
  if (!active) return <ArrowUpDown className="w-3.5 h-3.5 text-muted-foreground" />;
  return dir === 'asc'
    ? <ArrowUp className="w-3.5 h-3.5 text-primary" />
    : <ArrowDown className="w-3.5 h-3.5 text-primary" />;
}

export function StopsPage() {
  const { t } = useTranslation('stops');
  const { data: stops, isLoading } = useStops();
  const navigate = useNavigate();
  const setFlyToTarget = useAppStore((s) => s.setFlyToTarget);
  const [search, setSearch] = useState('');
  const [sortKey, setSortKey] = useState<SortKey>('name');
  const [sortDir, setSortDir] = useState<SortDir>('asc');

  const handleSort = (key: SortKey) => {
    if (sortKey === key) {
      setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortKey(key);
      setSortDir('asc');
    }
  };

  const filtered = useMemo(() => {
    if (!stops) return [];
    let result = stops as Stop[];

    const q = search.toLowerCase().trim();
    if (q) {
      result = result.filter((s) => s.stopName.toLowerCase().includes(q));
    }

    return result.sort((a, b) => {
      const aVal = sortKey === 'name' ? a.stopName : a.stopId;
      const bVal = sortKey === 'name' ? b.stopName : b.stopId;
      const cmp = aVal.localeCompare(bVal);
      return sortDir === 'asc' ? cmp : -cmp;
    });
  }, [stops, search, sortKey, sortDir]);

  if (isLoading) return (
    <div className="space-y-4">
      <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>
      <div className="sm:hidden space-y-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <SkeletonCard key={i} />
        ))}
      </div>
      <div className="hidden sm:block">
        <SkeletonTable rows={5} cols={3} />
      </div>
    </div>
  );

  return (
    <div className="space-y-4">
      <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>

      <div className="flex items-center gap-3">
        <input
          type="text"
          placeholder={t('search')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="flex-1 text-sm border border-input rounded-md px-3 py-1.5 bg-card text-foreground placeholder:text-muted-foreground"
          aria-label={t('search_aria')}
        />
      </div>

      <p className="text-xs text-muted-foreground">{t('stops_found', { count: filtered.length })}</p>

      {filtered.length === 0 ? (
        <EmptyState icon={MapPin} title={t('no_match')} />
      ) : (
        <>
          <div className="sm:hidden space-y-2">
            {filtered.map((s, i) => (
              <motion.button
                key={s.stopId}
                onClick={() => navigate(`/stops/${s.stopId}`)}
                initial={{ opacity: 0, y: 6 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: i * 0.01, duration: 0.15 }}
                className="w-full bg-card border border-border rounded-lg p-3 text-left hover:shadow-md hover:-translate-y-0.5 transition-all"
              >
                <div className="flex items-center gap-2 mb-1">
                  <MapPin className="w-4 h-4 text-primary flex-shrink-0" />
                  <span className="font-medium text-foreground truncate">{s.stopName}</span>
                </div>
                <div className="flex items-center justify-between text-xs text-muted-foreground">
                  <span className="font-mono">{s.stopId}</span>
                  <div className="flex items-center gap-2">
                    {s.routeCount != null && (
                      <span className="text-xs">{t('route_count', { count: s.routeCount })}</span>
                    )}
                    <span className="font-mono">{s.lat?.toFixed(4) ?? '\u2014'}, {s.lon?.toFixed(4) ?? '\u2014'}</span>
                  </div>
                </div>
              </motion.button>
            ))}
          </div>

          <div className="hidden sm:block bg-card border border-border rounded-lg shadow-sm overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border bg-muted/50">
                    <th
                      className="text-left p-3 font-medium text-muted-foreground cursor-pointer hover:text-foreground select-none"
                      onClick={() => handleSort('name')}
                      aria-sort={sortKey === 'name' ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
                    >
                      <div className="flex items-center gap-1">
                        {t('name')}
                        <SortIcon active={sortKey === 'name'} dir={sortDir} />
                      </div>
                    </th>
                    <th
                      className="text-left p-3 font-medium text-muted-foreground cursor-pointer hover:text-foreground select-none"
                      onClick={() => handleSort('id')}
                      aria-sort={sortKey === 'id' ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
                    >
                      <div className="flex items-center gap-1">
                        {t('id')}
                        <SortIcon active={sortKey === 'id'} dir={sortDir} />
                      </div>
                    </th>
                    <th className="text-left p-3 font-medium text-muted-foreground">{t('coordinates')}</th>
                    <th className="p-3 w-10" />
                  </tr>
                </thead>
                <tbody>
                  {filtered.map((s) => (
                    <tr
                      key={s.stopId}
                      tabIndex={0}
                      role="button"
                      aria-label={t('view_details', { name: s.stopName })}
                      onClick={() => navigate(`/stops/${s.stopId}`)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' || e.key === ' ') {
                          e.preventDefault();
                          navigate(`/stops/${s.stopId}`);
                        }
                      }}
                      className="border-b border-border hover:bg-accent cursor-pointer transition-colors focus:outline-none focus:ring-2 focus:ring-inset focus:ring-ring"
                    >
                      <td className="p-3">
                        <div className="flex items-center gap-2">
                          <MapPin className="w-4 h-4 text-primary flex-shrink-0" />
                          <span className="font-medium text-foreground">{s.stopName}</span>
                          <FavoriteButton entityType="stop" entityId={s.stopId} size="sm" />
                        </div>
                      </td>
                      <td className="p-3 text-muted-foreground font-mono text-xs">{s.stopId}</td>
                      <td className="p-3 text-muted-foreground font-mono text-xs">
                        {s.lat?.toFixed(4) ?? '\u2014'}, {s.lon?.toFixed(4) ?? '\u2014'}
                      </td>
                      <td className="p-3">
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            setFlyToTarget({ lat: s.lat, lon: s.lon, zoom: 17 });
                            navigate('/');
                          }}
                          className="p-1.5 rounded-md hover:bg-secondary text-muted-foreground hover:text-foreground transition-colors"
                          aria-label={`${t('view_on_map_aria')} ${s.stopName}`}
                          title={t('view_on_map_aria')}
                        >
                          <Map className="w-4 h-4" />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
