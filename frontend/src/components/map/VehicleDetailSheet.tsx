import { motion, AnimatePresence } from 'motion/react';
import { X, Navigation, Gauge, Clock } from 'lucide-react';
import { useAppStore, type Vehicle } from '../../store/useAppStore';
import { TransitTypeRouteColor } from '../../constants/transit';
import { useTranslation } from 'react-i18next';
import { getLocale } from '../../lib/utils';

function getRouteType(routeId: string | null): number | undefined {
  if (!routeId) return undefined;
  if (routeId.includes('-tram-')) return 0;
  if (routeId.startsWith('r-m')) return 1;
  if (routeId.includes('-trol-')) return 11;
  return 3;
}

function bearingToCardinal(b: number): string {
  const dirs = ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW'];
  return dirs[Math.round(b / 45) % 8];
}

interface Props {
  routeNames: Record<string, string>;
}

export function VehicleDetailSheet({ routeNames }: Props) {
  const { t } = useTranslation('map');
  const { t: tCommon } = useTranslation('common');
  const selectedVehicle = useAppStore((s) => s.selectedVehicle);
  const setSelectedVehicle = useAppStore((s) => s.setSelectedVehicle);

  const vehicle: Vehicle | null = selectedVehicle;

  const routeType = getRouteType(vehicle?.routeId ?? null);
  const color = routeType != null ? (TransitTypeRouteColor[routeType] ?? '#3b82f6') : '#3b82f6';
  const displayRoute = vehicle?.routeId ? (routeNames[vehicle.routeId] ?? vehicle.routeId) : null;
  const direction = vehicle != null ? bearingToCardinal(vehicle.bearing) : '';

  return (
    <AnimatePresence>
      {vehicle && (
        <motion.div
          initial={{ y: '100%' }}
          animate={{ y: 0 }}
          exit={{ y: '100%' }}
          transition={{ type: 'spring', damping: 30, stiffness: 300 }}
          className="absolute bottom-0 left-0 right-0 z-[1100] pointer-events-auto pb-20 lg:pb-2"
        >
          <div
            className="mx-2 sm:mx-4 mb-2 sm:mb-4 bg-card border border-border rounded-2xl shadow-2xl overflow-hidden"
            style={{ borderTopWidth: '4px', borderTopColor: color }}
          >
            <div className="p-4 sm:p-5">
              <div className="flex items-start justify-between gap-3 mb-4">
                <div className="flex items-center gap-3 min-w-0">
                  <div
                    className="w-10 h-10 sm:w-12 sm:h-12 rounded-full flex items-center justify-center text-white text-lg font-bold flex-shrink-0"
                    style={{ backgroundColor: color }}
                  >
                    {String.fromCodePoint(0x1F68C)}
                  </div>
                  <div className="min-w-0">
                    <h2 className="text-lg sm:text-xl font-bold text-foreground truncate">
                      {displayRoute ?? tCommon('unknown_route')}
                    </h2>
                    <p className="text-xs sm:text-sm text-muted-foreground truncate">
                      {vehicle.vehicleId}
                    </p>
                  </div>
                </div>
                <button
                  onClick={() => setSelectedVehicle(null)}
                  className="p-1.5 rounded-lg hover:bg-secondary text-muted-foreground hover:text-foreground transition-colors flex-shrink-0"
                  aria-label={t('close_details')}
                >
                  <X className="w-5 h-5" />
                </button>
              </div>

              <div className="grid grid-cols-3 gap-3 sm:gap-4">
                <div className="bg-secondary rounded-xl p-3 text-center">
                  <Gauge className="w-4 h-4 text-muted-foreground mx-auto mb-1" />
                  <p className="text-lg sm:text-xl font-bold text-foreground">
                    {vehicle.speed.toFixed(0)}
                  </p>
                  <p className="text-[10px] sm:text-xs text-muted-foreground">{tCommon('km_h')}</p>
                </div>
                <div className="bg-secondary rounded-xl p-3 text-center">
                  <Navigation
                    className="w-4 h-4 mx-auto mb-1"
                    style={{ color, transform: `rotate(${vehicle.bearing}deg)` }}
                  />
                  <p className="text-lg sm:text-xl font-bold text-foreground">
                    {vehicle.bearing}&deg;
                  </p>
                  <p className="text-[10px] sm:text-xs text-muted-foreground">{direction}</p>
                </div>
                <div className="bg-secondary rounded-xl p-3 text-center">
                  <Clock className="w-4 h-4 text-muted-foreground mx-auto mb-1" />
                  <p className="text-xs sm:text-sm font-semibold text-foreground">
                    {new Date(vehicle.recordedAt).toLocaleTimeString(getLocale())}
                  </p>
                  <p className="text-[10px] sm:text-xs text-muted-foreground">{t('updated_label')}</p>
                </div>
              </div>

              {vehicle.tripId && (
                <div className="mt-3 pt-3 border-t border-border">
                  <p className="text-xs text-muted-foreground">
                    {tCommon('trip_label')} <span className="text-foreground font-mono">{vehicle.tripId}</span>
                  </p>
                </div>
              )}
            </div>
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}
