# QA: diagnóstico y preparación del despliegue

## Alcance y protección

Correcciones en `fix/qa-arranque-despliegue`, basada en `origin/qa` (1b6e35f).
No se cambia `main`, el workflow de Desarrollo, su conexión ni recursos Azure.
No se ejecutan migraciones en Azure, database update, push, merge ni despliegue.

Destino exclusivo: RG_PRBS_GDLL / GLLDAPWBPRBS / plan GDLLAPSPPRBS.
SQL esperado: srvsqlgdllprbs.database.windows.net / GDLLSQLPRBS.

## Evidencia del fallo

El 19 de octubre de 2026 la URL QA responde HTTP 500 y muestra 500.30.
La CLI Azure disponible devuelve AuthorizationFailed al leer el sitio y su configuración
con la suscripción activa. No fue posible consultar la excepción interna ni verificar
las variables reales del App Service. No se cambiaron permisos ni se copiaron credenciales.

**La causa exacta del 500.30 no está confirmada.** La ausencia de Angular explica que
falte la interfaz, pero no demuestra una excepción de arranque de ASP.NET Core.

Problemas confirmados del repositorio:

- QA publicaba toda la solución en un solo directorio: incluía proyectos de pruebas y
  podía mezclar artefactos. Desarrollo publica explícitamente SistemaLlantas.Api.csproj.
- QA no compilaba ni copiaba Angular. Desarrollo sí lo hace.
- QA elegía SDK 10.x, mientras global.json fija 10.0.400 con rollForward latestPatch.
- El lockfile no declaraba @playwright/test ^1.64.1 ni @types/node ^26.6.5, presentes en
  package.json. `pnpm install --frozen-lockfile` reprodujo ERR_PNPM_OUTDATED_LOCKFILE.

## Cambios preparados

- .github/workflows/qa_glldapwbprbs.yml: Node 22, pnpm 11.19.0 y frozen-lockfile;
  build Angular production; SDK desde global.json; build/publish exclusivamente API;
  Angular en wwwroot; validación de DLL, deps, runtimeconfig, web.config, configuración QA
  e index.html; prueba de arranque antes de generar el artefacto.
- El paquete excluye appsettings.Development.json, sin modificar el archivo fuente.
- Despliegue restringido a refs/heads/qa y GLLDAPWBPRBS. Conserva exactamente la referencia
  al secreto publish-profile que ya tenía QA. No usa el secreto de Desarrollo.
- backend/src/SistemaLlantas.Api/appsettings.QA.json y Infrastructure/DependencyInjection.cs:
  SqlCredentials:AllowOverrides=false en QA impide reemplazar User ID/Password de la cadena
  por claves heredadas. Fuera de QA, sin esa opción, el comportamiento anterior se conserva.
- backend/tests/SistemaLlantas.Api.IntegrationTests/QaStartupConfigurationTests.cs: 10 pruebas
  sin conexión SQL: autenticación SQL implícita/explicita, overrides y errores de configuración.
- frontend/sistema-llantas/pnpm-lock.yaml: sincronizado por pnpm, sin cambiar package.json.
  Se conserva la instalación estricta en CI; no se elimina frozen-lockfile.
- scripts/Test-QaPackage.ps1: inicia el paquete local con entorno QA, firma efímera y
  conexión ficticia 127.0.0.1:1. Comprueba Angular HTTP 200 y API inexistente HTTP 404.
  No ejecuta consultas ni migraciones. No altera variables persistentes del equipo.

## Arranque y configuración

Program.cs utiliza WebApplication.CreateBuilder(args): carga appsettings.json,
appsettings.QA.json y después variables de entorno. FrontendUrl ya es sobrescribible.
No necesita agregar manualmente proveedores JSON ni nuevas claves secretas al repositorio.

AddInfrastructure usa configuration.GetConnectionString("SqlServer"). Si falta la clave,
lanza `Falta ConnectionStrings:SqlServer.`; una cadena mal formada también puede lanzar
excepción al construir SqlConnectionStringBuilder. Esto ocurre antes de abrir SQL.

AuthenticationConfiguration rechaza una clave Jwt ausente fuera de Development o con
menos de 32 bytes UTF-8. Issuer/Audience vienen de appsettings.json. La comprobación es
correcta y no debe relajarse para ocultar un error de configuración.

El único seeder automático está condicionado a IsDevelopment() y
Authentication:SeedDevelopmentUsers=true. QA tiene false. No se registran servicios
hospedados que migren o consulten tablas durante el arranque. Las tablas de seguridad se
consultan al iniciar sesión o validar un token, no para servir index.html.

Con WebApplicationBuilder, DOTNET_ENVIRONMENT tiene precedencia sobre
ASPNETCORE_ENVIRONMENT: no dejar valores contradictorios.

## Configuración que debe verificar un administrador de Azure

En GLLDAPWBPRBS, revisar sin exportar valores secretos:

| Configuración | Valor/condición |
|---|---|
| ASPNETCORE_ENVIRONMENT | QA |
| DOTNET_ENVIRONMENT | Ausente o QA |
| ConnectionStrings__SqlServer | Servidor srvsqlgdllprbs.database.windows.net; base GDLLSQLPRBS; credencial exclusiva QA |
| Jwt__Key | Secreto propio de QA, aleatorio y de al menos 32 bytes; preferiblemente referencia Key Vault |
| FrontendUrl | Origen real del frontend QA, sin ruta ni barra final |
| Authentication__SeedDevelopmentUsers | false o ausente |
| SqlCredentials__AllowOverrides | false en QA; no sobrescribirlo con true en Azure |
| SqlCredentials__Username / Password | Evitar claves heredadas; con AllowOverrides=false quedan ignoradas |
| Stack / hosting | Runtime ASP.NET Core .NET 10 compatible con el paquete y arquitectura del worker |

