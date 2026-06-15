# Reported Bugs & Issues — AndyTipster V2

## Summary

| # | Category | Severity | Title |
|---|----------|----------|-------|
| 1 | Navigation | Critical | Navbar role casing mismatch — Admin link never shows |
| 2 | Navigation | Critical | Navbar links to non-existent routes (dead ends) |
| 3 | Navigation | High | No subscriber nav items in navbar |
| 4 | Security | High | Moderators can access all admin pages (no per-page permission guard) |
| 5 | Security | High | Free Users can access subscriber routes (no subscription check) |
| 6 | Seed Data | High | No comprehensive seed data for demo/development |
| 7 | DTO Mismatch | Critical | LoginResponse field names don't align between API and frontend |
| 8 | DTO Mismatch | High | Enable2FA response property names don't match |
| 9 | DTO Mismatch | High | RoleResponse.Permissions returns objects but frontend expects string[] |
| 10 | Components | Medium | Admin listing pages don't use the reusable DataTable component |
| 11 | Components | Medium | Currency formatting is inconsistent across the app |
| 12 | Components | Medium | Date formatting is inconsistent (mixed pipe formats) |
| 13 | Components | Low | Form pattern inconsistency (template-driven vs reactive) |
| 14 | NgRx | Medium | Admin pages bypass NgRx store — call services directly |
| 15 | NgRx | Medium | Load effects use exhaustMap (should be switchMap) |
| 16 | NgRx | Medium | Missing updateTip effect — dispatching action does nothing |
| 17 | NgRx | Low | Dual roles/permissions storage in AuthState and dedicated slices |
| 18 | NgRx | Low | Type casts (as unknown as Tip) hide DTO mapping bugs |
| 19 | NgRx | Low | Factory selectors not cached across calls |

---

## Bug 1: Navbar role casing mismatch

**Severity:** Critical  
**Category:** Navigation  
**Location:** `src/andytipster-client/src/app/shared/components/navbar/navbar.component.ts`

**Problem:**  
The navbar checks for roles `'admin'` and `'super-admin'` (lowercase, hyphenated) but the actual role names from the backend RoleSeeder are `'Admin'` and `'Super Admin'` (title case, spaces). The Admin nav item and `isAdmin` flag never evaluate to true.

**Impact:**  
- Admin users cannot see the "Admin" link in the navbar
- Admin dropdown items ("Admin Dashboard") never appear
- Admins must manually type `/admin` in the URL bar

**Fix Plan:**  
1. Change navbar `navItems` roles to `['Admin', 'Super Admin']`
2. Change `isAdmin` check to `roles.includes('Admin') || roles.includes('Super Admin')`
3. Add case-insensitive comparison utility to avoid this class of bug

---

## Bug 2: Navbar links to non-existent routes

**Severity:** Critical  
**Category:** Navigation  
**Location:** `src/andytipster-client/src/app/shared/components/navbar/navbar.component.ts`

**Problem:**  
The navbar contains these broken links:
- `/tips` — No route exists. Tips are at `/subscriber/tips`
- `/plans` — No route exists. Pricing is at `/pricing` (public routes)
- User dropdown links to `/profile` — Should be `/subscriber/profile`
- User dropdown links to `/settings` — No such route exists

**Impact:**  
All 4 links redirect to home page via the `**` wildcard route. Users hit dead ends.

**Fix Plan:**  
1. Change `/tips` to `/subscriber/tips`
2. Change `/plans` to `/pricing`
3. Change dropdown `/profile` to `/subscriber/profile`
4. Remove or redirect `/settings` to `/subscriber/profile` (settings are tabs within profile)

---

## Bug 3: No subscriber nav items in navbar

**Severity:** High  
**Category:** Navigation  
**Location:** `src/andytipster-client/src/app/shared/components/navbar/navbar.component.ts`

**Problem:**  
The navbar only defines 4 items: Home, Tips, Plans, Admin. When a subscriber logs in, they have no way to navigate to their dashboard, billing, results, or referrals from the navbar.

**Impact:**  
Subscribers must rely on the mobile bottom nav or manually type URLs.

**Fix Plan:**  
1. Add nav items for authenticated users: Dashboard, Results, Billing, Referrals
2. Use `authRequired: true` to show them only when logged in
3. Consider a "My Account" dropdown with sub-links

---

## Bug 4: Moderators can access all admin pages

**Severity:** High  
**Category:** Security  
**Location:** `src/andytipster-client/src/app/features/admin/admin.routes.ts`

**Problem:**  
The admin routes have a single `roleGuard` at the parent level checking `['Super Admin', 'Admin', 'Moderator']`. All 13 child routes are accessible to Moderators, but Moderators should only have access to Users.View, Tips.View/Edit, CMS.View/Edit, and Analytics.View.

