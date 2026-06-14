# Requirements Document

## Introduction

AndyTipster V2 is a complete rebuild of the horse racing tips subscription platform. The system provides authenticated users with daily horse racing tips, manages subscriptions via PayPal and Stripe, delivers content through a custom CMS, and offers comprehensive admin tools for managing the platform. The platform is built on .NET 10 Web API with Angular 20 frontend, deployed on Azure, and targets UK/Ireland horse racing enthusiasts.

## Glossary

- **Platform**: The AndyTipster V2 web application encompassing API, frontend, and all supporting services
- **API**: The .NET 10 Web API backend service handling all business logic and data access
- **Frontend**: The Angular 20 single-page application serving the user interface
- **Auth_Service**: The authentication and authorization subsystem using ASP.NET Core Identity and JWT tokens
- **User_Manager**: The admin module for managing user accounts, roles, and permissions
- **Plan_Builder**: The admin interface for creating and configuring subscription plans
- **Payment_Gateway**: The integrated PayPal and Stripe payment processing subsystem
- **PayPal_Service**: The PayPal integration subsystem handling subscriptions, payments, and webhooks
- **Stripe_Service**: The Stripe integration subsystem handling card payments and subscriptions
- **CMS**: The custom block-based content management system for all editable site content
- **Page_Builder**: The visual drag-and-drop page editor within the CMS
- **Media_Library**: The centralised asset management system using Azure Blob Storage and CDN
- **Tips_Engine**: The subsystem for creating, scheduling, publishing, and tracking horse racing tips
- **Analytics_Dashboard**: The reporting module showing performance metrics, revenue, and user statistics
- **Notification_Service**: The multi-channel notification system supporting email, push, Telegram, and in-app alerts
- **Help_Bot**: The automated chatbot widget providing customer support via conversation flows
- **Social_Components**: The reusable Angular component library for social media integration
- **GDPR_Module**: The compliance subsystem handling cookie consent, data export, and account deletion
- **PWA_Shell**: The Progressive Web App wrapper enabling offline access and mobile installation
- **NgRx_Store**: The Angular state management layer using NgRx Store, Effects, Selectors, and Entity
- **Data_Table**: The shared generic data table component used across all list views
- **Admin_Panel**: The administrative interface accessible to Super Admin, Admin, and Moderator roles
- **Subscriber**: A paying user with an active subscription granting access to tips
- **Free_User**: A registered user without an active subscription
- **Guest**: An unauthenticated visitor viewing public pages only

## Requirements

---

### Requirement 1: User Registration and Authentication

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a Guest, I want to register and authenticate with the Platform, so that I can access personalised features and subscription content.

#### Acceptance Criteria

1. WHEN a Guest submits a registration form with a valid email address and a password that meets complexity requirements, THE Auth_Service SHALL create a new user account and send a verification email within 5 seconds
2. WHEN a Guest submits a registration form with a password shorter than 8 characters, or missing at least one uppercase letter, one lowercase letter, one digit, or one special character, THE Auth_Service SHALL reject the registration and return a validation error indicating which specific requirements were not met
3. WHEN a Guest submits a registration form with an email address that is already associated with an existing account, THE Auth_Service SHALL reject the registration and return an error indicating the email is already in use
4. WHEN a registered user clicks the verification link in the email within 24 hours of registration, THE Auth_Service SHALL mark the account as verified and redirect the user to the login page
5. IF a verification link has expired or has already been used, THEN THE Auth_Service SHALL reject the verification attempt and provide an option to request a new verification email
6. WHEN an unverified user attempts to log in, THE Auth_Service SHALL reject the login and display a message instructing the user to verify their email
7. WHEN a verified user submits valid credentials, THE Auth_Service SHALL issue a JWT access token with 15-minute expiry and a refresh token with 7-day expiry
8. WHEN a user selects a social login provider (Google, Facebook, or Apple), THE Auth_Service SHALL authenticate via OAuth2 and create a new account if no account exists for that email, or link the social provider to the existing account matching that email address
9. WHEN an access token expires and a valid refresh token is presented, THE Auth_Service SHALL issue a new access token and rotate the refresh token
10. WHEN a user submits 5 consecutive failed login attempts within a 30-minute window, THE Auth_Service SHALL lock the account for 15 minutes and notify the user via email
11. WHEN a user requests a password reset, THE Auth_Service SHALL send a reset email with a single-use token that expires in 1 hour
12. IF a password reset token has expired or has already been used, THEN THE Auth_Service SHALL reject the reset attempt and instruct the user to request a new token

---

### Requirement 2: Two-Factor Authentication

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a registered user, I want to enable two-factor authentication, so that my account has an additional layer of security.

#### Acceptance Criteria

1. WHEN a user enables two-factor authentication, THE Auth_Service SHALL generate a TOTP secret, display a QR code for authenticator app registration, and require the user to submit a valid TOTP code to confirm setup before activating 2FA on the account
2. WHEN a user with 2FA enabled submits valid credentials, THE Auth_Service SHALL prompt for a TOTP code before issuing tokens, accepting codes valid within a 30-second time step with one-step clock skew tolerance
3. WHEN a user provides an invalid TOTP code, THE Auth_Service SHALL reject the login attempt and increment the failed attempt counter, and IF 5 consecutive invalid TOTP codes are submitted, THEN THE Auth_Service SHALL lock the account for 15 minutes
4. WHEN a user disables two-factor authentication after confirming their password, THE Auth_Service SHALL remove the TOTP secret and revert to single-factor login
5. WHEN a user successfully enables two-factor authentication, THE Auth_Service SHALL generate a set of 8 single-use recovery codes and display them once for the user to store securely
6. WHEN a user with 2FA enabled submits a valid unused recovery code instead of a TOTP code, THE Auth_Service SHALL authenticate the login, mark that recovery code as consumed, and prompt the user to re-register their authenticator app if fewer than 2 recovery codes remain

---

### Requirement 3: Multi-Role Authorization System

**Priority:** P0 (MVP — Phase 1)

**User Story:** As an administrator, I want to assign roles with granular permissions to users, so that I can control access to platform features based on responsibility.

#### Acceptance Criteria

1. THE Platform SHALL support six base roles in descending privilege order: Super Admin, Admin, Moderator, Subscriber, Free User, and Guest, where each role is assignable to users and enforced across both API and Frontend
2. WHEN a Super Admin creates a custom role by providing a unique role name and selecting one or more permissions from the platform's defined permission set, THE User_Manager SHALL persist the role and make it available for assignment within 2 seconds
3. WHEN a user with a specific role accesses the Frontend, THE Frontend SHALL hide navigation items and features not permitted by that role, displaying only those the user is authorized to access
4. WHEN an unauthenticated request reaches a protected API endpoint, THE API SHALL return HTTP 401 Unauthorized
5. WHEN an authenticated user without the required permission accesses a restricted endpoint, THE API SHALL return HTTP 403 Forbidden
6. WHEN an Admin assigns a role to a user, THE User_Manager SHALL log the change in the audit trail with timestamp, actor, and target user
7. THE Platform SHALL enforce role hierarchy in the fixed order Super Admin > Admin > Moderator > Subscriber > Free User > Guest, so that a user may only assign or modify roles at a level strictly below their own role level, and custom roles SHALL be positioned at the hierarchy level specified during creation
8. WHEN a new user registers on the Platform, THE User_Manager SHALL assign the Free User role by default
9. IF an Admin attempts to delete a role that is currently assigned to one or more users, THEN THE User_Manager SHALL reject the deletion and return an error indicating the role is still in use along with the count of affected users
10. IF a user is assigned multiple roles, THEN THE Platform SHALL grant the union of all permissions from the assigned roles, and the user's effective hierarchy level SHALL be that of their highest-privilege role

