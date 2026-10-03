# .NET API Clean Architecture Scaffolding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the default API template with a buildable, tested .NET 8 Clean Architecture and DDD scaffold whose production projects live under `api/src`, test projects live under `api/tests`, and every project belongs to `api/AB.SmokPrzewodnik.sln`.

**Architecture:** Use four production assemblies with inward-only dependencies: `Api -> Application + Infrastructure`, `Infrastructure -> Application + Domain`, `Application -> Domain`, and `Domain -> no project`. Product modules from the design remain feature namespaces inside these assemblies until a demonstrated need justifies separate bounded-context assemblies. Protect the dependency graph with executable architecture tests and prove the API composition root with an in-process integration test.

**Tech Stack:** .NET SDK 8.0.425, ASP.NET Core 8.0.31, C# 12, xUnit 2.9.3, Microsoft.AspNetCore.Mvc.Testing 8.0.31, Swagger/OpenAPI, central NuGet package management.

**Spec:** `docs/superpowers/specs/2026-10-03-smok-przewodnik-design.md`

## Global Constraints

- Keep all production project source under `api/src` and all test projects under `api/tests`.
- Add every production and test project to `api/AB.SmokPrzewodnik.sln` under matching `src` and `tests` solution folders.
- Target `net8.0`, enable nullable reference types and implicit usings, and compile with C# 12.
- Treat compiler warnings as errors for repository-owned code.
- Domain must not reference Application, Infrastructure, ASP.NET Core, Supabase, persistence, or transport packages.
- Application may reference Domain and dependency-injection abstractions only; it must not reference Infrastructure or Api.
- Infrastructure may reference Application and Domain; it must not reference Api.
- Api is the composition root and may reference Application and Infrastructure.
- Preserve the .NET-to-Supabase and Expo-to-.NET boundary from the design; this scaffold does not implement Supabase access yet.
- Organize future product behavior by feature module within each layer: Identity and Profiles, Spatial Catalog, Routing, Navigation, Planning and Personalization, Contributions and Trust, City Data, Offline Sync, Rewards, and Media.
- Do not introduce CQRS, mediator, ORM, validation, mapping, or Supabase packages until a concrete feature needs them.
- Do not commit changes unless the user explicitly requests a commit.

## Review Focus

- Two distinct entity instances with an unassigned/default identifier must not compare equal; Task 2 pins this in `EntityTests`.
- Entities of different runtime types must not compare equal even when their identifiers match; Task 2 pins this in `EntityTests`.
- Domain events must not remain attached after dispatch preparation clears them; Task 2 pins this in `AggregateRootTests`.
- A failed application result must not accept `Error.None`, and a successful result must not carry an error; Task 3 pins both constructor invariants through factory behavior.
- The integration test host must start with the same composition root as production and expose a healthy endpoint without external infrastructure; Task 4 pins this with `WebApplicationFactory<Program>`.

---

## Planned File Structure

```text
api/
  AB.SmokPrzewodnik.sln
  Directory.Build.props
  Directory.Packages.props
  src/
    AB.SmokPrzewodnik.Domain/
      AB.SmokPrzewodnik.Domain.csproj
      AssemblyReference.cs
      Common/Entity.cs
      Common/AggregateRoot.cs
      Common/IDomainEvent.cs
    AB.SmokPrzewodnik.Application/
      AB.SmokPrzewodnik.Application.csproj
      AssemblyReference.cs
      DependencyInjection.cs
      Common/Errors/Error.cs
      Common/Results/Result.cs
    AB.SmokPrzewodnik.Infrastructure/
      AB.SmokPrzewodnik.Infrastructure.csproj
      AssemblyReference.cs
      DependencyInjection.cs
    AB.SmokPrzewodnik.Api/
      AB.SmokPrzewodnik.Api.csproj
      AB.SmokPrzewodnik.Api.http
      Program.cs
      Endpoints/HealthEndpoints.cs
      Properties/launchSettings.json
      appsettings.json
      appsettings.Development.json
  tests/
    AB.SmokPrzewodnik.Domain.UnitTests/
      AB.SmokPrzewodnik.Domain.UnitTests.csproj
      Common/EntityTests.cs
      Common/AggregateRootTests.cs
    AB.SmokPrzewodnik.Application.UnitTests/
      AB.SmokPrzewodnik.Application.UnitTests.csproj
      Common/Results/ResultTests.cs
    AB.SmokPrzewodnik.ArchitectureTests/
      AB.SmokPrzewodnik.ArchitectureTests.csproj
      DependencyRulesTests.cs
    AB.SmokPrzewodnik.Api.IntegrationTests/
      AB.SmokPrzewodnik.Api.IntegrationTests.csproj
      HealthEndpointTests.cs
```

