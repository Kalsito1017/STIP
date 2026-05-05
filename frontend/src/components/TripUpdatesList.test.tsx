import { render, screen } from '@testing-library/react';
import { TripUpdatesList } from './TripUpdatesList';

import { useAppStore as store } from '../store/useAppStore';

describe('TripUpdatesList', () => {
  beforeEach(() => {
    store.setState({
      tripUpdates: [],
      connectionState: 'disconnected',
    });
  });

  it('shows skeleton rows when connection is not connected and no data', () => {
    store.setState({ tripUpdates: [], connectionState: 'reconnecting' });
    const { container } = render(<TripUpdatesList />);
    const skeletons = container.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThanOrEqual(3);
  });

  it('shows "No trip updates" when connected but empty', () => {
    store.setState({ tripUpdates: [], connectionState: 'connected' });
    render(<TripUpdatesList />);
    expect(screen.getByText(/no_trip_updates|No trip/i)).toBeInTheDocument();
  });

  it('shows trip update rows when data exists', () => {
    store.setState({
      tripUpdates: [
        {
          tripId: 'trip-1',
          routeId: '94',
          startTime: null,
          startDate: null,
          scheduleRelationship: 0,
          vehicleId: null,
          stopTimeUpdates: [
            { stopSequence: 1, stopId: 's1', arrivalDelay: 120, arrivalTime: null, departureDelay: null, departureTime: null, scheduleRelationship: 0 },
          ],
          recordedAt: new Date().toISOString(),
        },
      ],
      connectionState: 'connected',
    });

    render(<TripUpdatesList />);
    expect(screen.getByText('94')).toBeInTheDocument();
  });

  it('formats delay values with correct color coding', () => {
    store.setState({
      tripUpdates: [
        {
          tripId: 'trip-2',
          routeId: '280',
          startTime: null,
          startDate: null,
          scheduleRelationship: 0,
          vehicleId: null,
          stopTimeUpdates: [
            { stopSequence: 1, stopId: 's2', arrivalDelay: 300, arrivalTime: null, departureDelay: null, departureTime: null, scheduleRelationship: 0 },
          ],
          recordedAt: new Date().toISOString(),
        },
      ],
      connectionState: 'connected',
    });

    const { container } = render(<TripUpdatesList />);

    // Delay >= 180s should have red color class on the font-mono span
    const delayCells = container.querySelectorAll('.font-mono.text-red-600');
    expect(delayCells.length).toBeGreaterThanOrEqual(1);
  });
});