No mezclar la entrada ConnectionStrings__SqlServer con otra cadena SqlServer que apunte
al ambiente incorrecto. Revisar también Azure connection strings SQLAZURECONNSTR/SQLCONNSTR.
El slot llamado Production en el workflow es el slot principal **del sitio QA**, no el
App Service de Desarrollo ni un recurso de producción.

## Registros necesarios para confirmar el 500.30

Un administrador con acceso al recurso correcto debe obtener, alrededor de la hora del fallo:

1. App Service > Diagnose and solve problems > disponibilidad/errores de arranque.
2. Kudu > LogFiles > eventlog.xml: excepción de ASP.NET Core Module V2 y stderr.
3. Si no hay detalle, habilitar temporalmente stdout del módulo ANCM SOLO en QA,
   reproducir el fallo una vez y recoger LogFiles/stdout*.log; después deshabilitarlo.
4. Verificar en Kudu el contenido desplegado: web.config, DLL, runtimeconfig,
   appsettings.QA.json y wwwroot/index.html. Comparar runtime instalado con net10.0.

Compartir excepción, stack trace y timestamp, ocultando cadenas de conexión, claves y tokens.
Un despliegue verde solo acredita la transferencia del paquete, no que el proceso arrancó.

## Base de datos y permisos

Hay migraciones EF en backend/src/SistemaLlantas.Infrastructure/Persistence/Migrations.
La última incluida es AddDisposicionSistemaVerde (20261006162816).
No se ha inspeccionado ni alterado GDLLSQLPRBS; su esquema y datos iniciales quedan pendientes
para una revisión autorizada separada.

El usuario de ejecución necesita conexión y SELECT/INSERT/UPDATE/DELETE sobre las tablas
utilizadas por la aplicación, con alcance mínimo; no necesita db_owner ni permisos DDL para
el arranque QA. Las migraciones requieren una identidad administrativa separada cuando se
autoricen. Login requiere usuarios, roles, permisos y centros existentes; no sembrar usuarios
de Desarrollo en QA. Revisar firewall/red y el método SQL/Entra elegido sin probar credenciales
contra Desarrollo.

## Validación local

Validación del 19/10/2026: 341 pruebas backend existentes aprobadas; 10 pruebas nuevas de configuración aprobadas; 191 pruebas Angular aprobadas. Instalación frozen con pnpm 11.19.0 y Node 22.23.3 correcta en copia aislada. Build Angular production, publish API y smoke QA con Angular/API correctos. La prueba de paquete es local:
no acredita runtime instalado, configuración, conectividad ni esquema de Azure QA.
Las pruebas Playwright existentes son ejemplos que navegan a playwright.dev; no validan
Gestión de Llantas ni sustituyen aceptación del ambiente QA.

## Excepciones localizadas y diff de la corrección QA

- WebApplication.CreateBuilder(args): JSON inválido (FormatException/InvalidDataException),
  archivos o directorios sin acceso (IOException/UnauthorizedAccessException), content root
  inválido o argumentos de configuración inválidos. No contiene aquí acceso SQL. Un entorno
  distinto puede cargar otro archivo; no necesariamente lanza excepción.
- AddInfrastructure: conexión ausente (InvalidOperationException); palabra clave o formato
  inválido de SqlConnectionStringBuilder (ArgumentException/FormatException según valor).
  La opción AllowOverrides debe ser true/false; un valor distinto lanza FormatException.
  Credenciales válidas sintácticamente pero incorrectas no se autentican durante este registro:
  fallarán al abrir SQL, no explican por sí solas el 500.30.
- AddApplicationAuthentication: InvalidOperationException si falta Jwt__Key fuera de
  Development, si tiene menos de 32 bytes o si faltan Jwt:Issuer/Jwt:Audience.
- Una FrontendUrl incorrecta puede causar problemas CORS; no se ha demostrado que sea
  la excepción de arranque de este sitio.

Reproducción de la corrección: antes, la prueba esperaba inline_user con AllowOverrides=false
pero obtenía legacy_user. Después, preserva la cadena completa. Con true conserva el comportamiento
anterior. Esta reproducción no demuestra que las variables heredadas existan en Azure QA.

```diff
+  "SqlCredentials": {
+    "AllowOverrides": false
+  },
```

```diff
- if (connectionBuilder.Authentication is SqlAuthenticationMethod.NotSpecified
-     or SqlAuthenticationMethod.SqlPassword)
+ var allowOverrides = configuration["SqlCredentials:AllowOverrides"];
+ if ((allowOverrides is null || bool.Parse(allowOverrides))
+     && (connectionBuilder.Authentication is SqlAuthenticationMethod.NotSpecified
+         or SqlAuthenticationMethod.SqlPassword))
```

Los nombres abreviados del enum en este fragmento son solo para lectura; el diff de Git
contiene los nombres completos. Program.cs, autenticación JWT, appsettings.Development.json,
workflow de Desarrollo y package.json no se modificaron.
