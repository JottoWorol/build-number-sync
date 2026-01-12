# Build Number Sync API - Cloudflare Workers

Build number synchronization service using Cloudflare Workers and KV storage.

## Table of Contents

- [Tech Stack](#tech-stack)
- [Components](#components)
- [Prerequisites](#prerequisites)
- [Setup and Deployment (Wrangler CLI)](#setup-and-deployment-wrangler-cli)
- [Setup and Deployment (Automated Script - Experimental)](#setup-and-deployment-automated-script-experimental)
- [Development](#development)
- [Costs](#costs)

---

## Tech Stack

- **Cloudflare Workers** - Serverless edge compute platform
- **Cloudflare KV** - Global key-value storage
- **Language:** TypeScript
- **Build Tool:** Wrangler CLI

---

## Components

This deployment uses the following Cloudflare components:

### Cloudflare Worker (`build-number-sync`)
Serverless function that runs on Cloudflare's global edge network, processing API requests and managing build numbers via KV storage. Executes in multiple regions simultaneously.

### Cloudflare KV (`BUILD_VERSION_SYNC`)
Global key-value storage distributed across Cloudflare's network. Stores build number records with eventual consistency, optimized for high read volumes.

**Data structure:**
```
Key: build_number_{bundleId}_{platform}
Value: {buildNumber} (as string)
```

---

## Prerequisites

- **Cloudflare account** - [Sign up](https://dash.cloudflare.com/sign-up) (free tier available)
- **Node.js** - Includes npm for package management
- **Wrangler CLI** - Cloudflare's command-line tool (installed during setup)

**Note:** Provided commands use bash syntax, however Wrangler CLI is available for Windows as well. The CLI commands and arguments are the same across all platforms.

---

## Setup and Deployment (Wrangler CLI)

### Step 1: Install Dependencies

**Navigate to the project directory:**

```bash
cd /path/to/cloudflare-worker-openapi
```

Replace `/path/to/cloudflare-worker-openapi` with the actual path to this directory.

**Install Node.js dependencies:**

```bash
npm install
```

This installs Wrangler CLI and all required packages.

---

### Step 2: Authenticate with Cloudflare

**Login to your Cloudflare account:**

```bash
npx wrangler login
```

This opens your browser to authorize Wrangler with your Cloudflare account.

---

### Step 3: Create KV Namespace

**Create the KV namespace for storing build numbers:**

```bash
npx wrangler kv namespace create "BUILD_VERSION_SYNC"
```

**Example output:**
```
⛅️ wrangler 3.x.x
-------------------
🌀 Creating namespace with title "build-number-sync-BUILD_VERSION_SYNC"
✨ Success!
Add the following to your configuration file in your kv_namespaces array:
{ binding = "BUILD_VERSION_SYNC", id = "abc123xxxxxxxxxxxxx" }
```

---

### Step 4: Update Configuration

**Edit `wrangler.jsonc` and add the KV namespace binding:**

```jsonc
{
  "name": "build-number-sync",
  "main": "src/index.ts",
  "compatibility_date": "2024-01-01",
  "kv_namespaces": [
    {
      "binding": "BUILD_VERSION_SYNC",
      "id": "YOUR_NAMESPACE_ID_HERE"
    }
  ]
}
```

Replace `YOUR_NAMESPACE_ID_HERE` with the namespace ID from Step 3.

**Alternatively:** When prompted by Wrangler, agree to automatically update the configuration file.

**Configuration options:**
- `name` - Worker name (determines the worker URL)
- `main` - Entry point file
- `compatibility_date` - Cloudflare runtime compatibility date
- `kv_namespaces.binding` - Variable name used in code to access KV
- `kv_namespaces.id` - Your KV namespace ID

---

### Step 5: Deploy Worker

**Deploy to Cloudflare:**

```bash
npx wrangler deploy
```

**Example output:**
```
Published build-number-sync (X.XX sec)
  https://build-number-sync.YOUR_SUBDOMAIN.workers.dev
```

Copy the worker URL - this is your API endpoint.

---

## Setup and Deployment (Automated Script - Experimental)

**Note:** The automated scripts are experimental and may not work in all environments. The Wrangler CLI method above is recommended for reliable deployment.

### Quick Setup Script

The automated script detects your OS and handles the entire setup process:

```bash
node setup.js
```

**What the script does:**
1. Installs Wrangler CLI (if not present)
2. Authenticates with Cloudflare
3. Creates KV namespace for build numbers
4. Updates `wrangler.jsonc` with namespace ID
5. Deploys the worker to Cloudflare

**Important:** When Wrangler creates the KV namespace, it will ask to update your configuration file. You must agree for the setup to complete successfully.

### Platform-Specific Scripts

**macOS/Linux:**
```bash
chmod +x setup.sh
./setup.sh
```

**Windows:**
```powershell
./setup.ps1
```

---

## Development

### Update Existing Deployment

To deploy a new version after making code changes:

**Navigate to the project directory:**

```bash
cd /path/to/cloudflare-worker-openapi
```

**Deploy the updated worker:**

```bash
npx wrangler deploy
```

The worker will be updated with your changes immediately.

---

### Local Testing

Test your worker locally before deploying:

```bash
npx wrangler dev
```

This starts a local development server at `http://localhost:8787` with hot reload. Changes to your code will automatically reload the worker.

---

### View Live Logs

Stream real-time logs from your deployed worker:

```bash
npx wrangler tail
```

Press `Ctrl+C` to stop streaming logs.

---

## Costs

### Free Tier

Cloudflare Workers offers a generous free tier:
- **100,000 requests per day** (~3 million per month)
- **KV:** 100,000 reads per day, 1,000 writes per day, 1 GB stored data

For typical build number sync usage, you'll likely stay within the free tier limits.

### Paid Plan

If you exceed free tier limits, the Workers Paid plan ($5/month) provides:
- **10 million requests per month** (included)
- **KV:** 10 million reads, 1 million writes per month (included)
- Additional requests: $0.50 per million

**Pricing documentation:**
- https://developers.cloudflare.com/workers/platform/pricing/
- https://developers.cloudflare.com/kv/platform/pricing/
