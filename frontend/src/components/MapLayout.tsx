import { Outlet, useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';
import { Sun, Moon, User, LogOut, LogIn } from 'lucide-react';
import { useAppStore } from '../store/useAppStore';
import { useLogout } from '../hooks/useAuth';
import { SearchBar } from './SearchBar';
import { Button } from './ui/button';

export function MapLayout() {
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
          <div className="flex items-center gap-2 pointer-events-auto">
            <Button
              variant="outline"
              size="icon"
              onClick={toggleDarkMode}
              aria-label={darkMode ? 'Switch to light mode' : 'Switch to dark mode'}
              className="bg-card shadow-sm h-10 w-10"
            >
              {darkMode ? <Sun className="w-4 h-4" /> : <Moon className="w-4 h-4" />}
            </Button>

            {isAuthenticated ? (
              <div className="relative" ref={menuRef}>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setMenuOpen((v) => !v)}
                  className="bg-card shadow-sm gap-2 h-10"
                >
                  <div className="w-6 h-6 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                    <User className="w-3.5 h-3.5 text-primary" />
                  </div>
                  <span className="hidden sm:inline text-xs max-w-[100px] truncate">
                    {user?.fullName?.split(' ')[0]}
                  </span>
                </Button>
                {menuOpen && (
                  <div className="absolute right-0 top-full mt-1 w-48 bg-card border border-border rounded-lg shadow-lg py-1 z-50">
                    <div className="px-3 py-2 border-b border-border">
                      <p className="text-sm font-medium text-foreground truncate">{user?.fullName}</p>
                      <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
                    </div>
                    <button
                      onClick={() => { logout(); setMenuOpen(false); }}
                      className="flex items-center gap-2 w-full px-3 py-2 text-sm text-muted-foreground hover:bg-accent hover:text-destructive"
                    >
                      <LogOut className="w-4 h-4" />
                      Sign Out
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <Button
                size="sm"
                onClick={() => navigate('/login')}
                className="shadow-sm gap-2 h-10"
              >
                <LogIn className="w-4 h-4" />
                <span className="hidden sm:inline">Sign In</span>
              </Button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
