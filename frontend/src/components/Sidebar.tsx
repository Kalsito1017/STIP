import { NavLink } from 'react-router-dom';
import { Map, LayoutDashboard, Bus, MapPin, TrendingUp } from 'lucide-react';

const navItems = [
  { to: '/map', label: 'Live Map', icon: Map },
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/routes', label: 'Routes', icon: Bus },
  { to: '/stops', label: 'Stops', icon: MapPin },
  { to: '/analytics', label: 'Analytics', icon: TrendingUp },
];

export function Sidebar() {
  return (
    <aside className="fixed left-0 top-0 h-full w-56 bg-white border-r border-slate-200 flex flex-col z-30">
      <div className="p-4 border-b border-slate-200">
        <h1 className="text-lg font-bold text-slate-900">STIP</h1>
        <p className="text-xs text-slate-500">Sofia Transport Intelligence</p>
      </div>
      <nav className="flex-1 p-3 space-y-1">
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
            <Icon className="w-4 h-4" />
            {label}
          </NavLink>
        ))}
      </nav>
    </aside>
  );
}
