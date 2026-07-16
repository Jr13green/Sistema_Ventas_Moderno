export const Formatters = {
  currency: (amount: number, currency = 'USD'): string =>
    new Intl.NumberFormat('es-DO', { style: 'currency', currency }).format(amount),

  date: (date: string | Date): string =>
    new Intl.DateTimeFormat('es-DO', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    }).format(new Date(date)),

  number: (num: string): string => num.toString().padStart(2, '0'),
};