### Task 1: Establish the solution and project dependency graph

**Files:**
- Create: `api/Directory.Build.props`
- Create: `api/Directory.Packages.props`
- Create: `api/src/AB.SmokPrzewodnik.Domain/AB.SmokPrzewodnik.Domain.csproj`
- Create: `api/src/AB.SmokPrzewodnik.Application/AB.SmokPrzewodnik.Application.csproj`
- Create: `api/src/AB.SmokPrzewodnik.Infrastructure/AB.SmokPrzewodnik.Infrastructure.csproj`
- Modify: `api/src/AB.SmokPrzewodnik.Api/AB.SmokPrzewodnik.Api.csproj`
- Create: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/AB.SmokPrzewodnik.Domain.UnitTests.csproj`
- Create: `api/tests/AB.SmokPrzewodnik.Application.UnitTests/AB.SmokPrzewodnik.Application.UnitTests.csproj`
- Create: `api/tests/AB.SmokPrzewodnik.ArchitectureTests/AB.SmokPrzewodnik.ArchitectureTests.csproj`
- Create: `api/tests/AB.SmokPrzewodnik.Api.IntegrationTests/AB.SmokPrzewodnik.Api.IntegrationTests.csproj`
- Modify: `api/AB.SmokPrzewodnik.sln`

**Interfaces:**
- Consumes: Existing `AB.SmokPrzewodnik.Api` ASP.NET Core project and solution.
- Produces: The assembly dependency graph described in the Architecture header; all later tasks compile within this graph.

This task is generated project and build configuration. Approval of this plan is the explicit TDD exception for project files; no behavioral production type is added here.

- [ ] **Step 1: Add repository-wide .NET build settings**

Create `Directory.Build.props` with `TargetFramework=net8.0`, `LangVersion=12.0`, `Nullable=enable`, `ImplicitUsings=enable`, and `TreatWarningsAsErrors=true`.

- [ ] **Step 2: Add central package versions**

Create `Directory.Packages.props` with `ManagePackageVersionsCentrally=true` and these versions:

```text
Microsoft.AspNetCore.OpenApi                 8.0.31
Microsoft.AspNetCore.Mvc.Testing             8.0.31
Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2
Swashbuckle.AspNetCore                       6.6.2
Microsoft.NET.Test.Sdk                       17.14.1
xunit                                        2.9.3
xunit.runner.visualstudio                    2.8.1
coverlet.collector                           6.0.4
```

- [ ] **Step 3: Create production and test project files**

Use class-library SDK projects for Domain, Application, and Infrastructure, and xUnit test SDK projects for all four test assemblies. Mark runner and coverage packages with `PrivateAssets=all`; mark the integration project with `Microsoft.AspNetCore.Mvc.Testing`.

- [ ] **Step 4: Add project references**

Configure exact references:

```text
Application -> Domain
Infrastructure -> Application, Domain
Api -> Application, Infrastructure
Domain.UnitTests -> Domain
Application.UnitTests -> Application, Domain
ArchitectureTests -> Domain, Application, Infrastructure, Api
Api.IntegrationTests -> Api
```

- [ ] **Step 5: Add all projects to the solution**

Use `dotnet sln AB.SmokPrzewodnik.sln add ... --solution-folder src` for production projects and `--solution-folder tests` for test projects. Remove generated `Class1.cs` and `UnitTest1.cs` files.

- [ ] **Step 6: Restore and list the solution**

Run: `dotnet restore AB.SmokPrzewodnik.sln && dotnet sln AB.SmokPrzewodnik.sln list`

Expected: restore succeeds and the solution lists four projects under `src` and four under `tests`.

### Task 2: Add DDD domain primitives with unit tests

**Files:**
- Create: `api/src/AB.SmokPrzewodnik.Domain/AssemblyReference.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/Common/IDomainEvent.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/Common/Entity.cs`
- Create: `api/src/AB.SmokPrzewodnik.Domain/Common/AggregateRoot.cs`
- Create: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Common/EntityTests.cs`
- Create: `api/tests/AB.SmokPrzewodnik.Domain.UnitTests/Common/AggregateRootTests.cs`

