import { useQuery } from '@tanstack/react-query';
import { routesApi } from '../services/api';

export function useRoutes() {
  return useQuery({
    queryKey: ['routes'],
    queryFn: () => routesApi.getAll(),
    staleTime: 300_000,
  });
}

export function useRouteDetail(routeId: string) {
  return useQuery({
    queryKey: ['route', routeId],
    queryFn: () => routesApi.getById(routeId),
    enabled: !!routeId,
  });
}