---

### Requirement 4: User Management Admin Panel

**Priority:** P0 (MVP — Phase 1)

**User Story:** As an Admin, I want to search, filter, and manage all user accounts from a centralised panel, so that I can efficiently handle user support and administration.

#### Acceptance Criteria

1. WHEN an Admin navigates to the user management page, THE User_Manager SHALL display a paginated table of all users with columns for name, email, role, status, plan, and registration date, with a default page size of 25 rows and default sort by registration date descending
2. WHEN an Admin applies search or filter criteria, THE User_Manager SHALL support searching by name and email, and filtering by role, plan, status, registration date range, and last login date range, returning matching results within 200ms for datasets up to 100,000 users
3. WHEN an Admin selects the impersonate action on a user, THE Platform SHALL create a read-only session that renders the site as that user sees it, with a persistent banner displaying the impersonated user's name and a button to end impersonation
4. WHEN an Admin clicks the end-impersonation control, THE Platform SHALL terminate the impersonated session and return the Admin to the user management page within 2 seconds
5. WHEN an Admin performs a bulk action (suspend, role change, or export) on selected users, THE User_Manager SHALL display a confirmation prompt listing the selected user count and action type, and apply the action only after the Admin confirms
6. IF one or more records fail during a bulk action, THEN THE User_Manager SHALL complete processing of all remaining records, then display a summary indicating the count of successful and failed operations with the reason for each failure
7. WHEN an Admin exports users, THE User_Manager SHALL generate a CSV or Excel file containing name, email, role, status, plan, and registration date columns for the currently filtered dataset and initiate a download within 30 seconds for up to 100,000 records
8. WHEN an Admin suspends a user account, THE Auth_Service SHALL revoke all active sessions and tokens for that user within 5 seconds of the suspension action

---

### Requirement 5: User Profile Management

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a registered user, I want to manage my profile information and preferences, so that I can personalise my experience on the Platform.

#### Acceptance Criteria

1. WHEN a user uploads an avatar image of type JPG, PNG, WebP, or GIF not exceeding 5 MB, THE Platform SHALL allow the user to crop the image, resize it to 256x256 pixels, and store it in the Media_Library
2. IF a user uploads an avatar image that is not of type JPG, PNG, WebP, or GIF, or exceeds 5 MB, THEN THE Platform SHALL reject the upload and display an error message indicating the accepted file types and maximum file size
3. WHEN a user updates their display name (3–50 characters), bio (maximum 500 characters), or timezone, THE Platform SHALL persist the changes and reflect them across the application within 2 seconds
4. IF a user submits a display name shorter than 3 characters or longer than 50 characters, or a bio longer than 500 characters, THEN THE Platform SHALL reject the update and display a validation error message indicating the allowed length
5. WHEN a user accesses the activity log, THE Platform SHALL display login history and subscription changes sorted by date descending, showing the most recent 50 entries per page with pagination controls
6. WHEN a user navigates to the settings page, THE Frontend SHALL display tabs for Profile, Security, Notifications, Billing, Privacy, and Appearance

---

### Requirement 6: Frontend Architecture and State Management

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a developer, I want a well-structured Angular application with centralised state management, so that the codebase is maintainable and performant.

#### Acceptance Criteria

1. THE Frontend SHALL use NgRx Store for global state management with dedicated state slices for auth (tokens, login status), user (profile data), roles, and permissions
2. THE Frontend SHALL use NgRx Effects for all asynchronous API operations including token refresh
3. THE Frontend SHALL use lazy-loaded feature modules for admin, subscriber, and public areas such that none of these feature modules are included in the initial main bundle
4. THE Frontend SHALL apply OnPush change detection on all components
5. THE Frontend SHALL use HTTP interceptors to inject JWT tokens and queue outgoing requests while a token refresh is in progress, replaying them once the new token is obtained
6. WHEN a route guard determines the current user lacks the required role or permission, THE Frontend SHALL deny navigation and redirect the user to an unauthorised page
7. IF a token refresh attempt fails (expired refresh token or server error), THEN THE Frontend SHALL clear the auth state, revoke the stored tokens, and redirect the user to the login page
8. WHEN the Frontend application loads, THE Frontend SHALL load only the main bundle and shared dependencies, deferring all feature module code until the user navigates to the corresponding route

---

### Requirement 7: UI Design System and Theming

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a user, I want a consistent and modern visual experience with dark mode support, so that the Platform feels professional and is comfortable to use.

#### Acceptance Criteria

1. THE Frontend SHALL use DaisyUI with a custom theme defining primary, secondary, accent, and neutral colour tokens, and all UI components SHALL render using these tokens rather than hard-coded colour values
2. WHEN a user toggles dark mode, THE Frontend SHALL switch the active data-theme attribute within 150ms and persist the preference in local storage
3. IF no user preference is stored in local storage, THEN THE Frontend SHALL detect the system colour scheme preference via the prefers-color-scheme media query and apply the matching theme on initial page load
4. IF local storage is unavailable or write fails, THEN THE Frontend SHALL continue to apply the selected theme for the current session without displaying an error to the user
5. THE Frontend SHALL apply a single-column layout at the mobile breakpoint (375px), a two-column layout at tablet (768px), and a multi-column layout at desktop (1024px) and wide (1280px) breakpoints, with no horizontal scrollbar and no content overflow at any breakpoint
6. WHEN data is being fetched, THE Frontend SHALL display skeleton loaders with shimmer animation in place of content within 200ms of the fetch starting, and IF the fetch does not complete within 10 seconds, THEN THE Frontend SHALL replace the skeleton loader with an error state offering a retry action
7. WHEN a list or view contains no data, THE Frontend SHALL display an empty state illustration accompanied by a message stating why no data is present and a single call-to-action button that navigates the user to the relevant creation or import action
8. THE Frontend SHALL apply transition durations of 150ms for micro-interactions, 300ms for standard transitions, and 500ms for emphasis animations

---

### Requirement 8: Subscription Plan Builder

**Priority:** P0 (MVP — Phase 2)

**User Story:** As a Super Admin, I want to create and configure subscription plans with flexible pricing options, so that I can offer different tiers to attract varied subscriber segments.

#### Acceptance Criteria

