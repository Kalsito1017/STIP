import { useQuery } from '@tanstack/react-query';
import { analyticsApi, routesApi } from '../services/api';

export function useDelayHeatmap(from?: string, to?: string) {
  return useQuery({
    queryKey: ['heatmap', from, to],
    queryFn: () => analyticsApi.getHeatmap(from, to),
    staleTime: 60_000,
  });
}

export function useReliabilityRanking(top = 10, best = true) {
  return useQuery({
    queryKey: ['ranking', top, best],
    queryFn: () => analyticsApi.getRanking(top, best),
    staleTime: 300_000,
  });
}

export function useRouteDelayPattern(routeId: string, date?: string) {
  return useQuery({
    queryKey: ['delayPattern', routeId, date],
    queryFn: () => routesApi.getDelayPattern(routeId, date),
    enabled: !!routeId,
  });
}
