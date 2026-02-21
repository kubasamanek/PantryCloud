# Kubernetes Secrets

All deployment secrets are created by `create-secrets.sh`. No credentials are stored in version control; the script is the single source of truth.

## Usage

```bash
# Default: create secrets in namespace "pantry"
./create-secrets.sh
```

**When to run:** Before `make infra-install` and `make deploy-services`. The main Makefile target `make up` runs `make secrets` first.

**Prerequisites:** `kubectl` configured against a cluster, `openssl` installed.

## Secrets Created

| Secret | Used by | Contents |
|--------|---------|----------|
| **jwt-signing-keys** | Identity service | `private.pem`, `public.pem` (RSA 2048, generated each run) |
| **pantry-infra-credentials** | PostgreSQL, RabbitMQ, MongoDB StatefulSets | `POSTGRES_USER`, `POSTGRES_PASSWORD`, `RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS`, `MONGO_INITDB_ROOT_USERNAME`, `MONGO_INITDB_ROOT_PASSWORD` |
| **identity-api-secrets** | identity-api | `ConnectionStrings__DefaultConnection`, `Email__*` |
| **household-api-secrets** | household-api | `ConnectionStrings__DefaultConnection`, `Messaging__RabbitMQ__Username`, `Messaging__RabbitMQ__Password` |
| **pantry-api-secrets** | pantry-api | Same as household |
| **shoppinglist-api-secrets** | shoppinglist-api | Same as household |
| **audit-api-secrets** | audit-api | Same as household |
| **notification-api-secrets** | notification-api | `ConnectionStrings__DefaultConnection`, RabbitMQ creds, `ConnectionStrings__Redis` |
| **recipe-api-secrets** | recipe-api | `ConnectionStrings__DefaultConnection` (MongoDB URI) |

Application services reference these via `existingSecrets` in their Helm values; infrastructure templates use `valueFrom.secretKeyRef` to `pantry-infra-credentials`.

## Overriding Defaults

Set environment variables before running the script to use non-default credentials or hosts:

- **PostgreSQL:** `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_HOST`, `POSTGRES_PORT`
- **RabbitMQ:** `RABBITMQ_USER`, `RABBITMQ_PASSWORD`
- **MongoDB:** `MONGODB_ROOT_USER`, `MONGODB_ROOT_PASSWORD`, `MONGODB_HOST`, `MONGODB_PORT`
- **Redis:** `REDIS_HOST`, `REDIS_PORT`

The script writes these into `pantry-infra-credentials` (and into per-service connection strings). The infrastructure Helm chart reads the secret for PostgreSQL (user, password), RabbitMQ, and MongoDB—init scripts and probes use the secret, so you do not need to change any Helm values when overriding.

Example:

```bash
POSTGRES_PASSWORD=mysecret ./create-secrets.sh pantry
```
