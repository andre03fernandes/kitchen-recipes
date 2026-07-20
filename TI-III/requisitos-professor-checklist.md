# Checklist de Requisitos do Professor (Sem Fotos)

Este ficheiro resume, de forma objetiva, onde cada requisito está implementado no projeto.

## 1. Aplicação ASP.NET (C#)
- Estado: Cumprido
- Evidência principal:
- Backend Web API em ASP.NET Core: [src/backend/KitchenRecipes.Web/Program.cs](src/backend/KitchenRecipes.Web/Program.cs)
- Solução .NET: [KitchenRecipes.sln](KitchenRecipes.sln)

## 2. Estrutura MVC com operação CRUD (mínimo 3 tabelas)
- Estado: Cumprido
- Tabelas/entidades (mais de 3):
- [src/backend/KitchenRecipes.Infrastructure/Data/ApplicationDbContext.cs](src/backend/KitchenRecipes.Infrastructure/Data/ApplicationDbContext.cs)
- CRUD 1 (Receitas):
- Controller: [src/backend/KitchenRecipes.Web/Controllers/Api/RecipesController.cs](src/backend/KitchenRecipes.Web/Controllers/Api/RecipesController.cs)
- Serviço: [src/backend/KitchenRecipes.Application/Services/RecipeService.cs](src/backend/KitchenRecipes.Application/Services/RecipeService.cs)
- CRUD 2 (Pantry):
- Controller: [src/backend/KitchenRecipes.Web/Controllers/Api/PantryItemsController.cs](src/backend/KitchenRecipes.Web/Controllers/Api/PantryItemsController.cs)
- Serviço: [src/backend/KitchenRecipes.Application/Services/PantryItemService.cs](src/backend/KitchenRecipes.Application/Services/PantryItemService.cs)
- CRUD 3 (Newsletter Subscribers):
- Controller: [src/backend/KitchenRecipes.Web/Controllers/Api/NewsletterSubscribersController.cs](src/backend/KitchenRecipes.Web/Controllers/Api/NewsletterSubscribersController.cs)
- Serviço: [src/backend/KitchenRecipes.Application/Services/NewsletterSubscriberService.cs](src/backend/KitchenRecipes.Application/Services/NewsletterSubscriberService.cs)

## 3. Base de dados SQL Server (com Triggers / Procedures)
- Estado: Cumprido
- Trigger de auditoria de mudança de role:
- Migration SQL: [src/backend/KitchenRecipes.Infrastructure/Data/Migrations/20260707221619_AddNewsletterSqlObjects.cs](src/backend/KitchenRecipes.Infrastructure/Data/Migrations/20260707221619_AddNewsletterSqlObjects.cs)
- Procedure para estatísticas de campanha:
- Migration SQL: [src/backend/KitchenRecipes.Infrastructure/Data/Migrations/20260707221619_AddNewsletterSqlObjects.cs](src/backend/KitchenRecipes.Infrastructure/Data/Migrations/20260707221619_AddNewsletterSqlObjects.cs)
- Uso da stored procedure na aplicação:
- [src/backend/KitchenRecipes.Infrastructure/Services/NewsletterReportingService.cs](src/backend/KitchenRecipes.Infrastructure/Services/NewsletterReportingService.cs)

## 4. Chamadas assíncronas no acesso aos dados do servidor
- Estado: Cumprido
- Evidências de acesso async a dados:
- [src/backend/KitchenRecipes.Application/Services/AccountService.cs](src/backend/KitchenRecipes.Application/Services/AccountService.cs)
- [src/backend/KitchenRecipes.Application/Services/RecipeService.cs](src/backend/KitchenRecipes.Application/Services/RecipeService.cs)
- [src/backend/KitchenRecipes.Application/Services/PantryItemService.cs](src/backend/KitchenRecipes.Application/Services/PantryItemService.cs)
- [src/backend/KitchenRecipes.Infrastructure/Services/NewsletterReportingService.cs](src/backend/KitchenRecipes.Infrastructure/Services/NewsletterReportingService.cs)

## 5. Contas de utilizador
- Estado: Cumprido
- Endpoints de conta e autenticação:
- [src/backend/KitchenRecipes.Web/Controllers/Api/AccountController.cs](src/backend/KitchenRecipes.Web/Controllers/Api/AccountController.cs)
- Lógica de password hash, login, roles, soft-delete e reativação:
- [src/backend/KitchenRecipes.Application/Services/AccountService.cs](src/backend/KitchenRecipes.Application/Services/AccountService.cs)
- UI de conta no frontend:
- [src/frontend/kitchenrecipes-ui/src/pages/account/AccountPage.tsx](src/frontend/kitchenrecipes-ui/src/pages/account/AccountPage.tsx)

## 6. PDF, Excel e Word
- Estado: Cumprido
- Ferramentas de exportação:
- [src/frontend/kitchenrecipes-ui/src/utils/tableTools.ts](src/frontend/kitchenrecipes-ui/src/utils/tableTools.ts)
- Uso nas páginas:
- [src/frontend/kitchenrecipes-ui/src/pages/recipes/RecipeListPage.tsx](src/frontend/kitchenrecipes-ui/src/pages/recipes/RecipeListPage.tsx)
- [src/frontend/kitchenrecipes-ui/src/pages/pantry/PantryListPage.tsx](src/frontend/kitchenrecipes-ui/src/pages/pantry/PantryListPage.tsx)
- [src/frontend/kitchenrecipes-ui/src/pages/newsletter/NewsletterListPage.tsx](src/frontend/kitchenrecipes-ui/src/pages/newsletter/NewsletterListPage.tsx)
- [src/frontend/kitchenrecipes-ui/src/pages/account/AccountPage.tsx](src/frontend/kitchenrecipes-ui/src/pages/account/AccountPage.tsx)

## 7. Envio de Email
- Estado: Cumprido
- Serviço SMTP:
- [src/backend/KitchenRecipes.Infrastructure/Services/SmtpNewsletterSender.cs](src/backend/KitchenRecipes.Infrastructure/Services/SmtpNewsletterSender.cs)
- Configuração SMTP:
- [src/backend/KitchenRecipes.Web/appsettings.Development.json](src/backend/KitchenRecipes.Web/appsettings.Development.json)

## 8. Observação para demonstração
- Se o professor pedir prova rápida, mostrar:
- 1) Login no módulo de conta
- 2) Um CRUD completo (ex.: Recipes)
- 3) Query de stats de newsletter (usa stored procedure)
- 4) Export para PDF/Excel/Word numa tabela
- 5) Envio de campanha newsletter (SMTP)

## 9. Estado final
- Resultado: Projeto cobre os requisitos principais e extras do enunciado apresentado.
