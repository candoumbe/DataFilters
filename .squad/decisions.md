# Squad Decisions

## Active Decisions

### Issue #338 - NuGet Auditing Strategy for DataFilters

**Issue:** #338 — Enable NuGet auditing  
**Author:** Holden (Lead/Architect)  
**Date:** 2026-03-25  
**Status:** PROPOSED (awaiting team consensus)

#### Context

- DataFilters uses centralized package management via `Directory.Packages.props`.
- NuGet auditing is currently not active in CI.
- The project uses .NET SDK 10.0, so native NuGet audit support is available.

#### Decisions

- Audit all packages, direct and transitive (`NuGetAuditMode=all`).
- Start with `NuGetAuditLevel=high` (high + critical).
- Keep local developer builds warn-only.
- Fail CI and Release builds by promoting NU1900-NU1904 to errors.
- Handle suppressions with explicit justification and architect review.

#### Intended Central Configuration

```xml
<PropertyGroup>
	<NuGetAudit>true</NuGetAudit>
	<NuGetAuditMode>all</NuGetAuditMode>
	<NuGetAuditLevel>high</NuGetAuditLevel>
</PropertyGroup>

<PropertyGroup Condition="$(ContinuousIntegrationBuild) == 'true' OR '$(Configuration)' == 'Release'">
	<WarningsAsErrors>$(WarningsAsErrors);NU1900;NU1901;NU1902;NU1903;NU1904</WarningsAsErrors>
</PropertyGroup>
```

#### Process Notes

- CI failures must be resolved by package update, justified suppression, or architect escalation.
- Local users can inspect full vulnerability details with `dotnet list package --vulnerable`.
- A follow-up may raise the level to `moderate` after stabilization.

### Issue #338 - CI/CD Implementation Plan

**Issue:** #338 — Enable NuGet auditing  
**Author:** Amos (DevOps Engineer)  
**Date:** 2026-03-25  
**Status:** PROPOSED (ready for Holden + Naomi review)

#### Implementation Decisions

- Implement audit settings in `Directory.Packages.props` only, as centralized source of truth.
- Reuse existing CI signal from `core.props` (`GITHUB_ACTIONS` -> `ContinuousIntegrationBuild=true`).
- Do not change `integration.yml`, `delivery.yml`, or `build/Build.cs` for initial rollout.

#### Behavior Matrix

- Local default build: audit warnings visible, build succeeds.
- CI build: NU1900-NU1904 treated as errors, build fails on vulnerable packages.
- Release local build (`-c Release`): CI-like failure behavior for pre-push validation.

#### Suppression & Rollback

- Prefer per-CVE suppression via `NuGetAuditSuppress` with documented reason.
- Per-package `NoWarn` is allowed but less preferred.
- Rollback options defined: disable audit, lower strictness, or stop NU warning promotion.

### Issue #338 - Test Strategy

**Issue:** #338 — Enable NuGet auditing  
**Author:** Naomi (QA Engineer)  
**Date:** 2026-03-25  
**Status:** PROPOSED

#### Test Scope

- Audit detection for vulnerable packages.
- Severity handling (critical/high fail path, moderate/low warning path).
- CI blocking behavior versus local non-blocking behavior.
- Transitive dependency vulnerability detection.
- Suppression validation.

#### Test Organization

- Proposed dedicated test project: `test/DataFilters.NuGetAudit.Tests/`.
- Planned test files:
	- `AuditDetectionTests.cs`
	- `SeverityFilteringTests.cs`
	- `CIBlockingBehaviorTests.cs`
	- `LocalBehaviorTests.cs`
	- `TransitiveDependencyTests.cs`
	- `SuppressionTests.cs`
	- `EdgeCaseTests.cs`

#### Quality Goals

- Validate CI fail-fast on high/critical vulnerabilities.
- Validate local developer flow remains warn-only.
- Validate suppression and transitive behavior with repeatable scenarios.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
