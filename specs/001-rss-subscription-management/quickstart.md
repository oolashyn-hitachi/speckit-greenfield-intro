# Quickstart: RSS Subscription Management

## Before You Begin

This repository currently contains no source projects. Scaffold the backend and frontend projects before using these instructions to validate the implementation.

Use .NET 10. The expected project paths and local configuration are:

| Component | Expected path | Default URL |
|---|---|---|
| ASP.NET Core API | `backend/RSSFeedReader.Api` | `http://localhost:5151` |
| Blazor WebAssembly UI | `frontend/RSSFeedReader.UI` | `http://localhost:5213` |

Configure the frontend API base URL as `http://localhost:5151/api/`. Configure backend CORS to allow the frontend origins `http://localhost:5213` and `https://localhost:7025`, matching the configured launch settings.

## Build and Test

From the repository root, build each application:

```powershell
dotnet build backend/RSSFeedReader.Api
dotnet build frontend/RSSFeedReader.UI
```

Once the solution and test projects have been scaffolded, run the test suite from the repository root. Ensure the solution includes the API unit and integration tests and the Blazor component tests.

```powershell
dotnet test
```

## Run the Applications

Start the API in one terminal:

```powershell
dotnet run --project backend/RSSFeedReader.Api
```

Start the UI in a second terminal:

```powershell
dotnet run --project frontend/RSSFeedReader.UI
```

Open the frontend URL shown by the UI process. Confirm the API is listening at `http://localhost:5151` and that the frontend configuration points to that API base URL.

## Validate Subscription Management

1. With a newly started API, open the subscription view. The list should be empty, with a prompt or message indicating there are no subscriptions.
2. Submit one non-whitespace URL. It should appear in the list without a manual page refresh.
3. Submit a different URL. Both entries should remain visible.
4. Submit the first URL again. It should appear as a separate entry; duplicates are accepted.
5. Submit an empty value and a whitespace-only value. Neither should be added, and the UI should prompt for a URL. At the API boundary, invalid blank input returns `400`.
6. Confirm the API contract: `GET /api/subscriptions` returns an ordered JSON array of `{ "url": "..." }` items. `POST /api/subscriptions` accepts `{ "url": "..." }` and returns `200` with the accepted item. Missing or blank `url` is rejected with `400`.
7. Confirm no feed content is fetched or parsed. Adding a subscription should only make requests to the local API; the entered URL should not be requested, and no feed items should appear.
8. To check the failure message, load the UI while the API is running, stop the API, then submit a non-whitespace value. The UI should show a visible failure message and should not add the value to the list. Existing displayed entries should remain unchanged.
9. Reload the browser while the API is still running. The subscriptions should remain, because state belongs to the API process.
10. Stop and restart the API, then reload the UI. The list should be empty; restarting the API clears its in-memory state.

## Check Browser Networking

Open the browser's Developer Tools, select **Network**, and use the subscription workflow. Confirm requests go to the configured local API, successful list/add requests complete, and no request is made to a submitted feed URL. Check the **Console** and **Network** views for CORS errors or failed API requests. If CORS errors appear, verify the frontend's actual origin, the API base URL, and the backend's allowed CORS origins and ports.