# Domain Models for Specification Sections 3.1-3.7 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the dependency-free Domain entities, aggregates, value objects, projections, and contract models described by specification sections 3.1-3.4, 3.6, and 3.7.

**Architecture:** Mutable business concepts are entities or aggregate roots with private state and invariant-preserving methods. Immutable descriptive concepts, read projections, routing results, navigation events, and offline contracts are records or value objects. PostgreSQL/PostGIS, EF Core, ASP.NET, and JSON transport configuration remain outside Domain.

**Tech Stack:** .NET 8, C# 12, xUnit 2.9.3, `System.Text.Json`; no new NuGet dependencies.

**Spec:** `docs/superpowers/specs/2026-10-03-smok-przewodnik-design.md`, sections 3.1-3.4, 3.6, and 3.7.

## Global Constraints

- Keep all implementation in `api/src/AB.SmokPrzewodnik.Domain` and tests in `api/tests/AB.SmokPrzewodnik.Domain.UnitTests`.
- Preserve inward-only Clean Architecture dependencies; Domain references no other project and no persistence or transport package.
- Use `Guid` entity identifiers, `DateTimeOffset` instants, `TimeSpan` durations, and `decimal` distances/scores.
- Constructors accept identifiers for rehydration; static `Create(...)` factories generate identifiers for new aggregates and entities.
- Expose collections as read-only views and mutate aggregate-owned collections only through domain methods.
- Use enums only for closed application-owned sets. Use validated string value objects for extensible taxonomy, provider, reason, content, resource, and command codes.
- Use dependency-free geometry value objects; PostGIS conversion belongs to Infrastructure.
- Clone `JsonElement` values on entry so domain payloads do not depend on a disposed `JsonDocument`.
- Reject invalid coordinates, empty required codes, invalid time ranges, scores outside `0..1`, negative quantities, and contradictory discriminated references.
- Do not introduce EF Core attributes, JSON attributes, ASP.NET types, CQRS, mediator, validation libraries, or mapping libraries.
- Do not commit changes.

## Review Focus

- A `JsonElement` created from a disposed document remains readable after entering `SourcePayload`, `ObservationPayload`, or `SyncPayload`.
- A route request cannot contain invalid coordinates, no travel mode, or a duplicate requested route profile.
- An itinerary cannot contain duplicate positions, an arrival after departure, or items outside its declared time window.
- A spatial detail type must match the owning `SpatialEntity.Kind`; mismatches are rejected by the aggregate.
- An area-pack delta cannot identify itself as its own base version, and resource byte sizes cannot be negative.

---

## Planned File Structure

```text
api/src/AB.SmokPrzewodnik.Domain/
  Common/
    AuditableEntity.cs
    Guard.cs
  Confidence/
    ConfidenceAssessment.cs
  Enums/
    ConfidenceState.cs
    ConstraintLevel.cs
    EntityKind.cs
    FeedbackChannel.cs
    GeometryKind.cs
    GraphElementType.cs
    LifecycleState.cs
    ObservationVoteKind.cs
    RewardPointState.cs
    TravelMode.cs
  ValueObjects/
    Code.cs
    GeoCoordinate.cs
    LocalizedContent.cs
    SpatialGeometry.cs
  Spatial/
    AccessibilityFact.cs
    AccessibilityFactTarget.cs
    AccessibilityValue.cs
    EntitySourceLink.cs
    EntityTranslation.cs
    EvidenceReference.cs
    GraphReference.cs
    SourceAssertion.cs
    SourceLicenseMetadata.cs
    SourcePayload.cs
    SpatialEntity.cs
    Details/PlaceDetails.cs
    Details/EventDetails.cs
    Details/InfrastructureDetails.cs
    Details/ObstacleDetails.cs
    Details/SpatialEntityDetails.cs
  Profiles/
    AccessibilityProfileSettings.cs
    AccountAccessibilityProfile.cs
    PresentationPreferences.cs
  Planning/
    FeedItem.cs
    Itinerary.cs
    ItineraryItem.cs
    SuggestionProjection.cs
  Routing/
    AccessibilitySummary.cs
    CostComponent.cs
    Maneuver.cs
    RouteAlternative.cs
    RouteEndpoint.cs
    RouteLeg.cs
    RoutePlanRequest.cs
  Navigation/
    NavigationEvent.cs
    NavigationEventType.cs
    NavigationUrgency.cs
  Offline/
    AreaPackManifest.cs
    AreaPackResource.cs
    SyncCommand.cs
    SyncPayload.cs
    SyncResult.cs
    SyncStatus.cs
```

