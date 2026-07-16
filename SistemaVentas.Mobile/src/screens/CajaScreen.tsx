import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, RefreshControl } from 'react-native';
import { ApiService } from '../services/ApiService';

interface CajaData {
  saldoActual: number;
  totalIngresos: number;
  totalEgresos: number;
}

export default function CajaScreen() {
  const [caja, setCaja] = useState<CajaData | null>(null);
  const [refreshing, setRefreshing] = useState(false);

  const loadCaja = async () => {
    try {
      const data = await ApiService.get<CajaData>('/api/v1/caja/saldo');
      setCaja(data);
    } catch (err) {
      console.warn('Error cargando caja:', err);
    }
  };

  const onRefresh = async () => {
    setRefreshing(true);
    await loadCaja();
    setRefreshing(false);
  };

  useEffect(() => {
    loadCaja();
  }, []);

  return (
    <ScrollView
      style={styles.container}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
    >
      <Text style={styles.title}>Caja</Text>
      <View style={[styles.card, styles.saldoCard]}>
        <Text style={[styles.cardLabel, styles.saldoLabel]}>Saldo Actual</Text>
        <Text style={styles.saldoValue}>${caja?.saldoActual?.toFixed(2) ?? '—'}</Text>
      </View>
      <View style={styles.card}>
        <Text style={styles.cardLabel}>Total Ingresos</Text>
        <Text style={[styles.cardValue, { color: '#4caf50' }]}>
          +${caja?.totalIngresos?.toFixed(2) ?? '—'}
        </Text>
      </View>
      <View style={styles.card}>
        <Text style={styles.cardLabel}>Total Egresos</Text>
        <Text style={[styles.cardValue, { color: '#f44336' }]}>
          -${caja?.totalEgresos?.toFixed(2) ?? '—'}
        </Text>
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
  saldoCard: { backgroundColor: '#1a73e8' },
  cardLabel: { fontSize: 14, color: '#666', marginBottom: 4 },
  saldoLabel: { color: '#dfe9ff' },
  saldoValue: { fontSize: 36, fontWeight: 'bold', color: '#fff' },
  cardValue: { fontSize: 24, fontWeight: 'bold' },
});
