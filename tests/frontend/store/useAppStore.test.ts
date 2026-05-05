import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useAppStore } from '@/store/useAppStore';

const initialState = useAppStore.getInitialState();

beforeEach(() => {
  useAppStore.setState(initialState);
  localStorage.clear();
});

describe('useAppStore', () => {
  describe('initial state', () => {
    it('has empty vehicles, tripUpdates, and alerts arrays', () => {
      const state = useAppStore.getState();
      expect(state.vehicles).toEqual([]);
      expect(state.tripUpdates).toEqual([]);
      expect(state.alerts).toEqual([]);
      expect(state.selectedRoute).toBeNull();
      expect(state.routeFilter).toBe('');
      expect(state.flyToTarget).toBeNull();
      expect(state.selectedVehicle).toBeNull();
      expect(state.connectionState).toBe('disconnected');
      expect(state.lastUpdatedAt).toBeNull();
      expect(state.token).toBeNull();
      expect(state.user).toBeNull();
      expect(state.isAuthenticated).toBe(false);
    });

    it('has darkMode as false by default', () => {
      expect(useAppStore.getState().darkMode).toBe(false);
    });

    it('has default language', () => {
      expect(useAppStore.getState().language).toBeDefined();
    });
  });

  describe('setVehicles', () => {
    it('replaces the vehicle list', () => {
      const vehicles = [
        { vehicleId: 'v1', routeId: 'R1', tripId: 't1', lat: 42.6, lon: 23.3, bearing: 90, speed: 10, recordedAt: '2024-01-01T00:00:00Z' },
      ];
      useAppStore.getState().setVehicles(vehicles);
      expect(useAppStore.getState().vehicles).toEqual(vehicles);
    });

    it('replaces a previously set list', () => {
      const vehicles1 = [
        { vehicleId: 'v1', routeId: 'R1', tripId: 't1', lat: 42.6, lon: 23.3, bearing: 90, speed: 10, recordedAt: '2024-01-01T00:00:00Z' },
      ];
      const vehicles2 = [
        { vehicleId: 'v2', routeId: 'R2', tripId: 't2', lat: 42.7, lon: 23.4, bearing: 180, speed: 0, recordedAt: '2024-01-01T00:01:00Z' },
      ];
      useAppStore.getState().setVehicles(vehicles1);
      useAppStore.getState().setVehicles(vehicles2);
      expect(useAppStore.getState().vehicles).toEqual(vehicles2);
    });
  });

  describe('updateVehicle', () => {
    it('adds a new vehicle when it does not exist', () => {
      const vehicle = { vehicleId: 'v1', routeId: 'R1', tripId: 't1', lat: 42.6, lon: 23.3, bearing: 90, speed: 10, recordedAt: '2024-01-01T00:00:00Z' };
      useAppStore.getState().updateVehicle(vehicle);
      expect(useAppStore.getState().vehicles).toEqual([vehicle]);
    });

    it('updates an existing vehicle by vehicleId', () => {
      const vehicle1 = { vehicleId: 'v1', routeId: 'R1', tripId: 't1', lat: 42.6, lon: 23.3, bearing: 90, speed: 10, recordedAt: '2024-01-01T00:00:00Z' };
      const vehicle2 = { vehicleId: 'v2', routeId: 'R2', tripId: 't2', lat: 42.7, lon: 23.4, bearing: 0, speed: 20, recordedAt: '2024-01-01T00:01:00Z' };
      const vehicle1updated = { vehicleId: 'v1', routeId: 'R1', tripId: 't1', lat: 42.61, lon: 23.31, bearing: 95, speed: 15, recordedAt: '2024-01-01T00:02:00Z' };

      useAppStore.getState().setVehicles([vehicle1, vehicle2]);
      useAppStore.getState().updateVehicle(vehicle1updated);

      const vehicles = useAppStore.getState().vehicles;
      expect(vehicles).toHaveLength(2);
      expect(vehicles.find((v) => v.vehicleId === 'v1')).toEqual(vehicle1updated);
      expect(vehicles.find((v) => v.vehicleId === 'v2')).toEqual(vehicle2);
    });
  });

  describe('addAlert', () => {
    it('adds a new alert', () => {
      const alert = {
        alertId: 'a1', headerText: 'Test', descriptionText: null, url: null,
        cause: 1, effect: 1, severity: 2, activePeriods: [], informedEntities: [], recordedAt: '2024-01-01T00:00:00Z',
      };
      useAppStore.getState().addAlert(alert);
      expect(useAppStore.getState().alerts).toEqual([alert]);
    });

    it('deduplicates by AlertId', () => {
      const alert1 = {
        alertId: 'a1', headerText: 'Test', descriptionText: null, url: null,
        cause: 1, effect: 1, severity: 2, activePeriods: [], informedEntities: [], recordedAt: '2024-01-01T00:00:00Z',
      };
      const alert2 = {
        alertId: 'a1', headerText: 'Updated', descriptionText: 'desc', url: null,
        cause: 1, effect: 1, severity: 3, activePeriods: [], informedEntities: [], recordedAt: '2024-01-01T00:01:00Z',
      };

      useAppStore.getState().addAlert(alert1);
      useAppStore.getState().addAlert(alert2);

      const alerts = useAppStore.getState().alerts;
      expect(alerts).toHaveLength(1);
      expect(alerts[0].headerText).toBe('Updated');
      expect(alerts[0].severity).toBe(3);
    });
  });

  describe('toggleDarkMode', () => {
    it('toggles darkMode from false to true', () => {
      expect(useAppStore.getState().darkMode).toBe(false);
      useAppStore.getState().toggleDarkMode();
      expect(useAppStore.getState().darkMode).toBe(true);
    });

    it('toggles darkMode from true to false', () => {
      useAppStore.setState({ darkMode: true });
      useAppStore.getState().toggleDarkMode();
      expect(useAppStore.getState().darkMode).toBe(false);
    });

    it('persists to localStorage', () => {
      useAppStore.getState().toggleDarkMode();
      expect(localStorage.getItem('darkMode')).toBe('true');
      useAppStore.getState().toggleDarkMode();
      expect(localStorage.getItem('darkMode')).toBe('false');
    });
  });

  describe('setLanguage', () => {
    it('changes language to bg', () => {
      useAppStore.getState().setLanguage('bg');
      expect(useAppStore.getState().language).toBe('bg');
    });

    it('changes language to en', () => {
      useAppStore.setState({ language: 'bg' });
      useAppStore.getState().setLanguage('en');
      expect(useAppStore.getState().language).toBe('en');
    });

    it('persists to localStorage', () => {
      useAppStore.getState().setLanguage('bg');
      expect(localStorage.getItem('language')).toBe('bg');
    });
  });

  describe('setAuth', () => {
    it('sets token, user, and isAuthenticated', () => {
      const user = { userId: 'u1', email: 'test@test.com', fullName: 'Test User' };
      useAppStore.getState().setAuth('token123', user);

      const state = useAppStore.getState();
      expect(state.token).toBe('token123');
      expect(state.user).toEqual(user);
      expect(state.isAuthenticated).toBe(true);
    });

    it('persists token and user to localStorage', () => {
      const user = { userId: 'u1', email: 'test@test.com', fullName: 'Test User' };
      useAppStore.getState().setAuth('token123', user);

      expect(localStorage.getItem('token')).toBe('token123');
      expect(JSON.parse(localStorage.getItem('user')!)).toEqual(user);
    });
  });

  describe('clearAuth', () => {
    it('removes token/user and sets isAuthenticated to false', () => {
      const user = { userId: 'u1', email: 'test@test.com', fullName: 'Test User' };
      useAppStore.setState({ token: 'token123', user, isAuthenticated: true });

      useAppStore.getState().clearAuth();

      const state = useAppStore.getState();
      expect(state.token).toBeNull();
      expect(state.user).toBeNull();
      expect(state.isAuthenticated).toBe(false);
    });

    it('removes token and user from localStorage', () => {
      localStorage.setItem('token', 'token123');
      localStorage.setItem('user', JSON.stringify({ userId: 'u1', email: 'test@test.com', fullName: 'Test User' }));

      const user = { userId: 'u1', email: 'test@test.com', fullName: 'Test User' };
      useAppStore.setState({ token: 'token123', user, isAuthenticated: true });
      useAppStore.getState().clearAuth();

      expect(localStorage.getItem('token')).toBeNull();
      expect(localStorage.getItem('user')).toBeNull();
    });
  });

  describe('setConnectionState', () => {
    it('updates connection state', () => {
      useAppStore.getState().setConnectionState('connected');
      expect(useAppStore.getState().connectionState).toBe('connected');

      useAppStore.getState().setConnectionState('reconnecting');
      expect(useAppStore.getState().connectionState).toBe('reconnecting');

      useAppStore.getState().setConnectionState('disconnected');
      expect(useAppStore.getState().connectionState).toBe('disconnected');
    });
  });

  describe('setVehicleTimestamp', () => {
    it('sets lastUpdatedAt to current ISO timestamp', () => {
      const before = new Date().toISOString();
      useAppStore.getState().setVehicleTimestamp();
      const after = new Date().toISOString();
      const ts = useAppStore.getState().lastUpdatedAt;
      expect(ts).toBeTruthy();
      expect(new Date(ts!).getTime()).toBeGreaterThanOrEqual(new Date(before).getTime());
      expect(new Date(ts!).getTime()).toBeLessThanOrEqual(new Date(after).getTime());
    });
  });
});
