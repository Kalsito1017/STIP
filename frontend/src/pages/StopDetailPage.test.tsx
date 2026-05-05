import { StopDetailPage } from './StopDetailPage';
import * as useStopsModule from '../hooks/useStops';
import { render } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

vi.mock('../hooks/useStops', () => ({
  useStops: vi.fn(),
  useStopCongestion: vi.fn(),
}));

const mockUseQueryResult = (overrides = {}) => ({
  data: undefined,
  isLoading: false,
  isError: false,
  error: null,
  refetch: vi.fn(),
  ...overrides,
});

const mockStop = { stopId: 's1', stopName: 'NDK', lat: 42.69, lon: 23.32 };

function renderStopDetail() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={['/stops/s1']}>
        <Routes>
          <Route path="/stops/:id" element={<StopDetailPage />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}

describe('StopDetailPage', () => {
  it('uses <Skeleton> component during initial page load (not raw divs)', () => {
    vi.mocked(useStopsModule.useStops).mockReturnValue(
      mockUseQueryResult({
        data: [mockStop],
        isLoading: true,
      }) as unknown as ReturnType<typeof useStopsModule.useStops>,
    );
    vi.mocked(useStopsModule.useStopCongestion).mockReturnValue(
      mockUseQueryResult() as unknown as ReturnType<typeof useStopsModule.useStopCongestion>,
    );

    const { container } = renderStopDetail();

    const skeletons = container.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThanOrEqual(2);
  });

  it('shows skeleton block in congestion chart while congLoading is true', () => {
    vi.mocked(useStopsModule.useStops).mockReturnValue(
      mockUseQueryResult({
        data: [mockStop],
        isLoading: false,
      }) as unknown as ReturnType<typeof useStopsModule.useStops>,
    );
    vi.mocked(useStopsModule.useStopCongestion).mockReturnValue(
      mockUseQueryResult({
        isLoading: true,
      }) as unknown as ReturnType<typeof useStopsModule.useStopCongestion>,
    );

    const { container } = renderStopDetail();

    const cardSkeletons = container.querySelectorAll('.animate-pulse');
    expect(cardSkeletons.length).toBeGreaterThanOrEqual(1);
  });

  it('shows stop details when data is loaded', () => {
    vi.mocked(useStopsModule.useStops).mockReturnValue(
      mockUseQueryResult({
        data: [mockStop],
        isLoading: false,
      }) as unknown as ReturnType<typeof useStopsModule.useStops>,
    );
    vi.mocked(useStopsModule.useStopCongestion).mockReturnValue(
      mockUseQueryResult({
        data: [{ hourOfDay: 8, vehicleCount: 15 }],
        isLoading: false,
      }) as unknown as ReturnType<typeof useStopsModule.useStopCongestion>,
    );

    const { getByText } = renderStopDetail();

    expect(getByText('NDK')).toBeInTheDocument();
  });

  it('shows "not found" when stop is missing', () => {
    vi.mocked(useStopsModule.useStops).mockReturnValue(
      mockUseQueryResult({
        data: [],
        isLoading: false,
      }) as unknown as ReturnType<typeof useStopsModule.useStops>,
    );
    vi.mocked(useStopsModule.useStopCongestion).mockReturnValue(
      mockUseQueryResult() as unknown as ReturnType<typeof useStopsModule.useStopCongestion>,
    );

    const { getByText } = renderStopDetail();
    expect(getByText(/not_found|not found/i)).toBeInTheDocument();
  });
});
