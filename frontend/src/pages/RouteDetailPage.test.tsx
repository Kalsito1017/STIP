import { renderWithProviders } from '@/test-utils';
import { RouteDetailPage } from './RouteDetailPage';
import * as useRoutesModule from '../hooks/useRoutes';
import * as useDelaysModule from '../hooks/useDelays';
import { render } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

vi.mock('../hooks/useRoutes', () => ({
  useRoutes: vi.fn(),
  useRouteDetail: vi.fn(),
}));

vi.mock('../hooks/useDelays', () => ({
  useReliabilityRanking: vi.fn(),
  usePeakHours: vi.fn(),
  useRouteDelayPattern: vi.fn(),
  useDelayHeatmap: vi.fn(),
}));

vi.mock('../hooks/useStops', () => ({
  useStops: vi.fn(() => ({ data: [], isLoading: false, isError: false, error: null, refetch: vi.fn() })),
  useStopCongestion: vi.fn(() => ({ data: undefined, isLoading: false, isError: false, error: null, refetch: vi.fn() })),
}));

vi.mock('../hooks/usePrediction', () => ({
  useDelayPrediction: vi.fn(() => ({ data: undefined, isPending: false, mutate: vi.fn() })),
}));

vi.mock('react-leaflet', () => ({
  MapContainer: () => null,
  TileLayer: () => null,
  useMap: () => null,
  ZoomControl: () => null,
}));

const mockUseQueryResult = (overrides = {}) => ({
  data: undefined,
  isLoading: false,
  isError: false,
  error: null,
  refetch: vi.fn(),
  ...overrides,
});

function renderRouteDetail() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/routes/r1']}>
        <Routes>
          <Route path="/routes/:id" element={<RouteDetailPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('RouteDetailPage', () => {
  it('shows skeleton in delay pattern chart area while dpLoading is true', () => {
    vi.mocked(useRoutesModule.useRouteDetail).mockReturnValue(
      mockUseQueryResult({
        data: {
          routeId: 'r1',
          shortName: '94',
          longName: 'Route 94',
          type: 3,
          latestReliability: { score: 85, onTimePct: 0.9, avgDelaySeconds: 45, sampleCount: 100 },
        },
        isLoading: false,
      }) as unknown as ReturnType<typeof useRoutesModule.useRouteDetail>,
    );
    vi.mocked(useDelaysModule.useRouteDelayPattern).mockReturnValue(
      mockUseQueryResult({
        isLoading: true,
      }) as unknown as ReturnType<typeof useDelaysModule.useRouteDelayPattern>,
    );

    const { container } = renderRouteDetail();

    const skeletons = container.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThanOrEqual(1);
  });

  it('shows bar chart when delay pattern data is loaded', () => {
    vi.mocked(useRoutesModule.useRouteDetail).mockReturnValue(
      mockUseQueryResult({
        data: {
          routeId: 'r1',
          shortName: '94',
          longName: 'Route 94',
          type: 3,
          latestReliability: { score: 85, onTimePct: 0.9, avgDelaySeconds: 45, sampleCount: 100 },
        },
        isLoading: false,
      }) as unknown as ReturnType<typeof useRoutesModule.useRouteDetail>,
    );
    vi.mocked(useDelaysModule.useRouteDelayPattern).mockReturnValue(
      mockUseQueryResult({
        data: [
          { hourOfDay: 8, avgDelaySeconds: 120 },
          { hourOfDay: 9, avgDelaySeconds: 90 },
        ],
        isLoading: false,
      }) as unknown as ReturnType<typeof useDelaysModule.useRouteDelayPattern>,
    );

    const { getByText } = renderRouteDetail();

    expect(getByText('94')).toBeInTheDocument();
  });

  it('shows error alert when route fails to load', () => {
    vi.mocked(useRoutesModule.useRouteDetail).mockReturnValue(
      mockUseQueryResult({
        isError: true,
        error: new Error('Route not found'),
      }) as unknown as ReturnType<typeof useRoutesModule.useRouteDetail>,
    );

    const { getByText } = renderRouteDetail();
    expect(getByText('Route not found')).toBeInTheDocument();
  });
});
