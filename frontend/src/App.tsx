import { useEffect, Suspense, lazy } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'sonner';
import { MapLayout } from './components/MapLayout';
import { Layout } from './components/Layout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { useAppStore } from './store/useAppStore';

const LiveMapPage = lazy(() => import('./pages/LiveMapPage').then(m => ({ default: m.LiveMapPage })));
const DashboardPage = lazy(() => import('./pages/DashboardPage').then(m => ({ default: m.DashboardPage })));
const RoutesPage = lazy(() => import('./pages/RoutesPage').then(m => ({ default: m.RoutesPage })));
const RouteDetailPage = lazy(() => import('./pages/RouteDetailPage').then(m => ({ default: m.RouteDetailPage })));
const StopsPage = lazy(() => import('./pages/StopsPage').then(m => ({ default: m.StopsPage })));
const StopDetailPage = lazy(() => import('./pages/StopDetailPage').then(m => ({ default: m.StopDetailPage })));
const AnalyticsPage = lazy(() => import('./pages/AnalyticsPage').then(m => ({ default: m.AnalyticsPage })));
const PredictionsPage = lazy(() => import('./pages/PredictionsPage').then(m => ({ default: m.PredictionsPage })));
const FavoritesPage = lazy(() => import('./pages/FavoritesPage').then(m => ({ default: m.FavoritesPage })));
const SettingsPage = lazy(() => import('./pages/SettingsPage').then(m => ({ default: m.SettingsPage })));

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
        <Suspense>
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
              <Route path="/favorites" element={<FavoritesPage />} />
              <Route path="/analytics" element={<AnalyticsPage />} />
              <Route path="/predictions" element={<PredictionsPage />} />
              <Route path="/settings" element={<SettingsPage />} />
            </Route>
          </Route>
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
        </Suspense>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
