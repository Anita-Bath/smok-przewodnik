# Smok Przewodnik: Product and Technical Design

Status: Draft for team review  
Date: 2026-10-03  
Initial deployment: Krakow, Poland  
Target architecture: City-agnostic mobile platform

## 1. Scope, General Assumptions, and Architecture Overview

### 1.1 Product purpose

Smok Przewodnik is an accessibility-first city navigation platform. It combines
ordinary map and transport data with municipal data and current community
observations so that a user can evaluate an entire journey, not only its start
and destination.

The app is intended for people with permanent, temporary, or situational
accessibility needs, including:

- wheelchair and mobility-aid users;
- people with limited mobility;
- blind and low-vision users;
- Deaf and hard-of-hearing users;
- people with cognitive or intellectual disabilities;
- older adults;
- people recovering from injury;
- parents and carers travelling with a stroller;
- people whose needs combine several of the above.

The application must not assume that a disability label fully determines a
person's needs. It models individual constraints, transport capabilities,
feedback channels, and presentation preferences. Named presets are convenient
starting points only.

### 1.2 Core product scope

The product includes:

- anonymous map browsing and route planning;
- multiple route alternatives, such as fastest, easiest, quietest, or
  best-supported;
- user-controlled routing constraints with three states: `allowed`,
  `prefer_avoid`, and `must_avoid`;
- multimodal travel based on the user's actual transport capabilities;
- visual, audio, haptic, and voice-assisted navigation;
- simplified instructions, landmarks, pictograms, and reduced-complexity
  presentation;
- permanent places, infrastructure elements, obstacles, and time-bound events;
- an integrated itinerary planner and a personalized city information feed;
- reports, confirmations, denials, alternative route suggestions, and live
  rerouting;
- municipal, transport, venue, OpenStreetMap, and community data;
- offline area packs, saved routes, saved places, and bounded caches;
- contributor reputation and progressive trust;
- partner-funded digital rewards purchased with earned points;
- Polish and English localization, with Polish as the preferred locale for the
  Krakow deployment;
- reusable city adapters so the platform can expand beyond Krakow.

Accounts are required for synchronized preferences, saved places, suggestions,
contributions, confirmations, route history, points, and rewards. Browsing,
navigation, and configuration of accessibility and navigation preferences remain
available without an account. Guest preferences persist only on the device.
Saved places and suggestions always synchronize for an account. Precise journey
history is collected and synchronized only when the user enables it.

### 1.3 Scope boundaries and planned extensions

The architecture leaves room for, but does not initially depend on:

- camera-based recognition of obstacles for blind and low-vision users;
- caregiver or companion account relationships;
- physical reward inventory and shipping;
- predictive personalization based on complete journey history;
- fully automated moderation of safety-critical reports.

These are extensions rather than separate product directions. Their future
addition must reuse the same identity, spatial entity, evidence, trust, and
accessibility models.

### 1.4 General assumptions

- OpenStreetMap is the practical baseline for the street and place network.
- Municipal and transport data enrich the baseline where integrations exist.
- Community evidence supplies current accessibility details and corrects stale
  or incomplete source data.
- Every imported or contributed fact retains its source, observation time,
  confidence, and applicable validity period.
- Official data starts with strong provenance but is not treated as infallible.
- A recent cluster of community observations can dispute a currently incorrect
  official record.
- No `must_avoid` constraint is relaxed silently. If no compliant route exists,
  the user receives an explanation and chooses whether to relax a constraint.
- The interface never represents incomplete accessibility data as a guarantee.
- Trust and reward points are independent. Rewards cannot purchase credibility.
- The first reward catalog contains digital vouchers, tickets, discounts, and
  access codes rather than physical inventory.

### 1.5 Quick architecture overview

The system uses a modular .NET monolith as its single business boundary.
The Expo application calls the .NET API for all domain queries and commands.
Supabase provides authentication, PostgreSQL/PostGIS, and object storage.

Two deliberate direct mobile-to-Supabase flows are allowed:

1. Expo uses Supabase Auth for sign-in and session refresh, then sends the
   resulting bearer token to the .NET API.
2. Expo transfers report media directly to Supabase Storage using a short-lived
   upload target authorized and issued by .NET.

Expo does not query or mutate application database tables directly.

