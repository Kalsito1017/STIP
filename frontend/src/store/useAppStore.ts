import { create } from 'zustand';
import i18n, { type Locale, SUPPORTED_LOCALES, DEFAULT_LOCALE } from '../i18n';

let pendingVehicleUpdates = new Map<string, Vehicle>();

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
  queueVehicleUpdate: (vehicle: Vehicle) => void;
  flushVehicleUpdates: () => void;
  removeStaleVehicles: (activeIds: string[]) => void;
  setTripUpdates: (updates: TripUpdate[]) => void;
  updateTripUpdate: (update: TripUpdate) => void;
  setAlerts: (alerts: ServiceAlert[]) => void;
  addAlert: (alert: ServiceAlert) => void;
  removeExpiredAlerts: () => void;
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

function safeLocalStorageGet(key: string): string | null {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

function safeLocalStorageSet(key: string, value: string): void {
  try {
    localStorage.setItem(key, value);
  } catch {
    // Storage full or unavailable — ignore silently
  }
}

function safeLocalStorageRemove(key: string): void {
  try {
    localStorage.removeItem(key);
  } catch {
    // ignore
  }
}

function loadUser(): User | null {
  try {
    const raw = safeLocalStorageGet('user');
    return raw ? JSON.parse(raw) : null;
  } catch {
    return null;
  }
}

function loadLanguage(): Locale {
  const detected = i18n.language;
  if (detected && (SUPPORTED_LOCALES as string[]).includes(detected)) {
    return detected as Locale;
  }
  return DEFAULT_LOCALE;
}

const initialToken = safeLocalStorageGet('token');
const initialUser = loadUser();
const initialDarkMode = safeLocalStorageGet('darkMode') === 'true';
const initialLanguage = loadLanguage();

const ALERT_TTL_MS = 30 * 60 * 1000; // 30 minutes
const TRIP_UPDATE_TTL_MS = 30 * 60 * 1000;

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
  queueVehicleUpdate: (vehicle) => {
    pendingVehicleUpdates.set(vehicle.vehicleId, vehicle);
  },
  flushVehicleUpdates: () => {
    if (pendingVehicleUpdates.size === 0) return;
    const updates = pendingVehicleUpdates;
    pendingVehicleUpdates = new Map();
    set((state) => {
      const prev = state.vehicles;
      let changed = false;
      const next = new Array(prev.length);
      const prevById = new Map(prev.map((v, i) => [v.vehicleId, i]));
      for (const [id, vehicle] of updates) {
        const idx = prevById.get(id);
        if (idx !== undefined) {
          next[idx] = vehicle;
          if (prev[idx] !== vehicle) changed = true;
        } else {
          next.push(vehicle);
          changed = true;
        }
      }
      if (!changed) return state;
      for (let i = 0; i < next.length; i++) {
        if (next[i] === undefined) next[i] = prev[i];
      }
      return { vehicles: next, lastUpdatedAt: new Date().toISOString() };
    });
  },
  removeStaleVehicles: (activeIds) =>
    set((state) => {
      const activeSet = new Set(activeIds);
      const filtered = state.vehicles.filter((v) => activeSet.has(v.vehicleId));
      return filtered.length === state.vehicles.length ? state : { vehicles: filtered };
    }),
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
  removeExpiredAlerts: () =>
    set((state) => {
      const now = Date.now();
      const filtered = state.alerts.filter((a) => {
        const recorded = new Date(a.recordedAt).getTime();
        return now - recorded < ALERT_TTL_MS;
      });
      const filteredTrips = state.tripUpdates.filter((tu) => {
        const recorded = new Date(tu.recordedAt).getTime();
        return now - recorded < TRIP_UPDATE_TTL_MS;
      });
      const changes: Partial<AppState> = {};
      if (filtered.length !== state.alerts.length) changes.alerts = filtered;
      if (filteredTrips.length !== state.tripUpdates.length) changes.tripUpdates = filteredTrips;
      return Object.keys(changes).length > 0 ? changes : state;
    }),
  setSelectedRoute: (routeId) => set({ selectedRoute: routeId }),
  setRouteFilter: (routeFilter) => set({ routeFilter }),
  setFlyToTarget: (flyToTarget) => set({ flyToTarget }),
  toggleDarkMode: () =>
    set((state) => {
      const next = !state.darkMode;
      safeLocalStorageSet('darkMode', String(next));
      return { darkMode: next };
    }),
  setConnectionState: (connectionState) => set({ connectionState }),
  setSelectedVehicle: (selectedVehicle) => set({ selectedVehicle }),
  setVehicleTimestamp: () => set({ lastUpdatedAt: new Date().toISOString() }),
  setAuth: (token, user) => {
    safeLocalStorageSet('token', token);
    safeLocalStorageSet('user', JSON.stringify(user));
    set({ token, user, isAuthenticated: true });
  },
  clearAuth: () => {
    safeLocalStorageRemove('token');
    safeLocalStorageRemove('user');
    set({ token: null, user: null, isAuthenticated: false });
  },
  setLanguage: (lang) => {
    safeLocalStorageSet('language', lang);
    i18n.changeLanguage(lang);
    set({ language: lang });
  },
}));
