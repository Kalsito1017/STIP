import { useMutation } from '@tanstack/react-query';
import {
  predictionsApi,
  type DelayPredictionRequest,
  type DelayPredictionResponse,
  type TravelTimePredictionResponse,
} from '../services/api';

export function useDelayPrediction() {
  return useMutation<DelayPredictionResponse, Error, DelayPredictionRequest>({
    mutationFn: (req) => predictionsApi.predictDelay(req),
  });
}

export function useTravelTimePrediction() {
  return useMutation<
    TravelTimePredictionResponse,
    Error,
    { routeId: string; fromStopId: string; toStopId: string; departureTime: string }
  >({
    mutationFn: (req) =>
      predictionsApi.predictTravelTime(req.routeId, req.fromStopId, req.toStopId, req.departureTime),
  });
}
