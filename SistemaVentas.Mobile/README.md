# SistemaVentas Mobile

React Native app para iOS y Android del Sistema de Ventas.

## Requisitos

- Node.js 18+
- React Native CLI
- iOS: Xcode 15+ (macOS)
- Android: Android Studio + JDK 17

## Instalación

```bash
npm install
```

### iOS

```bash
cd ios && pod install && cd ..
npx react-native run-ios
```

### Android

```bash
npx react-native run-android
```

## Features

- Login con JWT + autenticación biométrica (Face ID / Fingerprint)
- Dashboard en tiempo real
- Crear ventas online y offline (sync automático)
- Reportes semanales
- Gestión de caja
- Modo offline con SQLite local
- Sincronización bidireccional
