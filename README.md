# AndyTipster V2

A complete rebuild of the AndyTipster horse racing tips subscription platform — modern tech stack, scalable architecture, and a rich feature set designed for growth.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10 Web API (Minimal APIs + Controllers) |
| Frontend | Angular 20 + Tailwind CSS v4 + DaisyUI v5 |
| State Management | NgRx 20 (Store, Effects, Entity, DevTools) |
| Database | SQL Server (Azure SQL) |
| ORM | Entity Framework Core |
| Auth | ASP.NET Core Identity + JWT + Social Login |
| Payments | PayPal + Stripe |
| CMS | Custom block-based page builder |
| Media Storage | Azure Blob Storage + Azure CDN |
| Hosting | Azure App Service |
| Monitoring | Serilog + Azure Application Insights |
| Email | SendGrid |
| Push | Web Push API (Service Worker) |
| Messaging | Telegram Bot API |

## Project Structure

```
src/
├── AndyTipster.Api/            # Presentation layer — controllers, minimal APIs, middleware
├── AndyTipster.Application/    # Application layer — services, validators, DTOs
├── AndyTipster.Domain/         # Domain layer — entities, enumerations, value objects
├── AndyTipster.Infrastructure/ # Infrastructure — EF Core, external service clients
├── andytipster-client/         # Angular 20 SPA (standalone components, NgRx)
└── AndyTipster.slnx            # Solution file
```

## Architecture

### Backend (Layered / Clean Architecture)

- **Minimal APIs** for simple CRUD endpoints (performance)
- **Controllers** for complex domain operations (organisation)
- **CQRS-lite** — read/write separation at the service layer
- **RFC 7807 ProblemDetails** for all error responses
- **Rate limiting** — 100 req/min/IP on auth, 1000 req/min/user on general endpoints
- **Security headers** — HSTS, CSP, X-Frame-Options, X-Content-Type-Options
- **Health checks** — database, PayPal API, Azure Blob Storage

### Frontend (Angular 20 + NgRx)

- **Standalone components** with OnPush change detection throughout
- **NgRx Store** — 6 feature slices (auth, user, roles, permissions, tips, plans)
- **Lazy-loaded routes** — public, auth, subscriber, admin
- **JWT interceptor** with token refresh queuing and replay
- **Route guards** — auth, role, permission, unauth
- **DaisyUI theming** — custom light/dark themes with system preference detection
- **Responsive** — mobile (375px), tablet (768px), desktop (1024px), wide (1280px)

### Shared Components

- **Data Table** — generic, reusable table with pagination, sorting, filtering, search (300ms debounce), row selection, bulk actions, CSV/Excel export, skeleton loading, empty/error states, mobile scroll
- **Skeleton Loader** — shimmer animation with configurable timeout and retry
- **Empty State** — illustration + message + CTA
- **Theme Toggle** — dark/light mode switch with local storage persistence

## Features (Planned)

- User registration, login, 2FA, social login (Google, Facebook, Apple)
- Multi-role authorization (Super Admin, Admin, Moderator, Subscriber, Free User, Guest)
- Subscription management with PayPal and Stripe
- Custom CMS with drag-and-drop page builder
- Horse racing tips engine with P&L tracking
- Multi-channel notifications (email, push, Telegram, in-app)
- GDPR compliance (data export, account deletion, cookie consent)
- Progressive Web App (offline access, installable)
- Performance analytics and public proof stats
- Help bot, referral program, comments, polls

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) and npm
- [Angular CLI](https://angular.dev/tools/cli) (`npm install -g @angular/cli`)
- SQL Server (LocalDB for development)

### Backend

```bash
cd src
dotnet restore
dotnet build
dotnet run --project AndyTipster.Api
```

The API runs on `https://localhost:5001` with health checks at `/health`.

### Frontend

```bash
cd src/andytipster-client
npm install
ng serve
```

The Angular app runs on `http://localhost:4200`.

### Run Tests

```bash
# Angular unit tests
cd src/andytipster-client
ng test --watch=false --browsers=ChromeHeadless

# .NET tests (when added)
cd src
dotnet test
```

## Build Phases

| Phase | Focus | Status |
|-------|-------|--------|
| 1 | Foundation — scaffolding, auth, roles, user management, UI system | 🟡 In Progress |
| 2 | Payments — PayPal, Stripe, checkout, plan builder | ⬜ Planned |
| 3 | CMS — page builder, media library, SEO, navigation | ⬜ Planned |
| 4 | Tips Engine — creation, categories, results, P&L, blog | ⬜ Planned |
| 5 | Engagement — social components, help bot, notifications, referrals | ⬜ Planned |
| 6 | Analytics & Compliance — dashboards, GDPR, cookies, AI imagery | ⬜ Planned |
| 7 | PWA & Launch — offline support, mobile optimisation, go-live | ⬜ Planned |

## License

See [LICENSE](LICENSE) for details.
