import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { favoritesApi, type FavoriteDto } from '../services/api';
import { useAppStore } from '../store/useAppStore';
import { toast } from 'sonner';

export function useFavorites() {
  const isAuthenticated = useAppStore((s) => s.isAuthenticated);
  return useQuery<FavoriteDto[]>({
    queryKey: ['favorites'],
    queryFn: () => favoritesApi.getAll(),
    enabled: isAuthenticated,
  });
}

export function useIsFavorite(entityType: 'route' | 'stop', entityId: string): boolean {
  const { data: favorites } = useFavorites();
  return favorites?.some((f) => f.entityType === entityType && f.entityId === entityId) ?? false;
}

export function useGetFavoriteId(entityType: 'route' | 'stop', entityId: string): number | undefined {
  const { data: favorites } = useFavorites();
  return favorites?.find((f) => f.entityType === entityType && f.entityId === entityId)?.id;
}

export function useAddFavorite() {
  const queryClient = useQueryClient();
  const isAuthenticated = useAppStore((s) => s.isAuthenticated);

  return useMutation({
    mutationFn: (params: { entityType: 'route' | 'stop'; entityId: string }) => {
      if (!isAuthenticated) {
        toast.error('Sign in to save favorites');
        return Promise.reject(new Error('Not authenticated'));
      }
      return favoritesApi.add(params.entityType, params.entityId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['favorites'] });
      toast.success('Added to favorites');
    },
  });
}

export function useRemoveFavorite() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => favoritesApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['favorites'] });
      toast.success('Removed from favorites');
    },
  });
}
