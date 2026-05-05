import { useTranslation } from 'react-i18next';
import { TransitTypeBadgeClass } from "@/constants/transit";
import { Badge } from "@/components/ui/badge";

interface RouteBadgeProps {
  type: number | null | undefined;
  className?: string;
}

export function RouteBadge({ type, className }: RouteBadgeProps) {
  const { t } = useTranslation('transit');
  if (type == null) {
    return <Badge variant="secondary" className={className}>{t('unknown', { ns: 'common' })}</Badge>;
  }

  const transitNames: Record<number, string> = {
    0: t('tram'),
    1: t('metro'),
    3: t('bus'),
    11: t('trolley'),
  };

  const name = transitNames[type] ?? `Type ${type}`;
  const colorClass = TransitTypeBadgeClass[type] ?? "bg-slate-100 text-slate-800";

  return (
    <Badge variant="secondary" className={`${colorClass} ${className ?? ""}`}>
      {name}
    </Badge>
  );
}
