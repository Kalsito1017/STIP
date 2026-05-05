import { motion } from 'motion/react';
import type { LucideIcon } from 'lucide-react';

interface EmptyStateProps {
  icon?: LucideIcon;
  title: string;
  description?: string;
  action?: React.ReactNode;
}

function EmptyIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 120 80" fill="none" xmlns="http://www.w3.org/2000/svg">
      <rect x="8" y="12" width="44" height="56" rx="6" stroke="currentColor" strokeWidth="1.5" opacity="0.3" />
      <rect x="14" y="20" width="32" height="4" rx="2" fill="currentColor" opacity="0.15" />
      <rect x="14" y="28" width="24" height="3" rx="1.5" fill="currentColor" opacity="0.1" />
      <rect x="14" y="34" width="28" height="3" rx="1.5" fill="currentColor" opacity="0.1" />
      <rect x="14" y="40" width="20" height="3" rx="1.5" fill="currentColor" opacity="0.1" />
      <circle cx="82" cy="28" r="16" stroke="currentColor" strokeWidth="1.5" opacity="0.25" />
      <path d="M82 16v4M82 36v4M70 28h4M90 28h4" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" opacity="0.2" />
      <circle cx="82" cy="28" r="5" fill="currentColor" opacity="0.15" />
    </svg>
  );
}

export function EmptyState({ icon: Icon, title, description, action }: EmptyStateProps) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3, ease: 'easeOut' }}
      className="flex flex-col items-center justify-center py-12 sm:py-16 px-4 text-center"
    >
      {Icon ? (
        <Icon className="w-12 h-12 sm:w-16 sm:h-16 text-muted-foreground/30 mb-4" strokeWidth={1} />
      ) : (
        <EmptyIcon className="w-24 h-16 sm:w-32 sm:h-20 text-muted-foreground/25 mb-4" />
      )}
      <h3 className="text-base sm:text-lg font-semibold text-foreground/60 mb-1">{title}</h3>
      {description && (
        <p className="text-sm text-muted-foreground/60 max-w-sm">{description}</p>
      )}
      {action && <div className="mt-4">{action}</div>}
    </motion.div>
  );
}
