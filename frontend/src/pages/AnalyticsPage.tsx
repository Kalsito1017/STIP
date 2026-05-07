import { useState, useMemo } from 'react';
import { BarChart3, Activity, MapPin, Zap } from 'lucide-react';
import { motion } from 'motion/react';
import {
  useDelayHeatmap, useReliabilityRanking, usePeakHours,
  useSystemOverview, useStopCongestionAll,
} from '../hooks/useDelays';
import { useLiveVehicles } from '../hooks/useVehicles';
import { ErrorAlert } from '../components/ErrorAlert';
import { SkeletonChart, SkeletonRankingList } from '../components/Skeleton';
import { Card, CardHeader, CardTitle, CardContent } from '../components/ui/card';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  AreaChart, Area,
} from 'recharts';
import { useTranslation } from 'react-i18next';

type Tab = 'overview' | 'delays' | 'congestion' | 'live';

export function AnalyticsPage() {
  const { t } = useTranslation('analytics');
  const [activeTab, setActiveTab] = useState<Tab>('overview');

  const tabs = [
    { id: 'overview' as Tab, label: t('tab_overview'), icon: BarChart3 },
    { id: 'delays' as Tab, label: t('tab_delays'), icon: Activity },
    { id: 'congestion' as Tab, label: t('tab_congestion'), icon: MapPin },
    { id: 'live' as Tab, label: t('tab_live'), icon: Zap },
  ];

  return (
    <div className="space-y-4 sm:space-y-6">
      <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>

      <div className="flex gap-1 overflow-x-auto border-b border-border pb-px">
        {tabs.map(({ id, label, icon: Icon }) => (
          <button
            key={id}
            onClick={() => setActiveTab(id)}
            className={`flex items-center gap-2 px-4 py-2.5 text-sm font-medium whitespace-nowrap transition-colors ${
              activeTab === id
                ? 'text-primary border-b-2 border-primary -mb-px'
                : 'text-muted-foreground hover:text-foreground'
            }`}
          >
            <Icon className="w-4 h-4" />
            {label}
          </button>
        ))}
      </div>

      {activeTab === 'overview' && <OverviewTab />}
      {activeTab === 'delays' && <DelaysTab />}
      {activeTab === 'congestion' && <CongestionTab />}
      {activeTab === 'live' && <LiveTab />}
    </div>
  );
}

