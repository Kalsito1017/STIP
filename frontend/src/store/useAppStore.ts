import { create } from 'zustand';

interface Vehicle {
  vehicleId: string;
  routeId: string | null;
  tripId: string | null;
  lat: number;
  lon: number;
  bearing: number;
  speed: number;
  recordedAt: string;
}

interface AppState {
  vehicles: Vehicle[];
  selectedRoute: string | null;
  darkMode: boolean;
  setVehicles: (vehicles: Vehicle[]) => void;
  updateVehicle: (vehicle: Vehicle) => void;
  setSelectedRoute: (routeId: string | null) => void;
  toggleDarkMode: () => void;
}

export const useAppStore = create<AppState>((set) => ({
  vehicles: [],
  selectedRoute: null,
  darkMode: false,
  setVehicles: (vehicles) => set({ vehicles }),
  updateVehicle: (vehicle) =>
    set((state) => ({
      vehicles: [
        ...state.vehicles.filter((v) => v.vehicleId !== vehicle.vehicleId),
        vehicle,
      ],
    })),
  setSelectedRoute: (routeId) => set({ selectedRoute: routeId }),
  toggleDarkMode: () => set((state) => ({ darkMode: !state.darkMode })),
}));
