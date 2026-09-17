#!/usr/bin/env bash
# Bootstraps the ees-ubuntu2204-* pipeline agent scale set VMs from a vanilla
# Canonical Ubuntu 22.04 marketplace image, replacing the custom-built
# ees-ubuntu-2204 image.
#
# Deployed to the scale set as a Custom Script Extension (see
# https://learn.microsoft.com/en-us/azure/devops/pipelines/agents/scale-set-agents#customizing-virtual-machine-startup-via-the-custom-script-extension):
#
#   az vmss extension set \
#     --resource-group <resource-group> \
#     --vmss-name <scale-set-name> \
#     --name CustomScript \
#     --publisher Microsoft.Azure.Extensions \
#     --version 2.1 \
#     --protected-settings "{\"script\": \"$(base64 -w0 infrastructure/scripts/vmss-agent-bootstrap.sh)\"}"
#
# The extension runs on every VM create/reimage, before the Azure Pipelines
# agent extension. It must exit 0, or Azure DevOps deletes the VM as
# unhealthy.
#
# Docker and Chrome are installed from their own vendor apt repos (not
# Ubuntu's docker.io/chromium-browser packages) so that every fresh/reimaged
# VM picks up current versions, rather than whatever Ubuntu 22.04 happened to
# ship - the whole point of moving off a frozen custom image.
#
# Everything else the pipelines need (.NET, Python, Node) is installed at job
# time by the pipelines themselves, so keep this script to the bare minimum.
# libicu is the exception - it's a native OS dependency the .NET runtime
# needs at run time (not something UseDotNet@2 installs). The exact package
# name embeds an ABI version that changes every Ubuntu release (libicu70 on
# 22.04, libicu74 on 24.04, etc), so it's resolved dynamically below rather
# than hardcoded, to avoid silently breaking on the next OS version bump.
set -euxo pipefail
export DEBIAN_FRONTEND=noninteractive

# cloud-init holds the apt locks while it finishes provisioning the VM.
cloud-init status --wait || true

apt-get update
apt-get install -y --no-install-recommends \
  ca-certificates \
  curl \
  gnupg

install -m 0755 -d /etc/apt/keyrings

# Docker's official repo, so Engine stays current independent of Ubuntu's own
# package cadence.
curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
  | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" \
  > /etc/apt/sources.list.d/docker.list

# Google's official repo, for the same reason - and because Chrome isn't in
# Ubuntu's default repos at all.
curl -fsSL https://dl.google.com/linux/linux_signing_key.pub \
  | gpg --dearmor -o /etc/apt/keyrings/google-chrome.gpg
echo "deb [arch=amd64 signed-by=/etc/apt/keyrings/google-chrome.gpg] http://dl.google.com/linux/chrome/deb/ stable main" \
  > /etc/apt/sources.list.d/google-chrome.list

apt-get update

libicu_package="$(apt-cache pkgnames libicu | grep -E '^libicu[0-9]+$' | sort -V | tail -1)"
if [ -z "$libicu_package" ]; then
  echo "Could not resolve a libicu package for this Ubuntu release - check .NET's Linux runtime dependencies for the current package name." >&2
  exit 1
fi

apt-get install -y --no-install-recommends \
  git \
  zip \
  unzip \
  jq \
  "$libicu_package" \
  docker-ce \
  docker-ce-cli \
  containerd.io \
  docker-buildx-plugin \
  docker-compose-plugin \
  google-chrome-stable

systemctl enable --now docker

# The Azure Pipelines agent extension creates the AzDevOps user if it doesn't
# already exist. Create it here so it can be put in the docker group before
# the agent starts. Don't make it the primary admin or rely on its password -
# the agent extension changes the password during agent configuration.
id AzDevOps &> /dev/null || useradd --create-home --shell /bin/bash AzDevOps
usermod -aG docker AzDevOps
