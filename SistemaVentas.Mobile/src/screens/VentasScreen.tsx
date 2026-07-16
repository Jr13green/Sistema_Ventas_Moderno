import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  Alert,
  ScrollView,
} from 'react-native';
import NetInfo from '@react-native-community/netinfo';
import { ApiService } from '../services/ApiService';
import { SyncService } from '../services/SyncService';

export default function VentasScreen() {
  const [numero, setNumero] = useState('');
  const [monto, setMonto] = useState('');
  const [sorteoDiarioId] = useState(1);

  const handleCrearVenta = async () => {
    if (!numero.trim() || !monto.trim()) {
      Alert.alert('Error', 'Completa todos los campos');
      return;
    }

    const montoNum = parseFloat(monto);
    if (Number.isNaN(montoNum) || montoNum <= 0) {
      Alert.alert('Error', 'Monto inválido');
      return;
    }

    const netState = await NetInfo.fetch();
    const jugada = { sorteoDiarioId, numero: numero.trim(), monto: montoNum };

    if (netState.isConnected) {
      try {
        await ApiService.post('/api/v1/ventas', {
          usuarioId: 1,
          jugadas: [jugada],
        });
        Alert.alert('Éxito', 'Venta creada correctamente');
        setNumero('');
        setMonto('');
      } catch {
        Alert.alert('Error', 'No se pudo crear la venta');
      }
    } else {
      await SyncService.queueVenta({ usuarioId: 1, jugadas: [jugada] });
      Alert.alert('Guardado offline', 'Venta guardada, se sincronizará cuando haya conexión');
      setNumero('');
      setMonto('');
    }
  };

  return (
    <ScrollView style={styles.container}>
      <Text style={styles.title}>Nueva Venta</Text>
      <View style={styles.form}>
        <Text style={styles.label}>Número</Text>
        <TextInput
          style={styles.input}
          placeholder="00"
          value={numero}
          onChangeText={setNumero}
          keyboardType="numeric"
          maxLength={2}
        />
        <Text style={styles.label}>Monto</Text>
        <TextInput
          style={styles.input}
          placeholder="0.00"
          value={monto}
          onChangeText={setMonto}
          keyboardType="decimal-pad"
        />
        <TouchableOpacity style={styles.button} onPress={handleCrearVenta}>
          <Text style={styles.buttonText}>Crear Venta</Text>
        </TouchableOpacity>
      </View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f5f5', padding: 16 },
  title: { fontSize: 24, fontWeight: 'bold', marginBottom: 20, color: '#1a1a2e' },
  form: { backgroundColor: '#fff', borderRadius: 12, padding: 20, elevation: 2 },
  label: { fontSize: 14, color: '#666', marginBottom: 4, marginTop: 12 },
  input: { borderWidth: 1, borderColor: '#ddd', borderRadius: 8, padding: 12, fontSize: 16 },
  button: {
    backgroundColor: '#1a73e8',
    borderRadius: 8,
    padding: 14,
    alignItems: 'center',
    marginTop: 20,
  },
  buttonText: { color: '#fff', fontWeight: 'bold', fontSize: 16 },
});
