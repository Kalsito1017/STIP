import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAppStore } from '../store/useAppStore';

export function useRealtime() {
  const setVehicles = useAppStore((s) => s.setVehicles);
  const updateVehicle = useAppStore((s) => s.updateVehicle);
  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/vehicles')
      .withAutomaticReconnect()
      .build();

    connection.on('VehicleUpdated', (vehicle) => {
      updateVehicle(vehicle);
    });

    connection.on('VehicleBatch', (vehicles: any[]) => {
      setVehicles(vehicles);
    });

    connection.start().catch(console.error);
    connectionRef.current = connection;

    return () => {
      connection.stop();
    };
  }, [setVehicles, updateVehicle]);

  return connectionRef;
}
