import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useStops } from '../hooks/useStops';
import { SkeletonCard, SkeletonTable } from '../components/Skeleton';
import { MapPin, ArrowUpDown, ArrowUp, ArrowDown } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface Stop {
  stopId: string;
  stopName: string;
  lat: number;
  lon: number;
}

type SortKey = 'name' | 'id';
type SortDir = 'asc' | 'desc';

function SortIcon({ active, dir }: { active: boolean; dir: SortDir }) {
  if (!active) return <ArrowUpDown className="w-3.5 h-3.5 text-slate-400" />;
  return dir === 'asc'
    ? <ArrowUp className="w-3.5 h-3.5 text-blue-600" />
    : <ArrowDown className="w-3.5 h-3.5 text-blue-600" />;
}

export function StopsPage() {
  const { t } = useTranslation('stops');
  const { data: stops, isLoading } = useStops();
  const navigate = useNavigate();
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
      <h1 className="text-xl sm:text-2xl font-bold text-slate-900">{t('title')}</h1>
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

  const handleRowKeyDown = (e: React.KeyboardEvent, stopId: string) => {
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      navigate(`/stops/${stopId}`);
    }
  };

  return (
    <div className="space-y-4">
      <h1 className="text-xl sm:text-2xl font-bold text-slate-900">{t('title')}</h1>

      <div className="flex items-center gap-3">
        <input
          type="text"
          placeholder={t('search')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="flex-1 text-sm border border-slate-300 rounded-md px-3 py-1.5 bg-white"
          aria-label={t('search_aria')}
        />
      </div>

      <p className="text-xs text-slate-500">{t('stops_found', { count: filtered.length })}</p>

      {filtered.length === 0 ? (
        <p className="text-slate-500">{t('no_match')}</p>
      ) : (
        <>
          <div className="sm:hidden space-y-2">
            {filtered.map((s) => (
              <button
                key={s.stopId}
                onClick={() => navigate(`/stops/${s.stopId}`)}
                className="w-full bg-white border border-slate-200 rounded-lg p-3 text-left hover:border-blue-300 hover:shadow-sm transition-all"
              >
                <div className="flex items-center gap-2 mb-1">
                  <MapPin className="w-4 h-4 text-slate-400 flex-shrink-0" />
                  <span className="font-medium text-slate-800 truncate">{s.stopName}</span>
                </div>
                <div className="flex items-center justify-between text-xs text-slate-500">
                  <span className="font-mono">{s.stopId}</span>
                  <span className="font-mono">{s.lat?.toFixed(4) ?? '\u2014'}, {s.lon?.toFixed(4) ?? '\u2014'}</span>
                </div>
              </button>
            ))}
          </div>

          <div className="hidden sm:block bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-200 bg-slate-50">
                    <th
                      className="text-left p-3 font-medium text-slate-600 cursor-pointer hover:text-slate-900 select-none"
                      onClick={() => handleSort('name')}
                      aria-sort={sortKey === 'name' ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
                    >
                      <div className="flex items-center gap-1">
                        {t('name')}
                        <SortIcon active={sortKey === 'name'} dir={sortDir} />
                      </div>
                    </th>
                    <th
                      className="text-left p-3 font-medium text-slate-600 cursor-pointer hover:text-slate-900 select-none"
                      onClick={() => handleSort('id')}
                      aria-sort={sortKey === 'id' ? (sortDir === 'asc' ? 'ascending' : 'descending') : 'none'}
                    >
                      <div className="flex items-center gap-1">
                        {t('id')}
                        <SortIcon active={sortKey === 'id'} dir={sortDir} />
                      </div>
                    </th>
                    <th className="text-right p-3 font-medium text-slate-600">{t('coordinates')}</th>
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
                      onKeyDown={(e) => handleRowKeyDown(e, s.stopId)}
                      className="border-b border-slate-100 hover:bg-blue-50 cursor-pointer transition-colors focus:outline-none focus:ring-2 focus:ring-inset focus:ring-blue-400"
                    >
                      <td className="p-3">
                        <div className="flex items-center gap-2">
                          <MapPin className="w-4 h-4 text-slate-400 flex-shrink-0" />
                          <span className="font-medium text-slate-800">{s.stopName}</span>
                        </div>
                      </td>
                      <td className="p-3 text-slate-500 font-mono text-xs">{s.stopId}</td>
                      <td className="p-3 text-right text-slate-500 font-mono text-xs">
                        {s.lat?.toFixed(4) ?? '\u2014'}, {s.lon?.toFixed(4) ?? '\u2014'}
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
