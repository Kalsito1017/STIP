import { Loader2 } from 'lucide-react';
import { Skeleton } from './Skeleton';
import { useTranslation } from 'react-i18next';

interface Props {
  visible: boolean;
  layers?: { label: string; loaded: boolean }[];
}

export function MapLoadingOverlay({ visible, layers }: Props) {
  const { t } = useTranslation('map');
  if (!visible) return null;

  return (
    <div className="absolute inset-0 z-[1001] flex items-center justify-center bg-white/60 backdrop-blur-sm">
      <div className="bg-white border border-slate-200 rounded-xl shadow-lg p-6 sm:p-8 w-80 max-w-[90vw]">
        <div className="flex items-center gap-3 mb-5">
          <Loader2 className="w-5 h-5 text-blue-500 animate-spin" />
          <h2 className="text-base font-semibold text-slate-800">{t('loading_map_data')}</h2>
        </div>

        {layers && layers.length > 0 && (
          <div className="space-y-2.5">
            {layers.map((layer) => (
              <div key={layer.label} className="flex items-center gap-3">
                <div className="w-4 h-4 flex-shrink-0">
                  {layer.loaded ? (
                    <svg className="w-4 h-4 text-green-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                    </svg>
                  ) : (
                    <Loader2 className="w-4 h-4 text-slate-400 animate-spin" />
                  )}
                </div>
                <span className={`text-sm ${layer.loaded ? 'text-green-700' : 'text-slate-500'}`}>
                  {layer.label}
                </span>
                {!layer.loaded && (
                  <Skeleton className="h-3 w-20 ml-auto" />
                )}
              </div>
            ))}
          </div>
        )}

        {(!layers || layers.length === 0) && (
          <div className="space-y-3">
            <Skeleton className="h-3 w-full" />
            <Skeleton className="h-3 w-3/4" />
            <Skeleton className="h-3 w-5/6" />
          </div>
        )}
      </div>
    </div>
  );
}
