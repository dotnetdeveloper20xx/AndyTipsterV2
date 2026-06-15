# Implementation Plan: AndyTipster V2 Platform

## Overview

This plan implements the AndyTipster V2 horse racing tips subscription platform as a .NET 10 Web API backend with Angular 20 frontend, deployed on Azure. Tasks are organized by build phase, with each task referencing specific requirements from the spec. The backend uses C# with EF Core and the frontend uses TypeScript with Angular 20 + NgRx + Tailwind CSS + DaisyUI.

## Tasks

- [x] 1. Phase 1 — Foundation: Project scaffolding and core infrastructure
  - [x] 1.1 Create .NET 10 Web API project with layered architecture
    - Create solution structure: `src/AndyTipster.Api`, `src/AndyTipster.Application`, `src/AndyTipster.Domain`, `src/AndyTipster.Infrastructure`
    - Configure Minimal APIs + Controllers hybrid pattern
    - Add Serilog structured logging with Application Insights sink
    - Configure ProblemDetails error handling middleware (RFC 7807)
    - Add health check endpoints for database, PayPal API, and Azure Blob Storage
    - Configure HTTPS enforcement, CORS, rate limiting, and security headers
    - _Requirements: 46.1, 47.1, 47.2, 47.3, 47.5, 48.1, 48.2, 48.5_

  - [x] 1.2 Set up Azure SQL database with EF Core and entity models
    - Create EF Core DbContext with all entity configurations
    - Define entities: ApplicationUser, Role, Permission, UserRole, RolePermission, Plan, Subscription, Payment, Tip, TipResult, CmsPage, PageVersion, PageBlock, MediaAsset, Notification, AuditLog, RefreshToken, GdprConsent, PromoCode, Referral, Comment
    - Define enumerations: SubscriptionStatus, PaymentProvider, BillingCycle, Currency, TipStatus, TipResult, PlanSyncStatus, PageStatus, NotificationType
    - Create initial EF Core migration
    - Configure Azure SQL geo-redundant backup settings
    - _Requirements: 46.1, 47.6, 48.3_

  - [x] 1.3 Create Angular 20 project with NgRx architecture and lazy-loaded modules
    - Initialize Angular 20 project with standalone components
    - Configure NgRx Store with feature slices: auth, user, roles, permissions, tips, plans
    - Set up NgRx Effects for async API operations
    - Configure lazy-loaded feature modules: PublicModule, AuthModule, SubscriberModule, AdminModule
    - Set up HTTP interceptors for JWT injection and token refresh queuing
    - Set up route guards based on roles and permissions
    - Apply OnPush change detection strategy on all components
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 6.7, 6.8_

  - [x] 1.4 Implement DaisyUI theme system with dark mode support
    - Install and configure Tailwind CSS + DaisyUI with custom theme tokens (primary, secondary, accent, neutral)
    - Implement dark/light mode toggle with local storage persistence
    - Add system colour scheme detection via prefers-color-scheme media query
    - Configure responsive breakpoints: mobile (375px), tablet (768px), desktop (1024px), wide (1280px)
    - Implement skeleton loaders with shimmer animation and 10-second timeout to error state
    - Implement empty state components with illustration, message, and CTA
    - Configure transition durations: 150ms micro-interactions, 300ms standard, 500ms emphasis
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7, 7.8_

  - [ ]* 1.5 Write property test for Data Table sort and filter correctness
    - **Property 19: Data Table Sort and Filter Correctness**
    - **Validates: Requirements 45.1, 45.2, 45.3, 45.4**

  - [x] 1.6 Implement generic Data Table component
    - Build reusable Data Table with pagination (10, 25, 50, 100 page sizes) and total count
    - Add global text search with 300ms debounce
    - Add per-column sorting (ascending/descending) and multi-column sort
    - Add per-column filtering with type-appropriate controls (text, dropdown, date range)
    - Add row selection with checkboxes and contextual bulk action bar
    - Add CSV/Excel export for filtered/selected rows
    - Add skeleton rows during loading state
    - Add empty state with illustration and CTA
    - Add error state with retry button
    - Add mobile-responsive layout (horizontal scroll or card-based)
    - _Requirements: 45.1, 45.2, 45.3, 45.4, 45.5, 45.6, 45.7, 45.8, 45.9, 45.10_

  - [x] 1.7 Implement accessibility compliance baseline
    - Configure 4.5:1 minimum contrast ratio in theme tokens
    - Add ARIA labels, roles, and states to all interactive components
    - Implement full keyboard navigation with visible focus indicators
    - Add alt text requirement enforcement for images
    - Set up axe-core automated scanning in test pipeline
    - _Requirements: 49.1, 49.2, 49.3, 49.4, 49.5_

