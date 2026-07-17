# Setup Guide (From Zero)

This document explains everything required to run the Kitchen Recipes project from scratch.

## 1. Required Software

Install the following tools:

- Visual Studio Code
- .NET SDK 8.0+
- Node.js 20+ (or newer LTS)
- npm (comes with Node.js)
- SQL Server (Developer or Express)
- SQL Server Management Studio (optional but recommended)
- Git

## 2. Optional VS Code Extensions

- C# Dev Kit
- ES7+ React/Redux snippets
- Tailwind CSS IntelliSense
- ESLint

## 3. Clone and Open Project

```powershell
git clone <your-repository-url>
cd TI-III
code .
```

## 4. Restore Backend Dependencies

```powershell
dotnet restore KitchenRecipes.sln
```

## 5. Install Frontend Dependencies

```powershell
cd src/frontend/kitchenrecipes-ui
npm install
cd ../../..
```

## 6. Configure Application Settings

Edit:

- `src/backend/KitchenRecipes.Web/appsettings.json`
- `src/backend/KitchenRecipes.Web/appsettings.Development.json`

Update values:

- `ConnectionStrings:DefaultConnection`
- `Smtp:SenderEmail`
- `Smtp:Username`
- `Smtp:AppPassword`
- `Ai:Enabled`
- `Ai:Provider`
- `Ai:BaseUrl`
- `Ai:ActiveProfile`
- `Ai:FastModel`
- `Ai:QualityModel`

### Gmail SMTP Notes

Use a Google App Password (not your normal password):

1. Enable 2-step verification on your Google account.
2. Generate an App Password from Google account security settings.
3. Put that value into `Smtp:AppPassword`.

### AI Assistant Provider Notes (Ollama)

For local free AI usage with real streaming, use Ollama:

1. Install and run Ollama locally.
2. Pull one or more models (for example fast and quality profiles).
3. Set `Ai:Enabled` to `true`.
4. Set `Ai:Provider` to `ollama`.
5. Ensure `Ai:BaseUrl` matches your local Ollama endpoint.

Recommended Windows commands:

```powershell
winget install --id Ollama.Ollama -e --accept-package-agreements --accept-source-agreements
ollama pull qwen2.5:3b
ollama pull qwen2.5:7b
ollama list
```

The default backend profile in this repository uses:

- `Ai:FastModel = qwen2.5:3b`
- `Ai:QualityModel = qwen2.5:7b`

## 7. Create Database (EF Core)

If EF CLI is not installed globally:

```powershell
dotnet tool install --global dotnet-ef
```

Create first migration and update DB:

```powershell
dotnet ef migrations add InitialCreate --project src/backend/KitchenRecipes.Infrastructure --startup-project src/backend/KitchenRecipes.Web --context ApplicationDbContext --output-dir Data/Migrations
dotnet ef database update --project src/backend/KitchenRecipes.Infrastructure --startup-project src/backend/KitchenRecipes.Web
```

## 8. Build Frontend for MVC Hosting

```powershell
cd src/frontend/kitchenrecipes-ui
npm run build
cd ../../..
```

This outputs static assets into:

- `src/backend/KitchenRecipes.Web/wwwroot/react`

## 9. Run Backend (MVC + API + Hosted React)

```powershell
dotnet run --project src/backend/KitchenRecipes.Web
```

Open the local URL shown in terminal (usually `https://localhost:7244`).

At startup, the application seeds default development data automatically.

Default login users and passwords are documented in:

- `default-users.md`

## 10. Useful Commands

### Backend Build

```powershell
dotnet build KitchenRecipes.sln
```

### Backend Tests

```powershell
dotnet test src/tests/KitchenRecipes.Tests/KitchenRecipes.Tests.csproj
```

### Frontend Lint

```powershell
cd src/frontend/kitchenrecipes-ui
npm run lint
```

### Frontend Dev Server (optional UI-only dev)

```powershell
cd src/frontend/kitchenrecipes-ui
npm run dev
```

## 11. Text and Localization Strategy

All current frontend text is centralized in:

- `src/frontend/kitchenrecipes-ui/src/locales/en.json`

The UI consumes values through:

- `src/frontend/kitchenrecipes-ui/src/i18n/text.ts`

When you want to rename UI text, edit only `en.json`.

## 12. Current Scope of Step 1

Completed:

- Scalable layered architecture
- React + TypeScript + Tailwind shell UI
- SQL Server-ready EF Core context
- Health endpoint
- SMTP settings structure for newsletter

Pending for next steps:

- AI assistant module

## 13. Implemented Recipes CRUD Endpoints

- `GET /api/recipes`
- `GET /api/recipes/{id}`
- `POST /api/recipes`
- `PUT /api/recipes/{id}`
- `DELETE /api/recipes/{id}`

These endpoints are consumed by the React pages under:

- `src/frontend/kitchenrecipes-ui/src/pages/recipes`

## 14. Implemented Pantry CRUD Endpoints

- `GET /api/pantry-items`
- `GET /api/pantry-items/{id}`
- `POST /api/pantry-items`
- `PUT /api/pantry-items/{id}`
- `DELETE /api/pantry-items/{id}`

These endpoints are consumed by the React pages under:

- `src/frontend/kitchenrecipes-ui/src/pages/pantry`

## 15. Implemented Newsletter Subscribers CRUD Endpoints

- `GET /api/newsletter-subscribers`
- `GET /api/newsletter-subscribers/{id}`
- `POST /api/newsletter-subscribers`
- `PUT /api/newsletter-subscribers/{id}`
- `DELETE /api/newsletter-subscribers/{id}`

