# Improvement Plan — eQuantic.Core.Data.EntityFramework

> **📜 Historical — completed.** This is the original pre-migration analysis. Its recommendations have been
> delivered: the security/correctness/CI phases in PR #1, and the **v5 contract migration** (per-major
> `8.x`/`10.x` + multi-framework `4.x` packages, targets trimmed to net8/net10) shipped on top of it. It is
> kept for the record and describes the **old** state (net6–net10, v4 contracts) — it does **not** reflect
> the current codebase; see the [README](../README.md) and [walkthrough](../Repository.md) instead.

> Deep analysis performed on 2026-07-16 of this repository (v4.4.2 / published 6.x–10.x lines) and of the
> contracts repository [`eQuantic/core-data`](https://github.com/eQuantic/core-data) (v4.3.2).
> Every finding cites `file:line` and was verified against the source code, not inferred.

## Executive summary

The package works and has been published on nuget.org for years (106 versions of the main package, 57 of
the contract), but it accumulated debt across five layers. In order of severity:

1. **Security (critical):** `SqlExecutor` builds SQL by interpolating values without escaping — a real SQL
   injection in `ExecuteFunction`/`ExecuteProcedure` across the SqlServer, PostgreSql and MySql providers.
   In addition, CI publishes to nuget.org on every push to any branch.
2. **Correctness (critical/high):** double-dispose of the `UnitOfWork`, the MongoDb provider operating
   against the wrong database (silently writing/deleting), `EXEC` (T-SQL) copied to PostgreSQL/MySQL,
   parameters lost in `FromSqlRaw`, `configuration` discarded in `All`/`Any`, a DI registration that throws
   at runtime for MongoDb, and bulk updates that silently write the wrong values.
3. **Packaging/versioning (critical for consumers):** the same `PackageId` is published as parallel version
   lines (4.x multi-target + 6.x/7.x/8.x/9.x/10.x single-target). NuGet treats it all as a single timeline:
   "latest" = 10.0.2 (net10-only), breaking restore for anyone on net6–net9 and making the 4.x line (the
   most complete) look abandoned. There are 21 hand-maintained csproj files, and the scheme already produced
   real dependency-graph bugs.
4. **Structural duplication:** ~2,400 lines are identical copies between providers — PostgreSql and MySql
   are 100% renamed copies of SqlServer. That is the root cause of the `EXEC`→`CALL` bug (copied without
   adapting the dialect).
5. **Contracts (`eQuantic.Core.Data`):** combinatorial interface explosion
   (`IAsyncReadRepository<TConfig,TEntity,TKey>` has **100 members**; `SumAsync` alone is 30 overloads),
   NRT disabled, and pagination returning `IEnumerable<T>` with no total count.

The plan below is organized into **5 phases**. Phases 0–2 are **non contract-breaking** and can ship on the
current line. Phases 3–4 define the **v5.0.0 of the contracts** (deliberately breaking) and the versioning
strategy on nuget.org.

## Implementation status (Phase 1 — done)

Fixes already applied on this branch, **with tests** (28 tests passing; all packages build —
base/Relational/SqlServer/PostgreSql/MySql on net10, MongoDb on net8):

| Finding | Fix | Commit |
|--------|----------|--------|
| **S1** SQL injection + **P4** + **P3** | Parameterized `SqlExecutor` (placeholders instead of interpolating values) across the 3 SQL providers; `EXEC`→`CALL` on PG/MySql; `.ToArray()` in `ExecuteQuery`. Tests prove the malicious value never reaches the SQL. | `fix(sql)` |
| **C3** DI crash | `ISqlUnitOfWork` registered only when the implementation provides it; removed the duplicate `IQueryableUnitOfWork` registration. | `fix(core)` |
| **C1** double-dispose | Unified `_disposed` flag (`protected`); the UoW is disposed exactly once. | `fix(core)` |
| **A1** configuration discarded | Sync `All`/`Any` forward `configuration`. | `fix(read)` |
| **A2** default key | `Get`/`GetAsync` validate `id is null` instead of `default(TKey)`. | `fix(read)` |
| **P1/P6** MongoDb | Uses the database configured on the `DbContext` (not the collection name); throws instead of returning a silent `0`. | `fix(mongodb)` |
| **A4/M7** key | Key value parameterized (closure) instead of a literal; PK metadata cached; `EF.Property` for shadow keys; throws on a partial composite key. | `fix(core)` |
| **M4** DI | `AddRepository` honours the configured lifetime, uses `TryAdd`, and tolerates `ReflectionTypeLoadException`. | `fix(di)` |
| **M2** async | `ConfigureAwait(false)` across 91 library awaits (base + 4 providers). | `perf(async)` |
| **P2** MongoDb | `UpdateDefinitionBuilder` rejects (throws on) update expressions that reference the entity instead of silently writing a constant. New MongoDb test project. | `fix(mongodb)` |
| **C2** ownership ⚠️ | The repository no longer disposes the injected `UnitOfWork` (the creator — DI container or caller — owns the lifetime). **Behavioural change.** | `fix(core)` |
| **A5** pagination | `GetPaged`/`GetPagedAsync` order by the primary key when no explicit ordering is given (deterministic pagination); the caller's ordering is preserved. | `fix(read)` |

The placebo test (`Assert.Pass()`) was replaced with real coverage: DI, disposal, SQL parameterization,
queries, the key expression, pagination and the MongoDb `UpdateDefinitionBuilder`, via EF InMemory.

⚠️ **C2 is the only behavioural change in this batch.** Anyone who relied on disposing the repository to
close a manually-created context must now dispose the `UnitOfWork`/`DbContext` directly (the DI container
already does this). Documented in the commit.

**Investigated and corrected during diagnosis:** plan item **M8** (remove `Where(_ => true)` in
`GetAllAsync`) was **wrong** — that `Where` is load-bearing (`SetBase` does not implement
`IAsyncEnumerable`; the `Where` turns it into a real EF `IQueryable`). Documented in code + a regression
test; nothing removed.

Note on **P2**: the current fix **rejects** update expressions that reference the entity (avoids silent
corruption). Actually supporting them via `$inc`/pipeline updates is left for a future MongoDb iteration.

## Implementation status (Phase 0 — done, reduced scope)

| Finding | Fix |
|--------|----------|
| **PK7** dead MSBump | Removed `build/MSBump.props` (circular self-import), `build/MSBump.targets` (missing task) and `build/Directory.Build.targets` (outside any ancestor chain — never imported). Nothing referenced them; verified by grep before deleting. |
| **PK2** graph bug | `MySql.Net10.csproj` referenced the **Net9** base instead of **Net10** — fixed. |
| **Q1** accidental publishing | CI split into **`ci.yml`** (build + test on every push/PR, publishing nothing) and **`release.yml`** (only on a `vX.Y.Z` tag, publishing behind a `nuget-release` GitHub Environment). |
| **Q2** no tests in CI | `ci.yml` runs `dotnet test` on the 3 test projects (previously none ran). |
| **Q3** dated pipeline | Actions updated (`checkout@v4`, `setup-dotnet@v4`), `ubuntu-latest` instead of `windows-latest`, NuGet cache, `-p:ContinuousIntegrationBuild=true`. |
| **Q4** unpinned SDK | `global.json` at the root pinning `10.0.100` with `rollForward: latestFeature`. |

**Design decision (matrix build, not `dotnet build sln`):** I tried running
`dotnet build eQuantic.Core.Data.EntityFramework.sln` locally to simplify the 23 build steps — and
reproduced exactly the **PK4** finding: because several `.csproj` files of the same package
(`eQuantic.Core.Data.EntityFramework.csproj`, `.Net6.csproj`, `.Net7.csproj`, …) share the same folder
without their own `BaseIntermediateOutputPath`, the parallel solution build corrupted each other's
`project.assets.json` and hit a file-in-use `IOException`. The two new workflows keep the builds
**individual** (as the old workflow already did), but each one runs as a **matrix job** — i.e. an isolated
runner/checkout — which eliminates the shared `obj`/`bin` without having to settle the versioning decision
(Part IV) first.

⚠️ **Two things only a maintainer with GitHub access can finish:**
1. **Nothing publishes automatically until a tag exists.** Before, any push to any branch attempted to
   publish (mitigated only by `--skip-duplicate`). Now you need `git tag vX.Y.Z && git push origin vX.Y.Z`
   to trigger `release.yml`. This is intentional (finding Q1) but changes the workflow.
2. **The `environment: nuget-release` referenced by `release.yml` has no protection until configured.**
   GitHub auto-creates the Environment on first use with no required reviewers or branch/tag restriction —
   this session's PR tool has no permission to configure it. In *Settings → Environments → nuget-release*,
   add at least one required reviewer for the gate to actually work.

**Deliberately out of scope for Phase 0** (not done, because it depends on the still-open versioning
decision — Part IV): adopting MinVer (would change how each of the 23 csproj `<Version>` values is
determined) and any change to the published version numbers. Doing that now, before choosing between
consolidating to a single line (option A) and keeping per-.NET `PackageId`s (option B), would risk rework.

## Implementation status (Phase 2 — done)

Extracted the shared relational implementation into a **single package**,
`eQuantic.Core.Data.EntityFramework.Relational`, referenced by the 3 SQL providers.

| Finding | Fix |
|--------|----------|
| **Structural duplication** (§4) | `RelationalSqlExecutor`, `RelationalUnitOfWork`/`RelationalUnitOfWork<TDbContext>`, `RelationalSet` and the internal `ExpressionConverter`/`SqlConfigurationExtensions` now live once in the shared package. Each provider keeps thin `Set` and `UnitOfWork<TDbContext>` subclasses plus `DefaultUnitOfWork`, so the consumer-facing types stay in their namespaces. **~2,200 fewer lines of duplicated source.** |
| **P3 root cause** | The only genuine dialect difference — stored procedures use `EXEC` on SQL Server and the ANSI `CALL` elsewhere — is a single `BuildProcedureSql` virtual, overridden only by SQL Server. The copy-paste that let `EXEC`/`CALL` diverge is gone. |
| **PK3 (partial)** | MySql's per-framework variants are realigned to reference the multi-target base project (matching SqlServer/PostgreSql) so the shared multi-target project does not pull a second copy of the base assembly. |

**Why a new package rather than the base package or shared source** (the plan originally proposed "base
package with dialect hooks", which the deeper analysis showed to be wrong):
- The base package must **not** depend on `Microsoft.EntityFrameworkCore.Relational` — the MongoDb provider
  references the base and is not relational (it depends only on `Microsoft.EntityFrameworkCore` core +
  `MongoDB.Driver`). Putting relational code in the base would add `Relational` to every MongoDb consumer.
- **Linked source** (`<Compile Include>`) does not work for public types shared across providers: the type
  would be compiled into every provider assembly and collide if a consumer references two providers at once
  (a scenario the library supports).
- Changing the types' **namespace** would break the public API — that belongs to the v5 work.

A separate shared assembly is therefore the only clean option. It is multi-target only (net6–net10, one
version line) — no per-framework variants needed, because SqlServer/PostgreSql already reference the base
multi-target project everywhere, so the shared multi-target project composes with them with a single base
assembly (no duplicate).

Note: the implementation-only public types `SqlExecutor`, the non-generic `UnitOfWork` and
`SqlConfigurationExtensions` move to the `Relational` namespace, and `GetEntityByIdSpecification` now takes
`RelationalUnitOfWork`. These are implementation types (not the consumer-facing `DefaultUnitOfWork` /
`UnitOfWork<TDbContext>` / `Set<TEntity>`), but referencing them by name is a minor source break.

**What a maintainer still owns:** the new `eQuantic.Core.Data.EntityFramework.Relational` package is a new
published package the SQL providers now depend on. Its version (`1.0.0`) and how it fits the versioning
scheme is part of the Part IV decision.

**Next steps** (larger, architectural/breaking phases — awaiting a chosen approach):
- **Phase 3/4** — v5.0.0 of the contracts (`eQuantic.Core.Data`) and consolidation of the version lines on
  nuget.org (see Parts III and IV) — this includes the versioning decision that also blocks MinVer.

---

## Part I — Diagnosis

### 1. Security

| # | Sev. | Problem | Location |
|---|------|----------|-------|
| S1 | 🔴 Critical | **SQL injection**: `GetQueryParameters` interpolates values with `string.Format(" '{0}'", value)` without escaping single quotes; `string`/`Guid`/`DateTime` go raw into the SQL text, which reaches `FromSqlRaw`/`ExecuteSqlRaw` **with no `DbParameter`**. The function/procedure `name` is interpolated too. Identical file across the 3 SQL providers. | `SqlServer/Repository/SqlExecutor.cs:367,375-407` (same in PostgreSql and MySql) |
| S2 | 🟡 Medium | NuGet key exposed to any push: the workflow publishes with `secrets.nuget_key` on a push to **any branch**, with no protected environment and no tag/release gate. | `.github/workflows/dotnetcore.yml:3,66-67` |

**S1 fix:** emit placeholders (`@p0`/`$1`/`?`) and pass real `DbParameter`s — the infrastructure already
exists in the same file (`SetCommand`, `SqlExecutor.cs:419-445`) and is simply ignored by those methods.

### 2. Correctness bugs — Core (`eQuantic.Core.Data.EntityFramework`)

| # | Sev. | Problem | Location |
|---|------|----------|-------|
| C1 | 🔴 | **UnitOfWork double-dispose**: `AsyncQueryableRepository.Dispose(bool)` calls `base.Dispose()` (which already disposes the UoW) and disposes the UoW again — caused by a shadowed `_disposed` field on the derived class. | `Repository/AsyncQueryableRepository.cs:756-773` + `Repository/QueryableRepository.cs:363-378` |
| C2 | 🔴 | **Inverted UoW ownership**: repositories dispose the **injected** UnitOfWork; with `AddGenericRepositories` the container disposes it too → dead DbContext for the other repositories in the scope. | `Repository/QueryableRepository.cs:374`; `Read/QueryableReadRepository.cs:22`; `Write/WriteRepository.cs:15` |
| C3 | 🔴 | **DI registration throws at runtime**: `ISqlUnitOfWork` is registered unconditionally even when the implementation does not implement it (MongoDb) → `InvalidCastException` on resolve. Line 75 also duplicates the `IQueryableUnitOfWork` registration (dead code). | `Repository/Extensions/ServiceCollectionExtensions.cs:74-75` |
| A1 | 🟠 | **Sync `All`/`Any` discard `configuration`**: `return this.All(specification.SatisfiedBy());` ignores includes/no-tracking/sorting. The async variants do it right — proof it is a bug, not design. | `Read/QueryableReadRepository.cs:208,233` |
| A2 | 🟠 | **`Get(id)` rejects valid default keys with the wrong exception**: `if (Equals(id, default(TKey))) throw new ArgumentNullException` — `Get(0)`/`Guid.Empty` throw on a non-null argument. | `Read/QueryableReadRepository.cs:254-257`; `Read/AsyncQueryableReadRepository.cs:357-360` |
| A3 | 🟠 | **Sync deferred vs async materialized**: sync `GetAll`/`GetFiltered`/`GetPaged` return a live `IQueryable` disguised as `IEnumerable` (double enumeration = 2 queries; late `ObjectDisposedException`), while the async ones do `ToListAsync`. Same method, divergent semantics. | `Read/QueryableReadRepository.cs:47,271,299,396` vs `Read/AsyncQueryableReadRepository.cs:44,336,732` |
| A4 | 🟠 | **Key as `Expression.Constant`**: `GetFindByKeyExpression` embeds the key value in the tree → EF does not parameterize; each id creates a new query-cache entry and SQL with a literal (plan-cache pollution). | `Repository/Extensions/DbContextExtensions.cs:25,43` |
| A5 | 🟠 | **Pagination without `OrderBy`**: `Skip/Take` with no guaranteed ordering → non-deterministic pages + EF's `RowLimitingOperationWithoutOrderBy` warning. | `Read/QueryableReadRepository.cs:395`; `Read/AsyncQueryableReadRepository.cs:729` |
| M-core | 🟡 | Several: NRT disabled across the package; **zero `ConfigureAwait(false)`** in the whole library; `SumAsync` (~24 overloads) with no `CancellationToken` and no null validation; `AddRepository` ignores the configured lifetime; uncached reflection on the hot `Get(id, config)` path; `GetAllAsync` injects a redundant `Where(_ => true)`; inconsistent tracking between `Get(id)` (uses `Find`) and `Get(id, config)` (uses a query). | see the per-file detailed report |

### 3. Correctness bugs — Providers

| # | Sev. | Problem | Location |
|---|------|----------|-------|
| P1 | 🔴 | **MongoDb operates against the wrong database**: `GetDatabase(_collectionName)` uses the **collection** name as the **database** name → `DeleteMany`/`UpdateMany` run against a non-existent database and return `0` **silently**. | `MongoDb/Repository/Set.cs:129-131` |
| P2 | 🔴 | **MongoDb `UpdateDefinitionBuilder` writes a constant**: `x => new E { Count = x.Count + 1 }` is compiled and invoked against `Activator.CreateInstance(...)` (a default instance) → writes `0+1=1` on every document instead of incrementing. Silent corruption. | `MongoDb/UpdateDefinitionBuilder.cs:38-43` |
| P3 | 🟠 | **`EXEC` (T-SQL) copied to PG/MySQL**: `GetQueryProcedure` returns `$"EXEC {name}..."`; PostgreSQL/MySQL require `CALL` → `ExecuteProcedure` fails at runtime. Direct evidence of the copy-paste. | `PostgreSql/Repository/SqlExecutor.cs:367`; `MySql/…:367` |
| P4 | 🟠 | **`FromSqlRaw` receives an `IEnumerable` as 1 parameter**: `FromSqlRaw(sql, configuration.Parameters.Select(p => p.Value))` — the `Select` becomes a single element of the `params object[]`. Missing `.ToArray()`. | `SqlExecutor.cs:219` (3 SQL providers) |
| P5 | 🟠 | **Fragile `ExpressionConverter`**: `RewriteBinding` does not rebind the parameter → updates referencing the entity throw at runtime; `GetMethods().…Single(...)` breaks if EF adds a `SetProperty` overload; no cache. | `SqlServer/ExpressionConverter.cs:50-68,137-155` (3 SQL providers) |
| P6 | 🟡 | **MongoDb: silent returns**: without an `IMongoClient` in DI, `GetCollection()` returns null and the bulk ops return `0` without throwing. `IsSimpleType` does not cover `Guid`/`DateTimeOffset`/collections → discarded updates or `TargetParameterCountException`. | `MongoDb/Repository/Set.cs:29-65`; `UpdateDefinitionBuilder.cs:45-73` |
| P7 | 🟡 | **`SqlExecutor.Dispose` destroys the injected `DbContext`** (double-dispose in the DI scope). `IsMigrating` is a `static` shared across every context in the process. Unbounded `do/while` retry loop in `CommitAndRefreshChanges`. | `SqlExecutor.cs:483-497`; `UnitOfWork.cs:17,43-91` |
| P8 | 🟡 | **Orphaned `GetEntityByIdSpecification`**: exists only in SqlServer, nothing in it is SQL Server-specific (it delegates to the base `DbContextExtensions`), zero uses in the repo. Should be in the base or deprecated. | `SqlServer/Specifications/GetEntityByIdSpecification.cs` |

### 4. Structural duplication

| File | SqlServer | PostgreSql | MySql | MongoDb |
|---|---|---|---|---|
| `SqlExecutor.cs` | 498 lines | **identical** | **identical** | — |
| `UnitOfWork.cs` | 266 | **identical** | **identical** | ~180 same |
| `ExpressionConverter.cs` | 175 | **identical** | **identical** | — |
| `Set.cs` | 287 | 268 (= SqlServer minus the legacy blocks) | **identical to PG** | ~50 same |
| `SqlConfigurationExtensions.cs` | 14 | **identical** | **identical** | — |

**~2,400–2,500 redundant lines.** The only genuine dialect differences in `SqlExecutor` are 2 lines
(`GetQueryFunction`/`GetQueryProcedure`). It is not merely cosmetic debt: the P3 bug (`EXEC`→`CALL`) exists
precisely because the file was copied without adapting the dialect.

### 5. Packaging and versioning

| # | Sev. | Problem | Location |
|---|------|----------|-------|
| PK1 | 🔴 | **Parallel version lines on the same `PackageId`**: 4.x multi-target + 6.x/7.x/8.x/9.x/10.x single-target. NuGet sees a single timeline → `dotnet add package` pulls 10.0.2 (net10-only) and breaks restore on net6–net9; Dependabot suggests an impossible upgrade; the major stops meaning "breaking" (it means TFM). | 21 csproj in `src/`; confirmed on nuget.org |
| PK2 | 🔴 | **Dependency-graph bug**: `MySql.Net10.csproj` references the **Net9** core → the MySql 10.0.2 package declares a dependency on core `>= 9.1.2` (net9-only). SqlServer/PG/MongoDb NetX reference the multi-target core 4.4.2 — the families are already crossed. | `MySql/…MySql.Net10.csproj:66-67` |
| PK3 | 🔴 | **Pomelo 9 over EF Core 10**: MySql net10.0 uses `Pomelo.EntityFrameworkCore.MySql 9.0.0` (built for EF 9) with `Microsoft.EntityFrameworkCore 10.0.3`. There is no stable Pomelo 10 — risk of binary incompatibility. | `MySql/…MySql.csproj` (net10 block) |
| PK4 | 🟠 | **Shared `obj/`/`bin/` directories**: several csproj files in the same folder without `BaseIntermediateOutputPath` → `project.assets.json` overwritten on each restore; parallel builds (`dotnet build -m` of the solution) are a declared race. | `src/*/` with multiple csproj |
| PK5 | 🟠 | **MongoDb main package misaligned**: `Version 8.1.2.0`, `net8.0` only — there is no MongoDb in the 4.x family or a multi-target one; a net9/net10 consumer gets the net8 build. | `MongoDb/…MongoDb.csproj:7,9` |
| PK6 | 🟠 | **Rolling `AssemblyVersion`** (changes on each patch) → in a diamond between providers compiled against different core lines, risk of `MissingMethodException`/`FileLoadException`. | all csproj `:27` |
| PK7 | 🟡 | **Dead and broken MSBump**: `build/MSBump.props` imports itself (circular), `MSBump.targets` calls `BumpVersion` with no `UsingTask`, `Directory.Build.targets` is in `build/` (not an ancestor of `src/`, never applied), and its own comment says it is obsolete since NuGet 4.6. None of it is imported today. When it worked, it produced non-deterministic per-build versions. | `build/*` |

### 6. Contracts (`eQuantic.Core.Data`)

| # | Sev. | Problem | Location |
|---|------|----------|-------|
| K1 | 🟠 | **Combinatorial explosion**: `IAsyncReadRepository<TConfig,TEntity,TKey>` = **100 members**; `IReadRepository<TConfig,…>` = 56; `ISqlUnitOfWork` ≈ 47 (impossible to implement by hand — defeats the purpose of a contract). `GetPagedAsync` = 18 overloads; `SumAsync` = 30. The most recent commit *added* 60 Sum overloads — the trend is worsening. | `Read/IAsyncReadRepository.cs`; `Sql/ISqlUnitOfWork.cs` |
| K2 | 🟠 | **`TUnitOfWork` as a type parameter** enables a single member (`UnitOfWork { get; }`) but contaminates the whole hierarchy (~24 interfaces for 1 concept) and creates a circular UoW↔repository coupling. Inconsistent variance sync vs async. | `Repository/IRepository.cs:26,55`; `IAsyncRepository.cs:39` |
| K3 | 🟠 | **Pagination without metadata**: `GetPaged*` returns raw `IEnumerable<T>`, with no total/page → the consumer does a separate `Count()` (2 non-atomic round-trips). A `PagedResult<T>` is missing. | `Read/IReadRepository.cs:267-314` |
| K4 | 🟡 | **EF leaking into the contract**: `ISqlUnitOfWork` declares `GetPendingMigrations`/`UpdateDatabase`/`Attach` — contradicting the "persistence ignorance" its own XML docs claim. `IdentityGenerator` and `MigrationAttribute` are implementation in a contracts package. | `Sql/ISqlUnitOfWork.cs:23-127`; `IdentityGenerator.cs`; `Migration/MigrationAttribute.cs` |
| K5 | 🟡 | **`CancellationToken` missing** on 30 `SumAsync`, `AddAsync`/`MergeAsync`/`ModifyAsync`/`RemoveAsync`, `LoadCollectionAsync`. NRT disabled (`Get*` returns `Task<TEntity>` with no null annotation). No `IAsyncEnumerable`/streaming. | `Read/IAsyncReadRepository.cs`; `Write/IAsyncWriteRepository.cs` |
| K6 | 🟡 | **`new()` constraint everywhere** (hostile to DDD) and `IEntity`/`IEntity<TKey>` do not tie the repository's `TKey` to the entity's real key (`IRepository<Customer, Guid>` compiles even if the key is `int`). Should be `where TEntity : IEntity<TKey>`. | `Repository/IRepository.cs:40` |
| K7 | 🟢 | **Latent bug**: `IdentityGenerator.GuidRegex` contains 2 invisible zero-width characters (U+200C and U+200B) inside the `[0-9…a-fA-F]{12}` class — a copy-paste artifact (confirmed by a byte dump). Dead `eQuantic.Core` dependency (no file imports it). Typo `mintute` in `MigrationAttribute`. | `IdentityGenerator.cs:7`; `core-data.csproj` |

### 7. Process, CI and tests

| # | Sev. | Problem | Location |
|---|------|----------|-------|
| Q1 | 🔴 | **CI publishes on any push, without tests**: `on: [push]` → `dotnet nuget push` on every push to any branch, with the NuGet key. **No `dotnet test`** runs before publishing. | `.github/workflows/dotnetcore.yml:3,66-67` |
| Q2 | 🔴 | **Placebo tests**: `UnitTest1.cs` is an `Assert.Pass()`. There is no coverage of any bug above. | `tests/…Tests/UnitTest1.cs` |
| Q3 | 🟡 | 21 sequential `dotnet build` steps instead of the solution; outdated actions (`checkout@v3`, `setup-dotnet@v3`); unnecessary `windows-latest`; no `dotnet test`, cache, `global.json`, deterministic pack (`ContinuousIntegrationBuild`), symbols (snupkg) or provenance. | `dotnetcore.yml` |
| Q4 | 🟡 | No central `Directory.Build.props`/`Directory.Packages.props`: metadata and package versions repeated across 21 csproj (the real source of PK2/PK3). No consistent NRT/`GenerateDocumentationFile` (packages ship to NuGet with no IntelliSense). The README says "Version 4.4.0" and does not explain the version matrix; `Repository.md` uses `IContainer`/pre-DI service-locator. | repo root |

---

## Part II — Phased execution plan

### Phase 0 — Harden the pipeline (days, no production code) — **done**

Prerequisite for everything: stop publishing by accident and have a safety net.

1. **Split CI from Release.** `ci.yml` on `push`/`pull_request`: `restore` → `build` → **`dotnet test`** →
   `dotnet pack` as an artifact (no push). `release.yml` only on `push: tags: ['v*']`, with the
   `nuget_key` in a **protected GitHub Environment**.
2. Update the actions to v4; move to `ubuntu-latest`; add a NuGet cache and `global.json`.
3. **Delete `build/`** (dead MSBump). MinVer adoption is deferred until the versioning decision (Part IV).

### Phase 1 — Non contract-breaking fixes (current line, patch/minor) — **done**

Ship now, without touching `eQuantic.Core.Data`. Each covered by a test. See the status table above.

### Phase 2 — Structural de-duplication (minor, internal refactor) — **done**

Extracted the shared relational implementation into a new
`eQuantic.Core.Data.EntityFramework.Relational` package (see the status section above for why a new package
rather than the base package). Each provider becomes thin subclasses + a dialect override. Removes ~2,200
lines and kills the P3 bug class at the root. **Consumer-facing types keep their namespaces**; only a few
implementation-only types move.

### Phase 3 — Contracts redesign v5.0.0 (deliberately breaking — see Part III)

Consolidate the surface via *options objects*, introduce `PagedResult<T>`, a uniform `CancellationToken`,
annotated NRT, `where TEntity : IEntity<TKey>`, remove `TUnitOfWork` from the hierarchy and the EF-specific
content from the contracts. Reimplement on the EF package (which becomes ~10× smaller).

### Phase 4 — Migration on nuget.org (see Part IV)

Consolidate the version lines, deprecate the old ones without breaking anyone who already depends on them,
and publish the compatibility matrix.

---

## Part III — Changes requiring a contract break in `eQuantic.Core.Data`

General rule for the contracts package: **adding** a member to an interface already breaks every external
implementer (mocks, fakes, decorators, plus the EF impl itself), and **changing a signature/removing**
breaks callers too. Almost every fundamental improvement is therefore a v5.0.0. What requires breaking:

1. **`CancellationToken` on the members that lack it** (30 `SumAsync`, `AddAsync`/`MergeAsync`/`ModifyAsync`/`RemoveAsync`, `LoadCollectionAsync`). Partial non-breaking route: *default interface methods* (DIM) delegating to the existing overload — viable because all TFMs are ≥ net6.0, but it crystallizes the overload explosion.
2. **Consolidate overloads into options objects** (`QueryOptions<T>` absorbing filter/specification/config; `PageRequest`): remove the 18 `GetPagedAsync`, the 60 `Sum*` etc. Reduces `IAsyncReadRepository` from 100 to ~12 members. This is the heart of v5.
3. **`PagedResult<T>` instead of `IEnumerable<T>`** for pagination: a return-type change — hard breaking (not even DIM saves it; would require a new method with a different name, e.g. `QueryPagedAsync`).
4. **Remove `TUnitOfWork` from the hierarchy** (collapse `IRepository<TUoW,TEntity,TKey>` into `IRepository<TEntity,TKey>` + `IUnitOfWork UnitOfWork { get; }`): removes ~10 public interfaces; the EF package references those arities in `GetRepository<TUnitOfWork,…>`.
5. **`where TEntity : IEntity<TKey>` constraint** and/or removing `new()`: changes generic constraints — source+binary breaking.
6. **Split sync/async in `IUnitOfWork`** and **remove `ExecuteTransactionAsync` from `ISqlExecutor`** (an async method on the "sync" interface).
7. **Move `GetPendingMigrations`/`UpdateDatabase`/`MigrationAttribute`/`IdentityGenerator` to the EF package**: removes public types/members from the contract — a real break (the contract→EF direction rules out `[TypeForwardedTo]`).
8. **Annotated NRT** (`TEntity?` on `Get`/`GetFirst`/`GetSingle`): technically only produces new warnings — the cheapest break; requires both packages to be annotated together to stay consistent.

**Recommendation:** treat the next contract version as a **deliberately breaking v5.0.0** and reimplement
the EF package on top of it, rather than stacking DIMs over a 100-member surface. The EF package's
maintenance cost — today implementing ~156 members per provider × 4 providers — drops by an order of
magnitude.

---

## Part IV — Versioning strategy on nuget.org

The PK1 problem has two coherent exits. The recommended one is (A).

**(A) Consolidate into a single multi-target line per `PackageId` (recommended).**
One multi-target `.csproj` per package (`net8.0;net9.0;net10.0` — net6/net7 are EOL), a single version line,
resumed **above** the highest already published so the timeline becomes increasing and monotonic again
(e.g. **11.0.0**, or 5.0.0 if you accept the numeric "latest" dropping — which would confuse anyone already
on 10.x). Multi-target already delivers the right binary per TFM inside a single `.nupkg` — exactly what the
parallel-lines scheme tries to emulate by hand. This fixes PK1, PK2, PK4, PK5 and Q4 at once.

**(B) Keep per-.NET families, but with distinct `PackageId`s.**
E.g. `eQuantic.Core.Data.EntityFramework.Net8`. It is the only way NuGet treats the families as independent
lines (each id's "latest" is correct for its TFM). Cost: it fragments the consumer ecosystem and discovery
on nuget.org, and multiplies the packages to maintain. Only worth it if there is a strong reason to freeze
each TFM to its own API.

**Migration without breaking anyone already on the old versions** (applies to A and B):
- **Never** unpublish (`unlist` keeps restore working for pinned versions; `delete` breaks it). Use
  **deprecation** on nuget.org (`Legacy`/`Other`) on the old versions, pointing to the new one.
- Publish the **compatibility matrix** (TFM × package × version) in the README — today the README says
  "Version 4.4.0" and explains none of it.
- Align the core + 4 providers to release **always together, at the same version** (resolves the
  `AssemblyVersion` diamonds).
- Only then resume the consolidated numbering and point the release CI at tags.

Note: the new `eQuantic.Core.Data.EntityFramework.Relational` package introduced in Phase 2 must join
whichever scheme is chosen here.

---

## Appendix — Suggested order (what to do first)

1. **Phase 0** (pipeline) — unblocks everything safely. ✅ done
2. **S1, C1, C2, C3, P1, P2** — the 6 🔴 security/runtime findings, with tests. ✅ done
3. **Rest of Phase 1** (🟠 findings) on the current line. ✅ done
4. **Phase 2** (de-dup) — cheap, high return, no break. ✅ done
5. **Phases 3–4** — plan the contract v5.0.0 and the version consolidation as a separate milestone,
   communicated to consumers in advance.
