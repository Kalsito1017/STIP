import { useQuery } from '@tanstack/react-query';
import { analyticsApi } from '../services/api';

export function useDelayHeatmap() {
  return useQuery({
    queryKey: ['delayHeatmap'],
    queryFn: () => analyticsApi.getHeatmap(),
    staleTime: 300_000,
  });
}
