#/usr/bin/env sh

set -u

# This script should be run from inside the oasdiff Docker container.
# It checks for any undocumented changes in the public API.

has_undocumented_changes=false

for file in /api-docs/source/openapi-v*.json; do
  filename=$(basename $file)

  echo "Checking $filename"
  oasdiff changelog --flatten-allof -o INFO $file /api/wwwroot/$filename

  if [ $? != 0 ]; then
    has_undocumented_changes=true

    cat <<- EOF
ERROR: There were changes that must be documented in the public API docs.

To document these changes, you must update 'src/explore-education-statistics-api-docs/source/$filename'
by copying the changes from the OpenAPI endpoint when running the public API locally.

The OpenAPI endpoint locally is: http://localhost:5050/$filename

You should ensure that these changes are expected. If there are error level changes reported above, this
means there are breaking changes that are being introduced. If these changes are intentional, you should:

  1. Create a new major API version in the public API project.
  2. Create a new 'openapi-v*.json' for the major version in the public API docs.
  3. Document the major version and its breaking changes in the public API docs (e.g. in a changelog).
  4. Review any existing API documentation and update accordingly.
  5. Deprecate the previous API version when appropriate.

Consult the following DevOps wiki page for more information:
https://dfe-gov-uk.visualstudio.com.mcas.ms/s101-Explore-Education-Statistics/_wiki/wikis/s101-Explore-Education-Statistics.wiki/9070
EOF
  fi
done || true

if $has_undocumented_changes; then
  exit 1
fi

has_undocumented_version=false

# Check there isn't an undocumented API version
for file in /api/wwwroot/openapi-v*.json; do
  filename=$(basename $file)

  if [ ! -f "/api-docs/source/$filename" ]; then
    has_undocumented_version=true
    echo "ERROR: $filename is required in the public API docs"
  fi
done

if $has_undocumented_version; then
  exit 1
fi
