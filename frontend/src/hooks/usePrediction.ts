import { useMutation } from '@tanstack/react-query';
import { predictionsApi, type DelayPredictionRequest, type DelayPredictionResponse } from '../services/api';

export function useDelayPrediction() {
  return useMutation<DelayPredictionResponse, Error, DelayPredictionRequest>({
    mutationFn: (req) => predictionsApi.predictDelay(req),
  });
}
