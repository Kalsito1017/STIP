import { useState, useCallback, useEffect, Suspense } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { motion, AnimatePresence } from 'motion/react';
import { clsx } from 'clsx';
import { Menu, Wifi, WifiOff } from 'lucide-react';
import { Sidebar } from './Sidebar';
import { ErrorBoundary } from './ErrorBoundary';
import { Button } from './ui/button';
import { useAppStore } from '../store/useAppStore';
import { SkeletonPageCard } from './Skeleton';
import { MobileTabBar } from './MobileTabBar';
import { useTranslation } from 'react-i18next';

const PAGE_TITLE_KEYS: Record<string, string> = {
  '/dashboard': 'layout.dashboard',
  '/routes': 'layout.routes',
  '/stops': 'layout.stops',
  '/analytics': 'layout.analytics',
};

export function Layout() {
  const { t } = useTranslation();
  const location = useLocation();
  const user = useAppStore((s) => s.user);
  const connectionState = useAppStore((s) => s.connectionState);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const getPageTitle = (pathname: string): string => {
    if (PAGE_TITLE_KEYS[pathname]) return t(PAGE_TITLE_KEYS[pathname]);
    if (pathname.startsWith('/routes/')) return t('layout.route_detail');
    if (pathname.startsWith('/stops/')) return t('layout.stop_detail');
    if (pathname.startsWith('/settings')) return t('layout.settings');
    return t('common.appName');
  };

  const pageTitle = getPageTitle(location.pathname);

  useEffect(() => {
    setSidebarOpen(false);
  }, [location.pathname]);

  const handleClose = useCallback(() => setSidebarOpen(false), []);

  const connectionLabel =
    connectionState === 'connected' ? t('layout.connected') :
    connectionState === 'reconnecting' ? t('layout.reconnecting') : t('layout.disconnected');

  return (
    <div className="min-h-screen bg-background">
      <Sidebar open={sidebarOpen} onClose={handleClose} />

      {sidebarOpen && (
        <div
          className="fixed inset-0 z-20 bg-black/50 lg:hidden"
          onClick={handleClose}
          aria-hidden="true"
        />
      )}

      <div className="lg:ml-56">
        <header className="bg-card border-b border-border px-4 sm:px-6 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-3 min-w-0">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setSidebarOpen(true)}
              className="lg:hidden"
              aria-label={t('layout.open_menu')}
            >
              <Menu className="w-5 h-5" />
            </Button>
            <h1 className="text-base sm:text-lg font-semibold text-foreground truncate">
              {pageTitle}
            </h1>
          </div>
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-1.5">
              {connectionState === 'connected' ? (
                <Wifi className="w-3.5 h-3.5 text-green-500" />
              ) : (
                <WifiOff className="w-3.5 h-3.5 text-red-500" />
              )}
              <span className={clsx(
                'text-xs font-medium',
                connectionState === 'connected' && 'text-green-600',
                connectionState === 'reconnecting' && 'text-yellow-600',
                connectionState === 'disconnected' && 'text-red-600',
              )}>
                {connectionLabel}
              </span>
            </div>
            {user && (
              <span className="text-xs sm:text-sm text-muted-foreground flex-shrink-0 hidden sm:inline">
                {user.fullName}
              </span>
            )}
            {user && (
              <span className="text-xs text-muted-foreground flex-shrink-0 sm:hidden">
                {user.fullName.split(' ')[0]}
              </span>
            )}
          </div>
        </header>
        <main className="p-4 sm:p-6 pb-20 lg:pb-6">
          <ErrorBoundary>
            <Suspense fallback={<SkeletonPageCard />}>
              <AnimatePresence mode="wait">
                <motion.div
                  key={location.pathname}
                  initial={{ opacity: 0, y: 8 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -8 }}
                  transition={{ duration: 0.15, ease: "easeOut" }}
                >
                  <Outlet />
                </motion.div>
              </AnimatePresence>
            </Suspense>
          </ErrorBoundary>
        </main>
      </div>

      <MobileTabBar />
    </div>
  );
}
