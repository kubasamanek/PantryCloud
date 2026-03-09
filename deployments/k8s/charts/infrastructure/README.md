# pantry-infrastructure

Deploys the shared stateful infrastructure used by PantryCloud services: PostgreSQL, RabbitMQ, MongoDB, and Redis. Each component is a StatefulSet. Credentials are read from a single Kubernetes Secret; the chart does not create that Secret.

## Dependencies

Create the namespace and secrets first. From `deployments/k8s`: run `make secrets` (or `bash secrets/create-secrets.sh`) so that the Secret named by `credentialsSecret` exists. The infrastructure chart only references it.

## Install / upgrade

From `deployments/k8s`:

```bash
make infra-install
# or
helm upgrade --install pantry-infra ./charts/infrastructure -n pantry --wait --timeout 300s
```

Install order: `make secrets` → `make infra-install` → `make deploy-services`.

## Values reference

### Credentials

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `credentialsSecret` | string | `pantry-infra-credentials` | Name of the Secret that holds infrastructure credentials. Must match the Secret created by `secrets/create-secrets.sh`. The chart uses `valueFrom.secretKeyRef` to inject `POSTGRES_USER`, `POSTGRES_PASSWORD`, `RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS`, `MONGO_INITDB_ROOT_USERNAME`, `MONGO_INITDB_ROOT_PASSWORD` into the relevant containers. Do not put credentials in values; override them by setting environment variables when running the script (see [Secrets README](../../secrets/README.md)). |

### PostgreSQL

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `postgresql.image` | string | `postgres:17-alpine` | PostgreSQL image. |
| `postgresql.serviceName` | string | `pantry-postgresql` | Service and StatefulSet name. |
| `postgresql.port` | int | `5432` | Server port. |
| `postgresql.databases` | list | `identity_db`, `household_db`, … | Databases created by the init job. |
| `postgresql.storage` | string | `2Gi` | PVC size per instance. |
| `postgresql.resources` | object | requests/limits | CPU and memory. |

### RabbitMQ

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `rabbitmq.image` | string | `rabbitmq:4-management-alpine` | RabbitMQ image. |
| `rabbitmq.serviceName` | string | `pantry-rabbitmq` | Service and StatefulSet name. |
| `rabbitmq.port` | int | `5672` | AMQP port. |
| `rabbitmq.managementPort` | int | `15672` | Management UI port. |
| `rabbitmq.storage` | string | `1Gi` | PVC size. |
| `rabbitmq.resources` | object | requests/limits | CPU and memory. |

### MongoDB

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `mongodb.image` | string | `mongo:8.0` | MongoDB image. |
| `mongodb.serviceName` | string | `pantry-mongodb` | Service and StatefulSet name. |
| `mongodb.port` | int | `27017` | Server port. |
| `mongodb.storage` | string | `1Gi` | PVC size. |
| `mongodb.resources` | object | requests/limits | CPU and memory. |

### Redis

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `redis.image` | string | `redis:7-alpine` | Redis image. |
| `redis.serviceName` | string | `pantry-redis-master` | Service and StatefulSet name. |
| `redis.port` | int | `6379` | Server port. |
| `redis.storage` | string | `500Mi` | PVC size. |
| `redis.resources` | object | requests/limits | CPU and memory. |

## Secrets and configuration

Credentials are not in Helm values. The Secret named by `credentialsSecret` is created by `secrets/create-secrets.sh`. To use different credentials, set the script’s environment variables (e.g. `POSTGRES_PASSWORD`, `RABBITMQ_USER`) and run the script again; then upgrade the infrastructure release if needed. The init containers and probes read from the same Secret, so no extra values changes are required.

See [Secrets (Kubernetes)](../../secrets/README.md) and [Secrets (all deployment methods)](../../../SECRETS.md).
