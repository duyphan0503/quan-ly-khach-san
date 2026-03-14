# REPOSITORY KNOWLEDGE BASE

**Framework:** EF Core 10.0 (High Performance Focus)

## OVERVIEW
Decouples data access from business logic. Ensures strict SQL translation and query optimization.

## STRUCTURE
```
Repositories/
├── Interfaces/          # Abstractions (e.g., IBookingRepository.cs)
└── [Name]Repository     # Implementations
```

## WHERE TO LOOK
| Domain Area | Implementations | Key Feature |
|-------------|-----------------|-------------|
| Bookings | `BookingRepository` | Complex availability SQL |
| Rooms | `RoomRepository` | Grouping by RoomType |
| Guests | `GuestRepository` | Unique index on CCCD |

## CONVENTIONS
- **Read-Only**: Always use `.AsNoTracking()` for read operations.
- **Async Suffix**: All methods MUST be `async` and return `Task`.
- **Eager Loading**: Use `.Include()` explicitly; no lazy loading allowed.
- **IQueryable**: NEVER return `IQueryable` from Repository methods; return `List<T>`.

## ANTI-PATTERNS
- ❌ **Calculation Logic**: No tax/price calculation here; keep in Services.
- ❌ **Client-Side Filtering**: Do not use `.ToList()` before `.Where()`.
- ❌ **Cross-Injection**: Repositories should not inject other repositories.

