import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, MapPin } from 'lucide-react';
import { useStops, useStopCongestion } from '../hooks/useStops';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';

export function StopDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: stops } = useStops();
  const stop = stops?.find((s: { stopId: string }) => s.stopId === id);
  const { data: congestion } = useStopCongestion(id!);

  if (!stop) return <p className="text-slate-500">Stop not found</p>;

  return (
    <div className="space-y-6">
      <button onClick={() => navigate(-1)} className="flex items-center gap-2 text-sm text-slate-500 hover:text-slate-800">
        <ArrowLeft className="w-4 h-4" /> Back
      </button>

      <div className="bg-white border border-slate-200 rounded-lg p-6 shadow-sm">
        <div className="flex items-start gap-4">
          <MapPin className="w-8 h-8 text-blue-600 mt-1" />
          <div>
            <h1 className="text-2xl font-bold text-slate-900">{stop.stopName}</h1>
            <p className="text-sm text-slate-500 mt-1">Stop ID: {stop.stopId}</p>
            <p className="text-sm text-slate-500">
              Coordinates: {stop.lat.toFixed(4)}, {stop.lon.toFixed(4)}
            </p>
          </div>
        </div>
      </div>

      <div className="bg-white border border-slate-200 rounded-lg p-5 shadow-sm">
        <h3 className="text-sm font-semibold text-slate-700 mb-4">Hourly Congestion</h3>
        {congestion?.length ? (
          <ResponsiveContainer width="100%" height={250}>
            <BarChart data={congestion}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="hourOfDay" tickFormatter={(h) => `${h}:00`} />
              <YAxis />
              <Tooltip />
              <Bar dataKey="vehicleCount" fill="#f59e0b" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        ) : (
          <p className="text-slate-400 text-sm">No congestion data available</p>
        )}
      </div>
    </div>
  );
}
