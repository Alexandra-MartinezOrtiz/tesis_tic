# GUÍA DE PRUEBAS: SWAGGER, REPORTES Y CONSULTAS

Proyecto: **TIC-FIS** — Microservicios .NET 10 + Angular 17
Módulo: **Consultas y Reportes** (Estudiante B)

Credenciales de prueba:
```json
{ "email": "admin@ticfis.local", "password": "Admin123!" }
```

---

## 0. CAMBIOS RECIENTES QUE AFECTAN LAS PRUEBAS

> Lee esto primero: el módulo cambió y las pruebas ahora se ven distinto.

- **Las propuestas se crean directamente como `aprobada`** (ya **no** como `borrador`).
- Al crear una propuesta se ingresa **`estudiantesPropuestos`** (número de 0 a 5). Con eso se calculan:
  - **Cupos** = `estudiantesPropuestos / 5` (ej. `3/5`)
  - **Disponible** = `Sí` si `estudiantesPropuestos < 5`; `No` si llega a 5.
- **No se permite guardar más de 5 estudiantes** (HTTP **400**).
- **Reportes muestra SOLO propuestas aprobadas** (las de otros estados no aparecen).
- Los contadores y filtros del módulo ahora son de **disponibilidad** (Total aprobadas / Disponibles / No disponibles), no de estado.
- La tabla muestra columna **Cupos (X/5)** y **Disponible (Sí/No)**.
- El **PDF** (listado y detalle) replica el formulario oficial **F_AA_233A** con estilo documento e incluye cupos/disponibilidad.
- **No hay exportación a Excel** (fuera del alcance). No existe ningún endpoint de Excel.
- En el formulario de creación, el **Código** es un campo de uso interno (no forma parte del documento oficial).

---

## 1. SWAGGER (PRUEBAS DE BACKEND)

| Servicio | Puerto | URL de Swagger |
|----------|--------|----------------|
| Identity Service (auth/usuarios) | 5001 | http://localhost:5001/swagger |
| Propuestas Service | 5002 | http://localhost:5002/swagger |
| Reportes Service | 5003 | http://localhost:5003/swagger |
| API Gateway (YARP) | 5000 | No tiene Swagger propio (es proxy) |

> Swagger solo está activo en modo `Development`. El token de Identity sirve para **los tres** servicios (mismo JWT).

---

## 2. AUTENTICACIÓN (TOKEN)

### Paso 1 — Obtener el token (Identity, 5001)
En `http://localhost:5001/swagger` ejecutar:
```
POST /api/auth/login
Body: { "email": "admin@ticfis.local", "password": "Admin123!" }
```
La respuesta devuelve **`accessToken`** (válido 60 min) y `refreshToken` (7 días). Copia el `accessToken`.

### Paso 2 — Autorizar en Swagger
En cada servicio que vayas a probar:
1. Clic en **Authorize** (candado).
2. Escribe: `Bearer <accessToken>`  *(incluye la palabra `Bearer` y un espacio)*.
3. Clic en **Authorize**.

> Si un endpoint responde **401**, el token expiró → repite el Paso 1 y vuelve a autorizar.

---

## 3. CREAR PROPUESTAS (Propuestas, 5002)

En `http://localhost:5002/swagger` → **Authorize** con `Bearer <token>` → `POST /api/propuestas` → **Try it out**.

**Campos del body** (`CreatePropuestaRequest`):
`codigo`, `titulo`, `descripcion`, `problema`, `objetivoGeneral`, `alcance`, **`estudiantesPropuestos`** (0–5).

### Casos de prueba (criterios de aceptación)

**Caso A — 0 estudiantes → 0/5 Disponible: Sí**
```json
{ "codigo": "TIC-SW-001", "titulo": "Propuesta sin estudiantes", "descripcion": "desc", "problema": "prob", "objetivoGeneral": "obj", "alcance": "alc", "estudiantesPropuestos": 0 }
```
**Caso B — 1 estudiante → 1/5 Disponible: Sí**
```json
{ "codigo": "TIC-SW-002", "titulo": "Propuesta con 1 estudiante", "descripcion": "desc", "problema": "prob", "objetivoGeneral": "obj", "alcance": "alc", "estudiantesPropuestos": 1 }
```
**Caso C — 5 estudiantes → 5/5 Disponible: No**
```json
{ "codigo": "TIC-SW-003", "titulo": "Propuesta llena", "descripcion": "desc", "problema": "prob", "objetivoGeneral": "obj", "alcance": "alc", "estudiantesPropuestos": 5 }
```
**Caso D — 6 estudiantes → debe RECHAZAR (HTTP 400)**
```json
{ "codigo": "TIC-SW-004", "titulo": "Demasiados", "estudiantesPropuestos": 6 }
```

**Qué confirmar en la respuesta:**
- Casos A, B, C → `201/200` con `"estadoActual": "aprobada"` y el `"estudiantesPropuestos"` enviado.
- Caso D → **HTTP 400**, mensaje *"El número de estudiantes propuestos debe estar entre 0 y 5."*

---

## 4. REPORTES Y CONSULTAS (Reportes, 5003)

En `http://localhost:5003/swagger` → **Authorize** con el **mismo** `Bearer <token>`.

### Endpoints

