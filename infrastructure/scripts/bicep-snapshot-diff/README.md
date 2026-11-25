# Bicep snapshot diff

Compares the resources that two versions of the infrastructure templates would deploy, for every template under
`infrastructure/templates`. Use it to check that a change to a shared component or a refactoring only changes what you
expect, in every template that uses it.

It runs entirely locally, using [`az bicep snapshot`](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/bicep-cli#snapshot),
so it doesn't need an Azure login or access to any environment.

## Requirements

- The Azure CLI, with a version of Bicep that supports `az bicep snapshot` (`az bicep upgrade`).
- Python 3.
- Git.

## Usage

```bash
infrastructure/scripts/bicep-snapshot-diff/snapshot.sh <base ref> <head ref | WORKTREE> <output directory> [environment] [variant]
```

For example, to compare your uncommitted changes with `dev`, for the dev environment's ELZ param files:

```bash
infrastructure/scripts/bicep-snapshot-diff/snapshot.sh origin/dev WORKTREE ~/snapshots dev elz
```

- **base ref** and **head ref** are any git refs. Use `WORKTREE` as the head to include uncommitted changes and
  untracked files.
- **output directory** must be outside the repo, as `az bicep snapshot` writes its output beside each param file. The
  script exports each version of the `infrastructure` folder there, and leaves the snapshots there afterwards.
- **environment** is `dev` (the default), `test`, `preprod` or `prod`.
- **variant** is `legacy` (the default) or `elz`.

A full run takes around 3 minutes.

## Reading the output

For each template, it lists the resources added, removed and changed, and any changed outputs. For each changed
property, it shows a word-level diff, as ARM expressions are often a single very long line.

A changed resource marked **(same literals)** contains exactly the same strings and function names in both versions,
just in a different order or nesting. That's usually harmless, for example `union()` arguments being reordered, but
check the diff to be sure.

The snapshots show what the templates would deploy, not what's already in Azure. They can't show the values of things
that are only known at deploy time, such as `reference()` and `listKeys()` calls, so these appear as expressions.

## Parameters that the pipelines supply

Some parameters are passed in by the pipelines rather than set in the param files. `snapshot.sh` adds placeholder values
for these. If a snapshot fails with a missing parameter, add a placeholder for it in `snapshot.sh`.
