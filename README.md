# Sistema de Gestión de Llantas

Angular 20, ASP.NET Core/.NET 10 y EF Core/SQL Server. Se conserva Domain / Application / Infrastructure / Api.

## Desarrollo

- .NET SDK según `global.json`, Node 22 y pnpm 11.19.0.
- SQL Server local: `localhost\SQLEXPRESS`; configurar `ConnectionStrings__SqlServer` si cambia.
- `dotnet tool restore` y `dotnet ef database update --project backend/src/SistemaLlantas.Infrastructure --startup-project backend/src/SistemaLlantas.Api` con `ASPNETCORE_ENVIRONMENT=Development`.
- Para usuarios demo, habilitar explícitamente `Authentication__SeedDevelopmentUsers=true` solo en una base de desarrollo.
- API: `dotnet run --project backend/src/SistemaLlantas.Api` (puerto 5262).
- Frontend: en `frontend/sistema-llantas`, `pnpm install --frozen-lockfile` y `pnpm start:local`.

Producción utiliza Entra ID y autorización interna SQL. Configuración, aprovisionamiento, Azure SQL, migraciones y restricciones de despliegue: [operación del MVP](docs/mvp-operacion.md).

## Verificación

`dotnet test` ejecuta Domain, Application e Integration. Integración utiliza bases aisladas en LocalDB o el servidor indicado por `TEST_SQL_CONNECTION`, sin datos previos requeridos.
Frontend: `pnpm test --watch=false --browsers=ChromeHeadless` y `pnpm build`.
GitHub Actions ejecuta ambas suites y genera artefactos; no despliega.
