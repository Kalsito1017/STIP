import { useQuery } from '@tanstack/react-query';
import { analyticsApi } from '../services/api';

export function useDelayHeatmap(enabled = true) {
  return useQuery({
    queryKey: ['delayHeatmap'],
    queryFn: () => analyticsApi.getHeatmap(),
    enabled,
    staleTime: 300_000,
  });
}
