import { Bus, Clock, TrendingUp, AlertTriangle } from 'lucide-react';
import { StatCard } from '../components/StatCard';
import { AlertBanner } from '../components/AlertBanner';
import { TripUpdatesList } from '../components/TripUpdatesList';
import { useLiveVehicles } from '../hooks/useVehicles';
import { useReliabilityRanking, usePeakHours } from '../hooks/useDelays';
import { useAppStore } from '../store/useAppStore';
import { useMemo } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';

export function DashboardPage() {
  const { data: vehicles } = useLiveVehicles();
  const { data: ranking } = useReliabilityRanking(10, true);
  useReliabilityRanking(5, false);
  const { data: peakHours } = usePeakHours();
  const alerts = useAppStore((s) => s.alerts);
  const avgDelay = useMemo(() => {
    if (!ranking?.length) return 0;
    return Math.round(ranking.reduce((sum: number, r: { avgDelaySeconds: number }) => sum + r.avgDelaySeconds, 0) / ranking.length);
  }, [ranking]);

  return (
    <div className="space-y-4 sm:space-y-6">
      <h1 className="text-xl sm:text-2xl font-bold text-slate-900">Dashboard</h1>

      <div className="grid grid-cols-2 md:grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
        <StatCard
          title="Active Vehicles"
          value={vehicles?.length ?? 0}
          subtitle="Currently tracked"
          icon={Bus}
        />
        <StatCard
          title="Avg Delay"
          value={`${avgDelay}s`}
          subtitle="Across all routes"
          icon={Clock}
          trend={avgDelay < 120 ? 'up' : 'down'}
        />
        <StatCard
          title="Best Route"
          value={ranking?.[0]?.shortName ?? '\u2014'}
          subtitle={`Score: ${Math.round(ranking?.[0]?.score ?? 0)}`}
          icon={TrendingUp}
        />
        <StatCard
          title="Active Alerts"
          value={alerts.length}
          subtitle={alerts.some(a => a.severity === 3) ? 'Severe active' : 'Monitoring'}
          icon={AlertTriangle}
          trend={alerts.length === 0 ? 'up' : 'down'}
        />
      </div>

      <AlertBanner />

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6">
        <div className="bg-white rounded-lg border border-slate-200 p-4 sm:p-5 shadow-sm">
          <h3 className="text-sm font-semibold text-slate-700 mb-4">Peak Hour Delays</h3>
          {peakHours?.length ? (
            <ResponsiveContainer width="100%" height={250}>
              <BarChart data={peakHours}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="hourOfDay" tickFormatter={(h) => `${h}:00`} />
                <YAxis unit="s" />
                <Tooltip />
                <Bar dataKey="avgDelaySeconds" fill="#3b82f6" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-slate-400 text-sm">No peak hour data available</p>
          )}
        </div>

        <div className="bg-white rounded-lg border border-slate-200 p-4 sm:p-5 shadow-sm">
          <h3 className="text-sm font-semibold text-slate-700 mb-4">Reliability Ranking (Top 10)</h3>
          {ranking?.length ? (
            <div className="space-y-2 max-h-[250px] overflow-y-auto">
              {ranking.slice(0, 10).map((r: { routeId: string; shortName: string; score: number; onTimePct: number }, i: number) => (
                <div key={r.routeId} className="flex items-center gap-2 sm:gap-3 text-xs sm:text-sm">
                  <span className="w-5 sm:w-6 text-center font-mono text-slate-400 flex-shrink-0">{i + 1}</span>
                  <span className="font-medium text-slate-800 w-14 sm:w-16 truncate">{r.shortName}</span>
                  <div className="flex-1 bg-slate-100 rounded-full h-2">
                    <div
                      className="bg-blue-500 h-2 rounded-full"
                      style={{ width: `${Math.min(Math.max(r.score, 0), 100)}%` }}
                    />
                  </div>
                  <span className="text-slate-500 w-10 sm:w-12 text-right flex-shrink-0">{Math.round(r.score)}</span>
                  <span className="text-slate-400 w-14 sm:w-16 text-right flex-shrink-0">{(r.onTimePct * 100).toFixed(0)}%</span>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-slate-400 text-sm">No ranking data available</p>
          )}
        </div>
      </div>

      <TripUpdatesList />
    </div>
  );
}
