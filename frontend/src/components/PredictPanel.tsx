import { useState, useMemo } from 'react';
import { Zap, Loader2 } from 'lucide-react';
import { useDelayPrediction } from '../hooks/usePrediction';
import { useStops } from '../hooks/useStops';
import { useTranslation } from 'react-i18next';
import type { AxiosError } from 'axios';

interface PredictPanelProps {
  routeId: string;
  stopId?: string;
  stopSequence?: number;
}

export function PredictPanel({ routeId, stopId: initialStopId, stopSequence = 1 }: PredictPanelProps) {
  const { t } = useTranslation('predict');
  const [dayOfWeek, setDayOfWeek] = useState(new Date().getDay() === 0 ? 6 : new Date().getDay() - 1);
  const [hour, setHour] = useState(new Date().getHours());
  const [selectedStopId, setSelectedStopId] = useState(initialStopId ?? '');
  const prediction = useDelayPrediction();
  const { data: stops } = useStops();

  const DAYS = [
    t('days.monday'),
    t('days.tuesday'),
    t('days.wednesday'),
    t('days.thursday'),
    t('days.friday'),
    t('days.saturday'),
    t('days.sunday'),
  ];
  const HOURS = Array.from({ length: 24 }, (_, i) => i);

  const sortedStops = useMemo(() => {
    if (!stops) return [];
    return [...stops].sort((a: { stopName: string }, b: { stopName: string }) =>
      a.stopName.localeCompare(b.stopName)
    );
  }, [stops]);

  const canPredict = !!selectedStopId && !!routeId;

  const handlePredict = () => {
    if (!canPredict) return;
    prediction.mutate({ routeId, stopId: selectedStopId, stopSequence, hour, dayOfWeek });
  };

  const isPeak = (hour >= 7 && hour <= 9) || (hour >= 17 && hour <= 19);

  const delayBucket = prediction.data
    ? prediction.data.predictedDelaySeconds < 60
      ? t('on_time')
      : prediction.data.predictedDelaySeconds < 180
        ? t('slight_delay')
        : prediction.data.predictedDelaySeconds < 420
          ? t('moderate_delay')
          : t('severe_delay')
    : null;

  const bucketColor =
    delayBucket === t('on_time')
      ? 'text-green-600 dark:text-green-400'
      : delayBucket === t('slight_delay')
        ? 'text-amber-600 dark:text-amber-400'
        : delayBucket === t('moderate_delay')
          ? 'text-orange-600 dark:text-orange-400'
          : 'text-red-600 dark:text-red-400';

  const bucketBg =
    delayBucket === t('on_time')
      ? 'bg-green-50 dark:bg-green-950'
      : delayBucket === t('slight_delay')
        ? 'bg-amber-50 dark:bg-amber-950'
        : delayBucket === t('moderate_delay')
          ? 'bg-orange-50 dark:bg-orange-950'
          : 'bg-red-50 dark:bg-red-950';

  return (
    <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
      <h3 className="flex items-center gap-2 text-sm font-semibold text-foreground mb-4">
        <Zap className="w-4 h-4 text-purple-500" /> {t('title')}
      </h3>

      <div className="mb-3">
        <label className="block text-xs text-muted-foreground mb-1">{t('stop_label')}</label>
        <select
          value={selectedStopId}
          onChange={(e) => setSelectedStopId(e.target.value)}
          className="w-full text-sm border border-border rounded-md px-2 sm:px-3 py-1.5 bg-background text-foreground"
        >
          <option value="">{t('select_stop')}</option>
          {sortedStops.map((s: { stopId: string; stopName: string }) => (
            <option key={s.stopId} value={s.stopId}>
              {s.stopName} ({s.stopId})
            </option>
          ))}
        </select>
      </div>

      <div className="grid grid-cols-2 gap-2 sm:gap-3 mb-4">
        <div>
          <label className="block text-xs text-muted-foreground mb-1">{t('day_label')}</label>
          <select
            value={dayOfWeek}
            onChange={(e) => setDayOfWeek(Number(e.target.value))}
            className="w-full text-sm border border-border rounded-md px-2 sm:px-3 py-1.5 bg-background text-foreground"
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
            className="w-full text-sm border border-border rounded-md px-2 sm:px-3 py-1.5 bg-background text-foreground"
          >
            {HOURS.map((h) => (
              <option key={h} value={h}>{String(h).padStart(2, '0')}:00</option>
            ))}
          </select>
        </div>
      </div>

      {isPeak && (
        <div className="bg-amber-50 dark:bg-amber-950 border border-amber-200 dark:border-amber-800 rounded-md px-3 py-1.5 text-xs text-amber-700 dark:text-amber-300 mb-4">
          {t('peak_warning')}
        </div>
      )}

      <button
        onClick={handlePredict}
        disabled={prediction.isPending || !canPredict}
        className="w-full flex items-center justify-center gap-2 bg-purple-600 text-white text-sm font-medium rounded-md px-4 py-2 hover:bg-purple-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        title={!canPredict ? t('select_to_predict') : undefined}
      >
        {prediction.isPending ? (
          <>
            <Loader2 className="w-4 h-4 animate-spin" /> {t('predicting')}
          </>
        ) : (
          <>
            <Zap className="w-4 h-4" /> {t('predict_delay')}
          </>
        )}
      </button>

      {prediction.error && (
        <p className="text-destructive text-xs bg-destructive/10 border border-destructive/20 rounded-md p-2 mt-3">
          {(prediction.error as AxiosError<{ error?: string; details?: string[] }>).response?.data?.details?.join?.(', ')
            ?? (prediction.error as AxiosError<{ error?: string }>).response?.data?.error
            ?? prediction.error.message}
        </p>
      )}

      {prediction.data && (
        <div className={`mt-4 rounded-md p-3 sm:p-4 ${bucketBg}`}>
          <div className="flex items-center justify-between mb-2 flex-wrap gap-1">
            <span className="text-xs text-muted-foreground">{t('predicted_delay')}</span>
            <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${bucketBg} ${bucketColor}`}>
              {delayBucket}
            </span>
          </div>
          <div className="flex items-baseline gap-2">
            <span className={`text-2xl sm:text-3xl font-bold ${bucketColor}`}>
              {Math.round(prediction.data.predictedDelaySeconds)}s
            </span>
            <span className="text-xs text-muted-foreground">
              ±{Math.round(prediction.data.confidenceInterval[1] - prediction.data.predictedDelaySeconds)}s
            </span>
          </div>
          <div className="mt-2 space-y-1 text-xs text-muted-foreground">
            <div className="flex flex-wrap items-center justify-between gap-1">
              <span>{t('confidence_range')}</span>
              <span>
                {Math.round(prediction.data.confidenceInterval[0])}s \u2013 {Math.round(prediction.data.confidenceInterval[1])}s
              </span>
            </div>
            <div className="flex flex-wrap items-center justify-between gap-1">
              <span>{t('model')}</span>
              <span className="font-mono">{prediction.data.modelVersion}</span>
            </div>
          </div>
        </div>
      )}

      {!prediction.data && !prediction.isPending && !prediction.error && (
        <p className="text-xs text-muted-foreground mt-3 text-center">
          {t('instruction')}
        </p>
      )}
    </div>
  );
}
