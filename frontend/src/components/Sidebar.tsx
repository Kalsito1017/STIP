import { NavLink, useNavigate } from 'react-router-dom';
import { Map, LayoutDashboard, Bus, MapPin, TrendingUp, Brain, Star, Settings, User, LogOut, X, Lock, LogIn, Sun, Moon } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useAppStore } from '../store/useAppStore';
import { useLogout } from '../hooks/useAuth';
import { Button } from './ui/button';
import { LanguageSwitcher } from './LanguageSwitcher';

interface SidebarProps {
  open: boolean;
  onClose: () => void;
}

export function Sidebar({ open, onClose }: SidebarProps) {
  const { t } = useTranslation('layout');
  const user = useAppStore((s) => s.user);
  const isAuthenticated = useAppStore((s) => s.isAuthenticated);
  const darkMode = useAppStore((s) => s.darkMode);
  const toggleDarkMode = useAppStore((s) => s.toggleDarkMode);
  const logout = useLogout();
  const navigate = useNavigate();

  const navItems = [
    { to: '/', label: t('live_map'), icon: Map, protected: false },
    { to: '/dashboard', label: t('dashboard'), icon: LayoutDashboard, protected: false },
    { to: '/routes', label: t('routes'), icon: Bus, protected: false },
    { to: '/stops', label: t('stops'), icon: MapPin, protected: false },
    { to: '/favorites', label: t('favorites', { defaultValue: 'Favorites' }), icon: Star, protected: true },
    { to: '/analytics', label: t('analytics'), icon: TrendingUp, protected: true },
    { to: '/predictions', label: t('predictions'), icon: Brain, protected: true },
    { to: '/settings', label: t('settings'), icon: Settings, protected: true },
  ];

  const sidebarContent = (
    <>
      <div className="p-4 border-b border-border flex items-center justify-between">
        <NavLink to="/" onClick={onClose}>
          <h1 className="text-lg font-bold text-foreground" title="Sofia Transport Intelligence Platform">STIP</h1>
          <p className="text-xs text-muted-foreground">{t('appSubtitle', { ns: 'common' })}</p>
        </NavLink>
        <Button
          variant="ghost"
          size="icon"
          onClick={onClose}
          className="lg:hidden"
          aria-label={t('close_menu')}
        >
          <X className="w-5 h-5" />
        </Button>
      </div>
      <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
        {navItems.map(({ to, label, icon: Icon, protected: isProtected }) => {
          const locked = isProtected && !isAuthenticated;
          return (
            <NavLink
              key={to}
              to={locked ? '#' : to}
              onClick={(e) => {
                if (locked) {
                  e.preventDefault();
                  navigate('/login');
                }
                onClose();
              }}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                  locked
                    ? 'text-muted-foreground/50 cursor-not-allowed'
                    : isActive
                      ? 'bg-primary/10 text-primary'
                      : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                }`
              }
            >
              <Icon className="w-4 h-4 flex-shrink-0" />
              {label}
              {locked && <Lock className="w-3 h-3 flex-shrink-0 ml-auto" />}
            </NavLink>
          );
        })}
      </nav>

      <div className="border-t border-border p-3 space-y-2">
        <div className="flex items-center justify-between px-2">
          <LanguageSwitcher />
          <Button
            variant="ghost"
            size="icon"
            onClick={toggleDarkMode}
            aria-label={darkMode ? t('switch_light') : t('switch_dark')}
            title={darkMode ? t('switch_light') : t('switch_dark')}
          >
            {darkMode ? <Sun className="w-4 h-4" /> : <Moon className="w-4 h-4" />}
          </Button>
        </div>

        {isAuthenticated && user ? (
          <div className="space-y-2 pt-2">
            <div className="flex items-center gap-3 px-2">
              <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                <User className="w-4 h-4 text-primary" />
              </div>
              <div className="min-w-0 flex-1">
                <p className="text-sm font-medium text-foreground truncate">{user.fullName}</p>
                <p className="text-xs text-muted-foreground truncate">{user.email}</p>
              </div>
            </div>
            <button
              onClick={logout}
              className="flex items-center gap-2 w-full px-3 py-1.5 rounded-md text-sm font-medium text-muted-foreground hover:bg-accent hover:text-destructive transition-colors"
            >
              <LogOut className="w-4 h-4" />
              {t('sign_out')}
            </button>
          </div>
        ) : (
          <button
            onClick={() => navigate('/login')}
            className="flex items-center gap-2 w-full px-3 py-2 rounded-md text-sm font-medium bg-primary text-primary-foreground hover:bg-primary/90 transition-colors"
          >
            <LogIn className="w-4 h-4" />
            {t('sign_in', { ns: 'auth' })}
          </button>
        )}
      </div>
    </>
  );

  return (
    <>
      <aside className="hidden lg:flex fixed left-0 top-0 h-full w-56 bg-card border-r border-border flex-col z-30">
        {sidebarContent}
      </aside>

      <aside
        className={`lg:hidden fixed left-0 top-0 h-full w-64 bg-card border-r border-border flex flex-col z-30 transition-transform duration-300 ease-in-out ${
          open ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        {sidebarContent}
      </aside>
    </>
  );
}
