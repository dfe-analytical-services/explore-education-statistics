# EES agent scale set scripts

These scripts create and provision the Azure VM Scale Sets used as Azure
DevOps self-hosted build agents (`ees-ubuntu2604-large-v2`,
`ees-ubuntu2604-xlarge-v2`, in resource group `S101D01-RG-EES`). They
replace the previous approach of manually building a custom VM image with
Packer - these scale sets boot from a vanilla Canonical Ubuntu marketplace
image instead, with a small startup script installing the handful of things
the image doesn't already have.

## Files

- **`create-vmss-agent-pool.sh`** - creates a scale set with the right size,
  disk, networking, and encryption-at-host settings, and attaches the
  bootstrap script below to it. Run this to build a replacement for an
  existing pool (e.g. an OS version bump), or a new pool size variant. See
  the script's own header comment for exact usage, prerequisites, and env
  var overrides.
- **`vmss-agent-bootstrap.sh`** - runs once on every VM the scale set
  creates or reimages, before the Azure Pipelines agent starts. Installs
  Docker, Chrome, git, and a few small utilities.

## When to change `vmss-agent-bootstrap.sh`

Only add something here if one of these applies:

- **It genuinely can't be installed per-job.** Docker is the only current
  example - it's a background service the OS needs running before a job
  even starts, not something a pipeline step can reasonably set up.
- **It could be installed per-job, but there's no reason to.** Chrome is the
  current example: unlike .NET/Node/Python, nothing in this repo pins a
  specific Chrome version that has to be matched, so there's no benefit to
  fetching it fresh on every job the way those are. Baking it in (refreshed
  on every VM create/reimage, so it's still always current) gets the same
  result without paying an install cost on every job that lands on an
  already-warm agent.

**Don't add general dev tooling here just because you can.** .NET, Node,
and Python are deliberately installed fresh per job by the pipelines
instead (see `azure-pipelines-install-python.yml` for why) - specifically
because those *do* have a version pinned in the project's own config that
must be matched exactly, which something baked in at VM-creation time can't
guarantee stays in sync with.

## When to change `create-vmss-agent-pool.sh`

- **Different pool size**: just override `SCALE_SET_NAME`/`VM_SKU` when
  running it - no need to edit the file.
- **OS version bump** (e.g. the next Ubuntu LTS): update `IMAGE_URN`. Verify
  the exact marketplace SKU string against the live list first (see the
  script's header) - Canonical's naming isn't fully consistent between
  releases.
- **Disk size, subnet, or other settings shared by every pool**: these are
  defined once and used by both pool sizes - update them in one place here
  rather than duplicating per size.

## What's deliberately not automated here

- **Registering a created scale set as an Azure DevOps agent pool** is a
  manual step - Project settings > Pipelines > Agent pools > Add pool >
  Azure virtual machine scale set. Not scriptable via the Azure CLI.
- **Keeping Chrome and Docker current** needs no ongoing action - the
  bootstrap script always installs whatever's currently available from
  their own vendor repos, on every fresh/reimaged VM, rather than a version
  pinned at build time. Only the Ubuntu version itself (`IMAGE_URN`) is a
  deliberate, manually-made decision.
