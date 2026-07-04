# Kawwer on Microsoft Azure (Student $100 credit)

Complete guide to host **Kawwer.Api + PostgreSQL** on an Azure Linux VM with HTTPS, so your phone app works without your PC.

**What you end up with:**

```
Phone (WiFi or cellular)
    │
    ▼  HTTPS
Caddy (ports 80/443, auto Let's Encrypt certificate)
    │
    ▼  HTTP (internal Docker network)
Kawwer.Api (.NET 10, port 8080, SignalR + REST)
    │
    ▼
PostgreSQL 16 (internal only, never exposed to internet)
```

**Time:** ~45–90 minutes first time.

**Cost with Azure for Students:**

Kawwer is a **light hobby API** — a few REST endpoints, one SignalR hub, a small Postgres DB, and a background job that wakes every 5 minutes. At runtime it uses **~300–600 MB RAM** total (API + Postgres + Caddy). You do **not** need a powerful VM to *run* it; the only heavy moment is the **one-time Docker build** (compiling .NET).

| Resource | Typical cost |
|----------|----------------|
| **Standard_B1s** (recommended for Kawwer) | **750 free compute hours/month** — covers 24/7 (730 hrs). You mainly pay for disk + IP |
| **Disk + public IP** | ~\$4–8/month from your \$100 credit |
| **Managed Postgres** | **Not used** — Postgres runs in Docker to save credit |
| **Standard_B2s** | Only if B1s build fails — ~\$25–35/month extra compute |

> **How long will \$100 last?** With **B1s** (the right size for this project), expect **~\$5–8/month** in charges → **roughly 12–18 months** on one student credit, often the full 12-month offer period. B2s was listed as "easier" only for compiling — it burns credit **3–4× faster** for no runtime benefit.

---

## Table of contents

