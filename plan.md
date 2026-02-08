# Grandparents Calendar & Grocery List App

## Project Overview
A mobile-first Blazor WebAssembly PWA designed for elderly users (grandparents) to easily manage:
- **Shared Calendar** - Events assigned to configurable "People" (e.g., Grandpa, Grandma, Both)
- **Grocery List** - Shared shopping list management

**🎯 Core Principle: SIMPLICITY IS THE ULTIMATE GOAL**

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Blazor WebAssembly |
| UI Framework | Radzen Blazor Components |
| Backend | ASP.NET Core Web API |
| Database | PostgreSQL (containerized) |
| ORM | Entity Framework Core |
| Migrations | EF Core Migrations |
| PWA | Blazor PWA Template |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt |
| Containerization | Docker & Docker Compose |
| Target Device | 📱 iPhone (primary) |

## ✅ Resolved Decisions

| Decision | Answer |
|----------|--------|
| Docker installed? | ✅ Yes |
| HTTPS | HTTP for dev, HTTPS for prod |
| Offline capability | ❌ No - requires internet |
| Target device | 📱 iPhone (primary) |
| Login model | Single shared account |
| People management | User can add/edit "People" for calendar assignment |
| Password requirements | Minimum 8 characters, no complexity |
| Auth method | JWT tokens |
| Design philosophy | Simple, warm, elderly-friendly colors |

## Key Requirements

### 1. Authentication
- [ ] Single account login (username/password)
- [ ] Password stored as BCrypt hash
- [ ] Minimum 8 characters, no complexity rules
- [ ] JWT tokens for API authentication
- [ ] "Remember me" option (long-lived token)
- [ ] Simple logout functionality
- [ ] Large login form inputs (elderly-friendly)

### 2. Mobile-First / PWA
- [ ] iPhone primary target
- [ ] Installable on home screen
- [ ] Large, touch-friendly buttons (minimum 48x48px)
- [ ] High contrast / readable fonts
- [ ] Simple, uncluttered UI
- [ ] Bottom navigation bar

### 3. Calendar Features
- [ ] Monthly view (primary), Week/Day optional
- [ ] "People" management (add/edit people like "Grandpa", "Grandma")
- [ ] Event creation with person assignment:
  - Select one person
  - Select multiple people (joint event)
- [ ] Color coding per person
- [ ] Simple event details (title, date, time, person)

### 4. Grocery List Features
- [ ] Add/Remove items
- [ ] Check off items when purchased
- [ ] Shared list (single account sees everything)
- [ ] Preset categories (Produce, Dairy, Meat, etc.)
- [ ] Simple and fast to use

### 5. Database & Migrations
- [ ] PostgreSQL in Docker
- [ ] EF Core Code-First approach
- [ ] Migration support for schema changes
- [ ] Seed data for initial setup (default categories)

## Proposed Database Schema

```
┌─────────────────┐
│      User       │ (Single account)
├─────────────────┤
│ Id (PK)         │
│ Username        │
│ PasswordHash    │
│ CreatedAt       │
│ LastLoginAt     │
└─────────────────┘
        │
        │ (User creates People)
        ▼
┌─────────────────┐     ┌─────────────────────┐
│     Person      │     │    CalendarEvent    │
├─────────────────┤     ├─────────────────────┤
│ Id (PK)         │◄───┐│ Id (PK)             │
│ UserId (FK)     │    ││ UserId (FK)         │
│ Name            │    ││ Title               │
│ Color           │    ││ Description         │
│ SortOrder       │    ││ StartDate           │
│ CreatedAt       │    ││ EndDate             │
└─────────────────┘    ││ IsAllDay            │
                       ││ CreatedAt           │
                       │└─────────────────────┘
                       │
                       │  ┌─────────────────────┐
                       │  │ EventPerson (Join)  │
                       │  ├─────────────────────┤
                       └──│ EventId (FK)        │
                          │ PersonId (FK)       │
                          └─────────────────────┘

┌─────────────────┐     ┌─────────────────┐
│ GroceryCategory │     │   GroceryItem   │
├─────────────────┤     ├─────────────────┤
│ Id (PK)         │◄────│ Id (PK)         │
│ Name            │     │ UserId (FK)     │
│ SortOrder       │     │ CategoryId (FK) │
│ IsDefault       │     │ Name            │
└─────────────────┘     │ IsChecked       │
                        │ Quantity        │
                        │ CreatedAt       │
                        │ CheckedAt       │
                        └─────────────────┘
```

