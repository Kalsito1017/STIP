import { render, fireEvent } from '@testing-library/react';
import { FloatingFilterPanel } from './FloatingFilterPanel';

const defaultProps = {
  routes: [
    { routeId: '1', shortName: '94' },
    { routeId: '2', shortName: '280' },
  ],
  routeFilter: '',
  onRouteFilterChange: vi.fn(),
  vehicleCount: 42,
  clusterMode: false,
  onToggleCluster: vi.fn(),
  showRoutes: true,
  showStops: true,
  showHeatmap: false,
  showVehicles: true,
  onToggleRoutes: vi.fn(),
  onToggleStops: vi.fn(),
  onToggleHeatmap: vi.fn(),
  onToggleVehicles: vi.fn(),
};

describe('FloatingFilterPanel', () => {
  it('shows "Loading routes..." and disables select when routesLoading is true', () => {
    const { getByRole } = render(
      <FloatingFilterPanel {...defaultProps} routes={undefined} routesLoading={true} />,
    );

    fireEvent.click(getByRole('button', { name: /toggle filter/i }));

    const select = getByRole('combobox', { name: /filter vehicles/i });
    expect(select).toBeDisabled();
    expect(select.querySelector('option')!.textContent).toBe('Loading routes...');
  });

  it('shows "All routes" and enables select when loaded', () => {
    const { getByRole } = render(
      <FloatingFilterPanel {...defaultProps} />,
    );

    fireEvent.click(getByRole('button', { name: /toggle filter/i }));

    const select = getByRole('combobox', { name: /filter vehicles/i });
    expect(select).not.toBeDisabled();
    expect(select.querySelector('option')!.textContent).toBe('All routes');
  });

  it('expands and collapses panel on button click', () => {
    const { getByRole, queryByRole } = render(
      <FloatingFilterPanel {...defaultProps} />,
    );

    expect(queryByRole('combobox')).toBeNull();

    fireEvent.click(getByRole('button', { name: /toggle filter/i }));
    expect(getByRole('combobox')).toBeInTheDocument();

    fireEvent.click(getByRole('button', { name: /toggle filter/i }));
    expect(queryByRole('combobox')).toBeNull();
  });

  it('displays vehicle count', () => {
    const { getByRole, getByText } = render(
      <FloatingFilterPanel {...defaultProps} vehicleCount={42} />,
    );

    fireEvent.click(getByRole('button', { name: /toggle filter/i }));
    expect(getByText('42 vehicles tracking')).toBeInTheDocument();
  });

  it('calls onRouteFilterChange when select changes', () => {
    const onChange = vi.fn();
    const { getByRole } = render(
      <FloatingFilterPanel {...defaultProps} onRouteFilterChange={onChange} />,
    );

    fireEvent.click(getByRole('button', { name: /toggle filter/i }));
    fireEvent.change(getByRole('combobox'), { target: { value: '1' } });
    expect(onChange).toHaveBeenCalledWith('1');
  });
});