function OverviewTab() {
  const { t } = useTranslation('analytics');
  const { data: peakHours, isLoading: peakLoading, isError: peakError, error: peakErr, refetch: refetchPeak } = usePeakHours();
  const { data: bestRanking, isLoading: bestLoading, isError: bestError, error: bestErr, refetch: refetchBest } = useReliabilityRanking(5, true);
  const { data: worstRanking, isLoading: worstLoading, isError: worstError, error: worstErr, refetch: refetchWorst } = useReliabilityRanking(5, false);
  const { data: heatmap } = useDelayHeatmap();

  const isLoading = peakLoading || bestLoading || worstLoading;
  const hasAnyError = peakError || bestError || worstError;

  const avgDelay = useMemo(() => {
    if (!heatmap?.length) return 0;
    return Math.round(heatmap.reduce((s: number, p: { avgDelaySeconds: number }) => s + p.avgDelaySeconds, 0) / heatmap.length);
  }, [heatmap]);

  if (isLoading) return (
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
  );

  const retryAll = () => {
    if (peakError) refetchPeak();
    if (bestError) refetchBest();
    if (worstError) refetchWorst();
  };

  return (
    <div className="space-y-4 sm:space-y-6">
      {hasAnyError && <ErrorAlert message={t('error_loading')} onRetry={retryAll} />}

      {avgDelay > 0 && (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
          <StatBox label={t('stat_avg_delay')} value={`${avgDelay}s`} />
          <StatBox label={t('stat_data_points')} value={String(heatmap?.length ?? 0)} />
          <StatBox label={t('stat_best_route')} value={bestRanking?.[0]?.shortName ?? '—'} />
          <StatBox label={t('stat_worst_route')} value={worstRanking?.[0]?.shortName ?? '—'} />
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
        <Card className="p-4 sm:p-5">
          <CardHeader className="p-0 mb-4">
            <CardTitle>{t('system_peak_hours')}</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
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
          </CardContent>
        </Card>

        <div className="space-y-3 sm:space-y-4">
          <RankingCard
            title={t('best_routes')}
            titleColor="text-green-600 dark:text-green-400"
            barColor="bg-green-500"
            data={bestRanking}
            isError={bestError}
            error={bestErr}
            onRetry={() => refetchBest()}
          />
          <RankingCard
            title={t('worst_routes')}
            titleColor="text-red-500 dark:text-red-400"
            barColor="bg-red-500"
            data={worstRanking}
            isError={worstError}
            error={worstErr}
            onRetry={() => refetchWorst()}
          />
        </div>
      </div>
    </div>
  );
}

function DelaysTab() {
  const { t } = useTranslation('analytics');
  const { data: heatmap, isLoading, isError, error, refetch } = useDelayHeatmap();
  const { data: peakHours } = usePeakHours();

  const delayDistribution = useMemo(() => {
    if (!heatmap?.length) return [];
    const buckets = [
      { range: t('bucket_ontime'), min: 0, max: 60, count: 0, color: '#22c55e' },
      { range: t('bucket_slight'), min: 60, max: 180, count: 0, color: '#f59e0b' },
      { range: t('bucket_moderate'), min: 180, max: 420, count: 0, color: '#f97316' },
      { range: t('bucket_severe'), min: 420, max: Infinity, count: 0, color: '#ef4444' },
    ];
    for (const point of heatmap) {
      for (const bucket of buckets) {
        if (point.avgDelaySeconds >= bucket.min && point.avgDelaySeconds < bucket.max) {
          bucket.count += point.sampleCount;
          break;
        }
      }
    }
    return buckets;
  }, [heatmap, t]);

  const delayOverTime = useMemo(() => {
    if (!peakHours?.length) return [];
    return peakHours.map((h: { hourOfDay: number; avgDelaySeconds: number }) => ({
      hour: `${h.hourOfDay}:00`,
      delay: Math.round(h.avgDelaySeconds),
      severity: h.avgDelaySeconds < 60 ? 'low' : h.avgDelaySeconds < 180 ? 'medium' : 'high',
    }));
  }, [peakHours]);

  if (isLoading) return <SkeletonChart height={300} />;

  return (
    <div className="space-y-4 sm:gap-6">
      {isError && <ErrorAlert message={error.message} onRetry={refetch} />}

      <Card className="p-4 sm:p-5">
        <CardHeader className="p-0 mb-4">
          <CardTitle>{t('delay_distribution')}</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {delayDistribution.length > 0 ? (
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
              {delayDistribution.map((b) => (
                <div key={b.range} className="bg-secondary rounded-lg p-3 text-center">
                  <div className="text-2xl font-bold" style={{ color: b.color }}>{b.count}</div>
                  <div className="text-xs text-muted-foreground mt-1">{b.range}</div>
                </div>
              ))}
            </div>
          ) : (
            <p className="text-muted-foreground text-sm">{t('no_data', { ns: 'common' })}</p>
          )}
        </CardContent>
      </Card>

      <Card className="p-4 sm:p-5">
        <CardHeader className="p-0 mb-4">
          <CardTitle>{t('delay_over_hours')}</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {delayOverTime.length > 0 ? (
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={delayOverTime}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis dataKey="hour" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                <YAxis unit="s" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                <Tooltip />
                <Area type="monotone" dataKey="delay" stroke="hsl(var(--primary))" fill="hsl(var(--primary))" fillOpacity={0.15} strokeWidth={2} />
              </AreaChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-muted-foreground text-sm">{t('no_data', { ns: 'common' })}</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function CongestionTab() {
  const { t } = useTranslation('analytics');
  const { data: congestion, isLoading, isError, error, refetch } = useStopCongestionAll();

  const topCongested = useMemo(() => {
    if (!congestion?.length) return [];
    return congestion.slice(0, 15);
  }, [congestion]);

  const severityCounts = useMemo(() => {
    if (!congestion?.length) return { low: 0, medium: 0, high: 0, severe: 0 };
    return congestion.reduce(
      (acc: { low: number; medium: number; high: number; severe: number }, c: { severity: string }) => {
        acc[c.severity as keyof typeof acc]++;
        return acc;
      },
      { low: 0, medium: 0, high: 0, severe: 0 }
    );
  }, [congestion]);

  if (isLoading) return <SkeletonChart height={300} />;

  return (
    <div className="space-y-4 sm:gap-6">
      {isError && <ErrorAlert message={error.message} onRetry={refetch} />}

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <StatBox label={t('severity_low')} value={String(severityCounts.low)} color="text-green-600" />
        <StatBox label={t('severity_medium')} value={String(severityCounts.medium)} color="text-amber-600" />
        <StatBox label={t('severity_high')} value={String(severityCounts.high)} color="text-orange-600" />
        <StatBox label={t('severity_severe')} value={String(severityCounts.severe)} color="text-red-600" />
      </div>

      <Card className="p-4 sm:p-5">
        <CardHeader className="p-0 mb-4">
          <CardTitle>{t('top_congested_stops')}</CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {topCongested.length > 0 ? (
            <ResponsiveContainer width="100%" height={Math.max(300, topCongested.length * 28)}>
              <BarChart data={topCongested} layout="vertical" margin={{ left: 80 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis type="number" unit="s" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                <YAxis dataKey="stopName" type="category" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 11 }} width={80} />
                <Tooltip formatter={(v) => [`${Math.round(Number(v))}s`, t('avg_delay')]} />
                <Bar dataKey="avgDelaySeconds" fill="hsl(var(--primary))" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="text-muted-foreground text-sm">{t('no_data', { ns: 'common' })}</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function LiveTab() {
  const { t } = useTranslation('analytics');
  const { data: overview, isLoading: overviewLoading, isError: overviewError, error: overviewErr, refetch: refetchOverview } = useSystemOverview();
  const { data: vehicles } = useLiveVehicles();

  return (
    <div className="space-y-4 sm:gap-6">
      {overviewError && <ErrorAlert message={overviewErr.message} onRetry={refetchOverview} />}

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
        <StatBox
          label={t('live_vehicles')}
          value={overviewLoading ? '...' : String(overview?.liveVehicleCount ?? vehicles?.length ?? 0)}
          color="text-green-600"
        />
        <StatBox
          label={t('avg_delay_1h')}
          value={overviewLoading ? '...' : `${Math.round(overview?.avgDelaySecondsLastHour ?? 0)}s`}
          color="text-amber-600"
        />
        <StatBox
          label={t('total_routes')}
          value={overviewLoading ? '...' : String(overview?.totalRoutes ?? 0)}
        />
        <StatBox
          label={t('total_stops')}
          value={overviewLoading ? '...' : String(overview?.totalStops ?? 0)}
        />
      </div>

      {vehicles && vehicles.length > 0 && (
        <Card className="p-4 sm:p-5">
          <CardHeader className="p-0 mb-4">
            <CardTitle>{t('vehicle_speed_distribution')}</CardTitle>
          </CardHeader>
          <CardContent className="p-0">
            {(() => {
              const speedBuckets = [
                { range: '0–20 km/h', min: 0, max: 20, count: 0 },
                { range: '20–40 km/h', min: 20, max: 40, count: 0 },
                { range: '40–60 km/h', min: 40, max: 60, count: 0 },
                { range: '60+ km/h', min: 60, max: Infinity, count: 0 },
              ];
              for (const v of vehicles as { speed: number }[]) {
                const kmh = v.speed * 3.6;
                for (const b of speedBuckets) {
                  if (kmh >= b.min && kmh < b.max) { b.count++; break; }
                }
              }
              return (
                <ResponsiveContainer width="100%" height={200}>
                  <BarChart data={speedBuckets}>
                    <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                    <XAxis dataKey="range" tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                    <YAxis tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
                    <Tooltip />
                    <Bar dataKey="count" fill="hsl(var(--primary))" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              );
            })()}
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function StatBox({ label, value, color }: { label: string; value: string; color?: string }) {
  return (
    <div className="bg-card border border-border rounded-lg p-3 sm:p-4 shadow-sm">
      <div className={`text-xl sm:text-2xl font-bold ${color ?? 'text-foreground'}`}>{value}</div>
      <div className="text-xs text-muted-foreground mt-1">{label}</div>
    </div>
  );
}

function RankingCard({
  title, titleColor, barColor, data, isError, error, onRetry,
}: {
  title: string;
  titleColor: string;
  barColor: string;
  data?: { routeId: string; shortName: string; score: number }[];
  isError: boolean;
  error: Error | null;
  onRetry: () => void;
}) {
  return (
    <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
      <h3 className={`text-sm font-semibold ${titleColor} mb-3`}>{title}</h3>
      {isError ? (
        <ErrorAlert message={error?.message ?? 'Unknown error'} onRetry={onRetry} />
      ) : data?.length ? (
        <div className="space-y-2">
          {data.map((r, i) => (
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
                  className={`${barColor} h-1.5 rounded-full`}
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
        <p className="text-muted-foreground text-sm">No data</p>
      )}
    </div>
  );
}
