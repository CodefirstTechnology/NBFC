# NBFC VPS Deployment

Deploy the Patsanstha NBFC stack on a Linux VPS with **Docker Compose** and **host nginx** (SSL termination).

| Service | Domain | VPS IP |
|---------|--------|--------|
| Frontend (shell + 8 MFEs) | `nbfc.codingera.in` | `69.62.74.175` |
| Backend API (via YARP gateway) | `api.nbfc.codingera.in` | `69.62.74.175` |

---

## Architecture

```
Internet
   │
   ▼
Host nginx (:443)
   ├── nbfc.codingera.in ──► shell (:15200) + MFEs (:15201–15208)
   └── api.nbfc.codingera.in ──► gateway (:15000) ──► api (:15280)
                                              │
                                    postgres (:15432) + redis (:16379)
```

All Docker ports bind to **127.0.0.1** only so they do not conflict with your other projects on the same VPS. Only nginx listens on public ports 80/443.

Microfrontends are served via **path-based routing** on the same domain:

| Path | Remote |
|------|--------|
| `/` | shell |
| `/mfe-member/` | member MFE |
| `/mfe-deposit/` | deposit MFE |
| `/mfe-loan/` | loan MFE |
| `/mfe-collection/` | collection MFE |
| `/mfe-recovery/` | recovery MFE |
| `/mfe-accounting/` | accounting MFE |
| `/mfe-reports/` | reports MFE |
| `/mfe-admin/` | admin MFE |

---

## DNS (GoDaddy)

Add these A records pointing to `69.62.74.175`:

| Type | Name | Value |
|------|------|-------|
| A | `nbfc` | `69.62.74.175` |
| A | `api.nbfc` | `69.62.74.175` |

Wait 5–30 minutes for DNS propagation before running certbot.

---

## Prerequisites on VPS

- Ubuntu 22.04+ (or similar Linux)
- Docker Engine + Docker Compose v2
- nginx
- certbot (`certbot python3-certbot-nginx`)
- Git

```bash
# Example install (Ubuntu)
sudo apt update
sudo apt install -y docker.io docker-compose-v2 nginx certbot python3-certbot-nginx git
sudo usermod -aG docker $USER
# Log out and back in for docker group
```

---

## Step 1 — Clone and configure

```bash
cd /opt   # or your preferred deploy path
git clone <your-repo-url> nbfc
cd nbfc/NBFC/deployment

cp .env.example .env
nano .env
```

**Required secrets in `.env`:**

| Variable | Description |
|----------|-------------|
| `POSTGRES_PASSWORD` | Database password |
| `JWT__SigningKey` | Min 32 chars (`openssl rand -base64 32`) |
| `PiiEncryption__Key` | Min 32 chars |
| `IdentitySeed__AdminPassword` | First admin login password |

Domains are pre-set:

```
FRONTEND_ORIGIN=https://nbfc.codingera.in
API_BASE_URL=https://api.nbfc.codingera.in/api/v1
```

If any port clashes with existing projects, change `API_PORT`, `GATEWAY_PORT`, `SHELL_PORT`, `POSTGRES_PORT`, `REDIS_PORT` in `.env` and update the matching upstream ports in `nginx/*.conf`.

---

## Step 2 — Build and start containers

```bash
chmod +x scripts/*.sh
./scripts/deploy.sh
```

Or manually:

```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

First build takes 10–20 minutes (9 Angular apps + .NET API + gateway).

Verify locally on VPS:

```bash
curl http://127.0.0.1:15280/health/live   # API
curl http://127.0.0.1:15000/health/live   # Gateway
curl -I http://127.0.0.1:15200            # Shell
```

---

## Step 3 — Install nginx configs

```bash
sudo ./scripts/install-nginx.sh
```

Or manually:

```bash
sudo cp nginx/nbfc.codingera.in.conf /etc/nginx/sites-available/
sudo cp nginx/api.nbfc.codingera.in.conf /etc/nginx/sites-available/
sudo ln -sf /etc/nginx/sites-available/nbfc.codingera.in.conf /etc/nginx/sites-enabled/
sudo ln -sf /etc/nginx/sites-available/api.nbfc.codingera.in.conf /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx
```

---

## Step 4 — SSL with Let's Encrypt

```bash
sudo certbot --nginx -d nbfc.codingera.in -d api.nbfc.codingera.in
```

Certbot updates the nginx configs with certificate paths automatically.

---

## Step 5 — Verify production

```bash
curl https://api.nbfc.codingera.in/health/live
curl -I https://nbfc.codingera.in
```

Open in browser:

- **Frontend:** https://nbfc.codingera.in
- **Login:** use `IdentitySeed__AdminEmail` / `IdentitySeed__AdminPassword` from `.env`

---

## Useful commands

```bash
cd /opt/nbfc/NBFC/deployment

# View logs
docker compose -f docker-compose.prod.yml logs -f api
docker compose -f docker-compose.prod.yml logs -f shell

# Restart after code update
git pull
docker compose -f docker-compose.prod.yml --env-file .env up -d --build

# Stop stack
docker compose -f docker-compose.prod.yml down

# Stop and remove volumes (DESTRUCTS DATA)
docker compose -f docker-compose.prod.yml down -v
```

---

## Folder layout

```
deployment/
├── docker-compose.prod.yml    # Production stack (127.0.0.1 ports)
├── .env.example               # Secrets template
├── docker/
│   └── Dockerfile.frontend.prod   # Production MFE build with URL patching
├── nginx/
│   ├── nbfc.codingera.in.conf     # Frontend reverse proxy
│   ├── api.nbfc.codingera.in.conf # API reverse proxy
│   └── mfe-container.conf         # Internal nginx for MFE containers
└── scripts/
    ├── deploy.sh                  # Build + start + health check
    └── install-nginx.sh           # Copy nginx configs to /etc/nginx
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| Port already in use | Change ports in `.env` and nginx upstream blocks |
| CORS errors in browser | Ensure `FRONTEND_ORIGIN` in `.env` matches `https://nbfc.codingera.in` |
| MFE fails to load | Check nginx path routing; verify `curl http://127.0.0.1:15201/remoteEntry.json` |
| API 502 | `docker compose logs api gateway`; check postgres is healthy |
| SSL cert fails | Confirm DNS A records resolve to VPS IP: `dig nbfc.codingera.in` |
| certbot SSL paths missing before first cert | Temporarily comment out `ssl_certificate` lines, run certbot, then uncomment |

---

## Security notes

- Postgres, Redis, and all app ports are **localhost-only** by default
- Set `STARTUP_SEED_IDENTITY=false` after first deploy
- Never commit `deployment/.env`
- Rotate `JWT__SigningKey` and `PiiEncryption__Key` before going live
