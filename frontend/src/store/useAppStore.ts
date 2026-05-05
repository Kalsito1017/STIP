import { create } from 'zustand';
import i18n, { type Locale, SUPPORTED_LOCALES, DEFAULT_LOCALE } from '../i18n';

export interface User {
  userId: string;
  email: string;
  fullName: string;
}

export interface Vehicle {
  vehicleId: string;
  routeId: string | null;
  tripId: string | null;
  lat: number;
  lon: number;
  bearing: number;
  speed: number;
  recordedAt: string;
}

export interface StopTimeUpdate {
  stopSequence: number | null;
  stopId: string | null;
  arrivalDelay: number | null;
  arrivalTime: number | null;
  departureDelay: number | null;
  departureTime: number | null;
  scheduleRelationship: number;
}

export interface TripUpdate {
  tripId: string;
  routeId: string | null;
  startTime: string | null;
  startDate: string | null;
  scheduleRelationship: number;
  vehicleId: string | null;
  stopTimeUpdates: StopTimeUpdate[];
  recordedAt: string;
}

export interface ActivePeriod {
  start: number | null;
  end: number | null;
}

export interface InformedEntity {
  agencyId: string | null;
  routeId: string | null;
  routeType: number | null;
  tripId: string | null;
  stopId: string | null;
}

export interface ServiceAlert {
  alertId: string;
  headerText: string;
  descriptionText: string | null;
  url: string | null;
  cause: number;
  effect: number;
  severity: number | null;
  activePeriods: ActivePeriod[];
  informedEntities: InformedEntity[];
  recordedAt: string;
}

interface AppState {
  vehicles: Vehicle[];
  tripUpdates: TripUpdate[];
  alerts: ServiceAlert[];
  selectedRoute: string | null;
  routeFilter: string;
  flyToTarget: { lat: number; lon: number; zoom: number } | null;
  darkMode: boolean;
  selectedVehicle: Vehicle | null;
  connectionState: 'connected' | 'reconnecting' | 'disconnected';
  lastUpdatedAt: string | null;
  token: string | null;
  user: User | null;
  isAuthenticated: boolean;
  language: Locale;
  setVehicles: (vehicles: Vehicle[]) => void;
  updateVehicle: (vehicle: Vehicle) => void;
  setTripUpdates: (updates: TripUpdate[]) => void;
  updateTripUpdate: (update: TripUpdate) => void;
  setAlerts: (alerts: ServiceAlert[]) => void;
  addAlert: (alert: ServiceAlert) => void;
  setSelectedRoute: (routeId: string | null) => void;
  setRouteFilter: (routeId: string) => void;
  setFlyToTarget: (target: { lat: number; lon: number; zoom: number } | null) => void;
  toggleDarkMode: () => void;
  setSelectedVehicle: (vehicle: Vehicle | null) => void;
  setConnectionState: (state: 'connected' | 'reconnecting' | 'disconnected') => void;
  setVehicleTimestamp: () => void;
  setAuth: (token: string, user: User) => void;
  clearAuth: () => void;
  setLanguage: (lang: Locale) => void;
}

function loadUser(): User | null {
  try {
    const raw = localStorage.getItem('user');
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function loadLanguage(): Locale {
  const stored = localStorage.getItem('language');
  if (stored && (SUPPORTED_LOCALES as string[]).includes(stored)) {
    return stored as Locale;
  }
  return DEFAULT_LOCALE;
}

const initialToken = localStorage.getItem('token');
const initialUser = loadUser();
const initialDarkMode = localStorage.getItem('darkMode') === 'true';
const initialLanguage = loadLanguage();

export const useAppStore = create<AppState>((set) => ({
  vehicles: [],
  tripUpdates: [],
  alerts: [],
  selectedRoute: null,
  routeFilter: '',
  flyToTarget: null,
  darkMode: initialDarkMode,
  selectedVehicle: null,
  connectionState: 'disconnected',
  lastUpdatedAt: null,
  token: initialToken,
  user: initialUser,
  isAuthenticated: !!initialToken && !!initialUser,
  language: initialLanguage,
  setVehicles: (vehicles) => set({ vehicles }),
  updateVehicle: (vehicle) =>
    set((state) => ({
      vehicles: [
        ...state.vehicles.filter((v) => v.vehicleId !== vehicle.vehicleId),
        vehicle,
      ],
    })),
  setTripUpdates: (tripUpdates) => set({ tripUpdates }),
  updateTripUpdate: (update) =>
    set((state) => ({
      tripUpdates: [
        ...state.tripUpdates.filter((tu) => tu.tripId !== update.tripId),
        update,
      ],
    })),
  setAlerts: (alerts) => set({ alerts }),
  addAlert: (alert) =>
    set((state) => ({
      alerts: [
        ...state.alerts.filter((a) => a.alertId !== alert.alertId),
        alert,
      ],
    })),
  setSelectedRoute: (routeId) => set({ selectedRoute: routeId }),
  setRouteFilter: (routeFilter) => set({ routeFilter }),
  setFlyToTarget: (flyToTarget) => set({ flyToTarget }),
  toggleDarkMode: () =>
    set((state) => {
      const next = !state.darkMode;
      localStorage.setItem('darkMode', String(next));
      return { darkMode: next };
    }),
  setConnectionState: (connectionState) => set({ connectionState }),
  setSelectedVehicle: (selectedVehicle) => set({ selectedVehicle }),
  setVehicleTimestamp: () => set({ lastUpdatedAt: new Date().toISOString() }),
  setAuth: (token, user) => {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
    set({ token, user, isAuthenticated: true });
  },
  clearAuth: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    set({ token: null, user: null, isAuthenticated: false });
  },
  setLanguage: (lang) => {
    localStorage.setItem('language', lang);
    i18n.changeLanguage(lang);
    set({ language: lang });
  },
}));