### Authentication Flow

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   Login      │    │   API        │    │   Database   │
│   Page       │───►│   /auth      │───►│   Users      │
└──────────────┘    └──────────────┘    └──────────────┘
       │                   │
       │                   ▼
       │            ┌──────────────┐
       │            │ Verify Hash  │
       │            │ (BCrypt)     │
       │            └──────────────┘
       │                   │
       │                   ▼
       │            ┌──────────────┐
       ◄────────────│ Return JWT   │
                    └──────────────┘
```

### Password Security Notes
- **Hashing Algorithm**: BCrypt (recommended) or PBKDF2 via ASP.NET Core Identity
- **Never store plaintext passwords**
- **Salt is automatically handled by BCrypt**
- **Minimum password length**: TBD (keep simple for grandparents - maybe 6 chars?)

## UI/UX Design Philosophy

### 🎨 Color Palette (Warm, Elderly-Friendly)

| Element | Color | Hex | Notes |
|---------|-------|-----|-------|
| Primary | Warm Blue | `#4A6FA5` | Trustworthy, calm |
| Secondary | Soft Sage | `#7BA seventeen` | Easy on eyes |
| Background | Warm White | `#FAF9F6` | Not harsh white |
| Text | Dark Charcoal | `#2D3436` | High contrast |
| Success | Soft Green | `#6BBF59` | Checked items |
| Danger | Muted Red | `#E17055` | Delete actions |
| Person A | Warm Blue | `#4A6FA5` | Calendar color |
| Person B | Soft Rose | `#D4A5A5` | Calendar color |
| Joint | Soft Purple | `#9B89B3` | Both people |

### 📐 Design Principles

- **Large Touch Targets**: Minimum 48x48px, prefer 56x56px
- **Font Size**: Base 18px, headers 24px+
- **Spacing**: Generous padding, no cramped layouts
- **Icons**: Always paired with text labels
- **Contrast**: WCAG AA minimum (4.5:1 ratio)
- **Buttons**: Full-width on mobile, clear labels
- **Navigation**: Bottom tab bar (3-4 items max)
  - 📅 Calendar
  - 🛒 Grocery
  - ⚙️ Settings

### 📱 iPhone-Specific Considerations

- [ ] Safe area insets (notch, home indicator)
- [ ] PWA splash screen
- [ ] App icon (180x180 for iPhone)
- [ ] Status bar styling
- [ ] Pull-to-refresh patterns
- [ ] iOS-style haptic feedback consideration

## Docker Architecture

### Container Overview

```
┌─────────────────────────────────────────────────────────┐
│                    Docker Compose                        │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────┐   ┌──────────────┐   ┌──────────────┐ │
│  │   nginx      │   │   app        │   │   postgres   │ │
│  │   (proxy)    │──►│   (API +     │──►│   (database) │ │
│  │   :80/:443   │   │   Blazor)    │   │   :5432      │ │
│  └──────────────┘   │   :8080      │   └──────────────┘ │
│         │           └──────────────┘          │         │
│         │                                     │         │
│         ▼                                     ▼         │
│  ┌──────────────┐                    ┌──────────────┐   │
│  │   External   │                    │   Volume     │   │
│  │   Port 80    │                    │   pgdata     │   │
│  └──────────────┘                    └──────────────┘   │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### Docker Services

| Service | Image | Purpose | Port |
|---------|-------|---------|------|
| `postgres` | postgres:16-alpine | Database | 5432 (internal) |
| `app` | Custom .NET 8 | API + Blazor WASM | 8080 (internal) |
| `nginx` | nginx:alpine | Reverse proxy, SSL | 80, 443 (external) |

### Volumes & Persistence

| Volume | Purpose |
|--------|---------|
| `pgdata` | PostgreSQL data persistence |
| `nginx-certs` | SSL certificates (if needed) |

### Environment Variables

```
# PostgreSQL
POSTGRES_USER=gparents
POSTGRES_PASSWORD=<secure_password>
POSTGRES_DB=gparents_db

