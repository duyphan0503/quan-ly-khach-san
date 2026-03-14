# DOMAIN MODELS KNOWLEDGE BASE

**Last Updated:** Sun Mar 08 2026

## OVERVIEW
Core domain entities (POCOs) with DataAnnotations for validation and schema mapping. No external dependencies.

## KEY ENTITIES
- `ApplicationUser`: Custom identity user with `FullName`.
- `Guest`: Nationality defaults to "Việt Nam".
- `Booking`: CheckOut must be > CheckIn (SQL Check Constraint applied).
- `Invoice`: One-to-one with Booking.

## CONVENTIONS
- **Language**: `[Display(Name = "...")]` labels must be in **Vietnamese**.
- **Precision**: Currency fields use `decimal(18,2)`.
- **Navigation**: Collection properties initialized to prevent null references.

## ANTI-PATTERNS
- ❌ **Complex Logic**: Models are data containers. logic belongs in Services.
- ❌ **Magic Numbers**: Use Enums for statuses and methods.