```text
                         +-------------------------+
                         | OSM / municipal / GTFS  |
                         | venue / mobility feeds  |
                         +------------+------------+
                                      |
                                      v
+------------+   HTTPS/JWT   +--------+---------+   SQL/PostGIS   +-----------+
| Expo app   +-------------->+ .NET modular API +---------------->+ Supabase  |
| iOS/Android|<--------------+                  |<----------------+ PostgreSQL|
+-----+------+  JSON/realtime +---+----------+---+                 +-----+-----+
      |                           |          |                           |
      | auth/session              |          | routing adapter           | media
      | signed media upload       |          v                           |
      +---------------------------+   +------+--------+                  |
                                      | Routing engine|                  |
                                      +---------------+                  |
      +---------------------------------------------------------------+
```

The API remains one deployable service while enforcing module boundaries for
profiles, spatial data, routing, navigation, planning, personalization,
contributions, trust, offline sync, rewards, media, and city adapters. A module
should be extracted into a separate service only when demonstrated scale or
ownership needs justify it.

## 2. Detailed Technical Architecture

### 2.1 Mobile application

The Expo/React Native application has five primary product areas:

- **Explore:** map, search, filters, accessible places, infrastructure, events,
  and nearby observations.
- **Navigate:** route comparison, active guidance, accessible feedback, live
  warnings, confirmations, and rerouting.
- **Contribute:** submit, confirm, deny, correct, or resolve spatial information.
- **Plan:** assemble places and events into a timed itinerary and calculate
  accessible travel between them.
- **Account:** accessibility profile, transport capabilities, synchronization,
  privacy controls, contribution state, points, and rewards.

Mobile code is organized by feature and uses an API client generated from or
checked against the backend OpenAPI contract. A local database stores offline
packs, cached server data, queued commands, saved routes, and user preferences.
This includes a complete device-local accessibility and navigation profile for
guests. Authentication tokens use platform-secure storage.

Navigation guidance is represented internally as structured events rather than
preformatted sentences. A single event can be rendered through several enabled
channels:

- visual instruction, high-contrast state, icon, or light cue;
- localized speech and optional repetition;
- a small documented haptic vocabulary;
- a constrained voice-command response.

No critical instruction may depend solely on color, sound, vibration, a map
gesture, or fine motor control. The app supports screen readers, scalable text,
logical focus order, reduced motion, large touch targets, and keyboard or switch
access where the platform exposes it.

### 2.2 API host and module boundaries

The .NET API contains the following modules:

#### Identity and Profiles

- links Supabase subjects to application accounts;
- synchronizes account-owned accessibility constraints and transport
  capabilities;
- synchronizes account-owned visual, audio, haptic, voice, and
  simplified-presentation preferences;
- applies journey-history consent and account privacy rules;
- manages roles for users, organizations, partners, moderators, and admins.

Guest profiles use the same settings contract but remain in mobile local storage
and are included directly in route requests. When a guest signs in, the app may
offer to copy the local profile into the account. It must not silently overwrite
an existing synchronized profile.

#### Spatial Catalog

- owns the shared spatial entity contract;
- exposes places, events, infrastructure, and obstacles;
- resolves source records into canonical entities;
- manages localized names and descriptions;
- stores structured accessibility facts and their provenance.

#### Routing

- normalizes requests into a city-neutral routing contract;
- calls a replaceable base-routing adapter;
- applies hard exclusions and soft accessibility costs;
- generates meaningfully different route candidates;
- produces route trade-offs, confidence, and freshness explanations;
- persists saved route snapshots when requested.

#### Navigation

- tracks short-lived active journey sessions;
- matches current or newly reported conditions against upcoming segments;
- emits accessible warning and confirmation events through SignalR while the
  app is in the foreground;
- requests rerouting without changing a route unexpectedly;
- exposes cursor-based recovery for missed navigation events;
- continues route guidance locally when the app is backgrounded or disconnected;
- avoids retaining location traces when history collection is disabled.

#### Planning and Personalization

- stores synchronized itineraries containing places, events, notes, and route
  legs;
- recalculates affected legs when time, accessibility data, or constraints
  change;
- produces destination suggestions without requiring complete raw journey
  history;
- assembles a localized feed from relevant events, official information,
  followed places, accessibility changes, and saved interests;
- records dismissals and relevance feedback without changing public trust.

