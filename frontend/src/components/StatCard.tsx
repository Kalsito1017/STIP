import type { LucideIcon } from 'lucide-react';

interface StatCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: LucideIcon;
  trend?: 'up' | 'down';
}

export function StatCard({ title, value, subtitle, icon: Icon, trend }: StatCardProps) {
  return (
    <div className="bg-white rounded-lg border border-slate-200 p-4 shadow-sm">
      <div className="flex items-center justify-between mb-2">
        <span className="text-sm text-slate-500">{title}</span>
        <Icon className="w-5 h-5 text-slate-400" />
      </div>
      <div className="flex items-baseline gap-2">
        <span className="text-2xl font-bold text-slate-900">{value}</span>
        {trend && (
          <span className={`text-xs ${trend === 'up' ? 'text-green-600' : 'text-red-600'}`}>
            {trend === 'up' ? '▲' : '▼'}
          </span>
        )}
      </div>
      {subtitle && <p className="text-xs text-slate-400 mt-1">{subtitle}</p>}
    </div>
  );
}
