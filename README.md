# Proyecto_llantas

Migración del proyecto a arquitectura **Angular + ASP.NET Core**.

## Nueva estructura

- `frontend/`: aplicación Angular (SPA) con enrutamiento por módulos de menú.
- `backend/`: API REST en ASP.NET Core (Minimal API) para registros de vehículos, llantas e inspecciones.
- `index.html`, `app.js`, `views/`: versión anterior en HTML + JS (se conservan como referencia histórica).

## Backend (ASP.NET Core)

Requisitos:

- .NET SDK 8.0+

Ejecución:

```bash
cd backend
dotnet run
```

API base:

- `GET /api/records`
- `GET /api/records?type=vehicle`
- `POST /api/records`
- `PUT /api/records/{id}`
- `DELETE /api/records/{id}`

Por defecto se habilitó CORS abierto para facilitar desarrollo con Angular en local.

## Frontend (Angular)

Requisitos:

- Node.js 22+
- npm 11+

Instalación y ejecución:

```bash
cd frontend
npm install
npm run start
```

La aplicación Angular consume el backend en `http://localhost:5000/api/records`.

## Estado de la migración

- ✅ Base del sistema migrada a Angular (router + layout + páginas principales).
- ✅ Base del API migrada a ASP.NET Core.
- ⚠️ La lógica avanzada (modales, carga masiva, validaciones específicas del flujo anterior) quedó lista para implementarse sobre la nueva arquitectura.