**Interfaces:**
- Consumes: Domain project from Task 1.
- Produces: `IDomainEvent`, `Entity<TId>`, `AggregateRoot<TId>`, and `AB.SmokPrzewodnik.Domain.AssemblyReference`.

- [ ] **Step 1: Write failing entity identity tests**

Add tests named:

```text
EntitiesWithSameAssignedIdAndType_AreEqual
EntitiesWithSameIdButDifferentTypes_AreNotEqual
DistinctEntitiesWithDefaultIds_AreNotEqual
EntityComparedWithItself_IsEqual
EqualEntities_HaveEqualHashCodes
```

Use two private test entity types derived from the wished-for `Entity<Guid>` API. Assert through `Assert.Equal`, `Assert.NotEqual`, and hash-code equality.

- [ ] **Step 2: Run entity tests and verify RED**

Run: `dotnet test tests/AB.SmokPrzewodnik.Domain.UnitTests --filter FullyQualifiedName~EntityTests`

Expected: compilation fails because `Entity<TId>` does not exist.

- [ ] **Step 3: Implement `Entity<TId>`**

Create `public abstract class Entity<TId> where TId : notnull` with a protected constructor, `public TId Id { get; }`, equality by exact runtime type and assigned identifier, reference equality, `==`/`!=`, and a matching hash code. Default identifiers compare equal only for the same object reference.

- [ ] **Step 4: Run entity tests and verify GREEN**

Run: `dotnet test tests/AB.SmokPrzewodnik.Domain.UnitTests --filter FullyQualifiedName~EntityTests`

Expected: all `EntityTests` pass.

- [ ] **Step 5: Write failing aggregate-domain-event tests**

Add tests named:

```text
RaiseDomainEvent_AddsEventInInsertionOrder
ClearDomainEvents_RemovesAllRaisedEvents
DomainEvents_CannotBeMutatedThroughExposedCollection
```

Use a private aggregate subclass that exposes the protected raise operation and a private `TestDomainEvent : IDomainEvent`.

- [ ] **Step 6: Run aggregate tests and verify RED**

Run: `dotnet test tests/AB.SmokPrzewodnik.Domain.UnitTests --filter FullyQualifiedName~AggregateRootTests`

Expected: compilation fails because `AggregateRoot<TId>` and `IDomainEvent` do not exist.

- [ ] **Step 7: Implement domain-event primitives**

Create marker interface `public interface IDomainEvent`. Create `public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull` with `IReadOnlyCollection<IDomainEvent> DomainEvents`, protected `RaiseDomainEvent(IDomainEvent)`, and public `ClearDomainEvents()`. Return a read-only view, not the mutable list.

- [ ] **Step 8: Add the assembly marker and run Domain tests**

Create `public static class AssemblyReference` with `public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;`.

