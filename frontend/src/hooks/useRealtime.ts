import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { toast } from 'sonner';
import { useAppStore } from '../store/useAppStore';
import type { Vehicle, TripUpdate, ServiceAlert } from '../store/useAppStore';

export function useRealtime() {
  const updateTripUpdate = useAppStore((s) => s.updateTripUpdate);
  const addAlert = useAppStore((s) => s.addAlert);
  const removeExpiredAlerts = useAppStore((s) => s.removeExpiredAlerts);
  const setConnectionState = useAppStore((s) => s.setConnectionState);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const rafRef = useRef<number | null>(null);

  useEffect(() => {
    let cancelled = false;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/vehicles', {
        accessTokenFactory: () => useAppStore.getState().token ?? '',
      })
      .withAutomaticReconnect()
      .build();

    connection.on('VehicleUpdated', (vehicle: Vehicle) => {
      if (cancelled) return;
      useAppStore.getState().queueVehicleUpdate(vehicle);
      if (rafRef.current === null) {
        rafRef.current = requestAnimationFrame(() => {
          rafRef.current = null;
          if (!cancelled) {
            useAppStore.getState().flushVehicleUpdates();
          }
        });
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
      if (rafRef.current !== null) {
        cancelAnimationFrame(rafRef.current);
        rafRef.current = null;
      }
      connection.stop().catch(() => { /* ignore stop errors */ });
    };
  }, [updateTripUpdate, addAlert, removeExpiredAlerts, setConnectionState]);

  return connectionRef;
}
