# AndyTipster V2 — Complete Platform Guide

## What Is AndyTipster?

AndyTipster is a subscription-based horse racing tips platform. A professional tipster publishes daily selections, paying subscribers receive them in real-time, and the platform tracks verified profit/loss transparently.

It is NOT a gambling site. It is a professional sports analysis service with subscription billing, content delivery, and performance tracking.

---

## How The Platform Works (End to End)

```
ADMIN publishes tips daily
    ↓
SUBSCRIBERS receive tips via web + email + Telegram
    ↓
After each race, ADMIN records result (Won/Lost/Void)
    ↓
Platform auto-calculates P&L (level stakes)
    ↓
PUBLIC site shows verified performance proof
    ↓
VISITORS see results → subscribe → become paying members
```

---

## User Roles & Their Complete Workflows

### Role 1: GUEST (Unauthenticated Visitor)

**What they see:**
A public marketing site designed to convert them into subscribers.

**Their journey:**

1. **Landing Page** — Hero section with brand messaging, animated statistics showing track record (strike rate, monthly profit, subscriber count), pricing table showing 3 plans with features
2. **Public Results** — Full verified P&L history. Charts showing profit over time, win rate by category. This builds trust — "here's proof this works"
3. **Blog** — Racing analysis articles (SEO content to attract organic traffic)
4. **Free Tip of the Day** — One tip visible without subscribing. Shows the quality of selections without giving everything away
5. **Registration** — Email + password + display name. Email verification required before login

**Conversion funnel:**
```
Visit landing → See results proof → Read blog → View free tip → Register → Verify email → Choose plan → Pay → Subscriber
```

---

### Role 2: FREE USER (Registered, No Subscription)

**What they see:**
A limited dashboard encouraging them to subscribe.

**Their workflow:**

1. **Login** → Lands on pricing page (cannot access tips)
2. **Pricing** — 3 plans displayed with feature comparison. Can apply promo codes (WELCOME20 = 20% off)
3. **Checkout** — Selects plan → Chooses payment method (PayPal or Card via Stripe) → Pays → Subscription activates immediately
4. **Profile** — Can edit display name, bio, avatar, timezone. Can configure notification preferences

**What they CANNOT do:**
- View daily tips
- See their P&L
- Access the tips feed

---

### Role 3: SUBSCRIBER (Paying Customer) ⭐

**What they see:**
The full platform experience — daily tips, performance tracking, and personal profit data.

**Their daily workflow:**

