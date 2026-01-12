# Build Number Sync API Documentation

This document describes the API contract that the Build Number Sync Unity package expects from any backend implementation.

## Overview

The API manages build numbers for different applications (identified by bundle ID) and platforms. It provides endpoints to retrieve incremented build numbers and manually set build numbers.

## Base URL

The default API base URL is: `https://build-number-sync.jottoworol.top`

You can configure a custom base URL in the Unity package settings to use your own deployment.

## Endpoints

### Get Next Build Number

Retrieves and increments the build number for a specific bundle ID and platform.

**Endpoint:** `GET /getNextBuildNumber`

**Query Parameters:**
- `bundleId` (required, string) - Application bundle identifier (e.g., `com.example.myapp`)
- `platform` (optional, string) - Platform name (e.g., `iOS`, `Android`, `StandaloneWindows64`). Defaults to `default` if not specified.

**Behavior:**
- Returns a fresh, incremented build number for the specified bundle ID and platform
- Returns `1` if no build number exists yet for the given parameters
- Calling this endpoint increments the remotely stored build number

**Response:**
```json
{
  "success": true,
  "buildNumber": 42
}
```

**Response Fields:**
- `success` (boolean) - Whether the operation succeeded
- `buildNumber` (integer) - The newly assigned build number

---

### Set Build Number

Manually sets the build number for a specific bundle ID and platform.

**Endpoint:** `POST /setBuildNumber`

**Query Parameters:**
- `bundleId` (required, string) - Application bundle identifier
- `platform` (optional, string) - Platform name. Defaults to `default` if not specified.
- `buildNumber` (required, integer) - The build number to set

**Behavior:**
- Sets the remote build number to the specified value
- Use this endpoint to manually set or reset the remote build number

**Response:**
```json
{
  "success": true,
  "buildNumber": 42
}
```

**Response Fields:**
- `success` (boolean) - Whether the operation succeeded
- `buildNumber` (integer) - The build number that was set

---

### Delete Bundle ID

Deletes all build number data for a specific bundle ID and platform.

**Endpoint:** `DELETE /deleteBundleId`

**Query Parameters:**
- `bundleId` (required, string) - Application bundle identifier
- `platform` (optional, string) - Platform name. Defaults to `default` if not specified.

**Behavior:**
- Deletes the stored build number for the specified bundle ID and platform combination
- Returns 404 if the bundle ID doesn't exist
- Use this endpoint to clean up old projects or reset data

**Response:**
```json
{
  "success": true,
  "message": "Bundle ID 'com.example.myapp' deleted successfully"
}
```

**Response Fields:**
- `success` (boolean) - Whether the operation succeeded
- `message` (string) - Confirmation message

**Error Response (404):**
```json
{
  "success": false,
  "message": "Bundle ID 'com.example.myapp' not found"
}
```

---

## Error Handling

In case of errors, the API should return:
- Appropriate HTTP status codes (e.g., 400 for bad request, 500 for server error)
- A JSON response with `"success": false` and optionally an `"error"` field describing the issue

Example error response:
```json
{
  "success": false,
  "error": "Missing required parameter: bundleId"
}
```

## Implementation Notes

- Build numbers are stored per combination of `bundleId` and `platform`
- The API should handle concurrent requests safely to avoid race conditions when incrementing build numbers
- Platform names are case-sensitive and should be stored exactly as provided