- [x] 2. Phase 1 — Foundation: Authentication and authorization
  - [x] 2.1 Implement user registration with email verification
    - Configure ASP.NET Core Identity with custom ApplicationUser
    - Implement registration endpoint with password complexity validation (8+ chars, uppercase, lowercase, digit, special char)
    - Send verification email with 24-hour expiry link
    - Implement verification endpoint with link expiry and reuse checks
    - Reject login for unverified users with instructive message
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6_

  - [ ]* 2.2 Write property test for password validation completeness
    - **Property 1: Password Validation Completeness**
    - **Validates: Requirements 1.2**

  - [x] 2.3 Implement JWT authentication with refresh token rotation
    - Issue JWT access token (15-min expiry) with user ID, roles, permissions claims
    - Issue refresh token (7-day expiry) with secure storage
    - Implement token refresh endpoint with refresh token rotation and old token invalidation
    - Implement account lockout after 5 failed attempts within 30 minutes (15-min lock)
    - Send lockout notification email
    - Implement password reset with single-use 1-hour token
    - _Requirements: 1.7, 1.9, 1.10, 1.11, 1.12_

  - [ ]* 2.4 Write property test for JWT token structure and expiry
    - **Property 2: JWT Token Structure and Expiry**
    - **Validates: Requirements 1.7**

  - [ ]* 2.5 Write property test for token refresh rotation round-trip
    - **Property 3: Token Refresh Rotation Round-Trip**
    - **Validates: Requirements 1.9**

  - [ ]* 2.6 Write property test for authentication lockout threshold
    - **Property 4: Authentication Lockout Threshold**
    - **Validates: Requirements 1.10, 2.3**

  - [x] 2.7 Implement social login (Google, Facebook, Apple)
    - Configure OAuth2 providers for Google, Facebook, and Apple
    - Create or link account based on matching email address
    - _Requirements: 1.8_

  - [x] 2.8 Implement two-factor authentication (TOTP)
    - Generate TOTP secret and QR code for authenticator app registration
    - Require valid TOTP code confirmation before activating 2FA
    - Validate TOTP codes with 30-second time step and ±1 clock skew tolerance
    - Implement lockout after 5 consecutive invalid TOTP codes
    - Generate 8 single-use recovery codes on 2FA activation
    - Allow login via unused recovery code, mark as consumed
    - Prompt re-registration when fewer than 2 recovery codes remain
    - Allow 2FA disable after password confirmation
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6_

  - [ ]* 2.9 Write property test for TOTP time window validation
    - **Property 5: TOTP Time Window Validation**
    - **Validates: Requirements 2.2**

  - [ ]* 2.10 Write property test for recovery code single-use consumption
    - **Property 6: Recovery Code Single-Use Consumption**
    - **Validates: Requirements 2.6**

  - [x] 2.11 Implement multi-role authorization system
    - Define six base roles: Super Admin, Admin, Moderator, Subscriber, Free User, Guest
    - Implement role hierarchy enforcement (can only assign roles below own level)
    - Implement custom role creation with selected permissions
    - Implement permission union for multi-role users
    - Assign Free User role by default on registration
    - Prevent deletion of roles assigned to users
    - Log all role changes in audit trail
    - Enforce HTTP 401 for unauthenticated, HTTP 403 for unauthorized access
    - _Requirements: 3.1, 3.2, 3.4, 3.5, 3.6, 3.7, 3.8, 3.9, 3.10_

  - [ ]* 2.12 Write property test for permission resolution from role combinations
    - **Property 7: Permission Resolution from Role Combinations**
    - **Validates: Requirements 3.3, 3.10**

  - [ ]* 2.13 Write property test for role hierarchy enforcement
    - **Property 8: Role Hierarchy Enforcement**
    - **Validates: Requirements 3.7**

  - [x] 2.14 Implement Angular auth module with NgRx auth state
    - Build login, register, forgot password, 2FA setup components
    - Implement NgRx auth slice (tokens, login status, user info)
    - Implement HTTP interceptor for JWT injection with token refresh queuing
    - Implement route guards for role/permission-based navigation
    - Handle token refresh failure: clear state, redirect to login
    - Hide navigation items based on user role/permissions
    - _Requirements: 3.3, 6.1, 6.2, 6.5, 6.6, 6.7_

