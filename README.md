# Family Calendar & Grocery List

A mobile-first PWA for managing a shared family calendar and grocery list. Built with elderly users in mind — large touch targets, simple navigation, and a warm, readable design.

## Tech Stack

- **Frontend:** Blazor WebAssembly (.NET 8), Radzen Blazor Components
- **Backend:** ASP.NET Core Web API (.NET 8)
- **Database:** PostgreSQL 16
- **Auth:** JWT + BCrypt
- **Deployment:** Docker Compose + nginx (HTTPS)

## Features

### Calendar
- Month, week, and day views
- Click a day in month view to jump to that day's detail
- Create events from the "New Event" button or by selecting a time slot
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
- Installable on iPhone home screen ("Add to Home Screen")
- Standalone app experience with safe area support
- Bottom navigation: Calendar | Grocery | Settings

## Production Deployment

### Prerequisites

- A fresh Ubuntu server
- A domain name pointed at the server (A record)
- Port 80 and 443 open

### Install

```bash
git clone <your-repo-url> grandparentsApp
cd grandparentsApp
sudo bash install.sh
```

The install script handles everything on a fresh Ubuntu server:

1. Installs Docker and Docker Compose
2. Installs certbot
3. Generates secure database password and JWT secret
4. Obtains a Let's Encrypt SSL certificate for your domain
5. Sets up weekly automatic certificate renewal
6. Builds and starts the app

Your app will be available at `https://your-domain.com`.

### Managing the Production App

```bash
# View logs
docker compose -f docker-compose.prod.yml logs -f

# Restart
docker compose -f docker-compose.prod.yml restart

# Stop
docker compose -f docker-compose.prod.yml down

# Rebuild and restart (after pulling updates)
docker compose -f docker-compose.prod.yml up --build -d
```

## Local Development

### With Docker (recommended)

```bash
docker-compose up --build

# Access at http://localhost:5000
```

### Without Docker

Requires PostgreSQL running locally and the connection string configured in `src/GParents.Server/appsettings.json`:

```bash
dotnet build GParents.sln
dotnet run --project src/GParents.Server
```

The database auto-migrates on startup.

## Configuration

Environment variables (set in `.env` — generated automatically by `install.sh` in production):

| Variable | Purpose |
|----------|---------|
| `POSTGRES_USER` | Database username |
| `POSTGRES_PASSWORD` | Database password |
| `POSTGRES_DB` | Database name |
| `JWT__Secret` | JWT signing key (32+ characters) |
| `JWT__ExpiryDays` | Token lifetime for "remember me" (default: 30) |

For local development, copy the example and edit as needed:

```bash
cp .env.example .env
```

## Project Structure

```
grandparentsApp/
├── install.sh                 # Production install script
├── docker-compose.yml         # Dev: postgres + app on port 5000
├── docker-compose.prod.yml    # Prod: postgres + app + nginx with HTTPS
├── Dockerfile                 # Multi-stage .NET 8 build
├── docker/nginx/
│   ├── nginx.conf             # Dev nginx config
│   └── nginx.prod.conf        # Prod nginx config (HTTPS + redirect)
└── src/
    ├── GParents.Server/       # API, EF Core, auth, controllers
    ├── GParents.Client/       # Blazor WASM PWA
    └── GParents.Shared/       # Shared DTOs
```

## First Use

1. Open the app in a browser — the registration page appears (one-time setup)
2. Create a username and password (8+ characters)
3. You're redirected to the calendar
4. Go to Settings to add people (e.g. "Grandpa", "Grandma") with colors
5. Create events and assign them to people
6. Use the Grocery tab to manage your shopping list
7. On iPhone, tap Share > "Add to Home Screen" to install as a PWA