# Application
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=postgres;Database=gparents_db;Username=gparents;Password=<secure_password>
JWT__Secret=<jwt_secret_key>
JWT__ExpiryDays=30
```

## Project Structure (Proposed)

```
gparentsServer/
├── src/
│   ├── GParents.Web/           # Blazor WASM Client
│   │   ├── Pages/
│   │   ├── Components/
│   │   ├── Services/
│   │   └── wwwroot/
│   ├── GParents.API/           # ASP.NET Core API
│   │   ├── Controllers/
│   │   ├── Dockerfile          # API Dockerfile
│   │   └── Program.cs
│   ├── GParents.Core/          # Domain/Business Logic
│   │   ├── Entities/
│   │   └── Interfaces/
│   └── GParents.Infrastructure/# Data Access
│       ├── Data/
│       ├── Migrations/
│       └── Repositories/
├── docker/
│   ├── nginx/
│   │   └── nginx.conf          # Reverse proxy config
│   └── postgres/
│       └── init.sql            # Initial DB setup (optional)
├── tests/
├── .env.example                # Environment template
├── .env                        # Local env (gitignored)
├── docker-compose.yml          # Development compose
├── docker-compose.prod.yml     # Production compose
├── Dockerfile                  # Multi-stage app build
└── plan.md
```

## Docker Compose Configuration (Planned)

### Development (`docker-compose.yml`)
- Hot reload enabled
- PostgreSQL exposed on localhost:5432 for tooling
- No SSL/nginx (direct access to app) ✅ HTTP for dev

### Production (`docker-compose.prod.yml`)
- Optimized builds
- Nginx reverse proxy with HTTPS ✅
- PostgreSQL internal only
- Health checks enabled
- Restart policies

### Key Docker Features
- [ ] Multi-stage Dockerfile for smaller images
- [ ] Health checks for all services
- [ ] Named volumes for data persistence
- [ ] Network isolation (internal network for db)
- [ ] Environment variable management via `.env`
- [ ] Automatic database migrations on startup

## Database Migration Strategy (Docker)

```bash
# Migrations run automatically on app startup OR:

# Manual migration commands
docker-compose exec app dotnet ef migrations add <MigrationName>
docker-compose exec app dotnet ef database update

# Backup database
docker-compose exec postgres pg_dump -U gparents gparents_db > backup.sql

# Restore database
docker-compose exec -T postgres psql -U gparents gparents_db < backup.sql
```

---

## 🚨 Remaining Gaps

### Medium Priority (Feature Scope)

| Gap | Category | Notes |
|-----|----------|-------|
| Recurring events needed? | Calendar | Doctor appts, birthdays, etc. |
| Reminders/notifications? | Calendar | Push notifications complex on PWA |
| Past events handling | Calendar | Auto-hide? Archive? Keep visible? |
| Checked items behavior | Grocery | Disappear or stay until cleared? |
| Favorites/quick-add? | Grocery | "Buy again" functionality? |

---

## Remaining Questions

Just a few left:

1. **🔄 Recurring events?** Do they need repeating events?
   - Weekly (church, trash day)
   - Monthly (doctor visits)  
   - Yearly (birthdays)
   - Or skip for V1?

2. **🛒 Checked grocery items** - when checked off:
   - A) Disappear immediately
   - B) Stay visible (greyed out) until manually cleared
   - C) Move to bottom of list

3. **📅 Past events** - what happens to old events?
   - A) Stay visible forever
   - B) Auto-hide after X days
   - C) Manual archive/delete only

4. **🔔 Reminders?** Do they need notifications before events? (Can skip for V1 - adds complexity)

---

## Assumptions for Remaining Gaps

If you want to proceed, I'll use these defaults:

| Gap | Default |
|-----|---------|
| Recurring events | Skip for V1, add later |
| Checked items | Move to bottom, clear all button |
| Past events | Stay visible, manual delete |
| Reminders | Skip for V1 |

---

## Next Steps (Ready to Start)

1. [x] Finalize decisions ✅
2. [ ] Set up solution structure
3. [ ] Create Dockerfile (multi-stage build)
4. [ ] Create docker-compose.yml
5. [ ] Set up PostgreSQL + EF Core
6. [ ] Create initial migrations
7. [ ] Build API endpoints
8. [ ] Implement Blazor PWA with Radzen
9. [ ] Mobile-first styling (iPhone)
10. [ ] Test on iPhone
11. [ ] Deploy

---

*Last Updated: Planning Phase - Major Decisions Resolved*
