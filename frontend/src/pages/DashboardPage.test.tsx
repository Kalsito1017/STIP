import { renderWithProviders } from '@/test-utils';
import { DashboardPage } from './DashboardPage';
import * as useVehiclesModule from '../hooks/useVehicles';
import * as useDelaysModule from '../hooks/useDelays';
import { useAppStore as store } from '../store/useAppStore';

vi.mock('../hooks/useVehicles', () => ({
  useLiveVehicles: vi.fn(),
}));

vi.mock('../hooks/useDelays', () => ({
  useReliabilityRanking: vi.fn(),
  usePeakHours: vi.fn(),
  useRouteDelayPattern: vi.fn(),
  useDelayHeatmap: vi.fn(),
}));

const mockUseQueryResult = (overrides = {}) => ({
  data: undefined,
  isLoading: false,
  isError: false,
  error: null,
  isFetching: false,
  refetch: vi.fn(),
  ...overrides,
});

describe('DashboardPage', () => {
  beforeEach(() => {
    store.setState({ alerts: [] });
  });

  it('shows SkeletonRankingList during loading state', () => {
    vi.mocked(useVehiclesModule.useLiveVehicles).mockReturnValue(
      mockUseQueryResult({ data: [], isLoading: true }) as unknown as ReturnType<typeof useVehiclesModule.useLiveVehicles>,
    );
    vi.mocked(useDelaysModule.useReliabilityRanking).mockReturnValue(
      mockUseQueryResult({ isLoading: true }) as unknown as ReturnType<typeof useDelaysModule.useReliabilityRanking>,
    );
    vi.mocked(useDelaysModule.usePeakHours).mockReturnValue(
      mockUseQueryResult({ isLoading: true }) as unknown as ReturnType<typeof useDelaysModule.usePeakHours>,
    );

    const { container } = renderWithProviders(<DashboardPage />, { initialRoute: '/dashboard' });

    const skeletonCards = container.querySelectorAll('.animate-pulse');
    expect(skeletonCards.length).toBeGreaterThanOrEqual(4);
  });

  it('shows "Updating" indicator during background refetch', () => {
    vi.mocked(useVehiclesModule.useLiveVehicles).mockReturnValue(
      mockUseQueryResult({
        data: [{ vehicleId: 'v1', routeId: '94', tripId: null, lat: 42.7, lon: 23.3, bearing: 0, speed: 30, recordedAt: '' }],
        isLoading: false,
        isFetching: true,
      }) as unknown as ReturnType<typeof useVehiclesModule.useLiveVehicles>,
    );
    vi.mocked(useDelaysModule.useReliabilityRanking).mockReturnValue(
      mockUseQueryResult({
        data: [{ routeId: '94', shortName: '94', score: 85, onTimePct: 0.9, avgDelaySeconds: 45 }],
        isLoading: false,
        isFetching: false,
      }) as unknown as ReturnType<typeof useDelaysModule.useReliabilityRanking>,
    );
    vi.mocked(useDelaysModule.usePeakHours).mockReturnValue(
      mockUseQueryResult({
        data: [{ hourOfDay: 8, avgDelaySeconds: 120 }],
        isLoading: false,
        isFetching: false,
      }) as unknown as ReturnType<typeof useDelaysModule.usePeakHours>,
    );

    const { getByText } = renderWithProviders(<DashboardPage />, { initialRoute: '/dashboard' });

    expect(getByText('Updating')).toBeInTheDocument();
  });

  it('renders stat cards with correct values when loaded', () => {
    vi.mocked(useVehiclesModule.useLiveVehicles).mockReturnValue(
      mockUseQueryResult({
        data: [{ vehicleId: 'v1', routeId: '94', tripId: null, lat: 42.7, lon: 23.3, bearing: 0, speed: 30, recordedAt: '' }],
        isLoading: false,
        isFetching: false,
      }) as unknown as ReturnType<typeof useVehiclesModule.useLiveVehicles>,
    );
    vi.mocked(useDelaysModule.useReliabilityRanking).mockReturnValue(
      mockUseQueryResult({
        data: [{ routeId: '94', shortName: '94', score: 85, onTimePct: 0.9, avgDelaySeconds: 45 }],
        isLoading: false,
        isFetching: false,
      }) as unknown as ReturnType<typeof useDelaysModule.useReliabilityRanking>,
    );
    vi.mocked(useDelaysModule.usePeakHours).mockReturnValue(
      mockUseQueryResult({
        data: [{ hourOfDay: 8, avgDelaySeconds: 120 }],
        isLoading: false,
        isFetching: false,
      }) as unknown as ReturnType<typeof useDelaysModule.usePeakHours>,
    );

    const { getAllByText } = renderWithProviders(<DashboardPage />, { initialRoute: '/dashboard' });

    const matches = getAllByText('1');
    expect(matches.length).toBeGreaterThanOrEqual(1);
  });
});