#### Contributions and Trust

- accepts reports, corrections, confirmations, denials, and resolution evidence;
- detects duplicates and associates evidence with entities or graph segments;
- computes current confidence from immutable evidence;
- calculates contextual contributor reputation;
- supports disputes, expiry, appeals, and moderation.

#### City Data Adapters

- ingests OSM, transport, municipal, venue, and shared-mobility sources;
- stages and validates source records before publication;
- retains external identifiers, licenses, timestamps, and transform versions;
- isolates connector failure so one missing feed does not disable the platform.

#### Offline Sync

- publishes versioned area-pack manifests and incremental changes;
- accepts idempotent queued mobile commands;
- synchronizes account-owned data and consented history;
- reports cache age, compatibility, and invalidated routes.

#### Rewards

- owns the points account and append-only ledger;
- moves points through pending, spendable, spent, reversed, and expired states;
- exposes partner-funded digital catalog inventory;
- authorizes redemption and issues or reserves single-use codes;
- applies abuse controls independently of contributor trust.

#### Media

- validates upload intent, size, MIME type, and owning contribution;
- returns short-lived signed upload and read operations;
- processes moderation status and metadata after upload.

### 2.3 Authentication and authorization

Supabase Auth issues the mobile session. The Expo app sends the access token in
`Authorization: Bearer <token>` to .NET. The API validates issuer, audience,
expiry, signature, and required claims using the project's published signing
keys. It maps the stable token subject to the internal account.

The API distinguishes:

- unauthenticated guest access for browsing, search, packs, and route planning;
- device-local guest accessibility and navigation settings, which require no
  server authorization;
- authenticated user access for personal data and contributions;
- verified organization access for source-backed updates;
- partner access for reward inventory and fulfillment;
- moderator and administrator access for review and operations.

Authorization decisions belong to application policies, not controller code.
Supabase service credentials never enter the mobile bundle.

Reference: https://supabase.com/docs/guides/auth/jwts

### 2.4 City data ingestion and provenance

Each city deployment defines:

- city boundary, time zone, preferred and supported locales;
- available transport modes and networks;
- adapter configuration and refresh schedules;
- source attribution and licensing requirements;
- local taxonomy mappings without changing canonical taxonomy identifiers.

Imported records first enter source-specific staging tables. An ingestion run:

1. fetches or receives a versioned source payload;
2. validates shape, required fields, coordinates, and timestamps;
3. maps source categories to canonical codes;
4. matches or creates canonical spatial entities;
5. writes source assertions as accessibility facts or entity attributes;
6. recalculates affected projections and routing overlays;
7. publishes a change-set identifier for clients and offline packs.

Raw source assertions are not overwritten by community observations. A current
state projection combines them while preserving both pieces of evidence.

### 2.5 Route computation

A routing request includes origin, destination, enabled travel modes, constraint
states, route-profile preferences, locale, and known data versions.

The pipeline is:

1. Select a city and routing graph from coordinates.
2. Map accessibility facts and active obstacles onto graph nodes, edges, stops,
   vehicles, transfers, and destination entrances.
3. Remove edges or transitions violating `must_avoid` constraints.
4. Add normalized penalties for `prefer_avoid` constraints.
5. Generate several non-trivially different paths.
6. Score candidates across travel time, physical effort, surface, crossings,
   crowding, lighting, noise, transfers, accessibility confidence, and freshness.
7. Return a small Pareto-like set labelled by its dominant advantage rather than
   several nearly identical routes.

A normal route followed only by post-hoc rejection is not sufficient: it may
never discover an accessible detour. The initial base engine will therefore be
a self-hosted Valhalla deployment behind the routing adapter. Valhalla provides
OSM-based pedestrian, wheelchair, bicycle, automobile, and pedestrian/transit
routing, plus runtime costing and avoid-edge support. Early increments may use
its stock profiles plus accessibility-aware evaluation and strategic waypoints.
As the accessibility overlay matures, the adapter will supply exclusions and
the team can extend Valhalla costing or graph attribution without changing the
public API.

References:

- https://github.com/valhalla/valhalla/blob/master/docs/docs/api/openapi.yaml
- https://github.com/valhalla/valhalla/blob/master/docs/docs/concepts/index.md

