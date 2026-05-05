import { describe, it, expect, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { I18nextProvider } from 'react-i18next';
import i18n from '../../i18n';
import { useAppStore } from '../../store/useAppStore';
import { AlertBanner } from '../AlertBanner';

function renderWithI18n(ui: React.ReactElement) {
  return render(
    <I18nextProvider i18n={i18n}>
      {ui}
    </I18nextProvider>,
  );
}

const initialState = useAppStore.getInitialState();

beforeEach(() => {
  useAppStore.setState(initialState);
});

describe('AlertBanner', () => {
  it('returns null when alerts array is empty', () => {
    useAppStore.setState({ alerts: [] });
    const { container } = renderWithI18n(<AlertBanner />);
    expect(container.firstChild).toBeNull();
  });

  it('renders alert header text', () => {
    useAppStore.setState({
      alerts: [
        {
          alertId: 'a1',
          headerText: 'Service disruption on Route 1',
          descriptionText: null,
          url: null,
          cause: 1,
          effect: 1,
          severity: 2,
          activePeriods: [],
          informedEntities: [],
          recordedAt: '2024-01-01T00:00:00Z',
        },
      ],
    });

    renderWithI18n(<AlertBanner />);
    expect(screen.getByText('Service disruption on Route 1')).toBeInTheDocument();
  });

  it('renders severity badge', () => {
    useAppStore.setState({
      alerts: [
        {
          alertId: 'a2',
          headerText: 'Delay',
          descriptionText: null,
          url: null,
          cause: 1,
          effect: 1,
          severity: 3,
          activePeriods: [],
          informedEntities: [],
          recordedAt: '2024-01-01T00:00:00Z',
        },
      ],
    });

    renderWithI18n(<AlertBanner />);
    expect(screen.getByText('SEVERE')).toBeInTheDocument();
  });

  it('renders default severity label when severity is null', () => {
    // When severity is null, the component defaults to severity ?? 2, which is WARNING
    useAppStore.setState({
      alerts: [
        {
          alertId: 'a3',
          headerText: 'Info',
          descriptionText: null,
          url: null,
          cause: 1,
          effect: 1,
          severity: null,
          activePeriods: [],
          informedEntities: [],
          recordedAt: '2024-01-01T00:00:00Z',
        },
      ],
    });

    renderWithI18n(<AlertBanner />);
    // severity ?? 2 => 2 => severityLabels[2] => WARNING
    expect(screen.getByText('WARNING')).toBeInTheDocument();
  });

  it('shows route IDs when informedEntities have routeId', () => {
    useAppStore.setState({
      alerts: [
        {
          alertId: 'a4',
          headerText: 'Route delay',
          descriptionText: null,
          url: null,
          cause: 1,
          effect: 1,
          severity: 2,
          activePeriods: [],
          informedEntities: [
            { agencyId: null, routeId: 'R1', routeType: null, tripId: null, stopId: null },
            { agencyId: null, routeId: 'R2', routeType: null, tripId: null, stopId: null },
          ],
          recordedAt: '2024-01-01T00:00:00Z',
        },
      ],
    });

    renderWithI18n(<AlertBanner />);
    expect(screen.getByText(/Routes:/)).toBeInTheDocument();
    expect(screen.getByText(/R1, R2/)).toBeInTheDocument();
  });

  it('does not show route prefix when no informedEntities have routeId', () => {
    useAppStore.setState({
      alerts: [
        {
          alertId: 'a5',
          headerText: 'General notice',
          descriptionText: null,
          url: null,
          cause: 1,
          effect: 1,
          severity: 1,
          activePeriods: [],
          informedEntities: [],
          recordedAt: '2024-01-01T00:00:00Z',
        },
      ],
    });

    renderWithI18n(<AlertBanner />);
    expect(screen.queryByText(/Routes:/)).not.toBeInTheDocument();
  });

  it('renders descriptionText when provided', () => {
    useAppStore.setState({
      alerts: [
        {
          alertId: 'a6',
          headerText: 'Notice',
          descriptionText: 'Due to road works, expect delays.',
          url: null,
          cause: 1,
          effect: 1,
          severity: 2,
          activePeriods: [],
          informedEntities: [],
          recordedAt: '2024-01-01T00:00:00Z',
        },
      ],
    });

    renderWithI18n(<AlertBanner />);
    expect(screen.getByText('Due to road works, expect delays.')).toBeInTheDocument();
  });
});
