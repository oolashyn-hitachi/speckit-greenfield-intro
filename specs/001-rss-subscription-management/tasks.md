---
description: "Implementation tasks for RSS subscription management"
---

# Tasks: RSS Subscription Management

**Input**: Design documents from `specs/001-rss-subscription-management/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/subscriptions.openapi.yaml](contracts/subscriptions.openapi.yaml), [quickstart.md](quickstart.md)

**Tests**: Included because the project constitution requires automated acceptance coverage: xUnit and `WebApplicationFactory` for API behavior, bUnit for UI behavior, plus a local browser integration check.

**Organization**: Tasks are grouped by the single P1 user story so it can be implemented and validated as an independent MVP.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel with other marked tasks after their stated prerequisites.
- **[Story]**: User story label; setup and foundational tasks have no story label.
- Every task includes the exact affected file path or solution path.

## Phase 1: Setup

**Purpose**: Create the .NET solution and test project foundations.

- [X] T001 Create the .NET 10 solution and four projects at `RSSFeedReader.sln`, `backend/RSSFeedReader.Api/RSSFeedReader.Api.csproj`, `backend/RSSFeedReader.Api.Tests/RSSFeedReader.Api.Tests.csproj`, `frontend/RSSFeedReader.UI/RSSFeedReader.UI.csproj`, and `frontend/RSSFeedReader.UI.Tests/RSSFeedReader.UI.Tests.csproj`.
- [X] T002 Configure project references and test dependencies in `backend/RSSFeedReader.Api.Tests/RSSFeedReader.Api.Tests.csproj` and `frontend/RSSFeedReader.UI.Tests/RSSFeedReader.UI.Tests.csproj`, including xUnit, `Microsoft.AspNetCore.Mvc.Testing`, and bUnit compatible with .NET 10.

---

## Phase 2: Foundational

**Purpose**: Resolve the session boundary, local configuration, and template routing before story implementation.

- [X] T003 [P] Clarify in `specs/001-rss-subscription-management/spec.md` that the in-memory session ends when the API host stops or restarts; reloading or closing only the browser does not clear entries while the API remains running.
- [X] T004 [P] Configure the documented local launch ports in `backend/RSSFeedReader.Api/Properties/launchSettings.json` and `frontend/RSSFeedReader.UI/Properties/launchSettings.json` (`5151` for the API and `5213` for the UI).
- [X] T005 [P] Set `ApiBaseUrl` to `http://localhost:5151/api/` in `frontend/RSSFeedReader.UI/wwwroot/appsettings.json` and configure API CORS for `http://localhost:5213` and `https://localhost:7025` in `backend/RSSFeedReader.Api/Program.cs`.
- [X] T006 [P] Remove the Blazor template demo pages `frontend/RSSFeedReader.UI/Pages/Home.razor`, `frontend/RSSFeedReader.UI/Pages/Counter.razor`, and `frontend/RSSFeedReader.UI/Pages/Weather.razor`; remove the generated About toolbar in `frontend/RSSFeedReader.UI/Layout/MainLayout.razor`, update `frontend/RSSFeedReader.UI/Layout/NavMenu.razor`, and verify only one page owns the `/` route.
- [X] T007 Remove the generated `/weatherforecast` demo endpoint from `backend/RSSFeedReader.Api/Program.cs`, build the clean solution, and verify the route cleanup before feature work using `RSSFeedReader.sln` and `frontend/RSSFeedReader.UI/Pages/`.

**Checkpoint**: Both app projects build, local origins are configured, and the UI has no conflicting template routes.

---

## Phase 3: User Story 1 - Add and View Subscriptions (Priority: P1) 🎯 MVP

**Goal**: Let a local user submit non-whitespace feed URL values and immediately see successful additions, including duplicates, without fetching feed content.

**Independent Test**: Start with an empty list, add a non-whitespace value and verify it appears; add a second distinct value and verify both remain visible. Verify blank input is rejected, duplicate values appear separately, and no request is sent to a submitted feed URL.

### Tests for User Story 1

> Write and run these tests before feature implementation; confirm they fail for the missing behavior, then implement until they pass.

- [X] T008 [P] [US1] Add store unit tests for ordered append, duplicate preservation, exact preservation of arbitrary non-whitespace values, blank-only rejection without state changes, and concurrent add/list safety in `backend/RSSFeedReader.Api.Tests/SubscriptionStoreTests.cs`.
- [X] T009 [P] [US1] Add `WebApplicationFactory` tests for empty GET, POST then GET, ordering, duplicates, unchanged submitted values, and `400` responses for missing, null, empty, and whitespace-only values in `backend/RSSFeedReader.Api.Tests/SubscriptionsApiTests.cs`.
- [X] T010 [P] [US1] Add bUnit tests for the empty state, blank-input prompt, successful immediate list update, retained existing entries, duplicate rendering, and failed-add feedback without a list mutation in `frontend/RSSFeedReader.UI.Tests/SubscriptionPageTests.cs`.

