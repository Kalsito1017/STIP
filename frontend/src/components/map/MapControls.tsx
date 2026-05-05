import { useState } from 'react';
import { motion, AnimatePresence } from 'motion/react';
import { Layers, Bus, MapPin, Thermometer, Car, Crosshair, Radio } from 'lucide-react';
import { Button } from '../ui/button';
import { useTranslation } from 'react-i18next';

interface Props {
  clusterMode: boolean;
  onToggleCluster: () => void;
  showRoutes: boolean;
  showStops: boolean;
  showHeatmap: boolean;
  showVehicles: boolean;
  showCongestion: boolean;
  showNearby: boolean;
  onToggleRoutes: () => void;
  onToggleStops: () => void;
  onToggleHeatmap: () => void;
  onToggleVehicles: () => void;
  onToggleCongestion: () => void;
  onToggleNearby: () => void;
  onLocate: () => void;
}

export function MapControls({
  clusterMode,
  onToggleCluster,
  showRoutes,
  showStops,
  showHeatmap,
  showVehicles,
  showCongestion,
  showNearby,
  onToggleRoutes,
  onToggleStops,
  onToggleHeatmap,
  onToggleVehicles,
  onToggleCongestion,
  onToggleNearby,
  onLocate,
}: Props) {
  const { t } = useTranslation('map');
  const [expanded, setExpanded] = useState(false);

  return (
    <div className="absolute bottom-20 lg:bottom-6 right-3 sm:right-4 z-[1000] pointer-events-auto flex flex-col gap-2">
      <AnimatePresence>
        {expanded && (
          <motion.div
            initial={{ opacity: 0, scale: 0.9, y: 8 }}
            animate={{ opacity: 1, scale: 1, y: 0 }}
            exit={{ opacity: 0, scale: 0.9, y: 8 }}
            transition={{ duration: 0.15, ease: 'easeOut' }}
            className="bg-card border border-border rounded-2xl shadow-lg p-3 w-48"
          >
            <p className="text-[10px] font-semibold text-muted-foreground uppercase tracking-wider mb-2 px-1">
              {t('map_layers')}
            </p>
            <div className="space-y-0.5">
              <ToggleRow icon={Car} label={t('vehicles')} checked={showVehicles} onChange={onToggleVehicles} />
              <ToggleRow icon={Bus} label={t('route_shapes')} checked={showRoutes} onChange={onToggleRoutes} />
              <ToggleRow icon={MapPin} label={t('stops_label')} checked={showStops} onChange={onToggleStops} />
              <ToggleRow icon={Thermometer} label={t('delay_heatmap')} checked={showHeatmap} onChange={onToggleHeatmap} />
              <ToggleRow icon={Radio} label={t('stop_congestion')} checked={showCongestion} onChange={onToggleCongestion} />
            </div>
            <div className="pt-2 mt-2 border-t border-border">
              <ToggleRow
                icon={Layers}
                label={clusterMode ? t('cluster_markers') : t('individual_markers')}
                checked={clusterMode}
                onChange={onToggleCluster}
              />
              <ToggleRow
                icon={MapPin}
                label={t('nearby_stops')}
                checked={showNearby}
                onChange={onToggleNearby}
              />
            </div>
          </motion.div>
        )}
      </AnimatePresence>

      <div className="flex flex-col gap-2">
        <Button
          variant="outline"
          size="icon"
          onClick={onLocate}
          className="bg-card shadow-lg border-border rounded-full h-10 w-10"
          aria-label={t('locate_me')}
          title={t('show_location')}
        >
          <Crosshair className="w-4 h-4" />
        </Button>
        <Button
          variant="outline"
          size="icon"
          onClick={() => setExpanded((v) => !v)}
          className={`bg-card shadow-lg border-border rounded-full h-10 w-10 ${expanded ? 'ring-2 ring-primary' : ''}`}
          aria-label={t('toggle_layers')}
          title={t('toggle_layers')}
        >
          <Layers className="w-4 h-4" />
        </Button>
      </div>
    </div>
  );
}

function ToggleRow({ icon: Icon, label, checked, onChange }: {
  icon: React.ComponentType<{ className?: string }>;
  label: string;
  checked: boolean;
  onChange: () => void;
}) {
  return (
    <label className="flex items-center gap-2.5 py-1.5 px-1 cursor-pointer text-sm text-foreground hover:bg-secondary rounded-md transition-colors">
      <input
        type="checkbox"
        checked={checked}
        onChange={onChange}
        className="rounded border-input text-primary focus:ring-primary"
      />
      <Icon className="w-3.5 h-3.5 text-muted-foreground flex-shrink-0" />
      <span className="truncate">{label}</span>
    </label>
  );
}
