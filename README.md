# Build Number Sync UPM package

A small Unity package that keeps your project's build number in sync with an external API and exposes a runtime-friendly build number asset.

## Table of Contents

- [Overview](#overview)
- [User guide](#user-guide)
  - [Installation Options](#installation-options)
  - [Usage](#usage)
  - [Menu Commands](#menu-commands)
- [API](#api)
- [Deploying Your Own API](#deploying-your-own-api)
  - [Available Platforms](#available-platforms)
  - [What's Included](#whats-included)

## Overview
- The package provides an editor-side workflow to obtain and assign build numbers from a configurable API during build time, and a runtime provider to read the assigned build number.

## User guide

### Installation Options

- Add to your `manifest.json`:
    ```
    "com.jottoworol.build-number-sync": "https://github.com/JottoWorol/build-number-sync.git?path=/Packages/com.jottoworol.build-number-sync"
    ```

- Add package from Git URL via Unity Package Manager:
    ```
    https://github.com/JottoWorol/build-number-sync.git?path=/Packages/com.jottoworol.build-number-sync
    ```

### Usage

1) **Configure the API base URL**
   - Open **Tools → Build Number Sync → Create Settings Asset** to create a settings asset at `Assets/BuildNumberSyncSettings.asset`
   - Select the asset and set the **Api Base Url** field to your API endpoint
   - Leave it blank to use the default API URL

2) **Build your project**
   - Build the player using Unity's normal build process
   - During the build, the package will automatically contact the configured API and obtain a build number
   - The build number will be assigned to `PlayerSettings.iOS.buildNumber` or `PlayerSettings.Android.bundleVersionCode` depending on your target platform

3) **Read the build number at runtime**
   - At runtime, use the provided API to read the assigned build number:

    ```csharp
    using JottoWorol.BuildNumberSync.Runtime;
    
    public class BuildNumberDisplay : MonoBehaviour
    {
        void Start()
        {
            if (BuildNumberProvider.TryGetCurrentBuildNumber(out var buildNumber))
            {
                Debug.Log($"Build number: {buildNumber}");
            }
            else
            {
                Debug.Log("Build number not found.");
            }
        }
    }
    ```

### Menu Commands

The package provides additional menu commands under **Tools → Build Number Sync**:

- **Create Settings Asset** - Creates the settings asset if it doesn't exist
- **Open Settings** - Opens the settings asset in the Inspector
- **Pull & Assign New Build Number (Manual)** - Manually fetches and assigns a new build number without building
- **Push Current Build Number to Server** - Uploads your current build number to the server (useful for syncing after manual changes)

## API

The package expects a backend API that implements specific endpoints for managing build numbers.

**Default API:** `https://build-number-sync.jottoworol.top`

> **Note:** The default API is deployed on Cloudflare's edge network. Users in regions where Cloudflare services are restricted may experience connectivity issues. In such cases, consider deploying your own API instance using one of the available platforms below.

**API Documentation:** See [API.md](API.md) for the complete API contract and endpoint specifications.

---

## Deploying Your Own API

The `DeployProjects/` directory contains ready-to-deploy API implementations for multiple cloud platforms. Each project implements the required API contract and includes detailed deployment instructions.

### Available Platforms

- **[Cloudflare Workers](DeployProjects/cloudflare-worker-openapi/README.md)** - Edge computing platform with generous free tier (100K requests/day)
- **[Google Cloud Run](DeployProjects/google-cloud-run-function/README.md)** - Serverless containers with automatic Firestore integration (2M requests/month free)
- **[Yandex Cloud Functions](DeployProjects/yandex-cloud-function-nodejs/README.md)** - Serverless functions with YDB database (1M invocations/month free)

### What's Included?

Each deployment project contains:
- Complete, working API implementation
- Step-by-step deployment instructions (both CLI and manual/GUI)
- Platform-specific database/storage setup
- Cost information and pricing links
- No deep platform knowledge required - just follow the instructions

**Platform Recommendations:**
- **Cloudflare** - Best option if available in your region (no billing account or payment method required)
- **Google Cloud** - Fast setup with high global availability (requires billing account, but stays within free tier)
- **Yandex Cloud** - Alternative if other services are restricted or if you already have an account there

Simply choose your preferred platform, follow the deployment guide, and configure the Unity package to use your API URL.
