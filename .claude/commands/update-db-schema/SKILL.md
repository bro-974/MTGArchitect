---
name: update-db-schema
description: Regenerates docs/db.schema.md from the current EF Core model files in back/MTGArchitect.Data. Use when the user updates a model class, adds a new entity, changes a column, or asks to update the database schema documentation.
---

# Update DB Schema Documentation

Regenerate `docs/db.schema.md` from scratch by reading the current model files and DbContext configuration.

## Steps

1. **Read the source files** — read all of these files in full:
   - `back/MTGArchitect.Data/Models/*.cs`

2. **Extract the schema** from the source files:
   - For each entity: all properties, their CLR types, and any `[MaxLength]`, `[Required]`, or `[Column]` attributes
   - For each entity: Fluent API config in `OnModelCreating` (column types, max lengths, nullability, default values)
   - All foreign key relationships and their `DeleteBehavior`
   - Any new entities that appear as `DbSet<T>` properties

3. **Rewrite `docs/db.schema.md`** following this exact structure:

```
# Database Schema

[one paragraph: DB engine, ORM, number of custom entities, Identity note]

## ERD

[mermaid erDiagram block with all entities and relationships]

## Entities

### [EntityName]
[one line description if relevant, e.g. "Extends ASP.NET `IdentityUser`..."]

| Column | Type | Constraints | Notes |
...

[repeat for each entity]

---

## Relationships & Cascade Rules

[bullet list: one line per FK relationship, naming source → target, type, cascade behavior]
```

4. **Rules for the ERD block:**
   - Use `erDiagram` syntax
   - Show all FK relationships with correct cardinality (`||--o{` for one-to-many)
   - Include only columns that appear in the entity class (no inferred columns)
   - Use the actual SQL type names (uuid, text, varchar, int, bool) not C# types

5. **Rules for entity tables:**
   - `ApplicationUser`: list only custom properties (not IdentityUser inherited fields), add a note that it extends `IdentityUser`
   - All others: list every column
   - Constraints column: combine nullability + max length + default value (e.g. `not null, varchar(150)`)
   - Notes column: use for FK references, soft-delete flags, external IDs, or anything non-obvious

6. **Verify** the output is consistent: every FK shown in the ERD must appear in the Relationships section and in the entity table.