1. [Prerequisites](#1-prerequisites)
2. [Sign up for Azure for Students](#2-sign-up-for-azure-for-students)
3. [Set a budget alert](#3-set-a-budget-alert)
4. [Generate an SSH key on Windows](#4-generate-an-ssh-key-on-windows)
5. [Create a Linux VM](#5-create-a-linux-vm)
6. [Open firewall ports (NSG)](#6-open-firewall-ports-nsg)
7. [Connect to the VM](#7-connect-to-the-vm)
8. [Install Docker](#8-install-docker)
9. [Get a domain name and point DNS](#9-get-a-domain-name-and-point-dns)
10. [Deploy Kawwer](#10-deploy-kawwer)
11. [Verify the API](#11-verify-the-api)
12. [Point the mobile app at the server](#12-point-the-mobile-app-at-the-server)
13. [Update the server later](#13-update-the-server-later)
14. [Backups](#14-backups)
15. [Stop the VM to save credit](#15-stop-the-vm-to-save-credit)
16. [Troubleshooting](#16-troubleshooting)

---

## 1. Prerequisites

Before you start, gather:

| Item | Why |
|------|-----|
| **School email** (`.edu` or institution-verified) | Required for Azure for Students — no credit card needed |
| **A domain name** | Required for HTTPS (iOS blocks plain HTTP to public servers). Buy one (~\$10/yr) or use a free subdomain (DuckDNS) |
| **Firebase service account JSON** | Optional; needed for Android push. Local path: `C:\kawwerr_app\secrets\firebase-service-account.json` |
| **Git repo** | To clone Kawwer on the VM |

**Repo files used by this guide (already in the project):**

- `Dockerfile` — builds Kawwer.Api
- `docker-compose.yml` — API + Postgres + Caddy
- `Caddyfile.example` — HTTPS reverse proxy template
- `.env.example` — secrets template

---

## 2. Sign up for Azure for Students

1. Go to [https://azure.microsoft.com/free/students/](https://azure.microsoft.com/free/students/)
2. Click **Activate now**
3. Sign in with your **school Microsoft account** or verify student status
4. Complete signup — **no credit card required**
5. You receive **\$100 credit** valid for **12 months** (renewable yearly while you're a student)

6. Open the [Azure Portal](https://portal.azure.com/)

**Confirm your credit:**

- Portal → search **Cost Management + Billing** → **Credits** (or **Subscriptions** → your Azure for Students subscription)
- You should see **\$100.00** remaining

---

## 3. Set a budget alert

Avoid surprise credit burn:

1. Portal → **Cost Management + Billing** → **Budgets**
2. **Add** a budget:
   - Name: `kawwer-alert`
   - Amount: **\$10** (or \$25)
   - Alert at **50%**, **90%**, **100%**
   - Email: your address

You'll get warned before the \$100 runs out.

---

## 4. Generate an SSH key on Windows

In **PowerShell**:

```powershell
ssh-keygen -t ed25519 -C "kawwer-azure" -f "$env:USERPROFILE\.ssh\kawwer_azure"
```

Press Enter for no passphrase (or set one).

Copy the **public** key:

```powershell
Get-Content "$env:USERPROFILE\.ssh\kawwer_azure.pub" | Set-Clipboard
```

---

## 5. Create a Linux VM

### 5.1 Resource group

1. Portal → **Resource groups** → **Create**
2. Name: `kawwer-rg`
3. Region: pick one close to you (e.g. **West Europe**, **France Central**, **North Europe**)
4. **Review + create**

### 5.2 Virtual machine

1. Portal → **Virtual machines** → **Create** → **Azure virtual machine**

| Setting | Value |
|---------|-------|
| **Subscription** | Azure for Students |
| **Resource group** | `kawwer-rg` |
| **VM name** | `kawwer-api` |
| **Region** | Same as resource group |
| **Availability options** | No infrastructure redundancy required |
| **Security type** | Standard |
| **Image** | **Ubuntu Server 24.04 LTS - x64 Gen2** |
| **Size** | See below |
| **Authentication type** | **SSH public key** |
| **Username** | `azureuser` |
| **SSH public key source** | Use existing public key → paste from clipboard |
| **Public inbound ports** | Allow selected ports → **SSH (22)** only for now (we add 80/443 in §6) |

### 5.3 Choose VM size

Click **See all sizes**.

**Recommended for Kawwer (runtime is light — this is plenty):**

| Size | vCPU | RAM | Notes |
|------|------|-----|-------|
| **Standard_B1s** | 1 | 1 GB | **750 free hours/month** = 24/7 compute at \$0. Add swap (§7.2) before the one-time `docker compose build`. |

**Only if the build fails on B1s:**

| Size | vCPU | RAM | Notes |
|------|------|-----|-------|
| **Standard_B2s** | 2 | 4 GB | Easier compile; **resize back to B1s** after deploy, or build on your PC and push a pre-built image (§16). ~\$25–35/mo if left running 24/7. |

Select **B1s** → **Select**

### 5.4 Disks

| Setting | Value |
|---------|-------|
| **OS disk type** | **Standard SSD** (cheaper; Kawwer doesn't need Premium) |
| **Size** | **30 GB** (enough for OS + Docker images + small DB) |

### 5.5 Networking

| Setting | Value |
|---------|-------|
| **Virtual network** | Create new (default is fine) |
| **Subnet** | default |
| **Public IP** | Create new (default) |
| **NIC NSG** | Basic |
| **Public inbound ports** | SSH (22) only |

### 5.6 Create

1. **Review + create** → **Create**
2. Wait ~2 minutes for deployment
3. Go to the VM → copy **Public IP address**

---

## 6. Open firewall ports (NSG)

Azure uses a **Network Security Group (NSG)**. Only SSH is open by default.

1. Portal → **Virtual machines** → `kawwer-api`
2. Left menu → **Networking** (under Settings)
3. **Create port rule** → **Inbound port rule** — add **two** rules:

| Destination port ranges | Protocol | Name | Priority |
|-------------------------|----------|------|----------|
| `80` | TCP | `Allow-HTTP` | 1010 |
| `443` | TCP | `Allow-HTTPS` | 1020 |

SSH (22) should already exist.

**Do NOT open** `5432` (Postgres) or `8080` (API). They stay inside Docker only.

> Unlike Oracle Cloud, Azure Ubuntu images typically **do not** need extra iptables rules — the NSG is enough.

---

## 7. Connect to the VM

From PowerShell:

```powershell
ssh -i "$env:USERPROFILE\.ssh\kawwer_azure" azureuser@YOUR_PUBLIC_IP
```

Replace `YOUR_PUBLIC_IP` with the VM's public IP.

First connection: type `yes` for host authenticity.

Prompt: `azureuser@kawwer-api:~$`

### 7.1 Basic server setup

```bash
sudo apt-get update
sudo apt-get upgrade -y
sudo timedatectl set-timezone UTC
```

### 7.2 Add swap (required on B1s)

B1s has 1 GB RAM. **Running** Kawwer is fine; **compiling** .NET inside Docker needs more headroom once. Add 2 GB swap before `docker compose build` (keep swap after — harmless):

```bash
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
free -h
```

After the first successful build, day-to-day RAM usage stays low (~500 MB for all containers).

---

## 8. Install Docker

On the VM:

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker azureuser
exit
```

SSH in again:

```powershell
ssh -i "$env:USERPROFILE\.ssh\kawwer_azure" azureuser@YOUR_PUBLIC_IP
```

Verify:

```bash
docker --version
docker compose version
```

---

## 9. Get a domain name and point DNS

HTTPS is required for iOS on a public server.

### Option A — Your own domain

At your registrar (Cloudflare, Namecheap, etc.):

| Type | Name | Value | TTL |
|------|------|-------|-----|
| A | `api` | `YOUR_PUBLIC_IP` | 300 |

Result: `api.yourdomain.com` → Azure VM.

### Option B — Free subdomain (DuckDNS)

1. [https://www.duckdns.org](https://www.duckdns.org) → create `kawwer-api.duckdns.org`
2. Set IP to your Azure public IP

### Verify DNS (from your PC)

```powershell
nslookup api.yourdomain.com
```

Wait 5–30 minutes if needed.

---

## 10. Deploy Kawwer

All commands below run **on the VM**.

### 10.1 Clone the repository

```bash
cd ~
git clone https://github.com/YOUR_USER/kawwer.git
cd kawwer
```

Private repo: use a GitHub personal access token when prompted, or set up a deploy key.

**No GitHub?** Copy from your PC (see §16).

### 10.2 Create secrets

```bash
mkdir -p secrets
nano secrets/firebase-service-account.json
```

Paste Firebase service account JSON. Save: `Ctrl+O`, Enter, `Ctrl+X`.

**Or SCP from Windows** (run on PC):

```powershell
scp -i "$env:USERPROFILE\.ssh\kawwer_azure" "C:\kawwerr_app\secrets\firebase-service-account.json" azureuser@YOUR_PUBLIC_IP:~/kawwer/secrets/firebase-service-account.json
```

Placeholder if skipping Firebase for now:

```bash
echo '{}' > secrets/firebase-service-account.json
```

### 10.3 Create environment file

```bash
cp .env.example .env
nano .env
```

```env
POSTGRES_PASSWORD=<run: openssl rand -base64 32>
JWT_SIGNING_KEY=<run: openssl rand -base64 32>
```

Generate on the VM:

```bash
openssl rand -base64 32
```

Use one output per variable.

### 10.4 Configure Caddy

```bash
cp Caddyfile.example Caddyfile
nano Caddyfile
```

```
api.yourdomain.com {
    reverse_proxy api:8080
}
```

### 10.5 Build and start

```bash
docker compose build
docker compose up -d
```

First build: **5–15 minutes** (B1s may take longer).

```bash
docker compose ps
docker compose logs api --tail 50
docker compose logs caddy --tail 30
```

Look for: `Database migrations applied.`

Expected containers:

| Service | State |
|---------|-------|
| kawwer-api-1 | running |
| kawwer-postgres-1 | running |
| kawwer-caddy-1 | running |

---

## 11. Verify the API

### On the VM

```bash
curl -s http://localhost:8080/health
```

Expected: `{"status":"healthy"}`

### From your PC (HTTPS)

```powershell
curl https://api.yourdomain.com/health
```

### Register a test user

```powershell
curl -X POST https://api.yourdomain.com/api/v1/auth/register `
  -H "Content-Type: application/json" `
  -d '{"username":"testuser","email":"test@example.com","password":"TestPass123!","fullName":"Test User"}'
```

### SignalR

Mobile app connects to:

```
wss://api.yourdomain.com/hubs/match?access_token=...
```

Caddy forwards WebSockets automatically.

---

## 12. Point the mobile app at the server

### 12.1 Update AppConfig

Edit `Kawwer.Mobile/Services/AppConfig.cs` on your PC:

```csharp
private const string Host = "https://api.yourdomain.com";
```

HTTPS, no trailing slash, no `/api/v1`.

### 12.2 Rebuild IPA

Push to GitHub → CodeMagic `ios-release` workflow → download `.ipa` → Sideloadly.

### 12.3 Test

1. Phone on WiFi or cellular — PC **off**
2. Register / log in
3. Create a match

---

## 13. Update the server later

```bash
cd ~/kawwer
git pull
docker compose build api
docker compose up -d
docker compose logs -f api
```

Migrations run on API startup automatically.

---

## 14. Backups

### Manual backup

```bash
cd ~/kawwer
docker compose exec postgres pg_dump -U postgres kawwer > backup-$(date +%Y%m%d).sql
```

### Restore

```bash
cat backup-20260704.sql | docker compose exec -T postgres psql -U postgres kawwer
```

### Optional weekly cron

```bash
mkdir -p ~/backups
crontab -e
```

```
0 3 * * 0 cd /home/azureuser/kawwer && docker compose exec -T postgres pg_dump -U postgres kawwer > /home/azureuser/backups/kawwer-$(date +\%Y\%m\%d).sql
```

---

## 15. Stop the VM to save credit

**Important:** Shutting down the OS from inside the VM still **charges** for compute. You must **deallocate** in Azure.

### Stop (saves money, API goes offline)

1. Portal → **Virtual machines** → `kawwer-api` → **Stop**
2. Status becomes **Stopped (deallocated)**

### Start again

1. **Start** the VM
2. **Public IP may change** unless you use a **Static** IP (small extra cost)
   - Portal → VM → **Networking** → Public IP → **Configuration** → **Static**
   - Update DNS A record if IP changed

### Auto-shutdown (optional)

Portal → VM → **Auto-shutdown** → enable e.g. 2:00 AM if you only test evenings.

---

## 16. Troubleshooting

### Cannot SSH

- NSG allows port 22?
- Correct user: **`azureuser`** (not `ubuntu`)
- Correct key: `ssh -i ... azureuser@IP`
- VM state: **Running** (not deallocated)

### Site not loading

- NSG: ports **80** and **443** open?
- `docker compose ps` — all three containers running?
- `docker compose logs caddy`

### HTTPS certificate failed

- DNS A record points to VM public IP **before** starting Caddy
- `nslookup api.yourdomain.com`
- Port 80 open (Let's Encrypt HTTP challenge)

### API keeps restarting

```bash
docker compose logs api
```

| Error | Fix |
|-------|-----|
| Postgres connection refused | `docker compose restart api` after 30s |
| Wrong password | `.env` `POSTGRES_PASSWORD` must match compose |
| Firebase file missing | Create `secrets/firebase-service-account.json` |

### Docker build runs out of memory (B1s)

- Add swap (§7.2)
- Or resize VM to **B2s**: Portal → VM → **Size** → B2s → **Resize** (brief downtime)

### Public IP changed after restart

- Set IP to **Static** in Azure, update DNS
- Or use DuckDNS and update IP in their dashboard

### Phone errors but curl works

- `AppConfig.cs` still has LAN IP → rebuild IPA
- iOS needs **https://** not http://

### Copy project without GitHub

On your PC:

```powershell
scp -i "$env:USERPROFILE\.ssh\kawwer_azure" -r C:\kawwerr_app\kawwer azureuser@YOUR_PUBLIC_IP:~/kawwer
```

Then on VM: create `.env`, `Caddyfile`, `secrets/` manually.

### Credit running low

- **Deallocate** VM when not testing (§15)
- Use **B1s** + swap instead of B2s
- Check **Cost Management** → **Cost analysis** for what's burning credit (usually VM size + disk + public IP)

---

## Quick reference

| What | Value |
|------|-------|
| Portal | [https://portal.azure.com](https://portal.azure.com) |
| SSH | `ssh -i ~/.ssh/kawwer_azure azureuser@PUBLIC_IP` |
| Health | `https://api.yourdomain.com/health` |
| Mobile `Host` | `https://api.yourdomain.com` |
| Hub URL | `https://api.yourdomain.com/hubs/match` |
| Restart | `docker compose up -d` |
| Logs | `docker compose logs -f api` |
| Stop billing compute | Portal → VM → **Stop** (deallocate) |

---

## Security checklist

- [ ] Budget alert set (§3)
- [ ] Strong `POSTGRES_PASSWORD` and `JWT_SIGNING_KEY`
- [ ] Ports 5432 and 8080 **not** in NSG
- [ ] SSH key auth only
- [ ] `.env` and `secrets/` not in git
- [ ] HTTPS via Caddy
- [ ] Regular `pg_dump` backups

---

## What this does NOT cover

- **iOS push** — paid Apple Developer + Firebase APNs
- **Azure Database for PostgreSQL** (managed) — intentionally skipped to save credit; Postgres runs in Docker
- **App Service / Container Apps** — possible alternatives but more Azure-specific config for SignalR; VM + Docker is simpler for this project

---

## Credit math (rough)

| Setup | ~Monthly cost | \$100 lasts (24/7) |
|-------|----------------|---------------------|
| **B1s** + 30 GB Standard SSD + public IP | **~\$5–8** | **~\$12–18 months** (often whole student year) |
| B2s + 30 GB disk + public IP | ~\$28–38 | ~2.5–3.5 months |
| B1s, VM deallocated when not testing | ~\$2–4 (disk + IP only) | Years of intermittent use |

**What actually uses resources in Kawwer:**

| Component | Load |
|-----------|------|
| REST API | Tiny — a few requests per user action |
| SignalR | One lightweight WebSocket per active match screen |
| Postgres | Small DB, no heavy analytics |
| MatchReminderService | Wakes every 5 min, queries today's matches — negligible |
| Firebase push | Outbound HTTP to Google, not CPU-heavy |

Renew **Azure for Students** annually while enrolled to get another **\$100**.
