#!/usr/bin/env bash
set -euo pipefail

if [ -z "${DEPLOY_HOOK_URL:-}" ]; then
  echo "DEPLOY_HOOK_URL no está configurado: se asume que el hosting despliega solo la nueva imagen."
  exit 0
fi

status=$(curl -s -o /dev/null -w '%{http_code}' -X POST --max-time 60 "$DEPLOY_HOOK_URL")
if [[ "$status" != 2* ]]; then
  echo "El deploy hook respondió HTTP $status." >&2
  exit 1
fi

echo "Despliegue solicitado al hosting (HTTP $status)."
