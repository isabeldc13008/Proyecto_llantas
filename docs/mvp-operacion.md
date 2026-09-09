# Autenticación propia y SQL

La API emite JWT HS256 al verificar el hash de la contraseña del usuario de TBL_Usuario.
Cada petición autenticada vuelve a consultar el usuario activo, rol y permisos; desactivar un usuario revoca su acceso.
El frontend mantiene el token en memoria; recargar o cerrar la página requiere iniciar sesión nuevamente.

## Arranque local con la base existente

Ejecutar dotnet run --project backend/src/SistemaLlantas.Api --launch-profile local. Development utiliza Authentication=Active Directory Interactive: al abrir la primera conexión SQL solicita iniciar sesión con Microsoft y completar MFA si lo requiere la cuenta. No modifica la autenticación propia de GLLD.
La conexión apunta a srvsqlgdlldllo.database.windows.net, base GDLLSQLDLLO, con cifrado y validación de certificado.
En otra consola: cd frontend/sistema-llantas; npm start. Abrir http://localhost:4200.
Las credenciales SQL permiten a la API conectarse; el formulario requiere un usuario de la aplicación con hash de contraseña y rol activo.

## App Service

Configurar en variables del entorno del backend:
- ASPNETCORE_ENVIRONMENT=Production
- Jwt__Key: clave aleatoria secreta de al menos 32 bytes; persistente y compartida entre instancias.
- ConnectionStrings__SqlServer: Server=tcp:srvsqlgdlldllo.database.windows.net,1433;Database=GDLLSQLDLLO;Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30
- Usar la identidad asignada por el sistema de GLLDAPWBDLLO (Id. de objeto d4ff5ded-491d-4f14-8e3e-e247acfa1bde), previamente autorizada dentro de la base. No se requieren SqlCredentials__Username ni SqlCredentials__Password.
- FrontendUrl: origen HTTPS exacto del frontend.

Publicar frontend y API bajo el mismo origen con /api dirigido al backend. Activar HTTPS en el hosting.
No se aplican migraciones ni usuarios demo automáticamente en producción. Authentication:SeedDevelopmentUsers permanece false.
Las migraciones históricas se conservan: la columna de identidad anterior queda sin uso, no se elimina de bases existentes.
El modelo y su snapshot se alinearon con las 43 tablas y 574 columnas de los metadatos de GDLLSQLDLLO aportados el 2026-09-09. No se ejecutó DDL ni se alteró el historial de migraciones. Las migraciones históricas crean el esquema anterior: no usarlas para reconstruir esta base ni aplicarlas automáticamente. Para nuevas bases se requiere preparar una línea base coherente con este modelo.
No usar Initialize-LocalDatabase.ps1 contra esta base existente.
No se ha validado una conexión real ni un login con las credenciales SQL del usuario.