If no path satisfies every hard constraint, the response contains no route. It
identifies the blocking constraints and may calculate hypothetical routes only
as explicit relaxation options for the user to approve.

### 2.6 Active navigation and realtime changes

An active journey uses a short-lived navigation session. The app reports enough
coarse progress to identify the upcoming route corridor. It does not need to
persist a continuous trace.

When new evidence affects upcoming segments, the server evaluates severity,
confidence, freshness, and distance. A material change emits a navigation event
through SignalR while the app is active in the foreground. The app presents the
warning through all appropriate enabled feedback channels and offers a
recalculated route. It never switches routes without user confirmation.

When the app is backgrounded, the operating system may suspend its live
connection. Turn-by-turn guidance therefore continues locally from the loaded
route and device location. Important server-side changes may produce a push
notification, but push delivery is best-effort and is not treated as a reliable
navigation event stream. On foregrounding or reconnecting, the app retrieves
missed events through the session cursor before resuming live updates.

Users approaching an existing observation may receive an unobtrusive confirm or
deny prompt. The prompt must be suppressible, safe to answer without fine motor
interaction, and postponed when the navigation context requires attention.

### 2.7 Progressive trust

Evidence is immutable. Corrections and resolutions add new observations rather
than rewriting earlier claims. Confidence projections consider:

- independent confirmations and denials;
- observation age and expected duration;
- source provenance;
- reporter accuracy in the relevant category;
- proximity and plausibility signals;
- duplicate or coordinated behavior;
- supporting media or institutional evidence;
- contradiction with other recent observations.

New reports are visible immediately as unverified. Confidence can rise, fall,
become disputed, or expire. Verified organizations receive stronger initial
provenance, but they cannot silently erase credible current community evidence.

### 2.8 Offline operation and synchronization

An area pack may contain:

- vector or raster map resources permitted by the selected provider;
- current accessibility projections and source metadata;
- places and infrastructure;
- public transport schedules;
- bounded upcoming-event and active-obstacle caches;
- localization resources required by the pack.

The mobile app does not embed a routing engine or download a routing graph.
While offline, it can navigate routes whose geometry, maneuvers, and relevant
accessibility data were saved or recently cached while connected. Planning a new
route or recalculating an existing route requires connectivity. If an obstacle
invalidates a stored route while offline, the app warns the user but does not
pretend that it can calculate a safe detour.

Account-owned saved places and suggestions synchronize automatically. Recent
route retention has a configurable limit. Raw or precise journey history only
synchronizes when its separate toggle is enabled.

Every offline command receives a client-generated idempotency key. Reports and
confirmations retain the time at which the user observed them. On reconnect,
the server accepts each command once and returns its canonical identifier and
resulting state. Rewards remain server-authoritative; an offline contribution
can display estimated pending points but cannot mint a balance locally.

Packs use manifests, content hashes, and incremental change sets. Time-sensitive
records carry expiry metadata. The UI shows when live data is unavailable and
when a route or pack is stale.

### 2.9 Rewards and digital shop

Useful contributions create pending reward entries. Validation rules promote
eligible entries to spendable points. Confirmations can earn smaller rewards,
subject to cooldowns, rate limits, and diminishing returns.

Partners publish digital catalog items with price, stock, validity, locale,
eligibility, and fulfillment metadata. Redemption is a server transaction that:

1. checks the spendable balance and account risk state;
2. reserves catalog inventory;
3. debits the append-only ledger;
4. assigns a single-use code or partner fulfillment reference;
5. reverses both inventory and points consistently on failure.

Contributor reputation is never derived from spendable points or purchases.

### 2.10 Localization, privacy, and reliability

Polish and English are complete product locales. Polish is the preferred Krakow
locale. User-facing strings use stable localization keys. Spoken guidance,
taxonomy labels, event fields, imported descriptions, date/time formats, and
plural rules are localization-aware.

Accessibility preferences are private account data. Public contributions use a
pseudonymous presentation by default. The system avoids exposing author-history
correlations that reveal a routine. Users can export data, delete an account,
delete optional journey history, report harmful content, and appeal moderation.

Failure behavior is explicit:

- unavailable live feeds fall back to dated cached data;
- an unavailable routing provider can fall back only to an already saved or
  recently cached route;
