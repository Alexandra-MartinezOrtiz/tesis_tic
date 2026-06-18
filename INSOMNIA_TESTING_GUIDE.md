# 🧪 Guía: Probando TIC-FIS Backend en Insomnia

## ⚡ Inicio Rápido

### 1. **Descargar e Instalar Insomnia**
- Descargar desde: https://insomnia.rest/download
- Instalar en tu sistema

### 2. **Importar la Colección**
1. Abre **Insomnia**
2. Haz clic en **Create** → **Import from file**
3. Selecciona el archivo: `TIC-FIS_Insomnia_Collection.json`
4. ¡Listo! Se importará automáticamente con todos los endpoints

### 3. **Verificar que el Backend está Corriendo**
Ejecuta desde PowerShell (en `c:\ESPACIO_DE_TRABAJO\TESIS`):
```powershell
.\start-dev.ps1
```

O manualmente:
```powershell
# Terminal 1: PostgreSQL
cd backend
docker-compose up

# Terminal 2: Identity Service (puerto 5001)
cd backend\services\identity-service
dotnet run --project Identity.Api

# Terminal 3: Propuestas Service (puerto 5002)
cd backend\services\propuestas-service
dotnet run --project Propuestas.Api

# Terminal 4: Reportes Service (puerto 5003)
cd backend\services\reportes-service
dotnet run --project Reportes.Api

# Terminal 5: API Gateway (puerto 5000) - Opcional
cd backend\gateway\TicFis.ApiGateway
dotnet run
```

---

## 📋 Estructura de Endpoints

La colección está organizada en **4 carpetas principales**:

### 1. 🔐 **Auth (Identity Service)** - Puerto 5001
Autenticación y gestión de sesiones

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/auth/login` | `POST` | Iniciar sesión y obtener tokens |
| `/api/auth/refresh` | `POST` | Refrescar token de acceso |
| `/api/auth/forgot-password` | `POST` | Solicitar cambio de contraseña |
| `/api/auth/reset-password` | `POST` | Restablecer contraseña |

### 2. 👥 **Usuarios (Admin)** - Puerto 5001
Gestión de usuarios y roles *(requiere rol Admin)*

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/usuarios` | `GET` | Listar todos los usuarios |
| `/api/usuarios` | `POST` | Crear nuevo usuario |
| `/api/usuarios/{id}` | `PUT` | Actualizar usuario |
| `/api/usuarios/{id}/roles` | `POST` | Asignar roles a usuario |

### 3. 📋 **Propuestas** - Puerto 5002
Gestión completa del ciclo de vida de propuestas

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/propuestas` | `GET` | Listar propuestas (con filtro de estado) |
| `/api/propuestas/{id}` | `GET` | Obtener detalle de propuesta |
| `/api/propuestas` | `POST` | Crear nueva propuesta |
| `/api/propuestas/{id}` | `PUT` | Actualizar propuesta |
| `/api/propuestas/{id}/enviar-revision` | `POST` | Enviar a revisión |
| `/api/propuestas/{id}/aprobar` | `POST` | Aprobar propuesta (CPGIC) |
| `/api/propuestas/{id}/rechazar` | `POST` | Rechazar propuesta (CPGIC) |
| `/api/propuestas/{id}/pendiente` | `POST` | Marcar como pendiente (CPGIC) |

### 4. 📊 **Reportes** - Puerto 5003
Consultas y exportación de reportes

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/api/reportes/propuestas` | `GET` | Listar propuestas con filtros |
| `/api/reportes/propuestas/{id}` | `GET` | Obtener detalle para reporte |
| `/api/reportes/propuestas/export/pdf` | `GET` | Exportar a PDF |
| `/api/reportes/propuestas/export/excel` | `GET` | Exportar a Excel |

---

## 🔑 Flujo de Uso Recomendado

### **Paso 1: Autenticarse**
1. Abre el endpoint **"1. Login"** en la carpeta **Auth**
2. Modifica el email/password según tus datos de usuario (por defecto: `docente@example.com`)
3. Haz clic en **Send**
4. Copia el `accessToken` de la respuesta
5. **IMPORTANTE**: Pega el token en la variable de entorno:
   - Abre **Environment: TIC-FIS Local**
   - Busca `access_token` 
   - Pega el valor completo del token
   - Haz clic en **Save**

