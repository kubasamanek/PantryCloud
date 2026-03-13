#!/usr/bin/env bash
set -euo pipefail

# Build and push multi-architecture Docker images for PantryCloud services.
# Targets linux/amd64 and linux/arm64 so images work on both x86_64 and Apple Silicon.

TAG="${1:-}"

if [[ -z "${TAG}" ]]; then
  echo "Usage: $0 <tag>"
  echo "Example: $0 latest"
  exit 1
fi

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"

IMAGES=(
  "identityservice:src/Services/PantryCloud.Identity/PantryCloud.Identity.Presentation/Dockerfile"
  "householdservice:src/Services/PantryCloud.Household/PantryCloud.Household.Presentation/Dockerfile"
  "pantryservice:src/Services/PantryCloud.Pantry/PantryCloud.Pantry.Presentation/Dockerfile"
  "recipeservice:src/Services/PantryCloud.Recipe/PantryCloud.Recipe.Presentation/Dockerfile"
  "shoppinglistservice:src/Services/PantryCloud.ShoppingList/PantryCloud.ShoppingList.Presentation/Dockerfile"
  "notificationservice:src/Services/PantryCloud.Notification/PantryCloud.Notification.Presentation/Dockerfile"
  "auditservice:src/Services/PantryCloud.Audit/PantryCloud.Audit.Presentation/Dockerfile"
  "apigateway:src/Gateways/PantryCloud.ApiGateway/PantryCloud.ApiGateway.Presentation/Dockerfile"
  "pantrycloud-web:src/Web/PantryCloud.Web/Dockerfile"
)

BUILDX_BUILDER_NAME="pantrycloud-multiarch-builder"

if ! docker buildx inspect "${BUILDX_BUILDER_NAME}" >/dev/null 2>&1; then
  docker buildx create --name "${BUILDX_BUILDER_NAME}" --use
else
  docker buildx use "${BUILDX_BUILDER_NAME}"
fi

for item in "${IMAGES[@]}"; do
  NAME="${item%%:*}"
  DOCKERFILE_REL="${item#*:}"

  IMAGE="samanekj/${NAME}:${TAG}"
  DOCKERFILE="${REPO_ROOT}/${DOCKERFILE_REL}"

  echo "==> Building and pushing multi-arch image ${IMAGE}"

  docker buildx build \
    --platform linux/amd64,linux/arm64 \
    -t "${IMAGE}" \
    -f "${DOCKERFILE}" \
    "${REPO_ROOT}" \
    --push
done

echo "==> Done. Pushed multi-arch images for tag '${TAG}'."