- queued uploads retry without duplicating commands;
- contradictory evidence produces a disputed state;
- unavailable output hardware falls back to other enabled channels;
- a missing optional city adapter does not take unrelated navigation offline.

## 3. Data Models, Contracts, and Endpoint Proposals

This section proposes stable domain shapes. Exact table names and DTO syntax may
change during implementation, but semantic ownership and identifiers should not.

### 3.1 Shared enums and value objects

```text
ConstraintLevel = allowed | prefer_avoid | must_avoid
EntityKind         = place | event | infrastructure | obstacle
GeometryKind       = point | line | polygon
ObservationVoteKind = confirm | deny | resolved | corrected
ConfidenceState    = unverified | supported | disputed | stale | resolved
RewardPointState   = pending | spendable | spent | reversed | expired
TravelMode         = walk | mobility_aid | bicycle | micromobility |
                     public_transport | car
FeedbackChannel    = visual | audio | haptic | voice
```

Canonical taxonomy codes are data, not application enums, when cities or source
adapters may extend them. Examples include `stairs`, `curb`, `rough_surface`,
`crowding`, `elevator_unavailable`, `missing_audio_signal`, `quiet_space`, and
`accessible_parking_misuse`.

### 3.2 Spatial entity composition

```text
SpatialEntity
  id: UUID
  cityId: UUID
  kind: EntityKind
  geometry: PostGIS geometry
  lifecycleState
  aggregateConfidence
  createdAt / updatedAt

EntityTranslation
  entityId, locale, name, description

EntitySourceLink
  entityId, sourceId, externalId, retrievedAt, licenseMetadata,
  transformVersion

SourceAssertion
  id, sourceId, externalId, assertionType, structuredPayload
  retrievedAt, validFrom, validUntil, transformVersion

GraphReference
  routingProvider, graphVersion, elementType, externalElementId

AccessibilityFact
  id, entityId or graphReference
  attributeCode
  typedValue + unit
  sourceAssertionId or observationId
  observedAt, validFrom, validUntil
  confidenceWeight
```

Type-specific one-to-one records compose the shared entity:

```text
PlaceDetails
  entityId, categoryCode, openingHours, contact, website

EventDetails
  entityId, organizerEntityId, startsAt, endsAt, bookingUri, capacity

InfrastructureDetails
  entityId, infrastructureCode, operationalState, maintenanceReference

ObstacleDetails
  entityId, obstacleCode, severity, expectedUntil, affectedTravelModes
```

Events and places remain separate even if an event occurs at a place. The event
references its venue and carries its own time-specific accessibility facts.

### 3.3 Profiles and preferences

```text
AccessibilityProfileSettings
  preferredLocale
  constraints: Map<TaxonomyCode, ConstraintLevel>
  transportCapabilities: Set<TravelMode>
  feedbackChannels: Set<FeedbackChannel>
  presentationPreferences
  updatedAt

AccountAccessibilityProfile
  accountId
  settings: AccessibilityProfileSettings
  journeyHistorySyncEnabled
  serverVersion
```

Presets produce an initial profile patch; they are not stored as the source of
truth. Users can modify every generated preference independently. Guests persist
`AccessibilityProfileSettings` locally and submit the effective settings with a
route request. Signed-in users persist the same settings locally and synchronize
them through `AccountAccessibilityProfile`.

### 3.4 Planning, suggestions, and feed

```text
Itinerary
  id, accountId, localizedTitle
  startsAt, endsAt, timeZone
  items[] { order, entityId, plannedArrival, plannedDeparture, note }
  routeLegReferences[]
  updatedAt

SuggestionProjection
  accountId, suggestedEntityId
  reasonCodes, score, generatedAt, expiresAt

FeedItem
  id, cityId, contentType
  sourceId, linkedEntityIds[]
  localizedContent
  publishedAt, validUntil
  audienceTags, provenance
```

An itinerary composes existing places, events, and route snapshots rather than
copying their details. Suggestions can use saved places, selected interests,
profile-compatible popularity, and explicitly consented journey history. Saved
places and generated suggestions always synchronize. Feed ranking may use
profile relevance but must not expose private profile attributes to publishers.

### 3.5 Contributions and trust

