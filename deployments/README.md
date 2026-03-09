## PantryCloud deployments

This directory contains deployment configurations for running PantryCloud in different environments.

## Local development (Docker Compose)

Use the Docker Compose stack to run the full system on a single machine:

```bash
docker-compose -f deployments/docker-compose.yml up
```

From the `deployments/` directory you can also run:

```bash
docker-compose up
```

The stack brings up:

- PostgreSQL, RabbitMQ, MongoDB, Redis, and supporting services (e.g. smtp4dev)
- All backend microservices (Identity, Household, Pantry, Recipe, ShoppingList, Notification, Audit)
- API Gateway (`pantry-gateway`)
- Web frontend (`pantry-web`)

Typical endpoints:

- Web UI: `http://localhost:3000`
- API Gateway: `http://localhost:5050`

Secrets and credentials for the Compose stack are described in `[SECRETS.md](SECRETS.md)`.

## Local Kubernetes (Kind)

For a local Kubernetes cluster using Kind, including ingress, infrastructure, observability, and all services, use the Make targets under `deployments/k8s`:

- `[k8s/README.md](k8s/README.md)` – quick start, prerequisites, and deployment steps.
- `k8s/charts/*/README.md` – chart-specific values and install instructions.

## Secrets

Secrets and credentials for both Docker Compose and Kubernetes deployments are documented in:

- `[SECRETS.md](SECRETS.md)` – overview for all deployment methods.
- `[k8s/secrets/README.md](k8s/secrets/README.md)` – details of the `create-secrets.sh` script and the Secrets it creates.

