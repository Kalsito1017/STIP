import { useEffect, useRef } from 'react';
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
  const setConnectionState = useAppStore((s) => s.setConnectionState);
  const setVehicleTimestamp = useAppStore((s) => s.setVehicleTimestamp);
  const token = useAppStore((s) => s.token);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    let cancelled = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/vehicles', {
        accessTokenFactory: () => token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('VehicleUpdated', (vehicle: Vehicle) => {
      if (!cancelled) {
        updateVehicle(vehicle);
        setVehicleTimestamp();
      }
    });

    connection.on('VehicleBatch', (vehicles: Vehicle[]) => {
      if (!cancelled) {
        setVehicles(vehicles);
        setVehicleTimestamp();
      }
    });

    connection.on('TripUpdated', (tripUpdate: TripUpdate) => {
      if (!cancelled) updateTripUpdate(tripUpdate);
    });

    connection.on('AlertUpdated', (alert: ServiceAlert) => {
      if (!cancelled) addAlert(alert);
    });

    connection.onreconnecting(() => {
      if (!cancelled) setConnectionState('reconnecting');
    });

    connection.onreconnected(() => {
      if (!cancelled) setConnectionState('connected');
    });

    connection.onclose(() => {
      if (!cancelled) setConnectionState('disconnected');
    });

    connection.start()
      .then(() => {
        if (!cancelled) setConnectionState('connected');
      })
      .catch((err) => {
        console.error('SignalR connection failed:', err);
      });
    connectionRef.current = connection;

    return () => {
      cancelled = true;
      setConnectionState('disconnected');
      connection.stop().catch(() => { /* ignore stop errors */ });
    };
  }, [token, setVehicles, updateVehicle, setTripUpdates, updateTripUpdate, setAlerts, addAlert, setConnectionState, setVehicleTimestamp]);

  return connectionRef;
}
