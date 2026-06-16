# User Workflows — AndyTipster V2

This document describes the complete workflow for each user role — what they see, where they navigate, and what actions they can perform.

---

## 1. Guest (Unauthenticated Visitor)

**How they arrive:** Direct URL, search engine, social media link, referral link

**Layout:** Public top navigation bar (no sidebar)

### Pages & Workflows

| Page | URL | What They See | Actions Available |
|------|-----|---------------|-------------------|
| Landing Page | `/` | Hero section with brand messaging, animated stats (87% strike rate, £2,340 avg monthly profit, 1,200+ subscribers), pricing cards (3 plans with features), Tip of the Day teaser (locked), testimonials carousel, FAQ section | Browse plans, search FAQ, click "Sign Up" |
| Pricing | `/pricing` | 3 plan cards with full feature lists, billing cycles, trial info. "Most Popular" badge on Quarterly plan | Select a plan → redirected to register |
| Public Stats | `/stats` | Verified P&L charts (profit over time, win rate trends, category comparison), last 30 days summary, CSV/PDF export | View performance proof, export results |
| Blog | `/blog` | Published blog posts sorted by date, featured images, excerpts | Read articles, click through to full posts |
| Blog Post | `/blog/:slug` | Full blog post with rich content, SEO metadata | Read, share on social |
| FAQ | `/faq` | Searchable FAQ accordion (8 questions) | Search, expand/collapse answers |
| Login | `/auth/login` | Email + password form, social login buttons (Google, Facebook), forgot password link | Login, navigate to register |
| Register | `/auth/register` | Registration form (name, email, password, confirm password), password strength indicator | Create account → email verification sent |
| Forgot Password | `/auth/forgot-password` | Email input form | Request password reset link |

### Conversion Flow
```
Guest → Landing Page → View Pricing → Click "Sign Up" 
→ Register → Verify Email → Login → Free User
```

---

## 2. Free User (Registered, No Subscription)

**Login:** `free@test.com` / `Test123!`

**Layout:** Sidebar + Topbar (authenticated layout)

**Sidebar shows:** Dashboard, Pricing, Profile

### Pages & Workflows

| Page | URL | What They See | Actions Available |
|------|-----|---------------|-------------------|
| Dashboard (Pricing) | `/pricing` | Redirected here since they can't access tips. Same pricing page as guest but with "Subscribe Now" CTA | Choose a plan → checkout |
| Profile | `/subscriber/profile` | 6 tabs: Profile (avatar, name, bio, timezone), Security (2FA, password), Notifications (channel toggles), Billing (empty — "No Active Subscription"), Privacy (data export, account deletion), Appearance (theme toggle) | Edit profile, enable 2FA, manage privacy |
| Checkout | `/subscriber/checkout` | Order summary, payment method selection (PayPal/Stripe), promo code entry, trial display | Apply promo code, select payment, subscribe |
| Billing | `/subscriber/billing` | "No Active Subscription" empty state with "View Plans" CTA button | Navigate to pricing |

### Conversion Flow
```
Free User → Sees "Pricing" in sidebar → Selects plan 
→ Checkout → Enter payment → Apply promo "WELCOME20" (20% off)
→ Subscription activated → Becomes Subscriber
```

### What They CANNOT Do
- View tips feed (redirected to pricing)
- View results/performance
- Access admin pages
- See "My Tips" or "Results" in sidebar

---

## 3. Subscriber (Paying Customer)

**Login:** `subscriber@test.com` / `Test123!`

**Layout:** Sidebar + Topbar (authenticated layout)

