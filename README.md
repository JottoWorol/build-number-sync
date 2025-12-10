# Build Number Sync UPM package

A small Unity package that keeps your project's build number in sync with an external API and exposes a runtime-friendly build number asset.

## Overview
- The package provides an editor-side workflow to obtain and assign build numbers from a configurable API during build time, and a runtime provider to read the assigned build number.

## User guide

### Installation Options

- Add to your `manifest.json`:
    ```
    "com.jottoworol.build-number-sync": "https://github.com/JottoWorol/build-number-sync.git#v0.1.0?path=/Packages/com.jottoworol.build-number-sync"
    ```

- Add package from Git URL via Unity Package Manager:
    ```
    https://github.com/JottoWorol/build-number-sync.git#v0.1.0?path=/Packages/com.jottoworol.build-number-sync
    ```

### Usage

1) Configure the base URL
   - Open the config window (Tools → Build Number Sync → Config) and set the API base URL you want the project to use. If you leave it blank, the package uses the default base URL.

2) Build
   - Build the player using Unity's normal build process. During the build, the package will contact the configured API and obtain a build number which will be stored with the build.

3) Read the build number at runtime
   - At runtime, use the provided API to read the assigned build number. Example:

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

## API

### Default base URL
- Default API base URL: https://build-number-sync.jottoworol.top
- You can override the base URL in Tools → Build Number Sync → Config to point to your own implementation.

### Required Endpoints
- `GET /getNextBuildNumber?bundleId={bundleId}&platform={platform}`
  - Returns a fresh, incremented build number for the specified bundle id and platform.
  - Returns 1 if no build number exists yet for the given parameters.
  - Calling this endpoint increments the remotely stored build number.

- `GET /setBuildNumber?bundleId={bundleId}&platform={platform}&buildNumber={buildNumber}`
  - Sets the remote build number for the specified bundle id and platform.
  - Use it if you want to manually set or reset the remote build number.

Response format
- Both endpoints return JSON in the following format:
  ```json
  {
    "success": true|false,
    "buildNumber": <int>
  }
  ```

- `success` - whether the operation succeeded.
- `buildNumber` - latest synced build number.
