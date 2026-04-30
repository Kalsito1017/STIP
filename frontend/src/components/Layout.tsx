import { useState, useCallback, useEffect } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { clsx } from 'clsx';
import { Menu, Wifi, WifiOff } from 'lucide-react';
import { Sidebar } from './Sidebar';
import { ErrorBoundary } from './ErrorBoundary';
import { useAppStore } from '../store/useAppStore';

const pageTitles: Record<string, string> = {
  '/map': 'Live Map',
  '/dashboard': 'Dashboard',
  '/routes': 'Routes',
  '/stops': 'Stops',
  '/analytics': 'Analytics',
};

function getPageTitle(pathname: string): string {
  if (pageTitles[pathname]) return pageTitles[pathname];
  if (pathname.startsWith('/routes/')) return 'Route Detail';
  if (pathname.startsWith('/stops/')) return 'Stop Detail';
  return 'STIP';
}

export function Layout() {
  const location = useLocation();
  const user = useAppStore((s) => s.user);
  const connectionState = useAppStore((s) => s.connectionState);
  const pageTitle = getPageTitle(location.pathname);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Close sidebar on route change (mobile)
  useEffect(() => {
    setSidebarOpen(false);
  }, [location.pathname]);

  const handleClose = useCallback(() => setSidebarOpen(false), []);

  const connectionLabel =
    connectionState === 'connected' ? 'Live' :
    connectionState === 'reconnecting' ? 'Reconnecting...' : 'Offline';

  return (
    <div className="min-h-screen bg-slate-50">
      <Sidebar open={sidebarOpen} onClose={handleClose} />

      {/* Mobile/tablet sidebar backdrop */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-20 bg-black/50 lg:hidden"
          onClick={handleClose}
          aria-hidden="true"
        />
      )}

      <div className="lg:ml-56">
        <header className="bg-white border-b border-slate-200 px-4 sm:px-6 py-3 flex items-center justify-between gap-3">
          <div className="flex items-center gap-3 min-w-0">
            <button
              onClick={() => setSidebarOpen(true)}
              className="lg:hidden flex items-center justify-center w-8 h-8 rounded-md text-slate-600 hover:bg-slate-100 flex-shrink-0"
              aria-label="Open menu"
            >
              <Menu className="w-5 h-5" />
            </button>
            <h1 className="text-base sm:text-lg font-semibold text-slate-900 truncate">
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
              <span className="text-xs sm:text-sm text-slate-500 flex-shrink-0 hidden sm:inline">
                {user.fullName}
              </span>
            )}
            {user && (
              <span className="text-xs text-slate-500 flex-shrink-0 sm:hidden">
                {user.fullName.split(' ')[0]}
              </span>
            )}
          </div>
        </header>
        <main className="p-4 sm:p-6">
          <ErrorBoundary>
            <Outlet />
          </ErrorBoundary>
        </main>
      </div>
    </div>
  );
}
