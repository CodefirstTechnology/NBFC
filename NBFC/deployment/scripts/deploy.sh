#!/usr/bin/env bash
set -euo pipefail

# Deploy NBFC stack on VPS (run from NBFC/deployment/)
# Usage: ./scripts/deploy.sh [--build-only]

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
PROJECT_DIR="$(cd "${DEPLOY_DIR}/.." && pwd)"

cd "${DEPLOY_DIR}"

if [[ ! -f .env ]]; then
  echo "Missing deployment/.env — copy .env.example and fill in secrets:"
  echo "  cp .env.example .env && nano .env"
  exit 1
fi

echo "==> Building and starting NBFC containers..."
docker compose -f docker-compose.prod.yml --env-file .env up -d --build "$@"

echo ""
echo "==> Waiting for API health..."
for i in $(seq 1 30); do
  if curl -sf "http://127.0.0.1:${API_PORT:-15280}/health/live" >/dev/null 2>&1; then
    echo "API is healthy."
    break
  fi
  if [[ $i -eq 30 ]]; then
    echo "WARNING: API health check timed out. Check logs: docker compose -f docker-compose.prod.yml logs api"
  fi
  sleep 2
done

echo ""
echo "==> Container status:"
docker compose -f docker-compose.prod.yml ps

echo ""
echo "Next steps:"
echo "  1. Install nginx site configs (if not done):"
echo "       sudo cp nginx/nbfc.codingera.in.conf /etc/nginx/sites-available/"
echo "       sudo cp nginx/api.nbfc.codingera.in.conf /etc/nginx/sites-available/"
echo "       sudo ln -sf /etc/nginx/sites-available/nbfc.codingera.in.conf /etc/nginx/sites-enabled/"
echo "       sudo ln -sf /etc/nginx/sites-available/api.nbfc.codingera.in.conf /etc/nginx/sites-enabled/"
echo "  2. Obtain SSL certificates:"
echo "       sudo certbot --nginx -d nbfc.codingera.in -d api.nbfc.codingera.in"
echo "  3. Reload nginx: sudo nginx -t && sudo systemctl reload nginx"
echo ""
echo "URLs:"
echo "  Frontend: https://nbfc.codingera.in"
echo "  API:      https://api.nbfc.codingera.in/health/live"
