#!/usr/bin/env bash
#
# Snapshots every Bicep template under infrastructure/templates for two versions of the repo, then compares the
# resources that each would deploy. Runs entirely locally with "az bicep snapshot" - no Azure login is needed.
#
# Usage: snapshot.sh <base ref> <head ref | WORKTREE> <output directory> [environment] [variant]
#
#   base ref / head ref  Any git ref. Use WORKTREE as the head to include uncommitted changes.
#   output directory     Where the exported trees and snapshots go. Must be outside the repo, because
#                        "az bicep snapshot" writes its output beside each input file.
#   environment          dev (default), test, preprod or prod.
#   variant              legacy (default) or elz.
#
set -euo pipefail

if [ $# -lt 3 ]; then
  sed -n '2,15p' "$0"
  exit 1
fi

baseRef=$1
headRef=$2
outDir=$3
environment=${4:-dev}
variant=${5:-legacy}

scriptDir=$(cd "$(dirname "$0")" && pwd)
repoRoot=$(git -C "$scriptDir" rev-parse --show-toplevel)

mkdir -p "$outDir"
outDir=$(cd "$outDir" && pwd)
case "$outDir/" in
  "$repoRoot"/*)
    echo "The output directory must be outside the repo ($repoRoot)." >&2
    exit 1
    ;;
esac

if [ "$variant" == "elz" ]; then
  paramSubDir=parameters/elz
else
  paramSubDir=parameters
fi

# Exports the infrastructure folder of a ref (or of the working tree, including untracked files) into a directory.
export_tree() {
  local ref=$1
  local dest=$2
  rm -rf "$dest"
  mkdir -p "$dest"
  if [ "$ref" == "WORKTREE" ]; then
    (cd "$repoRoot" && git ls-files --cached --others --exclude-standard -z infrastructure \
      | tar --null --files-from=- --create --file=-) | tar --extract --file=- --directory="$dest"
  else
    git -C "$repoRoot" archive "$ref" infrastructure | tar --extract --file=- --directory="$dest"
  fi
}

# Copies a param file to snapshot-input.bicepparam beside it, adds placeholder values for the parameters that the
# pipelines pass on the command line, and snapshots it. Placeholders already set in the file are left alone.
snapshot_param_file() {
  local source=$1
  shift
  local input
  input="$(dirname "$source")/snapshot-input.bicepparam"
  cp "$source" "$input"

  local placeholder name
  for placeholder in "$@"; do
    name=$(echo "$placeholder" | awk '{print $2}')
    if ! grep -qE "^param $name( |=)" "$source"; then
      echo "$placeholder" >> "$input"
    fi
  done

  echo "  ${source#"$treeRoot"/}"
  az bicep snapshot --file "$input" --only-show-errors 2>&1 | grep -v '^WARNING' | cut -c1-300 | sed 's/^/    /' || true
}

# Placeholders for parameters that the pipelines pass in rather than setting in the param files. If a snapshot fails
# with a missing parameter, add a placeholder for it here.
mainInfrastructurePlaceholders=(
  "param environmentPipelineVariables = { maintenanceIpRanges: [{ name: 'maintenance', cidr: '10.0.0.1/32' }] }"
  "param adminProdAppSettings = { ExistingSetting: 'prod' }"
  "param adminStagingSlotAppSettings = { ExistingSetting: 'staging' }"
  "param publisherProdAppSettings = { ExistingSetting: 'prod' }"
  "param importerProdAppSettings = { ExistingSetting: 'prod' }"
  "param notifierProdAppSettings = { ExistingSetting: 'prod' }"
  "param dateProvisioned = '2026-01-01'"
)

appSettingsPlaceholders=(
  "param environmentIdentifier = 's101d01'"
  "param deployedAt = '2026-01-01T00:00:00Z'"
)

snapshot_tree() {
  treeRoot=$1
  local templates=$treeRoot/infrastructure/templates
  local template file appDir

  for template in core public-api screener search analytics data-factory-pipelines; do
    file=$templates/$template/$paramSubDir/main-$environment.bicepparam
    [ -f "$file" ] && snapshot_param_file "$file"
  done

  file=$templates/bicep-main-infrastructure-release/$paramSubDir/main-$environment.bicepparam
  [ -f "$file" ] && snapshot_param_file "$file" "${mainInfrastructurePlaceholders[@]}"

  for appDir in "$templates"/main-app-release/appsettings/*/; do
    file=$appDir$paramSubDir/appsettings-$environment.bicepparam
    [ -f "$file" ] || continue
    local extra=()
    if grep -qE "^param basicAuthEnabled " "$appDir"*-appsettings.bicep; then
      extra=("param basicAuthEnabled = false")
    fi
    snapshot_param_file "$file" "${appSettingsPlaceholders[@]}" "${extra[@]}"
  done
}

for side in base head; do
  if [ "$side" == "base" ]; then ref=$baseRef; else ref=$headRef; fi
  echo "Snapshotting $side ($ref), $environment $variant:"
  export_tree "$ref" "$outDir/$side"
  snapshot_tree "$outDir/$side"
done

echo
python3 -I "$scriptDir/diff-snapshots.py" "$outDir/base" "$outDir/head"
echo
echo "Snapshots are in $outDir/base and $outDir/head."
