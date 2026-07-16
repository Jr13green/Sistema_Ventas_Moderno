import AsyncStorage from '@react-native-async-storage/async-storage';

const TOKEN_KEY = '@sistema_ventas_token';
const USER_KEY = '@sistema_ventas_user';
const PENDING_KEY = '@pending_ventas';

class StorageServiceClass {
  async setToken(token: string): Promise<void> {
    await AsyncStorage.setItem(TOKEN_KEY, token);
  }

  async getToken(): Promise<string | null> {
    return AsyncStorage.getItem(TOKEN_KEY);
  }

  async clearToken(): Promise<void> {
    await AsyncStorage.removeItem(TOKEN_KEY);
  }

  async setUser(user: object): Promise<void> {
    await AsyncStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  async getUser<T>(): Promise<T | null> {
    const raw = await AsyncStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as T) : null;
  }

  async setPendingVentas(pendingVentas: object): Promise<void> {
    await AsyncStorage.setItem(PENDING_KEY, JSON.stringify(pendingVentas));
  }

  async getPendingVentas<T>(): Promise<T | null> {
    const raw = await AsyncStorage.getItem(PENDING_KEY);
    return raw ? (JSON.parse(raw) as T) : null;
  }

  async clear(): Promise<void> {
    await AsyncStorage.multiRemove([TOKEN_KEY, USER_KEY, PENDING_KEY]);
  }
}

export const StorageService = new StorageServiceClass();
