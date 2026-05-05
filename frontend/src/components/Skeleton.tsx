import { clsx } from 'clsx';
import type { CSSProperties } from 'react';

interface SkeletonProps {
  className?: string;
  style?: CSSProperties;
  shimmer?: boolean;
}

export function Skeleton({ className, style, shimmer = true }: SkeletonProps) {
  return (
    <div
      className={clsx(
        shimmer ? 'animate-shimmer' : 'animate-pulse bg-muted',
        'rounded',
        className
      )}
      style={style}
    />
  );
}

export function SkeletonCard() {
  return (
    <div className="bg-card border border-border rounded-lg p-3 sm:p-4 shadow-sm space-y-3">
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
    <div className="bg-card border border-border rounded-lg shadow-sm overflow-hidden">
      <div className="p-3 border-b border-border bg-muted/50 flex gap-4">
        {Array.from({ length: cols }).map((_, i) => (
          <Skeleton key={i} className="h-3 flex-1" />
        ))}
      </div>
      {Array.from({ length: rows }).map((_, row) => (
        <div key={row} className="p-3 border-b border-border flex gap-4">
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
    <div className="bg-card border border-border rounded-lg p-4 sm:p-5 shadow-sm">
      <Skeleton className="h-4 w-24 mb-6" />
      <div className="flex items-end gap-2" style={{ height }}>
        {[50, 70, 40, 90, 60, 30, 80, 55, 75, 45].map((pct, i) => (
          <Skeleton key={i} className="flex-1" style={{ height: `${pct}%` }} />
        ))}
      </div>
    </div>
  );
}

export function SkeletonRankingList({ rows = 5 }: { rows?: number }) {
  return (
    <div className="space-y-2">
      {Array.from({ length: rows }).map((_, i) => (
        <div key={i} className="flex items-center gap-2 sm:gap-3 text-xs sm:text-sm">
          <Skeleton className="w-5 sm:w-6 h-3 flex-shrink-0" />
          <Skeleton className="w-14 sm:w-16 h-3 flex-shrink-0" />
          <div className="flex-1 bg-muted rounded-full h-2 animate-shimmer" />
          <Skeleton className="w-10 sm:w-12 h-3 flex-shrink-0" />
          <Skeleton className="w-14 sm:w-16 h-3 flex-shrink-0" />
        </div>
      ))}
    </div>
  );
}

export function SkeletonPageCard() {
  return (
    <div className="space-y-4 sm:space-y-6">
      <div className="flex items-center gap-3">
        <Skeleton className="h-8 w-8 rounded-md" />
        <Skeleton className="h-6 w-40" />
      </div>
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 sm:gap-6">
        <Skeleton className="h-48 lg:col-span-2" />
        <Skeleton className="h-48" />
      </div>
    </div>
  );
}
