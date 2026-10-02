# Implementation Plan: RSS Subscription Management

**Branch**: `001-rss-subscription-management` | **Date**: 2026-10-02 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `specs/001-rss-subscription-management/spec.md`

## Summary

Deliver the subscription-only MVP: a local user can submit any non-whitespace feed URL and immediately see each successful submission in a list, including duplicates. Build a .NET 10 ASP.NET Core Web API and Blazor WebAssembly UI as separate local projects. Keep the authoritative list in an in-memory API service; do not fetch, parse, validate, persist, or remove feeds. The list resets when the API process stops or restarts.

## Technical Context

**Language/Version**: C# on .NET 10 (`net10.0`); SDK 10.0.401 is installed

**Primary Dependencies**: ASP.NET Core Minimal APIs; Blazor WebAssembly; xUnit; `Microsoft.AspNetCore.Mvc.Testing` for API integration tests; bUnit for focused Razor component tests

**Storage**: Process-local in-memory subscription store; no durable storage

**Testing**: xUnit unit and API integration tests; bUnit component tests; local browser smoke test for cross-origin behavior

**Target Platform**: Local development on Windows, macOS, or Linux; browser frontend using WebAssembly

**Project Type**: Two-project web application (backend API and browser frontend), plus focused test projects

**Performance Goals**: No benchmark target; local add/list operations must update the visible list in the normal request-response flow

**Constraints**: Single local user; in-memory state; no outbound feed requests, parsing, URL-format validation, persistence, removal, or background polling. Keep API and UI origins/base URL/CORS aligned. Treat submitted values as text, not HTML.

**Scale/Scope**: One subscription form and list; small proof of concept; no multi-user, production-scale, or feed-content requirements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Security by Design — PASS**: MVP performs no outbound feed requests, displays submitted values as text, and introduces no secrets or URL-fetching behavior.
- **Maintainable Boundaries — PASS**: API owns subscription state and contract; Blazor owns form and list presentation. No shared business state or unnecessary project is planned.
- **Verifiable Quality — PASS**: API behavior and UI behavior receive automated coverage; the configured local API/UI flow receives a browser smoke test.
- **MVP Scope and Simplicity — PASS**: In-memory add/list only; deferred feed, persistence, removal, and polling features are excluded.
- **Clear and Predictable Behavior — PASS**: Empty input, successful adds, duplicates, and failed requests have explicit visible outcomes.
- **Complexity gate — PASS**: No constitution violations or additional runtime dependencies are justified beyond the selected .NET stack and focused test tooling.

### Post-Design Review

- **Security by Design — PASS**: The API contract and data model contain no feed-fetching operation; arbitrary submitted values are displayed as text.
- **Maintainable Boundaries — PASS**: The OpenAPI contract keeps API responsibilities separate from the single-page Blazor UI; state remains owned by the API host.
- **Verifiable Quality — PASS**: The plan includes API unit/integration coverage, component tests, and browser verification of CORS and local integration.
- **MVP Scope and Simplicity — PASS**: Design artifacts exclude URL-format checks, feed retrieval/parsing, durable storage, deduplication, removal, and polling.
- **Clear and Predictable Behavior — PASS**: Contracts and quickstart define empty input, successful add, duplicates, request failure, and process restart outcomes.
- **Complexity gate — PASS**: No design artifact introduces a constitution violation or an unjustified runtime dependency.

## Project Structure

### Documentation (this feature)

```text
specs/001-rss-subscription-management/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (planned; no application source exists yet)

```text
RSSFeedReader.sln
backend/
├── RSSFeedReader.Api/
└── RSSFeedReader.Api.Tests/
frontend/
├── RSSFeedReader.UI/
└── RSSFeedReader.UI.Tests/
```

**Structure Decision**: The workspace currently contains stakeholder documents and Spec Kit artifacts, but no application projects. Create the documented backend and frontend as separate .NET 10 projects, with one focused xUnit API test project and one bUnit UI test project. Remove Blazor template demo pages and conflicting routes before implementing the subscription page, as required by `StakeholderDocuments/TechStack.md`.

### Downstream Implementation Context

- No application source or repository-specific agent instruction file exists yet; scaffold the projects shown above before implementing feature behavior.
- Treat the feature spec as the scope authority and use `research.md`, `data-model.md`, and `contracts/subscriptions.openapi.yaml` as the design decisions for implementation.
- The API process lifetime defines the in-memory session. Browser reloads retain the list while the API is running; stopping or restarting the API clears it.
- Keep the configured API base URL, launch ports, and CORS origins aligned with `quickstart.md`; never send a request to a submitted feed URL.

## Complexity Tracking

No violations; no entries are required.
