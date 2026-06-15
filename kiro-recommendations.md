# Kiro Recommendations — Steering Files for AndyTipster V2

## Purpose

This document instructs Kiro on the steering files it must generate before beginning implementation of AndyTipster V2. These steering files live in `.kiro/steering/` and ensure that every piece of code produced meets the project's architecture, quality, design, and completeness standards.

**You (Kiro) must generate ALL of these steering files FIRST, before writing any application code.** Each steering file acts as a guardrail that you will reference continuously during implementation. Nothing is "done" until every steering file's criteria are satisfied.

---

## Steering Files to Generate

### 1. `architecture.md` — System Architecture & Project Structure

**Purpose:** Define the exact project structure, layering, and architectural decisions so every file lands in the right place.

**Must include:**
- Monorepo structure: `/api` (.NET 10 Web API) and `/client` (Angular 20 app)
- Backend layers: Controllers → Services → Repositories → Domain Entities
- CQRS pattern with MediatR for commands/queries
- Angular module structure: feature modules (lazy-loaded), shared module, core module
- NgRx folder conventions: store/, actions/, reducers/, effects/, selectors/ per feature
- Database schema conventions (naming, relationships, soft deletes)
- API versioning strategy
- Dependency injection registration patterns
- Configuration and secrets management (Azure Key Vault references)
- File naming conventions (kebab-case Angular, PascalCase .NET)
- Shared DTOs / API contracts between frontend and backend
- Azure deployment topology (App Service, Azure SQL, Blob Storage, CDN)

**Inclusion:** always

---

### 2. `requirements.md` — Functional Requirements Reference

**Purpose:** The single source of truth for what the application must do. Reference `v2-proposal.md` and encode every feature as a verifiable requirement.

**Must include:**
- All 16 modules from v2-proposal.md broken into numbered requirements
- Each requirement has: ID, description, acceptance criteria, priority (P0/P1/P2)
- Cross-references between related requirements
- Non-functional requirements (performance, security, compliance) as testable statements
- Explicitly mark which requirements are MVP (Phase 1–3) vs post-MVP

**Inclusion:** always

---

### 3. `coding-standards.md` — Code Style & Best Practices

**Purpose:** Enforce consistent, high-quality code across the entire codebase.

**Must include:**

**Backend (.NET 10):**
- Follow official .NET coding conventions (Microsoft style)
- Async/await everywhere (no `.Result` or `.Wait()`)
- Use `record` types for DTOs and value objects
- Nullable reference types enabled, no `null` without explicit handling
- Result pattern for service layer (no throwing exceptions for business logic)
- FluentValidation for all request validation
- Strongly-typed configuration with `IOptions<T>`
- Repository pattern with EF Core (no DbContext in controllers)
- Unit of Work pattern for transactions
- All public APIs documented with XML comments
- No magic strings — use constants or enums
- Guard clauses at method entry points

**Frontend (Angular 20):**
- Strict TypeScript (`strict: true`, no `any` types)
- OnPush change detection on every component
- Smart/Dumb (Container/Presentational) component pattern
- Reactive Forms exclusively (no template-driven forms)
- RxJS best practices: unsubscribe via `takeUntilDestroyed()`, avoid nested subscribes
- NgRx conventions: actions as `[Source] Event Name`, selectors prefixed with `select`
- Components must not exceed 200 lines (extract into sub-components)
- Services must not exceed 300 lines (split by responsibility)
- All user-facing strings must support future i18n (use constants file)
- Accessibility: ARIA labels, keyboard navigation, focus management
- No inline styles — Tailwind utility classes or component styles only

**Inclusion:** always

---

### 4. `api-design.md` — API Design Standards

**Purpose:** Ensure consistent, RESTful, well-documented API design.

**Must include:**
- RESTful URL conventions: `/api/v1/{resource}` (plural nouns)
- HTTP methods: GET (read), POST (create), PUT (full update), PATCH (partial), DELETE
- Standard response envelope: `{ data, meta, errors }`
- Pagination: cursor-based or offset with `page`, `pageSize`, `totalCount`
- Filtering: query parameters with `filter[field]=value`
- Sorting: `sort=field:asc,field2:desc`
- Error response format: `{ status, code, message, details[] }`
- HTTP status codes: 200, 201, 204, 400, 401, 403, 404, 409, 422, 500
- Authentication: Bearer JWT in Authorization header
- Rate limiting headers: `X-RateLimit-Limit`, `X-RateLimit-Remaining`
- API documentation via Swagger/OpenAPI (auto-generated)
- Versioning via URL path (`/api/v1/`, `/api/v2/`)
- CORS policy definition

