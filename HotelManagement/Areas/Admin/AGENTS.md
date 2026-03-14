# ADMIN AREA KNOWLEDGE BASE

**Last Updated:** Sun Mar 08 2026
**Theme:** Liquid Glass (Exclusive Dark Mode)

## OVERVIEW
Management dashboard for hotel operations. High-density metrics and transaction-heavy workflows.

## STRUCTURE
```
Areas/Admin/Pages/
├── Dashboard/      # Revenue & Occupancy (Chart.js 4)
├── Bookings/       # Lifecycle management
├── Rooms/          # Inventory & Status
└── Shared/         # _AdminLayout.cshtml
```

## CONVENTIONS
- **Exclusive Dark**: Use `surface-1` and `backdrop-blur-2xl` for containers.
- **Glassmorphism**: Prefer `glass-card` and `backdrop-blur-xl` for content containers.
- **Design Tokens**: Use `.btn-luxury` for primary actions and `.text-gradient-accent` for headings.
- **Animations**: Apply `.animate-fade-up` to main page containers for smooth entry.
- **Form Layout**: For complex editing, use a `7/12 (Left) - 5/12 (Right)` grid ratio.
- **Iconography**: `iconify-icon` with `solar` bold-duotone set.
- **Language**: 100% Vietnamese.

## ANTI-PATTERNS
- ❌ **Light Mode**: Strictly forbidden in Admin area.
- ❌ **Raw UserManager**: Use Service wrappers for staff operations.
- ❌ **Bootstrap**: Usage will break the dark theme components.
