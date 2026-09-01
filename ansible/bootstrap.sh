#!/usr/bin/env bash
#
# Installs Ansible on a freshly formatted Ubuntu machine and runs the EES setup
# playbook against it. Run this as the user who will be developing on the
# machine, not as root - it will prompt for a sudo password.
#
#   ./ansible/bootstrap.sh
#
# Any arguments are passed straight through to ansible-playbook, so the toggles
# documented in README.md work here too:
#
#   ./ansible/bootstrap.sh -e ees_bootstrap_bare_database=true
#
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ "$(id -u)" -eq 0 ]]; then
  echo "Run this as your own user, not as root. It will ask for sudo when needed." >&2
  exit 1
fi

if ! command -v ansible-playbook >/dev/null 2>&1; then
  echo "==> Installing ansible-core"
  sudo apt-get update
  sudo apt-get install -y ansible-core
fi

echo "==> Running the setup playbook"
cd "$script_dir"
# ansible.cfg already asks for the sudo password.
exec ansible-playbook setup.yml "$@"
