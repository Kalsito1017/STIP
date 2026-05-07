import { useQuery } from '@tanstack/react-query';
import { analyticsApi, stopsApi } from '../services/api';

export function useStopCongestionAll(enabled = true) {
  return useQuery({
    queryKey: ['stopCongestionAll'],
    queryFn: () => analyticsApi.getStopCongestionAll(),
    enabled,
    staleTime: 300_000,
  });
}

export function useNearbyStops(lat: number | null, lon: number | null, radiusKm = 0.5) {
  return useQuery({
    queryKey: ['nearbyStops', lat, lon, radiusKm],
    queryFn: () => stopsApi.getNearby(lat!, lon!, radiusKm),
    enabled: lat !== null && lon !== null,
    staleTime: 60_000,
  });
}
