#!/usr/bin/env bash
set -euo pipefail

# Install nginx site configs for NBFC (run on VPS with sudo)
# Usage: sudo ./scripts/install-nginx.sh

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"

SITES_AVAILABLE="/etc/nginx/sites-available"
SITES_ENABLED="/etc/nginx/sites-enabled"

install_site() {
  local name="$1"
  cp "${DEPLOY_DIR}/nginx/${name}.conf" "${SITES_AVAILABLE}/${name}.conf"
  ln -sf "${SITES_AVAILABLE}/${name}.conf" "${SITES_ENABLED}/${name}.conf"
  echo "Installed ${name}"
}

install_site "nbfc.codingera.in"
install_site "api.nbfc.codingera.in"

nginx -t
systemctl reload nginx

echo ""
echo "Nginx configs installed. Run certbot if SSL is not yet configured:"
echo "  certbot --nginx -d nbfc.codingera.in -d api.nbfc.codingera.in"
