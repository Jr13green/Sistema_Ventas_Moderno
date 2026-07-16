import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface AuthState {
  token: string | null;
  usuario: string | null;
}

const authSlice = createSlice({
  name: 'auth',
  initialState: { token: null, usuario: null } as AuthState,
  reducers: {
    setCredentials: (state, action: PayloadAction<{ token: string; usuario: string }>) => {
      state.token = action.payload.token;
      state.usuario = action.payload.usuario;
    },
    clearCredentials: state => {
      state.token = null;
      state.usuario = null;
    },
  },
});

export const { setCredentials, clearCredentials } = authSlice.actions;
export default authSlice.reducer;
