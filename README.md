# 🏇 AndyTipster V2 — Horse Racing Tips Subscription Platform

> A production-grade SaaS platform delivering premium horse racing tips to subscribers via a modern, full-stack architecture. Built with .NET 10, Angular 20, and deployed on Azure.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular 20](https://img.shields.io/badge/Angular-20-DD0031?logo=angular)](https://angular.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.8-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-4.0-06B6D4?logo=tailwindcss)](https://tailwindcss.com/)
[![Azure](https://img.shields.io/badge/Azure-Cloud-0078D4?logo=microsoftazure)](https://azure.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## What Is AndyTipster?

AndyTipster is a **complete subscription business platform** for horse racing tipsters. It handles everything from user registration and payment processing to tip delivery and performance analytics — giving tipsters the tools to run a professional, profitable operation.

**Key Capabilities:**
- 💳 Dual payment gateway (PayPal + Stripe) with subscription management
- 📊 Automated P&L tracking with verified public performance stats
- 📱 Progressive Web App — installable, works offline, push notifications
- 🎨 Custom CMS with drag-and-drop page builder (18 block types)
- 🤖 Help bot, Telegram delivery, multi-channel notifications
- 🔒 Enterprise-grade auth: JWT, 2FA, RBAC with 6 roles and 27 permissions
- 📈 Revenue analytics, subscriber dashboards, referral program

---

## Screenshots & Demo

| Landing Page | Admin Dashboard | Tips Feed |
|:---:|:---:|:---:|
| Hero + pricing + social proof | MRR, subscribers, activity | Category filtering + P&L |

**Demo Credentials:**
| Role | Email | Password |
|------|-------|----------|
| Super Admin | `admin@andytipster.com` | `Admin123!` |
| Subscriber | `subscriber@test.com` | `Test123!` |
| Free User | `free@test.com` | `Test123!` |

---

## Tech Stack

| Layer | Technology | Why |
|-------|-----------|-----|
| **Backend** | .NET 10 Web API | Performance, type safety, mature ecosystem |
| **Frontend** | Angular 20 + NgRx | Enterprise SPA with predictable state |
| **UI** | Tailwind CSS 4 + DaisyUI 5 | Rapid, consistent, dark mode built-in |
| **Database** | SQL Server (Azure SQL) | Relational integrity, EF Core ORM |
| **Auth** | ASP.NET Core Identity + JWT | Industry standard, battle-tested |
| **Payments** | PayPal + Stripe | Maximum reach, webhook-driven |
| **CMS** | Custom block-based editor | No third-party CMS dependency |
| **Storage** | Azure Blob + CDN | Global asset delivery |
| **Monitoring** | Serilog + Application Insights | Structured logging, alerts |
| **Notifications** | SendGrid + Web Push + Telegram | Multi-channel delivery |
| **PWA** | Angular Service Worker | Offline access, installable |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Angular 20 SPA (PWA)                        │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────────┐   │
│  │  Public  │ │   Auth   │ │Subscriber│ │      Admin       │   │
│  │  Module  │ │  Module  │ │  Module  │ │     Module       │   │
│  └──────────┘ └──────────┘ └──────────┘ └──────────────────┘   │
│  ┌─────────────────── NgRx Store ───────────────────────────┐   │
│  │ Auth │ User │ Roles │ Permissions │ Tips │ Plans          │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                            │ REST + JWT
┌─────────────────────────────────────────────────────────────────┐
│                    .NET 10 Web API                              │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Controllers │ Minimal APIs │ Middleware │ Health Checks  │   │
│  └─────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │        Application Layer (Services, DTOs, Validators)    │   │
│  └─────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │     Domain Layer (Entities, Enumerations, Events)        │   │
│  └─────────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │  Infrastructure (EF Core, Azure, PayPal, Stripe, Email)  │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
          │              │              │              │
    ┌─────┴─────┐ ┌─────┴─────┐ ┌─────┴─────┐ ┌─────┴─────┐
    │ Azure SQL │ │ Blob + CDN│ │PayPal API │ │Stripe API │
    └───────────┘ └───────────┘ └───────────┘ └───────────┘
```

---

## Features

### 🔐 Authentication & Authorization
- Email/password registration with verification
- Social login (Google, Facebook, Apple)
- Two-factor authentication (TOTP + 8 recovery codes)
- JWT with 15-min access + 7-day refresh token rotation
- Account lockout (5 failed attempts → 15-min lock)
- 6 roles: Super Admin, Admin, Moderator, Subscriber, Free User, Guest
- 27 granular permissions with role hierarchy enforcement

### 💳 Payments & Subscriptions
- PayPal Smart Buttons + Stripe hosted fields
- Flexible plan builder (weekly to annual billing)
- Free trials (1-365 days), setup fees, promo codes
- Idempotent webhook processing with signature verification
- Subscription self-service (upgrade, downgrade, pause, cancel)
- In-app PayPal admin dashboard with MRR/ARR/churn analytics

### 🏇 Tips Engine
- Create, schedule, and publish racing tips
- CSV bulk import (500 rows per batch)
- Status workflow: Draft → Published → Archived
- Result tracking: Won, Lost, Void, Push
- Automatic P&L calculation (level stakes methodology)
- Content access gating by subscription plan + category

### 📝 Content Management System
- Drag-and-drop page builder with live preview
- 18 content block types (Hero, Rich Text, Pricing Table, FAQ, etc.)
- Version history with one-click rollback
- Scheduled publishing with timezone awareness
- Media library with upload, crop, resize, CDN delivery
- SEO management (meta tags, sitemap, JSON-LD structured data)

### 📱 Progressive Web App
- Installable on mobile home screen
- Offline access to cached tips and plans
- Service worker with smart caching strategies
- Mobile bottom navigation bar
- Offline action queue with auto-sync

### 🔔 Notifications
- Multi-channel: Email, Web Push, Telegram, In-app
- Per-user preferences with category/channel toggles and quiet hours
- Retry with exponential backoff (3 attempts)
- Notification bell with unread count (99+ cap)

### 📊 Analytics
- **Public**: Strike rate, ROI, monthly P&L charts
- **Subscriber**: Personal P&L, winning streak, performance summaries
- **Admin**: Combined revenue (PayPal + Stripe), MRR, churn, LTV, forecasting

### 🤝 Community & Engagement
- Comments on daily tips with moderation tools
- Polls with real-time results
- Referral program with tracking and configurable rewards
- Social media components (follow bar, share buttons, proof counter)
- Help bot widget with conversation flows and escalation

### 🛡️ Compliance & Security
- GDPR: Data export, 30-day account deletion, breach notification
- Cookie consent with granular category toggles
- HTTPS enforcement, rate limiting, security headers (CSP, HSTS)
- Append-only audit log with 2-year retention
- PCI DSS compliance via hosted payment fields

---

## Project Structure

```
src/
├── AndyTipster.slnx                     # .NET solution
├── AndyTipster.Api/                     # API endpoints, middleware, controllers
├── AndyTipster.Application/             # DTOs, service interfaces, validators
├── AndyTipster.Domain/                  # Entities, enumerations, domain events
├── AndyTipster.Infrastructure/          # EF Core, external services, seeding
└── andytipster-client/                  # Angular 20 SPA
    └── src/app/
        ├── core/                        # Guards, interceptors, services
        ├── shared/                      # DataTable, pipes, directives, components
        ├── store/                       # NgRx (auth, user, roles, permissions, tips, plans)
        └── features/
            ├── public/                  # Landing, pricing, blog, FAQ
            ├── auth/                    # Login, register, 2FA, forgot password
            ├── subscriber/             # Tips, results, billing, checkout, profile
            └── admin/                   # Users, plans, tips, CMS, analytics, audit
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) (LTS)
- [Angular CLI 20](https://angular.dev/) (`npm i -g @angular/cli`)
- SQL Server (LocalDB works for development)

### Quick Start

```bash
# Clone the repo
git clone https://github.com/dotnetdeveloper20xx/AndyTipsterV2.git
cd AndyTipsterV2

# Start the backend
cd src
dotnet restore
dotnet run --project AndyTipster.Api
# API runs at https://localhost:7001 — seed data loads automatically

# Start the frontend (new terminal)
cd src/andytipster-client
npm install
ng serve
# App runs at http://localhost:4200
```

### Configuration

Key settings in `src/AndyTipster.Api/appsettings.json`:

| Setting | Description |
|---------|-------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection |
| `Jwt:Key` | JWT signing key (min 32 chars) |
| `PayPal:ClientId` / `ClientSecret` | PayPal API credentials |
| `Stripe:SecretKey` / `WebhookSecret` | Stripe API credentials |
| `Authentication:Google:ClientId` | Google OAuth |
| `ApplicationInsights:ConnectionString` | Azure monitoring (optional) |

---

## API Endpoints

| Module | Endpoints | Auth |
|--------|-----------|------|
| Auth | `/api/auth/register`, `login`, `refresh`, `2fa/*`, `social-login` | Public |
| Users | `/api/users`, `/api/users/{id}/impersonate`, `/api/users/export` | Admin |
| Roles | `/api/roles`, `/api/roles/assign`, `/api/roles/permissions` | Admin |
| Profile | `/api/profile`, `/api/profile/avatar`, `/api/profile/activity` | User |
| Plans | `/api/plans`, `/api/promo-codes` | Admin/Public |
| Subscriptions | `/api/subscriptions`, `/api/checkout` | User |
| Webhooks | `/api/webhooks/paypal`, `/api/webhooks/stripe` | Signature |
| Tips | `/api/tips`, `/api/tips/feed`, `/api/tips/{id}/result` | Mixed |
| Categories | `/api/categories` | Public |
| CMS | `/api/cms/pages`, `/api/media`, `/api/navigation`, `/api/seo` | Admin |
| Blog | `/api/blog` | Public/Admin |
| Notifications | `/api/notifications`, `/api/notifications/preferences` | User |
| Analytics | `/api/analytics/revenue`, `performance`, `public-stats` | Mixed |
| GDPR | `/api/gdpr/export`, `/api/gdpr/delete-account` | User |
| Audit | `/api/audit` | Super Admin |

---

## Design Principles

- **Clean Architecture** — Domain at the center, no framework dependencies leak inward
- **CQRS-lite** — Read/write separation at the service layer
- **Event-driven webhooks** — Idempotent, signature-verified, dead-letter handling
- **OnPush everything** — Angular components use OnPush change detection
- **Signals + NgRx** — Reactive state management with predictable data flow
- **Mobile-first** — Responsive design, PWA, bottom nav, offline support
- **Security by default** — Rate limiting, CSP, HSTS, parameterized queries, no secrets in code
- **Accessibility** — WCAG AA contrast, ARIA labels, keyboard navigation, axe-core testing

---

## Testing

```bash
# Backend build
cd src && dotnet build

# Frontend build
cd src/andytipster-client && ng build

# Frontend unit tests (76 specs)
ng test --watch=false --browsers=ChromeHeadless

# Accessibility tests
npm run test:a11y
```

---

## Deployment

Designed for Azure App Service with:
- Azure SQL (geo-redundant backups)
- Azure Blob Storage + CDN for media
- Azure Application Insights for monitoring
- Azure Key Vault for secrets

---

## Roadmap

- [ ] CI/CD pipeline (GitHub Actions → Azure)
- [ ] E2E tests with Playwright
- [ ] Real-time WebSocket for live odds/results
- [ ] Mobile app (Capacitor or .NET MAUI)
- [ ] Multi-language support (i18n)
- [ ] AI-powered tip analysis

---

## License

This project is licensed under the MIT License — see [LICENSE](LICENSE) for details.

---

<p align="center">
  Built with ❤️ for the horse racing community
</p>
https://dotnetdeveloper.co.uk
