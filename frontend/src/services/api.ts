import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
  timeout: 10000,
});

export const routesApi = {
  getAll: () => api.get('/routes').then(r => r.data),
  getById: (id: string) => api.get(`/routes/${id}`).then(r => r.data),
  getDelayPattern: (id: string, date?: string) =>
    api.get(`/routes/${id}/delay-pattern`, { params: { date } }).then(r => r.data),
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

export const analyticsApi = {
  getHeatmap: (from?: string, to?: string) =>
    api.get('/analytics/heatmap/delays', { params: { from, to } }).then(r => r.data),
  getRanking: (top = 10, best = true) =>
    api.get('/analytics/reliability/ranking', { params: { top, best } }).then(r => r.data),
  getPeakHours: (date?: string) =>
    api.get('/analytics/peak-hours', { params: { date } }).then(r => r.data),
};

export default api;
