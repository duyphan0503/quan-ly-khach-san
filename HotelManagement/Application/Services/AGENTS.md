# SERVICES LAYER KNOWLEDGE BASE (HotelManagement.Application)

**Last Updated:** Sun Mar 08 2026

## OVERVIEW
The Services layer acts as the orchestrator of business logic, enforcing domain rules and managing transactions across repositories.

## STRUCTURE
```
Services/
├── Interfaces/    # Service abstractions (e.g., IBookingService.cs)
└── [Name]Service  # Core logic implementations (e.g., BookingService.cs)
```

## WHERE TO LOOK
| Type | Location | Implementation Detail |
|------|----------|-----------------------|
| **Validation** | `CreateAsync`/`UpdateAsync` | Date checks, room availability |
| **Pricing** | `BookingService.CreateAsync` | BasePrice * nights calculation |
| **Status Logic** | `UpdateStatusAsync` | Transitions between Confirmed/CheckedIn/etc. |
| **Transactions** | `await using var transaction` | Manual transaction handling in Services |

## CONVENTIONS
- **Result Tuples**: All mutations MUST return `Task<(bool Success, string Message)>`.
- **Async Suffix**: All methods MUST end in `Async`.
- **Validation**: Business rules (e.g., date overlaps) belong here, not in controllers/repos.
- **Localization**: Success/Error strings MUST be in **Vietnamese**.
- **Transaction Safety**: Use database transactions for multi-step updates (e.g., creating a booking + updating room status).

## ANTI-PATTERNS
- ❌ **UI Leakage**: NEVER reference `Microsoft.AspNetCore.Mvc` or `PageModel`.
- ❌ **DB Exposure**: NEVER inject `AppDbContext` where a Repository can be used (unless for transactions).
- ❌ **IQueryable Leak**: Always return concrete `List<T>` to ensure query execution.

