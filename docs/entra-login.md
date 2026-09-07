# Login Microsoft Entra ID en localhost

## 1. Crear dos App Registrations en el mismo tenant

En https://entra.microsoft.com, entra en Entra ID > App registrations > New registration.

**GLLD API**

1. Nombre: `GLLD API`. Supported account types: Accounts in this organizational directory only (single tenant). Sin redirect URI.
2. Copia Directory (tenant) ID como `<TENANT_ID>` y Application (client) ID como `<API_CLIENT_ID>`. No copies el Object ID de la aplicación.
3. Manifest: dentro del objeto `api`, establece `requestedAccessTokenVersion` en `2`, sin reemplazar el resto del manifest.
4. Expose an API > Application ID URI: `api://<API_CLIENT_ID>`.
5. Add a scope: Scope name `access_as_user`; Who can consent `Admins only`; Admin consent display name `Acceder a GLLD`; descripción `Permite usar GLLD en nombre del usuario autenticado`; State `Enabled`.
6. El scope completo `<API_SCOPE>` queda `api://<API_CLIENT_ID>/access_as_user`.
7. Token configuration > Add optional claim > Access > `upn`. El backend acepta `preferred_username` o `upn`; además exige el Object ID registrado en SQL.

**GLLD SPA**

1. Nombre: `GLLD SPA`; mismo tipo de cuenta single tenant.
2. Authentication > Add a platform > Single-page application.
3. Redirect URI exacto: `http://localhost:4200/acceso` (sin slash final).
4. Copia Application (client) ID como `<SPA_CLIENT_ID>`; el tenant es el mismo de la API.
5. No habilites implicit grant ni crees client secrets. MSAL Browser usa authorization code con PKCE.
6. API permissions > Add a permission > My APIs > GLLD API > Delegated permissions > `access_as_user` > Add permissions. Si no aparece en My APIs, revisa que estés en el mismo tenant y seas propietario de los registros.
7. Un administrador autorizado debe ejecutar Grant admin consent for el tenant y confirmar que el permiso aparece concedido.
8. No necesitas Microsoft Graph ni roles de Entra para los permisos de GLLD. Si Enterprise applications > GLLD SPA exige asignación de usuarios, asigna los usuarios/grupos autorizados.

## 2. Configurar el proyecto

Editar `frontend/sistema-llantas/public/auth-config.json`:

```json
{
  "mode": "Entra",
  "tenantId": "<TENANT_ID>",
  "clientId": "<SPA_CLIENT_ID>",
  "apiScope": "api://<API_CLIENT_ID>/access_as_user",
  "redirectUri": "/acceso"
}
```

`/acceso` se resuelve contra el origen del navegador: en local, `http://localhost:4200/acceso`. También se acepta esa URL absoluta para localhost. Los IDs no son secretos; nunca poner una contraseña o un client secret en este JSON.

En una terminal PowerShell, desde la raíz `C:\Users\idelg\Documents\Proyecto_llantas`:

```powershell
$env:Entra__TenantId = '<TENANT_ID>'
$env:Entra__ClientId = '<API_CLIENT_ID>'
$env:Entra__Scope = 'access_as_user'
$env:FrontendUrl = 'http://localhost:4200'
dotnet run --project backend/src/SistemaLlantas.Api --launch-profile http
```

El perfil `http` activa Entra y escucha en `http://localhost:5262`. `Entra__Scope` es el nombre corto; `apiScope` del frontend es la URI completa. Alternativamente sustituir los placeholders de `backend/src/SistemaLlantas.Api/appsettings.json` con esos identificadores públicos. Variables de entorno tienen prioridad. La cadena SQL local está en appsettings.Development.json; sobrescribirla mediante `ConnectionStrings__SqlServer` si cambia.

En otra terminal:

```powershell
Set-Location C:\Users\idelg\Documents\Proyecto_llantas\frontend\sistema-llantas
npm start
```

También sirve `pnpm start`. El proxy existente lleva `/api` a `http://localhost:5262`. CORS de la API admite exactamente `http://localhost:4200` mediante FrontendUrl. Abrir `http://localhost:4200/acceso`, no file:// ni el puerto de la API.

## 3. Habilitar el usuario interno

Aplicar las migraciones existentes antes de iniciar sesión. En Entra ID > Users > usuario corporativo, copiar su User principal name y Object ID (del usuario, no de las aplicaciones).
Un administrador de GLLD/SQL debe aprovisionar `TBL_Usuario` con Username = UPN en minúsculas, EntraObjectId = ese Object ID, Activo = true, RolId = un rol activo y los centros autorizados en `TBL_UsuarioCentro`. Para un usuario corporativo nuevo PasswordHash puede ser cadena vacía; Microsoft gestiona su contraseña.
No se concede acceso por registrarse en Microsoft: `/api/auth/me` exige ese vínculo y devuelve nombre, username, role, roleName, permissions, centerIds y canViewAllCenters desde SQL. El primer administrador requiere aprovisionamiento controlado; no ejecutar el seeder demo en producción. No modificar IDs internos existentes ni inventar roles/centros.

## 4. Comprobar y conservar login local

Después de login, en Network del navegador `/api/auth/me` debe responder 200 con los permisos internos. El access token va en Authorization: Bearer. No copiar tokens a chats ni servicios públicos.

- Error de placeholders: sustituir los tres valores del JSON frontend y los dos de la API.
- AADSTS50011: corregir el redirect SPA exacto.
- Invalid scope/resource: revisar API_CLIENT_ID, Application ID URI, scope y consentimiento.
- 401 en `/api/auth/me`: comprobar perfil `http` (Entra), token v2 destinado a la API, UPN/Object ID y usuario/rol activos.
- 403 en módulos: revisar permisos internos; no es un problema de contraseña Microsoft.

Login propio de desarrollo: API `--launch-profile local` y frontend `npm run start:local` (o pnpm). Sigue limitado a Development/localhost. Nunca publicar este modo como autenticación empresarial.

Producción: registrar también `https://<dominio>/acceso` en la SPA, configurar FrontendUrl y el proxy `/api` del hosting. El redirect relativo se adapta al dominio. No hay secretos necesarios para este flujo SPA → API; Managed Identity para SQL es una configuración separada.

Referencias oficiales: [registro SPA](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app), [exponer API/scopes](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-configure-app-expose-web-apis), [permisos del cliente](https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-access-web-apis), [claims opcionales](https://learn.microsoft.com/en-us/entra/identity-platform/optional-claims-reference).
