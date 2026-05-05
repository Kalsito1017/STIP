import { useDelayHeatmap, useReliabilityRanking, usePeakHours } from '../hooks/useDelays';
import { ErrorAlert } from '../components/ErrorAlert';
import { SkeletonChart, SkeletonRankingList } from '../components/Skeleton';
import { motion } from 'motion/react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import { useTranslation } from 'react-i18next';

export function AnalyticsPage() {
  const { t } = useTranslation('analytics');
  const { data: heatmap, isLoading: heatmapLoading, isError: heatmapError, error: heatmapErr, refetch: refetchHeatmap } = useDelayHeatmap();
  const { data: bestRanking, isLoading: bestLoading, isError: bestError, error: bestErr, refetch: refetchBest } = useReliabilityRanking(5, true);
  const { data: worstRanking, isLoading: worstLoading, isError: worstError, error: worstErr, refetch: refetchWorst } = useReliabilityRanking(5, false);
  const { data: peakHours, isLoading: peakLoading, isError: peakError, error: peakErr, refetch: refetchPeak } = usePeakHours();

  const hasAnyError = heatmapError || bestError || worstError || peakError;
  const isLoading = heatmapLoading || bestLoading || worstLoading || peakLoading;

  if (isLoading) return (
    <div className="space-y-4 sm:space-y-6">
      <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
        <SkeletonChart height={250} />
        <div className="space-y-3 sm:space-y-4">
          <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
            <div className="h-4 w-24 bg-muted rounded animate-pulse mb-3" />
            <SkeletonRankingList rows={5} />
          </div>
          <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
            <div className="h-4 w-24 bg-muted rounded animate-pulse mb-3" />
            <SkeletonRankingList rows={5} />
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

  const avgDelay = heatmap?.length
    ? Math.round(heatmap.reduce((s: number, p: { avgDelaySeconds: number }) => s + p.avgDelaySeconds, 0) / heatmap.length)
    : 0;

  return (
    <div className="space-y-4 sm:space-y-6">
      <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>

      {hasAnyError && (
        <ErrorAlert message={t('error_loading')} onRetry={retryAll} />
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
        <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
          <h3 className="text-sm font-semibold text-foreground mb-4">{t('system_peak_hours')}</h3>
          {peakError ? (
            <ErrorAlert message={peakErr.message} onRetry={() => refetchPeak()} />
          ) : peakHours?.length ? (
            <ResponsiveContainer width="100%" height={250}>
              <BarChart data={peakHours}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis dataKey="hourOfDay" tickFormatter={(h) => `${h}:00`} tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                <YAxis unit="s" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                <Tooltip />
                <Bar dataKey="avgDelaySeconds" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-muted-foreground text-sm">{t('no_data', { ns: 'common' })}</p>
          )}
        </div>

        <div className="space-y-3 sm:space-y-4">
          <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
            <h3 className="text-sm font-semibold text-green-600 mb-3">{t('best_routes')}</h3>
            {bestError ? (
              <ErrorAlert message={bestErr.message} onRetry={() => refetchBest()} />
            ) : bestRanking?.length ? (
              <div className="space-y-2">
                {bestRanking.map((r: { routeId: string; shortName: string; score: number }, i: number) => (
                  <motion.div
                    key={r.routeId}
                    initial={{ opacity: 0, x: -6 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: i * 0.05, duration: 0.2 }}
                    className="flex items-center gap-2 text-sm"
                  >
                    <span className="w-5 text-muted-foreground flex-shrink-0">{i + 1}.</span>
                    <span className="font-medium text-foreground truncate flex-1">{r.shortName}</span>
                    <div className="w-20 bg-secondary rounded-full h-1.5 flex-shrink-0">
                      <motion.div
                        className="bg-green-500 h-1.5 rounded-full"
                        initial={{ width: 0 }}
                        animate={{ width: `${Math.min(r.score, 100)}%` }}
                        transition={{ delay: i * 0.05 + 0.3, duration: 0.5, ease: 'easeOut' }}
                      />
                    </div>
                    <span className="text-muted-foreground w-8 text-right flex-shrink-0 text-xs">{Math.round(r.score)}</span>
                  </motion.div>
                ))}
              </div>
            ) : (
              <p className="text-muted-foreground text-sm">{t('no_data', { ns: 'common' })}</p>
            )}
          </div>

          <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
            <h3 className="text-sm font-semibold text-red-500 mb-3">{t('worst_routes')}</h3>
            {worstError ? (
              <ErrorAlert message={worstErr.message} onRetry={() => refetchWorst()} />
            ) : worstRanking?.length ? (
              <div className="space-y-2">
                {worstRanking.map((r: { routeId: string; shortName: string; score: number }, i: number) => (
                  <motion.div
                    key={r.routeId}
                    initial={{ opacity: 0, x: -6 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: i * 0.05, duration: 0.2 }}
                    className="flex items-center gap-2 text-sm"
                  >
                    <span className="w-5 text-muted-foreground flex-shrink-0">{i + 1}.</span>
                    <span className="font-medium text-foreground truncate flex-1">{r.shortName}</span>
                    <div className="w-20 bg-secondary rounded-full h-1.5 flex-shrink-0">
                      <motion.div
                        className="bg-red-500 h-1.5 rounded-full"
                        initial={{ width: 0 }}
                        animate={{ width: `${Math.min(r.score, 100)}%` }}
                        transition={{ delay: i * 0.05 + 0.3, duration: 0.5, ease: 'easeOut' }}
                      />
                    </div>
                    <span className="text-muted-foreground w-8 text-right flex-shrink-0 text-xs">{Math.round(r.score)}</span>
                  </motion.div>
                ))}
              </div>
            ) : (
              <p className="text-muted-foreground text-sm">{t('no_data', { ns: 'common' })}</p>
            )}
          </div>
        </div>
      </div>

      <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-foreground mb-2 sm:mb-4">{t('delay_heatmap')}</h3>
        {heatmapError ? (
          <ErrorAlert message={heatmapErr.message} onRetry={() => refetchHeatmap()} />
        ) : (
          <p className="text-sm text-muted-foreground">
            {heatmap?.length
              ? t('data_points', { count: heatmap.length, avg: avgDelay })
              : t('no_heatmap_data')}
          </p>
        )}
      </div>
    </div>
  );
}
