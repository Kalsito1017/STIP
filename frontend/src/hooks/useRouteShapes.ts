import { useQuery } from '@tanstack/react-query';
import { routesApi } from '../services/api';
import type { RouteShapeCollection } from '../types/map';

export function useRouteShape(routeId: string) {
  return useQuery<RouteShapeCollection>({
    queryKey: ['routeShape', routeId],
    queryFn: () => routesApi.getShape(routeId),
    enabled: !!routeId,
    staleTime: 1_800_000,
  });
}

export function useAllRouteShapes(enabled = true) {
  return useQuery<RouteShapeCollection>({
    queryKey: ['allRouteShapes'],
    queryFn: () => routesApi.getAllShapes(),
    enabled,
    staleTime: 1_800_000,
  });
}