Run: `dotnet test tests/AB.SmokPrzewodnik.Domain.UnitTests`

Expected: all Domain unit tests pass.

### Task 3: Add application result primitives and registration boundary

**Files:**
- Create: `api/src/AB.SmokPrzewodnik.Application/AssemblyReference.cs`
- Create: `api/src/AB.SmokPrzewodnik.Application/Common/Errors/Error.cs`
- Create: `api/src/AB.SmokPrzewodnik.Application/Common/Results/Result.cs`
- Create: `api/src/AB.SmokPrzewodnik.Application/DependencyInjection.cs`
- Create: `api/tests/AB.SmokPrzewodnik.Application.UnitTests/Common/Results/ResultTests.cs`

**Interfaces:**
- Consumes: Domain assembly marker pattern from Task 2 and `IServiceCollection`.
- Produces: `Error`, `Result`, `Application.DependencyInjection.AddApplication(IServiceCollection)`, and the Application assembly marker.

- [ ] **Step 1: Write failing result tests**

Add tests named:

```text
Success_IsSuccessfulAndHasNoError
Failure_IsFailedAndExposesProvidedError
Failure_WithNoneError_ThrowsArgumentException
```

Use the wished-for APIs `Result.Success()`, `Result.Failure(Error)`, `Result.IsSuccess`, `Result.IsFailure`, and `Result.Error`.

- [ ] **Step 2: Run result tests and verify RED**

Run: `dotnet test tests/AB.SmokPrzewodnik.Application.UnitTests --filter FullyQualifiedName~ResultTests`

Expected: compilation fails because `Result` and `Error` do not exist.

- [ ] **Step 3: Implement `Error` and `Result`**

Create `public sealed record Error(string Code, string Description)` with `public static readonly Error None`. Create a non-generic `Result` with private construction and the factories exercised by the tests. Enforce that success carries only `Error.None` and failure carries a non-None error.

- [ ] **Step 4: Run result tests and verify GREEN**

Run: `dotnet test tests/AB.SmokPrzewodnik.Application.UnitTests --filter FullyQualifiedName~ResultTests`

Expected: all `ResultTests` pass.

- [ ] **Step 5: Add Application assembly and registration markers**

Create the assembly marker matching Domain. Add `public static IServiceCollection AddApplication(this IServiceCollection services)` returning the same collection. This is the stable composition boundary; concrete feature registrations will be added here later.

- [ ] **Step 6: Run Application tests**

Run: `dotnet test tests/AB.SmokPrzewodnik.Application.UnitTests`

Expected: all Application unit tests pass.

### Task 4: Replace the template API with a tested composition root

**Files:**
- Create: `api/src/AB.SmokPrzewodnik.Infrastructure/AssemblyReference.cs`
- Create: `api/src/AB.SmokPrzewodnik.Infrastructure/DependencyInjection.cs`
- Modify: `api/src/AB.SmokPrzewodnik.Api/Program.cs`
- Create: `api/src/AB.SmokPrzewodnik.Api/Endpoints/HealthEndpoints.cs`
- Modify: `api/src/AB.SmokPrzewodnik.Api/AB.SmokPrzewodnik.Api.http`
- Create: `api/tests/AB.SmokPrzewodnik.Api.IntegrationTests/HealthEndpointTests.cs`

**Interfaces:**
- Consumes: `AddApplication(IServiceCollection)` from Task 3 and ASP.NET Core health checks.
- Produces: `Infrastructure.DependencyInjection.AddInfrastructure(IServiceCollection)`, `HealthEndpoints.MapHealthEndpoints(IEndpointRouteBuilder)`, a public testable `Program`, and `GET /health`.

- [ ] **Step 1: Write the failing health endpoint integration test**

Create `HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>`. Add `GetHealth_ReturnsHealthyResponse` that calls `/health`, asserts HTTP 200, and asserts the response body contains `Healthy`.