1. WHEN a Super Admin submits a new plan with name (1–100 characters), price (0.01 to 999,999.99), currency (GBP, EUR, or USD), billing cycle, and features list (1–50 items), THE Plan_Builder SHALL validate all required fields, persist the plan, and make it visible on the pricing page within 5 seconds of successful save
2. THE Plan_Builder SHALL support billing cycles of Weekly, Monthly, Quarterly, Semi-Annual, and Annual
3. WHEN a Super Admin sets a trial period on a plan, THE Plan_Builder SHALL accept a trial duration between 1 and 365 days and communicate the configured duration to the Payment_Gateway on subscription creation
4. WHEN a Super Admin archives a plan, THE Plan_Builder SHALL hide the plan from the pricing page while maintaining uninterrupted access for existing subscribers until their current billing cycle ends or they cancel
5. WHEN a Super Admin configures upgrade and downgrade paths between plans, THE Plan_Builder SHALL restrict subscriber plan changes to only the configured paths and display an error message indicating the change is not permitted if a subscriber attempts an unconfigured transition
6. THE Plan_Builder SHALL support configuring promo code compatibility (on/off), setup fees (0.00 to 999,999.99 in plan currency), grace period days (0 to 90), and auto-renewal (on/off) per plan
7. WHEN a Super Admin saves a plan, THE Plan_Builder SHALL automatically create or update the corresponding plan in PayPal via the Billing Plans API and display a confirmation with the PayPal plan ID upon success
8. IF the PayPal Billing Plans API call fails during plan save, THEN THE Plan_Builder SHALL persist the plan locally with a "sync pending" status, display an error message indicating the PayPal synchronisation failed, and provide a manual retry option
9. IF a Super Admin attempts to create a plan with a name that already exists, THEN THE Plan_Builder SHALL reject the submission and display an error message indicating the plan name must be unique

---

### Requirement 9: PayPal Subscription Integration

**Priority:** P0 (MVP — Phase 2)

**User Story:** As a subscriber, I want to pay for my subscription using PayPal, so that I can use my preferred payment method securely.

#### Acceptance Criteria

1. WHEN a user selects PayPal checkout for a selected plan, THE PayPal_Service SHALL render PayPal Smart Buttons and initiate a subscription via the PayPal Subscriptions API using the corresponding PayPal Billing Plan ID
2. WHEN a subscriber approves the subscription on PayPal and returns to the Platform, THE PayPal_Service SHALL activate the subscription and grant content access within 10 seconds of return
3. IF a subscriber cancels or abandons the PayPal approval flow without completing payment, THEN THE PayPal_Service SHALL return the user to the checkout page with the selected plan preserved and display a message indicating the subscription was not completed
4. WHEN PayPal sends a `BILLING.SUBSCRIPTION.ACTIVATED` webhook, THE PayPal_Service SHALL verify the webhook signature and update the subscription status in the database
5. WHEN PayPal sends a `BILLING.SUBSCRIPTION.CANCELLED` webhook, THE PayPal_Service SHALL revoke content access at the end of the current billing period
6. WHEN PayPal sends a `BILLING.SUBSCRIPTION.PAYMENT.FAILED` webhook, THE PayPal_Service SHALL mark the subscription as past-due, retain content access for the duration of the plan's configured grace period, and send a notification email to the subscriber within 5 minutes
7. WHEN PayPal sends a `PAYMENT.SALE.COMPLETED` webhook, THE PayPal_Service SHALL record the transaction with amount, fees, net, and PayPal transaction ID
8. IF webhook signature verification fails, THEN THE PayPal_Service SHALL reject the webhook with an HTTP 401 response and not modify any subscription state
9. THE PayPal_Service SHALL implement idempotent webhook processing using the PayPal event ID as a deduplication key so that duplicate deliveries do not create duplicate records or state changes
10. WHEN a subscriber requests a subscription pause, THE PayPal_Service SHALL suspend the subscription via the PayPal API and retain the subscriber's access until the current billing period ends
11. IF the PayPal API returns an error when the PayPal_Service attempts to suspend or cancel a subscription, THEN THE PayPal_Service SHALL display an error message indicating the action could not be completed and retry the operation up to 3 times with exponential backoff before marking the action as failed
12. THE PayPal_Service SHALL respond to all incoming PayPal webhook requests within 10 seconds to prevent PayPal from marking the endpoint as unhealthy

---

### Requirement 10: Stripe Payment Integration

**Priority:** P0 (MVP — Phase 2)

**User Story:** As a subscriber, I want to pay with my credit or debit card via Stripe, so that I have an alternative to PayPal for subscription payments.

#### Acceptance Criteria

1. WHEN a user selects card payment at checkout, THE Stripe_Service SHALL display Stripe hosted payment fields and create a subscription via the Stripe Subscriptions API
2. WHEN Stripe confirms a successful payment, THE Stripe_Service SHALL activate the subscription and grant content access within 10 seconds
3. WHEN Stripe sends an `invoice.payment_failed` webhook, THE Stripe_Service SHALL mark the subscription as past-due and notify the subscriber via email within 60 seconds of receiving the event
4. WHEN Stripe sends a `customer.subscription.deleted` webhook, THE Stripe_Service SHALL revoke content access at period end
5. THE Stripe_Service SHALL verify webhook signatures using the Stripe webhook signing secret on every received event
6. THE Stripe_Service SHALL never store raw card details, relying exclusively on Stripe hosted fields for PCI DSS compliance
7. IF webhook signature verification fails, THEN THE Stripe_Service SHALL reject the webhook with an HTTP 400 response, log a security warning, and not modify any subscription state
8. THE Stripe_Service SHALL implement idempotent webhook processing so that duplicate event deliveries do not create duplicate records or duplicate state transitions
9. WHEN Stripe sends an `invoice.payment_succeeded` webhook for a recurring payment, THE Stripe_Service SHALL record the transaction with amount, currency, and Stripe payment intent ID

---

### Requirement 11: Checkout Flow and Payment Method Selection

**Priority:** P0 (MVP — Phase 2)

**User Story:** As a user, I want to choose my preferred payment method during checkout, so that I can complete my subscription purchase conveniently.

#### Acceptance Criteria

1. WHEN a user selects a plan and proceeds to checkout, THE Frontend SHALL display available payment options (PayPal, Card via Stripe) and a order summary
2. WHEN a promo code is entered at checkout, THE Platform SHALL validate the code, display the discounted price, and apply the discount to the subscription
3. WHEN a plan includes a free trial, THE Frontend SHALL clearly display the trial duration and the date when billing will begin
4. WHEN checkout completes successfully, THE Platform SHALL redirect the user to a confirmation page showing subscription details and next billing date
5. IF checkout fails due to a payment error, THEN THE Platform SHALL display a user-friendly error message and allow the user to retry or choose a different payment method

---

### Requirement 12: PayPal Admin Dashboard

**Priority:** P0 (MVP — Phase 2)

**User Story:** As an Admin, I want to view and manage PayPal transactions, subscriptions, and revenue within the application, so that I do not need to switch between the Platform and the PayPal dashboard.

#### Acceptance Criteria

