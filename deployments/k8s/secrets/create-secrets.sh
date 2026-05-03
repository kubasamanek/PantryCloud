#!/usr/bin/env bash
set -euo pipefail

NAMESPACE="${1:-pantry}"

# ---------------------------------------------------------------------------
# Infrastructure credentials - override via environment variables for
# non-default setup. The defaults here are for local Kind cluster.
# ---------------------------------------------------------------------------
PG_USER="${POSTGRES_USER:-admin}"
PG_PASS="${POSTGRES_PASSWORD:-password}"
PG_HOST="${POSTGRES_HOST:-pantry-postgresql}"
PG_PORT="${POSTGRES_PORT:-5432}"

RMQ_USER="${RABBITMQ_USER:-admin}"
RMQ_PASS="${RABBITMQ_PASSWORD:-password}"

MONGO_USER="${MONGODB_ROOT_USER:-admin}"
MONGO_PASS="${MONGODB_ROOT_PASSWORD:-password}"
MONGO_HOST="${MONGODB_HOST:-pantry-mongodb}"
MONGO_PORT="${MONGODB_PORT:-27017}"

REDIS_HOST="${REDIS_HOST:-pantry-redis-master}"
REDIS_PORT="${REDIS_PORT:-6379}"

pg_conn() {
  local db="$1"
  echo "Host=${PG_HOST};Port=${PG_PORT};Database=${db};Username=${PG_USER};Password=${PG_PASS};"
}

# --- Namespace -------------------------------------------------------------
echo "==> Creating namespace ${NAMESPACE} (if not exists)..."
kubectl create namespace "${NAMESPACE}" --dry-run=client -o yaml | kubectl apply -f -

# --- JWT signing keys ------------------------------------------------------
echo "==> Generating RSA keys for JWT signing..."
TEMP_DIR=$(mktemp -d)
trap "rm -rf ${TEMP_DIR}" EXIT

openssl genrsa -out "${TEMP_DIR}/private.pem" 2048 2>/dev/null
openssl rsa -in "${TEMP_DIR}/private.pem" -pubout -out "${TEMP_DIR}/public.pem" 2>/dev/null

echo "==> Creating jwt-signing-keys secret..."
kubectl create secret generic jwt-signing-keys \
  --from-file=private.pem="${TEMP_DIR}/private.pem" \
  --from-file=public.pem="${TEMP_DIR}/public.pem" \
  --namespace "${NAMESPACE}" \
  --dry-run=client -o yaml | kubectl apply -f -

# --- Infrastructure credentials (referenced by StatefulSets) ---------------
echo "==> Creating pantry-infra-credentials secret..."
kubectl create secret generic pantry-infra-credentials \
  --from-literal=POSTGRES_USER="${PG_USER}" \
  --from-literal=POSTGRES_PASSWORD="${PG_PASS}" \
  --from-literal=RABBITMQ_DEFAULT_USER="${RMQ_USER}" \
  --from-literal=RABBITMQ_DEFAULT_PASS="${RMQ_PASS}" \
  --from-literal=MONGO_INITDB_ROOT_USERNAME="${MONGO_USER}" \
  --from-literal=MONGO_INITDB_ROOT_PASSWORD="${MONGO_PASS}" \
  --namespace "${NAMESPACE}" \
  --dry-run=client -o yaml | kubectl apply -f -

# --- Per-service secrets ---------------------------------------------------
echo "==> Creating identity-api-secrets..."
kubectl create secret generic identity-api-secrets \
  --from-literal=ConnectionStrings__DefaultConnection="$(pg_conn identity_db)" \
  --from-literal=Email__Host="" \
  --from-literal=Email__Port="2525" \
  --from-literal=Email__Username="" \
  --from-literal=Email__Password="" \
  --from-literal=Email__From="admin@pantrycloud.dev" \
  --namespace "${NAMESPACE}" \
  --dry-run=client -o yaml | kubectl apply -f -

for svc_db in "household:household_db" "pantry:pantry_db" "shoppinglist:shoppinglist_db" "audit:audit_db"; do
  svc="${svc_db%%:*}"
  db="${svc_db##*:}"
  echo "==> Creating ${svc}-api-secrets..."
  kubectl create secret generic "${svc}-api-secrets" \
    --from-literal=ConnectionStrings__DefaultConnection="$(pg_conn "${db}")" \
    --from-literal=Messaging__RabbitMQ__Username="${RMQ_USER}" \
    --from-literal=Messaging__RabbitMQ__Password="${RMQ_PASS}" \
    --namespace "${NAMESPACE}" \
    --dry-run=client -o yaml | kubectl apply -f -
done

echo "==> Creating notification-api-secrets..."
kubectl create secret generic notification-api-secrets \
  --from-literal=ConnectionStrings__DefaultConnection="$(pg_conn notification_db)" \
  --from-literal=Messaging__RabbitMQ__Username="${RMQ_USER}" \
  --from-literal=Messaging__RabbitMQ__Password="${RMQ_PASS}" \
  --from-literal=ConnectionStrings__Redis="${REDIS_HOST}:${REDIS_PORT}" \
  --namespace "${NAMESPACE}" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "==> Creating recipe-api-secrets..."
kubectl create secret generic recipe-api-secrets \
  --from-literal=ConnectionStrings__DefaultConnection="mongodb://${MONGO_USER}:${MONGO_PASS}@${MONGO_HOST}:${MONGO_PORT}" \
  --namespace "${NAMESPACE}" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "==> All secrets created in namespace ${NAMESPACE}."
