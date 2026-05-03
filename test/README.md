## Tests

The `test/` directory contains unit and integration tests for PantryCloud services and shared components.

## Running tests

From the repository root:

```bash
dotnet test
```

- **Unit tests** – live under `test/{ServiceName}/{ServiceName}.UnitTests/` and do not require any external infrastructure.
- **Integration tests** – live under `test/{ServiceName}/{ServiceName}.IntegrationTests/` and use Testcontainers to start dependent services (PostgreSQL, RabbitMQ, MongoDB, Redis, service images). Docker must be running; the first run will pull required images.

You can run a single test project by targeting its `.csproj`:

```bash
dotnet test test/PantryCloud.Identity/PantryCloud.Identity.UnitTests/PantryCloud.Identity.UnitTests.csproj
dotnet test test/PantryCloud.Identity/PantryCloud.Identity.IntegrationTests/PantryCloud.Identity.IntegrationTests.csproj
```

## Structure and conventions

Per service, tests are grouped as:

- `test/{ServiceName}/{ServiceName}.UnitTests/` – xUnit-based unit tests with NSubstitute and Shouldly.
- `test/{ServiceName}/{ServiceName}.IntegrationTests/` – xUnit-based integration tests using Testcontainers and helpers from `PantryCloud.SharedKernel.Testing`.

Integration tests typically follow this pattern:

- A `*Container` type per dependency (service API, Postgres, RabbitMQ, MongoDB, Redis) that builds the corresponding Testcontainers container.
- A `*TestFixture` that wires containers together, applies EF Core migrations via `PostgresDatabaseSetup`, and exposes typed HTTP clients or service endpoints.
- An `IntegrationTestCollection` so tests share the fixture instance.

Some test projects include additional READMEs for manual or service-specific verification steps, for example:

- `test/PantryCloud.ApiGateway/PantryCloud.ApiGateway.IntegrationTests/README.md` – manual checks for rate limiting and correlation IDs.