1. WHEN an Admin navigates to the PayPal dashboard, THE Admin_Panel SHALL display a transaction history with search, date range filter, status filter, and amount sorting
2. WHEN an Admin views a subscription, THE Admin_Panel SHALL show the lifecycle timeline including creation, activation, payments, and cancellation events
3. THE Admin_Panel SHALL display revenue analytics including Monthly Recurring Revenue, Annual Recurring Revenue, churn rate, and revenue by plan
4. WHEN an Admin initiates a refund on a transaction, THE PayPal_Service SHALL process the refund via the PayPal API and update the transaction status
5. WHEN an Admin exports transactions, THE Admin_Panel SHALL generate a CSV or PDF file with all filtered transaction records
6. THE Admin_Panel SHALL display the PayPal environment indicator (Sandbox or Live) as a visible banner when Sandbox mode is active

---

### Requirement 13: Subscription Self-Service Management

**Priority:** P0 (MVP — Phase 2)

**User Story:** As a subscriber, I want to manage my subscription including upgrading, downgrading, pausing, and cancelling, so that I have control over my billing.

#### Acceptance Criteria

1. WHEN a subscriber navigates to the billing settings page, THE Frontend SHALL display current plan details, next billing date, payment method, and payment history
2. WHEN a subscriber initiates a plan upgrade, THE Platform SHALL calculate the prorated amount and process the change via the appropriate Payment_Gateway
3. WHEN a subscriber cancels their subscription, THE Platform SHALL process the cancellation and maintain access until the end of the current billing period
4. WHEN a subscriber's payment method fails and the grace period expires, THE Platform SHALL revoke content access and notify the subscriber with instructions to update payment details

---

### Requirement 14: Free Trial and Promo Code System

**Priority:** P0 (MVP — Phase 2)

**User Story:** As a Super Admin, I want to offer free trials and promotional discounts, so that I can attract new subscribers and run marketing campaigns.

#### Acceptance Criteria

1. WHEN a plan has a trial period configured, THE Platform SHALL grant full plan access for the trial duration without charging the subscriber
2. WHEN a trial period expires, THE Platform SHALL automatically begin billing on the next billing cycle
3. WHEN a Super Admin creates a promo code, THE Plan_Builder SHALL store the code with discount type (percentage or fixed), applicable plans, maximum uses, and expiry date
4. WHEN a user applies a valid promo code at checkout, THE Platform SHALL reduce the subscription price according to the code's discount configuration
5. IF a promo code has reached its maximum usage count or expired, THEN THE Platform SHALL reject the code and display an informative message

---

### Requirement 15: CMS Page Builder

**Priority:** P0 (MVP — Phase 3)

**User Story:** As a Super Admin, I want to visually build and edit website pages using drag-and-drop blocks, so that I can update site content without code changes.

#### Acceptance Criteria

1. WHEN a Super Admin opens the page builder, THE CMS SHALL display a visual editor with a block palette listing all available block types, a canvas area showing the current page layout, and a live preview panel
2. WHEN a Super Admin drags a block from the palette onto the canvas, THE CMS SHALL insert the block at the drop position and display a configuration panel with the block-specific settings
3. WHEN a Super Admin reorders blocks via drag-and-drop, THE CMS SHALL update the page layout in the preview within 500 milliseconds of the drop action
4. WHEN a Super Admin saves a page, THE CMS SHALL create a version snapshot containing the author, timestamp, and change summary, and persist the page content as a JSON structure
5. WHEN a Super Admin selects "Preview", THE CMS SHALL render the page as it will appear to public visitors and provide viewport toggles for desktop (≥1024px width), tablet (768–1023px width), and mobile (<768px width)
6. WHILE a Super Admin is actively editing a page, THE CMS SHALL auto-save the current draft state every 30 seconds to prevent content loss
7. WHEN a Super Admin publishes a page, THE CMS SHALL make the content visible to the public within 5 seconds
8. WHEN a Super Admin selects a block on the canvas and triggers a delete action, THE CMS SHALL remove the block from the page layout and update the preview within 500 milliseconds
9. WHEN a Super Admin triggers an undo or redo action, THE CMS SHALL revert or reapply the last editing action respectively and update the canvas and preview to reflect the restored state
10. IF an auto-save operation fails due to a network or server error, THEN THE CMS SHALL display a visible warning indicator to the Super Admin and retry the save within 10 seconds

---

### Requirement 16: CMS Content Blocks

**Priority:** P0 (MVP — Phase 3)

**User Story:** As a Super Admin, I want a library of pre-built content blocks, so that I can compose rich pages without custom development.

#### Acceptance Criteria

1. THE CMS SHALL provide the following block types: Hero Section, Rich Text, Image, Image Gallery, Video Embed, CTA, Pricing Table, Testimonials, FAQ Accordion, Stats Counter, Social Media Feed, Contact Form, Divider, HTML Embed, Blog Post List, Tip of the Day, Countdown Timer, and Banner Alert
2. WHEN a Super Admin configures a Hero Section block, THE CMS SHALL accept a background image, heading, subheading, and CTA button configuration
3. WHEN a Super Admin configures a Rich Text block, THE CMS SHALL provide a WYSIWYG editor with formatting, links, and media embeds
4. WHEN a Super Admin configures a Pricing Table block, THE CMS SHALL auto-generate the table from active subscription plans or accept manual configuration
5. WHEN a Super Admin configures a Contact Form block, THE CMS SHALL accept field definitions and email delivery address

---

### Requirement 17: Media Library

**Priority:** P0 (MVP — Phase 3)

**User Story:** As a Super Admin, I want a centralised media library to upload, organise, and manage all images and files, so that assets are reusable across pages and content.

#### Acceptance Criteria

1. WHEN a Super Admin uploads an image, THE Media_Library SHALL store the file in Azure Blob Storage, compress to a maximum file size of 80% of the original, convert to WebP format, and return a CDN URL within 10 seconds of upload completion
2. WHEN a Super Admin uploads multiple files via drag-and-drop (up to 20 files per batch), THE Media_Library SHALL process all files in parallel and display a percentage-based upload progress indicator per file
3. THE Media_Library SHALL accept uploads of JPG, PNG, WebP, SVG, GIF, PDF, DOCX, and MP4 file formats with maximum file sizes of 10MB for images, 50MB for documents, and 500MB for videos
4. IF a Super Admin uploads a file that exceeds the maximum file size or is not a supported format, THEN THE Media_Library SHALL reject the file, display an error message indicating the reason for rejection, and preserve any other files in the batch that are valid
5. WHEN a Super Admin searches the media library, THE Media_Library SHALL filter assets by filename, tags, and folder and return results within 200ms
6. WHEN a Super Admin edits an image, THE Media_Library SHALL provide in-browser crop, resize, and rotate tools and retain the original file as a separate version
7. THE Media_Library SHALL prevent upload completion for images until an alt text field of 1 to 125 characters has been provided by the Super Admin
8. WHEN a Super Admin deletes an asset that is currently referenced by one or more pages, THE Media_Library SHALL display a confirmation dialog listing all pages where the asset is in use, and SHALL only proceed with deletion if the Super Admin explicitly confirms

---

### Requirement 18: Navigation and Menu Management

**Priority:** P0 (MVP — Phase 3)

**User Story:** As a Super Admin, I want to manage site navigation menus visually, so that I can control the site structure without code changes.

#### Acceptance Criteria

