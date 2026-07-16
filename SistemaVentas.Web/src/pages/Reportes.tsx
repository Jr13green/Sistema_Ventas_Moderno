import React, { useEffect, useState } from 'react';
import { Box, Typography, Card, CardContent, CircularProgress } from '@mui/material';
import { Bar } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';
import api from '../services/api';

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

interface ReporteDiario {
  fecha: string;
  totalVentas: number;
  totalJugadas: number;
}

export default function Reportes() {
  const [reporte, setReporte] = useState<ReporteDiario[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api
      .get<ReporteDiario[]>('/reportes/semana')
      .then(r => {
        setReporte(r.data);
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
    labels: reporte.map(r => new Date(r.fecha).toLocaleDateString()),
    datasets: [
      {
        label: 'Ventas',
        data: reporte.map(r => r.totalVentas),
        backgroundColor: 'rgba(26,115,232,0.8)',
        borderRadius: 4,
      },
    ],
  };

  return (
    <Box>
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        Reportes
      </Typography>
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Ventas Semanales
          </Typography>
          <Bar data={chartData} options={{ responsive: true }} />
        </CardContent>
      </Card>
    </Box>
  );
}