- [x] 3. Checkpoint — Phase 1 Auth & Core
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Phase 1 — Foundation: User management
  - [x] 4.1 Implement user management API endpoints
    - Build paginated user listing with search by name/email and filters (role, plan, status, date ranges)
    - Target 200ms response time for datasets up to 100,000 users
    - Implement user impersonation (read-only session with banner)
    - Implement bulk actions (suspend, role change, export) with confirmation and failure reporting
    - Implement CSV/Excel export for filtered dataset (up to 100,000 records within 30 seconds)
    - Implement suspend action that revokes all active sessions/tokens within 5 seconds
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8_

  - [x] 4.2 Implement user profile management API
    - Avatar upload with crop/resize to 256x256, validate file type (JPG, PNG, WebP, GIF) and size (max 5MB)
    - Display name (3-50 chars), bio (max 500 chars), timezone update
    - Activity log with login history and subscription changes (50 per page)
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

  - [x] 4.3 Implement Angular user management admin panel
    - Build user management page with Data Table integration (25 rows default, sort by registration date desc)
    - Add search/filter controls, inline quick actions
    - Build impersonation UI with persistent banner and end control
    - Build bulk action confirmation dialog with user count and action type
    - Build user profile settings page with tabs: Profile, Security, Notifications, Billing, Privacy, Appearance
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 5.6_

  - [x] 4.4 Implement audit logging system
    - Create append-only audit log for all admin actions (actor, target, action type, timestamp, before/after values)
    - Build searchable, filterable, paginated audit log view in admin panel
    - Configure 2-year retention policy
    - Ensure audit log is not editable by any user
    - _Requirements: 44.1, 44.2, 44.3, 44.4_

- [x] 5. Checkpoint — Phase 1 Complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 6. Phase 2 — Payments: Subscription plan builder
  - [x] 6.1 Implement subscription plan builder API
    - Build plan CRUD with validation: name (1-100 chars, unique), price (0.01-999,999.99), currency (GBP/EUR/USD), billing cycle, features list (1-50 items)
    - Support billing cycles: Weekly, Monthly, Quarterly, Semi-Annual, Annual
    - Support trial period (1-365 days), setup fee (0.00-999,999.99), grace period (0-90 days), auto-renewal, promo code compatibility
    - Implement plan archiving (hidden from pricing, existing subscribers unaffected)
    - Implement upgrade/downgrade path configuration
    - Auto-sync plan to PayPal Billing Plans API on save, handle sync failures with "sync pending" status and manual retry
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7, 8.8, 8.9_

  - [ ]* 6.2 Write property test for plan field validation
    - **Property 9: Plan Field Validation**
    - **Validates: Requirements 8.1, 8.3, 8.6, 8.9**

  - [ ]* 6.3 Write property test for subscription plan transition path enforcement
    - **Property 10: Subscription Plan Transition Path Enforcement**
    - **Validates: Requirements 8.5**

  - [x] 6.4 Implement promo code system
    - Build promo code CRUD: discount type (percentage/fixed), applicable plans, maximum uses, expiry date
    - Validate promo codes at checkout: check expiry, max usage, plan compatibility
    - Calculate discounted price based on discount configuration
    - _Requirements: 14.3, 14.4, 14.5_

  - [ ]* 6.5 Write property test for promo code discount calculation
    - **Property 12: Promo Code Discount Calculation**
    - **Validates: Requirements 14.4, 14.5**

  - [x] 6.6 Implement free trial system
    - Grant full plan access for trial duration without charge
    - Auto-begin billing when trial expires
    - Display trial duration and billing start date in checkout
    - _Requirements: 14.1, 14.2, 11.3_