Existing `Entities/*` types are moved into the focused namespaces above while preserving their public concepts. Call sites and tests use the new namespaces; no compatibility aliases are added because the types have not been released.

### Task 1: Repair foundations and complete shared value objects

**Files:**
- Modify: `api/Directory.Build.props`
- Modify: `api/src/AB.SmokPrzewodnik.Domain/Common/AuditableEntity.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/Common/Guard.cs`
- Modify: `api/src/AB.SmokPrzewodnik.Domain/Confidence/ConfidenceAssessment.cs`
- Modify: `api/src/AB.SmokPrzewodnik.Domain/Enums/ConfidenceState.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/Enums/RewardPointState.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/ValueObjects/Code.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/ValueObjects/GeoCoordinate.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/ValueObjects/SpatialGeometry.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/ValueObjects/LocalizedContent.cs`
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Common/AuditableEntityTests.cs`
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/ValueObjects/SharedValueObjectTests.cs`
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Confidence/ConfidenceAssessmentTests.cs`

**Interfaces:**
- Produces: `Code`, `GeoCoordinate`, `SpatialGeometry`, `LocalizedContent`, and validated `ConfidenceAssessment` used by every later task.

- [ ] **Step 1: Add failing foundation tests**

Test that `AuditableEntity<TId>` forwards its ID, begins with `UpdatedAt == null`, and updates its timestamp through `MarkUpdated(DateTimeOffset)`. Test that blank `Code` values, coordinates outside latitude `[-90, 90]` or longitude `[-180, 180]`, empty geometry coordinate sets, invalid polygon closure, empty localized content, confidence scores outside `0..1`, and negative evidence counts are rejected. Test value equality for valid objects.

- [ ] **Step 2: Run the focused tests and verify RED**

Run: `dotnet test api/tests/AB.SmokPrzewodnik.Domain.UnitTests --no-restore -m:1 --filter "FullyQualifiedName~AuditableEntityTests|FullyQualifiedName~SharedValueObjectTests|FullyQualifiedName~ConfidenceAssessmentTests"`

Expected: compilation or assertion failure because the APIs and validation do not yet exist.

- [ ] **Step 3: Implement the shared foundations**

Make `AuditableEntity<TId>` abstract, accept optional rehydrated timestamps, and expose `MarkUpdated`. Add internal `Guard` helpers for non-empty strings, time ranges, and decimal ranges. Implement:

```text
Code(string Value)
GeoCoordinate(decimal Latitude, decimal Longitude)
SpatialGeometry(GeometryKind Kind, IReadOnlyList<GeoCoordinate> Coordinates)
LocalizedContent(IReadOnlyDictionary<string, string> Values)
ConfidenceAssessment(ConfidenceState State, decimal Score,
                     uint EvidenceCount, DateTimeOffset EvaluatedAt)
```

Remove `Resolved` from `ConfidenceState`; add the spec's unused-but-shared `RewardPointState`. Add `**/.#*` to `DefaultItemExcludes` so editor lock symlinks are never compiled.

- [ ] **Step 4: Run the focused tests and verify GREEN**

Run the Step 2 command. Expected: all focused tests pass.

### Task 2: Complete spatial provenance and accessibility models

**Files:**
- Move and modify the existing source/accessibility types from `Domain/Entities` into `Domain/Spatial`
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Spatial/SourceModelTests.cs`
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Spatial/AccessibilityFactTests.cs`

