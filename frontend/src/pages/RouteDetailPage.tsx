import { useParams, useNavigate, Navigate } from 'react-router-dom';
import { ArrowLeft, Clock } from 'lucide-react';
import { useRouteDetail } from '../hooks/useRoutes';
import { useRouteDelayPattern } from '../hooks/useDelays';
import { PredictPanel } from '../components/PredictPanel';
import { ErrorAlert } from '../components/ErrorAlert';
import { Skeleton, SkeletonCard, SkeletonChart } from '../components/Skeleton';
import { RouteBadge } from '../components/RouteBadge';
import { Button } from '../components/ui/button';
import { Card, CardHeader, CardTitle, CardContent } from '../components/ui/card';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';

export function RouteDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  if (!id) return <Navigate to="/routes" replace />;

  const { data: route, isLoading, isError, error, refetch } = useRouteDetail(id);
  const { data: delayPattern, isError: dpError, error: dpErr, refetch: refetchDp } = useRouteDelayPattern(id);

  if (isLoading) return (
    <div className="space-y-4 sm:space-y-6">
      <Skeleton className="h-4 w-16" />
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6">
        <div className="lg:col-span-2 bg-white border border-slate-200 rounded-lg p-4 sm:p-6 shadow-sm space-y-4">
          <Skeleton className="h-7 w-32" />
          <Skeleton className="h-4 w-48" />
          <div className="grid grid-cols-3 gap-2 sm:gap-4">
            <Skeleton className="h-16" />
            <Skeleton className="h-16" />
            <Skeleton className="h-16" />
          </div>
        </div>
        <SkeletonCard />
      </div>
      <SkeletonChart height={250} />
    </div>
  );
  if (isError) return <ErrorAlert message={error.message} onRetry={() => refetch()} />;
  if (!route) return <p className="text-slate-500">Route not found</p>;

  const score = route.latestReliability?.score ?? null;
  const scoreColor = score === null ? 'text-slate-400' : score >= 70 ? 'text-green-600' : score >= 40 ? 'text-amber-600' : 'text-red-600';

  return (
    <div className="space-y-4 sm:space-y-6">
      <Button variant="ghost" size="sm" onClick={() => navigate(-1)}>
        <ArrowLeft className="w-4 h-4" /> Back
      </Button>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6">
        <div className="lg:col-span-2">
          <Card className="p-4 sm:p-6">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 mb-4">
              <div className="min-w-0">
                <div className="flex items-center gap-2">
                  <h1 className="text-xl sm:text-2xl font-bold text-slate-900 truncate">{route.shortName}</h1>
                  <RouteBadge type={route.type} />
                </div>
                <p className="text-sm text-slate-500 truncate">{route.longName ?? 'Route'}</p>
              </div>
              {score !== null && (
                <div className="text-center flex-shrink-0">
                  <div className={`text-2xl sm:text-3xl font-bold ${scoreColor}`}>{Math.round(score)}</div>
                  <div className="text-xs text-slate-400">Reliability Score</div>
                </div>
              )}
            </div>

            {route.latestReliability && (
              <div className="grid grid-cols-3 gap-2 sm:gap-4 mt-4 text-sm">
                <div className="bg-slate-50 rounded-md p-2 sm:p-3 text-center">
                  <div className="font-bold text-slate-900">{(route.latestReliability.onTimePct * 100).toFixed(1)}%</div>
                  <div className="text-slate-500 text-xs">On-Time</div>
                </div>
                <div className="bg-slate-50 rounded-md p-2 sm:p-3 text-center">
                  <div className="font-bold text-slate-900">{Math.round(route.latestReliability.avgDelaySeconds)}s</div>
                  <div className="text-slate-500 text-xs">Avg Delay</div>
                </div>
                <div className="bg-slate-50 rounded-md p-2 sm:p-3 text-center">
                  <div className="font-bold text-slate-900">{route.latestReliability.sampleCount}</div>
                  <div className="text-slate-500 text-xs">Samples</div>
                </div>
              </div>
            )}
          </Card>
        </div>

        <PredictPanel routeId={id} />
      </div>

      <Card className="p-4 sm:p-5">
        <CardHeader className="p-0 mb-4">
          <CardTitle className="flex items-center gap-2">
            <Clock className="w-4 h-4" /> Delay by Hour
          </CardTitle>
        </CardHeader>
        <CardContent className="p-0">
        {dpError ? (
          <ErrorAlert message={dpErr.message} onRetry={() => refetchDp()} />
        ) : delayPattern?.length ? (
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
        </CardContent>
      </Card>
    </div>
  );
}