- [x] 7. Phase 2 — Payments: PayPal integration
  - [x] 7.1 Implement PayPal subscription flow
    - Render PayPal Smart Buttons on checkout page
    - Initiate subscription via PayPal Subscriptions API with PayPal Billing Plan ID
    - Activate subscription and grant access within 10 seconds of return
    - Handle cancel/abandon: return to checkout with plan preserved
    - Implement subscription pause via PayPal API
    - Retry failed operations up to 3 times with exponential backoff
    - _Requirements: 9.1, 9.2, 9.3, 9.10, 9.11_

  - [x] 7.2 Implement PayPal webhook processing
    - Build webhook endpoint at `POST /api/webhooks/paypal`
    - Verify webhook signatures, return 401 on failure
    - Implement idempotent processing using PayPal event ID as deduplication key
    - Handle `BILLING.SUBSCRIPTION.ACTIVATED`: update status
    - Handle `BILLING.SUBSCRIPTION.CANCELLED`: revoke access at period end
    - Handle `BILLING.SUBSCRIPTION.PAYMENT.FAILED`: mark past-due, notify within 5 minutes, retain access for grace period
    - Handle `PAYMENT.SALE.COMPLETED`: record transaction (amount, fees, net, PayPal ID)
    - Respond to all webhooks within 10 seconds
    - _Requirements: 9.4, 9.5, 9.6, 9.7, 9.8, 9.9, 9.12_

  - [ ]* 7.3 Write property test for webhook idempotent processing
    - **Property 11: Webhook Idempotent Processing**
    - **Validates: Requirements 9.9, 10.8**

- [x] 8. Phase 2 — Payments: Stripe integration
  - [x] 8.1 Implement Stripe subscription flow
    - Display Stripe hosted payment fields (no raw card storage for PCI DSS compliance)
    - Create subscription via Stripe Subscriptions API
    - Activate subscription and grant access within 10 seconds
    - _Requirements: 10.1, 10.2, 10.6_

  - [x] 8.2 Implement Stripe webhook processing
    - Build webhook endpoint at `POST /api/webhooks/stripe`
    - Verify webhook signatures using Stripe signing secret, return 400 on failure
    - Implement idempotent processing
    - Handle `invoice.payment_failed`: mark past-due, email within 60 seconds
    - Handle `customer.subscription.deleted`: revoke access at period end
    - Handle `invoice.payment_succeeded`: record transaction (amount, currency, payment intent ID)
    - _Requirements: 10.3, 10.4, 10.5, 10.7, 10.8, 10.9_

- [x] 9. Phase 2 — Payments: Checkout and self-service
  - [x] 9.1 Implement checkout flow UI
    - Build checkout page with payment method selection (PayPal, Stripe)
    - Display order summary with plan details
    - Integrate promo code entry with validation and discounted price display
    - Display trial duration and billing start date when applicable
    - Redirect to confirmation page on success (subscription details, next billing date)
    - Display error message on failure with retry/alternative payment option
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5_

  - [x] 9.2 Implement subscription self-service management
    - Build billing settings page: current plan, next billing date, payment method, payment history
    - Implement plan upgrade with prorated amount calculation
    - Implement cancellation with access maintained until period end
    - Handle payment failure with grace period expiry: revoke access, notify subscriber
    - _Requirements: 13.1, 13.2, 13.3, 13.4_

  - [x] 9.3 Implement PayPal admin dashboard
    - Build transaction history with search, date range filter, status filter, amount sorting
    - Build subscription lifecycle timeline view
    - Display revenue analytics: MRR, ARR, churn rate, revenue by plan
    - Implement refund processing via PayPal API
    - Implement CSV/PDF export for filtered transactions
    - Display PayPal environment indicator (Sandbox/Live) banner
    - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.5, 12.6_

  - [x] 9.4 Implement admin dashboard overview
    - Build summary cards: subscriber count, MRR, today's tips status, recent signups, payment alerts
    - Build revenue and subscriber growth trend charts
    - Build recent activity feed (last 10 actions)
    - Add quick action shortcuts (create tips, manage plans, view subscribers)
    - Add onboarding cards for fresh install state
    - _Requirements: 43.1, 43.2, 43.3, 43.4, 43.5_

