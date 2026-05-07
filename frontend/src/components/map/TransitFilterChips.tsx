import { Bus, TramFront, Train, Zap } from 'lucide-react';
import { TransitType, TransitTypeRouteColor } from '../../constants/transit';
import { useTranslation } from 'react-i18next';

interface Props {
  activeTypes: Set<number>;
  onToggle: (type: number) => void;
}

const transitTypes = [
  { type: TransitType.Bus, icon: Bus },
  { type: TransitType.Tram, icon: TramFront },
  { type: TransitType.Metro, icon: Train },
  { type: TransitType.Trolley, icon: Zap },
];

const activeBadge: Record<number, string> = {
  [TransitType.Bus]: 'bg-green-100 border-green-300 text-green-800 dark:bg-green-900/40 dark:border-green-700 dark:text-green-300',
  [TransitType.Tram]: 'bg-amber-100 border-amber-300 text-amber-800 dark:bg-amber-900/40 dark:border-amber-700 dark:text-amber-300',
  [TransitType.Metro]: 'bg-blue-100 border-blue-300 text-blue-800 dark:bg-blue-900/40 dark:border-blue-700 dark:text-blue-300',
  [TransitType.Trolley]: 'bg-purple-100 border-purple-300 text-purple-800 dark:bg-purple-900/40 dark:border-purple-700 dark:text-purple-300',
};

export function TransitFilterChips({ activeTypes, onToggle }: Props) {
  const { t } = useTranslation('transit');

  return (
    <div className="flex flex-wrap gap-1.5">
      {transitTypes.map(({ type, icon: Icon }) => {
        const isActive = activeTypes.has(type);
        return (
          <button
            key={type}
            onClick={() => onToggle(type)}
            className={`inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-full text-xs font-medium border transition-all duration-150 ${
              isActive
                ? activeBadge[type] ?? 'bg-card border-border text-foreground'
                : 'bg-card/60 border-border/40 text-muted-foreground hover:bg-card/80'
            }`}
          >
            <Icon className="w-3 h-3" style={{ color: isActive ? TransitTypeRouteColor[type] : undefined }} />
            <span>{t(type === TransitType.Tram ? 'tram' : type === TransitType.Metro ? 'metro' : type === TransitType.Trolley ? 'trolley' : 'bus')}</span>
          </button>
        );
      })}
    </div>
  );
}
