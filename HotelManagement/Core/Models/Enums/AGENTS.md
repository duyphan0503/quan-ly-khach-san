# ENUMERATIONS KNOWLEDGE BASE

**Last Updated:** Sun Mar 08 2026

## OVERVIEW
System constants mapped to database integers. Enforced by Service layer switches.

## ENUM MAP
| Enum | Key States |
|------|------------|
| `RoomStatus` | `Available`, `Occupied`, `Maintenance`, `Reserved` |
| `BookingStatus` | `Confirmed`, `CheckedIn`, `CheckedOut`, `Cancelled`, `Pending` |
| `InvoiceStatus` | `Pending`, `Paid`, `Refunded` |

## USAGE RULES
- **No Logic**: Keep enums as pure value lists.
- **Switch Expressions**: Prefer `switch` expressions in UI/Services for mapping enums to Vietnamese labels.
- **Consistency**: Ensure UI dropdowns match enum names exactly.
