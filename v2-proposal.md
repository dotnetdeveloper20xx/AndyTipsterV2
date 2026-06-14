# AndyTipster V2 — Full Rebuild Proposal

## Overview

Complete rebuild of the AndyTipster horse racing tips subscription platform with a modern tech stack, enhanced features, and a scalable architecture designed for growth.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10 Web API (Minimal APIs + Controllers) |
| Frontend | Angular 20 + Tailwind CSS + DaisyUI |
| State Management | NgRx (Store, Effects, Selectors, Entity) |
| Database | SQL Server (Azure SQL) |
| ORM | Entity Framework Core |
| Auth | ASP.NET Core Identity + JWT + Social Login |
| Payments | PayPal (full integration) + Stripe (credit/debit cards) |
| CMS / Rich Text | Custom block-based editor (Angular CDK Drag-Drop + TipTap/Quill for rich text) |
| Media Storage | Azure Blob Storage + Azure CDN |
| Hosting | Azure App Service |
| Monitoring | Azure Application Insights |
| Email | SendGrid or Azure Communication Services |
| Push | Web Push API (Service Worker) |
| Messaging | Telegram Bot API |
| Images | AI-generated imagery (DALL-E / Midjourney assets) + Unsplash API |
| Animations | Angular Animations + Framer Motion (via wrapper) + CSS Transitions |

---

## Modules & Features

---

### 1. User Management System (Full)

#### 1.1 Authentication
- Registration with email/password (strong password policy)
- Social login: Google, Facebook, Apple
- Email verification with magic link option
- Password reset via email with token expiry
- Two-factor authentication (TOTP via authenticator app)
- Account lockout after failed attempts
- Remember me / persistent sessions
- JWT access tokens + refresh token rotation

#### 1.2 User Profiles
- Avatar upload (with crop/resize) or AI-generated default avatars
- Display name, bio, location, timezone
- Notification preferences (per channel, per category)
- Connected social accounts management
- Activity log (login history, subscription changes)
- Account deletion with data export (GDPR)

#### 1.3 Multi-Role System
| Role | Description | Permissions |
|------|-------------|-------------|
| Super Admin | Platform owner | Full system access, role management, billing config |
| Admin | Staff member | Manage content, subscribers, tips, view analytics |
| Moderator | Community manager | Manage comments, polls, reports |
| Subscriber | Paying user | Access tips based on plan, comment, vote |
| Free User | Registered non-paying | View public content, free tip of the day |
| Guest | Not logged in | View landing page, pricing, public stats |

#### 1.4 Role Management Frontend (Admin Panel)
- Create, edit, delete custom roles
- Granular permission matrix (CRUD per module)
- Assign/revoke roles per user
- Bulk role assignment
- Role hierarchy and inheritance
- Permission groups (e.g., "Content Editor" = tips + blog + FAQ permissions)
- Audit trail for all role/permission changes

#### 1.5 User Management Frontend (Admin Panel)
- Paginated, searchable, filterable user table
- Inline quick actions: suspend, activate, delete, impersonate
- User detail page: profile, subscription, payment history, activity
- Bulk actions: email, role change, suspend, export
- User impersonation (admin can view site as any user)
- Advanced filters: by role, plan, status, registration date, last login
- Export users as CSV/Excel

#### 1.6 Angular Frontend Architecture
- **NgRx Store** for global state (auth, user, roles, permissions)
- **NgRx Effects** for async operations (API calls, token refresh)
- **NgRx Selectors** for computed/derived state
- **NgRx Entity** for normalised collections (users, tips, plans)
- **Route Guards** based on roles and permissions
- **HTTP Interceptors** for JWT injection and token refresh
- **Lazy-loaded feature modules** (admin, subscriber, public)
- **Smart/Dumb component pattern** throughout
- **OnPush change detection** for performance
- **Reactive Forms** with custom validators

---

### 2. Modern UI & Visual Design

#### 2.1 Design System
- DaisyUI component library with custom theme (AndyTipster brand colours)
- Dark mode / Light mode toggle with system preference detection
- Consistent spacing, typography, and colour tokens
- Responsive breakpoints: mobile, tablet, desktop, wide