**Inclusion:** always

---

### 5. `database-design.md` — Database Schema & Conventions

**Purpose:** Define database design rules, naming, and migration practices.

**Must include:**
- Table naming: PascalCase, plural (e.g., `Users`, `Subscriptions`, `Tips`)
- Column naming: PascalCase (e.g., `CreatedAt`, `IsActive`, `PayPalPlanId`)
- Every table has: `Id` (GUID or int), `CreatedAt`, `UpdatedAt`, `IsDeleted` (soft delete)
- Foreign key naming: `{RelatedEntity}Id`
- Index naming: `IX_{Table}_{Column}`
- Constraint naming: `FK_{Table}_{RelatedTable}`, `UQ_{Table}_{Column}`
- Use EF Core migrations (code-first)
- Seed data strategy for development
- Audit trail pattern (who changed what, when)
- Multi-tenant considerations (if future-proofing)
- Performance: identify columns needing indexes upfront
- No stored procedures — all logic in application layer
- JSON columns for flexible/dynamic data (e.g., CMS block content)

**Inclusion:** always

---

### 6. `ui-design.md` — UI/UX Design Standards

**Purpose:** Ensure every page and component meets visual and UX quality standards.

**Must include:**
- DaisyUI theme configuration (primary, secondary, accent, neutral colours)
- Typography scale (headings, body, captions — Tailwind classes)
- Spacing system (consistent use of Tailwind spacing: 4, 8, 12, 16, 24, 32, 48, 64)
- Component sizing: buttons (sm, md, lg), inputs, cards
- Dark mode implementation rules (DaisyUI `data-theme` switching)
- Animation guidelines: duration (150ms micro, 300ms standard, 500ms emphasis), easing curves
- Loading states: skeleton loaders for all data-fetching views
- Empty states: illustration + message + CTA for every empty list/view
- Error states: inline errors on forms, toast for async failures, full-page for critical
- Responsive rules: mobile-first, breakpoints at sm(640), md(768), lg(1024), xl(1280)
- Icon system: Heroicons or Lucide (consistent set, no mixing)
- Image handling: aspect ratios, lazy loading, placeholder blur
- Form design: labels above inputs, inline validation, clear error messages
- Accessibility: minimum contrast ratio 4.5:1, focus rings, screen reader support
- Page layout patterns: sidebar admin, full-width public, centered auth pages
- Z-index scale: dropdown(10), sticky(20), modal(30), toast(40), tooltip(50)

**Inclusion:** always

---

### 7. `testing-strategy.md` — Testing Requirements & Standards

**Purpose:** Define what tests are required and when code is considered adequately tested.

**Must include:**

**Backend:**
- Unit tests for all service methods (xUnit + Moq/NSubstitute)
- Integration tests for all API endpoints (WebApplicationFactory)
- Repository tests with in-memory database or test containers
- Minimum 80% code coverage on service layer
- Test naming: `MethodName_Scenario_ExpectedResult`
- Arrange-Act-Assert pattern
- No tests that depend on external services (mock everything)
- PayPal/Stripe webhook handler tests with sample payloads

**Frontend:**
- Unit tests for all NgRx reducers and selectors (Jest or Karma)
- Unit tests for all services (HttpClientTestingModule)
- Component tests for all smart components (TestBed + mocked store)
- No tests required for pure presentational (dumb) components unless complex logic
- E2E tests for critical user flows (Playwright):
  - Registration → email verify → login
  - Subscribe → PayPal checkout → access tips
  - Admin: create tip → publish → verify visible to subscriber
  - Admin: CMS page edit → publish → verify on public site
  - Subscription cancel → access revoked
- Visual regression tests for key pages (optional but recommended)
- Accessibility tests: axe-core integration in E2E suite

**Test runs required before "done":**
- All unit tests pass
- All integration tests pass
- All E2E tests pass
- No accessibility violations (axe-core)
- No TypeScript errors (`ng build --configuration production`)
- No .NET build warnings treated as errors

**Inclusion:** always

---

### 8. `security.md` — Security Standards & Checklist

**Purpose:** Security rules that must be followed in every piece of code.

