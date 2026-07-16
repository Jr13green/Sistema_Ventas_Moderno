import NetInfo from '@react-native-community/netinfo';
import { ApiService } from './ApiService';
import { StorageService } from './StorageService';

interface PendingVenta {
  id: string;
  usuarioId: number;
  jugadas: Array<{ sorteoDiarioId: number; numero: string; monto: number }>;
  timestamp: string;
}

class SyncServiceClass {
  async queueVenta(venta: Omit<PendingVenta, 'id' | 'timestamp'>): Promise<void> {
    const pending = await this.getPending();
    pending.push({
      ...venta,
      id: `${Date.now()}-${Math.random().toString(36).slice(2)}`,
      timestamp: new Date().toISOString(),
    });
    await StorageService.setPendingVentas(pending);
  }

  private async getPending(): Promise<PendingVenta[]> {
    const raw = await StorageService.getPendingVentas<PendingVenta[]>();
    return raw ?? [];
  }

  async syncPending(): Promise<{ synced: number; failed: number }> {
    const state = await NetInfo.fetch();
    if (!state.isConnected) {
      return { synced: 0, failed: 0 };
    }

    const pending = await this.getPending();
    let synced = 0;
    let failed = 0;
    const remaining: PendingVenta[] = [];

    for (const venta of pending) {
      try {
        await ApiService.post('/api/v1/ventas', {
          usuarioId: venta.usuarioId,
          jugadas: venta.jugadas,
        });
        synced += 1;
      } catch {
        failed += 1;
        remaining.push(venta);
      }
    }

    await StorageService.setPendingVentas(remaining);
    return { synced, failed };
  }
}

export const SyncService = new SyncServiceClass();
