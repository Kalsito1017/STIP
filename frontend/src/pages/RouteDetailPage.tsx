import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Clock } from 'lucide-react';
import { useRouteDetail } from '../hooks/useRoutes';
import { useRouteDelayPattern } from '../hooks/useDelays';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';

const typeLabels: Record<number, string> = { 0: 'Tram', 1: 'Metro', 3: 'Bus', 11: 'Trolley' };

export function RouteDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: route, isLoading } = useRouteDetail(id!);
  const { data: delayPattern } = useRouteDelayPattern(id!);

  if (isLoading) return <p className="text-slate-500">Loading...</p>;
  if (!route) return <p className="text-slate-500">Route not found</p>;

  const score = route.latestReliability?.score ?? null;
  const scoreColor = score === null ? 'text-slate-400' : score >= 70 ? 'text-green-600' : score >= 40 ? 'text-amber-600' : 'text-red-600';

  return (
    <div className="space-y-6">
      <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-sm text-slate-500 hover:text-slate-800">
        <ArrowLeft className="w-4 h-4" /> Back
      </button>

      <div className="bg-white border border-slate-200 rounded-lg p-6 shadow-sm">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h1 className="text-2xl font-bold text-slate-900">{route.shortName}</h1>
            <p className="text-sm text-slate-500">{route.longName ?? typeLabels[route.type] ?? 'Route'}</p>
          </div>
          {score !== null && (
            <div className="text-center">
              <div className={`text-3xl font-bold ${scoreColor}`}>{Math.round(score)}</div>
              <div className="text-xs text-slate-400">Reliability Score</div>
            </div>
          )}
        </div>

        {route.latestReliability && (
          <div className="grid grid-cols-3 gap-4 mt-4 text-sm">
            <div className="bg-slate-50 rounded-md p-3 text-center">
              <div className="font-bold text-slate-900">{(route.latestReliability.onTimePct * 100).toFixed(1)}%</div>
              <div className="text-slate-500">On-Time</div>
            </div>
            <div className="bg-slate-50 rounded-md p-3 text-center">
              <div className="font-bold text-slate-900">{Math.round(route.latestReliability.avgDelaySeconds)}s</div>
              <div className="text-slate-500">Avg Delay</div>
            </div>
            <div className="bg-slate-50 rounded-md p-3 text-center">
              <div className="font-bold text-slate-900">{route.latestReliability.sampleCount}</div>
              <div className="text-slate-500">Samples</div>
            </div>
          </div>
        )}
      </div>

      <div className="bg-white border border-slate-200 rounded-lg p-5 shadow-sm">
        <h3 className="flex items-center gap-2 text-sm font-semibold text-slate-700 mb-4">
          <Clock className="w-4 h-4" /> Delay by Hour
        </h3>
        {delayPattern?.length ? (
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={delayPattern}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="hourOfDay" tickFormatter={(h) => `${h}:00`} />
              <YAxis unit="s" />
              <Tooltip />
              <Bar dataKey="avgDelaySeconds" fill="#8b5cf6" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        ) : (
          <p className="text-slate-400 text-sm">No delay pattern data available</p>
        )}
      </div>
    </div>
  );
}