**Interfaces:**
- Consumes: `Code`, `AuditableEntity<TId>`, and `Guard` from Task 1.
- Produces: immutable `GraphReference`, source evidence models, typed accessibility values, and validated `AccessibilityFact`.

- [ ] **Step 1: Add failing spatial evidence tests**

Test constructor initialization, blank external/provider/version rejection, graph-reference value equality, valid-range ordering, URI/license preservation, JSON payload survival after source document disposal, and `ConfidenceWeight` boundaries. Test every `AccessibilityFactTarget`, `EvidenceReference`, and `AccessibilityValue` variant.

- [ ] **Step 2: Run the focused tests and verify RED**

Run: `dotnet test api/tests/AB.SmokPrzewodnik.Domain.UnitTests --no-restore -m:1 --filter "FullyQualifiedName~SourceModelTests|FullyQualifiedName~AccessibilityFactTests"`

- [ ] **Step 3: Implement and organize the models**

Use these public shapes:

```text
EntitySourceLink(Guid id, Guid entityId, Guid sourceId, string externalId,
                 DateTimeOffset retrievedAt, SourceLicenseMetadata license,
                 string transformVersion, timestamps...)
SourceAssertion(Guid id, Guid sourceId, string externalId, Code assertionType,
                SourcePayload payload, DateTimeOffset retrievedAt,
                DateTimeOffset? validFrom, DateTimeOffset? validUntil,
                string transformVersion)
GraphReference(string routingProvider, string graphVersion,
               GraphElementType elementType, string externalElementId)
AccessibilityFact(Guid id, AccessibilityFactTarget target, Code attributeCode,
                  AccessibilityValue value, EvidenceReference evidence,
                  DateTimeOffset observedAt, DateTimeOffset? validFrom,
                  DateTimeOffset? validUntil, decimal confidenceWeight)
```

`SourcePayload` owns a cloned `JsonElement`. Preserve discriminated nested records for targets, evidence, and values. Remove public setters.

- [ ] **Step 4: Run the focused tests and verify GREEN**

Run the Step 2 command. Expected: all focused tests pass.

### Task 3: Implement the spatial aggregate and detail composition

**Files:**
- Move and modify: existing `SpatialEntity.cs`, `EntityTranslation.cs`, and `TaxonomyCode.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/Spatial/Details/SpatialEntityDetails.cs`
- Create: detail files listed in Planned File Structure
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Spatial/SpatialEntityTests.cs`

**Interfaces:**
- Consumes: Task 1 value objects and Task 2 provenance types.
- Produces: the `SpatialEntity` aggregate and its four kind-specific detail records.

- [ ] **Step 1: Add failing aggregate tests**

Test `Create` ID generation, rehydration with an existing ID, geometry and confidence initialization, translation uniqueness by normalized locale, detail-kind matching, source-link uniqueness by `(SourceId, ExternalId)`, lifecycle transitions, confidence replacement, and read-only collection exposure.

- [ ] **Step 2: Run `SpatialEntityTests` and verify RED**

Run: `dotnet test api/tests/AB.SmokPrzewodnik.Domain.UnitTests --no-restore -m:1 --filter FullyQualifiedName~SpatialEntityTests`

- [ ] **Step 3: Implement details and aggregate behavior**

Define `SpatialEntityDetails` with required `EntityKind Kind`, then:

```text
PlaceDetails(Code CategoryCode, string? OpeningHours, ContactDetails? Contact, Uri? Website)
EventDetails(Guid? OrganizerEntityId, DateTimeOffset StartsAt,
             DateTimeOffset EndsAt, Uri? BookingUri, uint? Capacity)
InfrastructureDetails(Code InfrastructureCode, Code OperationalState,
                      string? MaintenanceReference)
ObstacleDetails(Code ObstacleCode, uint Severity,
                DateTimeOffset? ExpectedUntil,
                IReadOnlySet<TravelMode> AffectedTravelModes)