1. WHEN a Super Admin opens the navigation editor, THE CMS SHALL display a visual tree representing the current menu structure
2. WHEN a Super Admin adds, removes, or reorders menu items via drag-and-drop, THE CMS SHALL update the navigation structure and persist changes
3. THE CMS SHALL support separate menus for header, footer, sidebar, and mobile navigation
4. WHEN a Super Admin sets visibility rules on a menu item, THE Frontend SHALL show or hide that item based on user role and subscription status
5. WHEN a Super Admin saves navigation changes, THE Frontend SHALL reflect the updated navigation for all users within 10 seconds

---

### Requirement 19: SEO Management

**Priority:** P0 (MVP — Phase 3)

**User Story:** As a Super Admin, I want to configure SEO metadata for every page, so that the Platform ranks well in search engine results.

#### Acceptance Criteria

1. WHEN a Super Admin edits a page's SEO settings, THE CMS SHALL accept meta title, meta description, Open Graph image, custom URL slug, and canonical URL
2. THE CMS SHALL display character counters for meta title (max 60) and meta description (max 160) with visual indicators when limits are exceeded
3. THE Platform SHALL auto-generate an XML sitemap reflecting all published pages and update it when pages are published or unpublished
4. WHEN a Super Admin sets a page to noindex, THE Frontend SHALL include the noindex meta tag in the page's HTML head
5. THE Frontend SHALL render structured data (JSON-LD) for pages that support rich search results

---

### Requirement 20: Content Version History and Rollback

**Priority:** P0 (MVP — Phase 3)

**User Story:** As a Super Admin, I want to view version history of all content changes and roll back to previous versions, so that I can recover from unintended edits.

#### Acceptance Criteria

1. WHEN a Super Admin saves content in the CMS, THE CMS SHALL create a version snapshot with timestamp, author, and change summary
2. WHEN a Super Admin views version history, THE CMS SHALL display a chronological list of all versions with the ability to preview each
3. WHEN a Super Admin selects "rollback" on a previous version, THE CMS SHALL restore that version as the current draft without deleting later versions
4. THE CMS SHALL retain version history according to a configurable retention policy (default: all versions kept)

---

### Requirement 21: Scheduled Publishing

**Priority:** P0 (MVP — Phase 3)

**User Story:** As a Super Admin, I want to schedule content publication and expiry, so that I can plan content releases in advance.

#### Acceptance Criteria

1. WHEN a Super Admin sets a future publish date on a page or block, THE CMS SHALL keep the content in draft state until the scheduled time, then publish automatically
2. WHEN a Super Admin sets an expiry date, THE CMS SHALL unpublish the content automatically at the specified time
3. THE CMS SHALL display a publishing queue view showing all scheduled content with dates and times
4. THE CMS SHALL execute scheduled publishing within 1 minute of the configured time, accounting for the configured timezone

---

### Requirement 22: Global Site Settings

**Priority:** P0 (MVP — Phase 3)

**User Story:** As a Super Admin, I want to manage global site settings including branding, maintenance mode, and analytics, so that I can control platform-wide configuration from one place.

#### Acceptance Criteria

1. WHEN a Super Admin updates the site name, logo, or favicon, THE Platform SHALL apply the changes across all pages within 10 seconds
2. WHEN a Super Admin enables maintenance mode, THE Platform SHALL display a "coming soon" page to all non-admin visitors while allowing admin access
3. WHEN a Super Admin injects analytics scripts, THE Platform SHALL include them on all pages subject to user cookie consent preferences
4. WHEN a Super Admin configures 301 redirects, THE API SHALL respond with permanent redirect status for matching URL paths

---

### Requirement 23: Tip Creation and Publishing

**Priority:** P1 (Phase 4)

**User Story:** As an Admin, I want to create, edit, and schedule horse racing tips, so that subscribers receive timely and accurate selections.

#### Acceptance Criteria

1. WHEN an Admin creates a tip with event date, race name (maximum 200 characters), selection (maximum 200 characters), odds (decimal format between 1.01 and 1000.00), stake (integer between 1 and 10 representing level stakes units), category (one of UK Racing, Irish Racing, or Other Sports), and optional rich text commentary (maximum 5000 characters), THE Tips_Engine SHALL validate all fields and persist the tip in draft status
2. IF an Admin submits a tip with any missing mandatory field or a value outside its permitted range, THEN THE Tips_Engine SHALL reject the submission, indicate which fields failed validation, and preserve the entered data for correction
3. WHEN an Admin publishes a tip, THE Tips_Engine SHALL make it visible to subscribers with access to the tip's category within 5 seconds
4. WHEN an Admin schedules a tip for future publication with a date and time (minimum 1 minute in the future, resolved to the nearest minute), THE Tips_Engine SHALL publish the tip automatically at the specified date and time
5. IF an Admin attempts to schedule a tip with a date and time that is in the past, THEN THE Tips_Engine SHALL reject the scheduling and display an error message indicating the scheduled time must be in the future
6. WHEN an Admin imports tips from a CSV file (maximum 500 rows, maximum 5 MB), THE Tips_Engine SHALL validate each row against the same field rules as manual creation, report a per-row list of errors with row number and failed field, and persist only the rows that pass validation
7. THE Tips_Engine SHALL enforce tip status transitions exclusively in the sequence Draft to Published to Archived, and SHALL reject any attempt to transition a tip backward or skip a status

---

### Requirement 24: Tip Categories

**Priority:** P1 (Phase 4)

**User Story:** As an Admin, I want to organise tips into categories, so that subscribers can view tips relevant to their interests and subscription plan.

#### Acceptance Criteria

1. THE Tips_Engine SHALL support default categories of UK Horse Racing, Irish Horse Racing, and Other Sports
2. WHEN an Admin creates a new category, THE Tips_Engine SHALL persist it and make it available for tip assignment and plan configuration
3. WHEN a subscriber's plan includes specific categories, THE Frontend SHALL display only tips in those categories in the subscriber's feed

---

### Requirement 25: Result Tracking and P&L Calculation

**Priority:** P1 (Phase 4)

**User Story:** As an Admin, I want to record tip results and have profit/loss calculated automatically, so that I can demonstrate platform performance transparently.

#### Acceptance Criteria

1. WHEN an Admin marks a tip result as Won, Lost, Void, or Push, THE Tips_Engine SHALL update the tip record and recalculate running P&L totals
2. THE Tips_Engine SHALL calculate P&L per day, week, month, year, and per category using level-stakes methodology
3. WHEN a tip is marked as Won, THE Tips_Engine SHALL calculate profit as (odds × stake) - stake
4. WHEN a tip is marked as Void or Push, THE Tips_Engine SHALL record zero profit/loss for that tip
5. THE Tips_Engine SHALL maintain a searchable archive of all tips filterable by date range, category, and result

---

### Requirement 26: Content Access Gating

**Priority:** P1 (Phase 4)

**User Story:** As a subscriber, I want to access tips content based on my subscription plan, so that I receive the content I am paying for.

#### Acceptance Criteria

