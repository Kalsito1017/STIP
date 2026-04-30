import { useDelayHeatmap, useReliabilityRanking, usePeakHours } from '../hooks/useDelays';
import { ErrorAlert } from '../components/ErrorAlert';
import { SkeletonChart } from '../components/Skeleton';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';

export function AnalyticsPage() {
  const { data: heatmap, isLoading: heatmapLoading, isError: heatmapError, error: heatmapErr, refetch: refetchHeatmap } = useDelayHeatmap();
  const { data: bestRanking, isLoading: bestLoading, isError: bestError, error: bestErr, refetch: refetchBest } = useReliabilityRanking(5, true);
  const { data: worstRanking, isLoading: worstLoading, isError: worstError, error: worstErr, refetch: refetchWorst } = useReliabilityRanking(5, false);
  const { data: peakHours, isLoading: peakLoading, isError: peakError, error: peakErr, refetch: refetchPeak } = usePeakHours();

  const hasAnyError = heatmapError || bestError || worstError || peakError;
  const isLoading = heatmapLoading || bestLoading || worstLoading || peakLoading;

  if (isLoading) return (
    <div className="space-y-4 sm:space-y-6">
      <h1 className="text-xl sm:text-2xl font-bold text-slate-900">Analytics</h1>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
        <SkeletonChart height={250} />
        <div className="space-y-3 sm:space-y-4">
          <div className="bg-white border border-slate-200 rounded-lg p-4 sm:p-5 shadow-sm space-y-3">
            <div className="h-4 w-24 bg-slate-200 rounded animate-pulse" />
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="h-3 w-full bg-slate-200 rounded animate-pulse" />
            ))}
          </div>
          <div className="bg-white border border-slate-200 rounded-lg p-4 sm:p-5 shadow-sm space-y-3">
            <div className="h-4 w-24 bg-slate-200 rounded animate-pulse" />
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="h-3 w-full bg-slate-200 rounded animate-pulse" />
            ))}
          </div>
        </div>
      </div>
    </div>
  );

  const retryAll = () => {
    if (heatmapError) refetchHeatmap();
    if (bestError) refetchBest();
    if (worstError) refetchWorst();
    if (peakError) refetchPeak();
  };

  return (
    <div className="space-y-4 sm:space-y-6">
      <h1 className="text-xl sm:text-2xl font-bold text-slate-900">Analytics</h1>

      {hasAnyError && (
        <ErrorAlert
          message="Some analytics data could not be loaded"
          onRetry={retryAll}
        />
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
        <div className="bg-white border border-slate-200 rounded-lg p-4 sm:p-5 shadow-sm">
          <h3 className="text-sm font-semibold text-slate-700 mb-4">System-Wide Peak Hours</h3>
          {peakError ? (
            <ErrorAlert message={peakErr.message} onRetry={() => refetchPeak()} />
          ) : peakHours?.length ? (
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
            <p className="text-slate-400 text-sm">No data</p>
          )}
        </div>

        <div className="space-y-3 sm:space-y-4">
          <div className="bg-white border border-slate-200 rounded-lg p-4 sm:p-5 shadow-sm">
            <h3 className="text-sm font-semibold text-green-700 mb-3">Best Routes</h3>
            {bestError ? (
              <ErrorAlert message={bestErr.message} onRetry={() => refetchBest()} />
            ) : bestRanking?.length ? (
              <div className="space-y-2">
                {bestRanking.map((r: { routeId: string; shortName: string; score: number }, i: number) => (
                  <div key={r.routeId} className="flex items-center gap-2 text-sm">
                    <span className="w-5 text-slate-400 flex-shrink-0">{i + 1}.</span>
                    <span className="font-medium text-slate-800 truncate">{r.shortName}</span>
                    <span className="ml-auto text-slate-500 flex-shrink-0">{Math.round(r.score)}</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-slate-400 text-sm">No data</p>
            )}
          </div>

          <div className="bg-white border border-slate-200 rounded-lg p-4 sm:p-5 shadow-sm">
            <h3 className="text-sm font-semibold text-red-700 mb-3">Worst Routes</h3>
            {worstError ? (
              <ErrorAlert message={worstErr.message} onRetry={() => refetchWorst()} />
            ) : worstRanking?.length ? (
              <div className="space-y-2">
                {worstRanking.map((r: { routeId: string; shortName: string; score: number }, i: number) => (
                  <div key={r.routeId} className="flex items-center gap-2 text-sm">
                    <span className="w-5 text-slate-400 flex-shrink-0">{i + 1}.</span>
                    <span className="font-medium text-slate-800 truncate">{r.shortName}</span>
                    <span className="ml-auto text-slate-500 flex-shrink-0">{Math.round(r.score)}</span>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-slate-400 text-sm">No data</p>
            )}
          </div>
        </div>
      </div>

      <div className="bg-white border border-slate-200 rounded-lg p-4 sm:p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-slate-700 mb-2 sm:mb-4">Delay Heatmap Data</h3>
        {heatmapError ? (
          <ErrorAlert message={heatmapErr.message} onRetry={() => refetchHeatmap()} />
        ) : (
          <p className="text-sm text-slate-400">
            {heatmap?.length
              ? `${heatmap.length} data points with avg delay ${Math.round(heatmap.reduce((s: number, p: { avgDelaySeconds: number }) => s + p.avgDelaySeconds, 0) / heatmap.length)}s`
              : 'No heatmap data available'}
          </p>
        )}
      </div>
    </div>
  );
}
