import { Bus, Clock, TrendingUp, AlertTriangle, Loader2 } from 'lucide-react';
import { motion } from 'motion/react';
import { StatCard } from '../components/StatCard';
import { AlertBanner } from '../components/AlertBanner';
import { TripUpdatesList } from '../components/TripUpdatesList';
import { ErrorAlert } from '../components/ErrorAlert';
import { SkeletonCard, SkeletonChart, SkeletonRankingList } from '../components/Skeleton';
import { useLiveVehicles } from '../hooks/useVehicles';
import { useReliabilityRanking, usePeakHours } from '../hooks/useDelays';
import { useAppStore } from '../store/useAppStore';
import { useMemo, useRef, useEffect, useState } from 'react';
import { Card, CardHeader, CardTitle, CardContent } from '../components/ui/card';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import { useTranslation } from 'react-i18next';

export function DashboardPage() {
  const { t } = useTranslation('dashboard');
  const statsRef = useRef<HTMLDivElement>(null);
  const [statsInView, setStatsInView] = useState(false);

  useEffect(() => {
    const el = statsRef.current;
    if (!el || statsInView) return;
    const obs = new IntersectionObserver(
      ([entry]) => { if (entry.isIntersecting) { setStatsInView(true); obs.disconnect(); } },
      { threshold: 0.3 }
    );
    obs.observe(el);
    return () => obs.disconnect();
  }, [statsInView]);
  const { data: vehicles, isLoading: vehiclesLoading, isFetching: vehiclesFetching, isError: vehiclesError, refetch: refetchVehicles } = useLiveVehicles();
  const { data: ranking, isLoading: rankingLoading, isFetching: rankingFetching, isError: rankingError, refetch: refetchRanking } = useReliabilityRanking(10, true);
  const { data: peakHours, isLoading: peakLoading, isFetching: peakFetching, isError: peakError, refetch: refetchPeak } = usePeakHours();
  const alerts = useAppStore((s) => s.alerts);

  const loading = vehiclesLoading || rankingLoading || peakLoading;
  const isBackgroundFetching = !loading && (vehiclesFetching || rankingFetching || peakFetching);
  const hasAnyError = vehiclesError || rankingError || peakError;

  const retryAll = () => {
    if (vehiclesError) refetchVehicles();
    if (rankingError) refetchRanking();
    if (peakError) refetchPeak();
  };

  const avgDelay = useMemo(() => {
    if (!ranking?.length) return 0;
    return Math.round(ranking.reduce((sum: number, r: { avgDelaySeconds: number }) => sum + r.avgDelaySeconds, 0) / ranking.length);
  }, [ranking]);

  const hasSevere = alerts.some(a => a.severity === 3);

  return (
    <div className="space-y-4 sm:space-y-6">
      <div className="flex items-center gap-3">
        <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>
        {isBackgroundFetching && (
          <span className="inline-flex items-center gap-1 text-xs text-muted-foreground">
            <Loader2 className="w-3 h-3 animate-spin" />
            Updating
          </span>
        )}
      </div>

      {loading ? (
        <>
          <div className="grid grid-cols-2 md:grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
            <SkeletonCard /><SkeletonCard /><SkeletonCard /><SkeletonCard />
          </div>
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6">
            <SkeletonChart height={250} />
            <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
              <div className="h-3 w-32 bg-muted rounded animate-pulse mb-4" />
              <SkeletonRankingList rows={10} />
            </div>
          </div>
        </>
      ) : hasAnyError ? (
        <ErrorAlert message={t('error_loading')} onRetry={retryAll} />
      ) : (
        <>
          <div ref={statsRef} className="grid grid-cols-2 md:grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
            <StatCard title={t('active_vehicles')} value={vehicles?.length ?? 0} subtitle={t('currently_tracked')} icon={Bus} iconColor="#22c55e" animate={statsInView} />
            <StatCard title={t('avg_delay')} value={`${avgDelay}s`} subtitle={t('across_all_routes')} icon={Clock} iconColor={avgDelay < 120 ? '#22c55e' : '#f59e0b'} trend={avgDelay < 120 ? 'up' : 'down'} />
            <StatCard title={t('best_route')} value={ranking?.[0]?.shortName ?? '\u2014'} subtitle={`Score: ${Math.round(ranking?.[0]?.score ?? 0)}`} icon={TrendingUp} iconColor="#3b82f6" />
            <StatCard title={t('active_alerts')} value={alerts.length} subtitle={hasSevere ? t('severe_active') : t('monitoring')} icon={AlertTriangle} iconColor={alerts.length > 0 ? '#ef4444' : '#22c55e'} trend={alerts.length === 0 ? 'up' : 'down'} animate={statsInView} />
          </div>

          <AlertBanner />

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6">
            <Card className="p-4 sm:p-5">
              <CardHeader className="p-0 mb-4">
                <CardTitle>{t('peak_hour_delays')}</CardTitle>
              </CardHeader>
              <CardContent className="p-0">
              {peakHours?.length ? (
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
                <p className="text-muted-foreground text-sm">{t('no_peak_hour_data')}</p>
              )}
              </CardContent>
            </Card>

            <Card className="p-4 sm:p-5">
              <CardHeader className="p-0 mb-4">
                <CardTitle>{t('reliability_ranking')}</CardTitle>
              </CardHeader>
              <CardContent className="p-0">
              {ranking?.length ? (
                <div className="space-y-2 max-h-[250px] overflow-y-auto">
                  {ranking.slice(0, 10).map((r: { routeId: string; shortName: string; score: number; onTimePct: number }, i: number) => (
                    <motion.div
                      key={r.routeId}
                      initial={{ opacity: 0, x: -8 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: i * 0.04, duration: 0.2 }}
                      className="flex items-center gap-2 sm:gap-3 text-xs sm:text-sm"
                    >
                      <span className="w-5 sm:w-6 text-center font-mono text-muted-foreground flex-shrink-0">{i + 1}</span>
                      <span className="font-medium text-foreground w-14 sm:w-16 truncate">{r.shortName}</span>
                      <div className="flex-1 bg-secondary rounded-full h-2">
                        <motion.div
                          className="bg-primary h-2 rounded-full"
                          initial={{ width: 0 }}
                          animate={{ width: `${Math.min(Math.max(r.score, 0), 100)}%` }}
                          transition={{ delay: i * 0.04 + 0.3, duration: 0.6, ease: 'easeOut' }}
                        />
                      </div>
                      <span className="text-muted-foreground w-10 sm:w-12 text-right flex-shrink-0">{Math.round(r.score)}</span>
                      <span className="text-muted-foreground/70 w-14 sm:w-16 text-right flex-shrink-0">{(r.onTimePct * 100).toFixed(0)}%</span>
                    </motion.div>
                  ))}
                </div>
              ) : (
                <p className="text-muted-foreground text-sm">{t('no_ranking_data')}</p>
              )}
              </CardContent>
            </Card>
          </div>

          <TripUpdatesList />
        </>
      )}
    </div>
  );
}
