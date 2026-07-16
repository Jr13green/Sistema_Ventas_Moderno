export const Validators = {
  isValidNumero: (numero: string): boolean => {
    const n = parseInt(numero, 10);
    return !Number.isNaN(n) && n >= 0 && n <= 99;
  },

  isValidMonto: (monto: string): boolean => {
    const m = parseFloat(monto);
    return !Number.isNaN(m) && m > 0 && m <= 10000;
  },

  isValidUsuario: (usuario: string): boolean =>
    usuario.trim().length >= 3 && usuario.trim().length <= 50,

  isValidContrasena: (contrasena: string): boolean => contrasena.length >= 6,
};
