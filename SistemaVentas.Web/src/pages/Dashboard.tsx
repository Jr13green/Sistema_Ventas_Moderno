import React, { useEffect, useState } from 'react';
import { Box, Grid, Card, CardContent, Typography, CircularProgress } from '@mui/material';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';
import api from '../services/api';

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

interface DashboardData {
  totalVentasHoy: number;
  totalJugadasHoy: number;
  saldoCaja: number;
  ventasSemana: Array<{ fecha: string; total: number }>;
}

export default function Dashboard() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .get<DashboardData>('/reportes/dashboard')
      .then(r => {
        setData(r.data);
        setLoading(false);
      })
      .catch(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" mt={8}>
        <CircularProgress />
      </Box>
    );
  }

  const chartData = {
    labels: data?.ventasSemana?.map(v => new Date(v.fecha).toLocaleDateString()) ?? [],
    datasets: [
      {
        label: 'Ventas',
        data: data?.ventasSemana?.map(v => v.total) ?? [],
        borderColor: '#1a73e8',
        backgroundColor: 'rgba(26,115,232,0.1)',
        fill: true,
        tension: 0.4,
      },
    ],
  };

  return (
    <Box>
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        Dashboard
      </Typography>
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {[
          { label: 'Ventas Hoy', value: `$${data?.totalVentasHoy?.toFixed(2) ?? '—'}` },
          { label: 'Jugadas Hoy', value: data?.totalJugadasHoy ?? '—' },
          { label: 'Saldo en Caja', value: `$${data?.saldoCaja?.toFixed(2) ?? '—'}` },
        ].map((item, i) => (
          <Grid item xs={12} sm={4} key={i}>
            <Card>
              <CardContent>
                <Typography color="text.secondary">{item.label}</Typography>
                <Typography variant="h4" fontWeight="bold" color="primary">
                  {item.value}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Ventas de la Semana
          </Typography>
          <Line
            data={chartData}
            options={{ responsive: true, plugins: { legend: { position: 'top' } } }}
          />
        </CardContent>
      </Card>
    </Box>
  );
}