**Must include:**
- Authentication: JWT with short expiry (15min access, 7d refresh), secure httpOnly cookies for refresh
- Authorization: policy-based auth on every endpoint, role checks in Angular guards
- Input validation: server-side validation on ALL endpoints (never trust client)
- SQL injection: parameterised queries only (EF Core handles this)
- XSS prevention: Angular's built-in sanitisation + CSP headers
- CSRF: anti-forgery tokens for state-changing operations
- CORS: whitelist only the Angular app origin
- Rate limiting: per-IP and per-user limits on auth endpoints
- Password: minimum 8 chars, bcrypt hashing, breach detection (HaveIBeenPwned API)
- Secrets: never in code, always Azure Key Vault or environment variables
- File upload: validate MIME type, limit size, scan for malware, store outside webroot
- PayPal webhooks: signature verification on every event
- Logging: never log passwords, tokens, or full card numbers
- Headers: `X-Content-Type-Options`, `X-Frame-Options`, `Strict-Transport-Security`
- Dependency scanning: check for known vulnerabilities in NuGet/npm packages

**Inclusion:** always

---

### 9. `code-review.md` — Self-Review Checklist

**Purpose:** Before presenting any code to the user, Kiro must self-review against this checklist.

**Must include:**

**Every file must pass:**
- [ ] Follows naming conventions from `coding-standards.md`
- [ ] No hardcoded values (use constants, config, or enums)
- [ ] No commented-out code
- [ ] No `TODO` or `HACK` comments left unresolved
- [ ] Error handling is comprehensive (no swallowed exceptions)
- [ ] Logging at appropriate levels (Info for flow, Warn for recoverable, Error for failures)
- [ ] No unused imports or dead code
- [ ] Accessibility attributes present (ARIA labels, alt text, roles)

**Every feature must pass:**
- [ ] API endpoint has request validation
- [ ] API endpoint has authorization attribute
- [ ] Database migration created and tested
- [ ] Frontend component has loading, error, and empty states
- [ ] Frontend form has validation with user-friendly messages
- [ ] Responsive on mobile, tablet, and desktop
- [ ] Dark mode works correctly
- [ ] NgRx actions, reducers, effects, and selectors are complete
- [ ] Unit tests written and passing
- [ ] No console.log or Debug.WriteLine left in code
- [ ] API returns proper HTTP status codes
- [ ] Swagger documentation is accurate

**Inclusion:** always

---

### 10. `definition-of-done.md` — Completion Criteria

**Purpose:** The ultimate checklist. Nothing is presented to the user as "done" until ALL of these are true. "Done" means a real user can log in, navigate every page, trigger every action, and see real responses — not placeholder screens.

**Must include:**

---

#### A. Full-Stack End-to-End Verification (THE PRIMARY GATE)

**Nothing is "done" unless the following journey works completely:**

1. **A user can register** → receive verification email → verify → land on dashboard
2. **A user can log in** → JWT issued → refresh works → session persists across page reload
3. **Every menu item is clickable** and navigates to a real, rendered page (never a blank/empty route)
4. **Every sub-menu item** leads to a fully functional page with real or seeded data
5. **Every page that shows a list** displays data (seeded if necessary — NEVER an empty table with no rows)
6. **Every button, link, and action on every page** performs its intended function
7. **Every form** submits successfully, validates inputs, shows errors, and persists data
8. **Every API endpoint** called from the frontend returns a proper response (never 404, never unhandled 500)
9. **The full request lifecycle works:** User action → Angular component → NgRx action → Effect → HTTP call → .NET Controller → Service → Repository → Database → Response → Effect → Reducer → Selector → Component re-renders with new data
10. **The full response lifecycle works:** API returns data → HTTP interceptor processes → NgRx store updates → Selectors emit → Component displays data → Loading states dismissed → User sees result

---

#### B. Navigation & Layout Completeness

**The application must ALWAYS have:**

1. **Full navigation menu** with ALL items for the user's role — never a partial menu
2. **All sub-menu items** present and routing to implemented pages
3. **Breadcrumb navigation** on all inner pages
4. **User profile menu** (top-right): avatar, name, profile link, settings, logout
5. **Notification bell** with unread count (even if zero)
6. **Settings page** with all sub-sections: profile, security, notifications, billing, privacy
7. **Mobile navigation** (hamburger menu) fully functional with all items
8. **Footer** with all links, social icons, legal pages
9. **Sidebar** (admin) with all sections expanded/collapsible, active state highlighted
10. **404 page** for unknown routes (styled, with navigation back)
11. **403 page** for unauthorized access attempts
12. **500 error page** for unexpected failures

