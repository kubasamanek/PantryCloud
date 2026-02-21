#!/bin/sh
set -e

SETTINGS_FILE="/usr/share/nginx/html/appsettings.json"

if [ -n "$GATEWAY_BASE_URL" ]; then
  sed -i "s|http://localhost:5050|${GATEWAY_BASE_URL}|g" "$SETTINGS_FILE"
fi

exec "$@"
