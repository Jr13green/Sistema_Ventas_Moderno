import React from 'react';
import { Box, Typography, Card, CardContent } from '@mui/material';

export default function Configuracion() {
  return (
    <Box>
      <Typography variant="h4" fontWeight="bold" gutterBottom>
        Configuración
      </Typography>
      <Card>
        <CardContent>
          <Typography variant="body1">
            Panel de configuración del sistema. Permite administrar parámetros del negocio,
            usuarios, y preferencias del sistema.
          </Typography>
        </CardContent>
      </Card>
    </Box>
  );
}