```text
Observation
  id, authorAccountId
  targetEntityId or targetGraphReference or proposedGeometry
  observationType, structuredPayload
  observedAt, expectedUntil
  mediaReferences
  moderationState
  idempotencyKey

EvidenceVote
  id, observationId, authorAccountId
  vote, observedAt, optionalPayload, idempotencyKey

ConfidenceProjection
  targetReference, state, score, computedAt, explanationCodes

ContributorReputationProjection
  accountId, categoryCode, cityId, score, evidenceCount, computedAt
```

An alternative route suggestion is an observation with line geometry and a
structured explanation of the segment it bypasses. Acceptance into the routing
graph requires validation rather than treating a drawn line as immediately safe.

### 3.6 Routes and navigation

Example route request:

```json
{
  "origin": { "latitude": 50.0617, "longitude": 19.9373 },
  "destination": { "entityId": "uuid" },
  "travelModes": ["mobility_aid", "public_transport"],
  "constraints": {
    "stairs": "must_avoid",
    "crowding": "prefer_avoid",
    "rough_surface": "prefer_avoid"
  },
  "profiles": ["fastest", "easiest", "quietest"],
  "locale": "pl-PL",
  "clientDataVersions": { "krakow": "change-set-id" }
}
```

Example route alternative shape:

```text
RouteAlternative
  id
  labelKey
  geometry
  legs[] / maneuvers[]
  duration, distance
  generalizedCostBreakdown
  accessibilitySummary
  preferenceTradeoffs[]
  confidenceSummary
  sourceVersions
  expiresAt
```

`accessibilitySummary` includes satisfied hard constraints, relevant unknowns,
and important facts. A successful route never contains a known hard-constraint
violation.

A structured navigation event contains event type, maneuver or hazard, urgency,
distance, landmark, localized parameter values, and supported feedback patterns.
The client decides how to render it through enabled channels.

### 3.7 Offline contracts

```text
AreaPackManifest
  cityId, areaId, version, createdAt
  minimumAppVersion
  resources[] { type, uri, hash, byteSize, expiresAt }
  baseVersionForDelta

SyncCommand
  idempotencyKey
  commandType
  occurredAt
  payload

SyncResult
  idempotencyKey
  status
  canonicalId
  serverVersion
  validationErrors[]
```

### 3.8 Rewards contracts

```text
RewardLedgerEntry
  id, accountId, state, amount, reasonCode
  sourceContributionId or redemptionId
  createdAt, effectiveAt, expiresAt

CatalogItem
  id, partnerId, localizedContent, pointPrice
  availableQuantity, validFrom, validUntil, eligibilityRules

Redemption
  id, accountId, catalogItemId, ledgerEntryId
  fulfillmentState, encryptedCodeReference, createdAt, fulfilledAt
```

Balances are projections of ledger entries, not independently editable values.

### 3.9 HTTP endpoint proposal

All routes are versioned under `/v1`. Cursor pagination is preferred for feeds.
Mutation endpoints accept `Idempotency-Key` where retry is expected.

#### Public and guest-capable

```text
GET  /v1/cities
GET  /v1/cities/{cityId}/configuration
GET  /v1/spatial-entities?bbox=&kinds=&categories=&updatedSince=
GET  /v1/spatial-entities/{entityId}
GET  /v1/places/{placeId}
GET  /v1/events?bbox=&from=&to=&categories=
GET  /v1/events/{eventId}
GET  /v1/feed?cityId=&cursor=
POST /v1/routes/plan
POST /v1/navigation/sessions
POST /v1/navigation/sessions/{sessionId}/progress
POST /v1/navigation/sessions/{sessionId}/reroute
GET  /v1/navigation/sessions/{sessionId}/events?afterSequence=
DELETE /v1/navigation/sessions/{sessionId}
GET  /v1/area-packs
GET  /v1/area-packs/{areaId}/manifest
GET  /v1/area-packs/{areaId}/changes?afterVersion=
```

Guest navigation sessions use short-lived opaque credentials and must not create
an account or durable journey record.

Area-pack endpoints are used only while connected to discover, download, and
incrementally refresh local data. Offline reads never call these endpoints; they
use the mobile database. Queued writes use the sync endpoints after connectivity
returns.

#### Authenticated user

