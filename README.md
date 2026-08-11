# Sistema de Gestión de Llantas

Monolito modular empresarial para administrar el ciclo de vida de llantas. La primera entrega contiene arquitectura limpia, API protegida por JWT/permisos, catálogos relacionados, inventario maestro de llantas, auditoría, concurrencia y frontend Angular.

## Requisitos

- .NET SDK 10.0.203 o compatible
- Node.js compatible con Angular 20 y pnpm
- SQL Server

## Inicio rápido

1. Ajuste `ConnectionStrings:SqlServer` mediante variables de entorno o secretos de usuario.
2. Defina `Jwt__Key` con al menos 32 caracteres. No guarde claves reales en el repositorio.
3. Ejecute `dotnet ef database update --project backend/src/SistemaLlantas.Infrastructure --startup-project backend/src/SistemaLlantas.Api`.
4. Inicie la API con `dotnet run --project backend/src/SistemaLlantas.Api --urls http://localhost:5080`.
5. En `frontend/sistema-llantas`, ejecute `pnpm start`.

La API exige un JWT emitido por el proveedor de identidad, con claims `permiso` y opcionalmente `centro_id`. Consulte [docs/arquitectura.md](docs/arquitectura.md).