- [x] 10. Checkpoint — Phase 2 Complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Phase 3 — CMS: Page builder and content blocks
  - [x] 11.1 Implement CMS page builder backend
    - Build page CRUD API with JSON block structure persistence
    - Implement version snapshot creation on every save (author, timestamp, change summary)
    - Implement auto-save every 30 seconds with failure warning and 10-second retry
    - Implement publish endpoint (make content visible within 5 seconds)
    - Implement scheduled publishing (within 1 minute of configured time, timezone-aware)
    - Implement content expiry (auto-unpublish at specified time)
    - Build publishing queue view endpoint
    - _Requirements: 15.4, 15.6, 15.7, 15.10, 21.1, 21.2, 21.3, 21.4_

  - [x] 11.2 Implement CMS page builder frontend
    - Build visual editor with block palette, canvas area, and live preview panel
    - Implement drag-and-drop block insertion and reordering (update preview within 500ms)
    - Build block configuration panel per block type
    - Implement responsive preview toggles: desktop (≥1024px), tablet (768-1023px), mobile (<768px)
    - Implement undo/redo with full action history
    - Implement block deletion with preview update within 500ms
    - _Requirements: 15.1, 15.2, 15.3, 15.5, 15.8, 15.9_

  - [x] 11.3 Implement all CMS content block types
    - Build block components: Hero Section, Rich Text (WYSIWYG), Image, Image Gallery, Video Embed, CTA, Pricing Table (auto from plans or manual), Testimonials, FAQ Accordion, Stats Counter, Social Media Feed, Contact Form, Divider, HTML Embed, Blog Post List, Tip of the Day, Countdown Timer, Banner Alert
    - Build block configuration for Hero (background image, heading, subheading, CTA)
    - Build Rich Text block with WYSIWYG editor (formatting, links, embeds)
    - Build Pricing Table auto-generation from active plans
    - Build Contact Form with configurable fields and email delivery
    - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.5_

  - [x] 11.4 Implement content version history and rollback
    - Build version history list with timestamp, author, change summary, and preview
    - Implement rollback to any previous version (restore as current draft without deleting later versions)
    - Configure retention policy (default: keep all versions)
    - _Requirements: 20.1, 20.2, 20.3, 20.4_

  - [ ]* 11.5 Write property test for CMS version rollback round-trip
    - **Property 13: CMS Version Rollback Round-Trip**
    - **Validates: Requirements 20.1, 20.3, 20.4**

- [x] 12. Phase 3 — CMS: Media library and supporting features
  - [x] 12.1 Implement media library backend
    - Build upload endpoint: store in Azure Blob Storage, compress to 80% original size, convert to WebP, return CDN URL within 10 seconds
    - Support batch upload (up to 20 files) with parallel processing and per-file progress
    - Enforce file type limits: images (JPG, PNG, WebP, SVG, GIF, max 10MB), documents (PDF, DOCX, max 50MB), videos (MP4, max 500MB)
    - Reject invalid files with reason, preserve valid files in batch
    - Implement search by filename, tags, folder (results within 200ms)
    - Implement in-browser crop, resize, rotate with original file retention
    - Require alt text (1-125 chars) before upload completion for images
    - Prevent deletion of in-use assets with confirmation dialog listing referencing pages
    - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5, 17.6, 17.7, 17.8_

  - [x] 12.2 Implement navigation and menu management
    - Build visual tree editor for menu structure
    - Implement add/remove/reorder menu items via drag-and-drop
    - Support separate menus: header, footer, sidebar, mobile
    - Implement visibility rules by user role and subscription status
    - Apply navigation changes across frontend within 10 seconds of save
    - _Requirements: 18.1, 18.2, 18.3, 18.4, 18.5_

  - [x] 12.3 Implement SEO management
    - Build per-page SEO editor: meta title, description, OG image, URL slug, canonical URL
    - Display character counters for title (max 60) and description (max 160) with visual indicators
    - Auto-generate XML sitemap for published pages, update on publish/unpublish
    - Implement noindex meta tag rendering
    - Implement structured data (JSON-LD) for rich search results
    - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5_

  - [x] 12.4 Implement global site settings
    - Build settings: site name, tagline, logo (light + dark), favicon
    - Apply branding changes across all pages within 10 seconds
    - Implement maintenance mode (show "coming soon" to non-admin visitors)
    - Implement analytics script injection subject to cookie consent
    - Implement 301 redirect management
    - _Requirements: 22.1, 22.2, 22.3, 22.4_

  - [x] 12.5 Implement landing page
    - Build hero section with AI-generated artwork, headline, subheadline, CTA
    - Build animated pricing table auto-generated from active plans
    - Build public results section with performance statistics
    - Build testimonials carousel (CMS-managed)
    - Build "Tip of the Day" free preview for non-subscribers
    - Build FAQ section with search
    - Implement parallax scrolling and scroll-triggered animations
    - _Requirements: 42.1, 42.2, 42.3, 42.4, 42.5, 42.6, 42.7_

