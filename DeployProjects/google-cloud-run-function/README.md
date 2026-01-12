# Build Number Sync API - Google Cloud Run

Build number synchronization service using Firestore for storage.

## Table of Contents

- [Tech Stack](#tech-stack)
- [Components](#components)
- [Prerequisites](#prerequisites)
- [Setup and Deployment (CLI)](#setup-and-deployment-cli)
- [Setup and Deployment (Manual)](#setup-and-deployment-manual)
- [Development](#development)
- [Costs](#costs)

---

## Tech Stack

- **Google Cloud Platform (GCP)**
  - Cloud Run (serverless containers)
  - Firestore (NoSQL database)
  - Cloud Build (for building from source/repository)

---

## Components

This deployment uses the following Google Cloud components:

### Cloud Run Service (`build-number-sync`)
Serverless container that runs the Node.js application, handling API requests and managing build numbers via Firestore.

**Note:** Cloud Run services in the same project can access Firestore without authentication setup.

### Firestore Database
Serverless NoSQL database that stores build number documents in the `buildNumbers` collection. Automatically accessible from Cloud Run in the same project.

**Document structure:**
```
buildNumbers/{bundleId}_{platform}:
  - bundleId (string)
  - platform (string)
  - buildNumber (number)
  - lastUpdated (timestamp)
```

### Cloud Build
Automatically triggered during deployment to containerize the application from source code or repository. Detects Node.js runtime and builds the container image.

---

## Prerequisites

- **Google Cloud account** with billing enabled
- **Google Cloud CLI (`gcloud`)** - [Installation guide](https://cloud.google.com/sdk/docs/install)

**Note:** Provided commands use bash syntax, however `gcloud` CLI is available for Windows as well. The CLI commands and arguments are the same across all platforms.

---

## Setup and Deployment (CLI)

### Step 1: Authenticate and Set Project

**Authenticate with Google Cloud:**

```bash
gcloud auth login
```

**List your projects to get the project ID:**

```bash
gcloud projects list
```

**Important:** The output will show three columns: `PROJECT_ID`, `NAME`, and `PROJECT_NUMBER`. Make sure to copy the **PROJECT_ID** (not the PROJECT_NUMBER). The project ID is typically lowercase with hyphens (e.g., `my-project-123`), while the project number is a long numeric value.

**Set your project ID:**

```bash
gcloud config set project YOUR_PROJECT_ID
```

Replace `YOUR_PROJECT_ID` with your actual Google Cloud project ID from the list above.

---

### Step 2: Enable Required APIs

**Enable Cloud Run, Cloud Build, and Firestore:**

```bash
gcloud services enable run.googleapis.com \
  cloudbuild.googleapis.com \
  firestore.googleapis.com
```

---

### Step 3: Create Firestore Database

**Create a Firestore database in Native mode:**

```bash
gcloud firestore databases create --location=us-central1
```

**Note:** Change `us-central1` to your preferred region if needed.

---

### Step 4: Deploy Cloud Run Service

**Navigate to the project directory:**

```bash
cd /path/to/google-cloud-run-function
```

Replace `/path/to/google-cloud-run-function` with the actual path to this directory.

**Deploy the service:**

```bash
gcloud run deploy build-number-sync \
  --source . \
  --region us-central1 \
  --allow-unauthenticated \
  --memory 256Mi \
  --cpu 0.25 \
  --min-instances 0 \
  --max-instances 10
```

**Note:** The `--source .` flag builds the container from the current directory automatically.

---

### Step 5: Get Service URL

**Retrieve the deployed service URL:**

```bash
gcloud run services describe build-number-sync \
  --region us-central1 \
  --format 'value(status.url)'
```

**Example output:**
```
https://build-number-sync-xxxxxxxxxx-uc.a.run.app
```

Your API is now accessible at this URL.

---

## Setup and Deployment (Manual)

**Note:** Button labels and field names in the Google Cloud Console may differ slightly from the CLI command names used above.

### Step 1: Create or Select Project

1. Open [Google Cloud Console](https://console.cloud.google.com/)
2. Click the project dropdown at the top
3. Click **New Project** or select an existing project
4. If creating new: Enter project name, select billing account, click **Create**

---

### Step 2: Create Firestore Database

1. In the left menu, go to **Firestore**
2. Click **Create database**
3. Select **Native mode**
4. Keep the default database name
5. Choose location: `us-central1` (or your preferred region)
6. Click **Create database**
7. Wait for database creation to complete

**Note:** The database will be automatically accessible to Cloud Run services in the same project without any authentication configuration.

---

### Step 3: Deploy Cloud Run Service

1. In the left menu, go to **Cloud Run**
2. Click **Create service**
3. Choose deployment method:

#### **Option A: Deploy from Repository (Continuous Deployment)**

1. Select **Continuously deploy from a repository**
2. Click **Set up with Cloud Build**
3. **Connect repository:**
   - Click **Manage connected repositories**
   - Connect your GitHub account
   - Select the repository containing this code
   - Click **Next**
4. **Build configuration:**
   - **Branch:** Select your branch (e.g., `main`)
   - **Build type:** Select **Node.js**
   - **Build context directory:** `/` (leave default)
   - **Dockerfile:** Leave blank
   - **Entry point:** Leave blank
   - **Function target:** Leave blank
5. Click **Save**
6. **Service settings:**
   - **Service name:** `build-number-sync`
   - **Region:** `us-central1` (or your preferred region)
   - **Authentication:** Select **Allow unauthenticated invocations**
7. **Container settings:**
   - **Memory:** `256 MiB`
   - **CPU:** `0.25`
   - **Minimum instances:** `0`
   - **Maximum instances:** `10`
8. Click **Create**
9. Wait for initial deployment to complete
10. Copy the service URL from the service details page

**Note:** Future pushes to the connected branch will automatically trigger build and deploy.

#### **Option B: Deploy from Inline Editor**

1. **Build configuration:**
   - **Branch:** Select your branch (e.g., `main`)
   - **Build type:** Select **Node.js**
   - **Build context directory:** `/` (leave default)
   - **Dockerfile:** Leave blank
   - **Entry point:** Leave blank
   - **Function target:** Leave blank
2. **Service settings:**
   - **Service name:** `build-number-sync`
   - **Region:** `us-central1` (or your preferred region)
   - **Authentication:** Select **Allow unauthenticated invocations**
3. **Container settings:**
   - **Memory:** `256 MiB`
   - **CPU:** `0.25`
   - **Minimum instances:** `0`
   - **Maximum instances:** `10`
4. Click **Create**
5. Once created, go to the Source section
6. In the editor, copy-paste the contents of `index.js` and `package.json` from this project
7. Click **Deploy**

**Note:** Cloud Build will treat the inline code just like repository content, automatically building and deploying your service. You only need to provide the two files - `index.js` and `package.json`.

Your API is now accessible at the displayed URL.

---

## Development

To deploy a new version after making code changes:

**Navigate to the project directory:**

```bash
cd /path/to/google-cloud-run-function
```

**Deploy the updated service:**

```bash
gcloud run deploy build-number-sync \
  --source . \
  --region us-central1
```

The service will rebuild and deploy automatically. Other settings (memory, CPU, authentication) are preserved from the previous deployment unless explicitly changed.

---

## Costs

### Free Tier

Google Cloud Platform offers a free tier for Cloud Run and Firestore:

**Cloud Run:**
- **2 million requests per month**
- 360,000 GB-seconds of memory
- 180,000 vCPU-seconds of compute time

**Firestore:**
- **50,000 document reads per day** (~1.5 million per month)
- **20,000 document writes per day** (~600,000 per month)
- 20,000 document deletes per day
- 1 GB storage

For typical build number sync usage, you'll likely stay within the free tier limits.

### Paid Pricing

Beyond free tier: heavily depends on settings

**Pricing documentation:**
- Cloud Run: https://cloud.google.com/run/pricing
- Firestore: https://cloud.google.com/firestore/pricing
