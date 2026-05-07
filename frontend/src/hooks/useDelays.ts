import { useQuery } from '@tanstack/react-query';
import { analyticsApi, routesApi } from '../services/api';

export function useDelayHeatmap(from?: string, to?: string) {
  return useQuery({
    queryKey: ['delayHeatmap', from, to],
    queryFn: () => analyticsApi.getHeatmap(from, to),
    staleTime: 60_000,
  });
}

export function useReliabilityRanking(top = 10, best = true) {
  return useQuery({
    queryKey: ['reliabilityRanking', top, best],
    queryFn: () => analyticsApi.getRanking(top, best),
    staleTime: 300_000,
  });
}

export function usePeakHours(date?: string) {
  return useQuery({
    queryKey: ['peakHours', date],
    queryFn: () => analyticsApi.getPeakHours(date),
    staleTime: 300_000,
  });
}

export function useRouteDelayPattern(routeId: string, date?: string) {
  return useQuery({
    queryKey: ['routeDelayPattern', routeId, date],
    queryFn: () => routesApi.getDelayPattern(routeId, date),
    enabled: !!routeId,
  });
}

export function useSystemOverview() {
  return useQuery({
    queryKey: ['systemOverview'],
    queryFn: () => analyticsApi.getOverview(),
    staleTime: 30_000,
    refetchInterval: 30_000,
  });
}

export function useStopCongestionAll(date?: string) {
  return useQuery({
    queryKey: ['stopCongestionAll', date],
    queryFn: () => analyticsApi.getStopCongestionAll(date),
    staleTime: 300_000,
  });
}
