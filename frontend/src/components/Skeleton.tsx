import { clsx } from 'clsx';
import type { CSSProperties } from 'react';

interface SkeletonProps {
  className?: string;
  style?: CSSProperties;
}

export function Skeleton({ className, style }: SkeletonProps) {
  return <div className={clsx('animate-pulse bg-slate-200 rounded', className)} style={style} />;
}

export function SkeletonCard() {
  return (
    <div className="bg-white border border-slate-200 rounded-lg p-3 sm:p-4 shadow-sm space-y-3">
      <div className="flex items-center justify-between">
        <Skeleton className="h-3 w-16" />
        <Skeleton className="h-4 w-4" />
      </div>
      <Skeleton className="h-7 w-20" />
      <Skeleton className="h-3 w-24" />
    </div>
  );
}

export function SkeletonTable({ rows = 5, cols = 3 }: { rows?: number; cols?: number }) {
  return (
    <div className="bg-white border border-slate-200 rounded-lg shadow-sm overflow-hidden">
      <div className="p-3 border-b border-slate-200 bg-slate-50 flex gap-4">
        {Array.from({ length: cols }).map((_, i) => (
          <Skeleton key={i} className="h-3 flex-1" />
        ))}
      </div>
      {Array.from({ length: rows }).map((_, row) => (
        <div key={row} className="p-3 border-b border-slate-100 flex gap-4">
          {Array.from({ length: cols }).map((_, col) => (
            <Skeleton key={col} className="h-3 flex-1" />
          ))}
        </div>
      ))}
    </div>
  );
}

export function SkeletonChart({ height = 250 }: { height?: number }) {
  return (
    <div className="bg-white border border-slate-200 rounded-lg p-4 sm:p-5 shadow-sm">
      <Skeleton className="h-4 w-24 mb-6" />
      <div className="flex items-end gap-2" style={{ height }}>
        {[50, 70, 40, 90, 60, 30, 80, 55, 75, 45].map((pct, i) => (
          <Skeleton key={i} className="flex-1" style={{ height: `${pct}%` }} />
        ))}
      </div>
    </div>
  );
}
