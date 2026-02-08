# Family Calendar & Grocery List

A mobile-first PWA for managing a shared family calendar and grocery list. Built with elderly users in mind — large touch targets, simple navigation, and a warm, readable design.

## Tech Stack

- **Frontend:** Blazor WebAssembly (.NET 8), Radzen Blazor Components
- **Backend:** ASP.NET Core Web API (.NET 8)
- **Database:** PostgreSQL 16
- **Auth:** JWT + BCrypt
- **Deployment:** Docker Compose

## Quick Start

```bash
# Clone and run
docker-compose up --build

# Access at http://localhost:5000
```

On first visit you'll see a registration page. After creating an account, you're redirected to the calendar.

## Features

### Calendar
- Monthly view with Radzen Scheduler
- Create events with title, description, date/time
- Assign events to people (color-coded)
- Recurring events: weekly, monthly, yearly
- Edit/delete affects the whole series

### Grocery List
- Add items to categorized lists (Produce, Dairy, Meat, Bakery, etc.)
- Check off items — they move to the bottom
- "Clear Checked" button to remove purchased items

### Settings
- Manage people (name + color) for calendar assignment
- Sign out

### PWA
- Installable on iPhone home screen
- Standalone app experience with safe area support
- Bottom navigation: Calendar | Grocery | Settings

## Project Structure

```
src/
├── GParents.Server/    # API, EF Core, auth
├── GParents.Client/    # Blazor WASM PWA
└── GParents.Shared/    # DTOs shared between server and client
```

## Configuration

Copy `.env.example` to `.env` and update the values:

```bash
cp .env.example .env
```

Key environment variables:

| Variable | Purpose |
|----------|---------|
| `POSTGRES_PASSWORD` | Database password |
| `JWT__Secret` | JWT signing key (32+ characters) |
| `JWT__ExpiryDays` | Token lifetime for "remember me" |

## Production

```bash
docker-compose -f docker-compose.prod.yml up --build
```

This adds an nginx reverse proxy on port 80. Configure SSL/DNS as needed for your setup.

## Development

Without Docker, you'll need PostgreSQL running locally and the connection string in `src/GParents.Server/appsettings.json`:

```bash
dotnet build GParents.sln
dotnet run --project src/GParents.Server
```

The database auto-migrates on startup.