```json
// Respuesta típica del login
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

### **Paso 2: Probar Propuestas**
Con el token ya configurado, ahora puedes probar:

1. **Crear propuesta**: `POST /api/propuestas`
   - Modifica los datos en el body según sea necesario
   - Haz clic en **Send**

2. **Listar propuestas**: `GET /api/propuestas`
   - Puedes ajustar los filtros de `page`, `pageSize`, `estado`
   - Haz clic en **Send**

3. **Obtener detalles**: `GET /api/propuestas/{id}`
   - Reemplaza `{id}` por el ID de una propuesta que exista

4. **Actualizar**: `PUT /api/propuestas/{id}`
   - Modifica los datos y envía

5. **Cambios de estado**:
   - **Enviar a revisión**: `POST /api/propuestas/{id}/enviar-revision`
   - **Aprobar** (requiere rol CPGIC): `POST /api/propuestas/{id}/aprobar`
   - **Rechazar** (requiere rol CPGIC): `POST /api/propuestas/{id}/rechazar`
   - **Marcar pendiente**: `POST /api/propuestas/{id}/pendiente`

### **Paso 3: Probar Reportes**
Una vez tengas propuestas aprobadas:

1. **Listar reportes**: `GET /api/reportes/propuestas`
   - Puedes filtrar por `estado` y `busqueda`

2. **Exportar PDF**: `GET /api/reportes/propuestas/export/pdf`
   - La respuesta descargará un archivo PDF

3. **Exportar Excel**: `GET /api/reportes/propuestas/export/excel`
   - La respuesta descargará un archivo XLSX

---

## 📝 Credenciales por Defecto

Para probar sin crear nuevos usuarios, usa estas credenciales:

| Rol | Email | Password | Notas |
|-----|-------|----------|-------|
| Docente | `docente@example.com` | `Password123!` | Puede crear y editar propuestas |
| CPGIC | `cpgic@example.com` | `Password123!` | Puede revisar y aprobar propuestas |
| Admin | `admin@example.com` | `Admin123!` | Acceso a gestión de usuarios |

> **Nota**: Estos datos vienen de las migraciones de BD. Verifica `backend/sql/` si necesitas otros usuarios.

---

## 🔧 Configuración de Variables de Entorno

En Insomnia, el archivo ya tiene las variables configuradas:

```
base_url = http://localhost:5000          (API Gateway)
identity_url = http://localhost:5001      (Identity Service)
propuestas_url = http://localhost:5002    (Propuestas Service)
reportes_url = http://localhost:5003      (Reportes Service)
access_token = <aquí va tu token>
refresh_token = <token para refrescar>
```

Para cambiar la URL base (ej. en producción):
1. Abre **Environment: TIC-FIS Local**
2. Modifica los valores según sea necesario
3. Haz clic en **Save**

---

## 🚀 Ejemplo Completo: Crear y Aprobar una Propuesta

### 1. Login
```bash
POST http://localhost:5001/api/auth/login
{
  "email": "docente@example.com",
  "password": "Password123!"
}
```
**Copia el `accessToken` a la variable `access_token`**

### 2. Crear Propuesta
```bash
POST http://localhost:5002/api/propuestas
Authorization: Bearer {{ access_token }}

{
  "titulo": "Mi Primera Propuesta",
  "descripcion": "Descripción de la propuesta",
  "problema": "El problema a resolver",
  "objetivoGeneral": "El objetivo general",
  "alcance": "El alcance del proyecto"
}
```
**Nota el ID de la propuesta creada (ej: 42)**

### 3. Enviar a Revisión
```bash
POST http://localhost:5002/api/propuestas/42/enviar-revision
Authorization: Bearer {{ access_token }}
```

### 4. Login como CPGIC
```bash
POST http://localhost:5001/api/auth/login
{
  "email": "cpgic@example.com",
  "password": "Password123!"
}
```
**Actualiza el `access_token`**

### 5. Aprobar Propuesta
```bash
POST http://localhost:5002/api/propuestas/42/aprobar
Authorization: Bearer {{ access_token }}
{
  "comentario": "Aprobada. Cumple con los requisitos."
}
```

### 6. Consultar en Reportes
```bash
GET http://localhost:5003/api/reportes/propuestas/42
Authorization: Bearer {{ access_token }}
```

### 7. Exportar PDF/Excel
```bash
GET http://localhost:5003/api/reportes/propuestas/export/pdf?estado=Aprobada
Authorization: Bearer {{ access_token }}
```

---

## ⚠️ Problemas Comunes

### **401 Unauthorized**
- El token expiró o es inválido
- **Solución**: Repite el login y copia el nuevo token a las variables

### **403 Forbidden**
- No tienes permisos para esa acción (ej. CPGIC intentando crear propuestas)
- **Solución**: Usa un usuario con el rol correcto

### **404 Not Found**
- La propuesta/usuario no existe
- **Solución**: Verifica el ID o crea primero el recurso

### **Connection Refused**
- El servicio no está corriendo en ese puerto
- **Solución**: Ejecuta `.\start-dev.ps1` o levanta los servicios manualmente

### **PostgreSQL No Inicia**
- Docker Desktop no está corriendo
- **Solución**: Abre Docker Desktop e intenta de nuevo

---

## 📚 Recursos Adicionales

- **Documentación de API**: Cada servicio expone Swagger en `/swagger/index.html`
  - Identity: `http://localhost:5001/swagger/index.html`
  - Propuestas: `http://localhost:5002/swagger/index.html`
  - Reportes: `http://localhost:5003/swagger/index.html`

- **Documentación de arquitectura**: Ver `DOC/arquitectura_tic_fis_modulos_abc.md`

- **Scripts SQL**: Ver `backend/sql/` para entender la estructura de BD

---

## 💡 Tips de Productividad

### Guardar respuestas automáticamente
1. En Insomnia, haz clic en **Timeline** (abajo a la derecha)
2. Verás el historial de todas tus solicitudes

### Crear solicitudes rápidamente
1. Duplica una solicitud existente
2. Modifica solo lo que necesites
3. Haz clic en **Save**

### Usar variables en el body
Puedes usar `{{ variable_name }}` en cualquier parte:
```json
{
  "token": "{{ access_token }}",
  "userId": 1
}
```

### Autocompletar URLs
Insomnia sugiere automáticamente URLs según tus búsquedas recientes

---

¡Listo para probar! 🎉 Cualquier duda, consulta la sección de problemas comunes o revisa los logs del backend.
