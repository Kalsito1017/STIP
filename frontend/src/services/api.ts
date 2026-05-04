import axios from 'axios';
import { toast } from 'sonner';

const api = axios.create({
  baseURL: '/api',
  timeout: 10000,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 400) {
      const isRegisterRoute = error.config?.url?.includes('/auth/register');
      if (!isRegisterRoute) {
        const data = error.response?.data;
        const message = data?.details?.join?.(', ') || data?.error || 'Bad request';
        toast.error(message);
      }
    }

    if (error.response?.status === 401) {
      import('../store/useAppStore').then(({ useAppStore }) => {
        useAppStore.getState().clearAuth();
      });
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  email: string;
  fullName: string;
  token: string;
}

export const authApi = {
  register: (data: RegisterRequest) =>
    api.post<AuthResponse>('/auth/register', data).then(r => r.data),
  login: (data: LoginRequest) =>
    api.post<AuthResponse>('/auth/login', data).then(r => r.data),
};

export const routesApi = {
  getAll: () => api.get('/routes').then(r => r.data),
  getById: (id: string) => api.get(`/routes/${id}`).then(r => r.data),
  getDelayPattern: (id: string, date?: string) =>
    api.get(`/routes/${id}/delay-pattern`, { params: { date } }).then(r => r.data),
  getShape: (id: string) =>
    api.get(`/routes/${id}/shape`).then(r => r.data),
  getAllShapes: () =>
    api.get('/routes/shapes').then(r => r.data),
};

export const stopsApi = {
  getAll: () => api.get('/stops').then(r => r.data),
  getCongestion: (id: string, date?: string) =>
    api.get(`/stops/${id}/congestion`, { params: { date } }).then(r => r.data),
};

export const vehiclesApi = {
  getLive: (routeId?: string) =>
    api.get('/vehicles/live', { params: { routeId } }).then(r => r.data),
};

export const tripUpdatesApi = {
  getLive: (routeId?: string) =>
    api.get('/tripupdates/live', { params: { routeId } }).then(r => r.data),
};

export const alertsApi = {
  getActive: (routeId?: string) =>
    api.get('/alerts', { params: { routeId } }).then(r => r.data),
};

export const analyticsApi = {
  getHeatmap: (from?: string, to?: string) =>
    api.get('/analytics/heatmap/delays', { params: { from, to } }).then(r => r.data),
  getRanking: (top = 10, best = true) =>
    api.get('/analytics/reliability/ranking', { params: { top, best } }).then(r => r.data),
  getPeakHours: (date?: string) =>
    api.get('/analytics/peak-hours', { params: { date } }).then(r => r.data),
};

export interface DelayPredictionRequest {
  routeId: string;
  stopId: string;
  stopSequence: number;
  hour: number;
  dayOfWeek: number;
}

export interface DelayPredictionResponse {
  predictedDelaySeconds: number;
  confidenceInterval: [number, number];
  modelVersion: string;
}

export interface TravelTimePredictionResponse {
  predictedTimeSeconds: number;
  confidenceInterval: number[];
  modelVersion: string;
}

export const predictionsApi = {
  predictDelay: (body: DelayPredictionRequest) =>
    api.post<DelayPredictionResponse>('/predictions/delay', body).then(r => r.data),
  predictTravelTime: (routeId: string, fromStopId: string, toStopId: string, departureTime: string) =>
    api.post<TravelTimePredictionResponse>('/predictions/travel-time', { routeId, fromStopId, toStopId, departureTime }).then(r => r.data),
};

export default api;