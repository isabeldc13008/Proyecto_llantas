# Listas dependientes de novedades en Inspecciones Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement parametrizable condition→cause→recommendation lists for inspections without changing existing inventory or inspection workflows.

**Architecture:** Add two EF Core join entities and tables keyed by stable catalog codes through migration seed SQL. Extend the inspection options contract with relation data and enforce active relationships in `InspeccionService`. Angular derives filtered lists locally, clears downstream selections on changes, and preserves persisted historical values.

**Tech Stack:** .NET 10, ASP.NET Core, EF Core SQL Server, Angular standalone components, TypeScript, xUnit.

**Spec:** `docs/superpowers/specs/2026-11-11-inspecciones-listas-dependientes-design.md`

## Global Constraints

- Do not execute migrations, deploy QA, or merge into `develop`.
- Do not include unrelated QA, documentation, or analytics changes.
- Do not delete or rename existing catalogs or historical inspection records.
- Stable catalog codes identify seed relationships; visible names are not relationship keys.

## Review Focus

- A condition change must clear both dependent selections and prevent stale IDs from being submitted; test in the Angular component.
- A cause change must clear recommendation and filter recommendation options; test in the Angular component.
- `SIN_NOVEDAD` must allow `Continuar operación` without a cause; test backend validation and component state.
- Existing inspections with inactive or removed relationships must still load their saved IDs; test DTO mapping and frontend fallback.
- Invalid condition/cause/recommendation combinations must be rejected server-side without changing inventory; test the service/controller path.

### Task 1: Add relationship domain model and EF migration

**Files:**
- Modify: `backend/src/SistemaLlantas.Domain/Entities/Inspecciones.cs`
- Modify: `backend/src/SistemaLlantas.Infrastructure/Persistence/LlantasDbContext.cs`
- Modify: `backend/src/SistemaLlantas.Infrastructure/Persistence/EntityConfigurations.cs`
- Create: `backend/src/SistemaLlantas.Infrastructure/Persistence/Migrations/<timestamp>_AddInspectionNoveltyRelations.cs`
- Create: matching migration designer file and update model snapshot
- Test: `backend/tests/SistemaLlantas.Api.IntegrationTests/InspectionNoveltyRelationsTests.cs`

- [ ] Write a failing model test asserting each relationship has unique condition/cause keys and active navigation.
- [ ] Run the focused test and verify it fails because relationship entities/tables do not exist.
- [ ] Add `CondicionCausaLlanta` and `CausaRecomendacionInspeccion` entities, DbSets, configurations, indexes and foreign keys.
- [ ] Add migration SQL that creates/seeds relations idempotently by catalog codes, including the supplied condition/cause matrix and existing/new recommendation codes; do not execute it.
- [ ] Run model and migration metadata tests.

### Task 2: Extend options and backend validation

**Files:**
- Modify: `backend/src/SistemaLlantas.Application/Inspecciones/InspeccionDtos.cs`
- Modify: `backend/src/SistemaLlantas.Infrastructure/Services/InspeccionService.cs`
- Modify: `backend/src/SistemaLlantas.Api/Controllers/InspeccionesController.cs`
- Test: `backend/tests/SistemaLlantas.Api.IntegrationTests/InspectionNoveltyRelationsTests.cs`

- [ ] Add failing tests for filtered options, invalid pair rejection, valid `SIN_NOVEDAD`, and no inventory mutation.
- [ ] Run tests to verify expected failures.
- [ ] Return relation records in `/api/inspecciones/opciones` and validate active condition/cause/recommendation links before saving.
- [ ] Preserve historical saved IDs when reading existing inspections and avoid changing movement/inventory code.
- [ ] Run focused integration tests and confirm they pass.

### Task 3: Implement Angular dependent selectors

**Files:**
- Modify: `frontend/sistema-llantas/src/app/features/inspection/inspection-page.ts`
- Modify: `frontend/sistema-llantas/src/app/features/inspection/inspection-page.spec.ts`

- [ ] Add failing component tests for disabled state, filtering, cascade clearing, `SIN_NOVEDAD`, and persisted values.
- [ ] Run the frontend focused tests and verify they fail for the missing behavior.
- [ ] Add relation-aware option types and computed filters; reset dependent state on selection changes; keep observation available.
- [ ] Disable cause/recommendation until prerequisites exist and preserve historical values when they are loaded.
- [ ] Run focused frontend tests.

### Task 4: Verify whole scope and handoff

- [ ] Run backend build and focused/full tests.
- [ ] Run frontend lint/test/build commands available in the repository.
- [ ] Inspect `git diff` and confirm only branch-scoped files changed.
- [ ] Confirm no database migration was executed and no QA deployment occurred.
- [ ] Commit the branch changes with a focused message; do not merge or push unless separately requested.