---

#### C. List Pages — Generic Data Table Component

**Every list/table page in the application MUST use a shared generic data table component that provides:**

1. **Pagination** — configurable page size (10, 25, 50, 100), page navigation, total count
2. **Search** — global text search with debounce (300ms)
3. **Column filtering** — per-column filter dropdowns/inputs based on data type
4. **Sorting** — click column header to sort asc/desc, multi-column sort support
5. **Column visibility** — user can show/hide columns
6. **Row selection** — checkbox selection for bulk actions
7. **Bulk actions** — contextual action bar appears on selection (delete, export, status change)
8. **Export** — export filtered/selected data as CSV or Excel
9. **Loading state** — skeleton rows while fetching
10. **Empty state** — when no data exists: illustration + message + CTA to create first item
11. **Error state** — when API fails: error message + retry button
12. **Responsive** — horizontal scroll on mobile, or card view for small screens
13. **Row actions** — inline action menu (edit, delete, view) per row
14. **Quick filters** — preset filter chips (e.g., "Active", "Expired", "This Month")
15. **Saved filters** — user can save custom filter combinations

**This component is used for:** Users list, Subscribers list, Transactions list, Tips list, Blog posts list, FAQ list, Pages list, Media library, Audit log, Webhook events, Comments, Referrals — EVERY entity that has a list view.

---

#### D. Dashboard Pages — Never Empty

**Every dashboard MUST show:**

1. **Summary cards** with key metrics (animated counters)
2. **Charts** showing trends (even if data is minimal — show "Getting Started" state with sample data)
3. **Recent activity** feed (last 5–10 actions)
4. **Quick actions** — shortcuts to common tasks
5. **Status indicators** — system health, subscription status, upcoming events

**Admin dashboard:** subscriber count, MRR, today's tips status, recent signups, payment alerts
**Subscriber dashboard:** current plan, next billing date, today's tips, P&L summary, notifications

**If the database is empty (fresh install), show onboarding cards:**
- "Create your first subscription plan →"
- "Post your first tip →"
- "Customise your landing page →"

---

#### E. User Profile & Settings (Always Present)

**User profile page MUST include:**
1. Avatar with upload/change capability
2. Display name, email, phone (editable)
3. Connected social accounts
4. Subscription details (current plan, next billing, payment method)
5. Activity log (recent logins, actions)

**Settings page MUST include:**
1. **Profile tab** — edit personal info, avatar, bio
2. **Security tab** — change password, 2FA setup, active sessions, logout all devices
3. **Notifications tab** — per-channel toggles (email, push, Telegram), per-category toggles
4. **Billing tab** — current plan, payment history, update payment method, cancel
5. **Privacy tab** — data export, account deletion, consent preferences
6. **Appearance tab** — dark/light mode, theme preference

---

#### F. Per Feature/Task Checklist

1. Code compiles with zero errors and zero warnings
2. All coding standards from `coding-standards.md` are followed
3. All security rules from `security.md` are satisfied
4. API design follows `api-design.md` conventions
5. Database changes follow `database-design.md` conventions
6. UI matches `ui-design.md` standards (responsive, dark mode, animations, loading/error/empty states)
7. Unit tests written and passing (backend + frontend)
8. Integration tests written and passing
9. E2E test coverage for the user flow
10. No accessibility violations (axe-core passes)
11. Self-review checklist from `code-review.md` is 100% complete
12. The feature works end-to-end: User Action → Component → Store → API → DB → Response → UI Update
13. Build succeeds: `dotnet build` (zero warnings) + `ng build --configuration production`
14. No regressions: ALL existing tests still pass
15. Documentation updated (API swagger, README if architectural change)
16. Seeded data exists so the page is never empty on first load
17. All related menu items and navigation links are wired up
18. The page is reachable from the main navigation (not orphaned)

---

#### G. Per Phase Completion Checklist

1. All features in the phase meet the per-feature criteria above
2. All E2E tests for the phase pass together
3. Application starts and runs without errors (`dotnet run` + `ng serve`)
4. Database migrations apply cleanly from empty database
5. Seed data populates all list pages with sample data
6. A user can log in and navigate to EVERY page without hitting a dead end
7. Every API endpoint is reachable and returns expected response shape
8. No security vulnerabilities in dependency scan
9. Performance: page load < 3s, API response < 200ms for standard queries
10. Lighthouse score: Performance > 90, Accessibility > 95, Best Practices > 90, SEO > 90
11. Dark mode works on every page without broken colours/contrast
12. Mobile responsive: every page usable at 375px width
13. No console errors in browser DevTools during full navigation walkthrough
14. No unhandled promise rejections or Observable errors