These endpoints are consumed by the React pages under:

- `src/frontend/kitchenrecipes-ui/src/pages/newsletter`

## 16. Implemented Account and Roles Endpoints

- `POST /api/account/register`
- `POST /api/account/login`
- `POST /api/account/logout`
- `GET /api/account/me`
- `GET /api/account/users` (Admin only)
- `PUT /api/account/users/{id}/role` (Admin only)

Notes:

- The first registered account is automatically assigned the `Admin` role.
- Next registered accounts are assigned the `User` role.
- Role-based protection is enabled on CRUD APIs:
	- read endpoints require authenticated user
	- write endpoints require `Admin`

## 17. Implemented Newsletter Campaign Endpoints (SMTP)

- `GET /api/newsletter-campaigns/templates` (Admin only)
- `GET /api/newsletter-campaigns/stats` (Admin only)
- `GET /api/newsletter-campaigns/history?limit=20` (Admin only)
- `GET /api/newsletter-campaigns/history/export?limit=100` (Admin only)
- `POST /api/newsletter-campaigns/send` (Admin only)

Campaign sends to confirmed subscribers using SMTP settings from `Smtp` section.

## 18. Five Ready-to-Send Newsletter Templates

- `weekly_menu` - Weekly Menu Boost
- `pantry_rescue` - Pantry Rescue
- `seasonal_special` - Seasonal Special
- `healthy_starters` - Healthy Starters
- `community_challenge` - Community Challenge

These templates are available in backend campaign service and selectable from the newsletter UI page.

## 19. Implemented SQL Trigger and Stored Procedure

- Trigger: `trg_AppUsers_RoleChange_Audit`
	- Audits role changes into `AppUserRoleAudits`.
- Stored Procedure: `usp_GetNewsletterCampaignStats`
	- Returns total subscribers, confirmed subscribers, total campaigns sent, and last campaign timestamp.

Both objects are created by migration:

- `src/backend/KitchenRecipes.Infrastructure/Data/Migrations/20260707221619_AddNewsletterSqlObjects.cs`

## 20. Implemented Assistant Endpoints (Global)

- `GET /api/assistant/suggestions?limit=5` (Authenticated user)
- `POST /api/assistant/chat` (Authenticated user)
- `GET /api/assistant/threads` (Authenticated user)
- `GET /api/assistant/threads/{threadId}` (Authenticated user)
- `POST /api/assistant/threads/message` (Authenticated user)
- `POST /api/assistant/threads/message/stream` (Authenticated user)
- `PATCH /api/assistant/threads/{threadId}` (Authenticated user)
- `DELETE /api/assistant/threads/{threadId}` (Authenticated user)
- `GET /api/assistant/profile` (Authenticated user)
- `PUT /api/assistant/profile` (Authenticated user)

Notes:

- Assistant API is standardized under `/api/assistant/*`.

## 21. Default Development Data Seeding

- Seed runs on application startup from:
	- `src/backend/KitchenRecipes.Infrastructure/Data/DefaultDataSeeder.cs`
- Ensures at least 10 records across key tables for demo/testing.
- Includes Admin and User accounts for immediate login.

## 22. Automated Service Tests

- Test project:
	- `src/tests/KitchenRecipes.Tests`
- Current scope:
	- account service rules and credential validation
	- assistant suggestions
	- newsletter campaign send and audit flow

## 23. API Integration Tests

- Integration tests live under:
	- `src/tests/KitchenRecipes.Tests/Integration`
- Current API coverage includes:
	- account register + current user endpoint
	- admin authorization for recipe writes
	- recipe create and fetch flow
	- newsletter campaign send endpoint with audit persistence

## 24. Quantity-Aware Assistant (Pantry Context)

- Assistant scoring now considers:
	- ingredient name match
	- pantry quantity vs. recipe requirement
	- unit compatibility
- The UI now shows stock coverage and low-stock ingredients for each suggestion.

## 25. Filtered Newsletter History and Export

- Newsletter campaign history remains available from the newsletter page for admins.
- Current table UX in the frontend uses:
	- simple text filter
	- pagination
	- export actions (PDF, DOCX, Excel)
- Backend CSV export endpoint remains available for API usage.

## 26. Extended API Integration Coverage

- Integration tests now cover:
	- newsletter reporting stats endpoint
	- admin-only access protection for newsletter stats
	- role promotion flow from `User` to `Admin`
- The role promotion integration flow verifies that a promoted user can sign in again and perform an admin-only recipe write.

## 27. Conversational AI Assistant (Site-Wide)

- The assistant is now a full-page site assistant, not only pantry-focused.
- Frontend route:
	- `/assistant`
- Main quick access is provided through the floating `AI Assistant` launcher.
- Users can ask for:
	- recipe and pantry guidance
	- newsletter workflow guidance
	- account/admin usage guidance

The assistant page includes:

- persistent conversation history
- rename/delete thread actions
- live token streaming responses
- follow-up prompt suggestions
- model profile switch (`fast` / `quality`)

## 28. Unit Conversion Support for Assistant

- Assistant stock coverage now supports practical unit conversions for common kitchen measures.
- Current conversions include common weight and volume units such as:
	- `kg` and `g`
	- `l` and `ml`
	- `cup`, `tbsp`, and `tsp`
- The assistant UI now surfaces conversion notes so users can see when a suggestion depended on unit conversion.

## 29. Assistant Streaming and Profile Modes

- Assistant streaming now uses provider-native token streaming.
- The UI auto-scrolls while new tokens arrive.
- Model profile can be switched in the assistant UI:
	- `fast`
	- `quality`