| Verbo | Ruta | Descripción |
|-------|------|-------------|
| GET | `/api/reportes/propuestas` | Listar **solo aprobadas** (con cupos y disponibilidad) |
| GET | `/api/reportes/propuestas/{id}` | Detalle de una propuesta aprobada |
| GET | `/api/reportes/propuestas/export/pdf` | PDF del listado (aprobadas, con Cupos/Disponible) |
| GET | `/api/reportes/propuestas/{id}/export/pdf` | PDF del detalle (formulario F_AA_233A) |
| GET | `/api/reportes/propuestas/export/pdf-formularios` | PDF con un formulario por propuesta |

**Query params:**
- `busqueda` — texto libre por **código, título o proponente**.
- `page`, `pageSize` — paginación.
- `estado` — **ya no es necesario**: el módulo siempre devuelve aprobadas (aunque se envíe, el resultado son aprobadas).

### Qué debes ver / probar

1. **Listar:** `GET /api/reportes/propuestas` → Execute.
   - Aparecen **solo las aprobadas** (las viejas en `borrador` NO salen).
   - Cada ítem trae ahora: `estudiantesPropuestos`, `cupoMaximo` (5) y `disponible` (true/false).

   Resultado esperado por caso:
   | Propuesta | estudiantesPropuestos | cupoMaximo | disponible |
   |---|---|---|---|
   | TIC-SW-001 | 0 | 5 | **true** |
   | TIC-SW-002 | 1 | 5 | **true** |
   | TIC-SW-003 | 5 | 5 | **false** |

2. **Buscar:** mismo endpoint con `busqueda = TIC-SW` → filtra por código/título/proponente.

3. **Ver detalle:** `GET /api/reportes/propuestas/{id}` con el `id` de una creada → devuelve descripción, objetivo, alcance, `estudiantesPropuestos`, `cupoMaximo`, `disponible` y la lista de estudiantes (si los hubiera).

4. **PDF del listado:** `GET /api/reportes/propuestas/export/pdf` → **Download file** → PDF de aprobadas con columnas **Cupos** y **Disponible**.

5. **PDF del detalle (F_AA_233A):** `GET /api/reportes/propuestas/{id}/export/pdf` → **Download file** → réplica del formulario oficial (estilo documento, secciones DATOS GENERALES / DESCRIPCIÓN / OBJETIVO / ALCANCE / COMPONENTES / SOLICITUD < 2 o > 5 / APROBACIONES) con los espacios en blanco para llenar a mano.

> **Excel:** no hay ningún endpoint de exportación a Excel. Si lo buscas en Swagger, no aparece (es correcto: está fuera del alcance).

---

## 5. PRUEBA TAMBIÉN EN LA INTERFAZ (Angular, 4200)

Entra a `http://localhost:4200` (mismo usuario):

- **Nueva propuesta** → estilo documento (gris/negro) igual al F_AA_233A; al escribir el "Número de participantes" aparecen los bloques de estudiante en blanco; no deja guardar con más de 5.
- **Consultas y Reportes** →
  - Contadores: **Total de propuestas aprobadas**, **Disponibles**, **No disponibles**, **Cupos ocupados**.
  - Filtros: **Todas / Disponibles / No disponibles** + buscador (código, título, proponente).
  - Tabla: Código, Título, Proponente, **Cupos (X/5)**, **Disponible (Sí/No)**, Últ. actualización, **Ver detalle**.
  - Botón **Imprimir listado** (PDF).
- **Ver detalle** → formulario F_AA_233A + botón **Imprimir formulario** (PDF de detalle).

---

## 6. ARCHIVOS CLAVE (referencia)

**Backend Propuestas**
- `Propuestas.Application/Dtos/PropuestaDtos.cs` — `estudiantesPropuestos` en Create/Update/List/Detail.
- `Propuestas.Infrastructure/Repositories/PropuestaRepository.cs` — estado inicial `aprobada`, guarda el número.
- `Propuestas.Application/Services/PropuestaService.cs` — validación 0–5.
- `backend/sql/ticfis_propuestas/003_add_estudiantes_propuestos.sql` — migración de la columna.

**Backend Reportes**
- `Reportes.Application/Dtos/ReporteDtos.cs` — `estudiantesPropuestos`, `cupoMaximo`, `disponible`.
- `Reportes.Infrastructure/Services/ReportesConsultaService.cs` — filtra aprobadas, calcula cupos/disponibilidad, genera PDF F_AA_233A (QuestPDF).

**Frontend**
- `features/propuestas/pages/propuesta-form.component.ts` — formulario estilo F_AA_233A con número de estudiantes y código interno.
- `features/reportes/pages/reportes-home.component.ts` — contadores, filtros de disponibilidad, columna Cupos.
- `features/reportes/pages/reporte-detalle.component.ts` — detalle F_AA_233A.
- `core/models/reporte.models.ts` y `core/models/propuesta.models.ts` — campos nuevos.

---

## 7. FLUJO RÁPIDO (RESUMEN)

1. Identity (5001) → `POST /api/auth/login` → copiar `accessToken`.
2. Propuestas (5002) → **Authorize** → `POST /api/propuestas` con `estudiantesPropuestos` (casos 0, 1, 5 y 6).
3. Reportes (5003) → **Authorize** → `GET /api/reportes/propuestas` (solo aprobadas, con cupos/disponible) → detalle → PDF.
4. Verificar: aprobada, cupos correctos, >5 rechazado, sin Excel.

---

*Actualizado: 2026-06-16 — alineado con el módulo de Consultas y Reportes (propuestas aprobadas, cupos y disponibilidad, PDF F_AA_233A, sin Excel).*
