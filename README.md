## PantryCloud

PantryCloud is a distributed pantry and household management system built as a set of .NET 9 microservices behind an API Gateway with a web frontend. The codebase follows Clean Architecture and Domain-Driven Design, with shared infrastructure hosted in a SharedKernel library.

## Prerequisites

- **.NET SDK**: 9.x
- **Docker** and **Docker Compose**
- **Git** (for typical clone and CI workflows)

For Kubernetes-based local deployment (Kind) prerequisites, see [deployments/k8s/README.md](deployments/k8s/README.md).

## Repository layout

- `src/Services/` – microservices:
  - Identity, Household, Pantry, Recipe, ShoppingList, Notification, Audit
  - Each service is split into Core, Application, Infrastructure, Presentation projects.
- `src/BuildingBlocks/` – SharedKernel runtime and testing utilities used by all services.
- `src/Gateways/` – API Gateway (`PantryCloud.ApiGateway`) built on YARP.
- `src/Web/` – web frontend (`PantryCloud.Web`).
- `test/` – unit and integration tests per service.
- `deployments/` – Docker Compose for local development and Kubernetes (Kind) manifests and Helm charts.

## Run locally (Docker Compose)

From the repository root:

```bash
docker-compose -f deployments/docker-compose.yml up
```

This brings up PostgreSQL, RabbitMQ, MongoDB, Redis, all backend services, the API Gateway, and the web frontend. Typical entry points:

- Web UI: `http://localhost:3000`
- API Gateway: `http://localhost:5050`

Secrets and credentials for the Compose stack are documented in [deployments/SECRETS.md](deployments/SECRETS.md).

## Run on Kubernetes (Kind)

For a local Kubernetes cluster using Kind, including NGINX ingress, infrastructure, observability, and all services, follow:

- [deployments/k8s/README.md](deployments/k8s/README.md) – full quick start, Make targets, and chart references.

Charts for individual components are documented under:

- `deployments/k8s/charts/pantry-microservice/README.md`
- `deployments/k8s/charts/infrastructure/README.md`
- `deployments/k8s/charts/observability/README.md`

Secrets for Kubernetes and Compose are described in [deployments/SECRETS.md](deployments/SECRETS.md) and [deployments/k8s/secrets/README.md](deployments/k8s/secrets/README.md).

## Tests

Run all tests from the solution root:

```bash
dotnet test
```

- **Unit tests** live under `test/{ServiceName}/{ServiceName}.UnitTests/` and do not require Docker.
- **Integration tests** live under `test/{ServiceName}/{ServiceName}.IntegrationTests/` and use Testcontainers to start service images and infrastructure; Docker must be running, and the first run will pull images.

Each service’s integration tests follow a common pattern:

- A shared test fixture that orchestrates containers (service API, Postgres, RabbitMQ, Redis, MongoDB as needed).
- Per-service `IntegrationTestCollection` so tests share the fixture instance.
- Helper types from `PantryCloud.SharedKernel.Testing` for containers, database setup, and JWTs.

## Secrets and deployment

How secrets and credentials are handled for both Kubernetes and Docker Compose is documented in:

- [deployments/SECRETS.md](deployments/SECRETS.md)
- [deployments/k8s/secrets/README.md](deployments/k8s/secrets/README.md)

## Architecture and standards

PantryCloud follows a strict layered architecture per service:

- **Core** – domain entities, value objects, repository interfaces, and configurations (no external dependencies).
- **Application** – commands, queries, handlers, DTOs, validators; depends on Core and SharedKernel.
- **Infrastructure** – EF Core DbContexts and repositories, MassTransit consumers, external integrations; depends on Application, Core, and SharedKernel.
- **Presentation** – ASP.NET Core entrypoint and controllers; depends on Application, Infrastructure, and SharedKernel.

Cross-cutting infrastructure (messaging, persistence helpers, identity, correlation, logging, testing utilities) lives in `src/BuildingBlocks/PantryCloud.SharedKernel` and `src/BuildingBlocks/PantryCloud.SharedKernel.Testing`. New services should reuse these building blocks rather than reimplementing infrastructure patterns.