#### 2.2 Animations & Micro-interactions
- Page transition animations (route animations with Angular)
- Card hover effects (scale, shadow lift, gradient shift)
- Loading skeletons (shimmer effect) for all data-fetching states
- Success/error toast notifications with slide-in animation
- Modal open/close with backdrop fade and scale
- Scroll-triggered reveal animations (intersection observer)
- Number counter animations for stats (profit, ROI, streak)
- Confetti animation on successful subscription purchase
- Smooth accordion and tab transitions
- Button press feedback (ripple effect)
- Parallax scrolling on landing page hero section

#### 2.3 AI-Generated Imagery
- Hero section backgrounds: AI-generated racing/sports themed artwork
- Category banners: unique AI art per tip category (UK racing, Irish racing, etc.)
- Achievement badges: AI-designed badge icons for loyalty milestones
- Empty state illustrations: custom AI art for "no tips today", "no results yet"
- User default avatars: AI-generated unique avatars based on username seed
- Blog post featured images: AI-generated per topic
- 404 and error pages: engaging AI-generated illustrations
- Seasonal themes: AI art for major racing events (Cheltenham, Ascot, etc.)

---

### 3. Chatbot / Help Bot ("Can I Help You?" Widget)

#### 3.1 Bot Features
- Floating chat widget (bottom-right corner) on all pages
- Expandable/collapsible with smooth animation
- Pre-built conversation flows:
  - "How do I subscribe?"
  - "What plan is right for me?"
  - "How do I view my tips?"
  - "I have a payment issue"
  - "How do I cancel?"
  - "What's your refund policy?"
