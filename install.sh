#!/usr/bin/env bash
set -euo pipefail

# ============================================================
# Family Calendar & Grocery List — Production Install Script
# Run on a fresh Ubuntu server after cloning the repo.
# Usage: sudo bash install.sh
# ============================================================

APP_DIR="$(cd "$(dirname "$0")" && pwd)"
NGINX_SSL_DIR="$APP_DIR/docker/nginx/ssl"
CERTBOT_WWW_DIR="$APP_DIR/docker/certbot/www"
ENV_FILE="$APP_DIR/.env"
NGINX_PROD_CONF="$APP_DIR/docker/nginx/nginx.prod.conf"

# --- Colors ---
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

info()  { echo -e "${GREEN}[INFO]${NC} $1"; }
warn()  { echo -e "${YELLOW}[WARN]${NC} $1"; }
error() { echo -e "${RED}[ERROR]${NC} $1"; exit 1; }

# --- Must be root ---
if [[ $EUID -ne 0 ]]; then
    error "This script must be run as root (sudo bash install.sh)"
fi

echo ""
echo "=========================================="
echo " Family Calendar — Production Installer"
echo "=========================================="
echo ""

# ============================================================
# 1. Gather configuration
# ============================================================

read -rp "Enter your domain name (e.g. calendar.example.com): " DOMAIN
if [[ -z "$DOMAIN" ]]; then
    error "Domain name is required for HTTPS"
fi

read -rp "Enter an email for Let's Encrypt notifications: " LE_EMAIL
if [[ -z "$LE_EMAIL" ]]; then
    error "Email is required for Let's Encrypt"
fi

info "Domain: $DOMAIN"
info "Email:  $LE_EMAIL"
echo ""

# ============================================================
# 2. Install Docker if not present
# ============================================================

if command -v docker &>/dev/null; then
    info "Docker already installed: $(docker --version)"
else
    info "Installing Docker..."
    apt-get update -qq
    apt-get install -y -qq ca-certificates curl gnupg lsb-release

    install -m 0755 -d /etc/apt/keyrings
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
    chmod a+r /etc/apt/keyrings/docker.asc

    echo \
      "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] https://download.docker.com/linux/ubuntu \
      $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
      tee /etc/apt/sources.list.d/docker.list > /dev/null

    apt-get update -qq
    apt-get install -y -qq docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

    systemctl enable docker
    systemctl start docker
    info "Docker installed: $(docker --version)"
fi

# Verify docker compose
if ! docker compose version &>/dev/null; then
    error "docker compose plugin not found. Please install docker-compose-plugin."
fi
info "Docker Compose: $(docker compose version --short)"

# ============================================================
# 3. Install certbot if not present
# ============================================================

if command -v certbot &>/dev/null; then
    info "Certbot already installed"
else
    info "Installing certbot..."
    apt-get install -y -qq certbot
    info "Certbot installed"
fi

# ============================================================
# 4. Generate secrets and create .env
# ============================================================

generate_password() {
    openssl rand -base64 32 | tr -d '/+=' | head -c 32
}

if [[ -f "$ENV_FILE" ]]; then
    warn ".env file already exists — keeping existing configuration"
else
    info "Generating secure .env file..."
    PG_PASSWORD=$(generate_password)
    JWT_SECRET=$(generate_password)$(generate_password)

    cat > "$ENV_FILE" <<EOF
# PostgreSQL
POSTGRES_USER=gparents
POSTGRES_PASSWORD=$PG_PASSWORD
POSTGRES_DB=gparents_db

# Application
JWT__Secret=$JWT_SECRET
JWT__ExpiryDays=30
EOF

    chmod 600 "$ENV_FILE"
    info ".env file created with generated secrets"
fi

# ============================================================
# 5. Set domain in nginx config
# ============================================================

info "Configuring nginx for domain: $DOMAIN"
sed -i "s/DOMAIN_PLACEHOLDER/$DOMAIN/g" "$NGINX_PROD_CONF"

# ============================================================
# 6. Obtain SSL certificate
# ============================================================

mkdir -p "$NGINX_SSL_DIR" "$CERTBOT_WWW_DIR"

if [[ -f "$NGINX_SSL_DIR/fullchain.pem" && -f "$NGINX_SSL_DIR/privkey.pem" ]]; then
    warn "SSL certificates already exist — skipping"
else
    info "Obtaining SSL certificate from Let's Encrypt..."
    info "Make sure port 80 is open and $DOMAIN points to this server!"
    echo ""
    read -rp "Press Enter to continue (or Ctrl+C to abort)..."

    # Stop anything on port 80
    if lsof -i :80 &>/dev/null; then
        warn "Something is running on port 80 — attempting to stop..."
        fuser -k 80/tcp 2>/dev/null || true
        sleep 2
    fi

    certbot certonly \
        --standalone \
        --non-interactive \
        --agree-tos \
        --email "$LE_EMAIL" \
        -d "$DOMAIN"

    # Copy certs to nginx ssl dir
    cp "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" "$NGINX_SSL_DIR/fullchain.pem"
    cp "/etc/letsencrypt/live/$DOMAIN/privkey.pem" "$NGINX_SSL_DIR/privkey.pem"
    chmod 600 "$NGINX_SSL_DIR/privkey.pem"

    info "SSL certificate obtained and installed"
fi

# ============================================================
# 7. Set up automatic cert renewal
# ============================================================

RENEW_SCRIPT="/etc/cron.weekly/gparents-cert-renew"
if [[ ! -f "$RENEW_SCRIPT" ]]; then
    info "Setting up automatic certificate renewal..."
    cat > "$RENEW_SCRIPT" <<EOF
#!/bin/bash
# Renew Let's Encrypt certs and copy to app
certbot renew --quiet

# Copy renewed certs
if [[ -f /etc/letsencrypt/live/$DOMAIN/fullchain.pem ]]; then
    cp /etc/letsencrypt/live/$DOMAIN/fullchain.pem $NGINX_SSL_DIR/fullchain.pem
    cp /etc/letsencrypt/live/$DOMAIN/privkey.pem $NGINX_SSL_DIR/privkey.pem
    # Reload nginx
    cd $APP_DIR && docker compose -f docker-compose.prod.yml exec -T nginx nginx -s reload 2>/dev/null || true
fi
EOF
    chmod +x "$RENEW_SCRIPT"
    info "Weekly cert renewal cron job created"
fi

# ============================================================
# 8. Build and start the application
# ============================================================

info "Building and starting the application..."
cd "$APP_DIR"
docker compose -f docker-compose.prod.yml up --build -d

# Wait for services
info "Waiting for services to start..."
sleep 10

# Check health
if docker compose -f docker-compose.prod.yml ps | grep -q "Up"; then
    echo ""
    echo "=========================================="
    echo -e " ${GREEN}Installation complete!${NC}"
    echo "=========================================="
    echo ""
    echo " Your app is running at:"
    echo ""
    echo "   https://$DOMAIN"
    echo ""
    echo " Useful commands:"
    echo "   cd $APP_DIR"
    echo "   docker compose -f docker-compose.prod.yml logs -f     # View logs"
    echo "   docker compose -f docker-compose.prod.yml restart      # Restart"
    echo "   docker compose -f docker-compose.prod.yml down         # Stop"
    echo "   docker compose -f docker-compose.prod.yml up --build -d  # Rebuild"
    echo ""
else
    error "Some services failed to start. Check: docker compose -f docker-compose.prod.yml logs"
fi