- [ ] **Step 2: Run the integration test and verify RED**

Run: `dotnet test tests/AB.SmokPrzewodnik.Api.IntegrationTests --filter FullyQualifiedName~HealthEndpointTests`

Expected: FAIL with HTTP 404 because the current template has no `/health` endpoint.

- [ ] **Step 3: Add the Infrastructure composition boundary**

Create the Infrastructure assembly marker and `public static IServiceCollection AddInfrastructure(this IServiceCollection services)` returning the same collection. No persistence provider is registered in this scaffold.

- [ ] **Step 4: Implement the API composition root**

Replace the weather template with registrations for Problem Details, health checks, OpenAPI/Swagger, `AddApplication()`, and `AddInfrastructure()`. Map `/health` through `HealthEndpoints.MapHealthEndpoints`. Retain HTTPS redirection and development-only Swagger. End `Program.cs` with `public partial class Program;` for `WebApplicationFactory`.

- [ ] **Step 5: Update the HTTP scratch file**

Replace the weather request with `GET {{AB.SmokPrzewodnik.Api_HostAddress}}/health`.

- [ ] **Step 6: Run the integration test and verify GREEN**

Run: `dotnet test tests/AB.SmokPrzewodnik.Api.IntegrationTests --filter FullyQualifiedName~HealthEndpointTests`

Expected: `GetHealth_ReturnsHealthyResponse` passes.

### Task 5: Add executable Clean Architecture dependency guards

**Files:**
- Create: `api/tests/AB.SmokPrzewodnik.ArchitectureTests/DependencyRulesTests.cs`

**Interfaces:**
- Consumes: Assembly markers from Domain, Application, and Infrastructure plus `typeof(Program).Assembly` for Api.
- Produces: Regression tests that reject forbidden inward/outward project references.

- [ ] **Step 1: Add dependency-rule tests**

Create tests named:

```text
Domain_DoesNotReferenceOuterProjects
Application_DoesNotReferenceInfrastructureOrApi
Infrastructure_DoesNotReferenceApi
Api_ReferencesApplicationAndInfrastructure
```

Do not require Domain to appear in Application's emitted assembly references:
the compiler may omit a declared project reference until a Domain type is used.

Inspect `Assembly.GetReferencedAssemblies()` by simple assembly name. Keep the helper private to the test file and assert explicitly on every required and forbidden `AB.SmokPrzewodnik.*` reference.

- [ ] **Step 2: Run architecture tests**

Run: `dotnet test tests/AB.SmokPrzewodnik.ArchitectureTests`

Expected: all four dependency-rule tests pass. A failure means the project graph from Task 1 or a composition-root usage is wrong; correct the references rather than weakening the assertions.

### Task 6: Verify the complete scaffold

**Files:**
- Verify only; no planned source changes.

**Interfaces:**
- Consumes: All projects and tests from Tasks 1-5.
- Produces: Evidence that the scaffold restores, builds, and tests as one solution.

- [ ] **Step 1: Format-check without modifying files**

Run: `dotnet format AB.SmokPrzewodnik.sln --verify-no-changes --no-restore`

Expected: exit code 0 with no formatting changes required.

- [ ] **Step 2: Build the full solution**

Run: `dotnet build AB.SmokPrzewodnik.sln --no-restore`

Expected: build succeeds with 0 warnings and 0 errors.

- [ ] **Step 3: Run the complete test suite**

Run: `dotnet test AB.SmokPrzewodnik.sln --no-build`

Expected: all Domain, Application, Architecture, and API integration tests pass with 0 failures.

- [ ] **Step 4: Inspect the final project graph and working tree**

Run: `dotnet sln AB.SmokPrzewodnik.sln list` from `api`, then `git status --short` from the repository root.

Expected: eight projects are listed; only the intended API scaffold, tests, plan, and already-reviewed design-document changes appear. Do not stage or commit them.
