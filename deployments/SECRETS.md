# Secrets and credentials

This document describes how secrets and credentials are handled for PantryCloud deployments: Kubernetes (Kind/cluster) and Docker Compose (local dev). No secrets are committed to the repository.

| Environment | Credentials source | JWT signing keys |
|-------------|---------------------|------------------|
| **Kubernetes** | `k8s/secrets/create-secrets.sh` creates Secrets; apps use `envFrom` (existingSecrets) and infra uses `secretKeyRef`. | Script generates RSA keys → `jwt-signing-keys` Secret → Identity mounts as volume at `/secrets/jwt`. |
| **Docker Compose** | Plain environment variables in `docker-compose.yml`. | One-off `jwt-keygen` container writes keys into named volume `jwt-secrets`; Identity mounts volume at `/app/secrets`. |

---

## Kubernetes

Secrets are created by the script at **`k8s/secrets/create-secrets.sh`**. Nothing is stored in Git; the script is the single source of truth for what exists in the cluster.

- **When to run:** Before `make infra-install` and `make deploy-services`. The full flow is: create namespace → run `create-secrets.sh` → install infrastructure → deploy services. `make up` runs the script automatically.
- **What it creates:** One Secret for JWT keys (`jwt-signing-keys`), one for infrastructure credentials (`pantry-infra-credentials`), and one per application service (e.g. `identity-api-secrets`, `household-api-secrets`). Application Secrets hold connection strings and messaging credentials; keys match .NET configuration (e.g. `ConnectionStrings__DefaultConnection`, `Messaging__RabbitMQ__Username`).
- **How apps get them:** Each service’s Helm values file lists the relevant Secret name(s) in `existingSecrets`. The chart injects those Secrets as `envFrom secretRef`, so every key in the Secret becomes an environment variable. The Identity service also mounts the `jwt-signing-keys` Secret as a read-only volume at `/secrets/jwt` (see `k8s/values/identity.yaml`).
- **Overriding:** Set environment variables before running the script (e.g. `POSTGRES_PASSWORD`, `RABBITMQ_USER`, `MONGODB_ROOT_PASSWORD`). The script writes them into `pantry-infra-credentials` and into the per-service Secrets. No need to change Helm values when you only change credentials.

Full reference: [k8s/secrets/README.md](k8s/secrets/README.md).

---

## Docker Compose (dev)

The Compose file in **`docker-compose.yml`** is for local development only. Credentials are not in a separate store.

- **Infrastructure and app credentials:** Stored as plain environment variables in the compose file. Examples: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `ConnectionStrings__DefaultConnection`, `Messaging__RabbitMQ__Username`, `Messaging__RabbitMQ__Password`. Intent is quick local dev; this layout is not for production.
- **JWT signing keys:** Not in env. A one-off service `jwt-keygen` runs `openssl` to generate RSA keys into a named volume `jwt-secrets`. The Identity service mounts that volume read-only at `/app/secrets` and is configured with `Jwt__PrivateKeyPath=/app/secrets/private.pem` and `Jwt__PublicKeyPath=/app/secrets/public.pem`. Keys are never in Git or in environment variables.
- **No `env_file`:** The stack does not use a `.env` file or Compose `env_file`; all configuration is inline in `docker-compose.yml`.

To change dev credentials, edit the `environment` blocks in `docker-compose.yml`. For production, use a proper secret store and do not rely on this file for secrets.
