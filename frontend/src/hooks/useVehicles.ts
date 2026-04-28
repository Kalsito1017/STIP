import { useQuery } from '@tanstack/react-query';
import { vehiclesApi } from '../services/api';

export function useLiveVehicles(routeId?: string) {
  return useQuery({
    queryKey: ['liveVehicles', routeId],
    queryFn: () => vehiclesApi.getLive(routeId),
    refetchInterval: 15_000,
  });
}
