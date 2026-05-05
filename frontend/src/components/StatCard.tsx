import type { LucideIcon } from 'lucide-react';
import { motion } from 'motion/react';
import { useCountUp } from '../hooks/useCountUp';

interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: LucideIcon;
  iconColor?: string;
  trend?: 'up' | 'down';
  animate?: boolean;
}

export function StatCard({ title, value, subtitle, icon: Icon, iconColor, trend, animate }: StatCardProps) {
  const numValue = typeof value === 'number' ? value : parseFloat(String(value));
  const animated = animate && !Number.isNaN(numValue) ? useCountUp(numValue, 800, animate) : null;
  const display = animated != null ? animated : value;

  return (
    <motion.div
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3, ease: 'easeOut' }}
      className="bg-card rounded-lg border border-border p-3 sm:p-4 shadow-sm"
    >
      <div className="flex items-center justify-between mb-2">
        <span className="text-xs sm:text-sm text-muted-foreground">{title}</span>
        <Icon className="w-4 h-4 sm:w-5 sm:h-5" style={iconColor ? { color: iconColor } : undefined} />
      </div>
      <div className="flex items-baseline gap-2">
        <span className="text-xl sm:text-2xl font-bold text-foreground">{display}</span>
        {trend && (
          <span className={`text-xs ${trend === 'up' ? 'text-green-500' : 'text-red-500'}`}>
            {trend === 'up' ? '\u25B2' : '\u25BC'}
          </span>
        )}
      </div>
      {subtitle && <p className="text-xs text-muted-foreground mt-1">{subtitle}</p>}
    </motion.div>
  );
}
