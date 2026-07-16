import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface AuthState {
  isAuthenticated: boolean;
  token: string | null;
  usuario: string | null;
}

const initialState: AuthState = {
  isAuthenticated: false,
  token: null,
  usuario: null,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setAuth: (state, action: PayloadAction<{ token: string; usuario: string }>) => {
      state.isAuthenticated = true;
      state.token = action.payload.token;
      state.usuario = action.payload.usuario;
    },
    clearAuth: state => {
      state.isAuthenticated = false;
      state.token = null;
      state.usuario = null;
    },
  },
});

export const { setAuth, clearAuth } = authSlice.actions;
export default authSlice.reducer;
