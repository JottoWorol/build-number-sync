# Build Number Sync

Automatically syncs build numbers across your team using a cloud API.

## Quick Start

1. **Tools → Build Number Sync → Create Settings Asset** (leave API URL blank to use default)
2. Build your project - build numbers sync automatically
3. Read at runtime:

> **⚠️ Note:** The default public API is for testing only and not recommended for production use due to privacy concerns. For production, deploy your own API instance (see documentation below).

```csharp
using JottoWorol.BuildNumberSync.Runtime;

if (BuildNumberProvider.TryGetCurrentBuildNumber(out var buildNumber))
{
    Debug.Log($"Build: {buildNumber}");
}
```

## Features

- ✅ Auto-sync during builds (iOS, Android, WebGL, Windows, macOS, Linux, tvOS, PS4, WSA)
- ✅ Free public API (Cloudflare Workers)
- ✅ Runtime build number access
- ✅ Manual sync commands
- ✅ CI/CD friendly

## Documentation

**Full documentation:** https://github.com/JottoWorol/build-number-sync

Including:
- Complete setup guide
- API deployment instructions (Cloudflare, Google Cloud, Yandex Cloud)
- Platform support details
- API specification

