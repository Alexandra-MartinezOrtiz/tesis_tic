/**
 * Contratos de rutas HTTP alineados al API Gateway (YARP) y sección 13 de la arquitectura.
 * El front debe apuntar a la base del gateway en desarrollo (p. ej. http://localhost:5000).
 */
export const ApiEndpoints = {
  gatewayBase: '/api',
  auth: {
    login: '/api/auth/login',
    refresh: '/api/auth/refresh',
    forgotPassword: '/api/auth/forgot-password',
    resetPassword: '/api/auth/reset-password',
  },
  usuarios: '/api/usuarios',
  propuestas: '/api/propuestas',
  reportes: '/api/reportes',
} as const;
