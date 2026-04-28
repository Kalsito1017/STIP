import { useQuery } from '@tanstack/react-query';
import { stopsApi } from '../services/api';

export function useStops() {
  return useQuery({
    queryKey: ['stops'],
    queryFn: () => stopsApi.getAll(),
    staleTime: 300_000,
  });
}

export function useStopCongestion(stopId: string, date?: string) {
  return useQuery({
    queryKey: ['stopCongestion', stopId, date],
    queryFn: () => stopsApi.getCongestion(stopId, date),
    enabled: !!stopId,
  });
}