- [x] 13. Checkpoint — Phase 3 Complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 14. Phase 4 — Tips Engine
  - [x] 14.1 Implement tip creation and publishing API
    - Build tip CRUD with validation: event date (required), race name (1-200 chars), selection (1-200 chars), odds (1.01-1000.00), stake (1-10), category, commentary (max 5000 chars)
    - Reject invalid submissions with field-specific errors, preserve entered data
    - Implement publish endpoint (visible to subscribers within 5 seconds)
    - Implement scheduled publishing (minimum 1 minute in future, reject past dates)
    - Implement CSV bulk import (max 500 rows, 5MB): validate per-row, report errors with row number, persist valid rows only
    - Enforce status transitions: Draft → Published → Archived only (reject backward/skip)
    - _Requirements: 23.1, 23.2, 23.3, 23.4, 23.5, 23.6, 23.7_

  - [ ]* 14.2 Write property test for tip field validation
    - **Property 14: Tip Field Validation**
    - **Validates: Requirements 23.1, 23.2**

  - [ ]* 14.3 Write property test for tip status state machine
    - **Property 15: Tip Status State Machine**
    - **Validates: Requirements 23.7**

  - [x] 14.4 Implement tip categories
    - Create default categories: UK Horse Racing, Irish Horse Racing, Other Sports
    - Build category CRUD for admin (create new, assign to tips and plans)
    - Filter subscriber feed by plan-included categories
    - _Requirements: 24.1, 24.2, 24.3_

  - [x] 14.5 Implement result tracking and P&L calculation
    - Build result recording: Won, Lost, Void, Push
    - Calculate P&L: Won = (odds × stake) - stake, Lost = -stake, Void/Push = 0
    - Calculate aggregate P&L per day, week, month, year, and per category
    - Build searchable archive filterable by date range, category, and result
    - _Requirements: 25.1, 25.2, 25.3, 25.4, 25.5_

  - [ ]* 14.6 Write property test for P&L calculation correctness
    - **Property 16: P&L Calculation Correctness**
    - **Validates: Requirements 25.1, 25.2, 25.3, 25.4**

  - [x] 14.7 Implement content access gating
    - Grant access to tips by plan category for active subscribers
    - Show "Tip of the Day" free preview for Free Users
    - Display paywall with plan options for Guests on gated content
    - Restrict access for subscribers with revoked access (payment failure past grace period)
    - _Requirements: 26.1, 26.2, 26.3, 26.4_

  - [ ]* 14.8 Write property test for content access gating
    - **Property 17: Content Access Gating**
    - **Validates: Requirements 26.1, 26.2, 26.3, 26.4**

  - [x] 14.9 Implement blog system
    - Build blog post CRUD with title, rich text, featured image, SEO fields
    - Implement publish with URL slug access and sitemap inclusion
    - Build blog listing page sorted by publish date with featured images and excerpts
    - Support draft, published, and scheduled states
    - _Requirements: 27.1, 27.2, 27.3, 27.4_

