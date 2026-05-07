import { useQuery } from '@tanstack/react-query';
import { stopsApi } from '../services/api';

export interface PredictedArrival {
  routeId: string;
  routeShortName: string;
  destination: string;
  scheduledMinutes: number;
  predictedDelaySeconds: number | null;
  predictionConfidence: string | null;
}

export function usePredictedArrivals(stopId: string | null) {
  return useQuery<PredictedArrival[]>({
    queryKey: ['predictedArrivals', stopId],
    queryFn: () => stopsApi.getPredictedArrivals(stopId!),
    enabled: !!stopId,
    staleTime: 30_000,
    refetchInterval: 60_000,
  });
}
