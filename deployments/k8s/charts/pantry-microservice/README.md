# pantry-microservice

Universal Helm chart for PantryCloud .NET microservices. One chart, multiple releases: each service (Identity, Household, Pantry, Recipe, ShoppingList, Notification, Audit, API Gateway, Web) is deployed as a separate release with its own `values/*.yaml` file.

## Dependencies

No Helm chart dependencies. The cluster must have infrastructure (PostgreSQL, RabbitMQ, MongoDB, Redis as needed) and secrets already created. Run `make secrets` and `make infra-install` before deploying services.

## Install / upgrade

From `deployments/k8s`:

```bash
# All services (recommended)
make deploy-services

# Single service
make deploy-identity
helm upgrade --install identity ./charts/pantry-microservice -n pantry -f values/identity.yaml --wait --timeout 180s
```

Values files live in `values/` (e.g. `values/identity.yaml`, `values/api-gateway.yaml`). The Makefile uses the chart once per service with the matching values file.

## Values reference

Values you are likely to set or override. Defaults are in `values.yaml`; service-specific overrides are in `values/<service>.yaml`.

### Image and naming

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `image.repository` | string | `""` | Container image repository (e.g. `samanekj/identityservice`). |
| `image.tag` | string | `latest` | Image tag. |
| `image.pullPolicy` | string | `IfNotPresent` | When to pull the image. |
| `fullnameOverride` | string | `""` | Overrides the default release-based name. Used so Deployments/Services have stable names (e.g. `identity-api`, `household-api`). |

### Replicas and scaling

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `replicaCount` | int | `1` | Used when HPA is disabled. |
| `hpa.enabled` | bool | `false` | Enable HorizontalPodAutoscaler. |
| `hpa.minReplicas` | int | `1` | Minimum replicas under HPA. |
| `hpa.maxReplicas` | int | `3` | Maximum replicas under HPA. |
| `hpa.targetCPUUtilizationPercentage` | int | `75` | CPU target for scaling. |
| `hpa.targetMemoryUtilizationPercentage` | int | `80` | Memory target for scaling. |

### Service

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `service.type` | string | `ClusterIP` | Service type. |
| `service.port` | int | `8080` | Port exposed by the service. Use `80` for the Web frontend. |

### Configuration (env and secrets)

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `env` | object | `{}` | Non-sensitive env vars. Rendered as a ConfigMap and injected via `envFrom`. Keys become env var names (e.g. `ASPNETCORE_ENVIRONMENT`, `Jwt__Issuer`). |
| `secretEnv` | object | `{}` | Sensitive env vars. The chart creates a Secret from this map and injects it via `envFrom`. Not used by current PantryCloud values; prefer `existingSecrets` for credentials. |
| `existingSecrets` | list of strings | `[]` | Names of existing Kubernetes Secrets. Each is injected as `envFrom secretRef`; keys in the Secret become env vars. Primary mechanism for DB connection strings, RabbitMQ credentials, etc. Secrets are created by `secrets/create-secrets.sh`. |

### Probes

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `probes.enabled` | bool | `true` | Enable startup, liveness, and readiness probes. |
| `probes.path` | string | `/health` | HTTP path for all probes. API Gateway uses `/api/health`. |
| `probes.liveness.initialDelaySeconds` | int | `15` | Delay before first liveness check. |
| `probes.liveness.periodSeconds` | int | `20` | Liveness check interval. |
| `probes.readiness.*` | int | (see values.yaml) | Readiness timing. |
| `probes.startup.*` | int | (see values.yaml) | Startup probe; increase if the app is slow to start. |

### Resources

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `resources.requests` | object | `cpu: 100m`, `memory: 128Mi` | Requested resources. |
| `resources.limits` | object | `cpu: 500m`, `memory: 512Mi` | Resource limits. Adjust for load. |

### Ingress

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `ingress.enabled` | bool | `false` | Expose the service via Ingress. |
| `ingress.className` | string | `nginx` | Ingress class (e.g. NGINX). |
| `ingress.hosts` | list | `[]` | List of `host` and `paths` (path, pathType). |
| `ingress.tls` | list | `[]` | TLS host and secretName per host. |
| `ingress.annotations` | object | `{}` | Annotations for the Ingress. |

### Volumes

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `volumes` | list | `[]` | Pod-level volumes. Identity uses a volume backed by Secret `jwt-signing-keys` for JWT signing keys. |
| `volumeMounts` | list | `[]` | Container-level mounts. Identity mounts the JWT secret at `/secrets/jwt` (read-only). |

For Identity, the JWT keys Secret is named `jwt-signing-keys`; it is created by `create-secrets.sh` and mounted at `/secrets/jwt` with files `private.pem` and `public.pem`. Values in `env` must point to these paths (e.g. `Jwt__PrivateKeyPath: /secrets/jwt/private.pem`).

### PodDisruptionBudget

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `pdb.enabled` | bool | `false` | Enable a PDB. |
| `pdb.minAvailable` | int or null | `null` | Minimum pods that must remain available. Use with multiple replicas (e.g. gateway, identity). |
| `pdb.maxUnavailable` | int or null | `1` | Maximum pods that can be unavailable. Only one of `minAvailable` / `maxUnavailable` should be set. |

### Observability

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `observability.otlpEndpoint` | string | `http://pantry-alloy:4317` | OTLP endpoint for traces/metrics. |

## Secrets and configuration

Sensitive configuration (connection strings, RabbitMQ credentials, JWT keys) is not stored in values. Application services receive credentials via **existingSecrets**: list the Secret names in `existingSecrets`; those Secrets are created by `secrets/create-secrets.sh` and contain keys that match the app’s configuration (e.g. `ConnectionStrings__DefaultConnection`, `Messaging__RabbitMQ__Username`). Identity also requires the **jwt-signing-keys** Secret mounted as a volume at `/secrets/jwt` (see `values/identity.yaml`).

Full details: [Secrets (Kubernetes)](../../secrets/README.md) and [Secrets (all deployment methods)](../../../SECRETS.md).
