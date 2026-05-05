import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { I18nextProvider } from 'react-i18next';
import i18n from '@/i18n';
import { DashboardPage } from '@/pages/DashboardPage';

vi.mock('@/hooks/useVehicles', () => ({
  useLiveVehicles: () => ({ data: [], isLoading: false }),
}));

vi.mock('@/hooks/useRoutes', () => ({
  useRoutes: () => ({ data: [], isLoading: false }),
}));

vi.mock('@/hooks/useAnalytics', () => ({
  useSystemOverview: () => ({
    data: { totalRoutes: 0, totalStops: 0, dailyPositions: 0, avgReliability: 0 },
    isLoading: false,
  }),
}));

vi.mock('@/hooks/useHeatmap', () => ({
  useDelayHeatmap: () => ({ data: [], isLoading: false }),
}));

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <I18nextProvider i18n={i18n}>
          <DashboardPage />
        </I18nextProvider>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('DashboardPage', () => {
  it('renders the page', () => {
    renderPage();
    expect(screen.getByText(/dashboard/i)).toBeInTheDocument();
  });
});
