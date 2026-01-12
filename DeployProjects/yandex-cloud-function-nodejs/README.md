# Build Number Sync API - Yandex Cloud Functions

Build number synchronization service using Yandex Cloud Functions and YDB (Yandex Database) for storage.

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

- **Yandex Cloud:**
  - **Cloud Functions** - Serverless compute (Node.js 22 runtime)
  - **YDB (Yandex Database)** - Serverless NoSQL database
  - **IAM** - Service accounts for authentication
  - **API Gateway** - API URL routing
- **Node.js Packages:**
  - `@ydbjs/core` - YDB driver core
  - `@ydbjs/query` - YQL queries and transactions
  - `@ydbjs/value` - YDB data types
  - `@ydbjs/auth` - Metadata-based authentication

---

## Components

This deployment uses the following Yandex Cloud components:

### Cloud Function (`build-number-sync`)
Serverless function that processes API requests and executes business logic for managing build numbers. Authenticates with YDB using the service account.

### YDB Database (`build-number-sync-db`)
Serverless NoSQL database that stores build number records in the `buildNumbers` table. The function accesses it via the service account credentials.

**Table schema:**
```
buildNumbers:
  - id (Utf8, Primary Key) - {bundleId}_{platform}
  - bundleId (Utf8)
  - platform (Utf8)
  - buildNumber (Int64)
  - lastUpdated (Timestamp)
```

### Service Account (`build-number-sync-sa`)
Identity that allows the Cloud Function to authenticate with YDB using metadata-based authentication (no stored credentials). Has `ydb.editor` role for database access.

### API Gateway (`build-number-sync-api`)
Routes HTTP requests to the Cloud Function based on URL paths, providing RESTful API endpoints.

---

## Prerequisites

