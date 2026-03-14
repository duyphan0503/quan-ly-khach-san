# IDENTITY AREA KNOWLEDGE BASE

**Target Area:** HotelManagement/Areas/Identity
**Scope:** Authentication, Authorization, and Profile Management

## OVERVIEW
Standard ASP.NET Core Identity pages overridden to match the "Liquid Glass" theme and Vietnamese localization requirements. Handles user registration, login, and secure profile management.

## STRUCTURE
```
Areas/Identity/Pages/Account/
├── Login.cshtml        # Entry point for staff/guests
├── Register.cshtml     # Membership creation
├── Manage/             # User self-service (Password, Email, Data)
└── Shared/             # Identity-specific layouts
```

## WHERE TO LOOK
| Task | Location | Notes |
|------|----------|-------|
| Login Logic | `Account/Login.cshtml.cs` | Cookie-based auth |
| Error Localization | `Infrastructure/Identity` | VietnameseIdentityErrorDescriber |
| Layout Styling | `Pages/Shared/_Layout.cshtml` | Liquid Glass integration |

## CONVENTIONS
- **Localization**: Error messages from Identity must be Vietnamese (via custom ErrorDescriber).
- **Redirection**: Default landing after login is determined in `Login.cshtml.cs` (usually `/Admin` for staff).
- **Theme Consistency**: Identity pages MUST use the global design tokens (`surface-1`, `backdrop-blur-2xl`).

## ANTI-PATTERNS
- ❌ **Insecure Defaults**: Never disable account lockout or password complexity.
- ❌ **Custom Auth Logic**: Use `SignInManager` and `UserManager` instead of rolling custom auth.
- ❌ **Bootstrap Scaffolding**: Identity default Bootstrap classes MUST be replaced with Tailwind.