### Implementation for User Story 1

- [X] T011 [US1] Define the one-field `Subscription` entity and nullable request DTO for omitted/null input handling in `backend/RSSFeedReader.Api/Models/Subscription.cs` and `backend/RSSFeedReader.Api/Models/AddSubscriptionRequest.cs`.
- [X] T012 [US1] Implement a process-lifetime ordered in-memory store with synchronized add/list operations; reject only null, empty, or whitespace-only values and preserve every accepted value unchanged in `backend/RSSFeedReader.Api/Services/InMemorySubscriptionStore.cs`.
- [X] T013 [US1] Register the store and implement `GET /api/subscriptions` and `POST /api/subscriptions` with the documented JSON shape, ordered results, duplicate support, `200` success, and `400` blank-input rejection in `backend/RSSFeedReader.Api/Program.cs`.
- [X] T014 [US1] Add the frontend `Subscription` representation and API client, then register the client using configured API settings in `frontend/RSSFeedReader.UI/Models/Subscription.cs`, `frontend/RSSFeedReader.UI/Services/SubscriptionsClient.cs`, and `frontend/RSSFeedReader.UI/Program.cs`.
- [X] T015 [US1] Implement the root subscription view with URL entry, empty/populated states, blank-input prompt, immediate display after API success, duplicate entries, and visible failure feedback; render values as text and do not trim, normalize, activate, or fetch them in `frontend/RSSFeedReader.UI/Pages/Subscriptions.razor`.

**Checkpoint**: User Story 1 passes its API and component tests and works through the configured local UI/API pair.

---

## Phase 4: Polish and Cross-Cutting Verification

**Purpose**: Confirm builds, automated coverage, and browser-only integration behavior.

- [X] T016 Run all automated tests and build the solution successfully with `dotnet test RSSFeedReader.sln` and `dotnet build RSSFeedReader.sln`.
- [X] T017 Follow `specs/001-rss-subscription-management/quickstart.md` in a browser; verify 10 distinct URL additions, five-entry retention, blank rejection, duplicates, failed-request feedback, no feed requests, browser-reload retention while the API runs, CORS behavior, and an empty list after API restart.
- [ ] T018 Conduct the unassisted first-time-user check described in `specs/001-rss-subscription-management/quickstart.md` with five participants and record the completion count and result; the criterion passes when at least four participants add a URL and confirm it without assistance.

## Dependencies and Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: T001 precedes T002; both are required before foundational or story work.
- **Foundational (Phase 2)**: T003-T006 can run in parallel after setup; T007 waits for all four and blocks story work.
- **User Story 1 (Phase 3)**: T008-T010 can be authored and run in parallel after foundation. Their expected red results precede implementation. T011 precedes T012, which precedes T013; T014 depends on T013; T015 depends on T014.
- **Polish (Phase 4)**: T016 and T017 depend on all US1 implementation and test tasks; run automated gates before the browser quickstart. T018 depends on T017 and access to five first-time participants.

### User Story Dependencies

- **User Story 1 (P1)**: Depends only on Setup and Foundational phases. There are no other user stories.

### Parallel Opportunities

- After T001-T002: T003, T004, T005, and T006 edit independent documentation/configuration/page files.
- After T007: T008, T009, and T010 target separate test projects and can be developed in parallel.
- Backend model/store/endpoint work and frontend client/view work are sequentially dependent as listed; keep the API contract stable between them.

## Parallel Example: User Story 1

```text
After T007, run the test-first work in parallel:
T008 backend store unit tests -> backend/RSSFeedReader.Api.Tests/SubscriptionStoreTests.cs
T009 API contract tests -> backend/RSSFeedReader.Api.Tests/SubscriptionsApiTests.cs
T010 UI behavior tests -> frontend/RSSFeedReader.UI.Tests/SubscriptionPageTests.cs
```

## Implementation Strategy

### MVP First

1. Complete Setup and Foundational phases, including the session-boundary clarification and Blazor route cleanup.
2. Write the US1 API and UI tests and confirm their expected failures.
3. Implement the model, API store and endpoints, frontend client, and subscription page.
4. Stop at the US1 checkpoint and validate the story independently.
5. Run the solution gates and quickstart browser checks; do not add deferred feed-reader features. Complete the five-participant usability check when participants are available.

### Incremental Delivery

User Story 1 is the complete MVP. No additional stories are defined in the spec; feed retrieval, persistence, removal, and polling remain out of scope.

## Notes

- Task IDs are sequential and every task uses the required checkbox format, with `[P]` only on independent work and `[US1]` only in the user-story phase.
- Automated test tasks are included to meet the project constitution's verification requirements.
- The task list follows the plan's API-host session boundary; T003 aligns the spec wording before implementation.
- T018 requires five human participants and cannot be completed by automated implementation checks alone.