# Kiro Session Resume — AndyTipster V2

## What This Project Is

AndyTipster V2 is a complete rebuild of a horse racing tips subscription platform for UK/Ireland enthusiasts. It uses .NET 10 Web API + Angular 20 + NgRx + Tailwind/DaisyUI + EF Core + Azure SQL, with PayPal/Stripe payments, a custom CMS, tips engine, and multi-channel notifications.

## Spec Location

All spec documents are at `.kiro/specs/andytipster-v2-platform/`:
- `requirements.md` — 49 requirements across 7 build phases
- `design.md` — Architecture, data models, 19 correctness properties, testing strategy
- `tasks.md` — Full implementation plan (97 tasks total, 65 non-optional leaf tasks)
- `.config.kiro` — Spec config (requirements-first workflow, feature type)

## Current Progress

**5 of 97 tasks completed (Wave 0 + Wave 1 partial):**

| Task | Status | What Was Done |
|------|--------|---------------|
| 1.1 Create .NET 10 Web API with layered architecture | ✅ Done | Solution with 4 projects (Api/Application/Domain/Infrastructure), Serilog, ProblemDetails, health checks, CORS, rate limiting, security headers |
| 1.2 Set up EF Core and entity models | ✅ Done | 24 entities, 9 enumerations, DbContext with all configurations, Identity integration |
| 1.3 Create Angular 20 with NgRx | ✅ Done | 6 NgRx store slices, 4 lazy-loaded feature modules, JWT interceptor, route guards, OnPush everywhere |
| 1.4 Implement DaisyUI theme with dark mode | ✅ Done | Tailwind v4 + DaisyUI v5, custom light/dark themes, ThemeService, skeleton loaders, empty states, transition utilities |
| 1.6 Implement generic Data Table | ✅ Done | Pagination, sort, filter, search (300ms debounce), row selection, bulk actions, CSV/Excel export, mobile responsive |

## Next Task to Execute

**Task 1.7: Implement accessibility compliance baseline**
- Configure 4.5:1 contrast ratio in theme tokens
- Add ARIA labels/roles/states to all interactive components
- Full keyboard navigation with visible focus indicators
- Alt text enforcement for images
- Set up axe-core automated scanning in test pipeline
- Requirements: 49.1, 49.2, 49.3, 49.4, 49.5

After 1.7, the next wave unlocks:
- 2.1 User registration with email verification
- 2.11 Multi-role authorization system

## Branch & Git State

- **Branch:** `feature/phase1-foundation-scaffolding`
- **Remote:** Pushed to `origin` (https://github.com/dotnetdeveloper20xx/AndyTipsterV2)
- **Last commit:** `feat: scaffold platform foundation — .NET 10 API, Angular 20, NgRx, DaisyUI, EF Core`
- **Working tree:** Clean (all changes committed and pushed)

## How to Resume

Tell Kiro:

> Continue implementing the spec tasks for andytipster-v2-platform. Pick up from where we left off — task 1.7 is next. Build and test everything before marking complete.

Or to run a batch:

> Implement the next 5 tasks from the andytipster-v2-platform spec. Build and test before handing back.

## Key Technical Decisions Already Made

- .NET 10 with `net10.0` TFM (confirmed working)
- Angular 20.1.6 (CLI v20) with standalone components (no NgModules)
- NgRx 20.1.0 with signals-compatible API
- Tailwind CSS v4 with `@tailwindcss/postcss` (not v3 PostCSS plugin)
- DaisyUI v5 with `@plugin` syntax in CSS (not tailwind.config.js)
- Custom breakpoints: sm=375px, md=768px, lg=1024px, xl=1280px
- Theme names: `andytipster-light` and `andytipster-dark`
- Backend uses Minimal APIs + Controllers hybrid pattern
- All API errors use RFC 7807 ProblemDetails format
- EF Core with Identity (ApplicationUser extends IdentityUser<Guid>)
- 66 unit tests passing in Angular, 0 errors in .NET build

## Build Verification Commands

```bash
# Backend
cd src
dotnet build   # Expect: 0 warnings, 0 errors

# Frontend
cd src/andytipster-client
ng build       # Expect: successful bundle generation
ng test --watch=false --browsers=ChromeHeadless   # Expect: 66 SUCCESS
```

## Project Structure

```
src/
├── AndyTipster.slnx
├── AndyTipster.Api/           # Controllers, Endpoints, Middleware, Program.cs
├── AndyTipster.Application/   # Services, validators (mostly placeholder for now)
├── AndyTipster.Domain/        # 24 Entities + 9 Enumerations
├── AndyTipster.Infrastructure/ # EF Core DbContext + 24 entity configurations
└── andytipster-client/        # Angular 20 SPA
    └── src/app/
        ├── core/              # Guards (4), Interceptors (JWT), Services (7)
        ├── shared/            # DataTable, SkeletonLoader, EmptyState, ThemeToggle
        ├── store/             # NgRx: auth, user, roles, permissions, tips, plans
        └── features/          # public, auth, subscriber, admin (all lazy-loaded)
```
