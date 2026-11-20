# Profundidad de inspecciones hasta 25 mm — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permitir lecturas exterior, centro e interior de 0 a 25 mm en Inspecciones.

**Architecture:** Se conserva el selector Angular en pasos de 0,5 mm y se amplía su colección hasta 25 mm. El DTO del backend aplica el mismo máximo como frontera de seguridad; no se modifica persistencia ni analítica.

**Tech Stack:** Angular 20, TypeScript, Jasmine/Karma, .NET 10, C#, DataAnnotations, xUnit.

**Spec:** `docs/superpowers/specs/2026-11-19-profundidad-inspecciones-25-design.md`

## Global Constraints

- Conservar selectores e incrementos de 0,5 mm.
- Aceptar valores entre 0 y 25 mm, inclusive.
- No modificar profundidad inicial, alertas, analítica, reglas de desgaste ni esquema de base de datos.

## Review Focus

- El valor exacto 25 debe aparecer en el selector y ser aceptado por el servidor.
- El valor 25,5 debe ser rechazado por el servidor.
- Los valores negativos deben continuar rechazándose.
- Los tres campos deben compartir exactamente el mismo límite.
- Los valores nulos deben seguir permitidos por el DTO durante el guardado parcial.

---

### Task 1: Ampliar el límite coordinado de profundidad

**Files:**
- Modify: `frontend/sistema-llantas/src/app/features/inspection/inspection-page.ts`
- Modify: `frontend/sistema-llantas/src/app/features/inspection/inspection-page.spec.ts`
- Modify: `backend/src/SistemaLlantas.Application/Inspecciones/InspeccionDtos.cs`
- Create: `backend/tests/SistemaLlantas.Application.Tests/InspeccionDetalleDtoTests.cs`

**Interfaces:**
- Consumes: `InspectionPage.depthOptions` y `GuardarDetalleInspeccionDto` existentes.
- Produces: opciones `0, 0.5, …, 25` y validación DataAnnotations inclusiva `[0,25]` para las tres profundidades.

- [ ] **Step 1: Escribir la prueba frontend que falla**

Agregar a `inspection-page.spec.ts` una prueba que compruebe que `depthOptions[0]` es `0`, el último valor es `25`, la longitud es `51` y cada incremento es `0.5`.

- [ ] **Step 2: Ejecutar la prueba frontend y confirmar el fallo**

Run: `npm test -- --watch=false --include='src/app/features/inspection/inspection-page.spec.ts'`

Expected: FAIL porque el último valor actual es `15` y la longitud es `31`.

- [ ] **Step 3: Escribir las pruebas backend que fallan**

Crear `InspeccionDetalleDtoTests` usando `Validator.TryValidateObject` para comprobar, en cada propiedad, que `25` es válido, `25.5` es inválido, `-0.5` es inválido y `null` es válido.

- [ ] **Step 4: Ejecutar las pruebas backend y confirmar el fallo**

Run: `dotnet test backend/tests/SistemaLlantas.Application.Tests/SistemaLlantas.Application.Tests.csproj --filter FullyQualifiedName~InspeccionDetalleDtoTests`

Expected: FAIL en los casos con `25`, porque el máximo actual es `15`.

- [ ] **Step 5: Implementar el cambio mínimo**

En `inspection-page.ts`, generar `51` opciones mediante `Array.from({length:51}, (_, index) => index * .5)`. En `InspeccionDtos.cs`, cambiar las tres anotaciones a `[Range(0, 25)]`.

- [ ] **Step 6: Ejecutar las pruebas focalizadas**

Run: `npm test -- --watch=false --include='src/app/features/inspection/inspection-page.spec.ts'`

Run: `dotnet test backend/tests/SistemaLlantas.Application.Tests/SistemaLlantas.Application.Tests.csproj --filter FullyQualifiedName~InspeccionDetalleDtoTests`

Expected: PASS en ambas suites.

- [ ] **Step 7: Ejecutar verificación de regresión**

Run: `npm test -- --watch=false`

Run: `dotnet test backend/SistemaLlantas.sln`

Expected: todas las pruebas aprobadas.

- [ ] **Step 8: Revisar el diff**

Run: `git diff --check`

Expected: sin errores de espacios ni cambios fuera del alcance.
