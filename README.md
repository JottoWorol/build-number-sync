# Build Number Sync UPM package

A Unity package that automatically increments build numbers for each build by syncing with an external API.

## Table of Contents

- [Overview](#overview)
- [Supported Platforms](#supported-platforms)
- [User guide](#user-guide)
  - [Installation Options](#installation-options)
  - [Usage](#usage)
  - [Menu Commands](#menu-commands)
- [API](#api)
- [Deploying Your Own API](#deploying-your-own-api)
  - [Available Platforms](#available-platforms)
  - [What's Included](#whats-included)

## Overview

The package automatically increments your project's build number during the build process by fetching the next number from a configured API endpoint or storing it locally.

**Key Features:**
- Automatic build number increment during builds
- Support for iOS, Android, WebGL, and desktop platforms
- **Remote mode:** Fresh build number fetched via API
- **Local mode:** Increments build number locally (for solo developers, no remote connection required)
- Runtime access to the current build number

## Supported Platforms

The package automatically manages build numbers for:

- **iOS** - Updates `PlayerSettings.iOS.buildNumber`
- **Android** - Updates `PlayerSettings.Android.bundleVersionCode`
- **WebGL** - Updates the build portion of `PlayerSettings.bundleVersion`
- **Windows, macOS, Linux** - Updates `PlayerSettings.macOS.buildNumber`
- **tvOS** - Updates `PlayerSettings.tvOS.buildNumber`
- **WSA/UWP** - Updates `PlayerSettings.WSA.packageVersion`
- **PS4** - Updates `PlayerSettings.PS4.appVersion`

## User guide

### Installation Options

- Add to your `manifest.json`:
    ```
    "com.jottoworol.build-number-sync": "https://github.com/JottoWorol/build-number-sync.git?path=/Packages/com.jottoworol.build-number-sync#0.3.1"
    ```

- Add package from Git URL via Unity Package Manager:
    ```
    https://github.com/JottoWorol/build-number-sync.git?path=/Packages/com.jottoworol.build-number-sync#0.3.1
    ```

### Usage

#### Option 1: Local Storage Mode (Solo Development)

1) **Configure local storage mode**
   - Open **Tools → Build Number Sync → Create Settings Asset** to create a settings asset
   - Enable the **"Use Local Only"** checkbox
   - Build numbers will be stored locally in PlayerSettings (not synced with remote API)

2) **Build your project**
   - Build the player using Unity's build process
   - Build numbers increment automatically on your machine

#### Option 2: Remote Storage Mode (API sync)

1) **Configure the API base URL**
   - Open **Tools → Build Number Sync → Create Settings Asset** to create a settings asset at `Assets/BuildNumberSyncSettings.asset`
   - Keep **"Use Local Only"** unchecked (default)
   - Set the **Api Base Url** field to your API endpoint
   - Leave blank to use the default API URL

2) **Build your project**
   - Build the player using Unity's build process
   - The package fetches the next build number from the API during the build
   - The build number is assigned to the appropriate PlayerSettings field based on your target platform

#### Runtime Access

3) **Read the build number at runtime** (Optional)
   - Applicable for both local and remote modes
   - Access the build number from your code:

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
        }
    }
    ```

### Menu Commands

The package provides additional menu commands under **Tools → Build Number Sync**:

- **Create Settings Asset** - Creates the settings asset if it doesn't exist
- **Open Settings** - Opens the settings asset in the Inspector
- **Pull Next from Remote** - Manually fetches and assigns a new build number from remote API without building (remote storage only)
- **Push Current to Remote** - Uploads your current build number to the remote server (remote storage only)
- **Delete Remote Data** - Deletes build number data from the remote server (remote storage only)

## API

The package expects a backend API that implements specific endpoints for managing build numbers.

**Default API:** `https://build-number-sync.jottoworol.top`

> **Note:** The default API is deployed on Cloudflare's edge network. Users in regions where Cloudflare services are restricted may experience connectivity issues. In such cases, deploy your own API instance using one of the available platforms below. Each platform provides a public API URL that you can use directly - no custom domain required.

**API Documentation:** See [API.md](API.md) for the complete API contract and endpoint specifications.

---

## Deploying Your Own API

The `DeployProjects/` directory contains API implementations for multiple cloud platforms.

### Available Platforms

- **[Cloudflare Workers](DeployProjects/cloudflare-worker-openapi/README.md)** - Edge computing platform (100K requests/day free tier)
- **[Google Cloud Run](DeployProjects/google-cloud-run-function/README.md)** - Serverless containers with Firestore (2M requests/month free tier)
- **[Yandex Cloud Functions](DeployProjects/yandex-cloud-function-nodejs/README.md)** - Serverless functions with YDB (1M invocations/month free tier)

### What's Included

Each deployment project contains:
- API implementation
- Deployment instructions (CLI and GUI)
- Database/storage setup
- Pricing information

**Platform Notes:**
- **Cloudflare** - No billing account required
- **Google Cloud** - Requires billing account
- **Yandex Cloud** - Alternative option

Choose a platform, follow the deployment instructions, and configure the Unity package with your API URL.
