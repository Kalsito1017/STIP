import { useLocation, useNavigate } from 'react-router-dom';
import { Map, LayoutDashboard, Bus, TrendingUp } from 'lucide-react';
import { motion } from 'motion/react';

interface Tab {
  path: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  match: (pathname: string) => boolean;
}

const tabs: Tab[] = [
  {
    path: '/',
    label: 'Map',
    icon: Map,
    match: (p) => p === '/' || p === '/map',
  },
  {
    path: '/dashboard',
    label: 'Dashboard',
    icon: LayoutDashboard,
    match: (p) => p.startsWith('/dashboard'),
  },
  {
    path: '/routes',
    label: 'Routes',
    icon: Bus,
    match: (p) => p.startsWith('/routes'),
  },
  {
    path: '/analytics',
    label: 'Analytics',
    icon: TrendingUp,
    match: (p) => p.startsWith('/analytics'),
  },
];

export function MobileTabBar() {
  const location = useLocation();
  const navigate = useNavigate();

  const activeIndex = tabs.findIndex((t) => t.match(location.pathname));
  const active = activeIndex >= 0 ? activeIndex : 0;

  return (
    <nav className="lg:hidden fixed bottom-0 left-0 right-0 z-[1100] bg-card/95 backdrop-blur-md border-t border-border safe-area-bottom">
      <div className="flex items-center justify-around h-14 max-w-lg mx-auto">
        {tabs.map((tab, i) => {
          const isActive = i === active;
          return (
            <button
              key={tab.path}
              onClick={() => navigate(tab.path)}
              className="relative flex flex-col items-center justify-center gap-0.5 h-full flex-1 tap-highlight-transparent"
              aria-label={tab.label}
              aria-current={isActive ? 'page' : undefined}
            >
              {isActive && (
                <motion.div
                  layoutId="tab-indicator"
                  className="absolute inset-x-2 top-1 bottom-1 bg-primary/10 rounded-xl"
                  transition={{ type: 'spring', stiffness: 400, damping: 30 }}
                />
              )}
              <tab.icon
                className={`w-5 h-5 relative z-10 transition-colors ${
                  isActive ? 'text-primary' : 'text-muted-foreground'
                }`}
              />
              <span
                className={`text-[10px] font-medium relative z-10 transition-colors ${
                  isActive ? 'text-primary' : 'text-muted-foreground'
                }`}
              >
                {tab.label}
              </span>
            </button>
          );
        })}
      </div>
    </nav>
  );
}
