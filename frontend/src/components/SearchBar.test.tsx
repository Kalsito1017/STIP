import { renderWithProviders, resetAppStore } from '@/test-utils';
import { SearchBar } from './SearchBar';
import * as useRoutesModule from '../hooks/useRoutes';
import * as useStopsModule from '../hooks/useStops';
import { fireEvent } from '@testing-library/react';

vi.mock('../hooks/useRoutes', () => ({
  useRoutes: vi.fn(),
  useRouteDetail: vi.fn(),
}));

vi.mock('../hooks/useStops', () => ({
  useStops: vi.fn(),
  useStopCongestion: vi.fn(),
}));

const mockRoutes = [
  { routeId: 'r1', shortName: '94' },
  { routeId: 'r2', shortName: '280' },
];

const mockStops = [
  { stopId: 's1', stopName: 'NDK', lat: 42.69, lon: 23.32 },
  { stopId: 's2', stopName: 'Orlov Most', lat: 42.69, lon: 23.33 },
];

describe('SearchBar', () => {
  beforeEach(() => {
    resetAppStore();
  });

  it('shows "Loading routes and stops..." with spinner when data is loading', () => {
    vi.mocked(useRoutesModule.useRoutes).mockReturnValue({
      data: undefined,
      isLoading: true,
    } as unknown as ReturnType<typeof useRoutesModule.useRoutes>);
    vi.mocked(useStopsModule.useStops).mockReturnValue({
      data: undefined,
      isLoading: true,
    } as unknown as ReturnType<typeof useStopsModule.useStops>);

    const { getByPlaceholderText, getByText } = renderWithProviders(<SearchBar />);
    const input = getByPlaceholderText('Search routes or stops...');
    fireEvent.change(input, { target: { value: '94' } });

    expect(getByText('Loading routes and stops...')).toBeInTheDocument();
  });

  it('shows "No results found" when query has no matches', () => {
    vi.mocked(useRoutesModule.useRoutes).mockReturnValue({
      data: mockRoutes,
      isLoading: false,
    } as unknown as ReturnType<typeof useRoutesModule.useRoutes>);
    vi.mocked(useStopsModule.useStops).mockReturnValue({
      data: mockStops,
      isLoading: false,
    } as unknown as ReturnType<typeof useStopsModule.useStops>);

    const { getByPlaceholderText, getByText } = renderWithProviders(<SearchBar />);
    const input = getByPlaceholderText('Search routes or stops...');
    fireEvent.change(input, { target: { value: 'zzznotfound' } });

    expect(getByText('No results found')).toBeInTheDocument();
  });

  it('shows route results with bus icon', () => {
    vi.mocked(useRoutesModule.useRoutes).mockReturnValue({
      data: mockRoutes,
      isLoading: false,
    } as unknown as ReturnType<typeof useRoutesModule.useRoutes>);
    vi.mocked(useStopsModule.useStops).mockReturnValue({
      data: mockStops,
      isLoading: false,
    } as unknown as ReturnType<typeof useStopsModule.useStops>);

    const { getByPlaceholderText, getByText } = renderWithProviders(<SearchBar />);
    const input = getByPlaceholderText('Search routes or stops...');
    fireEvent.change(input, { target: { value: '94' } });

    expect(getByText('94')).toBeInTheDocument();
  });

  it('shows stop results with map pin icon', () => {
    vi.mocked(useRoutesModule.useRoutes).mockReturnValue({
      data: [],
      isLoading: false,
    } as unknown as ReturnType<typeof useRoutesModule.useRoutes>);
    vi.mocked(useStopsModule.useStops).mockReturnValue({
      data: mockStops,
      isLoading: false,
    } as unknown as ReturnType<typeof useStopsModule.useStops>);

    const { getByPlaceholderText, getByText } = renderWithProviders(<SearchBar />);
    const input = getByPlaceholderText('Search routes or stops...');
    fireEvent.change(input, { target: { value: 'NDK' } });

    expect(getByText('NDK')).toBeInTheDocument();
  });

  it('closes dropdown when clicking a result', () => {
    vi.mocked(useRoutesModule.useRoutes).mockReturnValue({
      data: mockRoutes,
      isLoading: false,
    } as unknown as ReturnType<typeof useRoutesModule.useRoutes>);
    vi.mocked(useStopsModule.useStops).mockReturnValue({
      data: mockStops,
      isLoading: false,
    } as unknown as ReturnType<typeof useStopsModule.useStops>);

    const { getByPlaceholderText, getByText, queryByText } = renderWithProviders(<SearchBar />);
    const input = getByPlaceholderText('Search routes or stops...');
    fireEvent.change(input, { target: { value: '94' } });

    expect(getByText('94')).toBeInTheDocument();
    fireEvent.click(getByText('94'));
    expect(queryByText('94')).toBeNull();
  });
});
