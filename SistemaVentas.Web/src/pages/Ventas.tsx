import React, { useEffect, useState } from 'react';
import {
  Box,
  Typography,
  Button,
  TextField,
  Card,
  CardContent,
  Alert,
} from '@mui/material';
import { DataGrid, GridColDef, GridRenderCellParams } from '@mui/x-data-grid';
import api from '../services/api';

interface Venta {
  id: number;
  fecha: string;
  total: number;
  estado: string;
}

export default function Ventas() {
  const [ventas, setVentas] = useState<Venta[]>([]);
  const [numero, setNumero] = useState('');
  const [monto, setMonto] = useState('');
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');

  const columns: GridColDef<Venta>[] = [
    { field: 'id', headerName: 'ID', width: 80 },
    {
      field: 'fecha',
      headerName: 'Fecha',
      width: 180,
      renderCell: (params: GridRenderCellParams<Venta, string>) =>
        new Date(params.value ?? '').toLocaleString(),
    },
    {
      field: 'total',
      headerName: 'Total',
      width: 120,
      renderCell: (params: GridRenderCellParams<Venta, number>) =>
        `$${Number(params.value ?? 0).toFixed(2)}`,
    },
    { field: 'estado', headerName: 'Estado', width: 120 },
  ];

  const loadVentas = () => {
    api
      .get<Venta[]>('/ventas')
      .then(r => setVentas(r.data))
      .catch(console.error);
  };

  useEffect(() => {
    loadVentas();
  }, []);

  const handleCrear = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    try {
      await api.post('/ventas', {
        usuarioId: 1,
        jugadas: [{ sorteoDiarioId: 1, numero, monto: parseFloat(monto) }],
      });
      setSuccess('Venta creada correctamente');
      setNumero('');
      setMonto('');
      loadVentas();
    } catch {
      setError('Error al crear la venta');
    }
  };

  return (
    <Box>
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        Ventas
      </Typography>
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Nueva Venta
          </Typography>
          {success && (
            <Alert severity="success" sx={{ mb: 2 }}>
              {success}
            </Alert>
          )}
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <Box component="form" onSubmit={handleCrear} sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
            <TextField
              label="Número"
              value={numero}
              onChange={e => setNumero(e.target.value)}
              size="small"
              required
            />
            <TextField
              label="Monto"
              type="number"
              value={monto}
              onChange={e => setMonto(e.target.value)}
              size="small"
              required
            />
            <Button type="submit" variant="contained">
              Crear Venta
            </Button>
          </Box>
        </CardContent>
      </Card>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Historial
          </Typography>
          <DataGrid rows={ventas} columns={columns} autoHeight pageSizeOptions={[10, 25, 50]} />
        </CardContent>
      </Card>
    </Box>
  );
}
