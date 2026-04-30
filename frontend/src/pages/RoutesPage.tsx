import { useNavigate } from 'react-router-dom';
import { useRoutes } from '../hooks/useRoutes';

const typeLabels: Record<number, string> = { 0: 'Tram', 1: 'Metro', 3: 'Bus', 11: 'Trolley' };
const typeColors: Record<number, string> = {
  0: 'bg-amber-100 text-amber-800',
  1: 'bg-blue-100 text-blue-800',
  3: 'bg-green-100 text-green-800',
  11: 'bg-purple-100 text-purple-800',
};

export function RoutesPage() {
  const { data: routes, isLoading } = useRoutes();
  const navigate = useNavigate();

  return (
    <div className="space-y-4">
      <h1 className="text-xl sm:text-2xl font-bold text-slate-900">Routes</h1>
      {isLoading ? (
        <p className="text-slate-500">Loading routes...</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2 sm:gap-3">
          {routes?.map((r: { routeId: string; shortName: string; longName: string | null; type: number }) => (
            <button
              key={r.routeId}
              onClick={() => navigate(`/routes/${r.routeId}`)}
              className="bg-white border border-slate-200 rounded-lg p-3 sm:p-4 text-left hover:border-blue-300 hover:shadow-sm transition-all"
            >
              <div className="flex items-center justify-between mb-1 gap-2">
                <span className="font-bold text-base sm:text-lg text-slate-900 truncate">{r.shortName}</span>
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium flex-shrink-0 ${typeColors[r.type] ?? 'bg-slate-100 text-slate-700'}`}>
                  {typeLabels[r.type] ?? 'Unknown'}
                </span>
              </div>
              <p className="text-xs sm:text-sm text-slate-500 line-clamp-2">{r.longName ?? '\u2014'}</p>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
