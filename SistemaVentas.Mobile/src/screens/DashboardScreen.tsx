import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, RefreshControl } from 'react-native';
import { ApiService } from '../services/ApiService';
import { SyncService } from '../services/SyncService';

interface DashboardData {
  totalVentasHoy: number;
  totalJugadasHoy: number;
  saldoCaja: number;
}

export default function DashboardScreen() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  const loadData = async () => {
    try {
      const result = await ApiService.get<DashboardData>('/api/v1/reportes/dashboard');
      setData(result);
      await SyncService.syncPending();
    } catch (err) {
      console.warn('Error cargando dashboard:', err);
    }
  };

  const onRefresh = async () => {
    setRefreshing(true);
    await loadData();
    setRefreshing(false);
  };

  useEffect(() => {
    loadData();
  }, []);

  return (
    <ScrollView
      style={styles.container}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
    >
      <Text style={styles.title}>Dashboard</Text>
      <View style={styles.card}>
        <Text style={styles.cardLabel}>Ventas Hoy</Text>
        <Text style={styles.cardValue}>${data?.totalVentasHoy?.toFixed(2) ?? '—'}</Text>
      </View>
      <View style={styles.card}>
        <Text style={styles.cardLabel}>Jugadas Hoy</Text>
        <Text style={styles.cardValue}>{data?.totalJugadasHoy ?? '—'}</Text>
      </View>
      <View style={styles.card}>
        <Text style={styles.cardLabel}>Saldo en Caja</Text>
        <Text style={styles.cardValue}>${data?.saldoCaja?.toFixed(2) ?? '—'}</Text>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5', padding: 16 },
  title: { fontSize: 24, fontWeight: 'bold', marginBottom: 20, color: '#1a1a2e' },
  card: {
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: 20,
    marginBottom: 12,
    elevation: 2,
  },
  cardLabel: { fontSize: 14, color: '#666', marginBottom: 4 },
  cardValue: { fontSize: 28, fontWeight: 'bold', color: '#1a73e8' },
});