1. WHEN a subscriber with an active plan accesses the tips feed, THE Platform SHALL display tips in categories included in their plan
2. WHEN a Free User accesses the tips area, THE Platform SHALL display only the "Tip of the Day" free preview
3. WHEN a Guest accesses a gated content page, THE Platform SHALL display a paywall with plan options and CTA to subscribe
4. IF a subscriber's access has been revoked due to payment failure, THEN THE Platform SHALL restrict tips access and display a message to update payment details

---

### Requirement 27: Blog System

**Priority:** P1 (Phase 4)

**User Story:** As an Admin, I want to publish blog posts integrated with the CMS, so that I can create SEO-optimised content to attract organic traffic.

#### Acceptance Criteria

1. WHEN an Admin creates a blog post with title, rich text content, featured image, and SEO fields, THE CMS SHALL persist the post and make it available for publishing
2. WHEN an Admin publishes a blog post, THE Platform SHALL make it accessible at its configured URL slug and include it in the sitemap
3. THE Frontend SHALL display a blog listing page with posts sorted by publish date, showing featured images and excerpts
4. THE CMS SHALL support draft, published, and scheduled states for blog posts

---

### Requirement 28: Social Media Components

**Priority:** P1 (Phase 5)

**User Story:** As a Super Admin, I want configurable social media components that can be placed on any page, so that I can promote social channels and enable content sharing.

#### Acceptance Criteria

1. THE Social_Components SHALL include: Social Follow Bar, Share Buttons, Social Proof Counter, Twitter Feed Embed, Facebook Page Plugin, Instagram Gallery, Telegram Join Button, YouTube Latest Video, and Testimonial Cards
2. WHEN a Super Admin toggles a social component's visibility for a specific page, THE Frontend SHALL show or hide that component accordingly
3. WHEN a user clicks a Share Button, THE Social_Components SHALL open the selected platform's share dialog with pre-filled text and URL
4. THE Platform SHALL generate Open Graph meta tags for all public pages to ensure correct social media preview cards
5. WHEN a Super Admin updates social media profile URLs in global settings, THE Social_Components SHALL reflect the changes across all pages within 10 seconds

---

### Requirement 29: Help Bot Widget

**Priority:** P1 (Phase 5)

**User Story:** As a visitor or subscriber, I want to access an automated help bot, so that I can get answers to common questions without waiting for human support.

#### Acceptance Criteria

1. THE Help_Bot SHALL display as a floating widget in the bottom-right corner of all pages, expandable and collapsible with animation
2. WHEN a user opens the Help_Bot, THE Help_Bot SHALL present a welcome message and quick-reply buttons for common topics
3. WHEN a user selects a topic or types a keyword, THE Help_Bot SHALL match the input to a configured conversation flow and display the relevant response
4. WHEN the Help_Bot cannot resolve a query, THE Help_Bot SHALL offer to escalate by creating a support ticket with the conversation context
5. WHEN a Super Admin configures conversation flows via the drag-and-drop flow builder, THE Help_Bot SHALL use the updated flows for new conversations within 30 seconds
6. THE Help_Bot SHALL persist conversation history per user session for continuity during the visit

---

### Requirement 30: Notification System

**Priority:** P1 (Phase 5)

**User Story:** As a subscriber, I want to receive notifications through my preferred channels when new tips are posted or important events occur, so that I stay informed without constantly checking the Platform.

#### Acceptance Criteria

1. WHEN a new tip is published, THE Notification_Service SHALL send alerts through all channels the subscriber has enabled (email, web push, Telegram, in-app) within 60 seconds of publication
2. WHEN a subscriber's subscription renewal is 7 days away, THE Notification_Service SHALL send a reminder via email
3. WHEN a subscriber's payment fails, THE Notification_Service SHALL send an alert via email and in-app notification within 5 minutes
4. WHEN a user configures notification preferences (per-channel toggles, per-category toggles, quiet hours), THE Notification_Service SHALL apply those preferences to all subsequent notifications by suppressing delivery on disabled channels, filtering out disabled categories, and holding notifications generated during the user's defined quiet hours for delivery when the quiet-hours window ends
5. THE Frontend SHALL display a notification bell icon showing the count of unread notifications (displayed as "99+" when the count exceeds 99), and WHEN the bell icon is clicked, THE Frontend SHALL show a dropdown listing the 20 most recent notifications in reverse chronological order
6. WHEN an Admin broadcasts an announcement, THE Notification_Service SHALL deliver it to all active subscribers via their preferred channels within 5 minutes
7. IF delivery to a subscriber's enabled channel fails (email bounce, push endpoint expired, or Telegram API error), THEN THE Notification_Service SHALL retry delivery up to 3 times with exponential backoff, and IF all retries fail, THEN THE Notification_Service SHALL mark the notification as failed and record it as undelivered in the subscriber's in-app notification list
8. WHEN a tip result is updated (Won, Lost, Void, or Push), THE Notification_Service SHALL send a result update notification through all channels the subscriber has enabled for the tip's category within 60 seconds of the result being recorded

---

### Requirement 31: Telegram Bot Integration

**Priority:** P1 (Phase 5)

**User Story:** As a subscriber, I want to receive tips directly in my Telegram chat, so that I get instant notifications on my mobile device.

#### Acceptance Criteria

1. WHEN a subscriber links their Telegram account via a unique connection code, THE Notification_Service SHALL associate their Telegram chat ID with their platform account
2. WHEN a tip is published in a category the subscriber follows, THE Notification_Service SHALL deliver a formatted message to their Telegram chat within 30 seconds
3. WHEN a subscriber unlinks their Telegram account, THE Notification_Service SHALL stop all Telegram notifications immediately

---

### Requirement 32: Referral Program

**Priority:** P1 (Phase 5)

**User Story:** As a subscriber, I want to refer friends and earn rewards, so that I am incentivised to grow the Platform community.

#### Acceptance Criteria

1. THE Platform SHALL generate a unique referral link for each active subscriber
2. WHEN a new user subscribes using a referral link, THE Platform SHALL credit the referrer with a configurable discount on their next billing cycle
3. WHEN a subscriber views their referral dashboard, THE Frontend SHALL display total referral clicks, successful conversions, and earned rewards
4. WHEN a Super Admin configures reward amounts and limits, THE Platform SHALL apply those rules to all future referral completions

---

### Requirement 33: Comments and Community Features

**Priority:** P1 (Phase 5)

**User Story:** As a subscriber, I want to comment on daily tips and participate in polls, so that I can engage with the community and discuss selections.

#### Acceptance Criteria

1. WHEN a subscriber posts a comment under a daily tips section, THE Platform SHALL display the comment with author name, avatar, and timestamp
2. WHEN a Moderator deletes or hides a comment, THE Platform SHALL remove it from public view within 5 seconds
3. WHEN an Admin creates a poll with question and options, THE Platform SHALL display it to subscribers and accept one vote per subscriber
4. WHEN a subscriber votes in a poll, THE Platform SHALL update the results in real-time for all viewers
5. THE Platform SHALL support direct messaging from subscriber to Admin for support queries

---

### Requirement 34: Performance Analytics — Public Proof

**Priority:** P2 (Phase 6)

**User Story:** As a visitor, I want to see verified performance statistics on the public site, so that I can evaluate the Platform's track record before subscribing.

#### Acceptance Criteria

