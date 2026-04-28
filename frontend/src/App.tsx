import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Layout } from './components/Layout';
import { LiveMapPage } from './pages/LiveMapPage';
import { DashboardPage } from './pages/DashboardPage';
import { RoutesPage } from './pages/RoutesPage';
import { RouteDetailPage } from './pages/RouteDetailPage';
import { StopsPage } from './pages/StopsPage';
import { StopDetailPage } from './pages/StopDetailPage';
import { AnalyticsPage } from './pages/AnalyticsPage';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="/map" element={<LiveMapPage />} />
            <Route path="/dashboard" element={<DashboardPage />} />
            <Route path="/routes" element={<RoutesPage />} />
            <Route path="/routes/:id" element={<RouteDetailPage />} />
            <Route path="/stops" element={<StopsPage />} />
            <Route path="/stops/:id" element={<StopDetailPage />} />
            <Route path="/analytics" element={<AnalyticsPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
