# PUBLIC PORTAL KNOWLEDGE BASE

**Last Updated:** Sun Mar 08 2026
**Theme:** Liquid Glass (Light Theme Comfort)

## OVERVIEW
Digital storefront for guest booking. High-conversion, friction-free "Anonymous Booking" flow.

## STRUCTURE
```
Areas/Public/Pages/
├── Index.cshtml        # Hero & Featured
├── Rooms/              # Visual grid catalog
└── Booking/            # Request form
```

## CONVENTIONS
- **Liquid Glass Light**: `bg-base-100/80` with `backdrop-blur-2xl`.
- **Mobile-First**: Large touch targets (min 44px).
- **Minimalism**: Collect only HoTen, Email, SoDienThoai for initial booking.
- **Iconography**: `solar` bold-duotone for premium feel.

## ANTI-PATTERNS
- ❌ **Forced Login**: Never require login for booking requests.
- ❌ **Dark UI Leaks**: Strictly avoid Admin dark tokens here.
- ❌ **Bootstrap**: Usage forbidden; use DaisyUI/Tailwind only.
