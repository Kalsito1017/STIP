import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { toast } from 'sonner';
import { useAppStore } from '../store/useAppStore';
import type { Vehicle, TripUpdate, ServiceAlert } from '../store/useAppStore';

export function useRealtime() {
  const updateVehicle = useAppStore((s) => s.updateVehicle);
  const updateTripUpdate = useAppStore((s) => s.updateTripUpdate);
  const addAlert = useAppStore((s) => s.addAlert);
  const removeExpiredAlerts = useAppStore((s) => s.removeExpiredAlerts);
  const setConnectionState = useAppStore((s) => s.setConnectionState);
  const setVehicleTimestamp = useAppStore((s) => s.setVehicleTimestamp);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    let cancelled = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/vehicles', {
        accessTokenFactory: () => useAppStore.getState().token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('VehicleUpdated', (vehicle: Vehicle) => {
      if (!cancelled) {
        updateVehicle(vehicle);
        setVehicleTimestamp();
      }
    });

    connection.on('TripUpdated', (tripUpdate: TripUpdate) => {
      if (!cancelled) updateTripUpdate(tripUpdate);
    });

    connection.on('AlertUpdated', (alert: ServiceAlert) => {
      if (!cancelled) {
        addAlert(alert);
        toast(alert.headerText || 'New service alert', {
          description: alert.descriptionText?.slice(0, 120),
        });
      }
    });

    connection.onreconnecting(() => {
      if (!cancelled) {
        setConnectionState('reconnecting');
        toast.warning('Connection lost. Reconnecting...', { id: 'signalr-connection', duration: Infinity });
      }
    });

    connection.onreconnected(() => {
      if (!cancelled) {
        setConnectionState('connected');
        toast.success('Reconnected', { id: 'signalr-connection' });
        removeExpiredAlerts();
      }
    });

    connection.onclose(() => {
      if (!cancelled) {
        setConnectionState('disconnected');
        toast.error('Connection lost', { id: 'signalr-connection' });
      }
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
  }, [updateVehicle, updateTripUpdate, addAlert, removeExpiredAlerts, setConnectionState, setVehicleTimestamp]);

  return connectionRef;
}