```

Implement `SpatialEntity : AuditableEntity<Guid>` with `CityId`, `Kind`, `SpatialGeometry`, `LifecycleState`, `ConfidenceAssessment`, optional matching details, and owned translation/source-link collections. Use explicit methods such as `SetDetails`, `UpsertTranslation`, `AddSourceLink`, `ChangeLifecycle`, and `UpdateConfidence`.

- [ ] **Step 4: Run `SpatialEntityTests` and verify GREEN**

Run the Step 2 command. Expected: all tests pass.

### Task 4: Implement accessibility profiles

**Files:**
- Create: files under `Domain/Profiles` listed above
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Profiles/AccessibilityProfileTests.cs`

**Interfaces:**
- Consumes: `Code`, `ConstraintLevel`, `TravelMode`, and `FeedbackChannel`.
- Produces: device-local `AccessibilityProfileSettings` and synchronized `AccountAccessibilityProfile` aggregate.

- [ ] **Step 1: Add failing profile tests**

Test default-empty collections, constraint upsert/removal, travel capability and feedback-channel replacement, duplicate elimination, locale validation, presentation preference replacement, account/profile ID preservation, journey-history toggle, monotonically increasing server version, and read-only collection exposure.

- [ ] **Step 2: Run profile tests and verify RED**

Run: `dotnet test api/tests/AB.SmokPrzewodnik.Domain.UnitTests --no-restore -m:1 --filter FullyQualifiedName~AccessibilityProfileTests`

- [ ] **Step 3: Implement profile models**

Use `AccessibilityProfileSettings` as a mutable value owned by the profile with `IReadOnlyDictionary<Code, ConstraintLevel>`, `IReadOnlySet<TravelMode>`, `IReadOnlySet<FeedbackChannel>`, `PresentationPreferences`, locale, and `UpdatedAt`. Model presentation settings as an immutable record containing high contrast, large text, reduced motion, simplified guidance, and text scale. Implement `AccountAccessibilityProfile : AggregateRoot<Guid>` with `AccountId`, settings, consent toggle, and `long ServerVersion`.

- [ ] **Step 4: Run profile tests and verify GREEN**

Run the Step 2 command. Expected: all tests pass.

### Task 5: Implement planning, suggestion, and feed models

**Files:**
- Create: files under `Domain/Planning` listed above
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Planning/ItineraryTests.cs`
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Planning/ProjectionTests.cs`

**Interfaces:**
- Consumes: `LocalizedContent`, `Code`, and aggregate primitives.
- Produces: `Itinerary` aggregate plus immutable suggestion and feed projections.

- [ ] **Step 1: Add failing planning tests**

Test itinerary time-zone and range validation, item add/update/remove/reorder, duplicate position rejection, item arrival/departure ordering, route-leg reference replacement, read-only collections, suggestion score range, feed validity range, and non-empty linked entity/audience collections.

- [ ] **Step 2: Run planning tests and verify RED**

Run: `dotnet test api/tests/AB.SmokPrzewodnik.Domain.UnitTests --no-restore -m:1 --filter "FullyQualifiedName~ItineraryTests|FullyQualifiedName~ProjectionTests"`

- [ ] **Step 3: Implement planning models**

Implement `Itinerary : AggregateRoot<Guid>` owned by an account, with localized title, start/end, IANA time-zone identifier, itinerary items, route-leg references, and update methods. `ItineraryItem` is an immutable record keyed by its own `Guid` with `Position`, entity ID, optional arrival/departure, and note. `SuggestionProjection` and `FeedItem` are immutable records using `Code` collections and `LocalizedContent`; their constructors enforce score and validity invariants.

- [ ] **Step 4: Run planning tests and verify GREEN**

Run the Step 2 command. Expected: all tests pass.

### Task 6: Implement route and navigation contract models

**Files:**
- Create: files under `Domain/Routing` and `Domain/Navigation` listed above
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Routing/RouteModelTests.cs`
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Navigation/NavigationEventTests.cs`

