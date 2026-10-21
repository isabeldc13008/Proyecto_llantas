# Primer administrador QA

La rama incorpora un bootstrap exclusivo para `admin.qa`. No ejecuta migraciones, no crea objetos, roles ni permisos. Solo inserta una fila en TBL_Usuario; no usa SaveChangesAsync para evitar la auditoría adicional automática de EF. Se reutiliza PasswordHasher<UsuarioSistema> con las mismas opciones predeterminadas que AuthController.

## Configuración de Azure App Service

Configurar como ajustes del slot QA (no copiar a Producción):

| Nombre | Valor |
|---|---|
| ASPNETCORE_ENVIRONMENT | QA |
| Authentication | true |
| Authentication__Mode | Local |
| Authentication__QaAdminBootstrap__Enabled | true, solo durante el bootstrap |
| Authentication__QaAdminBootstrap__ExpectedServer | Endpoint exacto aprobado, igual al Data Source de la conexión |
| Authentication__QaAdminBootstrap__InitialPassword | Referencia Azure Key Vault a un secreto exclusivo de 16–256 caracteres |
| ConnectionStrings__SqlServer | Conexión segura existente con Initial Catalog=GDLLSQLPRBS, Encrypt=True y TrustServerCertificate=False |

Mantener Jwt__Key, Jwt__Issuer y Jwt__Audience configurados por el mecanismo seguro existente; esta modificación no los cambia. No configurar DOTNET_ENVIRONMENT con un valor distinto: el código usa el entorno efectivo de ASP.NET Core, que debe ser QA. No guardar el secreto en appsettings, código, Git, ticket o logs. El secreto debe resolver correctamente antes del arranque. Usar referencias Key Vault accesibles mediante la identidad del App Service.

## Comportamiento

Si el entorno no es exactamente QA, Authentication no es true o Enabled no es true, el componente retorna sin abrir conexiones. Cuando está habilitado, valida el catálogo y endpoint declarados, prohíbe failover/attach/user-instance y exige TLS con certificado válido antes de conectar. Confirma DB_NAME() después de abrir. La identidad física del endpoint/alias debe ser confirmada por TI.

El bootstrap abre una transacción serializable y obtiene un sp_getapplock exclusivo con propietario Transaction (15 segundos de espera). La búsqueda de usuario utiliza UPDLOCK/HOLDLOCK y normaliza espacios/capitalización para no crear una identidad equivalente. Si existe, termina sin modificar hash, flags, rol ni permisos, incluso si el usuario está inactivo. En caso contrario exige un único rol ADMINISTRADOR activo con las cinco concesiones activas: modulos.resumen.consultar, modulos.administracion.consultar, modulos.auditoria.consultar, catalogos.administrar, centros.ver_todos. Se permiten concesiones adicionales y se conservan. No se ejecuta ningún INSERT/UPDATE en tablas de rol o permisos.

El nuevo usuario queda activo, con cambio obligatorio y sin centro. Se verifica el hash antes del INSERT parametrizado. La transacción completa mantiene los locks hasta commit; ante error, se revierte al disponer la transacción, se libera el applock y el arranque falla con mensaje genérico sin secretos ni excepción SQL interna.

Permisos SQL mínimos a revisar por DBA: lectura de usuario/rol/relaciones/permisos, INSERT en usuario y posibilidad de usar sp_getapplock en la base. No requiere DDL. La cuenta de ejecución y los índices únicos existentes no se modifican.

Después de confirmar la creación y el login/cambio obligatorio en una intervención autorizada, establecer Enabled=false y retirar InitialPassword. Los arranques posteriores con Enabled=true conservan un usuario ya existente; no restauran privilegios ni credenciales.

## Validación disponible

19 casos QaAdminBootstrapTests aprobados sin bases: condiciones, destinos/TLS, usuario existente, rol ausente/inactivo/incompleto, secreto inválido, flags, hash y repetición. Ejecutados con filtro FullyQualifiedName~QaAdminBootstrapTests, sin iniciar la aplicación ni fixtures SQL. Compilación correcta y git diff --check correcto.

Pendiente de autorización independiente: ejecución real del SQL parametrizado, concurrencia entre procesos, rollback/locks con SQL Server, permisos de identidad Azure, conexión/certificado y login HTTP. Las pruebas con almacenamiento simulado no acreditan esos comportamientos reales.

El bootstrap no necesita ni sustituye el inicializador por fases ni resuelve la discrepancia histórica de migraciones: presupone el esquema QA compatible ya provisionado por TI. Ninguna operación contra una base se ejecutó durante la implementación.
