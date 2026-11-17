#!/usr/bin/env bash
set -euo pipefail

: "${API_URL:?Falta la variable API_URL.}"
: "${SMOKE_CUIT:?Falta la variable SMOKE_CUIT.}"
: "${SMOKE_PASSWORD:?Falta la variable SMOKE_PASSWORD.}"
API_URL="${API_URL%/}"

fail() {
  echo "$1" >&2
  exit 1
}

EXPECTED_VERSION="${EXPECTED_VERSION:-}"
MAX_ATTEMPTS="${MAX_ATTEMPTS:-60}"

ready=false
for attempt in $(seq 1 "$MAX_ATTEMPTS"); do
  body=$(curl -s --max-time 60 "$API_URL/health" || true)
  status=$(echo "$body" | jq -r '.status // empty' 2>/dev/null || true)
  version=$(echo "$body" | jq -r '.version // empty' 2>/dev/null || true)
  echo "Health intento $attempt: estado '${status:-sin respuesta}', versión '${version:-sin versión}'"
  if [ "$status" = "Healthy" ] && { [ -z "$EXPECTED_VERSION" ] || [ -z "$version" ] || [ "$version" = "$EXPECTED_VERSION" ]; }; then
    ready=true
    break
  fi
  sleep 15
done
[ "$ready" = true ] || fail "La API no respondió el health check con la versión esperada."

status=$(curl -s -o /dev/null -w '%{http_code}' "$API_URL/api/me")
[ "$status" = "401" ] || fail "GET /api/me sin token devolvió $status y se esperaba 401."

token=$(curl -fsS -X POST "$API_URL/api/authentication/authenticate" \
  -H 'Content-Type: application/json' \
  -d "{\"cuit\":\"$SMOKE_CUIT\",\"password\":\"$SMOKE_PASSWORD\"}")
token="${token//\"/}"
[ -n "$token" ] || fail "El login no devolvió un token."

status=$(curl -s -o /dev/null -w '%{http_code}' -H "Authorization: Bearer $token" "$API_URL/api/me")
[ "$status" = "200" ] || fail "GET /api/me con token devolvió $status y se esperaba 200."

if [ -n "${FRONTEND_URL:-}" ]; then
  origin="${FRONTEND_URL%/}"
  allowed=$(curl -s -o /dev/null -D - -X OPTIONS "$API_URL/api/me" \
    -H "Origin: $origin" -H 'Access-Control-Request-Method: GET' \
    | tr -d '\r' | awk -F': ' 'tolower($1)=="access-control-allow-origin"{print $2}')
  [ "$allowed" = "$origin" ] || fail "CORS no permite el origen $origin."
fi

echo "Smoke test de la API correcto."
