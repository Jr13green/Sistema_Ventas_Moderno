import React, { useEffect, useState } from 'react';
import { View, Text, StyleSheet, ScrollView, RefreshControl } from 'react-native';
import { ApiService } from '../services/ApiService';

interface ReporteItem {
  fecha: string;
  totalVentas: number;
  totalJugadas: number;
}

export default function ReportesScreen() {
  const [reporte, setReporte] = useState<ReporteItem[]>([]);
  const [refreshing, setRefreshing] = useState(false);

  const loadReporte = async () => {
    try {
      const data = await ApiService.get<ReporteItem[]>('/api/v1/reportes/semana');
      setReporte(data);
    } catch (err) {
      console.warn('Error cargando reporte:', err);
    }
  };

  const onRefresh = async () => {
    setRefreshing(true);
    await loadReporte();
    setRefreshing(false);
  };

  useEffect(() => {
    loadReporte();
  }, []);

  return (
    <ScrollView
      style={styles.container}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
    >
      <Text style={styles.title}>Reportes</Text>
      {reporte.map((item, idx) => (
        <View key={idx} style={styles.row}>
          <Text style={styles.fecha}>{new Date(item.fecha).toLocaleDateString()}</Text>
          <View style={styles.stats}>
            <Text style={styles.stat}>${item.totalVentas.toFixed(2)}</Text>
            <Text style={styles.statLabel}>{item.totalJugadas} jugadas</Text>
          </View>
        </View>
      ))}
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5', padding: 16 },
  title: { fontSize: 24, fontWeight: 'bold', marginBottom: 20, color: '#1a1a2e' },
  row: {
    backgroundColor: '#fff',
    borderRadius: 8,
    padding: 16,
    marginBottom: 8,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  fecha: { fontSize: 14, color: '#333' },
  stats: { alignItems: 'flex-end' },
  stat: { fontSize: 18, fontWeight: 'bold', color: '#1a73e8' },
  statLabel: { fontSize: 12, color: '#666' },
});
