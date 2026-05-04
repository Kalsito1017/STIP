import { TransitTypeName, TransitTypeBadgeClass } from "@/constants/transit";
import { Badge } from "@/components/ui/badge";

interface RouteBadgeProps {
  type: number | null | undefined;
  className?: string;
}

export function RouteBadge({ type, className }: RouteBadgeProps) {
  if (type == null) {
    return <Badge variant="secondary" className={className}>Unknown</Badge>;
  }

  const name = TransitTypeName[type] ?? `Type ${type}`;
  const colorClass = TransitTypeBadgeClass[type] ?? "bg-slate-100 text-slate-800";

  return (
    <Badge variant="secondary" className={`${colorClass} ${className ?? ""}`}>
      {name}
    </Badge>
  );
}
