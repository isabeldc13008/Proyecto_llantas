# Autenticación consecutiva Microsoft Entra e interna

La integración queda preparada y **deshabilitada** (`Entra:RequireDualAuthentication=false`). El login interno, roles, permisos y centros siguen vigentes. No se ejecutaron migraciones ni se cambió el modelo de usuarios.

Se asume provisionalmente un registro compartido SPA/API por ambiente, por instrucción del solicitante. No se creó otro Client ID ni se incorporó un Client Secret.

| Ambiente | Client ID | Tenant ID | Scope |
|---|---|---|---|
| Desarrollo | 44b4eaad-a201-404a-87f0-82277b0ebdde | 1be5ad33-84d3-42a0-a64a-1b673e4e9944 | api://44b4eaad-a201-404a-87f0-82277b0ebdde/access_as_user |
| QA | f168fa27-40f3-409c-9fd9-19513d7659e5 | 1be5ad33-84d3-42a0-a64a-1b673e4e9944 | api://f168fa27-40f3-409c-9fd9-19513d7659e5/access_as_user |

## Pendientes de TI y activación manual

Confirmar por ambiente las URI exactas de retorno y salida autorizadas como SPA (`RedirectUri`, `PostLogoutRedirectUri`), el registro compartido y el consentimiento del scope delegado. TI confirmó access tokens v1, por lo que `TokenVersion` queda en `1.0`; las URI siguen vacías hasta que TI entregue los valores autorizados. Confirmar que QA carga `appsettings.QA.json` (ambiente QA) o sus valores equivalentes mediante configuración del App Service; no se modifica el despliegue.

Solo después de la validación de TI: configurar esos valores y activar `Entra__RequireDualAuthentication=true`, con reinicio controlado. La aplicación rechaza una activación con configuración incompleta. No puede verificar por sí sola que una URI esté autorizada en el portal Entra. Las sesiones internas anteriores no vinculadas requerirán ambos inicios de sesión nuevamente.

## Contrato y seguridad

Angular obtiene configuración pública de `/api/auth/local-config`. MSAL usa Authorization Code con PKCE y almacenamiento de sesión. Primero autentica Microsoft, después solicita las credenciales internas existentes. No se persisten credenciales internas ni tokens Microsoft en `glld_session`.

El login interno valida explícitamente el bearer Microsoft antes de consultar usuarios cuando está activado. Firma el JWT interno con `entra_tid` y `entra_oid`. Cada endpoint protegido exige el JWT interno en `Authorization` y el access token Entra en `X-Entra-Authorization`; la API verifica firma RSA mediante metadata HTTPS, issuer, audience, expiración, versión, tenant, cliente y scope `access_as_user`, además de la igualdad de identidad vinculada. Los roles y centros se siguen leyendo del usuario interno. El encabezado adicional debe conservarse en los proxies existentes; no registrar tokens en logs.

La vinculación es de sesión (dos credenciales válidas); no inventa un vínculo permanente entre cuentas Microsoft y usuarios internos. Si TI exige asociación nominal permanente, debe definir su política y fuente autorizada antes de extender el modelo.

Para v1 se espera issuer `https://sts.windows.net/{tenant}/`, audience `api://{client}` y `appid`; para v2 issuer `https://login.microsoftonline.com/{tenant}/v2.0`, audience GUID y `azp`. Contrastar con el token real emitido antes de activar.

## Verificación pendiente en ambientes

Con cuentas de prueba autorizadas: completar Microsoft y login interno; probar credenciales internas erróneas, falta/expiración de cada token, token de otro ambiente/tenant/scope, intercambio de identidad Microsoft, recarga, cierre de sesión y preservación de permisos y centros. La autenticación real no puede verificarse hasta confirmar la configuración pendiente. Las pruebas automatizadas focalizadas no contactan SQL ni Entra.

Referencias: [validación de claims](https://learn.microsoft.com/en-us/entra/identity-platform/claims-validation), [inicialización MSAL Angular](https://learn.microsoft.com/en-us/entra/msal/javascript/angular/initialization).

## Archivos de esta intervención

- API: `Controllers/AuthController.cs`, `Security/AuthenticationConfiguration.cs`, nuevo `Security/EntraAuthentication.cs`, `Program.cs`.
- Configuración API: `appsettings.json`, `appsettings.Development.json`, nuevo `appsettings.QA.json`.
- Angular: `src/app/app.config.ts`, `core/auth/auth.service.ts`, nuevo `core/auth/entra-auth.service.ts`, `core/http/interceptors.ts`, `features/auth/login.html`.
- Dependencias: `package.json`, `package-lock.json`, `pnpm-lock.yaml` (MSAL Angular 4.1.1 y MSAL Browser 4.30.0).
- Pruebas nuevas: `backend/tests/SistemaLlantas.Api.IntegrationTests/EntraAuthenticationTests.cs`, `frontend/sistema-llantas/src/app/core/auth/entra-auth.spec.ts`.
- Este documento. Los cambios preexistentes de Inspecciones y otros archivos no pertenecen a esta intervención.
