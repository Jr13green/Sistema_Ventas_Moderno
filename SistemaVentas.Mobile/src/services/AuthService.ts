import ReactNativeBiometrics from 'react-native-biometrics';
import { ApiService } from './ApiService';
import { StorageService } from './StorageService';

interface LoginResponse {
  token: string;
  expiration: string;
  usuario: string;
}

type AuthListener = (isAuthenticated: boolean) => void;

class AuthServiceClass {
  private readonly rnBiometrics = new ReactNativeBiometrics();
  private readonly listeners = new Set<AuthListener>();

  subscribe(listener: AuthListener): () => void {
    this.listeners.add(listener);
    return () => {
      this.listeners.delete(listener);
    };
  }

  private notify(isAuthenticated: boolean): void {
    this.listeners.forEach(listener => listener(isAuthenticated));
  }

  async login(usuario: string, contrasena: string): Promise<boolean> {
    try {
      const response = await ApiService.post<LoginResponse>('/api/v1/auth/login', {
        usuario,
        contrasena,
      });
      await StorageService.setToken(response.token);
      await StorageService.setUser({
        usuario: response.usuario,
        expiration: response.expiration,
      });
      this.notify(true);
      return true;
    } catch {
      return false;
    }
  }

  async loginWithBiometrics(): Promise<boolean> {
    try {
      const { available } = await this.rnBiometrics.isSensorAvailable();
      if (!available) {
        return false;
      }

      const { success } = await this.rnBiometrics.simplePrompt({
        promptMessage: 'Confirmar identidad',
      });

      if (success) {
        const savedToken = await StorageService.getToken();
        const authenticated = !!savedToken;
        if (authenticated) {
          this.notify(true);
        }
        return authenticated;
      }

      return false;
    } catch {
      return false;
    }
  }

  async logout(): Promise<void> {
    await StorageService.clear();
    this.notify(false);
  }

  async checkAuth(): Promise<boolean> {
    const token = await StorageService.getToken();
    return !!token;
  }
}

export const AuthService = new AuthServiceClass();
