# DATA LAYER KNOWLEDGE BASE

**Last Updated:** Sun Mar 08 2026

## OVERVIEW
SQL Server persistence using EF Core with Identity integration and performance-optimized seeding.

## KEY FILES
- **AppDbContext.cs**: Primary database context. Manages IdentityUser, DbSets, and Fluent API configurations.
- **SeedData.cs**: Static initializer for roles, default users, and initial hotel content.

## CONFIGURATION
- **Persistence**: SQL Server 2025 running in Docker.
- **Fluent API**: Configurations in `OnModelCreating` handle unique indexes and restricted delete behaviors (e.g., `DeleteBehavior.Restrict` for Bookings/Rooms).
- **Environment Hacks**: `Program.cs` contains manual SQL to add `AvatarUrl` column to `Guests` and sync it from `AspNetUsers` due to environment-specific migration limitations.
- **Language**: All seed data (Room Types, Service names) must be in **Vietnamese**.

## SEEDING RULES
- **Order**: Roles -> Users -> RoomTypes -> Rooms -> Services -> Guests.
- **Security**: Default passwords use `Hotel@123`.
- **Scope**: Seeding runs within `Program.cs` startup logic.
