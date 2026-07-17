# Kitchen Recipes

Scalable base project for a cooking recipes platform using ASP.NET Core MVC (C#) with a React + TypeScript frontend.

## Project Goal

This first milestone creates a clean and scalable foundation that satisfies the original assignment constraints and your extra requirements:

- ASP.NET Core MVC backend in C#
- React + TypeScript frontend
- Tailwind CSS for responsive UI
- SQL Server-ready backend using EF Core
- Layered architecture (`Domain`, `Application`, `Infrastructure`, `Web`)
- Async-first service/API pattern
- Centralized English text in one file: `src/frontend/kitchenrecipes-ui/src/locales/en.json`
- Google SMTP configuration placeholders for future newsletter feature
- Structured to evolve into:
  - CRUD modules (minimum 3 tables)
  - User accounts
  - Stored procedures/triggers usage where needed
  - AI assistant for the full website

## Current Architecture

- `src/backend/KitchenRecipes.Domain`
  - Core entities and base entity
- `src/backend/KitchenRecipes.Application`
  - Contracts and use-case services
- `src/backend/KitchenRecipes.Infrastructure`
  - EF Core DbContext + SQL Server wiring
- `src/backend/KitchenRecipes.Web`
  - MVC host + API controllers + React static hosting
- `src/frontend/kitchenrecipes-ui`
  - React + TypeScript + Tailwind UI app

## Implemented in Step 1 (Foundation)

- Created multi-project .NET solution and references
- Added core domain entities:
  - `Recipe`
  - `Ingredient`
  - `PantryItem`
  - `AppUser`
  - `NewsletterSubscriber`
- Added EF Core `ApplicationDbContext` with SQL Server config
- Added a first API endpoint:
  - `GET /api/system/health`
- Wired cookie authentication baseline
- Wired React build output into MVC static hosting (`wwwroot/react`)
- Built responsive React shell pages with React Router
- Added centralized text lookup from `en.json`
- Added SMTP configuration section for Gmail App Password

## Implemented in Step 2 (Recipes CRUD)

- Added full async recipe service layer:
  - `IRecipeService`
  - `RecipeService`
- Added recipe DTO contracts:
  - `RecipeDto`
  - `UpsertRecipeRequest`
- Added full API controller for recipes:
  - `GET /api/recipes`
  - `GET /api/recipes/{id}`
  - `POST /api/recipes`
  - `PUT /api/recipes/{id}`
  - `DELETE /api/recipes/{id}`
- Added full React pages for recipe CRUD:
  - list page
  - create page
  - edit page
  - delete action
- Added frontend API layer (`axios`) and recipe types
- Added all recipe UI texts to centralized `en.json`
- Created EF Core migration:
  - `src/backend/KitchenRecipes.Infrastructure/Data/Migrations/20260707215453_InitialCreate.cs`

## Implemented in Step 3 (Pantry CRUD)

- Added full async pantry service layer:
  - `IPantryItemService`
  - `PantryItemService`
- Added pantry DTO contracts:
  - `PantryItemDto`
  - `UpsertPantryItemRequest`
- Added full API controller for pantry items:
  - `GET /api/pantry-items`
  - `GET /api/pantry-items/{id}`
  - `POST /api/pantry-items`
  - `PUT /api/pantry-items/{id}`
  - `DELETE /api/pantry-items/{id}`
- Added full React pages for pantry CRUD:
  - list page
  - create page
  - edit page
  - delete action
- Added frontend API layer and TypeScript models for pantry
- Added all pantry UI texts to centralized `en.json`

## Implemented in Step 4 (Newsletter Subscribers CRUD)

- Added full async newsletter subscriber service layer:
  - `INewsletterSubscriberService`
  - `NewsletterSubscriberService`
- Added newsletter subscriber DTO contracts:
  - `NewsletterSubscriberDto`
  - `UpsertNewsletterSubscriberRequest`
- Added full API controller for newsletter subscribers:
  - `GET /api/newsletter-subscribers`
  - `GET /api/newsletter-subscribers/{id}`
  - `POST /api/newsletter-subscribers`
  - `PUT /api/newsletter-subscribers/{id}`
  - `DELETE /api/newsletter-subscribers/{id}`
- Added full React pages for newsletter subscribers CRUD:
  - list page
  - create page
  - edit page
  - delete action
- Added frontend API layer and TypeScript models for newsletter subscribers
- Added all newsletter UI texts to centralized `en.json`

## Implemented in Step 5 (Users, Authentication, and Roles)

- Added full account API flows:
  - `POST /api/account/register`
  - `POST /api/account/login`
  - `POST /api/account/logout`
  - `GET /api/account/me`
- Added role management API (Admin only):
  - `GET /api/account/users`
  - `PUT /api/account/users/{id}/role`
- Added secure password hashing using PBKDF2 (salt + iterations)
- Added role claims in cookie authentication (`Admin`, `User`)
- Added role-based authorization to CRUD APIs:
  - all authenticated users can read
  - only `Admin` can create/update/delete
- Added account React page with:
  - register/login/logout
  - current user info
  - admin panel to change user roles
- Added DB role field and migration:
  - `src/backend/KitchenRecipes.Infrastructure/Data/Migrations/20260707220725_AddAppUserRole.cs`

## Implemented in Step 6 (Newsletter SMTP Delivery + 5 Ready Templates)

- Added newsletter campaign service with real SMTP delivery using Google App Password:
  - `INewsletterCampaignService`
  - `NewsletterCampaignService`
  - `INewsletterSender`
  - `SmtpNewsletterSender`
- Added Admin-only newsletter campaign API:
  - `GET /api/newsletter-campaigns/templates`
  - `GET /api/newsletter-campaigns/stats`
  - `POST /api/newsletter-campaigns/send`
- Added 5 ready-to-send newsletter templates:
  - `weekly_menu` (Weekly Menu Boost)
  - `pantry_rescue` (Pantry Rescue)
  - `seasonal_special` (Seasonal Special)
  - `healthy_starters` (Healthy Starters)
  - `community_challenge` (Community Challenge)
- Added newsletter campaign section in React UI to choose and send templates.
- Corrected decimal precision mapping for `Ingredient.Quantity` and `PantryItem.AvailableQuantity`.
- Added decimal precision migration:
  - `src/backend/KitchenRecipes.Infrastructure/Data/Migrations/20260707221304_AddDecimalPrecisionConfig.cs`

## Implemented in Step 7 (SQL Server Trigger + Stored Procedure)

- Added trigger-based audit for user role changes:
  - trigger: `trg_AppUsers_RoleChange_Audit`
  - audit table: `AppUserRoleAudits`
- Added stored procedure for newsletter reporting:
  - procedure: `usp_GetNewsletterCampaignStats`
- Added campaign audit table used by newsletter send flow:
  - `NewsletterCampaignAudits`
- Added endpoint powered by stored procedure (Admin only):
  - `GET /api/newsletter-campaigns/stats`
- Added campaign history endpoint (Admin only):
  - `GET /api/newsletter-campaigns/history?limit=20`
- Added campaign history CSV export endpoint (Admin only):
  - `GET /api/newsletter-campaigns/history/export?limit=100`
- Added migration with SQL objects:
  - `src/backend/KitchenRecipes.Infrastructure/Data/Migrations/20260707221619_AddNewsletterSqlObjects.cs`

## Implemented in Step 8 (Campaign History View)

- Added campaign history retrieval in backend reporting service.
- Added Admin endpoint for history list:
  - `GET /api/newsletter-campaigns/history?limit=20`
- Added campaign history table in newsletter UI (template, recipients, sent date).
- Added campaign history CSV export action in newsletter UI.

## Implemented in Step 9 (AI Assistant Foundations)

- Added initial AI assistant service to suggest recipes from current pantry stock.
- Added authenticated endpoint for suggestions:
  - `GET /api/assistant/suggestions?limit=5`
- Added assistant React page with:
  - suggestion cards
  - match score
  - matched pantry items
  - missing ingredients summary
- Added quick access button in pantry list page to open assistant.

## Implemented in Step 10 (Automated Service Tests)

- Added a dedicated test project:
  - `src/tests/KitchenRecipes.Tests`
- Added focused automated tests for:
  - `AccountService`
  - `PantryAssistantService`
  - `NewsletterCampaignService`
- Added in-memory EF Core test setup and fake newsletter sender.
- Current automated suite validates:
  - first-user admin registration rule
  - duplicate email protection
  - credential validation
  - pantry suggestion ranking and missing ingredients
  - newsletter delivery to confirmed subscribers only
  - newsletter audit creation

## Implemented in Step 11 (API Integration Tests)

- Added `WebApplicationFactory`-based API integration tests in:
  - `src/tests/KitchenRecipes.Tests/Integration`
- Added integration coverage for:
  - register + authenticated `/api/account/me`
  - admin-only protection on recipe creation
  - admin recipe create + fetch flow
  - newsletter campaign send endpoint with audit verification
- Added isolated test host configuration using in-memory EF Core and fake SMTP sender.

## Implemented in Step 12 (Default Seed Data + Dev Credentials)

- Added automatic startup seed for local development data.
- Ensured minimum seeded records in key tables:
  - `AppUsers`
  - `NewsletterSubscribers`
  - `PantryItems`
  - `Recipes`
  - `Ingredients`
  - `NewsletterCampaignAudits`
  - `AppUserRoleAudits`
- Added development users list with corresponding password in:
  - `default-users.md`
- Included Admin account with SMTP owner email:
  - `andre2411fernandes@gmail.com`

## Implemented in Step 13 (Quantity-Aware AI Assistant)

- Improved assistant scoring to consider pantry quantity and unit compatibility.
- Added stock coverage percentage to each suggestion.
- Added low-stock ingredient reporting for partially covered recipes.
- Updated assistant UI to show:
  - overall match score
  - stock coverage score
  - insufficient ingredients due to low available quantity

## Implemented in Step 14 (Filtered Campaign History + CSV Export)

- Added newsletter campaign history filters for:
  - template key
  - sent from date
  - sent to date
  - row limit
- Extended CSV export to respect the same history filters.
- Added API integration tests for filtered history and filtered CSV export.

## Implemented in Step 15 (Extended API Integration Coverage)

- Added integration coverage for newsletter reporting stats endpoint.
- Added integration coverage for admin-only authorization on newsletter stats.
- Added end-to-end integration coverage for user role promotion:
  - admin updates user role
  - promoted user signs in again
  - promoted user can access admin-only recipe write flow

## Implemented in Step 16 (Conversational AI Assistant)

- Upgraded the assistant into a conversational recipe helper.
- Added a new authenticated chat endpoint:
  - `POST /api/assistant/chat`
- The assistant now lets the user ask for recipes in natural language, for example:
  - quick meals
  - high-protein ideas
  - recipes using specific fridge ingredients first
- Added assistant responses with:
  - pantry highlights
  - top recommended recipes
  - follow-up prompt suggestions
- Reworked the assistant page into a chat-style UI while keeping recipe cards visible as decision support.
- Added automated tests for the new chat reply flow.

## Implemented in Step 17 (Unit Conversion Support for Assistant)

- Improved assistant quantity scoring with practical kitchen unit conversion support.
- Added conversions for common weight and volume units, including examples such as:
  - `kg` <-> `g`
  - `l` <-> `ml`
  - `cup` -> `ml`
  - `tbsp` / `tsp` -> `ml`
- Preserved partial fallback scoring when ingredient names match but units remain incompatible.
- Added conversion insight messages to assistant suggestions so the UI can explain how stock coverage was calculated.
- Added automated tests for converted weight coverage and converted volume shortage scenarios.

## Implemented in Step 18 (Persistent AI Assistant Workspace)

- Added persistent assistant conversations with user-scoped chat threads and messages.
- Added assistant thread endpoints for authenticated users:
  - `GET /api/assistant/threads`
  - `GET /api/assistant/threads/{threadId}`
  - `POST /api/assistant/threads/message`
  - `PATCH /api/assistant/threads/{threadId}`
  - `DELETE /api/assistant/threads/{threadId}`
- Added AI profile endpoints to switch model behavior:
  - `GET /api/assistant/profile`
  - `PUT /api/assistant/profile`
- Added full-page assistant UI with:
  - chat history sidebar
  - new chat creation
  - rename and delete conversation actions
  - floating launcher button
- Added model profile controls in UI:
  - Fast profile
  - Quality profile

## Implemented in Step 19 (Global Site-Wide Assistant + Real Streaming)

- Promoted the assistant from pantry-only to site-wide guidance.
- Added global assistant route in the frontend:
  - `/assistant`
- Kept quick access to assistant through the floating launcher and pantry shortcut.
- Assistant now supports both product guidance and cooking support:
  - recipes and pantry strategy
  - newsletter/admin workflow guidance
  - account/access guidance
- Upgraded streaming to real provider-driven token streaming (instead of synthetic token splitting).
- Added chat auto-scroll behavior and independent scroll regions for conversation and history.
- Preserved compatibility for legacy paths while standardizing on `/api/assistant/*`.

## Implemented in Step 20 (Tables Filtering, Pagination, and Document Export)

- Added filters in all frontend tables:
  - recipes list
  - pantry items list
  - newsletter subscribers list
  - newsletter campaign history
  - account admin users list
- Added pagination in all frontend tables with default page size set to 5 rows.
- Added per-table export actions in all frontend tables for:
  - PDF
  - DOCX
  - Excel (`.xlsx`)
- Added shared table tooling to keep behavior consistent across modules.

## Key File References

- Backend startup: `src/backend/KitchenRecipes.Web/Program.cs`
- DB context: `src/backend/KitchenRecipes.Infrastructure/Data/ApplicationDbContext.cs`
- Default data seeder: `src/backend/KitchenRecipes.Infrastructure/Data/DefaultDataSeeder.cs`
- Tests project: `src/tests/KitchenRecipes.Tests/KitchenRecipes.Tests.csproj`
- Integration test host: `src/tests/KitchenRecipes.Tests/Integration/CustomWebApplicationFactory.cs`
- Default users credentials: `default-users.md`
- API sample: `src/backend/KitchenRecipes.Web/Controllers/Api/SystemController.cs`
- Recipes API: `src/backend/KitchenRecipes.Web/Controllers/Api/RecipesController.cs`
- Pantry API: `src/backend/KitchenRecipes.Web/Controllers/Api/PantryItemsController.cs`
- Newsletter API: `src/backend/KitchenRecipes.Web/Controllers/Api/NewsletterSubscribersController.cs`
- Newsletter Campaign API: `src/backend/KitchenRecipes.Web/Controllers/Api/NewsletterCampaignsController.cs`
- Account API: `src/backend/KitchenRecipes.Web/Controllers/Api/AccountController.cs`
- Frontend app shell: `src/frontend/kitchenrecipes-ui/src/App.tsx`
- Recipes list page: `src/frontend/kitchenrecipes-ui/src/pages/recipes/RecipeListPage.tsx`
- Recipe form page: `src/frontend/kitchenrecipes-ui/src/pages/recipes/RecipeFormPage.tsx`
- Recipes API client: `src/frontend/kitchenrecipes-ui/src/services/recipesApi.ts`
- Pantry list page: `src/frontend/kitchenrecipes-ui/src/pages/pantry/PantryListPage.tsx`
- Assistant page: `src/frontend/kitchenrecipes-ui/src/pages/pantry/PantryAssistantPage.tsx`
- Pantry form page: `src/frontend/kitchenrecipes-ui/src/pages/pantry/PantryFormPage.tsx`
- Assistant and pantry API client: `src/frontend/kitchenrecipes-ui/src/services/pantryApi.ts`
- Newsletter list page: `src/frontend/kitchenrecipes-ui/src/pages/newsletter/NewsletterListPage.tsx`
- Newsletter form page: `src/frontend/kitchenrecipes-ui/src/pages/newsletter/NewsletterFormPage.tsx`
- Newsletter API client: `src/frontend/kitchenrecipes-ui/src/services/newsletterApi.ts`
- SMTP sender: `src/backend/KitchenRecipes.Infrastructure/Services/SmtpNewsletterSender.cs`
- Newsletter reporting service: `src/backend/KitchenRecipes.Infrastructure/Services/NewsletterReportingService.cs`
- Account page: `src/frontend/kitchenrecipes-ui/src/pages/account/AccountPage.tsx`
- Account API client: `src/frontend/kitchenrecipes-ui/src/services/accountApi.ts`
- Tailwind config: `src/frontend/kitchenrecipes-ui/tailwind.config.js`
- Global text dictionary: `src/frontend/kitchenrecipes-ui/src/locales/en.json`
- Setup guide: `setup.md`

## What Is Next (Step 21)

1. Persist assistant profile preference per user (database-backed profile selection).
2. Add richer site-context retrieval for non-cooking assistant requests.
3. Add assistant usage analytics and admin monitoring dashboard.

We will continue step by step, keeping code simple, scalable, and maintainable.