A Moderator can navigate to:
- `/admin/plans` (Plans.Create/Edit/Delete — not permitted)
- `/admin/paypal-dashboard` (Subscriptions.Manage — not permitted)
- `/admin/audit` (Audit.View — not permitted for Moderator)
- `/admin/notifications` (broadcast to all — not permitted)

**Impact:**  
Permission escalation. Moderators can view and potentially modify resources beyond their permission set.

**Fix Plan:**  
1. Add `permissionGuard` to individual admin child routes with required permissions
2. Route data should specify: `{ permissions: ['Plans.Create'] }` etc.
3. Create a `permissionGuard` that checks the JWT permission claims
4. Also hide sidebar/nav links for pages the user can't access

---

## Bug 5: Free Users can access subscriber routes

**Severity:** High  
**Category:** Security  
**Location:** `src/andytipster-client/src/app/features/subscriber/subscriber.routes.ts`

**Problem:**  
Subscriber routes use `authGuard` which only checks `isAuthenticated`. A Free User (registered but no active subscription) can navigate to `/subscriber/tips`, `/subscriber/results`, `/subscriber/billing`, etc.

**Impact:**  
While the backend should still enforce access gating on API responses, the frontend allows navigation to subscriber-only pages without a subscription, creating a confusing UX (empty pages or error states).

**Fix Plan:**  
1. Create a `subscriptionGuard` that checks if the user has an active subscription
2. Apply it to tips feed and results routes (not billing/checkout/profile which all users need)
3. Redirect Free Users to a "Subscribe" prompt page
4. Keep `/subscriber/checkout` and `/subscriber/profile` accessible to all authenticated users

---

## Bug 6: No comprehensive seed data

**Severity:** High  
**Category:** Seed Data  
**Location:** `src/AndyTipster.Infrastructure/Data/Seeding/`

**Problem:**  
Only `RoleSeeder.cs` exists. The application has no demo data for:
- Subscription plans
- Tip categories
- Sample tips with results
- Sample users (admin, subscriber, free user)
- Blog posts
- CMS pages (landing, FAQ, about)
- Navigation menus
- Site settings

**Impact:**  
Fresh install shows empty dashboards, empty tip feeds, empty plan pages. Cannot demo the application without manual data entry.

**Fix Plan:**  
1. Create `DemoSeeder.cs` that seeds:
   - 3 subscription plans (Monthly £19.99, Quarterly £49.99, Annual £149.99)
   - 3 tip categories (UK Horse Racing, Irish Horse Racing, Other Sports)
   - 1 Super Admin user (`admin@andytipster.com` / `Admin123!`)
   - 1 Subscriber user (`subscriber@test.com` / `Test123!`)
   - 1 Free User (`free@test.com` / `Test123!`)
   - 10 sample tips across categories with mixed results
   - 2 blog posts
   - Default navigation menus (header with 5 items, footer with 3)
   - Site settings (name, tagline)
2. Call from `Program.cs` in development environment
3. Make it idempotent (check if data exists before seeding)

---

## Bug 7: LoginResponse DTO field name mismatch

**Severity:** Critical  
**Category:** DTO Mismatch  
**Location:**  
- Backend: `src/AndyTipster.Application/Auth/DTOs/LoginResponse.cs`
- Frontend: `src/andytipster-client/src/app/core/services/auth.service.ts`

**Problem:**  
| Backend Property | JSON Output (camelCase) | Frontend Property |
|-----------------|-------------------------|-------------------|
| `RequiresTwoFactor` | `requiresTwoFactor` | `requires2FA` |
| `ExpiresAt` (DateTime) | `expiresAt` (ISO string) | `expiresAt` (number) |

The frontend expects `requires2FA` but the API sends `requiresTwoFactor`. The 2FA login flow is completely broken — the frontend never detects that 2FA is required.

The `expiresAt` type mismatch means token expiry comparison logic fails.

**Impact:**  
- 2FA login flow is broken — users with 2FA enabled can never log in
- Token expiry tracking doesn't work correctly

**Fix Plan:**  
1. Rename frontend `requires2FA` to `requiresTwoFactor` to match API
2. Change frontend `expiresAt` type from `number` to `string` and parse it as Date
3. OR: Change backend to return epoch milliseconds instead of DateTime
4. Update AuthState, Effects, and AuthService interface

---

## Bug 8: Enable2FA response property names don't match

**Severity:** High  
**Category:** DTO Mismatch  
**Location:**  
- Backend: `src/AndyTipster.Application/Auth/DTOs/Enable2FAResponse.cs`
- Frontend: `src/andytipster-client/src/app/core/services/auth.service.ts`

