#!/usr/bin/env bash
# Copies the latest CI verification output (results branches) into 02-maui/results/.
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"
git fetch -q origin maui-ci-android maui-ci-windows
R=02-maui/results
rm -rf "$R/screenshots/android" "$R/screenshots/windows" "$R/ci"
mkdir -p "$R/screenshots/android" "$R/screenshots/windows" "$R/ci/android" "$R/ci/windows"
for b in android windows; do
  tmp=$(mktemp -d)
  git archive "origin/maui-ci-$b" | tar -x -C "$tmp"
  cp "$tmp"/*.txt "$tmp"/*.json "$R/ci/$b/" 2>/dev/null || true
  [ -d "$tmp/ui-dumps" ] && cp -r "$tmp/ui-dumps" "$R/ci/$b/"
  if [ "$b" = android ]; then
    for f in "$tmp"/screenshots/*.png; do
      n=$(basename "$f" .png)
      case "$n" in
        *-412x915) cp "$f" "$R/screenshots/android/${n%-412x915}.png" ;;
        *) cp "$f" "$R/screenshots/android/${n}-1080x2400.png" ;;
      esac
    done
  else
    cp "$tmp"/screenshots/*.png "$R/screenshots/windows/"
  fi
  rm -rf "$tmp"
done
ls "$R/screenshots/android" "$R/screenshots/windows" | head -100
