import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface Jugada {
  sorteoDiarioId: number;
  numero: string;
  monto: number;
}

interface VentasState {
  pendingVentas: Array<{ id: string; jugadas: Jugada[]; timestamp: string }>;
  isSyncing: boolean;
}

const initialState: VentasState = {
  pendingVentas: [],
  isSyncing: false,
};

const ventasSlice = createSlice({
  name: 'ventas',
  initialState,
  reducers: {
    addPendingVenta: (state, action: PayloadAction<{ id: string; jugadas: Jugada[]; timestamp: string }>) => {
      state.pendingVentas.push(action.payload);
    },
    removePendingVenta: (state, action: PayloadAction<string>) => {
      state.pendingVentas = state.pendingVentas.filter(v => v.id !== action.payload);
    },
    setSyncing: (state, action: PayloadAction<boolean>) => {
      state.isSyncing = action.payload;
    },
  },
});

export const { addPendingVenta, removePendingVenta, setSyncing } = ventasSlice.actions;
export default ventasSlice.reducer;
