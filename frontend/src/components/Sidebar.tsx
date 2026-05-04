import { NavLink } from 'react-router-dom';
import { Map, LayoutDashboard, Bus, MapPin, TrendingUp, User, LogOut, X } from 'lucide-react';
import { useAppStore } from '../store/useAppStore';
import { useLogout } from '../hooks/useAuth';
import { Button } from './ui/button';

const navItems = [
  { to: '/map', label: 'Live Map', icon: Map },
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/routes', label: 'Routes', icon: Bus },
  { to: '/stops', label: 'Stops', icon: MapPin },
  { to: '/analytics', label: 'Analytics', icon: TrendingUp },
];

interface SidebarProps {
  open: boolean;
  onClose: () => void;
}

export function Sidebar({ open, onClose }: SidebarProps) {
  const user = useAppStore((s) => s.user);
  const logout = useLogout();

  const sidebarContent = (
    <>
      <div className="p-4 border-b border-slate-200 flex items-center justify-between">
        <div>
          <h1 className="text-lg font-bold text-slate-900" title="Sofia Transport Intelligence Platform">STIP</h1>
          <p className="text-xs text-slate-500">Sofia Transport Intelligence</p>
        </div>
        {/* Close button — visible only on mobile/tablet */}
        <Button
          variant="ghost"
          size="icon"
          onClick={onClose}
          className="lg:hidden"
          aria-label="Close menu"
        >
          <X className="w-5 h-5" />
        </Button>
      </div>
      <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
        {navItems.map(({ to, label, icon: Icon }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-blue-50 text-blue-700'
                  : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'
              }`
            }
          >
            <Icon className="w-4 h-4 flex-shrink-0" />
            {label}
          </NavLink>
        ))}
      </nav>

      {user && (
        <div className="border-t border-slate-200 p-3 space-y-3">
          <div className="flex items-center gap-3 px-2">
            <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center flex-shrink-0">
              <User className="w-4 h-4 text-blue-600" />
            </div>
            <div className="min-w-0 flex-1">
              <p className="text-sm font-medium text-slate-900 truncate">{user.fullName}</p>
              <p className="text-xs text-slate-500 truncate">{user.email}</p>
            </div>
          </div>
          <button
            onClick={logout}
            className="flex items-center gap-2 w-full px-3 py-1.5 rounded-md text-sm font-medium text-slate-600 hover:bg-slate-100 hover:text-red-600 transition-colors"
          >
            <LogOut className="w-4 h-4" />
            Sign Out
          </button>
        </div>
      )}
    </>
  );

  return (
    <>
      {/* Desktop sidebar: always visible, fixed */}
      <aside className="hidden lg:flex fixed left-0 top-0 h-full w-56 bg-white border-r border-slate-200 flex-col z-30">
        {sidebarContent}
      </aside>

      {/* Mobile/tablet drawer: slides in from left with transition */}
      <aside
        className={`lg:hidden fixed left-0 top-0 h-full w-64 bg-white border-r border-slate-200 flex flex-col z-30 transition-transform duration-300 ease-in-out ${
          open ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        {sidebarContent}
      </aside>
    </>
  );
}
