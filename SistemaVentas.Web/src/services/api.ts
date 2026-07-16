import axios from 'axios';

const api = axios.create({
  baseURL: '/api/v1',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers = {
      ...(config.headers ?? {}),
      Authorization: `******
    };
  }
  return config;
});

api.interceptors.response.use(
  res => res,
  err => {
    if (err.response?.status === 401) {
      localStorage.removeItem('token');
      window.dispatchEvent(new Event('sistema-ventas-auth-changed'));
      window.location.href = '/login';
    }
    return Promise.reject(err);
  },
);

export default api;
