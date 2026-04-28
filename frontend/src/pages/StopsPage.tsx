import { useNavigate } from 'react-router-dom';
import { useStops } from '../hooks/useStops';
import { MapPin } from 'lucide-react';

export function StopsPage() {
  const { data: stops, isLoading } = useStops();
  const navigate = useNavigate();

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-slate-900">Stops</h1>
      {isLoading ? (
        <p className="text-slate-500">Loading stops...</p>
      ) : (
        <div className="bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-200 bg-slate-50">
                <th className="text-left p-3 font-medium text-slate-600">Name</th>
                <th className="text-left p-3 font-medium text-slate-600">ID</th>
                <th className="text-right p-3 font-medium text-slate-600">Coordinates</th>
              </tr>
            </thead>
            <tbody>
              {stops?.map((s: { stopId: string; stopName: string; lat: number; lon: number }) => (
                <tr
                  key={s.stopId}
                  onClick={() => navigate(`/stops/${s.stopId}`)}
                  className="border-b border-slate-100 hover:bg-blue-50 cursor-pointer transition-colors"
                >
                  <td className="p-3">
                    <div className="flex items-center gap-2">
                      <MapPin className="w-4 h-4 text-slate-400" />
                      <span className="font-medium text-slate-800">{s.stopName}</span>
                    </div>
                  </td>
                  <td className="p-3 text-slate-500 font-mono text-xs">{s.stopId}</td>
                  <td className="p-3 text-right text-slate-500 font-mono text-xs">
                    {s.lat.toFixed(4)}, {s.lon.toFixed(4)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
