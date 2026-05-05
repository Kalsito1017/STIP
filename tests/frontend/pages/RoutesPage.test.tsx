import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { I18nextProvider } from 'react-i18next';
import i18n from '@/i18n';
import { RoutesPage } from '@/pages/RoutesPage';

vi.mock('@/hooks/useRoutes', () => ({
  useRoutes: () => ({
    data: [
      { routeId: 'r-1', shortName: '1', longName: 'Route 1', transitType: 3, latestReliability: 85 },
      { routeId: 'r-2', shortName: '2', longName: 'Route 2', transitType: 0, latestReliability: 92 },
    ],
    isLoading: false,
  }),
}));

function renderPage() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <I18nextProvider i18n={i18n}>
          <RoutesPage />
        </I18nextProvider>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('RoutesPage', () => {
  it('renders route list', () => {
    renderPage();
    expect(screen.getByText('1')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
  });
});
