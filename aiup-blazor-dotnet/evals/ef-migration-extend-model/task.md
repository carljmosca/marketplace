# Extend the Data Model for Order Management (UC-020)

## Problem Description

The ShopCatalog team is preparing UC-020 "Place Order". The entity model at `docs/entity_model.md`
has been extended with two new entities, `Customer` and `Order`, alongside the existing `Product`.

The project is a .NET 10 Blazor application using EF Core with PostgreSQL. The `Product` entity is
already mapped (`Domain/Product.cs`, `Data/ProductConfiguration.cs`) and registered in
`Data/AppDbContext.cs`.

## Output Specification

Bring the EF Core model in line with `docs/entity_model.md`:

1. **Entity classes** for `Customer` and `Order` under `Domain/`, honoring every data type,
   length/precision, default, and relationship in the entity model.
2. **EF Core configurations** for both entities under `Data/`, and whatever `AppDbContext` changes
   are needed so the new entities are part of the model.
3. **Migration**: this environment has no .NET SDK installed, so do **not** try to execute the EF
   Core CLI. Instead, finish by writing out the exact migration command the team should run,
   with the migration name the ef-migration conventions call for.

Do **not** create any test files, and do **not** put database credentials anywhere in source code.

This session's conversation is not kept. Append every message you write to the user, word for word and in order, to
`session-log.md` in the root of the working directory; that log is not part of the project.