---

#### H. The "Walk-Through Test" (MANDATORY Final Check)

**Before declaring ANY phase or feature complete, Kiro MUST mentally (or via E2E test) perform this walk-through:**

1. Open the app in a browser
2. Register a new account → verify email → log in
3. Click EVERY menu item (top nav, sidebar, footer) — confirm each loads a real page
4. Click EVERY sub-menu item — confirm each loads a real page
5. Visit the user profile → confirm all fields display
6. Visit settings → confirm all tabs render with content
7. Check the notification bell → confirm it renders (even if empty with "No notifications" message)
8. On EVERY list page → confirm data is visible (seeded), search works, sort works, pagination works
9. On EVERY form → fill and submit → confirm success feedback → confirm data persisted
10. On EVERY dashboard → confirm cards show numbers, charts render, activity feed shows items
11. Test as Admin role → confirm admin-only pages are accessible
12. Test as Subscriber role → confirm admin pages are blocked (403)
13. Test on mobile viewport (375px) → confirm navigation works, pages are usable
14. Toggle dark mode → confirm all pages render correctly
15. Check browser console → confirm ZERO errors during entire walk-through

**If ANY step fails → the feature/phase is NOT done. Fix it first.**

**Inclusion:** always

---

### 11. `paypal-integration.md` — PayPal-Specific Implementation Guide

**Purpose:** Detailed reference for implementing the full PayPal integration correctly.

**Must include:**
- PayPal REST API v2 endpoints and authentication (OAuth2 client credentials)
- Subscription Plans API: create, update, activate, deactivate, list
- Subscriptions API: create, get, suspend, cancel, activate, revise
- Orders API: create, capture, authorize
- Payments API: refunds (full/partial)
- Webhook event types and handling for each
- Webhook signature verification implementation
- Sandbox vs Live environment switching
- Error handling and retry strategy
- Idempotency keys for payment operations
- Data mapping: PayPal entities ↔ our database entities
- Testing strategy: sandbox accounts, simulated webhooks
- Currency handling (minor units vs major units)
- PayPal button rendering (JavaScript SDK integration in Angular)

**Inclusion:** fileMatch
**fileMatchPattern:** `**/paypal/**,**/payment/**,**/subscription/**,**/billing/**`

---

### 12. `cms-implementation.md` — CMS Architecture Guide

**Purpose:** Detailed reference for building the block-based CMS correctly.

**Must include:**
- Block data model: JSON schema for each block type
- Block rendering pipeline: DB → API → Angular dynamic component loader
- Page composition: ordered list of blocks with per-block configuration
- Media upload flow: client → Azure Blob Storage → CDN URL returned
- Image optimisation pipeline: resize, compress, WebP conversion
- Version history: snapshot strategy (full page JSON vs diff-based)
- Draft/published/scheduled state machine
- Admin editor UX: Angular CDK drag-drop for block reordering
- Rich text editor integration (TipTap or Quill) within text blocks
- SEO metadata storage and rendering (SSR or prerendering considerations)
- Social widget configuration schema and rendering
- Navigation menu data model and rendering
- Performance: lazy load block components, virtualise long pages in editor

**Inclusion:** fileMatch
**fileMatchPattern:** `**/cms/**,**/content/**,**/media/**,**/pages/**`

---

### 13. `angular-patterns.md` — Angular-Specific Patterns & Conventions

**Purpose:** Angular-specific implementation patterns beyond general coding standards.

**Must include:**
- Standalone components (Angular 20 default — no NgModules for components)
- Signal-based reactivity where appropriate (Angular Signals)
- NgRx SignalStore for simpler local state (vs global NgRx Store)
- Lazy loading routes with `loadComponent` / `loadChildren`
- Resolver and Guard patterns (functional guards in Angular 20)
- HTTP interceptors: auth token, error handling, loading state
- Form patterns: dynamic form generation for CMS and plan builder
- Directive usage: structural directives for permission checks (`*appHasRole`)
- Pipe conventions: pure pipes for display transforms
- Component communication: Input/Output for parent-child, NgRx for cross-feature
- CDK usage: drag-drop (CMS), overlay (modals), virtual scroll (lists)
- Animation patterns: route transitions, list stagger, enter/leave
- Error boundary pattern: catch component errors gracefully
- PWA service worker: caching strategy, offline fallback

