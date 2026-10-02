# Data Model: RSS Subscription Management

## Scope

The MVP stores only submitted subscription values. It does not fetch or parse feeds, persist subscriptions, or model feed content. The API host owns the authoritative state for its process lifetime; restarting that process clears the list.

## Entity: Subscription

| Field | Type | Required | Description |
|---|---|---:|---|
| `url` | string | Yes | The value submitted for the subscription. It must contain at least one non-whitespace character. Accepted values are preserved exactly as submitted. |

There are no optional entity fields. Do not add an identifier, timestamp, normalized URL, feed metadata, or feed-item fields for this MVP. Identical URL values represent separate subscription entries, distinguished by their occurrence in the ordered collection rather than by URL uniqueness.

## Validation and Preservation

- Reject a missing, `null`, empty, or whitespace-only `url`.
- Accept any other string. Do not validate or normalize its format, scheme, reachability, or feed contents.
- Preserve accepted values exactly: do not trim, change casing, decode, or otherwise transform them.
- Treat the submitted value as untrusted text. Display it as text; do not interpret it as HTML or automatically fetch it.
- A validation rejection or failed add must not change the collection. A rejected or failed value must not appear in the list.

## Collection, Duplicates, and Order

The API host maintains one in-memory ordered collection of `Subscription` entries.

- Every successful submission appends a new entry, including values identical to existing entries.
- Listing returns entries in append order.
- For concurrent additions, order is the order in which additions are accepted by the synchronized store.
- Listing and adding must be synchronized so concurrent requests do not corrupt state or produce a partial update.
- The collection is the source of truth. The frontend displays API-accepted entries and does not maintain a competing authoritative copy.

## Lifetime and State Transitions

The in-memory session is the lifetime of the API host process. Closing or reloading the browser alone does not clear the list while that process continues running. Stopping or restarting the API host clears all subscriptions.

| Current state | Event | Result |
|---|---|---|
| Empty or populated | List requested | Return the current ordered collection; state is unchanged. |
| Empty or populated | Non-whitespace value accepted | Append one new entry; retain all existing entries. |
| Empty or populated | Missing, null, empty, or whitespace-only value submitted | Reject the submission; state is unchanged. |
| Empty or populated | Add attempt fails before acceptance | Do not append an entry; retain existing entries and report failure to the user. |
| Any in-memory state | API host stops or restarts | Discard the collection; the next host process starts empty. |

A URL's format, scheme, reachability, or content is not a reason to reject an add. Feed retrieval, parsing, removal, persistence, and background refresh are outside this model.

## API Representation

The request and response representation contains only `url`:

```json
{ "url": "submitted value" }
```

`POST /api/subscriptions` accepts the request and returns the accepted subscription on success. `GET /api/subscriptions` returns an ordered JSON array of subscription objects, initially empty. These representations do not introduce addressable resources or imply unique URLs.

## Acceptance Requirement Mapping

| Acceptance requirement | Data-model rule |
|---|---|
| Scenario 1: Empty subscription view | A newly started API host has an empty collection. |
| Scenario 2: Add and view without manual refresh | Successful add appends an entry that the UI can display immediately after API acceptance. |
| Scenario 3: Add a different value without losing entries | Add appends; existing entries remain unchanged. |
| Scenario 4: Keep entries during the session and clear on restart | State lives in API-host memory and resets when that process stops or restarts. |
| Scenario 5: Reject empty or whitespace-only input | `url` is required and must contain a non-whitespace character. |
| Scenario 6: Accept repeated values as separate entries | No URL-based uniqueness constraint or deduplication; each successful submission appends. |
| Scenario 7: Failed add leaves state unchanged and reports failure | Failed operations append nothing; existing entries remain; the UI reports the failure. |
| Scenario 8: Show only the subscription URL, with no feed content | `url` is the only entity field; no fetching or parsing occurs. |
| FR-001, FR-006 | Validate only non-whitespace input; preserve all other values without URL checks or normalization. |
| FR-002, FR-003, FR-004 | Return the current ordered collection and append accepted entries without removing prior entries. |
| FR-005 | Subscription operations do not fetch or parse feed content. |
| FR-007 | In-memory collection lifetime is the API host process. |
| FR-008 | Rejected or failed adds do not alter the collection; failures are visible to the user. |
| FR-009 | Duplicate values remain distinct entries in insertion order. |
| SC-001, SC-002 | Distinct accepted values append and remain listed until the API host stops or restarts. |