- [x] 15. Checkpoint — Phase 4 Complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 16. Phase 5 — Social, Bot, and Engagement
  - [x] 16.1 Implement social media components
    - Build component library: Social Follow Bar, Share Buttons, Social Proof Counter, Twitter Feed Embed, Facebook Page Plugin, Instagram Gallery, Telegram Join Button, YouTube Latest Video, Testimonial Cards
    - Implement per-page visibility toggle for each component
    - Implement share dialog with pre-filled text and URL
    - Generate Open Graph meta tags for all public pages
    - Reflect social media URL changes across all pages within 10 seconds
    - _Requirements: 28.1, 28.2, 28.3, 28.4, 28.5_

  - [x] 16.2 Implement help bot widget
    - Build floating chat widget (bottom-right, expandable/collapsible with animation)
    - Display welcome message with quick-reply buttons
    - Implement keyword matching to conversation flows
    - Implement escalation to support ticket when unresolved
    - Build admin drag-and-drop conversation flow builder
    - Apply updated flows within 30 seconds
    - Persist conversation history per user session
    - _Requirements: 29.1, 29.2, 29.3, 29.4, 29.5, 29.6_

  - [x] 16.3 Implement notification system backend
    - Build multi-channel notification pipeline: email (SendGrid), web push, Telegram, in-app
    - Send tip publication alerts within 60 seconds on all enabled channels
    - Send renewal reminder 7 days before via email
    - Send payment failure alert via email and in-app within 5 minutes
    - Send tip result update within 60 seconds on enabled channels for category
    - Implement retry: up to 3 times with exponential backoff on delivery failure
    - Mark as failed and record in in-app list after all retries fail
    - Send admin broadcasts to all active subscribers within 5 minutes
    - _Requirements: 30.1, 30.2, 30.3, 30.6, 30.7, 30.8_

  - [x] 16.4 Implement notification preferences and frontend
    - Build preferences: per-channel toggles, per-category toggles, quiet hours
    - Apply preferences to all notifications (suppress disabled channels/categories, hold during quiet hours)
    - Build notification bell with unread count (display "99+" when >99)
    - Build dropdown listing 20 most recent notifications in reverse chronological order
    - _Requirements: 30.4, 30.5_

  - [ ]* 16.5 Write property test for notification preference filtering
    - **Property 18: Notification Preference Filtering**
    - **Validates: Requirements 30.4**

  - [x] 16.6 Implement Telegram bot integration
    - Implement subscriber linking via unique connection code (associate Telegram chat ID)
    - Deliver formatted tip messages within 30 seconds of publication
    - Implement unlinking with immediate notification stop
    - _Requirements: 31.1, 31.2, 31.3_

  - [x] 16.7 Implement referral program
    - Generate unique referral link per active subscriber
    - Credit referrer with configurable discount on next billing cycle when referee subscribes
    - Build referral dashboard: total clicks, conversions, earned rewards
    - Allow Super Admin to configure reward amounts and limits
    - _Requirements: 32.1, 32.2, 32.3, 32.4_

  - [x] 16.8 Implement comments and community features
    - Build comments under daily tips: display author name, avatar, timestamp
    - Implement moderator delete/hide (removed from view within 5 seconds)
    - Build polls: admin creates question with options, one vote per subscriber, real-time results
    - Implement subscriber-to-admin direct messaging
    - _Requirements: 33.1, 33.2, 33.3, 33.4, 33.5_

- [x] 17. Checkpoint — Phase 5 Complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 18. Phase 6 — Analytics and Compliance
  - [x] 18.1 Implement public performance analytics
    - Build public stats page: strike rate, ROI, monthly P&L from verified results
    - Build charts: profit over time, win rate trends, category comparison
    - Build last-30-days summary for landing page
    - Implement CSV/PDF export of displayed results
    - _Requirements: 34.1, 34.2, 34.3, 34.4_

  - [x] 18.2 Implement subscriber performance dashboard
    - Build personal P&L at level stakes for subscribed categories
    - Add filtering by category and date range
    - Display current winning/losing streak
    - Build monthly performance summaries with optional email digest
    - _Requirements: 35.1, 35.2, 35.3, 35.4_

  - [x] 18.3 Implement admin revenue analytics
    - Build combined PayPal + Stripe revenue unified view
    - Calculate MRR, churn rate, LTV per subscriber, revenue by plan
    - Build revenue trend charts: daily, weekly, monthly granularity
    - Implement subscriber growth forecasting
    - _Requirements: 36.1, 36.2, 36.3, 36.4_

  - [x] 18.4 Implement GDPR data subject rights
    - Build data export: generate JSON/CSV archive (profile, subscriptions, payments, tip access, comments, preferences, consent records), notify via email with 7-day download link, complete within 24 hours
    - Build account deletion: 30-day soft delete grace period, email confirmation with end date, permanent purge after 30 days
    - Allow deletion cancellation during grace period if user logs in
    - Support right to rectification (edit personal data, update within 5 seconds)
    - Maintain data processing records and consent timestamps (3-year retention)
    - Build breach notification: Super Admin sends email to affected users within 1 hour
    - _Requirements: 37.1, 37.2, 37.3, 37.4, 37.5, 37.6_

  - [x] 18.5 Implement cookie consent system
    - Build cookie consent banner: Accept All, Reject All, Customise options
    - Build granular toggles: Essential (always on), Analytics, Marketing, Preferences
    - Block non-essential scripts until consent given
    - Persist consent, re-prompt on policy change or after 12 months
    - Make consent auditable with timestamp and selected preferences
    - _Requirements: 38.1, 38.2, 38.3, 38.4, 38.5_

  - [x] 18.6 Implement AI-generated imagery integration
    - Build DALL-E API integration in Media Library for text-to-image generation
    - Store generated images in Azure Blob Storage as standard assets
    - Implement Unsplash and Pexels integration for stock photo search and import
    - _Requirements: 39.1, 39.2, 39.3_

  - [x] 18.7 Implement animations and micro-interactions
    - Build Angular route transition animations (300ms)
    - Implement card hover effects (scale + shadow lift, 150ms)
    - Implement scroll-triggered reveal animations via Intersection Observer
    - Implement number counter animation (zero to target over 1 second)
    - Implement confetti animation on successful subscription purchase
    - _Requirements: 40.1, 40.2, 40.3, 40.4, 40.5_