1. THE Analytics_Dashboard SHALL display a public stats page showing strike rate, ROI, and monthly P&L calculated from verified tip results
2. THE Frontend SHALL display charts for profit over time, win rate trends, and category comparison on the public stats page
3. THE Frontend SHALL display a last-30-days performance summary on the landing page
4. WHEN a visitor requests a performance export, THE Platform SHALL generate a CSV or PDF of the displayed results

---

### Requirement 35: Subscriber Performance Dashboard

**Priority:** P2 (Phase 6)

**User Story:** As a subscriber, I want a personal performance dashboard, so that I can track my results if following the Platform's tips.

#### Acceptance Criteria

1. WHEN a subscriber views their dashboard, THE Analytics_Dashboard SHALL display personal P&L calculated at level stakes for all tips in their subscribed categories
2. THE Analytics_Dashboard SHALL allow filtering by category and date range
3. THE Frontend SHALL display current winning and losing streak information
4. THE Analytics_Dashboard SHALL provide monthly performance summaries with optional email digest

---

### Requirement 36: Admin Revenue Analytics

**Priority:** P2 (Phase 6)

**User Story:** As a Super Admin, I want combined revenue analytics across all payment providers, so that I can monitor business health and forecast growth.

#### Acceptance Criteria

1. THE Admin_Panel SHALL display combined revenue from PayPal and Stripe in a unified view
2. THE Admin_Panel SHALL calculate and display Monthly Recurring Revenue, churn rate, Lifetime Value per subscriber, and revenue by plan
3. THE Admin_Panel SHALL display revenue trend charts with daily, weekly, and monthly granularity
4. THE Admin_Panel SHALL provide subscriber growth forecasting based on current trajectory

---

### Requirement 37: GDPR Compliance — Data Subject Rights

**Priority:** P2 (Phase 6)

**User Story:** As a registered user, I want to exercise my GDPR rights including data export and account deletion, so that I have control over my personal data.

#### Acceptance Criteria

1. WHEN a user requests a data export, THE GDPR_Module SHALL generate a downloadable archive (JSON and CSV) containing all personal data (profile data, subscription history, payment records excluding card details, tip access history, comments, notification preferences, and consent records) and notify the user via email with a download link within 24 hours, with the download link remaining accessible for 7 days
2. WHEN a user requests account deletion, THE GDPR_Module SHALL initiate a 30-day grace period using soft delete, notify the user via email confirming the deletion request and grace period end date, and permanently purge all personal data after the 30-day period expires
3. IF a user logs in during the 30-day deletion grace period, THEN THE GDPR_Module SHALL display a notice that deletion is pending and provide an option to cancel the deletion request, restoring full account access upon cancellation
4. WHEN a user edits their personal data, THE Platform SHALL update the records within 5 seconds and display a confirmation of the change (Right to Rectification)
5. THE GDPR_Module SHALL maintain a record of all data processing activities and consent timestamps per user, retaining these records for a minimum of 3 years from the date of the activity
6. WHEN a Super Admin triggers a breach notification, THE GDPR_Module SHALL send notification emails to all affected users (as designated by the Super Admin) within 1 hour, including a description of the breach and recommended user actions

---

### Requirement 38: Cookie Consent System

**Priority:** P2 (Phase 6)

**User Story:** As a visitor, I want to control which cookies are stored on my device, so that I can manage my privacy in accordance with regulations.

#### Acceptance Criteria

1. WHEN a first-time visitor loads any page, THE GDPR_Module SHALL display a cookie consent banner with Accept All, Reject All, and Customise options
2. WHEN a visitor selects Customise, THE GDPR_Module SHALL display granular toggles for Essential (always on), Analytics, Marketing, and Preferences categories
3. WHILE a visitor has not consented to non-essential cookies, THE Frontend SHALL block all non-essential scripts including analytics and marketing pixels
4. WHEN a visitor saves cookie preferences, THE GDPR_Module SHALL persist the consent and not re-prompt until the cookie policy changes or 12 months have elapsed
5. THE GDPR_Module SHALL make saved consent auditable with timestamp and selected preferences per user or session

---

### Requirement 39: AI-Generated Imagery Integration

**Priority:** P2 (Phase 6)

**User Story:** As a Super Admin, I want to generate custom images using AI directly from the Media Library, so that I can create unique visual content without external design tools.

#### Acceptance Criteria

1. WHEN a Super Admin enters a text prompt in the Media Library AI generator, THE Media_Library SHALL call the DALL-E API and return generated images for selection
2. WHEN a Super Admin selects a generated image, THE Media_Library SHALL store it in Azure Blob Storage and make it available like any uploaded asset
3. THE Media_Library SHALL also provide Unsplash and Pexels integration for searching and importing free stock photos directly

---

### Requirement 40: Animations and Micro-Interactions

**Priority:** P2 (Phase 6)

**User Story:** As a user, I want smooth animations and visual feedback throughout the Platform, so that interactions feel responsive and polished.

#### Acceptance Criteria

1. THE Frontend SHALL implement page transition animations using Angular route animations with duration of 300ms
2. WHEN a user hovers over a card element, THE Frontend SHALL apply a scale and shadow lift animation within 150ms
3. THE Frontend SHALL display scroll-triggered reveal animations using Intersection Observer for content blocks entering the viewport
4. WHEN a number counter component enters the viewport, THE Frontend SHALL animate from zero to the target value over 1 second
5. THE Frontend SHALL implement a confetti animation on successful subscription purchase confirmation

---

### Requirement 41: Progressive Web App

**Priority:** P2 (Phase 7)

**User Story:** As a mobile user, I want to install the Platform as an app on my device and access previously loaded content offline, so that I have a native-like experience.

#### Acceptance Criteria

1. THE PWA_Shell SHALL register a service worker that caches the application shell and previously loaded tip data for offline access
2. WHEN a user installs the PWA, THE Frontend SHALL display a branded splash screen and app icon on the device home screen
3. WHILE the device is offline, THE PWA_Shell SHALL serve cached tips and display a connectivity indicator
4. WHEN connectivity is restored, THE PWA_Shell SHALL sync any queued actions and fetch updated content
5. THE Frontend SHALL implement a bottom navigation bar optimised for mobile interaction patterns

---

### Requirement 42: Landing Page

**Priority:** P0 (MVP — Phase 3)

**User Story:** As a Guest, I want an engaging landing page that communicates the Platform's value, so that I am motivated to subscribe.

#### Acceptance Criteria

1. THE Frontend SHALL display a hero section with AI-generated artwork, headline, subheadline, and CTA button above the fold
2. THE Frontend SHALL display an animated pricing table auto-generated from active subscription plans
3. THE Frontend SHALL display a public results section showing recent performance statistics
4. THE Frontend SHALL display a testimonials carousel managed via the CMS
5. THE Frontend SHALL display a "Tip of the Day" free preview accessible to non-subscribers
6. THE Frontend SHALL display an FAQ section with search functionality
7. THE Frontend SHALL implement parallax scrolling and scroll-triggered animations on the landing page

---

### Requirement 43: Admin Dashboard Overview

**Priority:** P0 (MVP — Phase 2)