```text
GET   /v1/me
GET   /v1/me/accessibility-profile
PUT   /v1/me/accessibility-profile
GET   /v1/me/saved-entities
PUT   /v1/me/saved-entities/{entityId}
DELETE /v1/me/saved-entities/{entityId}
GET   /v1/me/routes?cursor=
POST  /v1/me/routes/{routeId}/save
DELETE /v1/me/routes/{routeId}
PUT   /v1/me/privacy/journey-history
GET   /v1/me/suggestions
POST  /v1/me/suggestions/{suggestionId}/feedback
GET   /v1/me/itineraries
POST  /v1/me/itineraries
GET   /v1/me/itineraries/{itineraryId}
PUT   /v1/me/itineraries/{itineraryId}
DELETE /v1/me/itineraries/{itineraryId}
POST  /v1/me/itineraries/{itineraryId}/recalculate

POST  /v1/observations
GET   /v1/observations/{observationId}
POST  /v1/observations/{observationId}/votes
POST  /v1/spatial-entities/proposals
POST  /v1/events/proposals
POST  /v1/media/upload-authorizations

POST  /v1/sync/commands
GET   /v1/sync/changes?after=

GET   /v1/rewards/balance
GET   /v1/rewards/ledger?cursor=
GET   /v1/rewards/catalog?cursor=
POST  /v1/rewards/redemptions
GET   /v1/rewards/redemptions/{redemptionId}
```

#### Organizations, partners, and moderation

```text
POST /v1/organizations/{organizationId}/source-assertions
POST /v1/organizations/{organizationId}/events

POST /v1/partners/{partnerId}/catalog-items
PUT  /v1/partners/{partnerId}/catalog-items/{itemId}
GET  /v1/partners/{partnerId}/redemptions
POST /v1/partners/{partnerId}/redemptions/{id}/fulfill

GET  /v1/moderation/queue
POST /v1/moderation/cases/{caseId}/decisions
POST /v1/moderation/appeals/{appealId}/decisions
```

Partner and moderation operations may later use a separate web console, but the
contracts remain part of the same API boundary.

### 3.10 Live updates, push, and durable recovery

Realtime transport responsibilities are intentionally narrow:

- **SignalR:** foreground events for an active navigation session;
- **HTTP:** all commands, queries, area-pack changes, account synchronization,
  and recovery of missed events;
- **push notifications:** best-effort background alerts that invite the app to
  fetch current state;
- **local processing:** maneuver timing and enabled visual, audio, and haptic
  guidance from the currently loaded route.

The .NET API exposes an ASP.NET Core SignalR hub for active navigation. SignalR
prefers WebSockets and may negotiate a supported fallback transport. The client
uses bounded automatic reconnection and then switches to cursor recovery rather
than retrying indefinitely.

```text
navigation:{sessionId}   warning | reroute_available | source_status
```

Every navigation event contains a monotonically increasing session sequence.
The server retains events for the short lifetime of the navigation session. A
client that reconnects requests events after its last applied sequence through
the HTTP session-events endpoint, applies them in order, and then resumes the
SignalR stream. Event handlers are idempotent.

City-feed, entity, reward, moderation, and ordinary sync changes do not require
persistent SignalR subscriptions. They use cursor-based HTTP synchronization;
important account-level changes may additionally trigger a push notification.
Push payloads contain only a notification type and opaque reference, not precise
location, accessibility-profile data, or reward codes. The app fetches and
authorizes the current details after it opens.

SignalR and push notifications are delivery optimizations, never the durable
source of truth. Losing either channel cannot lose a domain change.

### 3.11 Verification strategy

- Unit tests cover constraint evaluation, route scoring, confidence decay,
  contextual reputation, ledger transitions, and redemption rules.
- Integration tests use PostgreSQL/PostGIS and exercise authentication, storage
  authorization, idempotency, synchronization, and geospatial queries.
- Contract tests protect every routing and city-data adapter.
- Mobile tests cover route comparison, active guidance, reporting, offline use,
  reconnect, itinerary recalculation, feed relevance, localization, and session
  recovery.
- Accessibility tests cover screen-reader semantics, focus order, dynamic text,
  contrast, touch targets, reduced motion, nonvisual operation, and output-channel
  fallback.
- End-to-end fixtures cover mobility, visual, hearing, cognitive, temporary
  injury, stroller, and mixed-needs profiles.
- Field testing with affected users validates haptic vocabulary, spoken guidance,
  reporting prompts, route explanations, and real-world safety assumptions.

