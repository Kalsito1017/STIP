import { useQuery } from '@tanstack/react-query';
import { routesApi } from '../services/api';
import type { RouteShapeCollection } from '../types/map';

export function useRouteShapes(routeId: string) {
  return useQuery<RouteShapeCollection>({
    queryKey: ['routeShape', routeId],
    queryFn: () => routesApi.getShape(routeId),
    enabled: !!routeId,
    staleTime: 1_800_000,
  });
}

export function useAllRouteShapes(routeIds: string[]) {
  return useQuery<RouteShapeCollection[]>({
    queryKey: ['allRouteShapes', ...routeIds],
    queryFn: async () => {
      const results = await Promise.all(
        routeIds.map((id) => routesApi.getShape(id))
      );
      return results;
    },
    enabled: routeIds.length > 0,
    staleTime: 1_800_000,
  });
}