**User Story:** As an Admin, I want a dashboard overview page with key metrics and quick actions, so that I can monitor the platform health at a glance.

#### Acceptance Criteria

1. WHEN an Admin navigates to the admin dashboard, THE Admin_Panel SHALL display summary cards for subscriber count, MRR, today's tips status, recent signups, and payment alerts
2. THE Admin_Panel SHALL display trend charts for revenue and subscriber growth
3. THE Admin_Panel SHALL display a recent activity feed showing the last 10 admin and system actions
4. THE Admin_Panel SHALL display quick action shortcuts for creating tips, managing plans, and viewing subscribers
5. WHEN the database contains no data (fresh install), THE Admin_Panel SHALL display onboarding cards guiding the admin to create their first plan, post a tip, and customise the landing page

---

### Requirement 44: Audit Logging

**Priority:** P0 (MVP — Phase 2)

**User Story:** As a Super Admin, I want a complete audit trail of all administrative actions, so that I can track who made changes and when for security and compliance.

#### Acceptance Criteria

1. WHEN any admin-level action is performed (user change, plan change, content change, role change), THE Platform SHALL log the action with actor, target, action type, timestamp, and before/after values
2. WHEN a Super Admin views the audit log, THE Admin_Panel SHALL display a searchable, filterable, paginated list of all audit events
3. THE Platform SHALL retain audit logs for a minimum of 2 years
4. THE audit log SHALL be append-only and not editable by any user including Super Admins

---

### Requirement 45: Generic Data Table Component

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a developer, I want a reusable data table component with standard features, so that all list views across the Platform have consistent functionality.

#### Acceptance Criteria

1. THE Data_Table SHALL support pagination with configurable page sizes (10, 25, 50, 100) and total count display
2. THE Data_Table SHALL support global text search with 300ms debounce
3. THE Data_Table SHALL support per-column sorting (ascending and descending) and multi-column sort
4. THE Data_Table SHALL support per-column filtering with type-appropriate controls (text, dropdown, date range)
5. THE Data_Table SHALL support row selection with checkboxes and a contextual bulk action bar
6. THE Data_Table SHALL support data export of filtered or selected rows as CSV or Excel
7. WHEN the Data_Table is loading data, THE Data_Table SHALL display skeleton rows
8. WHEN the Data_Table has no matching data, THE Data_Table SHALL display an empty state with illustration and CTA
9. WHEN the API returns an error, THE Data_Table SHALL display an error message with a retry button
10. THE Data_Table SHALL adapt to mobile viewports with horizontal scroll or card-based layout

---

## Non-Functional Requirements

---

### Requirement 46: API Performance

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a user, I want fast API responses, so that the Platform feels responsive during all interactions.

#### Acceptance Criteria

1. THE API SHALL respond to standard read endpoints within 200ms at the 95th percentile under normal load
2. THE API SHALL implement a caching layer for frequently accessed data including plans, published tips, and CMS pages
3. THE Frontend SHALL implement lazy loading for all feature modules to keep initial bundle size below 250KB gzipped
4. THE Platform SHALL serve all static assets via Azure CDN with cache headers set for minimum 1 year on versioned assets
5. THE Frontend SHALL implement virtual scrolling for any list exceeding 100 items

---

### Requirement 47: Security Standards

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a platform operator, I want comprehensive security measures, so that user data and financial transactions are protected from common attack vectors.

#### Acceptance Criteria

1. THE Platform SHALL enforce HTTPS on all communications and redirect HTTP to HTTPS
2. THE API SHALL implement rate limiting at 100 requests per minute per IP on authentication endpoints and 1000 requests per minute per user on general endpoints
3. THE API SHALL validate and sanitise all input on every endpoint using server-side validation
4. THE Platform SHALL store all secrets in Azure Key Vault with no credentials hardcoded in source code
5. THE API SHALL include security headers: X-Content-Type-Options, X-Frame-Options, Strict-Transport-Security, and Content-Security-Policy
6. THE Platform SHALL use parameterised queries exclusively via Entity Framework Core to prevent SQL injection
7. THE API SHALL never log passwords, authentication tokens, or payment card details
8. THE Platform SHALL scan NuGet and npm dependencies for known vulnerabilities during build

---

### Requirement 48: Reliability and Observability

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a platform operator, I want structured logging, health checks, and reliable data backups, so that I can monitor system health and recover from failures.

#### Acceptance Criteria

1. THE API SHALL implement health check endpoints for database connectivity, PayPal API availability, and Azure Blob Storage access
2. THE Platform SHALL use structured logging via Serilog writing to Azure Application Insights with correlation IDs per request
3. THE Platform SHALL configure Azure SQL geo-redundant backups with point-in-time restore capability
4. WHEN an error spike exceeds 10 errors per minute, THE Platform SHALL trigger an automated alert to the operations team
5. THE API SHALL return user-friendly error messages with ProblemDetails format for all error responses, never exposing stack traces in production

---

### Requirement 49: Accessibility Compliance

**Priority:** P0 (MVP — Phase 1)

**User Story:** As a user with disabilities, I want the Platform to meet accessibility standards, so that I can use all features with assistive technologies.

#### Acceptance Criteria

1. THE Frontend SHALL maintain a minimum contrast ratio of 4.5:1 for all text elements
2. THE Frontend SHALL include ARIA labels, roles, and states on all interactive elements
3. THE Frontend SHALL support full keyboard navigation including visible focus indicators
4. THE Frontend SHALL pass automated axe-core accessibility scans with zero violations on all pages
5. THE Frontend SHALL provide alt text for all images and meaningful link text for all hyperlinks

---

## Cross-Reference Matrix

| Requirement | Related Requirements | Relationship |
|---|---|---|
| R1 (Auth) | R2 (2FA), R3 (Roles), R6 (NgRx), R47 (Security) | Auth feeds into authorization and state management |
| R3 (Roles) | R4 (User Mgmt), R18 (Navigation), R26 (Access Gating) | Roles control visibility and access across modules |
| R8 (Plans) | R9 (PayPal), R10 (Stripe), R11 (Checkout), R14 (Promos), R26 (Access Gating) | Plans are the core entity connecting payments to content access |
| R9 (PayPal) | R10 (Stripe), R12 (PayPal Dashboard), R13 (Self-Service), R36 (Revenue) | Payment processing feeds analytics and management |
| R15 (Page Builder) | R16 (Blocks), R17 (Media), R19 (SEO), R20 (Versions), R21 (Scheduling) | CMS modules are interdependent |
| R23 (Tips) | R24 (Categories), R25 (Results), R26 (Gating), R30 (Notifications) | Tips flow through categorisation to results to delivery |
| R25 (Results) | R34 (Public Analytics), R35 (Subscriber Dashboard) | Results feed both public and subscriber analytics |
| R37 (GDPR) | R38 (Cookies), R5 (Profile), R44 (Audit) | Compliance ties together consent, data control, and logging |
| R41 (PWA) | R6 (Frontend Arch), R46 (Performance) | PWA builds on frontend architecture and performance standards |
| R45 (Data Table) | R4 (User Mgmt), R12 (PayPal Dashboard), R44 (Audit) | Shared component used in all list views |
