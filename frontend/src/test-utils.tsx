import type { ReactNode } from 'react';
import { render, type RenderOptions } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { I18nextProvider } from 'react-i18next';
import i18n from './i18n';
import { useAppStore } from './store/useAppStore';

function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
}

interface WrapperOptions {
  initialRoute?: string;
}

export function renderWithProviders(
  ui: React.ReactNode,
  options?: RenderOptions & WrapperOptions,
) {
  const { initialRoute = '/', ...renderOptions } = options ?? {};
  const queryClient = createTestQueryClient();

  function Wrapper({ children }: { children: ReactNode }) {
    return (
      <I18nextProvider i18n={i18n}>
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={[initialRoute]}>
            {children}
          </MemoryRouter>
        </QueryClientProvider>
      </I18nextProvider>
    );
  }

  return render(ui, { wrapper: Wrapper, ...renderOptions });
}

export function resetAppStore() {
  useAppStore.setState({
    vehicles: [],
    tripUpdates: [],
    alerts: [],
    selectedRoute: null,
    routeFilter: '',
    flyToTarget: null,
    connectionState: 'disconnected',
    lastUpdatedAt: null,
    token: null,
    user: null,
    isAuthenticated: false,
  });
}
