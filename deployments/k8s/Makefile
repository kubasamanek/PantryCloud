CLUSTER_NAME   := pantry-cloud
NAMESPACE      := pantry
REPO_ROOT      := $(shell git -C ../.. rev-parse --show-toplevel 2>/dev/null || echo "../..")
CHART          := ./charts/pantry-microservice
IMAGE_TAG      := latest

# ──────────────────────────────────────────────
# Image Source: set IMAGE_SOURCE=local to build from Dockerfiles
#               set IMAGE_SOURCE=hub  to pull from Docker Hub (default)
#
# Usage:
#   make up                       # pulls from Docker Hub
#   make up IMAGE_SOURCE=local    # builds from local Dockerfiles
# ──────────────────────────────────────────────
IMAGE_SOURCE ?= hub

# Docker Hub repositories (used when IMAGE_SOURCE=hub)
HUB_identity      := samanekj/identityservice
HUB_household     := samanekj/householdservice
HUB_pantry        := samanekj/pantryservice
HUB_recipe        := samanekj/recipeservice
HUB_shoppinglist  := samanekj/shoppinglistservice
HUB_notification  := samanekj/notificationservice
HUB_audit         := samanekj/auditservice
HUB_api-gateway   := samanekj/apigateway
HUB_web           := samanekj/pantrycloud-web

# Dockerfile paths (used when IMAGE_SOURCE=local, relative to REPO_ROOT)
DF_identity      := src/Services/PantryCloud.Identity/PantryCloud.Identity.Presentation/Dockerfile
DF_household     := src/Services/PantryCloud.Household/PantryCloud.Household.Presentation/Dockerfile
DF_pantry        := src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Presentation/Dockerfile
DF_recipe        := src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Presentation/Dockerfile
DF_shoppinglist  := src/Services/PantryCloud.ShoppingList/PantryCloud.ShoppingList.Presentation/Dockerfile
DF_notification  := src/Services/PantryCloud.Notification/PantryCloud.Notification.Presentation/Dockerfile
DF_audit         := src/Services/PantryCloud.Audit/PantryCloud.Audit.Presentation/Dockerfile
DF_api-gateway   := src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Presentation/Dockerfile
DF_web           := src/Web/PantryCloud.Web/Dockerfile

ALL_COMPONENTS := identity household pantry recipe shoppinglist notification audit api-gateway web