- **Yandex Cloud account** with billing enabled
- **Yandex CLI (`yc`)** - [Installation guide](https://cloud.yandex.com/en/docs/cli/quickstart)

**Note:** Provided commands use bash syntax, however `yc` CLI is available for Windows as well. The CLI commands and arguments are the same across all platforms.

---

## Setup and Deployment (CLI)

### Step 1: Initialize Yandex CLI

**Install and configure the Yandex CLI:**

```bash
# Install yc CLI (macOS/Linux)
curl -sSL https://storage.yandexcloud.net/yandexcloud-yc/install.sh | bash

# For Windows or other platforms: https://cloud.yandex.com/en/docs/cli/quickstart

# Initialize and authenticate
yc init
```

Follow the prompts to select or create a cloud and folder. This sets your default configuration.

---

### Step 2: Get Your Folder ID

**Get your folder ID** (needed for subsequent commands):

```bash
yc config list
```

Look for the `folder-id` value. Save it - you'll use it as `YOUR_FOLDER_ID` in commands below.

**Example output:**
```
folder-id: b1gxxxxxxxxxxxxx
```

---

### Step 3: Create YDB Database

**Create a serverless YDB database** to store build numbers:

```bash
yc ydb database create build-number-sync-db \
  --serverless \
  --folder-id YOUR_FOLDER_ID
```

Replace `YOUR_FOLDER_ID` with your actual folder ID from Step 2.


---

### Step 4: Get Database Connection String

**Retrieve database information:**

```bash
yc ydb database get build-number-sync-db --folder-id YOUR_FOLDER_ID
```

From the output, copy the `endpoint` value. This is your complete connection string.

**Example output:**
```
endpoint: grpcs://ydb.serverless.yandexcloud.net:2135/?database=/ru-central1/b1gxxxxxxxxxxxxx/etnxxxxxxxxxxxxx
```

Save this endpoint value - you'll use it as `YOUR_YDB_CONNECTION_STRING` later.

---

### Step 5: Create Service Account

**Create a service account** for the function to authenticate with YDB:

```bash
yc iam service-account create \
  --name build-number-sync-sa \
  --folder-id YOUR_FOLDER_ID
```

**Get the service account ID:**

```bash
yc iam service-account get build-number-sync-sa \
  --folder-id YOUR_FOLDER_ID
```

From the output, copy the `id` value.

**Example output:**
```
id: ajexxxxxxxxxxxxx
folder_id: b1gxxxxxxxxxxxxx
created_at: "2026-01-08T22:15:30.123Z"
name: build-number-sync-sa
```

Save the `id` value - you'll use it as `YOUR_SERVICE_ACCOUNT_ID` below.

**Grant YDB access to the service account:**

```bash
yc ydb database add-access-binding build-number-sync-db \
  --service-account-id YOUR_SERVICE_ACCOUNT_ID \
  --role ydb.editor \
  --folder-id YOUR_FOLDER_ID
```

Replace `YOUR_SERVICE_ACCOUNT_ID` and `YOUR_FOLDER_ID` with your actual values.


---

### Step 6: Deploy the Function

**Navigate to the project directory:**

```bash
cd /path/to/yandex-cloud-function-nodejs
```

Replace `/path/to/yandex-cloud-function-nodejs` with the actual path to this directory.

**Create the function:**

```bash
yc serverless function create \
  --name build-number-sync \
  --folder-id YOUR_FOLDER_ID
```

**Deploy the function code:**

```bash
yc serverless function version create \
  --function-name build-number-sync \
  --runtime nodejs22 \
  --entrypoint index.handler \
  --memory 256m \
  --execution-timeout 10s \
  --source-path . \
  --service-account-id YOUR_SERVICE_ACCOUNT_ID \
  --environment "YDB_CONNECTION_STRING=YOUR_YDB_CONNECTION_STRING" \
  --folder-id YOUR_FOLDER_ID
```

**Note:** The `--source-path .` flag deploys files from the current directory.

Replace:
- `YOUR_FOLDER_ID` - Your folder ID from Step 2
- `YOUR_SERVICE_ACCOUNT_ID` - Service account ID from Step 5
- `YOUR_YDB_CONNECTION_STRING` - Connection string from Step 4

**Note:** If your connection string contains `?` or `&` characters, wrap it in quotes.

**Make the function publicly accessible:**

```bash
yc serverless function allow-unauthenticated-invoke build-number-sync \
  --folder-id YOUR_FOLDER_ID
```


---

### Step 7: Get Function ID

**Get your function ID** (needed for API Gateway setup):

```bash
yc serverless function get build-number-sync --folder-id YOUR_FOLDER_ID
```

From the output, copy the `id` value.

**Example output:**
```
id: d4exxxxxxxxxxxxx
folder_id: b1gxxxxxxxxxxxxx
created_at: "2026-01-08T22:28:46.523Z"
name: build-number-sync
http_invoke_url: https://functions.yandexcloud.net/d4exxxxxxxxxxxxx
status: ACTIVE
```

Save the `id` value - you'll use it as `YOUR_FUNCTION_ID` in Step 8.

---

### Step 8: Set Up API Gateway

**API Gateway is required** to route requests to the function endpoints.

**Edit the `api-gateway.yaml` file:**

Open `api-gateway.yaml` and replace:
- `YOUR_FUNCTION_ID_HERE` with your function ID from Step 7
- `YOUR_SERVICE_ACCOUNT_ID_HERE` with your service account ID from Step 5

**Create the API Gateway:**

```bash
yc serverless api-gateway create \
  --name build-number-sync-api \
  --spec api-gateway.yaml \
  --folder-id YOUR_FOLDER_ID
```

**Get the API Gateway URL:**

```bash
yc serverless api-gateway get build-number-sync-api --folder-id YOUR_FOLDER_ID
```

Look for the `domain` field. This is your API endpoint.

**Example output:**
```
domain: d5dxxxxx.apigw.yandexcloud.net
```

Your API is now accessible at: `https://d5dxxxxx.apigw.yandexcloud.net`

---

## Setup and Deployment (Manual)

### Step 1: Create Folder

1. Open [Yandex Cloud Console](https://console.cloud.yandex.com/)
2. Select your cloud
3. Click **Create folder**
4. Enter name: `build-number-sync` (or any name you prefer)
5. Click **Create**
6. Open the created folder

---

### Step 2: Create YDB Database

1. Inside your folder, click **Create resource**
2. Select **YDB database**
3. Choose **Serverless** configuration
4. Enter database name: `build-number-sync-db`
5. Click **Create database**
6. Wait for the database status to become **RUNNING**

---

### Step 3: Get Database Connection String

1. Open the `build-number-sync-db` database
2. Go to **Overview** tab
3. Find the **Endpoint** parameter
4. Copy the endpoint value (e.g., `grpcs://ydb.serverless.yandexcloud.net:2135/?database=/ru-central1/b1gxxxxxxxxxxxxx/etnxxxxxxxxxxxxx`)
5. Save this value - you'll use it as `YDB_CONNECTION_STRING` in the function environment variables

---

### Step 4: Create Service Account

1. In the left menu, go to **Service accounts**
2. Click **Create service account**
3. Enter name: `build-number-sync-sa`
4. Click **Create**
5. Open the created service account
6. Go to **Overview** tab
7. **Copy the service account ID** - open `api-gateway.yaml` and replace `YOUR_SERVICE_ACCOUNT_ID_HERE` with this ID

---

### Step 5: Grant Service Account Access to YDB

1. Go back to your YDB database (`build-number-sync-db`)
2. Go to **Access bindings** tab
3. Click **Assign bindings**
4. Click **Select user**
5. Switch to **Service accounts** tab
6. Select `build-number-sync-sa`
7. Click **Add role**
8. Select role: `ydb.editor`
9. Click **Save**

---

### Step 6: Create and Deploy Function

1. In the left menu, go to **Cloud Functions**
2. Click **Create function**
3. Enter name: `build-number-sync`
4. Click **Create**
5. **Create version**
6. **Runtime environment:**
   - Runtime: `nodejs22`
   - Entry point: `index.handler`
   - Timeout: `10 sec`
   - Memory: `256 MB`
7. **Code:**
   - Method: **ZIP archive**
   - Upload the `Archive.zip` file from this directory
   - Or use **Code editor** and paste the contents of `index.js` and `package.json`
8. **Service account:** Select `build-number-sync-sa`
9. **Environment variables:**
   - Click **Add environment variable**
   - Key: `YDB_CONNECTION_STRING`
   - Value: (paste the endpoint value from Step 3)
10. Click **Save change**
11. Wait for build & deployment to complete
12. Go to **Overview** tab
13. **Copy the function ID** - open `api-gateway.yaml` and replace `YOUR_FUNCTION_ID_HERE` with this ID

---

### Step 7: Make Function Public

1. In the function page, go to **Overview** tab
2. In the **General information** section, click **Make public**
3. Confirm by clicking **Make public** in the dialog

---

### Step 8: Create API Gateway

1. In the left menu, go to **API Gateway**
2. Click **Create API gateway**
3. Enter name: `build-number-sync-api`
4. **Specification:**
   - Click **Upload file**
   - Select the `api-gateway.yaml` file (make sure you replaced the IDs in Steps 4 and 6)
5. Click **Create**
6. Wait for deployment to complete (~30 seconds)
7. Copy the **Service domain** URL (e.g., `https://d5dxxxxx.apigw.yandexcloud.net`)

Your API is now accessible at the copied domain URL.

---

## Development

To deploy a new version after making code changes:

**Navigate to the project directory:**

```bash
cd /path/to/yandex-cloud-function-nodejs
```

**Deploy the updated function:**

```bash
yc serverless function version create \
  --function-name build-number-sync \
  --runtime nodejs22 \
  --entrypoint index.handler \
  --memory 256m \
  --execution-timeout 10s \
  --source-path . \
  --service-account-id YOUR_SERVICE_ACCOUNT_ID \
  --environment "YDB_CONNECTION_STRING=YOUR_YDB_CONNECTION_STRING" \
  --folder-id YOUR_FOLDER_ID
```

Replace placeholders with your actual values from previous steps.

---

## Costs

### Free Tier

Yandex Cloud offers a free tier for Cloud Functions and YDB:

**Cloud Functions:**
- **1 million invocations per month**
- 10 GB-hours of compute time

**YDB Serverless:**
- **1 million document requests per month** (reads + writes combined)
- 1 GB storage

**API Gameway:**
- **1 million requests per month**

For typical build number sync usage, you'll likely stay within the free tier limits.

### Paid Pricing

**Pricing documentation:**
- Cloud Functions: https://cloud.yandex.com/en/docs/functions/pricing
- YDB: https://cloud.yandex.com/en/docs/ydb/pricing/serverless
- API Gateway: https://cloud.yandex.com/en/docs/api-gateway/pricing
