import { Loader2, Check } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface Props {
  visible: boolean;
  layers?: { label: string; loaded: boolean }[];
}

export function MapLoadingOverlay({ visible, layers }: Props) {
  const { t } = useTranslation('map');
  if (!visible) return null;

  const loadedCount = layers?.filter((l) => l.loaded).length ?? 0;
  const totalCount = layers?.length ?? 0;

  return (
    <div className="absolute top-0 left-0 right-0 z-[1001] pointer-events-none">
      <div className="mx-2 sm:mx-3 mt-2 sm:mt-3 bg-card/95 backdrop-blur-md border border-border/60 rounded-xl shadow-lg overflow-hidden max-w-sm">
        <div className="flex items-center gap-3 px-4 py-2.5">
          <Loader2 className="w-4 h-4 text-primary animate-spin flex-shrink-0" />
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-foreground truncate">{t('loading_map_data')}</p>
            {totalCount > 0 && (
              <p className="text-xs text-muted-foreground">
                {loadedCount}/{totalCount} {t('loading_layers', { defaultValue: 'layers loaded' })}
              </p>
            )}
          </div>
        </div>

        {layers && layers.length > 0 && (
          <div className="px-4 pb-2.5 flex flex-wrap gap-1.5">
            {layers.map((layer) => (
              <span
                key={layer.label}
                className={`inline-flex items-center gap-1 text-[10px] font-medium px-2 py-0.5 rounded-full transition-colors ${
                  layer.loaded
                    ? 'bg-green-50 text-green-700 dark:bg-green-900/30 dark:text-green-400'
                    : 'bg-muted text-muted-foreground'
                }`}
              >
                {layer.loaded ? <Check className="w-2.5 h-2.5" /> : <Loader2 className="w-2.5 h-2.5 animate-spin" />}
                {layer.label}
              </span>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