- Keyword detection for intelligent routing
- Escalation to admin (creates support ticket if bot can't resolve)
- Chat history persisted per user session
- Typing indicator animation
- Quick-reply buttons for common choices

#### 3.2 Admin Configuration
- Drag-and-drop conversation flow builder
- Custom Q&A pairs (admin adds questions and answers)
- Analytics: most asked questions, resolution rate, escalation rate
- Office hours configuration (show "leave a message" outside hours)
- Customisable bot avatar and name

---

### 4. Social Media Components (Reusable)

#### 4.1 Component Library
All social components are **standalone Angular components** that can be placed on any page with show/hide configuration per page via admin settings.

| Component | Description |
|-----------|-------------|
| Social Follow Bar | Icons linking to all social profiles (Facebook, Twitter/X, Instagram, Telegram, YouTube) |
| Share Buttons | Share current page/tip to social platforms with pre-filled text |
| Social Proof Counter | "Join 5,000+ subscribers" with animated counter |
| Twitter/X Feed Embed | Live feed of latest posts from AndyTipster account |
| Facebook Page Plugin | Embedded Facebook page with like button |
| Instagram Gallery | Latest posts grid from Instagram |
| Telegram Join Button | Direct link to Telegram channel with member count |
| YouTube Latest Video | Embedded latest video from channel |
| Social Login Buttons | Google, Facebook, Apple sign-in buttons |
| Testimonial Cards | Social-style cards with user avatar, quote, rating |

#### 4.2 Configuration (Admin Panel)
- Per-page visibility toggle for each social component
- Drag-and-drop positioning (top, bottom, sidebar, floating)
- A/B testing: show different components to different user segments
- Social media links management (update URLs without code changes)
- Analytics: clicks, shares, conversions per component

#### 4.3 Social Sharing Features
- Auto-generate Open Graph meta tags per page
- Custom share images (AI-generated per tip/result)
- Share results with P&L stats as image cards (like betting slip style)
- "Share your winning tip" CTA after tip is marked as Won
- Social sharing incentive: "Share and get 10% off next month"

---

### 5. GDPR & Cookie Compliance (Industry Standard)

#### 5.1 Cookie Consent
- Cookie consent banner on first visit (bottom bar or modal)
- Granular cookie preferences:
  - **Essential** (always on): auth, session, CSRF
  - **Analytics**: Application Insights, usage tracking
  - **Marketing**: social media pixels, conversion tracking
  - **Preferences**: theme, language, notification settings
- "Accept All" / "Reject All" / "Customise" buttons
- Consent saved and retrievable (auditable)
- Re-consent trigger if cookie policy changes
- Consent expiry (re-ask after 12 months)

#### 5.2 GDPR Features
- **Right to Access**: User can download all their data (JSON/CSV export)
- **Right to Erasure**: Account deletion with 30-day grace period
- **Right to Rectification**: Users can edit all personal data
- **Right to Portability**: Export data in machine-readable format
- **Data Processing Records**: Admin view of what data is collected and why
- **Consent Management**: Track what each user has consented to and when
- **Data Retention Policies**: Auto-purge inactive accounts after configurable period
- **Privacy Policy**: Dynamic page (admin-editable, versioned)
- **Terms of Service**: Dynamic page (admin-editable, versioned)
- **Breach Notification**: Admin tool to notify all users of data breach

#### 5.3 Technical Implementation
- Cookie service (Angular) that blocks non-essential scripts until consent
- Server-side consent validation on API endpoints
- Anonymised analytics option (no PII in tracking)
- IP anonymisation in logs
- Encrypted PII fields in database
- Admin GDPR dashboard: data requests, deletions, consent stats

---

### 6. Subscription Plans System (Generic & Flexible)

#### 6.1 Plan Builder (Admin)
Plans are fully generic — admin can create any plan with any combination of fields:

| Field | Type | Description |
|-------|------|-------------|
| Name | String | Plan display name |
| Slug | String | URL-friendly identifier |
| Description | Rich Text | Full description with formatting |
| Short Description | String | One-liner for pricing cards |
| Price | Decimal | Amount in selected currency |
| Currency | Enum | GBP, EUR, USD (extensible) |
| Billing Cycle | Enum | Weekly, Monthly, Quarterly, Semi-Annual, Annual |
| Trial Period | Integer | Days of free trial (0 = no trial) |
| Setup Fee | Decimal | One-time initial fee (0 = none) |
| Features | List<String> | Bullet points shown on pricing card |
| Categories Included | Multi-select | Which tip categories this plan grants access to |
| Max Users | Integer | For future group/team plans (0 = unlimited) |
| Is Active | Boolean | Show/hide on pricing page |
| Sort Order | Integer | Display order on pricing page |
| Badge | String | "Popular", "Best Value", "New" — shown as ribbon |
| Colour Theme | Colour Picker | Card accent colour |
| Icon | Upload/Select | Plan icon or AI-generated image |
| Promo Code Compatible | Boolean | Can promo codes apply to this plan |
| Auto-Renew | Boolean | Does the plan auto-renew |
| Grace Period Days | Integer | Days after failed payment before access revoked |

#### 6.2 Plan Features
- Unlimited number of plans
- Plans can be archived (hidden but existing subscribers remain)
- Plan comparison table auto-generated from features list
- Upgrade/downgrade paths configurable per plan
- Plan-specific landing pages (deep-link for marketing campaigns)
- A/B test different pricing on different plan variants

---

### 7. PayPal Full Integration (Complete PayPal Dashboard in App)

#### 7.1 PayPal Subscription Plans (Linked)
- Each app subscription plan maps 1:1 to a PayPal Billing Plan
- Admin creates plan in our app → automatically creates matching PayPal plan via API
- Plan updates sync both ways (price changes, status changes)
- Plan activation/deactivation synced with PayPal

#### 7.2 PayPal Checkout & Subscriptions
- PayPal Smart Buttons embedded in checkout (PayPal, Pay Later, Debit/Credit)
- Subscription creation via PayPal Subscriptions API v1/v2
- Support for:
  - Monthly, quarterly, annual recurring billing
  - Free trial periods
  - Setup fees
  - Plan upgrades/downgrades (revision)
  - Subscription pause and resume
  - Subscription cancellation with reason
- Return/Cancel URL handling with token verification
- Subscriber approval flow (redirect to PayPal → return to app)

#### 7.3 PayPal Payments (One-Off)
- One-time payments for add-ons or premium content
- PayPal Orders API for instant captures
- Payment authorization and capture (hold then charge)
- Refund processing (full and partial)
- Payment status tracking

#### 7.4 PayPal Webhooks (Real-Time Events)
- Webhook endpoint for all PayPal events:
  - `BILLING.SUBSCRIPTION.CREATED`
  - `BILLING.SUBSCRIPTION.ACTIVATED`
  - `BILLING.SUBSCRIPTION.SUSPENDED`
  - `BILLING.SUBSCRIPTION.CANCELLED`
  - `BILLING.SUBSCRIPTION.EXPIRED`
  - `BILLING.SUBSCRIPTION.PAYMENT.FAILED`
  - `PAYMENT.SALE.COMPLETED`
  - `PAYMENT.SALE.REFUNDED`
  - `BILLING.PLAN.CREATED`
  - `BILLING.PLAN.UPDATED`
- Webhook signature verification for security
- Retry handling for failed webhook processing
- Webhook event log with full payload (admin viewable)

#### 7.5 PayPal Dashboard (In-App Admin Panel)
Replicate key PayPal dashboard functionality inside the application:

**Transactions View**
- Full transaction history with search, filter, sort
- Filter by: date range, status, amount, subscriber, plan
- Transaction details: amount, fees, net, status, PayPal ID
- Export transactions as CSV/PDF
- Daily/weekly/monthly transaction summaries

**Subscription Management**
- All active subscriptions with status
- Subscriber details linked to app user profile
- Actions: suspend, reactivate, cancel, update
- Subscription lifecycle timeline (created → active → payments → cancelled)
- Next billing date and amount display
- Payment history per subscription

**Revenue Analytics**
- MRR (Monthly Recurring Revenue) with trend chart
- ARR (Annual Recurring Revenue)
- Revenue by plan breakdown (pie chart)
- Revenue over time (line chart)
- Failed payment rate and recovery stats
- Churn rate (monthly/quarterly)
- LTV (Lifetime Value) per subscriber
- Net revenue after PayPal fees

**Billing Plans**
- All plans with status (Active, Inactive, Created)
- Subscriber count per plan
- Revenue per plan
- Plan creation/edit form synced with PayPal
- Plan pricing history

**Refunds & Disputes**
- Refund requests with approval workflow
- Dispute/chargeback tracking
- Refund history with reasons
- Auto-suspend access on chargeback

**Payouts (if applicable)**
- Payout history
- Pending payouts
- Payout schedule configuration

#### 7.6 PayPal Sandbox & Live Switching
- Environment toggle: Sandbox ↔ Live
- Separate credentials per environment (stored in Azure Key Vault)
- Test mode indicator in UI (banner: "SANDBOX MODE — No real charges")
- Sandbox webhook testing tools

---

### 8. Content Management System (Full CMS)

The CMS empowers Admin and Super Admin to edit all website content visually — no code changes required. It functions as a true headless CMS built into the application.

#### 8.1 Page Builder (Visual Editor)
- Drag-and-drop block-based page editor with live preview
- WYSIWYG editing — what admin sees is what the public sees
- Pages supported: Landing, About, FAQ, Contact, How It Works, Privacy, Terms, Blog posts, Custom pages
- Add/remove/reorder content blocks per page
- Responsive preview (mobile, tablet, desktop toggle in editor)
- Save as draft, preview before publish, schedule publication
- Undo/redo with full action history per editing session
- Keyboard shortcuts for power users
- Auto-save every 30 seconds

#### 8.2 Content Blocks (Reusable Components)
Pre-built blocks that admin can drag onto any page:

| Block Type | Description |
|-----------|-------------|
| Hero Section | Full-width banner with background image/video, heading, subheading, CTA button |
| Rich Text | WYSIWYG text editor with formatting, links, embeds |
| Image | Single image with caption, alt text, link |
| Image Gallery | Grid/carousel of images with lightbox |
| Video Embed | YouTube, Vimeo, or self-hosted video |
| Call to Action (CTA) | Highlighted box with heading, text, button |
| Pricing Table | Auto-generated from subscription plans or manually configured |
| Testimonials | Carousel or grid of subscriber testimonials |
| FAQ Accordion | Collapsible question/answer pairs |
| Stats Counter | Animated number counters (profit, subscribers, win rate) |
| Social Media Feed | Embedded Twitter/Instagram/Facebook feed |
| Social Follow Bar | Social platform icons with links |
| Contact Form | Configurable form fields with email delivery |
| Divider/Spacer | Visual spacing between blocks |
| HTML/Embed | Raw HTML or third-party embed code |
| Blog Post List | Latest posts grid with featured images |
| Tip of the Day | Free tip preview block for non-subscribers |
| Countdown Timer | Timer to next tip publication or event |
| Banner/Alert | Top-of-page notification bar (dismissible) |

#### 8.3 Media Library (Centralised Asset Management)
- **Upload**: Drag-and-drop single or bulk file upload
- **File types**: Images (JPG, PNG, WebP, SVG, GIF), Documents (PDF, DOCX), Videos (MP4)
- **Organisation**: Folder tree structure, tags, search by filename/tag
- **Image Editing**: In-browser crop, resize, rotate, compress
- **Auto-optimisation**: Images auto-compressed and converted to WebP on upload
- **CDN Delivery**: All assets served via Azure CDN for fast global delivery
- **Alt Text**: Required field for accessibility compliance
- **Usage Tracking**: See which pages/blocks use each asset
- **Storage Quota**: Configurable per role (Admin vs Super Admin)
- **Bulk Actions**: Multi-select delete, move to folder, tag
- **AI Image Generation**: Generate images directly from the media library using text prompts (DALL-E integration)
- **Unsplash/Pexels Integration**: Search and import free stock photos directly

#### 8.4 Social Media Widget Configuration (Per-Page)
- Admin toggles social media components on/off per page from the CMS
- Drag-and-drop reordering of social widgets within page layout
- Per-widget configuration:
  - Which platforms to show (Facebook, Twitter/X, Instagram, Telegram, YouTube, TikTok)
  - Display style: icons only, icons + labels, full embed
  - Position: header, footer, sidebar, inline, floating
  - Visibility rules: show to all / subscribers only / logged-out only
- Global social media settings:
  - Platform URLs (one place to update all links site-wide)
  - Default share text templates per platform
  - Social proof counter configuration (real or manual number)
- Preview changes before publishing

#### 8.5 Navigation & Menu Management
- Visual tree editor for site navigation
- Multiple menu locations: header, footer, sidebar, mobile
- Drag-and-drop reordering and nesting (sub-menus)
- Menu item types: page link, external URL, category, custom
- Visibility rules per menu item: by role, by subscription status
- Icon selection per menu item (Font Awesome / custom upload)
- "New" or "Hot" badges on menu items (configurable)
- Mobile navigation separately configurable

#### 8.6 SEO Management (Per Page)
- Meta title and description editor with character counter
- Open Graph image upload/select from media library
- Custom URL slug editor
- Canonical URL configuration
- Structured data (JSON-LD) for rich search results
- XML sitemap auto-generation
- Robots.txt configuration
- Noindex/nofollow toggle per page
- Social preview card (how it looks when shared on Facebook/Twitter)
- SEO score indicator with improvement suggestions

#### 8.7 Version History & Rollback
- Every content save creates a version snapshot
- Version list with: timestamp, author, change summary
- Side-by-side diff view (previous vs current)
- One-click rollback to any previous version
- Draft versions (work in progress without affecting live site)
- Version retention policy (configurable: keep last N versions or all)

#### 8.8 Scheduled Publishing & Expiry
- Set publish date/time for any page or content block
- Set expiry date/time (auto-unpublish)
- Publishing queue view (upcoming scheduled content)
- Timezone-aware scheduling
- Email notification to admin when content goes live
- Recurring content (e.g., "show this banner every Friday")

#### 8.9 Global Site Settings (Admin)
- Site name, tagline, logo upload (light + dark variants)
- Favicon upload
- Default colour scheme / theme selection
- Footer content editor (text, links, copyright)
- Maintenance mode toggle (show "coming soon" page to public)
- Custom CSS injection (for minor styling tweaks)
- Analytics script injection (Google Analytics, Meta Pixel, etc.)
- Cookie consent text customisation
- 301 Redirect management (old URL → new URL)

---

### 9. Tips Engine (Daily Tips System)

#### 8.1 Tip Publishing
- Admin creates tips with: event date, race/match name, selection, odds, stake, category
- Rich text editor for additional commentary per tip
- Scheduling — queue tips for future auto-publication at a set time
- Draft/published/archived status workflow
- Bulk import tips from spreadsheet (CSV)

#### 8.2 Tip Categories
- UK Horse Racing (AndyTipster brand)
- Irish Horse Racing
- Other Sports (football, greyhounds, etc.)
- Admin can create new categories dynamically

#### 8.3 Result Tracking
- Admin marks each tip as: Won, Lost, Void, Push
- Automated P&L calculation per day/week/month/year
- Running profit tracker per category
- Tips history with full archive, searchable by date, category, result

---

### 10. Performance Analytics & Proof

#### 9.1 Public (Marketing)
- Public stats page showing strike rate, ROI, monthly P&L
- Charts: profit over time, win rate trends, category comparison
- Last 30 days performance summary on landing page
- Exportable results (CSV/PDF) for transparency

#### 9.2 Subscriber Dashboard
- Personal P&L tracker (if following all tips at level stakes)
- Filterable by category and date range
- Current winning/losing streak display
- Monthly performance summaries with email digest option

---

### 11. Notification System

#### 10.1 Channels
- Email alerts (new tips posted, results updated)
- Web push notifications (browser)
- Telegram bot — delivers tips directly to Telegram chat
- In-app notification bell with unread count

#### 10.2 Configuration
- Per-user preferences: choose channels, categories, quiet hours
- Subscription renewal reminders (7 days, 1 day before)
- Payment failure alerts
- Welcome sequence for new subscribers (onboarding emails)

---

### 12. Admin Dashboard

#### 11.1 Subscriber Management
- View, search, filter, sort all subscribers
- Export subscriber list (CSV)
- View individual subscriber details (plan, payment history, activity)
- Manually grant/revoke access
- Bulk actions (email all, suspend, etc.)

#### 11.2 Revenue Analytics
- Full PayPal dashboard (see Section 7.5)
- Stripe revenue metrics alongside PayPal
- Combined revenue view across payment providers
- Forecasting based on current subscriber trajectory

#### 11.3 Content Management
- Tips CRUD with scheduling
- Landing page editor (drag-and-drop blocks)
- FAQ management
- About page editor
- Blog post editor (rich text, images, SEO fields)
- Testimonials management
- Social media component visibility per page

#### 11.4 System
- Audit log of all admin actions
- Webhook event log (PayPal + Stripe events)
- System health dashboard
- GDPR compliance dashboard

---

### 13. Landing Page & Public Site

- Modern marketing landing page with hero section (AI-generated artwork)
- Animated pricing table with plan comparison
- Public results/proof section (recent performance)
- Testimonials carousel (admin-managed)
- "Tip of the Day" free preview — one tip visible to non-subscribers
- FAQ section with search
- Blog / news section (SEO-optimized, admin-authored)
- Contact form with email delivery
- How it works / Getting started guide
- Responsive mobile-first design throughout
- Parallax scrolling and scroll-triggered animations

---

### 14. Engagement & Growth Features

#### 13.1 Referral Program
- Unique referral link per subscriber
- Reward: discount on next billing cycle for successful referral
- Referral tracking dashboard (clicks, conversions)
- Admin configurable reward amounts

#### 13.2 Loyalty & Retention
- Long-term subscriber rewards (6-month, 12-month milestones)
- Bonus content for loyal subscribers
- Win-back campaigns for cancelled subscribers (automated emails)

#### 13.3 Social
- Social sharing buttons on results pages
- Subscriber ratings/reviews system (displayed on landing page)
- Share tip results to Twitter/Facebook with one click

---

### 15. Communication & Community

- Admin broadcast messaging (announcements to all subscribers)
- Comments section under each day's tips (subscribers discuss picks)
- Polls (e.g., "Which race are you backing today?")
- Direct message from subscriber to admin (support)
- Read receipts on announcements

---

### 16. Mobile Experience (PWA)

- Progressive Web App — installable on mobile home screen
- Offline access to previously loaded tips
- Mobile-optimised tip cards with swipe navigation
- Bottom navigation bar for key sections
- Native-feel animations and transitions
- App-like splash screen and loading states

---

## Non-Functional Requirements

### Security
- HTTPS everywhere (enforced via Azure)
- OWASP Top 10 compliance
- Rate limiting on all API endpoints
- Input validation and sanitisation
- CORS configuration for Angular frontend
- Secrets in Azure Key Vault (no hardcoded credentials)
- SQL injection prevention via parameterised queries (EF Core)
- PayPal webhook signature verification
- JWT token encryption and refresh rotation

### Performance
- API response time < 200ms for standard endpoints
- Lazy loading of Angular modules
- Image optimisation and CDN delivery (Azure CDN)
- Database indexing strategy
- Caching layer (Redis or in-memory) for frequently accessed data
- Virtual scrolling for large lists
- OnPush change detection throughout Angular app

### Compliance
- GDPR compliant: full cookie consent, data export, deletion, consent management
- Cookie consent with granular preferences (industry standard implementation)
- Privacy policy and terms of service (admin-editable, versioned)
- Data retention policies with auto-purge
- PCI DSS compliance via PayPal/Stripe hosted fields (no card data on our servers)

### Reliability
- Azure SQL geo-redundant backups
- Health check endpoints
- Graceful error handling with user-friendly messages
- Structured logging (Serilog → Application Insights)
- Automated alerts on error spikes
- PayPal webhook retry and dead-letter handling

### DevOps (Future — Not in V2 Initial Release)
- CI/CD pipeline planned for post-launch
- For V2 launch: manual deployment to Azure App Service
- Database migrations via EF Core CLI

---

## Proposed Build Phases

### Phase 1 — Foundation (Weeks 1–4)
- Project scaffolding (.NET 10 API + Angular 20 app)
- NgRx store architecture setup
- Database design and EF Core setup
- Authentication system (Identity + JWT + social login)
- Multi-role system with permission matrix
- User management admin panel
- Basic UI theme (DaisyUI + Tailwind + dark mode)

### Phase 2 — Payments & Subscriptions (Weeks 5–9)
- Generic subscription plan builder (admin)
- Full PayPal integration (plans, subscriptions, webhooks)
- Stripe integration (cards, subscriptions, webhooks)
- Checkout flow with payment method choice
- PayPal dashboard (in-app admin panel)
- Self-service subscription management
- Free trial and promo code system

### Phase 3 — CMS & Content (Weeks 10–14)
- Page builder with drag-and-drop blocks
- Media library with upload, crop, CDN delivery
- Content blocks library (all block types)
- Navigation & menu management
- Social media widget per-page configuration
- SEO management tools
- Version history and rollback
- Scheduled publishing
- Global site settings

### Phase 4 — Tips Engine (Weeks 15–17)
- Tip creation, editing, scheduling
- Category management
- Result tracking and P&L calculation
- Tips archive and search
- Content access gating based on subscription
- Blog system (integrated with CMS)

### Phase 5 — Social, Bot & Engagement (Weeks 18–21)
- Social media component library (reusable, configurable)
- Help bot widget with conversation flows
- Notification system (email, push, Telegram)
- Referral program
- Comments and community features
- Polls system

### Phase 6 — Analytics & Compliance (Weeks 22–24)
- Performance analytics (public + subscriber dashboard)
- Admin revenue analytics
- GDPR implementation (consent, export, deletion)
- Cookie consent system
- AI-generated imagery integration
- Animations and micro-interactions polish

### Phase 7 — PWA & Launch (Weeks 25–26)
- PWA setup and mobile optimisation
- Offline support
- Performance testing and optimisation
- Security audit
- Production deployment and go-live

---

## Summary

AndyTipster V2 is a complete platform rebuild delivering:
- **Full user management** with multi-role system and admin frontend
- **Modern Angular architecture** with NgRx, lazy loading, and reactive patterns
- **Stunning UI** with DaisyUI, animations, and AI-generated artwork
- **True CMS** — drag-and-drop page builder, media library, content blocks, version history, and scheduled publishing
- **Complete PayPal integration** — subscriptions, payments, webhooks, and an in-app PayPal dashboard
- **Flexible subscription system** — generic plan builder, any plan shape
- **Help bot** for subscriber support
- **Reusable social media components** configurable per page via CMS
- **Industry-standard GDPR & cookie compliance**
- **Multi-channel tip delivery** (web, email, push, Telegram)
- **Community features** for subscriber retention
- **Mobile-first PWA** experience

Total estimated build time: **26 weeks** for a single developer, with potential to parallelise frontend and backend work.
