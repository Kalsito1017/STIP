import { Star } from 'lucide-react';
import { useIsFavorite, useGetFavoriteId, useAddFavorite, useRemoveFavorite } from '../hooks/useFavorites';
import { useAppStore } from '../store/useAppStore';
import { toast } from 'sonner';

interface Props {
  entityType: 'route' | 'stop';
  entityId: string;
  size?: 'sm' | 'md';
}

export function FavoriteButton({ entityType, entityId, size = 'md' }: Props) {
  const isAuthenticated = useAppStore((s) => s.isAuthenticated);
  const isFav = useIsFavorite(entityType, entityId);
  const favId = useGetFavoriteId(entityType, entityId);
  const addFav = useAddFavorite();
  const removeFav = useRemoveFavorite();

  const iconSize = size === 'sm' ? 'w-4 h-4' : 'w-5 h-5';

  const handleClick = (e: React.MouseEvent) => {
    e.stopPropagation();
    e.preventDefault();

    if (!isAuthenticated) {
      toast.error('Sign in to save favorites');
      return;
    }

    if (isFav && favId !== undefined) {
      removeFav.mutate(favId);
    } else {
      addFav.mutate({ entityType, entityId });
    }
  };

  return (
    <button
      onClick={handleClick}
      className={`p-1.5 rounded-lg transition-colors ${
        isFav
          ? 'text-amber-500 hover:text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20'
          : 'text-muted-foreground hover:text-amber-500 hover:bg-accent'
      }`}
      aria-label={isFav ? 'Remove from favorites' : 'Add to favorites'}
    >
      <Star className={`${iconSize} ${isFav ? 'fill-current' : ''}`} />
    </button>
  );
}
