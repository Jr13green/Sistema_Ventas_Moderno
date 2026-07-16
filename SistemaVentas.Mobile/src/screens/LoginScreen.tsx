import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  Alert,
  ActivityIndicator,
} from 'react-native';
import { AuthService } from '../services/AuthService';

export default function LoginScreen({ navigation }: any) {
  const [usuario, setUsuario] = useState('');
  const [contrasena, setContrasena] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async () => {
    if (!usuario.trim() || !contrasena.trim()) {
      Alert.alert('Error', 'Por favor ingresa usuario y contraseña');
      return;
    }

    setLoading(true);
    const success = await AuthService.login(usuario, contrasena);
    setLoading(false);

    if (success) {
      navigation.replace('Main');
    } else {
      Alert.alert('Error', 'Credenciales inválidas');
    }
  };

  const handleBiometricLogin = async () => {
    setLoading(true);
    const success = await AuthService.loginWithBiometrics();
    setLoading(false);

    if (success) {
      navigation.replace('Main');
    } else {
      Alert.alert('Error', 'Autenticación biométrica fallida');
    }
  };

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Sistema de Ventas</Text>
      <TextInput
        style={styles.input}
        placeholder="Usuario"
        value={usuario}
        onChangeText={setUsuario}
        autoCapitalize="none"
      />
      <TextInput
        style={styles.input}
        placeholder="Contraseña"
        value={contrasena}
        onChangeText={setContrasena}
        secureTextEntry
      />
      <TouchableOpacity style={styles.button} onPress={handleLogin} disabled={loading}>
        {loading ? (
          <ActivityIndicator color="#fff" />
        ) : (
          <Text style={styles.buttonText}>Iniciar Sesión</Text>
        )}
      </TouchableOpacity>
      <TouchableOpacity style={styles.biometricButton} onPress={handleBiometricLogin}>
        <Text style={styles.biometricText}>Usar biométrico</Text>
      </TouchableOpacity>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    padding: 24,
    backgroundColor: '#f5f5f5',
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    textAlign: 'center',
    marginBottom: 32,
    color: '#1a1a2e',
  },
  input: {
    backgroundColor: '#fff',
    borderRadius: 8,
    padding: 12,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: '#ddd',
  },
  button: {
    backgroundColor: '#1a73e8',
    borderRadius: 8,
    padding: 14,
    alignItems: 'center',
  },
  buttonText: { color: '#fff', fontWeight: 'bold', fontSize: 16 },
  biometricButton: { marginTop: 16, alignItems: 'center' },
  biometricText: { color: '#1a73e8', fontSize: 14 },
});
