import { Outlet, useLocation } from 'react-router-dom';
import { Sidebar } from './Sidebar';
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
  const pageTitle = getPageTitle(location.pathname);

  return (
    <div className="min-h-screen bg-slate-50">
      <Sidebar />
      <div className="ml-56">
        <header className="bg-white border-b border-slate-200 px-6 py-3 flex items-center justify-between">
          <h1 className="text-lg font-semibold text-slate-900">{pageTitle}</h1>
          {user && (
            <span className="text-sm text-slate-500">
              {user.fullName}
            </span>
          )}
        </header>
        <main className="p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