- [x] 19. Checkpoint — Phase 6 Complete
  - Ensure all tests pass, ask the user if questions arise.

- [x] 20. Phase 7 — PWA and Final Integration
  - [x] 20.1 Implement Progressive Web App
    - Register service worker caching app shell and previously loaded tip data
    - Configure branded splash screen and app icon for device installation
    - Serve cached tips offline with connectivity indicator
    - Sync queued actions and fetch updated content on connectivity restore
    - Implement mobile bottom navigation bar
    - _Requirements: 41.1, 41.2, 41.3, 41.4, 41.5_

  - [x] 20.2 Final integration and wiring
    - Verify all lazy-loaded modules load correctly from routes
    - Verify caching layer for plans, published tips, and CMS pages
    - Verify virtual scrolling for lists exceeding 100 items
    - Verify all static assets served via Azure CDN with 1-year cache headers
    - Verify initial bundle size below 250KB gzipped
    - Verify rate limiting on auth endpoints (100/min/IP) and general (1000/min/user)
    - Verify secrets stored in Azure Key Vault with no hardcoded credentials
    - Verify dependency vulnerability scanning for NuGet and npm
    - _Requirements: 46.2, 46.3, 46.4, 46.5, 47.2, 47.4, 47.8_

- [x] 21. Final Checkpoint — All tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation between phases
- Property tests validate universal correctness properties from the design document using FsCheck (.NET) and fast-check (TypeScript)
- Unit tests validate specific examples and edge cases
- Backend uses C# with .NET 10, EF Core, ASP.NET Core Identity
- Frontend uses TypeScript with Angular 20, NgRx, Tailwind CSS, DaisyUI
- All API errors use RFC 7807 ProblemDetails format
- Webhook processing is idempotent with event ID deduplication

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2", "1.3", "1.4"] },
    { "id": 1, "tasks": ["1.5", "1.6", "1.7"] },
    { "id": 2, "tasks": ["2.1", "2.11"] },
    { "id": 3, "tasks": ["2.2", "2.3", "2.7", "2.8"] },
    { "id": 4, "tasks": ["2.4", "2.5", "2.6", "2.9", "2.10", "2.12", "2.13"] },
    { "id": 5, "tasks": ["2.14"] },
    { "id": 6, "tasks": ["4.1", "4.2", "4.4"] },
    { "id": 7, "tasks": ["4.3"] },
    { "id": 8, "tasks": ["6.1", "6.4", "6.6"] },
    { "id": 9, "tasks": ["6.2", "6.3", "6.5"] },
    { "id": 10, "tasks": ["7.1", "8.1"] },
    { "id": 11, "tasks": ["7.2", "7.3", "8.2"] },
    { "id": 12, "tasks": ["9.1", "9.2", "9.3", "9.4"] },
    { "id": 13, "tasks": ["11.1", "12.1"] },
    { "id": 14, "tasks": ["11.2", "11.3", "12.2", "12.3", "12.4"] },
    { "id": 15, "tasks": ["11.4", "12.5"] },
    { "id": 16, "tasks": ["11.5"] },
    { "id": 17, "tasks": ["14.1", "14.4"] },
    { "id": 18, "tasks": ["14.2", "14.3", "14.5", "14.7", "14.9"] },
    { "id": 19, "tasks": ["14.6", "14.8"] },
    { "id": 20, "tasks": ["16.1", "16.2", "16.3", "16.6", "16.7", "16.8"] },
    { "id": 21, "tasks": ["16.4", "16.5"] },
    { "id": 22, "tasks": ["18.1", "18.2", "18.3", "18.4", "18.5", "18.6", "18.7"] },
    { "id": 23, "tasks": ["20.1", "20.2"] }
  ]
}
```
