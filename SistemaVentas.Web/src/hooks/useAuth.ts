import { useSyncExternalStore } from 'react';

const AUTH_EVENT = 'sistema-ventas-auth-changed';

function getSnapshot() {
  return localStorage.getItem('token');
}

function subscribe(callback: () => void) {
  const handler = () => callback();
  window.addEventListener('storage', handler);
  window.addEventListener(AUTH_EVENT, handler);

  return () => {
    window.removeEventListener('storage', handler);
    window.removeEventListener(AUTH_EVENT, handler);
  };
}

function notifyAuthChange() {
  window.dispatchEvent(new Event(AUTH_EVENT));
}

export function useAuth() {
  const token = useSyncExternalStore(subscribe, getSnapshot, () => null);
  const isAuthenticated = !!token;

  const login = (newToken: string) => {
    localStorage.setItem('token', newToken);
    notifyAuthChange();
  };

  const logout = () => {
    localStorage.removeItem('token');
    notifyAuthChange();
  };

  return { isAuthenticated, token, login, logout };
}