**Inclusion:** fileMatch
**fileMatchPattern:** `**/client/**,**/*.component.ts,**/*.service.ts,**/*.module.ts`

---

### 14. `dotnet-patterns.md` — .NET-Specific Patterns & Conventions

**Purpose:** .NET-specific implementation patterns beyond general coding standards.

**Must include:**
- Minimal API vs Controller decision guide (Controllers for complex, Minimal for simple CRUD)
- MediatR handler pattern: one handler per command/query
- FluentValidation: one validator per request DTO
- EF Core patterns: include strategy, query splitting, compiled queries for hot paths
- Background jobs: IHostedService for recurring tasks (e.g., scheduled publishing, dunning)
- Middleware pipeline order: auth, CORS, rate limiting, error handling, routing
- Exception handling middleware: global handler, ProblemDetails response format
- Caching: IMemoryCache for hot data, IDistributedCache (Redis) for shared state
- File upload handling: stream directly to Blob Storage (don't buffer in memory)
- Health checks: database, PayPal API, Azure Blob Storage
- Logging: structured logging with Serilog, correlation IDs per request
- Configuration: layered (appsettings.json → appsettings.{env}.json → Key Vault)
- Outbox pattern for reliable webhook/event processing

**Inclusion:** fileMatch
**fileMatchPattern:** `**/api/**,**/*.cs`

---

## How Kiro Should Use These Steering Files

1. **Before writing any code**, generate all 14 steering files above in `.kiro/steering/`
2. **During implementation**, reference the relevant steering files for every decision
3. **Before marking any task as complete**, run through `definition-of-done.md` checklist
4. **Before presenting code to the user**, run through `code-review.md` self-review
5. **If unsure about a pattern**, check the specific guide (`angular-patterns.md`, `dotnet-patterns.md`, etc.)
6. **Run all tests** and verify build succeeds before declaring anything done
7. **Never skip the E2E verification** — the feature must work end-to-end from the user's perspective

---

## File Generation Order

Generate steering files in this order (dependencies flow downward):

```
1. architecture.md          (foundation — everything depends on this)
2. database-design.md       (data layer conventions)
3. api-design.md            (interface between backend and frontend)
4. coding-standards.md      (code quality rules)
5. dotnet-patterns.md       (.NET specifics)
6. angular-patterns.md      (Angular specifics)
7. ui-design.md             (visual standards)
8. security.md              (security guardrails)
9. testing-strategy.md      (test requirements)
10. paypal-integration.md   (payment specifics)
11. cms-implementation.md   (CMS specifics)
12. requirements.md         (what to build)
13. code-review.md          (self-review before delivery)
14. definition-of-done.md   (final gate — nothing ships without this)
```

---

## Critical Instruction to Kiro

**DO NOT write application code until all 14 steering files are generated and saved in `.kiro/steering/`.**

**DO NOT tell the user a feature is "done" until EVERY item in `definition-of-done.md` is verified.**

**DO NOT skip tests.** If tests fail, fix the code. If you cannot fix it, explain why and ask for guidance.

**DO NOT deviate from the architecture.** If you need to deviate, explain why in a comment and document the decision.

**Reference `v2-proposal.md` as the feature specification.** The steering files tell you HOW to build. The proposal tells you WHAT to build.

---

## Trusted Commands Configuration

**Kiro must add the following trusted commands to the workspace settings** so that builds, tests, and file operations can run without manual approval. Create or update `.kiro/settings/settings.json` with:

```json
{
  "trustedCommands": [
    "dotnet *",
    "npm *",
    "npx *",
    "ng *",
    "node *",
    "git *",
    "Get-ChildItem *",
    "Get-Content *",
    "Select-String *",
    "Test-Path *",
    "mkdir *",
    "New-Item *",
    "Remove-Item *",
    "Copy-Item *",
    "Move-Item *",
    "Rename-Item *",
    "Set-Content *",
    "Add-Content *",
    "Out-File *",
    "Write-Output *",
    "Where-Object *",
    "ForEach-Object *",
    "Sort-Object *",
    "Select-Object *",
    "Measure-Object *",
    "Group-Object *",
    "Format-Table *",
    "Format-List *",
    "ConvertTo-Json *",
    "ConvertFrom-Json *"
  ]
}
```

**Do this immediately after creating the workspace folder structure**, before generating steering files or writing any code. This ensures all build, test, and file operations execute without interruption during development.
