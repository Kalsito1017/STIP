import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Bus } from 'lucide-react';
import { motion } from 'motion/react';
import { useRoutes } from '../hooks/useRoutes';
import { ErrorAlert } from '../components/ErrorAlert';
import { SkeletonCard } from '../components/Skeleton';
import { RouteBadge } from '../components/RouteBadge';
import { Input } from '../components/ui/input';
import { EmptyState } from '../components/EmptyState';
import { TransitTypeRouteColor } from '../constants/transit';
import { useTranslation } from 'react-i18next';

interface Route {
  routeId: string;
  shortName: string;
  longName: string | null;
  type: number;
}

export function RoutesPage() {
  const { t } = useTranslation('routes');
  const { t: tTransit } = useTranslation('transit');
  const { data: routes, isLoading, isError, error, refetch } = useRoutes();
  const navigate = useNavigate();
  const [search, setSearch] = useState('');
  const [typeFilter, setTypeFilter] = useState<number | null>(null);

  const filtered = useMemo(() => {
    if (!routes) return [];
    let result = routes as Route[];

    if (typeFilter !== null) {
      result = result.filter((r) => r.type === typeFilter);
    }

    const q = search.toLowerCase().trim();
    if (q) {
      result = result.filter(
        (r) =>
          r.shortName.toLowerCase().includes(q) ||
          (r.longName ?? '').toLowerCase().includes(q)
      );
    }

    return result.sort((a, b) => a.shortName.localeCompare(b.shortName));
  }, [routes, search, typeFilter]);

  if (isLoading) return (
    <div className="space-y-4">
      <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2 sm:gap-3">
        {Array.from({ length: 9 }).map((_, i) => (
          <SkeletonCard key={i} />
        ))}
      </div>
    </div>
  );

  if (isError) return <ErrorAlert message={error.message} onRetry={() => refetch()} />;

  return (
    <div className="space-y-4">
      <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>

      <div className="flex flex-col sm:flex-row gap-2 sm:gap-3">
        <Input
          type="text"
          placeholder={t('search')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          aria-label={t('search_aria')}
          className="flex-1"
        />
        <select
          value={typeFilter ?? ''}
          onChange={(e) => setTypeFilter(e.target.value ? Number(e.target.value) : null)}
          className="text-sm border border-input rounded-md px-3 py-1.5 bg-card text-foreground"
          aria-label={t('filter_type')}
        >
          <option value="">{t('all_types')}</option>
          <option value="0">{tTransit('tram')}</option>
          <option value="1">{tTransit('metro')}</option>
          <option value="3">{tTransit('bus')}</option>
          <option value="11">{tTransit('trolley')}</option>
        </select>
      </div>

      <p className="text-xs text-muted-foreground">{t('routes_found', { count: filtered.length })}</p>

      {filtered.length === 0 ? (
        <EmptyState icon={Bus} title={t('no_match')} />
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2 sm:gap-3">
          {filtered.map((r, i) => {
            const accentColor = TransitTypeRouteColor[r.type] ?? '#64748b';
            return (
              <motion.button
                key={r.routeId}
                onClick={() => navigate(`/routes/${r.routeId}`)}
                initial={{ opacity: 0, y: 8 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: i * 0.02, duration: 0.2 }}
                className="bg-card border border-border rounded-lg p-3 sm:p-4 text-left hover:shadow-md hover:-translate-y-0.5 transition-all"
                style={{ borderLeftWidth: '4px', borderLeftColor: accentColor }}
              >
                <div className="flex items-center justify-between mb-1 gap-2">
                  <span className="font-bold text-base sm:text-lg text-foreground truncate">{r.shortName}</span>
                  <RouteBadge type={r.type} />
                </div>
                <p className="text-xs sm:text-sm text-muted-foreground line-clamp-2">{r.longName ?? '\u2014'}</p>
              </motion.button>
            );
          })}
        </div>
      )}
    </div>
  );
}
