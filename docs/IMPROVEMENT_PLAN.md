# Plano de Melhoria — eQuantic.Core.Data.EntityFramework

> Análise profunda realizada em 2026-07-16 sobre este repositório (v4.4.2 / linhas 6.x–10.x publicadas)
> e sobre o repositório de contratos [`eQuantic/core-data`](https://github.com/eQuantic/core-data) (v4.3.2).
> Todos os achados citam `arquivo:linha` e foram verificados no código-fonte, não inferidos.

## Sumário executivo

O pacote funciona e está publicado no nuget.org há anos (106 versões do pacote principal, 57 do
contrato), mas acumulou dívida em cinco camadas. Em ordem de gravidade:

1. **Segurança (crítico):** o `SqlExecutor` monta SQL interpolando valores sem escape — injeção de SQL
   real em `ExecuteFunction`/`ExecuteProcedure` nos providers SqlServer, PostgreSql e MySql. Além disso,
   o CI publica no nuget.org a cada push em qualquer branch.
2. **Correção (crítico/alto):** disposal duplo do `UnitOfWork`, provider MongoDb operando no banco
   errado (grava/apaga em silêncio), `EXEC` (T-SQL) copiado para PostgreSQL/MySQL, parâmetros perdidos no
   `FromSqlRaw`, `configuration` descartado em `All`/`Any`, registro de DI que quebra em runtime para o
   MongoDb, e updates em massa que gravam valores errados sem erro.
3. **Packaging/versionamento (crítico para consumidores):** o mesmo `PackageId` é publicado em linhas de
   versão paralelas (4.x multi-target + 6.x/7.x/8.x/9.x/10.x single-target). O NuGet trata tudo como uma
   linha do tempo única: "latest" = 10.0.2 (net10-only), quebrando o restore de quem está em net6–net9 e
   fazendo a linha 4.x (a mais completa) parecer abandonada. São 21 csproj mantidos à mão, e o esquema já
   produziu bugs reais de grafo de dependência.
4. **Duplicação estrutural:** ~2.400 linhas são cópias idênticas entre providers — PostgreSql e MySql são
   100% cópias renomeadas do SqlServer. É a causa-raiz do bug `EXEC`→`CALL` (copiado sem adaptar).
5. **Contratos (`eQuantic.Core.Data`):** explosão combinatória de interfaces
   (`IAsyncReadRepository<TConfig,TEntity,TKey>` tem **100 membros**; só `SumAsync` são 30 overloads),
   NRT desabilitado, e a paginação retorna `IEnumerable<T>` sem total de registros.

O plano abaixo está em **5 fases**. As Fases 0–2 **não quebram contrato** e podem sair na linha atual.
As Fases 3–4 definem a **v5.0.0 dos contratos** (breaking deliberado) e a estratégia de versionamento
no nuget.org.

## Status de implementação (Fase 1 iniciada)

Correções já aplicadas nesta branch, com testes (17 testes passando, todos os 5 pacotes compilando em net10):

| Achado | Correção | Commit |
|--------|----------|--------|
| **S1** injeção de SQL + **P4** + **P3** | `SqlExecutor` parametrizado (placeholders em vez de interpolar valores) nos 3 providers SQL; `EXEC`→`CALL` em PG/MySql; `.ToArray()` no `ExecuteQuery`. Testes provam que o valor malicioso não chega ao SQL. | `🔒 fix(sql)` |
| **C3** crash de DI | `ISqlUnitOfWork` só registrado quando a impl o implementa; removido registro duplicado de `IQueryableUnitOfWork`. | `🐞 fix(core)` |
| **C1** double-dispose | `_disposed` unificado (`protected`); UoW disposto exatamente uma vez. | `🐞 fix(core)` |
| **A1** config descartada | `All`/`Any` sync repassam `configuration`. | `🐞 fix(read)` |
| **A2** chave default | `Get`/`GetAsync` validam `id is null` em vez de `default(TKey)`. | `🐞 fix(read)` |

Substituído o teste placebo (`Assert.Pass()`) por cobertura real (DI, disposal, parametrização de SQL,
queries via EF InMemory). **Pendente na Fase 1** (próximos passos): C2 (ownership do UoW injetado —
comportamental, precisa de decisão), P1/P2 (MongoDb banco errado / update de constante), M-core
(`ConfigureAwait(false)`, cache de expressão de chave, `AddRepository` respeitando lifetime), A4/A5.

---

## Parte I — Diagnóstico

### 1. Segurança

| # | Sev. | Problema | Local |
|---|------|----------|-------|
| S1 | 🔴 Crítico | **Injeção de SQL**: `GetQueryParameters` interpola valores com `string.Format(" '{0}'", value)` sem escapar aspas simples; `string`/`Guid`/`DateTime` entram crus no texto SQL, que vai para `FromSqlRaw`/`ExecuteSqlRaw` **sem `DbParameter`**. O `name` da função/procedure também é interpolado. Arquivo idêntico nos 3 providers SQL. | `SqlServer/Repository/SqlExecutor.cs:367,375-407` (idem PostgreSql e MySql) |
| S2 | 🟡 Médio | Chave NuGet exposta a qualquer push: workflow publica com `secrets.nuget_key` em push de **qualquer branch**, sem environment protegido nem gate de tag/release. | `.github/workflows/dotnetcore.yml:3,66-67` |

**Correção do S1:** gerar placeholders (`@p0`/`$1`/`?`) e passar `DbParameter`s reais — a infraestrutura já
existe no próprio arquivo (`SetCommand`, `SqlExecutor.cs:419-445`) e é simplesmente ignorada nesses métodos.

### 2. Bugs de correção — Core (`eQuantic.Core.Data.EntityFramework`)

| # | Sev. | Problema | Local |
|---|------|----------|-------|
| C1 | 🔴 | **Double-dispose do UnitOfWork**: `AsyncQueryableRepository.Dispose(bool)` chama `base.Dispose()` (que já dispõe o UoW) e dispõe o UoW de novo — causado por campo `_disposed` sombreado na derivada. | `Repository/AsyncQueryableRepository.cs:756-773` + `Repository/QueryableRepository.cs:363-378` |
| C2 | 🔴 | **Ownership invertido do UoW**: repositórios dispõem o UnitOfWork **injetado**; com `AddGenericRepositories` o container também o dispõe → DbContext morto para os demais repositórios do escopo. | `Repository/QueryableRepository.cs:374`; `Read/QueryableReadRepository.cs:22`; `Write/WriteRepository.cs:15` |
| C3 | 🔴 | **Registro de DI quebra em runtime**: `ISqlUnitOfWork` é registrado incondicionalmente mesmo quando a implementação não o implementa (MongoDb) → `InvalidCastException` ao resolver. A linha 75 ainda duplica o registro de `IQueryableUnitOfWork` (código morto). | `Repository/Extensions/ServiceCollectionExtensions.cs:74-75` |
| A1 | 🟠 | **`All`/`Any` (sync) descartam `configuration`**: `return this.All(specification.SatisfiedBy());` ignora includes/no-tracking/sorting. As variantes async fazem certo — prova de que é bug, não design. | `Read/QueryableReadRepository.cs:208,233` |
| A2 | 🟠 | **`Get(id)` rejeita chaves default válidas com exceção errada**: `if (Equals(id, default(TKey))) throw new ArgumentNullException` — `Get(0)`/`Guid.Empty` lançam sobre um argumento não-nulo. | `Read/QueryableReadRepository.cs:254-257`; `Read/AsyncQueryableReadRepository.cs:357-360` |
| A3 | 🟠 | **Sync deferred vs async materializado**: `GetAll`/`GetFiltered`/`GetPaged` sync devolvem `IQueryable` viva disfarçada de `IEnumerable` (dupla enumeração = 2 queries; `ObjectDisposedException` tardia), enquanto os async fazem `ToListAsync`. Mesmo método, semânticas divergentes. | `Read/QueryableReadRepository.cs:47,271,299,396` vs `Read/AsyncQueryableReadRepository.cs:44,336,732` |
| A4 | 🟠 | **Chave como `Expression.Constant`**: `GetFindByKeyExpression` embute o valor da chave na árvore → EF não parametriza; cada id gera entrada nova no cache de queries e SQL com literal (poluição do plan cache). | `Repository/Extensions/DbContextExtensions.cs:25,43` |
| A5 | 🟠 | **Paginação sem `OrderBy`**: `Skip/Take` sem ordenação garantida → páginas não determinísticas + warning `RowLimitingOperationWithoutOrderBy` do EF. | `Read/QueryableReadRepository.cs:395`; `Read/AsyncQueryableReadRepository.cs:729` |
| M-core | 🟡 | Vários: NRT desabilitado no pacote inteiro; **zero `ConfigureAwait(false)`** em toda a biblioteca; `SumAsync` (≈24 overloads) sem `CancellationToken` nem validação de null; `AddRepository` ignora o lifetime configurado; reflection sem cache no caminho quente de `Get(id, config)`; `GetAllAsync` injeta `Where(_ => true)` redundante; tracking inconsistente entre `Get(id)` (usa `Find`) e `Get(id, config)` (usa query). | ver relatório detalhado por arquivo |

### 3. Bugs de correção — Providers

| # | Sev. | Problema | Local |
|---|------|----------|-------|
| P1 | 🔴 | **MongoDb opera no banco errado**: `GetDatabase(_collectionName)` usa o nome da **coleção** como nome do **banco** → `DeleteMany`/`UpdateMany` executam contra banco inexistente e retornam `0` **silenciosamente**. | `MongoDb/Repository/Set.cs:129-131` |
| P2 | 🔴 | **MongoDb `UpdateDefinitionBuilder` grava constante**: `x => new E { Count = x.Count + 1 }` é compilado e invocado contra `Activator.CreateInstance(...)` (instância default) → grava `0+1=1` em todos os documentos, em vez de incrementar. Corrupção silenciosa. | `MongoDb/UpdateDefinitionBuilder.cs:38-43` |
| P3 | 🟠 | **`EXEC` (T-SQL) copiado para PG/MySQL**: `GetQueryProcedure` retorna `$"EXEC {name}..."`; PostgreSQL/MySQL exigem `CALL` → `ExecuteProcedure` falha em runtime. Evidência direta do copy-paste. | `PostgreSql/Repository/SqlExecutor.cs:367`; `MySql/…:367` |
| P4 | 🟠 | **`FromSqlRaw` recebe `IEnumerable` como 1 parâmetro**: `FromSqlRaw(sql, configuration.Parameters.Select(p => p.Value))` — o `Select` vira um único elemento do `params object[]`. Falta `.ToArray()`. | `SqlExecutor.cs:219` (3 providers SQL) |
| P5 | 🟠 | **`ExpressionConverter` frágil**: `RewriteBinding` não faz rebind do parâmetro → updates que referenciam a entidade lançam em runtime; `GetMethods().…Single(...)` quebra se o EF adicionar overload de `SetProperty`; sem cache. | `SqlServer/ExpressionConverter.cs:50-68,137-155` (3 providers SQL) |
| P6 | 🟡 | **MongoDb: retornos silenciosos**: sem `IMongoClient` no DI, `GetCollection()` retorna null e os bulk ops retornam `0` sem lançar. `IsSimpleType` não cobre `Guid`/`DateTimeOffset`/coleções → updates descartados ou `TargetParameterCountException`. | `MongoDb/Repository/Set.cs:29-65`; `UpdateDefinitionBuilder.cs:45-73` |
| P7 | 🟡 | **`SqlExecutor.Dispose` destrói o `DbContext` injetado** (double-dispose no escopo DI). `IsMigrating` é `static` compartilhado entre todos os contextos do processo. Loop `do/while` de retry sem limite em `CommitAndRefreshChanges`. | `SqlExecutor.cs:483-497`; `UnitOfWork.cs:17,43-91` |
| P8 | 🟡 | **`GetEntityByIdSpecification` órfão**: existe só no SqlServer, nada nele é específico de SQL Server (delega para `DbContextExtensions` do base), zero usos no repo. Deveria estar no base ou ser deprecado. | `SqlServer/Specifications/GetEntityByIdSpecification.cs` |

### 4. Duplicação estrutural

| Arquivo | SqlServer | PostgreSql | MySql | MongoDb |
|---|---|---|---|---|
| `SqlExecutor.cs` | 498 linhas | **idêntico** | **idêntico** | — |
| `UnitOfWork.cs` | 266 | **idêntico** | **idêntico** | ~180 iguais |
| `ExpressionConverter.cs` | 175 | **idêntico** | **idêntico** | — |
| `Set.cs` | 287 | 268 (= SqlServer sem blocos legados) | **idêntico ao PG** | ~50 iguais |
| `SqlConfigurationExtensions.cs` | 14 | **idêntico** | **idêntico** | — |

**~2.400–2.500 linhas redundantes.** As únicas diferenças genuínas de dialeto em `SqlExecutor` são 2 linhas
(`GetQueryFunction`/`GetQueryProcedure`). Não é dívida só estética: o bug P3 (`EXEC`→`CALL`) existe
justamente porque o arquivo foi copiado sem adaptar o dialeto.

### 5. Packaging e versionamento

| # | Sev. | Problema | Local |
|---|------|----------|-------|
| PK1 | 🔴 | **Linhas de versão paralelas no mesmo `PackageId`**: 4.x multi-target + 6.x/7.x/8.x/9.x/10.x single-target. O NuGet vê uma linha do tempo única → `dotnet add package` puxa 10.0.2 (net10-only) e quebra restore em net6–net9; Dependabot sugere upgrade impossível; major deixa de significar breaking (significa TFM). | 21 csproj em `src/`; confirmado no nuget.org |
| PK2 | 🔴 | **Bug de grafo de dependência**: `MySql.Net10.csproj` referencia o core **Net9** → o pacote MySql 10.0.2 declara dependência do core `>= 9.1.2` (net9-only). SqlServer/PG/MongoDb NetX referenciam o core multi-target 4.4.2 — famílias já cruzadas. | `MySql/…MySql.Net10.csproj:66-67` |
| PK3 | 🔴 | **Pomelo 9 sobre EF Core 10**: MySql net10.0 usa `Pomelo.EntityFrameworkCore.MySql 9.0.0` (compilado p/ EF 9) com `Microsoft.EntityFrameworkCore 10.0.3`. Não há Pomelo 10 estável — risco de incompatibilidade binária. | `MySql/…MySql.csproj` (bloco net10) |
| PK4 | 🟠 | **Diretórios `obj/`/`bin/` compartilhados**: vários csproj na mesma pasta sem `BaseIntermediateOutputPath` → `project.assets.json` sobrescrito a cada restore; builds paralelos (`dotnet build -m` da solution) são corrida declarada. | `src/*/` com múltiplos csproj |
| PK5 | 🟠 | **MongoDb principal desalinhado**: `Version 8.1.2.0`, só `net8.0` — não há MongoDb na família 4.x nem multi-target; consumidor net9/net10 recebe o build net8. | `MongoDb/…MongoDb.csproj:7,9` |
| PK6 | 🟠 | **`AssemblyVersion` rotativa** (muda a cada patch) → num diamante entre providers compilados contra linhas diferentes do core, risco de `MissingMethodException`/`FileLoadException`. | todos os csproj `:27` |
| PK7 | 🟡 | **MSBump morto e quebrado**: `build/MSBump.props` importa a si mesmo (circular), `MSBump.targets` chama `BumpVersion` sem `UsingTask`, `Directory.Build.targets` está em `build/` (não é ancestral de `src/`, nunca é aplicado) e o próprio comentário diz que é obsoleto desde NuGet 4.6. Nada disso é importado hoje. Quando funcionava, gerava versões não determinísticas por build. | `build/*` |

### 6. Contratos (`eQuantic.Core.Data`)

| # | Sev. | Problema | Local |
|---|------|----------|-------|
| K1 | 🟠 | **Explosão combinatória**: `IAsyncReadRepository<TConfig,TEntity,TKey>` = **100 membros**; `IReadRepository<TConfig,…>` = 56; `ISqlUnitOfWork` ≈ 47 (inviável implementar à mão — anula o propósito de um contrato). `GetPagedAsync` = 18 overloads; `SumAsync` = 30. O commit mais recente *adicionou* 60 overloads de Sum — a tendência é piorar. | `Read/IAsyncReadRepository.cs`; `Sql/ISqlUnitOfWork.cs` |
| K2 | 🟠 | **`TUnitOfWork` como type parameter** habilita um único membro (`UnitOfWork { get; }`) mas contamina toda a hierarquia (~24 interfaces para 1 conceito) e cria acoplamento circular UoW↔repositório. Variância inconsistente sync vs async. | `Repository/IRepository.cs:26,55`; `IAsyncRepository.cs:39` |
| K3 | 🟠 | **Paginação sem metadados**: `GetPaged*` retorna `IEnumerable<T>` cru, sem total/página → consumidor faz `Count()` separado (2 round-trips não atômicas). Falta um `PagedResult<T>`. | `Read/IReadRepository.cs:267-314` |
| K4 | 🟡 | **Vazamento de EF no contrato**: `ISqlUnitOfWork` declara `GetPendingMigrations`/`UpdateDatabase`/`Attach` — contradiz a "persistence ignorance" que os próprios XML docs reivindicam. `IdentityGenerator` e `MigrationAttribute` são implementação num pacote de contratos. | `Sql/ISqlUnitOfWork.cs:23-127`; `IdentityGenerator.cs`; `Migration/MigrationAttribute.cs` |
| K5 | 🟡 | **`CancellationToken` ausente** em 30 `SumAsync`, `AddAsync`/`MergeAsync`/`ModifyAsync`/`RemoveAsync`, `LoadCollectionAsync`. NRT desabilitado (`Get*` retorna `Task<TEntity>` sem anotar null). Sem `IAsyncEnumerable`/streaming. | `Read/IAsyncReadRepository.cs`; `Write/IAsyncWriteRepository.cs` |
| K6 | 🟡 | **Constraint `new()` em tudo** (hostil a DDD) e `IEntity`/`IEntity<TKey>` não ligam o `TKey` do repositório à chave real da entidade (`IRepository<Cliente, Guid>` compila mesmo se a chave for `int`). Deveria ser `where TEntity : IEntity<TKey>`. | `Repository/IRepository.cs:40` |
| K7 | 🟢 | **Bug latente**: `IdentityGenerator.GuidRegex` contém 2 caracteres invisíveis de largura zero (U+200C e U+200B) dentro da classe `[0-9…a-fA-F]{12}` — artefato de copy-paste (confirmado por dump de bytes). Dependência morta `eQuantic.Core` (nenhum arquivo a importa). Typo `mintute` em `MigrationAttribute`. | `IdentityGenerator.cs:7`; `core-data.csproj` |

### 7. Processo, CI e testes

| # | Sev. | Problema | Local |
|---|------|----------|-------|
| Q1 | 🔴 | **CI publica em qualquer push, sem testes**: `on: [push]` → `dotnet nuget push` a cada push em qualquer branch, com a chave NuGet. **Nenhum `dotnet test`** roda antes de publicar. | `.github/workflows/dotnetcore.yml:3,66-67` |
| Q2 | 🔴 | **Testes são placebo**: `UnitTest1.cs` é um `Assert.Pass()`. Não há cobertura de nenhum bug acima. | `tests/…Tests/UnitTest1.cs` |
| Q3 | 🟡 | 21 `dotnet build` sequenciais em vez da solution; ações desatualizadas (`checkout@v3`, `setup-dotnet@v3`); `windows-latest` desnecessário; sem `dotnet test`, cache, `global.json`, pack determinístico (`ContinuousIntegrationBuild`), símbolos (snupkg) ou provenance. | `dotnetcore.yml` |
| Q4 | 🟡 | Sem `Directory.Build.props`/`Directory.Packages.props` centrais: metadados e versões de pacote repetidos em 21 csproj (fonte real de PK2/PK3). Sem NRT/`GenerateDocumentationFile` consistentes (pacotes vão ao NuGet sem IntelliSense). README diz "Version 4.4.0" e não explica a matriz de versões; `Repository.md` usa `IContainer`/service-locator pré-DI. | raiz do repo |

---

## Parte II — Plano de execução em fases

### Fase 0 — Blindar o pipeline (dias, sem tocar código de produção)

Pré-requisito de tudo: parar de publicar por acidente e ter uma rede de segurança.

1. **Separar CI de Release.** `ci.yml` em `push`/`pull_request`: `restore` → `build -warnaserror` → **`dotnet test`** → `dotnet pack` como artefato (sem push). `release.yml` só em `push: tags: ['v*']` (ou `release: published`), com a chave `nuget_key` num **GitHub Environment protegido**.
2. **Trocar os 21 builds** por `dotnet build eQuantic.Core.Data.EntityFramework.sln -c Release` (ou `dotnet pack`); mudar para `ubuntu-latest`; atualizar ações para v4; adicionar cache NuGet e `global.json`.
3. **Adotar MinVer** (versão derivada de tag git) e **deletar `build/`** (MSBump morto). Fixar `AssemblyVersion` por major.

### Fase 1 — Correções que não quebram contrato (linha atual, patch/minor)

Podem sair já, sem tocar o `eQuantic.Core.Data`. Cobrir cada uma com teste (Fase 0 garante que rodam).

- **Segurança S1:** parametrizar `SqlExecutor` (usar a infra `SetCommand` já existente).
- **Crash C3:** registrar `ISqlUnitOfWork` só se a impl o implementar; remover o registro duplicado.
- **Disposal C1/C2/P7:** unificar o flag `_disposed`, não dispor o UoW injetado (ownership de quem cria), respeitar o escopo do DI.
- **Correção de queries A1, A2, A4, A5, P4:** repassar `configuration` em `All`/`Any`; validar `id is null` em vez de `default`; parametrizar a chave; fallback de `OrderBy` pela PK; `.ToArray()` no `FromSqlRaw`.
- **MongoDb P1/P2/P6:** corrigir o `GetDatabase`, rejeitar (ou traduzir para `$inc`) updates que referenciam a entidade, lançar em vez de retornar `0` silencioso.
- **Dialeto P3:** `EXEC`→`CALL` em PG/MySql.
- **Higiene M-core:** `ConfigureAwait(false)`, cache da expressão de chave em `ConcurrentDictionary`, `AddRepository` respeitando lifetime.
- **PK2/PK3/PK5:** corrigir a referência do `MySql.Net10` para o core net10; alinhar MongoDb; documentar/pinar o risco Pomelo↔EF10.
- **K7 (contrato, não-breaking):** remover os caracteres zero-width da regex, a dependência morta `eQuantic.Core`, adicionar `[AttributeUsage]`/`GenerateDocumentationFile`.

### Fase 2 — De-duplicação estrutural (minor, refactor interno)

Criar no pacote base `eQuantic.Core.Data.EntityFramework`:
- `SqlExecutorBase` com `GetQueryFunction`/`GetQueryProcedure` `protected abstract` (dialeto) — colapsa ~500×2 linhas.
- `UnitOfWorkBase`/`UnitOfWorkBase<TDbContext>`, `ExpressionConverter<TEntity>` e o `GetQueryable`/`Load*` de `Set` no base.
- Mover `GetEntityByIdSpecification` (P8) para o base.

Cada provider passa a ser só o override de dialeto + o `csproj`. Remove ~2.400 linhas e mata a classe de bug do P3 na raiz. **Não muda API pública** — só reorganiza a implementação.

### Fase 3 — Redesenho dos contratos v5.0.0 (breaking deliberado — ver Parte III)

Consolidar a superfície via *options objects*, introduzir `PagedResult<T>`, `CancellationToken` uniforme,
NRT anotado, `where TEntity : IEntity<TKey>`, remover `TUnitOfWork` da hierarquia e o conteúdo
EF-specific dos contratos. Reimplementar no pacote EF (que fica ~10× menor).

### Fase 4 — Migração no nuget.org (ver Parte IV)

Consolidar as linhas de versão, deprecar as antigas sem quebrar quem já depende delas, e publicar a matriz
de compatibilidade.

---

## Parte III — Mudanças que exigem quebra de contrato no `eQuantic.Core.Data`

Regra geral do pacote de contratos: **adicionar** membro a uma interface já quebra todo implementador
externo (mocks, fakes, decorators, além da própria impl EF), e **mudar assinatura/remover** quebra também
os callers. Quase toda melhoria de fundo é, portanto, uma v5.0.0. O que exige breaking:

1. **`CancellationToken` nos membros que não têm** (30 `SumAsync`, `AddAsync`/`MergeAsync`/`ModifyAsync`/`RemoveAsync`, `LoadCollectionAsync`). Rota não-breaking parcial: *default interface methods* (DIM) delegando para a sobrecarga existente — viável porque todos os TFMs são ≥ net6.0, mas cristaliza a explosão de overloads.
2. **Consolidar overloads em options objects** (`QueryOptions<T>` absorvendo filter/specification/config; `PageRequest`): remover os 18 `GetPagedAsync`, os 60 `Sum*` etc. Reduz `IAsyncReadRepository` de 100 para ~12 membros. É o coração da v5.
3. **`PagedResult<T>` em vez de `IEnumerable<T>`** na paginação: mudança de tipo de retorno — breaking duro (nem DIM salva; exigiria método novo com outro nome, ex. `QueryPagedAsync`).
4. **Remover `TUnitOfWork` da hierarquia** (colapsar `IRepository<TUoW,TEntity,TKey>` em `IRepository<TEntity,TKey>` + `IUnitOfWork UnitOfWork { get; }`): remove ~10 interfaces públicas; o pacote EF referencia essas aridades em `GetRepository<TUnitOfWork,…>`.
5. **Constraint `where TEntity : IEntity<TKey>`** e/ou remover `new()`: muda constraints genéricas — source+binary breaking.
6. **Separar sync/async de `IUnitOfWork`** e **remover `ExecuteTransactionAsync` de `ISqlExecutor`** (método async na interface "sync").
7. **Mover `GetPendingMigrations`/`UpdateDatabase`/`MigrationAttribute`/`IdentityGenerator` para o pacote EF**: remove tipos/membros públicos do contrato — breaking real (a direção contrato→EF impede `[TypeForwardedTo]`).
8. **NRT anotado** (`TEntity?` em `Get`/`GetFirst`/`GetSingle`): tecnicamente só gera warnings novos — o breaking mais barato; exige que os dois pacotes sejam anotados em conjunto para ficarem coerentes.

**Recomendação:** tratar a próxima versão do contrato como **v5.0.0 deliberadamente breaking** e reimplementar
o pacote EF sobre ela, em vez de empilhar DIMs sobre uma superfície de 100 membros. O custo de manutenção
do pacote EF — que hoje implementa ~156 membros por provider × 4 providers — cai uma ordem de magnitude.

---

## Parte IV — Estratégia de versionamento no nuget.org

O problema PK1 tem duas saídas coerentes. A recomendada é a (A).

**(A) Consolidar numa única linha multi-target por `PackageId` (recomendado).**
Um `.csproj` multi-target por pacote (`net8.0;net9.0;net10.0` — net6/net7 estão EOL), uma única linha de
versão, retomada **acima** da mais alta já publicada para a linha do tempo voltar a ser crescente e
monotônica (ex.: **11.0.0**, ou 5.0.0 se aceitar que a "latest" numérica caia — o que confundiria quem já
está em 10.x). O multi-target já entrega o binário certo por TFM dentro de um único `.nupkg` — é
exatamente o que o esquema de linhas paralelas tenta emular à mão. Isso corrige PK1, PK2, PK4, PK5 e Q4 de
uma vez.

**(B) Manter famílias por .NET, mas com `PackageId` distintos.**
Ex.: `eQuantic.Core.Data.EntityFramework.Net8`. É a única forma de o NuGet tratar as famílias como linhas
independentes (a "latest" de cada id fica correta para seu TFM). Custo: fragmenta o ecossistema de
consumidores e a descoberta no nuget.org, e multiplica os pacotes a manter. Só vale se houver uma razão
forte para congelar cada TFM numa API própria.

**Migração sem quebrar quem já depende das versões antigas** (vale para A e B):
- **Nunca** despublicar (`unlist` mantém o restore de quem tem a versão fixada; `delete` quebra). Usar
  **deprecação** no nuget.org (`Legacy`/`Other`) nas versões antigas, apontando para a nova.
- Publicar a **matriz de compatibilidade** (TFM × pacote × versão) no README — hoje o README diz
  "Version 4.4.0" e não explica nada disso.
- Alinhar core + 4 providers para lançarem **sempre juntos, na mesma versão** (resolve os diamantes de
  `AssemblyVersion`).
- Só então retomar a numeração consolidada e apontar o CI de release para tags.

---

## Apêndice — Ordem sugerida (o que fazer primeiro)

1. **Fase 0** (pipeline) — desbloqueia tudo com segurança.
2. **S1, C1, C2, C3, P1, P2** — os 6 achados 🔴 de segurança/runtime, com testes.
3. **Fase 1 restante** (achados 🟠) na mesma linha atual.
4. **Fase 2** (de-dup) — barato e alto retorno, sem breaking.
5. **Fases 3–4** — planejar a v5.0.0 do contrato e a consolidação de versões como um marco à parte,
   comunicado com antecedência aos consumidores.
