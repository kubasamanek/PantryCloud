# pantry-observability

Deploys the observability stack for local or dev use: Prometheus, Grafana, Loki, Alloy, and Tempo. Used for metrics, dashboards, logs, and traces when running PantryCloud on Kind or a similar cluster.

## Dependencies

Namespace must exist. Infrastructure and application services do not depend on this chart; you can install observability before or after them. For full tracing, deploy apps after observability so they can target the Alloy OTLP endpoint.

## Install / upgrade

From `deployments/k8s`:

```bash
make observability-install
# or
helm upgrade --install pantry-observability ./charts/observability -n pantry --wait --timeout 300s
```

## Values reference

### Namespace

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `namespace` | string | `pantry` | Namespace where resources are created. |

### Prometheus

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `prometheus.image` | string | `prom/prometheus:v3.9.1` | Prometheus image. |
| `prometheus.serviceName` | string | `pantry-prometheus` | Service name. |
| `prometheus.port` | int | `9090` | HTTP port. |
| `prometheus.storage` | string | `2Gi` | PVC size. |
| `prometheus.retention` | string | `15d` | Data retention. |
| `prometheus.resources` | object | requests/limits | CPU and memory. |

### Grafana

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `grafana.image` | string | `grafana/grafana:12.3.3` | Grafana image. |
| `grafana.serviceName` | string | `pantry-grafana` | Service name. |
| `grafana.port` | int | `3000` | HTTP port. |
| `grafana.storage` | string | `1Gi` | PVC size. |
| `grafana.adminUser` | string | `admin` | Admin username. Rendered into a Secret used by the Grafana deployment. |
| `grafana.adminPassword` | string | `admin` | Admin password. **Dev/local only.** Override in values or use an external secret manager for production. |
| `grafana.resources` | object | requests/limits | CPU and memory. |

### Loki

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `loki.image` | string | `grafana/loki:3.6.6` | Loki image. |
| `loki.serviceName` | string | `pantry-loki` | Service name. |
| `loki.port` | int | `3100` | HTTP port. |
| `loki.storage` | string | `5Gi` | PVC size. |
| `loki.retentionPeriod` | string | `168h` | Log retention. |
| `loki.resources` | object | requests/limits | CPU and memory. |

### Alloy

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `alloy.image` | string | `grafana/alloy:v1.13.1` | Alloy image. |
| `alloy.serviceName` | string | `pantry-alloy` | Service name. Used by app services as the OTLP endpoint (`http://pantry-alloy:4317`). |
| `alloy.resources` | object | requests/limits | CPU and memory. |

### Tempo

| Value | Type | Default | Notes |
|-------|------|---------|-------|
| `tempo.image` | string | `grafana/tempo:2.10.1` | Tempo image. |
| `tempo.serviceName` | string | `pantry-tempo` | Service name. |
| `tempo.port` | int | `3200` | HTTP port. |
| `tempo.otlpGrpcPort` | int | `4317` | OTLP gRPC port. |
| `tempo.storage` | string | `5Gi` | PVC size. |
| `tempo.retentionPeriod` | string | `72h` | Trace retention. |
| `tempo.resources` | object | requests/limits | CPU and memory. |

## Secrets and configuration

Grafana admin credentials (`adminUser`, `adminPassword`) are in values and are written into a Kubernetes Secret by the chart. This is acceptable for local/Kind. For production, override these via Helm values (e.g. from CI or a secret store) or inject credentials via an external secrets operator and do not rely on the default `admin`/`admin`.

No other secrets are required for this chart. Datasources (Prometheus, Loki, Tempo) are provisioned via ConfigMaps pointing at the in-cluster service names.
