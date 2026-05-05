import { useQuery } from '@tanstack/react-query';
import { vehiclesApi } from '../services/api';

export function useLiveVehicles() {
  return useQuery({
    queryKey: ['liveVehicles'],
    queryFn: () => vehiclesApi.getLive(),
    refetchInterval: 15_000,
  });
}
