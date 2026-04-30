import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAppStore } from '../store/useAppStore';
import type { Vehicle, TripUpdate, ServiceAlert } from '../store/useAppStore';

export function useRealtime() {
  const setVehicles = useAppStore((s) => s.setVehicles);
  const updateVehicle = useAppStore((s) => s.updateVehicle);
  const setTripUpdates = useAppStore((s) => s.setTripUpdates);
  const updateTripUpdate = useAppStore((s) => s.updateTripUpdate);
  const setAlerts = useAppStore((s) => s.setAlerts);
  const addAlert = useAppStore((s) => s.addAlert);
  const token = useAppStore((s) => s.token);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    let cancelled = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/vehicles', {
        accessTokenFactory: () => token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('VehicleUpdated', (vehicle: Vehicle) => {
      if (!cancelled) updateVehicle(vehicle);
    });

    connection.on('VehicleBatch', (vehicles: Vehicle[]) => {
      if (!cancelled) setVehicles(vehicles);
    });

    connection.on('TripUpdated', (tripUpdate: TripUpdate) => {
      if (!cancelled) updateTripUpdate(tripUpdate);
    });

    connection.on('AlertUpdated', (alert: ServiceAlert) => {
      if (!cancelled) addAlert(alert);
    });

    connection.onreconnected(() => {
      if (!cancelled) setIsConnected(true);
    });

    connection.onclose(() => {
      if (!cancelled) setIsConnected(false);
    });

    connection.start().then(() => {
      if (!cancelled) setIsConnected(true);
    }).catch((err) => {
      console.error('SignalR connection failed:', err);
    });
    connectionRef.current = connection;

    return () => {
      cancelled = true;
      connection.stop().catch(() => { /* ignore stop errors */ });
    };
  }, [token, setVehicles, updateVehicle, setTripUpdates, updateTripUpdate, setAlerts, addAlert]);

  return { connectionRef, isConnected };
}
