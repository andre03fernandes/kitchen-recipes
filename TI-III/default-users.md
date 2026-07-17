# Default Users and Passwords (Development)

This file lists the current development users available in the local seeded database.

## Shared Password

All users listed below use the same development password:

- `P@ssw0rd123!`

## Users

| Id | Full Name | Email | Role | IsActive |
|---:|---|---|---|---|
| 1 | Admin Smoke | admin20260707232435@test.local | Admin | true |
| 2 | Andre Fernandes | andre2411fernandes@gmail.com | Admin | true |
| 3 | Platform Admin | admin@kitchenrecipes.local | Admin | true |
| 4 | Demo User 01 | demo.user01@kitchenrecipes.local | User | true |
| 5 | Demo User 02 | demo.user02@kitchenrecipes.local | User | true |
| 6 | Demo User 03 | demo.user03@kitchenrecipes.local | User | true |
| 7 | Demo User 04 | demo.user04@kitchenrecipes.local | User | true |
| 8 | Demo User 05 | demo.user05@kitchenrecipes.local | User | true |
| 9 | Demo User 06 | demo.user06@kitchenrecipes.local | User | true |
| 10 | Demo User 07 | demo.user07@kitchenrecipes.local | User | true |

## Notes

- This is for local development only.
- Users and counts are ensured by startup seeding logic in:
  - `src/backend/KitchenRecipes.Infrastructure/Data/DefaultDataSeeder.cs`
