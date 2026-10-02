# Research: RSS Subscription Management

**Date**: 2026-10-02

## Decisions

### Target Framework and API Style

- **Decision**: Target .NET 10 (`net10.0`) and use ASP.NET Core Minimal APIs for the subscription endpoints.
- **Rationale**: .NET SDK 10.0.401 is installed. The MVP needs only add and list operations; Minimal APIs avoid controller scaffolding that does not add value for this contract.
- **Alternatives considered**: ASP.NET Core controllers remain viable if the API grows to need MVC-specific filters or conventions; not needed for this MVP.
- **References**: [ASP.NET Core APIs overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis?view=aspnetcore-10.0); [Minimal API parameter binding](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-10.0).

### In-Memory State

- **Decision**: Use one API-host-lifetime subscription store backed by an ordered collection and synchronized for concurrent requests. Preserve duplicates and submitted values exactly, except reject null, empty, or whitespace-only values.
- **Rationale**: A singleton service keeps state authoritative in the API boundary and naturally resets when that process stops or restarts. Synchronization protects concurrent HTTP reads and writes while preserving insertion order.
- **Alternatives considered**: Static mutable state complicates isolation and testing; a database or distributed cache violates the MVP's in-memory scope. A set would incorrectly remove duplicates.
- **Session interpretation**: The API host process defines the in-memory session. Reloading or closing only the browser does not clear the list while the API remains running; stopping or restarting the API does. This makes the documented in-memory API responsibility precise.
- **References**: [ASP.NET Core dependency injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-10.0); [.NET thread-safe collections](https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/).

### API Contract

- **Decision**: Expose `GET /api/subscriptions` and `POST /api/subscriptions`. GET returns an ordered JSON array of subscription objects, initially empty. POST accepts a JSON object with `url`, returns the accepted object on success, and rejects only missing/blank values with a client error. Repeated values create separate entries.
- **Rationale**: This contract supports immediate UI updates and a subsequent authoritative list read without inventing resource identifiers or a feed domain model.
- **Alternatives considered**: `201 Created` with a `Location` header would imply addressable subscription resources not needed by this MVP; use a successful `200` response instead. Raw URL strings in GET were considered; a one-field `{ "url": "..." }` object keeps the request, response, and entity contract consistent.
- **Error handling**: The frontend displays a generic failure message and leaves its list unchanged if the HTTP request fails. URL format, scheme, reachability, and feed content are never validation conditions.
- **References**: [Minimal API responses](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-10.0); [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110.html); [RFC 9457 Problem Details](https://www.rfc-editor.org/rfc/rfc9457.html).

### Testing

- **Decision**: Use xUnit for store and API tests, `Microsoft.AspNetCore.Mvc.Testing` / `WebApplicationFactory` for the HTTP boundary, and bUnit for isolated Razor component behavior. Add a manual local browser smoke check for cross-origin behavior.
- **Rationale**: The acceptance criteria cross both API and UI behavior. API tests cover add/list and validation boundaries; component tests cover blank input, success, duplicate rendering, and failure feedback. A browser check is still needed because CORS is enforced by browsers, not by in-process .NET tests.
- **Alternatives considered**: Playwright would automate a full browser but adds setup beyond this proof of concept; begin with a documented smoke test and add browser automation only if needed. A mocking library is unnecessary; use a small fake service or test HTTP handler.
- **References**: [Minimal API testing](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/test-min-api?view=aspnetcore-10.0); [ASP.NET Core integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0); [Blazor testing](https://learn.microsoft.com/en-us/aspnet/core/blazor/test?view=aspnetcore-10.0); [bUnit getting started](https://bunit.dev/docs/getting-started/).

### Blazor UI and Local Development

- **Decision**: Use a single subscription page with explicit input, submission, list, and message state. Configure the API base URL rather than scattering it in component code. Permit only the documented frontend origins through API CORS.
- **Rationale**: The MVP has one user journey and the stakeholder documents define separate localhost origins and ports. A single page and narrow CORS policy keep the user flow and local integration straightforward.
- **Alternatives considered**: A multi-page navigation structure or broad wildcard CORS policy provides no MVP value. Playwright is deferred in favor of the required local browser smoke check.
- **Security note**: Render submitted URLs as text, never raw HTML or automatically active links for arbitrary schemes. Browser-delivered configuration must contain no secrets.
- **References**: [Blazor forms](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/?view=aspnetcore-10.0); [Call web APIs from Blazor](https://learn.microsoft.com/en-us/aspnet/core/blazor/call-web-api?view=aspnetcore-10.0); [Blazor configuration](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/configuration?view=aspnetcore-10.0); [ASP.NET Core CORS](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-10.0).

## Risks and Controls

- The empty workspace means project scaffolding is a prerequisite; use the stakeholder-documented paths and remove template demo routes before feature UI work.
- In-memory state belongs to the API process, not the browser tab. The quickstart must distinguish browser reload from API restart.
- Cross-origin configuration can fail due to mismatched localhost scheme, port, or CORS origin. Validate using the actual browser and configured ports.
- Arbitrary non-whitespace input must not be interpreted as HTML or fetched as a URL.