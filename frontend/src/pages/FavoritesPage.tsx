import { useNavigate } from 'react-router-dom';
import { Star, Map, Bus, MapPin, Trash2 } from 'lucide-react';
import { motion } from 'motion/react';
import { useFavorites, useRemoveFavorite } from '../hooks/useFavorites';
import { TransitTypeRouteColor } from '../constants/transit';
import { useAppStore } from '../store/useAppStore';
import { EmptyState } from '../components/EmptyState';
import { Skeleton } from '../components/Skeleton';
import { useTranslation } from 'react-i18next';

function getRouteTypeFromId(routeId: string): number {
  if (routeId.includes('-tram-')) return 0;
  if (routeId.startsWith('r-m')) return 1;
  if (routeId.includes('-trol-')) return 11;
  return 3;
}

export function FavoritesPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const setFlyToTarget = useAppStore((s) => s.setFlyToTarget);
  const isAuthenticated = useAppStore((s) => s.isAuthenticated);
  const { data: favorites, isLoading } = useFavorites();
  const removeFav = useRemoveFavorite();

  const routeFavorites = favorites?.filter((f) => f.entityType === 'route') ?? [];
  const stopFavorites = favorites?.filter((f) => f.entityType === 'stop') ?? [];

  if (!isAuthenticated) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-bold text-foreground">{t('layout:favorites', { defaultValue: 'Favorites' })}</h1>
        <EmptyState
          icon={Star}
          title="Sign in required"
          description="Sign in to save your favorite routes and stops."
        />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-foreground">{t('layout:favorites', { defaultValue: 'Favorites' })}</h1>

      {isLoading ? (
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-16 w-full rounded-xl" />
          ))}
        </div>
      ) : favorites && favorites.length === 0 ? (
        <EmptyState
          icon={Star}
          title="No favorites yet"
          description="Tap the star on any route or stop to save it here."
        />
      ) : (
        <>
          {routeFavorites.length > 0 && (
            <div>
              <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider mb-3">Saved Routes</h2>
              <div className="space-y-2">
                {routeFavorites.map((fav) => {
                  const routeType = getRouteTypeFromId(fav.entityId);
                  const color = TransitTypeRouteColor[routeType] ?? '#64748b';
                  return (
                    <motion.div
                      key={fav.id}
                      layout
                      initial={{ opacity: 0, y: 4 }}
                      animate={{ opacity: 1, y: 0 }}
                      className="flex items-center gap-3 bg-card border border-border rounded-xl px-4 py-3 shadow-sm hover:shadow-md transition-shadow"
                    >
                      <div
                        className="w-8 h-8 rounded-full flex items-center justify-center text-white text-xs font-bold flex-shrink-0"
                        style={{ backgroundColor: color }}
                      >
                        <Bus className="w-4 h-4" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-foreground truncate">{fav.entityId}</p>
                      </div>
                      <button
                        onClick={() => {
                          setFlyToTarget({ lat: 42.6977, lon: 23.3219, zoom: 14 });
                          navigate('/');
                        }}
                        className="p-1.5 rounded-lg hover:bg-accent text-muted-foreground hover:text-foreground transition-colors"
                        aria-label="View on map"
                      >
                        <Map className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => removeFav.mutate(fav.id)}
                        className="p-1.5 rounded-lg hover:bg-destructive/10 text-muted-foreground hover:text-destructive transition-colors"
                        aria-label="Remove favorite"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </motion.div>
                  );
                })}
              </div>
            </div>
          )}

          {stopFavorites.length > 0 && (
            <div>
              <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider mb-3">Saved Stops</h2>
              <div className="space-y-2">
                {stopFavorites.map((fav) => (
                  <motion.div
                    key={fav.id}
                    layout
                    initial={{ opacity: 0, y: 4 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="flex items-center gap-3 bg-card border border-border rounded-xl px-4 py-3 shadow-sm hover:shadow-md transition-shadow"
                  >
                    <div className="w-8 h-8 rounded-full bg-red-500 flex items-center justify-center text-white flex-shrink-0">
                      <MapPin className="w-4 h-4" />
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-foreground truncate">{fav.entityId}</p>
                    </div>
                    <button
                      onClick={() => navigate(`/stops/${fav.entityId}`)}
                      className="p-1.5 rounded-lg hover:bg-accent text-muted-foreground hover:text-foreground transition-colors"
                      aria-label="View stop details"
                    >
                      <Map className="w-4 h-4" />
                    </button>
                    <button
                      onClick={() => removeFav.mutate(fav.id)}
                      className="p-1.5 rounded-lg hover:bg-destructive/10 text-muted-foreground hover:text-destructive transition-colors"
                      aria-label="Remove favorite"
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </motion.div>
                ))}
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
