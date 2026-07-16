import axios, { AxiosInstance } from 'axios';
import { StorageService } from './StorageService';

class ApiServiceClass {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: 'https://api.sistemaventas.app',
      timeout: 10000,
      headers: { 'Content-Type': 'application/json' },
    });

    this.client.interceptors.request.use(async config => {
      const token = await StorageService.getToken();
      if (token) {
        config.headers = {
          ...(config.headers ?? {}),
          Authorization: `******
        };
      }
      return config;
    });

    this.client.interceptors.response.use(
      response => response,
      async error => {
        if (error.response?.status === 401) {
          await StorageService.clearToken();
        }
        return Promise.reject(error);
      },
    );
  }

  async get<T>(url: string, params?: object): Promise<T> {
    const { data } = await this.client.get<T>(url, { params });
    return data;
  }

  async post<T>(url: string, body?: object): Promise<T> {
    const { data } = await this.client.post<T>(url, body);
    return data;
  }

  async put<T>(url: string, body?: object): Promise<T> {
    const { data } = await this.client.put<T>(url, body);
    return data;
  }

  async delete<T>(url: string): Promise<T> {
    const { data } = await this.client.delete<T>(url);
    return data;
  }
}

export const ApiService = new ApiServiceClass();