**Problem:**  
| Backend Property | JSON Output | Frontend Property |
|-----------------|-------------|-------------------|
| `QrCodeUri` | `qrCodeUri` | `qrCodeUrl` |
| `ManualEntryKey` | `manualEntryKey` | `secret` |

**Impact:**  
2FA setup page displays undefined values for the QR code and manual key.

**Fix Plan:**  
1. Update frontend `Enable2FAResponse` interface: `qrCodeUrl` → `qrCodeUri`, `secret` → `manualEntryKey`
2. Update the 2FA setup component to use the corrected property names

---

## Bug 9: RoleResponse returns objects but frontend expects string[]

**Severity:** High  
**Category:** DTO Mismatch  
**Location:**  
- Backend: `src/AndyTipster.Application/Roles/DTOs/RoleResponse.cs`
- Frontend: `src/andytipster-client/src/app/store/roles/roles.state.ts`

**Problem:**  
Backend `RoleResponse.Permissions` is `List<PermissionResponse>` (objects with Id, Name, Description, Module). Frontend `Role.permissions` is `string[]` (just names). The effects directly cast the API response to `Role[]` without mapping.

**Impact:**  
Permission-based selectors operate on object references instead of string values, causing permission checks to fail silently.

**Fix Plan:**  
1. Add a mapping function in `RolesEffects` that transforms `PermissionResponse[]` to `string[]` (extracting `.name`)
2. OR: Update frontend `Role` interface to use `PermissionResponse[]` and update all selectors

---

## Bug 10: Admin listing pages don't use DataTable component

**Severity:** Medium  
**Category:** Components  
**Location:**  
- `src/andytipster-client/src/app/features/admin/pages/user-management/`
- `src/andytipster-client/src/app/features/admin/pages/tip-management/`
- `src/andytipster-client/src/app/features/admin/pages/plan-management/`

**Problem:**  
A full-featured `DataTableComponent` exists (`shared/components/data-table/`) with pagination, sorting, filtering, bulk actions, export, skeleton loading, and empty states. However, none of the admin pages use it — each builds its own table with ~200 lines of duplicated logic.

**Impact:**  
- ~600 lines of duplicated table code across 3 pages
- Inconsistent UX (different pagination styles, different sort indicators)
- Changes to table behavior must be made in 4 places instead of 1
- Export logic duplicated in user management

**Fix Plan:**  
1. Refactor `UserManagementComponent` to use `<app-data-table>` with column definitions
2. Refactor `TipManagementComponent` to use `<app-data-table>`
3. Refactor `PlanManagementComponent` promo code table to use `<app-data-table>`
4. Keep plan cards as they are (cards are appropriate for that layout)

---

## Bug 11: Currency formatting is inconsistent

**Severity:** Medium  
**Category:** Components  
**Location:** Multiple components

**Problem:**  
- Plan management: `{{ plan.currency }} {{ plan.price }}` — raw text, no formatting
- Tips P&L: `{{ tip.profitLoss | number:'1.2-2' }}` — formatted number, no currency symbol
- Promo codes: `'£' + code.discountValue` — hardcoded GBP symbol
- Checkout: various inconsistencies

**Impact:**  
Professional appearance undermined. Will break entirely if EUR/USD plans are created.

**Fix Plan:**  
1. Create a `CurrencyDisplayPipe` that accepts `(amount, currencyCode)` and formats correctly using `Intl.NumberFormat`
2. Replace all inline currency formatting with the pipe
3. Handle GBP (£), EUR (€), USD ($) correctly

---

## Bug 12: Date formatting is inconsistent

**Severity:** Medium  
**Category:** Components  
**Location:** Multiple components

**Problem:**  
Different Angular date pipe formats used across the app:
- `date:'shortDate'` in some places
- `date:'mediumDate'` in others
- `date:'short'` (includes time) elsewhere
- `date:'medium'` in others

**Impact:**  
Inconsistent date display. Users see different date formats on different pages.

**Fix Plan:**  
1. Define date format constants in a shared file: `DATE_FORMAT = 'dd MMM yyyy'`, `DATETIME_FORMAT = 'dd MMM yyyy, HH:mm'`
2. Create a shared pipe or use a consistent format string across all components
3. Document the standard: dates use `dd MMM yyyy`, datetimes include time

---

## Bug 13: Form pattern inconsistency

**Severity:** Low  
**Category:** Components  
**Location:** Auth pages vs Admin pages

**Problem:**  
Auth pages (login, register, forgot-password) use Angular Reactive Forms (`FormBuilder`, `FormGroup`, validators). Admin pages (user management, plan management) use template-driven forms with `[(ngModel)]`.

**Impact:**  
Harder to maintain, inconsistent validation patterns, harder for new developers to follow.

**Fix Plan:**  
1. Standardise on Reactive Forms for all form-heavy pages
2. Convert admin page forms to use `FormBuilder` with proper validators
3. Keep `[(ngModel)]` only for simple one-off bindings (search inputs, filters)

