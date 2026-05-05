import { render } from '@testing-library/react';
import { MapLoadingOverlay } from './MapLoadingOverlay';

describe('MapLoadingOverlay', () => {
  it('returns null when visible is false', () => {
    const { container } = render(<MapLoadingOverlay visible={false} />);
    expect(container.firstChild).toBeNull();
  });

  it('renders overlay with spinner when visible is true', () => {
    const { getByText } = render(<MapLoadingOverlay visible={true} />);
    expect(getByText('Loading map data')).toBeInTheDocument();
  });

  it('shows checkmark for loaded layers and spinner for loading layers', () => {
    const layers = [
      { label: 'Routes', loaded: true },
      { label: 'Stops', loaded: false },
      { label: 'Vehicles', loaded: true },
    ];

    const { getByText } = render(
      <MapLoadingOverlay visible={true} layers={layers} />,
    );

    expect(getByText('Routes')).toHaveClass('text-green-700');
    expect(getByText('Stops')).toHaveClass('text-slate-500');
    expect(getByText('Vehicles')).toHaveClass('text-green-700');

    // Loading layer should have a skeleton bar next to it
    const stopsRow = getByText('Stops').closest('.flex');
    expect(stopsRow).not.toBeNull();
    const pulseInStopsRow = stopsRow!.querySelector('.animate-pulse');
    expect(pulseInStopsRow).toBeInTheDocument();
  });

  it('shows generic skeleton bars when no layers prop provided', () => {
    const { container } = render(<MapLoadingOverlay visible={true} />);
    const skeletons = container.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThanOrEqual(2);
  });

  it('shows all layers as loaded when every layer is loaded', () => {
    const layers = [
      { label: 'Routes', loaded: true },
      { label: 'Stops', loaded: true },
    ];

    const { getByText } = render(
      <MapLoadingOverlay visible={true} layers={layers} />,
    );

    expect(getByText('Routes')).toHaveClass('text-green-700');
    expect(getByText('Stops')).toHaveClass('text-green-700');
  });
});
