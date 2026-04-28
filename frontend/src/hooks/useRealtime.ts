import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAppStore } from '../store/useAppStore';
import type { TripUpdate, ServiceAlert } from '../store/useAppStore';

export function useRealtime() {
  const setVehicles = useAppStore((s) => s.setVehicles);
  const updateVehicle = useAppStore((s) => s.updateVehicle);
  const setTripUpdates = useAppStore((s) => s.setTripUpdates);
  const updateTripUpdate = useAppStore((s) => s.updateTripUpdate);
  const setAlerts = useAppStore((s) => s.setAlerts);
  const addAlert = useAppStore((s) => s.addAlert);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    let cancelled = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/vehicles')
      .withAutomaticReconnect()
      .build();

    connection.on('VehicleUpdated', (vehicle) => {
      if (!cancelled) updateVehicle(vehicle);
    });

    connection.on('VehicleBatch', (vehicles: any[]) => {
      if (!cancelled) setVehicles(vehicles);
    });

    connection.on('TripUpdated', (tripUpdate: TripUpdate) => {
      if (!cancelled) updateTripUpdate(tripUpdate);
    });

    connection.on('AlertUpdated', (alert: ServiceAlert) => {
      if (!cancelled) addAlert(alert);
    });

    connection.start().catch(console.error);
    connectionRef.current = connection;

    return () => {
      cancelled = true;
      connection.stop();
    };
  }, [setVehicles, updateVehicle, setTripUpdates, updateTripUpdate, setAlerts, addAlert]);

  return connectionRef;
}