#### Morning (Tips Published)
1. **Open app** → "Today" view shows immediately
2. **Today's Tips** — Each tip displayed as a card:
   - Race name and time (e.g., "3:15 Cheltenham")
   - Selection (horse name)
   - Odds (e.g., 4/1)
   - Stake (1-10 points)
   - Category (UK Racing / Irish Racing / Other Sports)
   - Commentary (why this selection was chosen)
   - Status: ⏳ Pending (race hasn't happened yet)

3. **Notifications arrive** — Email + push notification + Telegram message (if configured):
   ```
   "New tip: 3:15 Cheltenham — Golden Arrow at 4/1 (2pt stake)"
   ```

#### After Races (Results Updated)
4. **Results update live** — Each tip card updates:
   - ✅ WON at 4/1 • +£7.00 profit
   - ❌ LOST • -£1.00
   - ⬜ VOID (race cancelled/non-runner)

5. **Daily P&L banner** — "Today's P&L: +£18.50" displayed prominently

6. **Previous Days** — Collapsible sections showing last 7 days:
   - "Yesterday: 2 from 3 • +£12.00"
   - "Monday: 3 from 4 • +£28.50"

#### Weekly/Monthly Review
7. **My Performance page** — Full personal P&L breakdown:
   - Current streak (🔥 7W = seven consecutive winners)
   - This month's profit (e.g., +£132)
   - Best ever month (e.g., +£412 in March)
   - All-time ROI percentage
   - Strike rate (percentage of winners)
   - Monthly performance table (Jan-Dec with P&L per month)
   - Category breakdown (which racing types are most profitable)

8. **Personal Profit Tracker** (always visible at top):
   ```
   "Total tips: 84 • Total profit: +£890 • Win rate: 62% • Current streak: 🔥 7W"
   ```

#### Billing Management
9. **Billing page** shows:
   - Current plan name and price
   - Next billing date
   - Payment method (PayPal or Stripe card)
   - Payment history (every payment with date, amount, status)
   - Upgrade/downgrade options
   - Cancel button (keeps access until period end)

#### Other Features
10. **Referral Program** — Unique referral link. When a friend subscribes using the link, the referrer gets a discount on their next bill
11. **Profile Settings** — 6 tabs: Profile, Security (2FA), Notifications (channel preferences), Billing, Privacy (GDPR), Appearance (dark mode)
12. **Notification Bell** — Shows unread count, lists last 20 notifications

---

### Role 4: ADMIN (The Tipster / Business Owner) ⭐⭐

**What they see:**
Full business management platform — publish tips, manage subscribers, track revenue.

**Their daily workflow:**

#### Publishing Tips (The Core Job)

**Quick Tip Mode (10 seconds):**
1. Open admin panel → "Today" page shows Quick Add panel at top
2. Type: Race name, Selection, Odds → Click "Publish Now"
3. Tip is LIVE immediately. All subscribers get notified.

**Full Tip Mode (for commentary):**
1. Click "Create Tip" → Modal opens
2. Fill: Event Date, Race Name, Selection, Odds, Stake (1-10), Category, Commentary
3. Click "Create" → Saved as Draft
4. Click "Publish" → Goes live to subscribers

**CSV Bulk Import (for busy days):**
1. Click "CSV Import" → Upload a file with up to 500 tips
2. System validates each row → Shows errors for invalid data
3. Valid tips saved as Draft → Publish individually or batch publish

#### Recording Results (After Each Race)

1. Go to Tip Management → Find the published tip
2. Click "Record Result" → Choose: Won / Lost / Void / Push
3. System auto-calculates P&L:
   - Won: profit = (odds × stake) - stake
   - Lost: profit = -stake
   - Void/Push: profit = 0
4. Running totals update across all dashboards

#### Business Dashboard

Admin sees at a glance:
- **Total Subscribers** — How many paying members
- **MRR** (Monthly Recurring Revenue) — Current monthly income
- **Tips Published Today** — How many selections went out
- **Recent Signups** — New members this week
- **Payment Alerts** — Failed payments needing attention
- **Revenue Trend** — Chart showing income over past 12 months
- **Activity Feed** — Last 10 admin actions

#### Managing Subscribers (Members)

1. **Search/filter** — Find any user by name, email, role, plan, status
2. **View details** — See their subscription, payment history, activity
3. **Suspend** — Block access immediately (revokes all tokens)
4. **Impersonate** — View the site exactly as that user sees it (for support)
5. **Bulk actions** — Suspend multiple users, change roles, export to CSV

#### Managing Subscriptions (Packages)

1. **Create Plans** — Set name, price, currency, billing cycle, features, trial period
2. **Archive Plans** — Hide from pricing page (existing subscribers keep access)
3. **Promo Codes** — Create discount codes (percentage or fixed amount, expiry date, max uses)
4. **PayPal Sync** — Plans auto-sync to PayPal Billing Plans API

#### Content Management (CMS)

1. **Page Builder** — Drag-and-drop blocks (Hero, Rich Text, FAQ, Pricing Table, etc.)
2. **Blog** — Create racing analysis posts (title, rich text, featured image, SEO fields)
3. **Media Library** — Upload images, manage assets
4. **Navigation** — Edit site menu items (header, footer, mobile)

#### Revenue Analytics

- Combined PayPal + Stripe revenue view
- MRR, ARR, churn rate, lifetime value per subscriber
- Revenue trend charts (daily/weekly/monthly)
- Revenue breakdown by plan

#### Audit Trail

- Every admin action logged (who did what, when, before/after values)
- Searchable, filterable, 2-year retention
- Cannot be edited or deleted (compliance)

---

## Payment System — How It Actually Works

### Overview

The platform supports TWO payment gateways:
- **PayPal** — For users who prefer PayPal balance, bank transfer, or PayPal Credit
- **Stripe** — For credit/debit card payments

Both handle recurring subscriptions. The platform NEVER stores card details — all card data stays with PayPal/Stripe (PCI DSS compliant).

### Payment Flow (Step by Step)

#### Subscriber Checkout:

```
1. User selects a plan (e.g., Monthly Premium £19.99)
2. User selects payment method: PayPal or Card
3. IF PayPal:
   a. Platform calls PayPal Subscriptions API → creates subscription
   b. User is redirected to PayPal to approve
   c. User approves on PayPal → returns to our site
   d. Platform activates subscription → grants access
   
4. IF Stripe (Card):
   a. Platform displays Stripe Hosted Payment Fields (card form)
   b. User enters card details (stays on our site, but form is Stripe's)
   c. Stripe processes payment → confirms success
   d. Platform activates subscription → grants access
```

#### Recurring Billing (Automatic):

```
Every billing cycle (monthly/quarterly/annually):
1. PayPal/Stripe automatically charges the customer
2. They send our server a WEBHOOK event:
   - "payment succeeded" → we record the transaction
   - "payment failed" → we mark subscription as "past due"
3. If payment fails:
   - Grace period starts (configurable, default 7 days)
   - User gets email + in-app notification to update payment
   - If not resolved within grace period → access revoked
```

#### Webhook Processing (How We Stay In Sync):

```
PayPal/Stripe → POST to our webhook endpoint
    ↓
Step 1: Verify signature (prevent fake webhooks)
Step 2: Check idempotency (prevent duplicate processing)
Step 3: Process event:
    - PAYMENT.COMPLETED → record transaction, keep access active
    - PAYMENT.FAILED → mark as past-due, notify user, start grace period
    - SUBSCRIPTION.CANCELLED → revoke access at period end
    - SUBSCRIPTION.ACTIVATED → grant access immediately
Step 4: Return 200 OK within 10 seconds
```

#### Admin Manages Payments:

1. **PayPal Dashboard** — View all transactions in-app (no need to login to PayPal separately)
2. **Revenue Analytics** — MRR, ARR, churn rate calculated from payment data
3. **Process Refunds** — One-click refund via PayPal API
4. **Export** — Download transaction history as CSV/PDF
5. **Sandbox/Live Toggle** — Test mode for development (no real charges)

### Plan → Payment Gateway Mapping:

```
When admin CREATES a plan in our app:
1. Plan saved to our database (name, price, currency, billing cycle)
2. Platform calls PayPal Billing Plans API → creates matching plan on PayPal
3. PayPal returns a Plan ID → we store it
4. When user subscribes, we use that Plan ID to create the subscription

Same for Stripe:
1. Plan saved to our database
2. We create a Stripe Price object → store the Price ID
3. When user subscribes with card, we use that Price ID
```

### What Happens When User Cancels:

```
1. User clicks "Cancel Subscription" on billing page
2. Platform calls PayPal/Stripe API to cancel the subscription
3. Subscription status changes to "Cancelled"
4. User KEEPS ACCESS until the end of their current paid period
5. After period ends → access automatically revoked
6. User can re-subscribe anytime
```

### What Happens When Payment Fails:

```
1. PayPal/Stripe webhook: "payment failed"
2. Platform marks subscription as "Past Due"
3. Email sent: "Your payment failed. Please update your payment method."
4. In-app notification shown
5. Grace period starts (e.g., 7 days)
6. If user updates payment method → retry succeeds → back to active
7. If grace period expires → access revoked
8. Admin can see all failed payments in PayPal Dashboard
```

---

## Notification System — How Users Get Updates

### Channels:
1. **In-App** — Notification bell with unread count (always)
2. **Email** — Via SendGrid (configurable per user)
3. **Web Push** — Browser notifications (configurable per user)
4. **Telegram** — Bot messages to linked Telegram account

### What Triggers Notifications:

| Event | Channels | Timing |
|-------|----------|--------|
| New tip published | All enabled channels | Within 60 seconds |
| Tip result updated (Won/Lost) | All enabled channels | Within 60 seconds |
| Payment failed | Email + In-app | Within 5 minutes |
| Renewal reminder | Email | 7 days before billing |
| Admin broadcast | All channels | Within 5 minutes |
| Welcome message | Email | On registration |

### User Controls:
- Per-channel toggles (enable/disable email, push, Telegram independently)
- Per-category toggles (only get notified for UK Racing, not Irish)
- Quiet hours (no notifications between 10pm-7am, held until morning)

---

## Security Features

| Feature | How It Works |
|---------|-------------|
| Password Policy | Min 8 chars, uppercase, lowercase, digit, special char |
| Email Verification | Must verify email before login (24-hour expiry link) |
| Two-Factor Auth | TOTP via authenticator app, 8 recovery codes |
| Account Lockout | 5 failed attempts → 15-minute lock |
| JWT Tokens | 15-minute access token + 7-day refresh token with rotation |
| Rate Limiting | 100 req/min on auth, 1000 req/min general |
| HTTPS Only | All traffic encrypted |
| Security Headers | CSP, HSTS, X-Frame-Options, X-Content-Type-Options |
| Audit Trail | Every admin action logged with before/after values |
| GDPR | Data export, 30-day account deletion, cookie consent |

---

## Technical Architecture (Simple Version)

```
USER'S BROWSER (Angular app)
    ↕ REST API + JWT
OUR SERVER (.NET Web API on Azure)
    ↕
OUR DATABASE (SQL Server on Azure)
    ↕
PAYMENT PROVIDERS (PayPal API + Stripe API)
    ↕ Webhooks
OUR SERVER (processes payment events)
```

**Key principle:** The platform works WITHOUT any external dependencies except payment processing. All tips, results, analytics, content, and user management runs entirely on our own infrastructure.

---

## Pricing Model

| Plan | Price | Billing | What's Included |
|------|-------|---------|----------------|
| Monthly Premium | £19.99/mo | Monthly | UK Racing tips, P&L tracking, email notifications, mobile access |
| Quarterly Value | £49.99/qtr | Quarterly | UK + Irish Racing, all notification channels, priority support |
| Annual Gold | £149.99/yr | Annual | All categories, all channels, Telegram delivery, 7-day free trial |

All plans include:
- Full platform access during subscription
- Verified P&L tracking
- Cancel anytime (keep access until period end)
- Money-back guarantee

---

## What Makes This Platform Different

1. **Full Transparency** — Every tip is recorded with a timestamp. P&L is publicly verifiable. No hiding losses.
2. **Self-Contained** — No dependency on third-party data providers. The tipster controls everything.
3. **Professional Tool** — Not a gambling site. It's a subscription business platform that happens to deliver racing tips.
4. **Multi-Channel Delivery** — Web, email, push, Telegram. Users choose how they get tips.
5. **Admin Efficiency** — Publish a tip in 10 seconds. Record results in 2 clicks. The platform does the rest.
6. **Subscriber Retention** — Personal profit tracking, streaks, and monthly performance data make the value tangible.
