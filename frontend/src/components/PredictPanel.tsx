import { useState } from 'react';
import { Zap, Loader2 } from 'lucide-react';
import { useDelayPrediction } from '../hooks/usePrediction';

const DAYS = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
const HOURS = Array.from({ length: 24 }, (_, i) => i);

interface PredictPanelProps {
  routeId: string;
  stopId: string;
}

export function PredictPanel({ routeId, stopId }: PredictPanelProps) {
  const [dayOfWeek, setDayOfWeek] = useState(new Date().getDay() === 0 ? 6 : new Date().getDay() - 1);
  const [hour, setHour] = useState(new Date().getHours());
  const prediction = useDelayPrediction();

  const handlePredict = () => {
    prediction.mutate({ routeId, stopId, hour, dayOfWeek });
  };

  const isPeak = (hour >= 7 && hour <= 9) || (hour >= 17 && hour <= 19);

  const delayBucket = prediction.data
    ? prediction.data.predictedDelaySeconds < 60
      ? 'On Time'
      : prediction.data.predictedDelaySeconds < 180
        ? 'Slight Delay'
        : prediction.data.predictedDelaySeconds < 420
          ? 'Moderate Delay'
          : 'Severe Delay'
    : null;

  const bucketColor =
    delayBucket === 'On Time'
      ? 'text-green-600'
      : delayBucket === 'Slight Delay'
        ? 'text-amber-600'
        : delayBucket === 'Moderate Delay'
          ? 'text-orange-600'
          : 'text-red-600';

  const bucketBg =
    delayBucket === 'On Time'
      ? 'bg-green-50'
      : delayBucket === 'Slight Delay'
        ? 'bg-amber-50'
        : delayBucket === 'Moderate Delay'
          ? 'bg-orange-50'
          : 'bg-red-50';

  return (
    <div className="bg-white border border-slate-200 rounded-lg p-5 shadow-sm">
      <h3 className="flex items-center gap-2 text-sm font-semibold text-slate-700 mb-4">
        <Zap className="w-4 h-4 text-purple-500" /> ML Delay Prediction
      </h3>

      <div className="grid grid-cols-2 gap-3 mb-4">
        <div>
          <label className="block text-xs text-slate-500 mb-1">Day</label>
          <select
            value={dayOfWeek}
            onChange={(e) => setDayOfWeek(Number(e.target.value))}
            className="w-full text-sm border border-slate-300 rounded-md px-3 py-1.5 bg-white"
          >
            {DAYS.map((d, i) => (
              <option key={d} value={i}>{d}</option>
            ))}
          </select>
        </div>
        <div>
          <label className="block text-xs text-slate-500 mb-1">Hour</label>
          <select
            value={hour}
            onChange={(e) => setHour(Number(e.target.value))}
            className="w-full text-sm border border-slate-300 rounded-md px-3 py-1.5 bg-white"
          >
            {HOURS.map((h) => (
              <option key={h} value={h}>{String(h).padStart(2, '0')}:00</option>
            ))}
          </select>
        </div>
      </div>

      {isPeak && (
        <div className="bg-amber-50 border border-amber-200 rounded-md px-3 py-1.5 text-xs text-amber-700 mb-4">
          Peak hours selected — delays are typically higher during this window
        </div>
      )}

      <button
        onClick={handlePredict}
        disabled={prediction.isPending}
        className="w-full flex items-center justify-center gap-2 bg-purple-600 text-white text-sm font-medium rounded-md px-4 py-2 hover:bg-purple-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
      >
        {prediction.isPending ? (
          <>
            <Loader2 className="w-4 h-4 animate-spin" /> Predicting...
          </>
        ) : (
          <>
            <Zap className="w-4 h-4" /> Predict Delay
          </>
        )}
      </button>

      {prediction.error && (
        <p className="text-red-600 text-xs mt-3 bg-red-50 border border-red-200 rounded-md p-2">
          {prediction.error.message}
        </p>
      )}

      {prediction.data && (
        <div className={`mt-4 rounded-md p-4 ${bucketBg}`}>
          <div className="flex items-center justify-between mb-2">
            <span className="text-xs text-slate-500">Predicted Delay</span>
            <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${bucketBg} ${bucketColor}`}>
              {delayBucket}
            </span>
          </div>
          <div className="flex items-baseline gap-2">
            <span className={`text-3xl font-bold ${bucketColor}`}>
              {Math.round(prediction.data.predictedDelaySeconds)}s
            </span>
            <span className="text-xs text-slate-500">
              ±{Math.round(prediction.data.confidenceInterval[1] - prediction.data.predictedDelaySeconds)}s
            </span>
          </div>
          <div className="mt-2 space-y-1 text-xs text-slate-500">
            <div className="flex items-center justify-between">
              <span>Confidence range</span>
              <span>
                {Math.round(prediction.data.confidenceInterval[0])}s – {Math.round(prediction.data.confidenceInterval[1])}s
              </span>
            </div>
            <div className="flex items-center justify-between">
              <span>Model</span>
              <span className="font-mono">{prediction.data.modelVersion}</span>
            </div>
          </div>
        </div>
      )}

      {!prediction.data && !prediction.isPending && !prediction.error && (
        <p className="text-xs text-slate-400 mt-3 text-center">
          Select a day and hour, then click Predict to get an ML-based delay estimate for this route.
        </p>
      )}
    </div>
  );
}
