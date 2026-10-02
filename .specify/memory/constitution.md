<!--
Sync Impact Report
Version change: Uninitialized template -> 1.0.0
Modified principles: Template placeholders replaced with five project principles
Added sections: Technology and Product Constraints; Development Workflow
Removed sections: None
Follow-up TODOs: None
-->

# RSS Feed Reader Constitution

## Core Principles

### I. Security by Design
The MVP MUST store and display submitted feed URLs without making outbound requests. All API input MUST be treated as untrusted, and secrets MUST NOT be committed or exposed in client-side configuration. Before feed fetching is added, requests MUST be limited to HTTP or HTTPS, checked against loopback, private, link-local, and metadata-service destinations, and rechecked after redirects. Fetching MUST have response-size and time limits. Feed-provided markup MUST NOT be rendered unless it has been sanitized. These controls prevent the reader from becoming a server-side request or content-injection risk as its scope grows.

### II. Maintainable Boundaries
The ASP.NET Core Web API MUST own subscription operations and their HTTP contract; the Blazor WebAssembly frontend MUST own presentation and user interaction. Business rules and authoritative state MUST NOT be duplicated between layers. Implementations MUST favor small, clearly named units and built-in platform capabilities. New dependencies require a concrete need that the existing stack cannot reasonably meet. Environment-specific endpoints MUST come from configuration rather than being scattered or hardcoded in application code.

### III. Verifiable Quality
Every behavior change MUST add or update automated tests for its acceptance criteria, using the repository's established test tooling. For the MVP, tests MUST cover adding a subscription and observing it in the returned/displayed list; API contract or cross-origin changes MUST include coverage of the affected boundary. Before a change is considered complete, the affected projects MUST build and relevant tests MUST pass. A test gap caused by missing project tooling MUST be recorded and resolved as part of the change, not silently treated as verification.

### IV. MVP Scope and Simplicity
The MVP is limited to adding a subscription by URL and displaying the subscription list. Subscriptions remain in memory; the MVP MUST NOT fetch or parse feeds, validate submitted URLs, persist data, remove subscriptions, or poll in the background. Features beyond this boundary require explicit scope approval and MUST be implemented as a separately described increment. This keeps the proof of concept small while leaving future work deliberate.

### V. Clear and Predictable Behavior
Adding a subscription MUST update the visible list immediately after the API accepts it. The interface MUST make the URL entry, add action, and current subscription list clear, and MUST communicate failures that prevent the requested action. Feed item display, rich-content rendering, and detailed feed diagnostics are outside the MVP. User-visible behavior and error messages MUST remain consistent with the API's actual result.

## Technology and Product Constraints

The project uses an ASP.NET Core Web API backend and a Blazor WebAssembly frontend. For the MVP, the backend exposes operations to add and list subscriptions and stores them in memory; the frontend provides the subscription form and list. Feed fetching and parsing using `System.ServiceModel.Syndication`, persistence, and background work are later increments, not implicit MVP requirements. Local development MUST keep the API base URL, frontend origin, launch ports, and backend CORS policy consistent. The application is intended to run locally and be developed and tested on Windows, macOS, and Linux.

## Development Workflow

Changes MUST be checked against the relevant principles and the current feature scope. Tests and builds MUST be run for affected backend and frontend projects; changes to the API/frontend integration MUST also be exercised together with the configured local origins. New routes or pages MUST be checked for route conflicts before feature work is considered complete. Reviews MUST confirm that tests cover changed behavior, configuration is not unnecessarily hardcoded, and deferred features have not entered the MVP unintentionally. Any exception MUST state its rationale and impact in the change review.

## Governance

This constitution governs project decisions and takes precedence over conflicting implementation guidance. Amendments MUST update this document, state the reason for the change, and receive review with the affected work. Use semantic versioning: increment MAJOR for incompatible governance changes, MINOR for new or materially expanded principles or sections, and PATCH for clarifications that do not change obligations. The Last Amended date MUST be updated whenever the constitution changes. Each change review MUST check compliance with applicable principles; deviations require an explicit, reviewed exception or a constitution amendment. The project constitution MUST be rechecked when scope, architecture, or security-sensitive behavior changes.

**Version**: 1.0.0 | **Ratified**: 2026-10-02 | **Last Amended**: 2026-10-02
