import { useState, useMemo } from 'react';
import { Zap, Clock, Loader2, ArrowRight } from 'lucide-react';
import { motion } from 'motion/react';
import { useDelayPrediction, useTravelTimePrediction } from '../hooks/usePrediction';
import { useRoutes } from '../hooks/useRoutes';
import { useStops } from '../hooks/useStops';
import { useTranslation } from 'react-i18next';
import { Card, CardHeader, CardTitle, CardContent } from '../components/ui/card';
import { ErrorAlert } from '../components/ErrorAlert';
import type { AxiosError } from 'axios';

type Tab = 'delay' | 'travel-time';

export function PredictionsPage() {
  const { t } = useTranslation('predictions');
  const [activeTab, setActiveTab] = useState<Tab>('delay');
  const { data: routes, isLoading: routesLoading, isError: routesError, error: routesErr } = useRoutes();
  const { data: stops, isLoading: stopsLoading } = useStops();

  const sortedRoutes = useMemo(() => {
    if (!routes) return [];
    return [...routes].sort((a: { shortName: string }, b: { shortName: string }) =>
      a.shortName.localeCompare(b.shortName)
    );
  }, [routes]);

  const sortedStops = useMemo(() => {
    if (!stops) return [];
    return [...stops].sort((a: { stopName: string }, b: { stopName: string }) =>
      a.stopName.localeCompare(b.stopName)
    );
  }, [stops]);

  if (routesLoading || stopsLoading) {
    return (
      <div className="space-y-4 sm:space-y-6">
        <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 sm:gap-6">
          {[1, 2].map((i) => (
            <div key={i} className="bg-card border border-border rounded-lg p-6 shadow-sm space-y-4">
              <div className="h-5 w-32 bg-muted rounded animate-pulse" />
              <div className="h-4 w-48 bg-muted rounded animate-pulse" />
              <div className="h-10 w-full bg-muted rounded animate-pulse" />
              <div className="h-10 w-full bg-muted rounded animate-pulse" />
              <div className="h-10 w-32 bg-muted rounded animate-pulse" />
            </div>
          ))}
        </div>
      </div>
    );
  }

  if (routesError) return <ErrorAlert message={routesErr.message} />;

  return (
    <div className="space-y-4 sm:space-y-6">
      <h1 className="text-xl sm:text-2xl font-bold text-foreground">{t('title')}</h1>

      <div className="flex gap-2 border-b border-border pb-2">
        <button
          onClick={() => setActiveTab('delay')}
          className={`flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-t-md transition-colors ${
            activeTab === 'delay'
              ? 'bg-primary/10 text-primary border-b-2 border-primary'
              : 'text-muted-foreground hover:text-foreground hover:bg-accent'
          }`}
        >
          <Zap className="w-4 h-4" />
          {t('delay_tab')}
        </button>
        <button
          onClick={() => setActiveTab('travel-time')}
          className={`flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-t-md transition-colors ${
            activeTab === 'travel-time'
              ? 'bg-primary/10 text-primary border-b-2 border-primary'
              : 'text-muted-foreground hover:text-foreground hover:bg-accent'
          }`}
        >
          <Clock className="w-4 h-4" />
          {t('travel_time_tab')}
        </button>
      </div>

      {activeTab === 'delay' ? (
        <DelayPredictionCard routes={sortedRoutes} stops={sortedStops} />
      ) : (
        <TravelTimeCard routes={sortedRoutes} stops={sortedStops} />
      )}
    </div>
  );
}

