#!/usr/bin/env bash
# Creates a new VMSS-based Azure Pipelines agent scale set, booting from a
# vanilla Canonical Ubuntu marketplace image instead of a custom-built one,
# with encryption-at-host enabled from creation (this can only be set when a
# scale set/instance is first created, not added to an existing one).
#
# This replaces the manually-created ees-ubuntu2204-large scale set. Values
# below were read directly from that scale set's current configuration on
# 2026-09-16, so the new one matches it in every way except the image and
# encryption-at-host.
#
# Prerequisites (one-time, per subscription):
#   az feature register --namespace Microsoft.Compute --name EncryptionAtHost
#   az feature show --namespace Microsoft.Compute --name EncryptionAtHost --query properties.state
#   (wait until the above says "Registered", then:)
#   az provider register --namespace Microsoft.Compute
#
# Before running this for real, verify IMAGE_URN's sku against the live list:
#   az vm image list --all --publisher Canonical --offer ubuntu-26_04-lts -o table
#
# Usage:
#   bash infrastructure/scripts/create-vmss-agent-pool.sh
set -euxo pipefail

# On Git Bash/MSYS2 (Windows), arguments starting with "/" - like the
# subnet resource ID below - get silently mangled into Windows-style paths
# before az ever sees them, causing a confusing "incorrect usage" error.
# This disables that auto-conversion. Harmless/unused on real Bash (Cloud
# Shell, Linux, macOS).
export MSYS_NO_PATHCONV=1

RESOURCE_GROUP="S101D01-RG-EES"
SCALE_SET_NAME="ees-ubuntu2604-large-v2"
LOCATION="westeurope"   # confirmed from the resource group's "West Europe" location in the portal

# Same VM size and initial capacity as the current ees-ubuntu2204-large.
VM_SKU="Standard_D4ds_v5"
INSTANCE_COUNT=2   # Azure Pipelines will take over managing this count once the pool is registered

# Canonical:offer:sku:version - verify the sku before running (see comment above).
IMAGE_URN="Canonical:ubuntu-26_04-lts:server:latest"

# Same OS disk settings as today: 86GB, Standard_LRS, ephemeral (local/resource
# disk, read-only caching) - Standard_LRS is required for ephemeral OS disks.
OS_DISK_SIZE_GB=86
STORAGE_SKU="Standard_LRS"

# Same subnet the current scale set uses.
SUBNET_ID="/subscriptions/48ea0797-73c6-4202-bf90-b01c817058e9/resourceGroups/s101d01-rg-ees/providers/Microsoft.Network/virtualNetworks/s101d01-vnet-ees-runners/subnets/s101d01-snet-ees-runners-ubuntu2204"

az vmss create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$SCALE_SET_NAME" \
  --location "$LOCATION" \
  --image "$IMAGE_URN" \
  --vm-sku "$VM_SKU" \
  --instance-count "$INSTANCE_COUNT" \
  --storage-sku "$STORAGE_SKU" \
  --os-disk-size-gb "$OS_DISK_SIZE_GB" \
  --ephemeral-os-disk true \
  --os-disk-caching readonly \
  --encryption-at-host true \
  --subnet "$SUBNET_ID" \
  --admin-username azureuser \
  --authentication-type SSH \
  --generate-ssh-keys \
  --disable-overprovision \
  --upgrade-policy-mode manual \
  --single-placement-group false \
  --platform-fault-domain-count 1 \
  --load-balancer "" \
  --orchestration-mode Uniform

# Attach the bootstrap script (installs Docker, Chrome, git, and a couple of
# small utilities). Runs on every VM create/reimage, before the Azure
# Pipelines agent extension.
az vmss extension set \
  --resource-group "$RESOURCE_GROUP" \
  --vmss-name "$SCALE_SET_NAME" \
  --name CustomScript \
  --publisher Microsoft.Azure.Extensions \
  --version 2.1 \
  --protected-settings "{\"script\": \"$(base64 -w0 infrastructure/scripts/vmss-agent-bootstrap.sh)\"}"

echo "Scale set created. Next: register it as a new Azure DevOps agent pool"
echo "(Project settings > Pipelines > Agent pools > Add pool > Azure virtual machine scale set) - this part is a manual UI step, not scriptable."
