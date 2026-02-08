# Grandparents Calendar & Grocery List App

## Project Overview
A mobile-first Blazor WebAssembly PWA designed for elderly users (grandparents) to easily manage:
- **Shared Calendar** - Events assigned to configurable "People" (e.g., Grandpa, Grandma, Both)
- **Grocery List** - Shared shopping list management

**Core Principle: SIMPLICITY IS THE ULTIMATE GOAL**

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Blazor WebAssembly (.NET 8) |
| UI Framework | Radzen Blazor Components |
| Backend | ASP.NET Core Web API (.NET 8) |
| Database | PostgreSQL 16 (containerized) |
| ORM | Entity Framework Core 8 |
| PWA | Blazor PWA Template |
| Authentication | JWT Bearer Tokens |
| Password Hashing | BCrypt |
| Containerization | Docker & Docker Compose |
| Target Device | iPhone (primary) |

## Resolved Decisions

| Decision | Answer |
|----------|--------|
| .NET Version | .NET 8 |
| Project Structure | 3 projects: Server, Client, Shared |
| Login model | Single shared account |
| Registration | One-time page (disabled after first account) |
| Recurring events | V1: Weekly, Monthly, Yearly (whole series edit/delete only) |
| Checked grocery items | Move to bottom; "Clear all checked" button |
| Past events | Stay visible, manual delete |
| Reminders | Skip for V1 |
| Offline capability | No - requires internet |
| Password requirements | Minimum 8 characters, no complexity |

## Project Structure

```
grandparentsApp/
├── GParents.sln
├── plan.md
├── .gitignore
├── .env.example
├── docker-compose.yml
├── docker-compose.prod.yml
├── Dockerfile
├── docker/nginx/nginx.conf
└── src/
    ├── GParents.Server/
    │   ├── Program.cs
    │   ├── appsettings.json
    │   ├── Controllers/
    │   │   ├── AuthController.cs
    │   │   ├── PeopleController.cs
    │   │   ├── EventsController.cs
    │   │   └── GroceryController.cs
    │   ├── Data/
    │   │   ├── AppDbContext.cs
    │   │   └── Migrations/
    │   ├── Entities/
    │   │   ├── User.cs
    │   │   ├── Person.cs
    │   │   ├── CalendarEvent.cs
    │   │   ├── EventPerson.cs
    │   │   ├── GroceryCategory.cs
    │   │   └── GroceryItem.cs
    │   └── Services/
    │       ├── AuthService.cs
    │       └── TokenService.cs
    ├── GParents.Client/
    │   ├── Program.cs
    │   ├── App.razor
    │   ├── _Imports.razor
    │   ├── Layout/
    │   │   ├── MainLayout.razor
    │   │   └── BottomNav.razor
    │   ├── Pages/
    │   │   ├── Login.razor
    │   │   ├── Register.razor
    │   │   ├── Calendar.razor
    │   │   ├── Grocery.razor
    │   │   └── Settings.razor
    │   ├── Components/
    │   │   ├── EventDialog.razor
    │   │   ├── PersonManager.razor
    │   │   └── RedirectToLogin.razor
    │   ├── Services/
    │   │   ├── ApiService.cs
    │   │   ├── AuthService.cs
    │   │   └── CustomAuthStateProvider.cs
    │   └── wwwroot/
    │       ├── index.html
    │       ├── css/app.css
    │       ├── manifest.webmanifest
    │       ├── service-worker.js
    │       ├── icon-180.png
    │       ├── icon-192.png
    │       └── icon-512.png
    └── GParents.Shared/
        └── DTOs/
            ├── AuthDtos.cs
            ├── PersonDto.cs
            ├── CalendarEventDto.cs
            └── GroceryDtos.cs
```

## Database Schema

Six entities with EF Core Code-First + auto-migrate on startup:

- **User** - Id, Username, PasswordHash, CreatedAt, LastLoginAt
- **Person** - Id, UserId(FK), Name, Color, SortOrder, CreatedAt
- **CalendarEvent** - Id, UserId(FK), Title, Description, StartDate, EndDate, IsAllDay, RecurrenceType(None/Weekly/Monthly/Yearly), RecurrenceEndDate, CreatedAt
- **EventPerson** - EventId(FK), PersonId(FK) (composite PK, join table)
- **GroceryCategory** - Id, Name, SortOrder, IsDefault (seeded: Produce, Dairy, Meat & Seafood, Bakery, Frozen, Pantry, Beverages, Snacks, Household, Other)
- **GroceryItem** - Id, UserId(FK), CategoryId(FK), Name, IsChecked, Quantity, CreatedAt, CheckedAt

## API Endpoints

### Auth (no auth required)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/auth/status` | Returns `{ accountExists: bool }` |
| POST | `/api/auth/register` | Create account (409 if exists) |
| POST | `/api/auth/login` | Login, returns JWT |

### People (auth required)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/people` | List all people |
| POST | `/api/people` | Create person |
| PUT | `/api/people/{id}` | Update person |
| DELETE | `/api/people/{id}` | Delete person |

### Events (auth required)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/events?start=&end=` | Get events in range (expands recurring) |
| GET | `/api/events/{id}` | Get single event source record |
| POST | `/api/events` | Create event |
| PUT | `/api/events/{id}` | Update event (whole series) |
| DELETE | `/api/events/{id}` | Delete event (whole series) |

### Grocery (auth required)
| Method | Route | Purpose |
|--------|-------|---------|
| GET | `/api/grocery/categories` | Categories with items (unchecked first) |
| POST | `/api/grocery/items` | Add item |
| PUT | `/api/grocery/items/{id}` | Update item |
| DELETE | `/api/grocery/items/{id}` | Delete item |
| DELETE | `/api/grocery/items/checked` | Clear all checked items |

## Running the App

```bash
# Development with Docker
docker-compose up --build

# Access at http://localhost:5000
# First visit shows Register page (no account yet)
# After registration, redirects to Calendar
```

## Recurring Events Strategy

- Store one record per recurring event with RecurrenceType and optional RecurrenceEndDate
- Server expands occurrences at query time within the requested date range
- Weekly: +7 days, Monthly: +1 month, Yearly: +1 year
- If no end date, cap expansion at 2 years out
- Edit/delete always affects the whole series

## Styling

- CSS custom properties: Primary #4A6FA5, Background #FAF9F6, Text #2D3436
- Base font 18px, touch targets min 56px
- Radzen component overrides for elderly-friendly sizing
- iPhone safe area insets via env(safe-area-inset-*)
- Bottom nav fixed with home indicator padding
