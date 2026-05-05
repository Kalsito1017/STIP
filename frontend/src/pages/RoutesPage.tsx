import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRoutes } from '../hooks/useRoutes';
import { ErrorAlert } from '../components/ErrorAlert';
import { SkeletonCard } from '../components/Skeleton';
import { RouteBadge } from '../components/RouteBadge';
import { Input } from '../components/ui/input';
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
      <h1 className="text-xl sm:text-2xl font-bold text-slate-900">{t('title')}</h1>
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
      <h1 className="text-xl sm:text-2xl font-bold text-slate-900">{t('title')}</h1>

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
          className="text-sm border border-slate-300 rounded-md px-3 py-1.5 bg-white"
          aria-label={t('filter_type')}
        >
          <option value="">{t('all_types')}</option>
          <option value="0">{tTransit('tram')}</option>
          <option value="1">{tTransit('metro')}</option>
          <option value="3">{tTransit('bus')}</option>
          <option value="11">{tTransit('trolley')}</option>
        </select>
      </div>

      <p className="text-xs text-slate-500">{t('routes_found', { count: filtered.length })}</p>

      {filtered.length === 0 ? (
        <p className="text-slate-500">{t('no_match')}</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2 sm:gap-3">
          {filtered.map((r) => (
            <button
              key={r.routeId}
              onClick={() => navigate(`/routes/${r.routeId}`)}
              className="bg-white border border-slate-200 rounded-lg p-3 sm:p-4 text-left hover:border-blue-300 hover:shadow-sm transition-all"
            >
              <div className="flex items-center justify-between mb-1 gap-2">
                <span className="font-bold text-base sm:text-lg text-slate-900 truncate">{r.shortName}</span>
                <RouteBadge type={r.type} />
              </div>
              <p className="text-xs sm:text-sm text-slate-500 line-clamp-2">{r.longName ?? '\u2014'}</p>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
