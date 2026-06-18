#!/usr/bin/env bash
set -euo pipefail

REPO_URL="${REPO_URL:-git@github.com:Alexandra-MartinezOrtiz/tesis_tic.git}"
BRANCH="${BRANCH:-main}"
APP_DIR="${APP_DIR:-$HOME/proyecto/tesis_tic}"

mkdir -p "$(dirname "$APP_DIR")"

if [ ! -d "$APP_DIR/.git" ]; then
  git clone --branch "$BRANCH" "$REPO_URL" "$APP_DIR"
fi

cd "$APP_DIR"
git fetch origin "$BRANCH"
git checkout "$BRANCH"
git pull --ff-only origin "$BRANCH"

docker compose down
docker compose up -d --build
docker compose ps