Operational measures include route latency, pack size and update time, stale
data rates, adapter failures, report validation time, disputed-report rate,
rerouting reliability, reward reversals, and redemption fraud signals.

## 4. Milestones and Delivery Steps

These milestones define a dependency-aware delivery order. They do not narrow
the intended product scope. Each milestone should produce an integrated,
demonstrable increment and leave its module contracts usable by the next stage.

### Milestone 0: Engineering foundation

- Establish API module boundaries and shared contract conventions.
- Configure Supabase environments, PostGIS migrations, authentication, storage,
  secrets, and local development.
- Add mobile API client, local persistence, localization, and design-system
  accessibility primitives.
- Add CI checks for .NET, TypeScript, migrations, contracts, and tests.

### Milestone 1: Profiles and spatial catalog

- Implement account mapping and guest access.
- Implement composable accessibility profiles and presets for both device-local
  guests and synchronized accounts.
- Implement an explicit local-to-account profile import flow that does not
  overwrite existing synchronized settings without confirmation.
- Create spatial entity, translation, source, and accessibility-fact models.
- Ingest a bounded Krakow OSM dataset and expose map/search endpoints.
- Render places, infrastructure, obstacles, and events as distinct experiences.

### Milestone 2: Accessibility-aware route planning

- Implement the routing adapter and graph-reference model.
- Support travel capabilities and tri-state constraints.
- Generate and explain multiple route alternatives.
- Demonstrate hard exclusion, soft preference penalties, data confidence, and
  the no-valid-route flow.
- Add public guest routing and saved authenticated routes.

### Milestone 3: Accessible active navigation

- Implement structured navigation events.
- Render visual, spoken, haptic, and voice-assisted controls.
- Add simplified and landmark-based presentation.
- Add short-lived navigation sessions, progress, warnings, and user-approved
  rerouting.
- Add foreground SignalR delivery, cursor-based event recovery, and local
  guidance continuity when the connection or foreground session is lost.
- Validate the experience with representative accessibility profiles.

### Milestone 4: Community evidence and progressive trust

- Add reports, media, confirmations, denials, correction, and resolution flows.
- Publish unverified observations and confidence projections.
- Add contextual reputation, expiry, disputes, and organization provenance.
- Push new obstacle warnings into active navigation.
- Add abuse limits and an initial moderation queue.

### Milestone 5: Offline packs and synchronization

- Package Krakow map, place, transport-schedule, and accessibility data.
- Navigate saved and recently cached route geometry and instructions without a
  connection; require connectivity for new routes and rerouting.
- Support cached events and obstacles with visible freshness.
- Add idempotent offline contribution queues and incremental sync.
- Add saved places, bounded recent routes, and optional journey-history sync.
- Test interrupted downloads, reconnects, conflicts, and stale routes.

### Milestone 6: City and transport integrations

- Add Krakow municipal and public-transport adapters.
- Add scheduled and live feed status where sources permit.
- Implement staging, reconciliation, source attribution, and change sets.
- Prove city portability with a second minimal city configuration or fixture.

### Milestone 7: Planning and personalized discovery

- Build synchronized itineraries from places, events, notes, and route legs.
- Recalculate itinerary legs when schedules or accessibility facts change.
- Generate synchronized destination suggestions from privacy-permitted inputs.
- Add a localized feed for relevant events, official information, followed
  places, and accessibility updates.
- Cache an appropriate subset of itineraries and feed items for offline use.

### Milestone 8: Rewards and partner shop

- Implement pending and spendable points with an append-only ledger.
- Award points only after validation rules are satisfied.
- Add partner catalog management, digital inventory, and redemption.
- Add fraud signals, rate limits, reversals, and reconciliation.
- Keep trust projections isolated from reward value and spending.

### Milestone 9: Production hardening and expansion

- Complete privacy export/deletion, moderation appeals, and operational tooling.
- Run accessibility audits and field trials with target users.
- Load-test routing, ingestion, realtime, pack delivery, and redemption.
- Add observability, backups, recovery exercises, and adapter health reporting.
- Document the repeatable onboarding process for another city.

### Milestone acceptance principle

No milestone is complete only because its happy path works. Each increment must
include its failure states, accessibility checks, Polish and English content,
data provenance, authorization, and focused automated tests.