---

## Bug 14: Admin pages bypass NgRx store

**Severity:** Medium  
**Category:** NgRx  
**Location:** All admin feature components

**Problem:**  
Admin pages call services directly:
```typescript
this.usersService.getUsers(request).subscribe({...});
this.tipsService.getTips(filter).subscribe({...});
```
Instead of dispatching actions through the store.

**Impact:**  
- No centralized loading/error state tracking
- No state caching between navigations (re-fetches on every page visit)
- Inconsistent patterns with auth/tips/plans slices that do use NgRx

**Fix Plan:**  
1. For Phase 1: Accept this pattern for admin pages (it works, just not ideal)
2. For Phase 2: Create admin-specific store slices if performance/UX warrants it
3. Document the decision: "Store is for shared app state; admin pages may use direct service calls for isolated CRUD views"

---

## Bug 15: Load effects use exhaustMap

**Severity:** Medium  
**Category:** NgRx  
**Location:** `src/andytipster-client/src/app/store/tips/tips.effects.ts`

**Problem:**  
`loadTips$` uses `exhaustMap` which ignores new dispatches while one is in flight. If a user changes page/filter quickly, the second request is dropped.

**Impact:**  
Stale data shown after rapid navigation. User must wait for first request to complete.

**Fix Plan:**  
1. Change `loadTips$` to use `switchMap` (cancels in-flight and uses latest)
2. Change `loadTip$` to use `switchMap`
3. Keep `createTip$`, `publishTip$`, `recordResult$` on `exhaustMap` (mutations should not be cancelled)

---

## Bug 16: Missing updateTip effect

**Severity:** Medium  
**Category:** NgRx  
**Location:** `src/andytipster-client/src/app/store/tips/tips.effects.ts`

**Problem:**  
`TipsActions.updateTip` is defined in actions but no corresponding effect exists to handle it.

**Impact:**  
Dispatching `updateTip` does nothing. If any component uses this action, the tip won't be updated.

**Fix Plan:**  
1. Add an `updateTip$` effect that calls `tipsService.updateTip(id, dto)` and dispatches `updateTipSuccess`/`updateTipFailure`
2. Use `exhaustMap` (mutation — don't cancel)

---

## Bug 17: Dual roles/permissions storage

**Severity:** Low  
**Category:** NgRx  
**Location:**  
- `src/andytipster-client/src/app/store/auth/auth.state.ts` (roles[], permissions[])
- `src/andytipster-client/src/app/store/roles/` (dedicated slice)
- `src/andytipster-client/src/app/store/permissions/` (dedicated slice)

**Problem:**  
Roles and permissions are stored in both the auth state (populated on login) AND in dedicated slices (populated from separate API calls). Components may read from different sources, leading to inconsistency.

**Impact:**  
Minor — if both are populated from the same JWT token it works. But if one slice updates without the other, guards/selectors may disagree.

**Fix Plan:**  
1. Designate auth state as the single source of truth for current user's roles/permissions
2. Use dedicated roles/permissions slices only for admin management (listing all roles/permissions in the system)
3. Update guards and navbar to consistently read from `selectAuthRoles` / `selectAuthPermissions`

---

## Bug 18: Type casts hide DTO mapping bugs

**Severity:** Low  
**Category:** NgRx  
**Location:** `src/andytipster-client/src/app/store/tips/tips.effects.ts`

**Problem:**  
Effects use `as unknown as Tip` to cast API responses to store types. This hides actual property mismatches between DTOs and state interfaces.

**Impact:**  
If `TipDto` and `Tip` interfaces diverge, runtime errors occur with no compile-time warning.

**Fix Plan:**  
1. Create explicit mapper functions: `mapTipDtoToTip(dto: TipDto): Tip`
2. Use mappers in effects instead of casts
3. This will surface any actual mapping issues at compile time

---

## Bug 19: Factory selectors not cached

**Severity:** Low  
**Category:** NgRx  
**Location:** `src/andytipster-client/src/app/store/auth/auth.selectors.ts`

**Problem:**  
```typescript
export const selectHasRole = (role: string) =>
  createSelector(selectAuthRoles, (roles) => roles.includes(role));
```
Each call creates a new selector instance. If used in templates with `*ngIf="selectHasRole('Admin') | ngrxPush"`, a new selector is created on every change detection cycle.

**Impact:**  
Minor performance issue. Only noticeable with many role/permission checks in a single template.

**Fix Plan:**  
1. Pre-define commonly used selectors: `selectIsAdmin`, `selectIsSuperAdmin`, `selectIsModerator`
2. For dynamic checks, cache factory selectors in the component as class properties
3. Consider using a `hasPermission` pipe that internally caches the selector
