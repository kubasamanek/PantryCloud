# PantryCloud Kubernetes Deployment

This directory contains the necessary configuration to deploy the PantryCloud distributed system locally using [Kind](https://kind.sigs.k8s.io/) (Kubernetes IN Docker).

## Prerequisites

Ensure the following tools are installed on your system:

| Tool      | Minimum Version | Purpose |
|-----------|-----------------|---------|
| Docker    | 24+             | Container runtime |
| Kind      | 0.20+           | Local Kubernetes cluster |
| kubectl   | 1.28+           | Cluster management |
| Helm      | 3.12+           | Package manager |
| Make      | -               | Build orchestration |
| openssl   | -               | JWT key generation |

**macOS Installation (Homebrew):**
```bash
make install-prereqs
```
*(Note: Docker must be installed manually via Docker Desktop or OrbStack).*

To verify your environment, run:
```bash
make check-prereqs
```

## Quick Start

1. Add the local DNS entry to your host machine:
   ```bash
   echo "127.0.0.1 pantry.test" | sudo tee -a /etc/hosts
   ```
2. Navigate to the deployment directory:
   ```bash
   cd deployments/k8s
   ```
3. Deploy the entire stack (pulls latest images from Docker Hub):
   ```bash
   make up
   ```
4. Access the application at [http://pantry.test](http://pantry.test).

### Testing Access
If the application is unreachable, ensure that your local port `80` is successfully bound to the cluster. Alternatively, use port-forwarding:
```bash
kubectl port-forward -n ingress-nginx svc/ingress-nginx-controller 8080:80
```
Then access the app at `http://localhost:8080`.

## Image Source Configuration

By default, deployments pull pre-built images from Docker Hub (`samanekj/*`). You can override this behavior to build images from your local source code by setting the `IMAGE_SOURCE` variable.

```bash
# Pull from Docker Hub (Default)
make images

# Build from local Dockerfiles
make images IMAGE_SOURCE=local
make redeploy IMAGE_SOURCE=local
make up IMAGE_SOURCE=local
```

## Step-by-Step Deployment

The `make up` command orchestrates the entire deployment. To deploy components individually, maintain the following order:

```bash
# 1. Provision the Kind cluster
make cluster-create

# 2. Install the NGINX Ingress controller
make ingress-install

# 3. Pull or build images and load them into the cluster
make images

# 4. Generate and inject secrets (JWT keys, infra credentials, per-service secrets)
make secrets

# 5. Deploy stateful infrastructure (PostgreSQL, RabbitMQ, MongoDB, Redis)
make infra-install

# 6. Deploy backend services and Web SPA (EF Core migrations run at service startup)
make deploy-services
```

## Available Commands

| Command | Description |
|---------|-------------|
| `make up` | Full automated deployment (cluster, secrets, infra, apps; migrations run at app startup). |
| `make down` | Tear down the Kind cluster. |
| `make images` | Load images into Kind (use `IMAGE_SOURCE=local` to build locally). |
| `make redeploy` | Re-load images and restart application deployments. |
| `make status` | Display the status of pods, services, and ingress. |
| `make deploy-<svc>` | Deploy a specific service (e.g., `make deploy-identity`). |
| `make logs-<svc>` | Tail logs for a specific service deployment. |
