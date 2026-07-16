import { configureStore } from '@reduxjs/toolkit';
import authReducer from './slices/authSlice';
import ventasReducer from './slices/ventasSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    ventas: ventasReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
