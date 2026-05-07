import { useParams, useNavigate, Navigate } from 'react-router-dom';
import { ArrowLeft, MapPin } from 'lucide-react';
import { motion } from 'motion/react';
import { useStops, useStopCongestion } from '../hooks/useStops';
import { ErrorAlert } from '../components/ErrorAlert';
import { FavoriteButton } from '../components/FavoriteButton';
import { Skeleton, SkeletonChart } from '../components/Skeleton';
import { ArrivalsBoard } from '../components/ArrivalsBoard';
import { StopMap } from '../components/map/StopMap';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import { useTranslation } from 'react-i18next';

export function StopDetailPage() {
  const { t } = useTranslation('stops');
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  if (!id) return <Navigate to="/stops" replace />;
  const { data: stops, isLoading, isError: stopsError, error: stopsErr, refetch: refetchStops } = useStops();
  const stop = stops?.find((s: { stopId: string }) => s.stopId === id);
  const { data: congestion, isLoading: congLoading, isError: congError, error: congErr, refetch: refetchCong } = useStopCongestion(id);

  if (isLoading) return (
    <div className="space-y-4 sm:space-y-6">
      <Skeleton className="h-4 w-16" />
      <div className="bg-card border border-border rounded-lg p-4 sm:p-6 shadow-sm space-y-3">
        <Skeleton className="h-6 w-48" />
        <Skeleton className="h-4 w-32" />
        <Skeleton className="h-4 w-40" />
      </div>
      <SkeletonChart height={250} />
    </div>
  );

  if (stopsError) return <ErrorAlert message={stopsErr.message} onRetry={() => refetchStops()} />;
  if (!stop) return <p className="text-muted-foreground">{t('not_found')}</p>;

  return (
    <div className="space-y-4 sm:space-y-6">
      <button onClick={() => navigate('/stops')} className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors">
        <ArrowLeft className="w-4 h-4" /> {t('back')}
      </button>

      <motion.div
        initial={{ opacity: 0, y: 8 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.25 }}
      >
        <StopMap stopId={stop.stopId} lat={stop.lat} lon={stop.lon} stopName={stop.stopName} />

        <div className="bg-card border border-border rounded-xl p-4 sm:p-6 shadow-sm mt-3">
          <div className="flex items-start gap-3 sm:gap-4">
            <MapPin className="w-6 h-6 sm:w-8 sm:h-8 text-primary mt-1 flex-shrink-0" />
            <div className="min-w-0">
              <h1 className="text-xl sm:text-2xl font-bold text-foreground truncate">{stop.stopName}</h1>
              <FavoriteButton entityType="stop" entityId={stop.stopId} />
              <p className="text-xs sm:text-sm text-muted-foreground mt-1">{t('stop_id')}: {stop.stopId}</p>
              <p className="text-xs sm:text-sm text-muted-foreground">
                {t('coordinates')}: {stop.lat.toFixed(5)}, {stop.lon.toFixed(5)}
              </p>
            </div>
          </div>
        </div>
      </motion.div>

      <ArrivalsBoard stopId={stop.stopId} />

      <div className="bg-card border border-border rounded-xl p-4 sm:p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-foreground mb-4">{t('hourly_congestion')}</h3>
        {congLoading ? (
          <Skeleton className="h-[250px] w-full" />
        ) : congError ? (
          <ErrorAlert message={congErr.message} onRetry={() => refetchCong()} />
        ) : congestion?.length ? (
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={congestion}>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
              <XAxis dataKey="hourOfDay" tickFormatter={(h) => `${h}:00`} tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
              <YAxis tick={{ fill: 'hsl(var(--muted-foreground))', fontSize: 12 }} />
              <Tooltip />
              <Bar dataKey="vehicleCount" fill="#f59e0b" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        ) : (
          <p className="text-muted-foreground text-sm">{t('no_congestion_data')}</p>
        )}
      </div>
    </div>
  );
}
