# Configuración del MVP

## Identidad y autorización

Producción exige `Authentication__Mode=Entra`. Configurar `Entra__TenantId`, `Entra__ClientId` (registro de la API), `Entra__Scope=access_as_user` y `FrontendUrl=https://<frontend>`.
Registrar una SPA y una API de un solo tenant. La API debe emitir tokens v2 (`requestedAccessTokenVersion=2`), exponer el scope delegado `access_as_user` y proporcionar `preferred_username`/UPN. Autorizar ese scope para la SPA y conceder consentimiento.
Registrar `https://<frontend>/acceso` como redirect URI SPA. Publicar `auth-config.json` con `mode: Entra`, `tenantId`, `clientId` de la SPA y `apiScope: api://<api-client-id>/access_as_user`. Son identificadores públicos; nunca poner un client secret en Angular.

Preaprovisionar `TBL_Usuario.Username` con UPN normalizado en minúsculas y `EntraObjectId` con el Object ID del usuario en ese tenant. El primer administrador y los roles/permisos aprobados requieren aprovisionamiento SQL controlado; no ejecutar el seeder demo en producción. Mantener IDs internos para no perder trazabilidad. Los usuarios corporativos nuevos usan `PasswordHash=''`; no se valida ni cambia en modo Entra. Los usuarios locales existentes no obtienen acceso corporativo automáticamente.
Cada petición valida firma, emisor, audiencia, vigencia, tenant y scope; luego exige usuario/rol activos en SQL. Los permisos y centros se reconstruyen desde SQL, sin confiar en roles externos. Desactivar un usuario revoca acceso en la siguiente petición. Reaprovisionar el UPN si cambia y revisar todos los vínculos antes de cambiar de tenant.

MSAL conserva su caché en la sesión del navegador y renueva silenciosamente el access token. La API se publica bajo `/api` en el mismo origen mediante proxy del hosting; no se adjuntan tokens a URLs externas. Un 401 lleva al acceso; un 403 conserva sesión y muestra el error. El frontend vuelve a consultar permisos al iniciar sesión/recargar.

## SQL y migraciones

Local: `ConnectionStrings__SqlServer=Server=localhost\SQLEXPRESS;Database=SistemaLlantas;Integrated Security=True;Encrypt=True;TrustServerCertificate=True`.

Azure: `ConnectionStrings__SqlServer=Server=tcp:<servidor>.database.windows.net,1433;Database=<base>;Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False`. Para identidad administrada asignada por usuario, añadir `User Id=<client-id-identidad>`.
Alternativa Service Principal: `Authentication=Active Directory Service Principal;User Id=<client-id>;Password=<secreto>`; guardar la cadena completa en variables protegidas/Key Vault. El backend rechaza `Active Directory Interactive`.
Crear el usuario de base de datos correspondiente a la identidad y conceder permisos mínimos de ejecución/lectura/escritura. Usar otra identidad con permisos DDL para migrar. No se aplican migraciones al arrancar producción.

Desde la raíz: `dotnet tool restore`, después `dotnet ef migrations script --idempotent --project backend/src/SistemaLlantas.Infrastructure --startup-project backend/src/SistemaLlantas.Api --output artifacts/database.sql` con configuración del entorno. Revisar/aplicar el SQL antes de arrancar la nueva API. La migración Entra agrega una columna nullable y un índice único filtrado; no modifica migraciones anteriores.

## Desarrollo y CI

API: `ASPNETCORE_ENVIRONMENT=Development`, `Authentication__Mode=Local`. Para crear usuarios demo explícitamente: `Authentication__SeedDevelopmentUsers=true` únicamente en una base local de desarrollo. La clave HMAC se genera por proceso si no se proporciona `Jwt__Key`; reiniciar invalida esas sesiones. Frontend: `pnpm start:local`. Los tokens locales solo permanecen en memoria.

CI compila y prueba .NET/Angular, verifica el modelo EF y genera artefactos API, Angular y SQL. Integración crea y elimina una base `SistemaLlantas_Test_<guid>` por fixture en LocalDB. Para otro servidor de pruebas configurar `TEST_SQL_CONNECTION`; la identidad requiere crear/eliminar esas bases. No utilizar una conexión empresarial. CI no despliega.

Pendientes operativos: aprobar registros Entra y usuarios iniciales, hosting con HTTPS/proxy `/api`, identidad y red de Azure SQL, respaldo/restauración y almacenamiento persistente para `App_Data` (evidencias). No ejecutar varias réplicas con discos locales independientes. Historial, Analítica y Auditoría se muestran explícitamente pendientes; la trazabilidad por llanta y el resumen existente siguen disponibles.

Se conservan DOCX/PDF funcionales. Se retiran logs, tarballs y PNG de renderizado QA, reproducibles desde los documentos. Los índices existentes de llantas por código/serial/centro-estado y las consultas paginadas se conservan. Los listados históricos/exportaciones sin límite requieren medir volumen real antes de declarar capacidad de 20.000+ llantas.

Referencias: [MSAL y adquisición de tokens](https://learn.microsoft.com/en-us/entra/msal/javascript/browser/acquire-token), [identidad administrada en Azure SQL](https://learn.microsoft.com/en-us/azure/azure-sql/database/authentication-azure-ad-user-assigned-managed-identity?view=azuresql).
