---
name: Check Endpoint Documentation Freshness
description: Verify that README endpoint contracts match the real Minimal API endpoint definitions.
---

Use this skill when you need to validate that endpoint documentation is synchronized with source contracts.

## What to check
- `MTGArchitectServices.ApiService/Endpoint.cs`
- `MTGArchitectServices.AuthApiService/Endpoint.cs`
- `README`

## Command
Run from repository root:

`powershell -ExecutionPolicy Bypass -File .\scripts\Check-EndpointContracts.ps1`

## Expected behavior
- Exit code `0`: documentation is up to date.
- Exit code `1`: differences are listed (missing or extra documented endpoints).