.PHONY: help
help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-25s\033[0m %s\n", $$1, $$2}'

# ──────────────────────────────────────────────
# Prerequisites
# ──────────────────────────────────────────────

REQUIRED := docker kind kubectl helm openssl

.PHONY: check-prereqs
check-prereqs: ## Verify required tools are installed
	@missing=""; \
	for cmd in $(REQUIRED); do \
		command -v $$cmd >/dev/null 2>&1 || missing="$$missing $$cmd"; \
	done; \
	if [ -n "$$missing" ]; then \
		echo ""; \
		echo "Missing required tools:$$missing"; \
		echo ""; \
		echo "Install with Homebrew (macOS):"; \
		echo "  brew install kind kubectl helm openssl"; \
		echo "  Docker: install Docker Desktop from https://docs.docker.com/get-docker/"; \
		echo ""; \
		echo "Or run:  make install-prereqs"; \
		echo ""; \
		exit 1; \
	fi; \
	echo "==> All required tools found: $(REQUIRED)"

.PHONY: install-prereqs
install-prereqs: ## Install Kind, kubectl, Helm via Homebrew (Docker must be installed separately)
	@command -v brew >/dev/null 2>&1 || { echo "Homebrew required. Install from https://brew.sh"; exit 1; }
	@echo "==> Installing kind, kubectl, helm..."
	@brew install kind kubectl helm openssl
	@echo ""
	@echo "==> Done. Ensure Docker is running (Docker Desktop or colima)."
	@echo "     Then run: make up"

# ──────────────────────────────────────────────
# Cluster Lifecycle
# ──────────────────────────────────────────────

.PHONY: cluster-create
cluster-create: check-prereqs ## Create Kind cluster
	kind create cluster --config kind-cluster.yaml

.PHONY: cluster-delete
cluster-delete: ## Delete Kind cluster
	kind delete cluster --name $(CLUSTER_NAME)

.PHONY: calico-install
calico-install: ## Install Calico CNI (required for NetworkPolicy enforcement)
	kubectl apply -f https://raw.githubusercontent.com/projectcalico/calico/v3.29.3/manifests/calico.yaml
	@echo "==> Waiting for Calico pods to be ready..."
	kubectl wait --namespace kube-system \
		--for=condition=ready pod \
		--selector=k8s-app=calico-node \
		--timeout=180s
	@echo "==> Waiting for nodes to be ready..."
	kubectl wait --for=condition=Ready nodes --all --timeout=120s

.PHONY: require-cluster
require-cluster: ## Check that a Kubernetes cluster is reachable (used by infra-install, secrets, deploy-services)
	@kubectl cluster-info --request-timeout=5s >/dev/null 2>&1 || (echo ""; echo "Error: No Kubernetes cluster is reachable."; echo "  Create the Kind cluster first:  make cluster-create"; echo "  Or run the full deploy:         make up"; echo ""; exit 1)

.PHONY: ingress-install
ingress-install: ## Install NGINX Ingress Controller for Kind
	kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml
	@echo "==> Pinning ingress controller to control-plane node (where Kind exposes host ports)..."
	@kubectl rollout status deployment/ingress-nginx-controller -n ingress-nginx --timeout=60s 2>/dev/null || true
	kubectl patch deployment ingress-nginx-controller -n ingress-nginx \
		--type=json -p='[{"op":"add","path":"/spec/template/spec/nodeSelector/ingress-ready","value":"true"}]'
	@echo "==> Waiting for ingress controller to be ready..."
	kubectl wait --namespace ingress-nginx \
		--for=condition=ready pod \
		--selector=app.kubernetes.io/component=controller \
		--timeout=120s

# ──────────────────────────────────────────────
# Docker Images
# ──────────────────────────────────────────────

.PHONY: images
images: ## Pull or build all images, then load into Kind (respects IMAGE_SOURCE)
ifeq ($(IMAGE_SOURCE),local)
	@echo "==> Building images from local Dockerfiles..."
	docker build -t $(HUB_identity):$(IMAGE_TAG)     -f $(REPO_ROOT)/$(DF_identity)     $(REPO_ROOT)
	docker build -t $(HUB_household):$(IMAGE_TAG)    -f $(REPO_ROOT)/$(DF_household)    $(REPO_ROOT)
	docker build -t $(HUB_pantry):$(IMAGE_TAG)       -f $(REPO_ROOT)/$(DF_pantry)       $(REPO_ROOT)
	docker build -t $(HUB_recipe):$(IMAGE_TAG)       -f $(REPO_ROOT)/$(DF_recipe)       $(REPO_ROOT)
	docker build -t $(HUB_shoppinglist):$(IMAGE_TAG) -f $(REPO_ROOT)/$(DF_shoppinglist) $(REPO_ROOT)
	docker build -t $(HUB_notification):$(IMAGE_TAG) -f $(REPO_ROOT)/$(DF_notification) $(REPO_ROOT)
	docker build -t $(HUB_audit):$(IMAGE_TAG)        -f $(REPO_ROOT)/$(DF_audit)        $(REPO_ROOT)
	docker build -t $(HUB_api-gateway):$(IMAGE_TAG)  -f $(REPO_ROOT)/$(DF_api-gateway)  $(REPO_ROOT)
	docker build -t $(HUB_web):$(IMAGE_TAG)          -f $(REPO_ROOT)/$(DF_web)          $(REPO_ROOT)
else
	@echo "==> Pulling images from Docker Hub..."
	docker pull $(HUB_identity):$(IMAGE_TAG)
	docker pull $(HUB_household):$(IMAGE_TAG)
	docker pull $(HUB_pantry):$(IMAGE_TAG)
	docker pull $(HUB_recipe):$(IMAGE_TAG)
	docker pull $(HUB_shoppinglist):$(IMAGE_TAG)
	docker pull $(HUB_notification):$(IMAGE_TAG)
	docker pull $(HUB_audit):$(IMAGE_TAG)
	docker pull $(HUB_api-gateway):$(IMAGE_TAG)
	docker pull $(HUB_web):$(IMAGE_TAG)
endif
	@echo "==> Loading images into Kind..."
	kind load docker-image $(HUB_identity):$(IMAGE_TAG)     --name $(CLUSTER_NAME)
	kind load docker-image $(HUB_household):$(IMAGE_TAG)    --name $(CLUSTER_NAME)
	kind load docker-image $(HUB_pantry):$(IMAGE_TAG)       --name $(CLUSTER_NAME)
	kind load docker-image $(HUB_recipe):$(IMAGE_TAG)       --name $(CLUSTER_NAME)
	kind load docker-image $(HUB_shoppinglist):$(IMAGE_TAG) --name $(CLUSTER_NAME)
	kind load docker-image $(HUB_notification):$(IMAGE_TAG) --name $(CLUSTER_NAME)
	kind load docker-image $(HUB_audit):$(IMAGE_TAG)        --name $(CLUSTER_NAME)
	kind load docker-image $(HUB_api-gateway):$(IMAGE_TAG)  --name $(CLUSTER_NAME)
	kind load docker-image $(HUB_web):$(IMAGE_TAG)          --name $(CLUSTER_NAME)

# ──────────────────────────────────────────────
# Infrastructure
# ──────────────────────────────────────────────

INFRA_CHART := ./charts/infrastructure

.PHONY: namespace
namespace: require-cluster ## Create the pantry namespace
	kubectl create namespace $(NAMESPACE) --dry-run=client -o yaml | kubectl apply -f -

# Infrastructure images (official Docker Hub images)
INFRA_IMAGES := postgres:17-alpine rabbitmq:4-management-alpine mongo:8.0 redis:7-alpine

# Observability images
OBSERVABILITY_IMAGES := prom/prometheus:v3.9.1 grafana/grafana:12.3.3

.PHONY: infra-images
infra-images: ## Pre-pull infrastructure images and load into Kind
	@echo "==> Pulling infrastructure images on host..."
	@for img in $(INFRA_IMAGES); do \
		echo "  pulling $$img ..."; \
		docker pull $$img; \
	done
	@echo "==> Loading infrastructure images into Kind..."
	@for img in $(INFRA_IMAGES); do \
		echo "  loading $$img ..."; \
		kind load docker-image $$img --name $(CLUSTER_NAME); \
	done
	@echo "==> Done. Now run: make infra-install"

.PHONY: infra-install
infra-install: namespace ## Deploy infrastructure (PostgreSQL, RabbitMQ, MongoDB, Redis)
	helm upgrade --install pantry-infra $(INFRA_CHART) \
		--namespace $(NAMESPACE) \
		--wait --timeout 300s

.PHONY: infra-delete
infra-delete: ## Delete infrastructure release
	helm uninstall pantry-infra --namespace $(NAMESPACE) || true

# ──────────────────────────────────────────────
# Observability (Prometheus, Grafana)
# ──────────────────────────────────────────────

OBSERVABILITY_CHART := ./charts/observability

.PHONY: observability-install
observability-install: namespace ## Deploy observability stack (Prometheus, Grafana)
	helm upgrade --install pantry-observability $(OBSERVABILITY_CHART) \
		--namespace $(NAMESPACE) \
		--wait --timeout 300s

.PHONY: observability-delete
observability-delete: ## Delete observability stack
	helm uninstall pantry-observability --namespace $(NAMESPACE) || true

# ──────────────────────────────────────────────
# Secrets
# ──────────────────────────────────────────────

.PHONY: secrets
secrets: namespace ## Create JWT signing keys and other secrets (requires cluster)
	bash secrets/create-secrets.sh $(NAMESPACE)

# ──────────────────────────────────────────────
# Application Services
# ──────────────────────────────────────────────

.PHONY: deploy-services
deploy-services: require-cluster ## Deploy all application services in dependency order (requires cluster)
	@echo "==> Deploying Identity..."
	helm upgrade --install identity $(CHART) \
		--namespace $(NAMESPACE) -f values/identity.yaml --wait --timeout 180s
	@echo "==> Deploying Household..."
	helm upgrade --install household $(CHART) \
		--namespace $(NAMESPACE) -f values/household.yaml --wait --timeout 180s
	@echo "==> Deploying Pantry..."
	helm upgrade --install pantry $(CHART) \
		--namespace $(NAMESPACE) -f values/pantry.yaml --wait --timeout 180s
	@echo "==> Deploying ShoppingList..."
	helm upgrade --install shoppinglist $(CHART) \
		--namespace $(NAMESPACE) -f values/shoppinglist.yaml --wait --timeout 180s
	@echo "==> Deploying Recipe..."
	helm upgrade --install recipe $(CHART) \
		--namespace $(NAMESPACE) -f values/recipe.yaml --wait --timeout 180s
	@echo "==> Deploying Notification..."
	helm upgrade --install notification $(CHART) \
		--namespace $(NAMESPACE) -f values/notification.yaml --wait --timeout 180s
	@echo "==> Deploying Audit..."
	helm upgrade --install audit $(CHART) \
		--namespace $(NAMESPACE) -f values/audit.yaml --wait --timeout 180s
	@echo "==> Deploying API Gateway..."
	helm upgrade --install api-gateway $(CHART) \
		--namespace $(NAMESPACE) -f values/api-gateway.yaml --wait --timeout 180s
	@echo "==> Deploying Web..."
	helm upgrade --install web $(CHART) \
		--namespace $(NAMESPACE) -f values/web.yaml --wait --timeout 180s

.PHONY: deploy-%
deploy-%: ## Deploy a single service (e.g., make deploy-identity)
	helm upgrade --install $* $(CHART) \
		--namespace $(NAMESPACE) -f values/$*.yaml --wait --timeout 180s

.PHONY: undeploy-services
undeploy-services: ## Undeploy all application services
	@for comp in web api-gateway $(filter-out api-gateway web,$(ALL_COMPONENTS)); do \
		helm uninstall $${comp} --namespace $(NAMESPACE) 2>/dev/null || true; \
	done

# ──────────────────────────────────────────────
# Full Lifecycle
# ──────────────────────────────────────────────

.PHONY: up
up: cluster-create calico-install ingress-install images secrets infra-install observability-install deploy-services ## Full deploy: cluster + calico + images + secrets + infra + observability + apps
	@echo ""
	@echo "============================================="
	@echo " PantryCloud is running!"
	@echo " Add to /etc/hosts:  127.0.0.1 pantry.test"
	@echo " Open: http://pantry.test"
	@echo "============================================="

.PHONY: down
down: cluster-delete ## Tear down everything

.PHONY: redeploy
redeploy: images deploy-services ## Re-pull/rebuild images and redeploy services

.PHONY: status
status: ## Show cluster status
	@echo "==> Pods:"
	@kubectl get pods -n $(NAMESPACE) -o wide
	@echo ""
	@echo "==> Services:"
	@kubectl get svc -n $(NAMESPACE)
	@echo ""
	@echo "==> Ingress:"
	@kubectl get ingress -n $(NAMESPACE)

define deployment_name
$(if $(filter api-gateway web,$(1)),$(1),$(1)-api)
endef
.PHONY: logs-%
logs-%: ## Tail logs for a service (e.g., make logs-identity, make logs-api-gateway, make logs-web)
	kubectl logs -f deployment/$(call deployment_name,$*) -n $(NAMESPACE)
