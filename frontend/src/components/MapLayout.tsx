import { Outlet, useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';
import { Sun, Moon, User, LogOut, LogIn } from 'lucide-react';
import { useAppStore } from '../store/useAppStore';
import { useLogout } from '../hooks/useAuth';
import { SearchBar } from './SearchBar';
import { MobileTabBar } from './MobileTabBar';
import { Button } from './ui/button';
import { ConnectionIndicator } from './map/ConnectionIndicator';
import { useTranslation } from 'react-i18next';

export function MapLayout() {
  const { t } = useTranslation('layout');
  const darkMode = useAppStore((s) => s.darkMode);
  const toggleDarkMode = useAppStore((s) => s.toggleDarkMode);
  const isAuthenticated = useAppStore((s) => s.isAuthenticated);
  const user = useAppStore((s) => s.user);
  const logout = useLogout();
  const navigate = useNavigate();
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setMenuOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  return (
    <div className="relative h-screen w-screen overflow-hidden">
      <Outlet />

      <div className="absolute top-0 left-0 right-0 z-[1000] p-2 sm:p-3 pointer-events-none">
        <div className="flex items-start gap-2 max-w-screen-2xl mx-auto">
          <div className="flex-1 pointer-events-auto max-w-md sm:max-w-xl">
            <SearchBar />
          </div>
          <div className="flex items-center gap-1.5 pointer-events-auto">
            <ConnectionIndicator />
            <div className="flex items-center gap-1 bg-card/80 backdrop-blur-md rounded-full shadow-sm border border-border/60 p-1">
              <Button
                variant="ghost"
                size="icon"
                onClick={toggleDarkMode}
                aria-label={darkMode ? t('switch_light') : t('switch_dark')}
                className="h-8 w-8 rounded-full"
              >
                {darkMode ? <Sun className="w-4 h-4" /> : <Moon className="w-4 h-4" />}
              </Button>

              {isAuthenticated ? (
                <div className="relative" ref={menuRef}>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setMenuOpen((v) => !v)}
                    className="gap-1.5 h-8 px-2 rounded-full"
                  >
                    <div className="w-5 h-5 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                      <User className="w-3 h-3 text-primary" />
                    </div>
                    <span className="hidden sm:inline text-xs max-w-[80px] truncate">
                      {user?.fullName?.split(' ')[0]}
                    </span>
                  </Button>
                  {menuOpen && (
                    <div className="absolute right-0 top-full mt-1 w-48 bg-card border border-border rounded-xl shadow-xl py-1 z-50">
                      <div className="px-3 py-2 border-b border-border">
                        <p className="text-sm font-medium text-foreground truncate">{user?.fullName}</p>
                        <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
                      </div>
                      <button
                        onClick={() => { logout(); setMenuOpen(false); }}
                        className="flex items-center gap-2 w-full px-3 py-2 text-sm text-muted-foreground hover:bg-accent hover:text-destructive transition-colors"
                      >
                        <LogOut className="w-4 h-4" />
                        {t('sign_out')}
                      </button>
                    </div>
                  )}
                </div>
              ) : (
                <Button
                  size="sm"
                  onClick={() => navigate('/login')}
                  className="gap-1.5 h-8 px-3 rounded-full"
                >
                  <LogIn className="w-3.5 h-3.5" />
                  <span className="hidden sm:inline text-xs">{t('sign_in')}</span>
                </Button>
              )}
            </div>
          </div>
        </div>
      </div>

      <MobileTabBar />
    </div>
  );
}