**Interfaces:**
- Consumes: geometry, codes, profile constraint enums, confidence values, and localized parameter values.
- Produces: immutable route-planning input/output and structured navigation events.

- [ ] **Step 1: Add failing routing/navigation tests**

Test coordinate/entity endpoint alternatives, request validation, read-only constraint/profile/version maps, nonnegative distance/duration/cost values, non-empty alternatives, leg/maneuver ordering, expiry after creation, navigation-event sequence and distance validation, and supported feedback-pattern preservation.

- [ ] **Step 2: Run routing/navigation tests and verify RED**

Run: `dotnet test api/tests/AB.SmokPrzewodnik.Domain.UnitTests --no-restore -m:1 --filter "FullyQualifiedName~RouteModelTests|FullyQualifiedName~NavigationEventTests"`

- [ ] **Step 3: Implement immutable contract records**

Model `RouteEndpoint` as coordinate or entity discriminated records. Define `RoutePlanRequest` with origins/destinations, a non-empty travel-mode set, constraints, requested profile codes, locale, and client data versions. Define alternatives from ordered legs and maneuvers, `SpatialGeometry`, duration, distance metres, cost components, accessibility summary, trade-off codes, confidence summary, source versions, and expiry. Define navigation events with session ID, positive sequence, event type, urgency, optional maneuver/hazard/landmark, remaining distance, localized parameter map, and supported feedback patterns.

- [ ] **Step 4: Run routing/navigation tests and verify GREEN**

Run the Step 2 command. Expected: all tests pass.

### Task 7: Implement offline contracts and verify the full Domain

**Files:**
- Create: files under `Domain/Offline` listed above
- Test: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Offline/OfflineContractTests.cs`

**Interfaces:**
- Consumes: `Code`, cloned JSON payload pattern, and time/value validation.
- Produces: immutable area-pack manifests/resources and sync command/results.

- [ ] **Step 1: Add failing offline-contract tests**

Test required IDs and versions, non-empty resource lists, nonnegative byte sizes, SHA-256 hash shape, resource validity ranges, minimum app version preservation, distinct delta base/current versions, payload survival after JSON document disposal, success/error sync result invariants, and validation-error collection immutability.

- [ ] **Step 2: Run offline tests and verify RED**

Run: `dotnet test api/tests/AB.SmokPrzewodnik.Domain.UnitTests --no-restore -m:1 --filter FullyQualifiedName~OfflineContractTests`

- [ ] **Step 3: Implement offline records**

Implement immutable `AreaPackManifest`, `AreaPackResource`, `SyncCommand`, cloned `SyncPayload`, `SyncResult`, and closed `SyncStatus`. Use `Uri` for resource location, `long` byte sizes, lowercase hexadecimal SHA-256 strings, semantic-version strings, `long` server versions, and `IReadOnlyList<string>` validation errors.

- [ ] **Step 4: Run offline tests and verify GREEN**

Run the Step 2 command. Expected: all tests pass.

- [ ] **Step 5: Format, build, and run the full solution**

Run:

```bash
dotnet format api/AB.SmokPrzewodnik.sln --verify-no-changes --no-restore
dotnet build api/AB.SmokPrzewodnik.sln --no-restore -m:1
dotnet test api/AB.SmokPrzewodnik.sln --no-build --no-restore -m:1
```

Expected: formatting check succeeds, build has zero warnings/errors, and every test passes.

## Self-Review

- Spec coverage: sections 3.1-3.4, 3.6, and 3.7 are covered. Contributions/trust and rewards behavior from sections 3.5 and 3.8 are intentionally excluded; only their shared enums or evidence references required by included sections remain.
- Type consistency: every task consumes the exact shared types introduced by Tasks 1-3; route/offline types do not depend on Application or Infrastructure.
- Scope: projections and wire-like immutable models are not promoted to aggregates. Persistence mapping and endpoints remain outside this plan.
- Review focus: each listed failure mode has an explicit owning test in Tasks 2, 3, 5, 6, or 7.
- Proportion: signatures and invariants are specified; implementation bodies are left to the executor.
