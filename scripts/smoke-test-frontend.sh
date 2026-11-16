#!/usr/bin/env bash
set -euo pipefail

: "${FRONTEND_URL:?Falta la variable FRONTEND_URL.}"
FRONTEND_URL="${FRONTEND_URL%/}"

for path in / /ventas; do
  body=$(curl -fsS --max-time 30 "$FRONTEND_URL$path") || { echo "GET $path falló." >&2; exit 1; }
  echo "$body" | grep -q 'id="root"' || { echo "GET $path no devolvió la aplicación." >&2; exit 1; }
  echo "GET $path correcto."
done

echo "Smoke test del frontend correcto."