**Sidebar shows:** Dashboard, Tips (Today's Tips, Results), Performance, Billing, Profile, Referrals

### Pages & Workflows

| Page | URL | What They See | Actions Available |
|------|-----|---------------|-------------------|
| Tips Feed (Dashboard) | `/subscriber/tips` | All published tips for their plan categories (UK Racing for Monthly Premium). Each tip shows: race name, selection, odds, stake, category, status, result, P&L, commentary | View tips, filter by category |
| Results | `/subscriber/results` | Personal P&L dashboard: total profit/loss, strike rate, winning/losing streak, monthly summaries. Filterable by category and date range | Filter by category, date. View performance trends |
| Performance | `/subscriber/results` | Same as Results — detailed P&L breakdown | Export data |
| Billing | `/subscriber/billing` | Current plan ("Monthly Premium"), status (Active), next billing date, payment method, 2 payment history entries (£19.99 each). Cancel/upgrade buttons | Upgrade plan, cancel subscription, view payment history |
| Profile | `/subscriber/profile` | 6 tabs: Profile, Security, Notifications, Billing, Privacy, Appearance | Edit all personal settings |
| Checkout | `/subscriber/checkout` | Used for plan upgrades — shows new plan details, prorated amount | Upgrade subscription |
| Referrals | `/subscriber/referrals` | Unique referral link (JOHN-REF-2025), stats (clicks, conversions, rewards earned), referral history | Copy link, share, view rewards |

### Daily Workflow
```
Subscriber logs in → Dashboard (Tips Feed)
→ Views today's published tips with odds and commentary
→ Checks Results tab for running P&L
→ Gets notification when new tip published
→ Checks Billing for next payment date
→ Shares referral link with friends for discount
```

### Notifications They Receive
- New tip published (via email, push, Telegram, in-app)
- Tip result updated (Won/Lost/Void)
- Renewal reminder (7 days before)
- Payment failure alert

### What They CANNOT Do
- Create/edit/delete tips
- Access admin panel
- Manage other users
- Modify plans or promo codes

---

## 4. Moderator (Community Manager)

**Layout:** Sidebar + Topbar (authenticated layout)

**Sidebar shows:** Dashboard, Tips (Today's Tips, Results), Members, Performance, Content (CMS, Blog, Media), Settings

### Pages & Workflows

| Page | URL | What They See | Actions Available |
|------|-----|---------------|-------------------|
| Admin Dashboard | `/admin/dashboard` | Summary cards (subscribers, MRR, tips today), revenue chart, recent activity feed, quick actions | View metrics, navigate to modules |
| Tip Management | `/admin/tips` | All tips in DataTable (10 columns, sortable, filterable). Can VIEW and EDIT tips but NOT create or delete | Edit tip commentary, view results |
| Results | `/subscriber/results` | Full performance data across all categories | View P&L, filter results |
| Members | `/admin/users` | User list (3 seeded users) — can VIEW only | View user details (no suspend, no impersonate) |
| Analytics | `/admin/analytics` | Revenue overview, subscriber growth | View charts |
| CMS | `/admin/cms` | Page list, page editor | Edit existing pages (no publish/delete) |
| Blog | `/admin/blog` | Blog post list (2 posts) | Edit posts, manage content |
| Media | `/admin/media-library` | Media library with upload | Upload and manage assets |
| Navigation | `/admin/navigation` | Menu tree editor | Edit navigation menus |

### Moderation Workflow
```
Moderator logs in → Dashboard (overview)
→ Check Tips for any that need editing
→ Moderate comments (approve/hide/delete)
→ Edit CMS content if updates needed
→ Upload media for blog/CMS
```

### What They CANNOT Do
- Create/delete tips
- Create/manage subscription plans
- Create promo codes
- Suspend users or change roles
- View audit logs
- Send broadcast notifications
- Access PayPal billing dashboard

---

## 5. Admin (Staff Member)

**Layout:** Sidebar + Topbar (authenticated layout)

**Sidebar shows:** ALL items (Dashboard, Tips, Subscriptions, Members, Performance, Reports, Content, Settings)

### Pages & Workflows

| Page | URL | What They See | Actions Available |
|------|-----|---------------|-------------------|
| Dashboard | `/admin/dashboard` | KPIs: 1 subscriber, £19.99 MRR, tips published, signups. Revenue trend chart (2 data points). Activity feed (5 entries). Quick action buttons | Navigate to any module |
| Today's Tips | `/admin/tips` | DataTable: 11 tips with full CRUD. Filter by status/category/result. P&L summary stats (total P&L, strike rate, won/lost counts) | Create tips, publish, archive, record results (Won/Lost/Void/Push), CSV import (500 rows), delete |
| Results | `/subscriber/results` | Full P&L across all categories with admin bypass | View comprehensive performance data |
| Packages | `/admin/plans` | 4 plan cards with features, sync status badges. Create/archive plans. Promo codes table (WELCOME20, ANNUAL50) | Create plans, edit pricing, create promo codes, sync to PayPal, archive |
| Billing (PayPal) | `/admin/paypal-dashboard` | Transaction history (2 entries), revenue analytics (MRR £19.99, ARR), subscription lifecycle, Sandbox/Live indicator | View transactions, process refunds, export CSV/PDF |
| Members | `/admin/users` | DataTable: 3 users with roles, status, plan. Search/filter. Bulk actions bar | Search, filter, bulk suspend, bulk role change, CSV export |
| Performance | `/admin/analytics` | Revenue analytics: MRR, churn rate (0%), LTV, revenue by plan, growth forecasting | View all financial metrics |
| Reports (Audit) | `/admin/audit` | Audit log DataTable: 5+ entries with actor, action, target, timestamp, before/after JSON | Search, filter by action/actor/date, view details |
| CMS | `/admin/cms` | Page list (Home page). Full page builder with 18 block types, drag-drop, preview, undo/redo, auto-save, publish | Create pages, add blocks, edit content, publish/unpublish, rollback versions |
| Blog | `/admin/blog` | 2 posts (Published + Draft). Full CRUD | Create posts, edit, publish, schedule, delete |
| Media | `/admin/media-library` | Upload zone, asset grid, search by filename/tags | Upload (20 files batch), search, crop/resize, delete |
| Settings (Navigation) | `/admin/navigation` | Tree editor: Header menu with 5 items | Add/remove/reorder menu items, set visibility rules |

### Daily Admin Workflow
```
Admin logs in → Dashboard (check KPIs)
→ "Tips" → Create today's tips (race, selection, odds, stake, category, commentary)
→ Publish tips → Subscribers get notified
→ After races: Record results (Won/Lost/Void/Push) → P&L auto-calculated
→ Check Members for new signups
→ Check Billing for payment issues
→ Edit blog post for tomorrow's preview
→ Logout
```

### Tip Creation Workflow (Detail)
```
1. Navigate to "Today's Tips"
2. Click "Create Tip"
3. Fill form: Event Date, Race Name, Selection, Odds (1.01-1000), Stake (1-10), Category, Commentary
4. Click "Create" → Tip saved as Draft
5. Click "Publish" on the tip → Status changes to Published
6. Subscribers see the tip immediately
7. After the race: Click "Record Result" → Choose Won/Lost/Void/Push
8. P&L automatically calculated: Won = (odds × stake) - stake
```

### CSV Import Workflow
```
1. Click "CSV Import" button
2. Select .csv file (max 500 rows, 5MB)
3. Format: EventDate, RaceName, Selection, Odds, Stake, Category, Commentary
4. System validates each row
5. Shows result: "Imported 48/50. 2 errors: Row 12 (invalid odds), Row 37 (missing category)"
6. Valid rows are saved as Draft tips
```

### What They CANNOT Do
- Impersonate users (Super Admin only)
- Delete roles (Super Admin only)
- Create custom roles (Super Admin only)

---

## 6. Super Admin (Platform Owner)

**Login:** `admin@andytipster.com` / `Admin123!`

**Layout:** Sidebar + Topbar (authenticated layout)

**Sidebar shows:** ALL items (same as Admin)

### Additional Capabilities Beyond Admin

| Feature | What Super Admin Can Do |
|---------|------------------------|
| User Impersonation | Click "Impersonate" on any user → see the site exactly as they see it (read-only), with persistent banner. Click "End Impersonation" to return |
| Role Management | Create custom roles (name + hierarchy level + permissions), edit, delete. Assign any role below their level |
| Audit Logs | Full access to append-only audit trail (2-year retention) |
| GDPR Breach Notification | Trigger breach notification email to all affected users |
| Broadcast Notifications | Send announcements to ALL active subscribers via all channels |
| System Configuration | Full access to global site settings, maintenance mode, analytics scripts, redirects |

### Super Admin Unique Workflows

#### Impersonation
```
1. Navigate to "Members"
2. Find user "John Subscriber"
3. Click "Impersonate" (👤 icon)
4. See the site as John sees it (tips feed, billing, profile)
5. Yellow banner shows: "Impersonating: John Subscriber (subscriber@test.com)"
6. Click "End Impersonation" → return to admin view
```

#### Custom Role Creation
```
1. Navigate to Roles API (POST /api/roles)
2. Create role: "VIP Analyst" at hierarchy level 3
3. Assign permissions: Tips.View, Tips.Create, Analytics.View
4. Assign the role to a user
5. That user now sees tips + analytics but not plans/users/CMS
```

#### GDPR Data Export (on behalf of user)
```
1. User requests data export via profile Privacy tab
2. System generates JSON/CSV archive (profile, subscriptions, payments, tip access, comments, preferences)
3. User receives email with 7-day download link
4. Link expires automatically
```

#### Account Deletion Flow
```
1. User requests deletion via Privacy tab
2. 30-day soft-delete grace period begins
3. User receives email confirming deletion date
4. If user logs in during grace period → option to cancel deletion
5. After 30 days → permanent purge of all personal data
```

---

## Navigation Summary by Role

| Sidebar Item | Guest | Free User | Subscriber | Moderator | Admin | Super Admin |
|:--|:--:|:--:|:--:|:--:|:--:|:--:|
| Dashboard | — | — | — | ✅ | ✅ | ✅ |
| Tips > Today's Tips | — | — | ✅ (view) | ✅ (view/edit) | ✅ (full CRUD) | ✅ (full CRUD) |
| Tips > Results | — | — | ✅ | ✅ | ✅ | ✅ |
| Subscriptions > Packages | — | — | — | — | ✅ | ✅ |
| Subscriptions > Billing | — | — | — | — | ✅ | ✅ |
| Members | — | — | — | ✅ (view) | ✅ (manage) | ✅ (manage + impersonate) |
| Performance | — | — | ✅ | ✅ | ✅ | ✅ |
| Reports (Audit) | — | — | — | — | ✅ | ✅ |
| Content > CMS | — | — | — | ✅ (edit) | ✅ (full) | ✅ (full) |
| Content > Blog | — | — | — | ✅ (edit) | ✅ (full) | ✅ (full) |
| Content > Media | — | — | — | ✅ | ✅ | ✅ |
| Settings | — | — | — | ✅ | ✅ | ✅ |
| Profile | — | ✅ | ✅ | ✅ | ✅ | ✅ |
| Pricing | ✅ (public) | ✅ (sidebar) | — | — | — | — |
| Billing | — | ✅ (empty) | ✅ (active sub) | — | — | — |
| Referrals | — | — | ✅ | — | — | — |

---

## API Endpoints by Page

| Page | Primary API Endpoint | Auth Required |
|------|---------------------|:---:|
| Landing | `GET /api/plans` | No |
| Public Stats | `GET /api/analytics/public/stats` | No |
| Blog List | `GET /api/blog/published` | No |
| Login | `POST /api/auth/login` | No |
| Register | `POST /api/auth/register` | No |
| Tips Feed | `GET /api/tips/feed` | Yes |
| Results | `GET /api/analytics/subscriber/performance` | Yes |
| Billing | `GET /api/subscriptions/me` | Yes |
| Profile | `GET /api/profile` | Yes |
| Referrals | `GET /api/referrals/my-link` | Yes |
| Admin Dashboard | `GET /api/admin/dashboard/summary` | Admin |
| Tip Management | `GET /api/tips` | Admin |
| Plan Management | `GET /api/plans` + `GET /api/plans/promo-codes` | Admin |
| PayPal Dashboard | `GET /api/admin/dashboard/transactions` | Admin |
| User Management | `GET /api/users` | Admin |
| Analytics | `GET /api/analytics/admin/revenue` | Admin |
| Audit Log | `GET /api/audit` | Super Admin |
| CMS Pages | `GET /api/cms/pages` | Admin |
| Blog Admin | `GET /api/blog` | Admin |
| Media Library | `GET /api/media` | Admin |
| Navigation | `GET /api/navigation` | Admin |