function DelayPredictionCard({
  routes,
  stops,
}: {
  routes: { routeId: string; shortName: string; longName?: string }[];
  stops: { stopId: string; stopName: string }[];
}) {
  const { t } = useTranslation('predictions');
  const [routeId, setRouteId] = useState('');
  const [stopId, setStopId] = useState('');
  const [stopSequence, setStopSequence] = useState(1);
  const [dayOfWeek, setDayOfWeek] = useState(new Date().getDay() === 0 ? 6 : new Date().getDay() - 1);
  const [hour, setHour] = useState(new Date().getHours());
  const prediction = useDelayPrediction();

  const DAYS = [
    t('days.monday'), t('days.tuesday'), t('days.wednesday'),
    t('days.thursday'), t('days.friday'), t('days.saturday'), t('days.sunday'),
  ];
  const HOURS = Array.from({ length: 24 }, (_, i) => i);

  const canPredict = !!routeId && !!stopId;

  const handlePredict = () => {
    if (!canPredict) return;
    prediction.mutate({ routeId, stopId, stopSequence, hour, dayOfWeek });
  };

  const isPeak = (hour >= 7 && hour <= 9) || (hour >= 17 && hour <= 19);

  const getBucketInfo = (delaySeconds: number) => {
    if (delaySeconds < 60) return { label: t('on_time'), color: 'text-green-600', bg: 'bg-green-50 dark:bg-green-950' };
    if (delaySeconds < 180) return { label: t('slight_delay'), color: 'text-amber-600', bg: 'bg-amber-50 dark:bg-amber-950' };
    if (delaySeconds < 420) return { label: t('moderate_delay'), color: 'text-orange-600', bg: 'bg-orange-50 dark:bg-orange-950' };
    return { label: t('severe_delay'), color: 'text-red-600', bg: 'bg-red-50 dark:bg-red-950' };
  };

  const bucket = prediction.data ? getBucketInfo(prediction.data.predictedDelaySeconds) : null;

  return (
    <Card className="p-4 sm:p-6">
      <CardHeader className="p-0 mb-4">
        <CardTitle className="flex items-center gap-2 text-sm">
          <Zap className="w-4 h-4 text-purple-500" />
          {t('delay_card_title')}
        </CardTitle>
      </CardHeader>
      <CardContent className="p-0 space-y-4">
        <div>
          <label className="block text-xs text-muted-foreground mb-1">{t('route_label')}</label>
          <select
            value={routeId}
            onChange={(e) => setRouteId(e.target.value)}
            className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
          >
            <option value="">{t('select_route')}</option>
            {routes.map((r) => (
              <option key={r.routeId} value={r.routeId}>{r.shortName} — {r.longName ?? r.routeId}</option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-xs text-muted-foreground mb-1">{t('stop_label')}</label>
          <select
            value={stopId}
            onChange={(e) => setStopId(e.target.value)}
            className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
          >
            <option value="">{t('select_stop')}</option>
            {stops.map((s) => (
              <option key={s.stopId} value={s.stopId}>{s.stopName}</option>
            ))}
          </select>
        </div>

        <div className="grid grid-cols-3 gap-3">
          <div>
            <label className="block text-xs text-muted-foreground mb-1">{t('stop_sequence')}</label>
            <input
              type="number"
              min={1}
              value={stopSequence}
              onChange={(e) => setStopSequence(Number(e.target.value))}
              className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
            />
          </div>
          <div>
            <label className="block text-xs text-muted-foreground mb-1">{t('day_label')}</label>
            <select
              value={dayOfWeek}
              onChange={(e) => setDayOfWeek(Number(e.target.value))}
              className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
            >
              {DAYS.map((d, i) => (
                <option key={d} value={i}>{d}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-xs text-muted-foreground mb-1">{t('hour_label')}</label>
            <select
              value={hour}
              onChange={(e) => setHour(Number(e.target.value))}
              className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
            >
              {HOURS.map((h) => (
                <option key={h} value={h}>{String(h).padStart(2, '0')}:00</option>
              ))}
            </select>
          </div>
        </div>

        {isPeak && (
          <div className="bg-amber-50 dark:bg-amber-950 border border-amber-200 dark:border-amber-800 rounded-md px-3 py-2 text-xs text-amber-700 dark:text-amber-300">
            {t('peak_warning')}
          </div>
        )}

        <button
          onClick={handlePredict}
          disabled={prediction.isPending || !canPredict}
          className="w-full flex items-center justify-center gap-2 bg-purple-600 text-white text-sm font-medium rounded-md px-4 py-2.5 hover:bg-purple-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {prediction.isPending ? (
            <><Loader2 className="w-4 h-4 animate-spin" /> {t('predicting')}</>
          ) : (
            <><Zap className="w-4 h-4" /> {t('predict_delay')}</>
          )}
        </button>

        {prediction.error && (
          <p className="text-destructive text-xs bg-destructive/10 border border-destructive/20 rounded-md p-2">
            {(prediction.error as AxiosError<{ error?: string; details?: string[] }>).response?.data?.details?.join?.(', ')
              ?? (prediction.error as AxiosError<{ error?: string }>).response?.data?.error
              ?? prediction.error.message}
          </p>
        )}

        {prediction.data && bucket && (
          <motion.div
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            className={`rounded-lg p-4 ${bucket.bg} border border-border`}
          >
            <div className="flex items-center justify-between mb-2">
              <span className="text-xs text-muted-foreground">{t('predicted_delay')}</span>
              <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${bucket.color}`}>
                {bucket.label}
              </span>
            </div>
            <div className="flex items-baseline gap-2">
              <span className={`text-3xl font-bold ${bucket.color}`}>
                {Math.round(prediction.data.predictedDelaySeconds)}s
              </span>
              <span className="text-xs text-muted-foreground">
                ±{Math.round(prediction.data.confidenceInterval[1] - prediction.data.predictedDelaySeconds)}s
              </span>
            </div>
            <div className="mt-3 space-y-1 text-xs text-muted-foreground">
              <div className="flex justify-between">
                <span>{t('confidence_range')}</span>
                <span>{Math.round(prediction.data.confidenceInterval[0])}s – {Math.round(prediction.data.confidenceInterval[1])}s</span>
              </div>
              <div className="flex justify-between">
                <span>{t('model')}</span>
                <span className="font-mono">{prediction.data.modelVersion}</span>
              </div>
            </div>
          </motion.div>
        )}

        {!prediction.data && !prediction.isPending && !prediction.error && (
          <p className="text-xs text-muted-foreground text-center pt-2">{t('delay_instruction')}</p>
        )}
      </CardContent>
    </Card>
  );
}

function TravelTimeCard({
  routes,
  stops,
}: {
  routes: { routeId: string; shortName: string; longName?: string }[];
  stops: { stopId: string; stopName: string }[];
}) {
  const { t } = useTranslation('predictions');
  const [routeId, setRouteId] = useState('');
  const [fromStopId, setFromStopId] = useState('');
  const [toStopId, setToStopId] = useState('');
  const [departureDate, setDepartureDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [departureTime, setDepartureTime] = useState(() => {
    const now = new Date();
    return `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;
  });
  const travelTime = useTravelTimePrediction();

  const canPredict = !!routeId && !!fromStopId && !!toStopId && fromStopId !== toStopId;

  const handlePredict = () => {
    if (!canPredict) return;
    const dt = `${departureDate}T${departureTime}:00Z`;
    travelTime.mutate({ routeId, fromStopId, toStopId, departureTime: dt });
  };

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = Math.round(seconds % 60);
    return mins > 0 ? `${mins}m ${secs}s` : `${secs}s`;
  };

  return (
    <Card className="p-4 sm:p-6">
      <CardHeader className="p-0 mb-4">
        <CardTitle className="flex items-center gap-2 text-sm">
          <Clock className="w-4 h-4 text-blue-500" />
          {t('travel_time_card_title')}
        </CardTitle>
      </CardHeader>
      <CardContent className="p-0 space-y-4">
        <div>
          <label className="block text-xs text-muted-foreground mb-1">{t('route_label')}</label>
          <select
            value={routeId}
            onChange={(e) => setRouteId(e.target.value)}
            className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
          >
            <option value="">{t('select_route')}</option>
            {routes.map((r) => (
              <option key={r.routeId} value={r.routeId}>{r.shortName} — {r.longName ?? r.routeId}</option>
            ))}
          </select>
        </div>

        <div className="grid grid-cols-[1fr_auto_1fr] gap-2 items-end">
          <div>
            <label className="block text-xs text-muted-foreground mb-1">{t('from_stop')}</label>
            <select
              value={fromStopId}
              onChange={(e) => setFromStopId(e.target.value)}
              className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
            >
              <option value="">{t('select_stop')}</option>
              {stops.map((s) => (
                <option key={s.stopId} value={s.stopId}>{s.stopName}</option>
              ))}
            </select>
          </div>
          <ArrowRight className="w-4 h-4 text-muted-foreground mb-2.5 flex-shrink-0" />
          <div>
            <label className="block text-xs text-muted-foreground mb-1">{t('to_stop')}</label>
            <select
              value={toStopId}
              onChange={(e) => setToStopId(e.target.value)}
              className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
            >
              <option value="">{t('select_stop')}</option>
              {stops.map((s) => (
                <option key={s.stopId} value={s.stopId}>{s.stopName}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs text-muted-foreground mb-1">{t('departure_date')}</label>
            <input
              type="date"
              value={departureDate}
              onChange={(e) => setDepartureDate(e.target.value)}
              className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
            />
          </div>
          <div>
            <label className="block text-xs text-muted-foreground mb-1">{t('departure_time')}</label>
            <input
              type="time"
              value={departureTime}
              onChange={(e) => setDepartureTime(e.target.value)}
              className="w-full text-sm border border-border rounded-md px-3 py-2 bg-background text-foreground"
            />
          </div>
        </div>

        <button
          onClick={handlePredict}
          disabled={travelTime.isPending || !canPredict}
          className="w-full flex items-center justify-center gap-2 bg-blue-600 text-white text-sm font-medium rounded-md px-4 py-2.5 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {travelTime.isPending ? (
            <><Loader2 className="w-4 h-4 animate-spin" /> {t('predicting')}</>
          ) : (
            <><Clock className="w-4 h-4" /> {t('predict_travel_time')}</>
          )}
        </button>

        {travelTime.error && (
          <p className="text-destructive text-xs bg-destructive/10 border border-destructive/20 rounded-md p-2">
            {(travelTime.error as AxiosError<{ error?: string; details?: string[] }>).response?.data?.details?.join?.(', ')
              ?? (travelTime.error as AxiosError<{ error?: string }>).response?.data?.error
              ?? travelTime.error.message}
          </p>
        )}

        {travelTime.data && (
          <motion.div
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-blue-50 dark:bg-blue-950 border border-blue-200 dark:border-blue-800 rounded-lg p-4"
          >
            <div className="flex items-center justify-between mb-2">
              <span className="text-xs text-muted-foreground">{t('predicted_travel_time')}</span>
            </div>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-bold text-blue-600">
                {formatTime(travelTime.data.predictedTimeSeconds)}
              </span>
              <span className="text-xs text-muted-foreground">
                ({Math.round(travelTime.data.predictedTimeSeconds)}s)
              </span>
            </div>
            <div className="mt-3 space-y-1 text-xs text-muted-foreground">
              {travelTime.data.confidenceInterval.length >= 2 && (
                <div className="flex justify-between">
                  <span>{t('confidence_range')}</span>
                  <span>{formatTime(travelTime.data.confidenceInterval[0])} – {formatTime(travelTime.data.confidenceInterval[1])}</span>
                </div>
              )}
              <div className="flex justify-between">
                <span>{t('model')}</span>
                <span className="font-mono">{travelTime.data.modelVersion}</span>
              </div>
            </div>
          </motion.div>
        )}

        {!travelTime.data && !travelTime.isPending && !travelTime.error && (
          <p className="text-xs text-muted-foreground text-center pt-2">{t('travel_time_instruction')}</p>
        )}
      </CardContent>
    </Card>
  );
}
