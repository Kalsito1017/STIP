import { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { MapLayout } from './components/MapLayout';
import { Layout } from './components/Layout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { LiveMapPage } from './pages/LiveMapPage';
import { DashboardPage } from './pages/DashboardPage';
import { RoutesPage } from './pages/RoutesPage';
import { RouteDetailPage } from './pages/RouteDetailPage';
import { StopsPage } from './pages/StopsPage';
import { StopDetailPage } from './pages/StopDetailPage';
import { AnalyticsPage } from './pages/AnalyticsPage';
import { SettingsPage } from './pages/SettingsPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { useAppStore } from './store/useAppStore';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
      staleTime: 60_000,
    },
  },
});

function useDarkModeClass() {
  const darkMode = useAppStore((s) => s.darkMode);

  useEffect(() => {
    document.documentElement.classList.toggle('dark', darkMode);
  }, [darkMode]);
}

function useSyncLanguage() {
  const language = useAppStore((s) => s.language);

  useEffect(() => {
    document.documentElement.lang = language;
  }, [language]);
}

function App() {
  useDarkModeClass();
  useSyncLanguage();

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Toaster
          position="top-right"
          toastOptions={{
            style: {
              borderRadius: 'var(--radius-lg)',
              border: '1px solid hsl(var(--border))',
              background: 'hsl(var(--card))',
              color: 'hsl(var(--foreground))',
            },
          }}
        />
        <Routes>
          <Route element={<MapLayout />}>
            <Route index element={<LiveMapPage />} />
          </Route>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/map" element={<Navigate to="/" replace />} />
          <Route element={<Layout />}>
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/routes" element={<RoutesPage />} />
            <Route path="/routes/:id" element={<RouteDetailPage />} />
            <Route path="/stops" element={<StopsPage />} />
            <Route path="/stops/:id" element={<StopDetailPage />} />
            <Route element={<ProtectedRoute />}>
              <Route path="/analytics" element={<AnalyticsPage />} />
              <Route path="/settings" element={<SettingsPage />} />
            </Route>
          </Route>
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
