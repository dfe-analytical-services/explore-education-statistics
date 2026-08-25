# Dashboard

As an alternative to the `start` script, a browser-based dashboard is available for starting/stopping services,
viewing logs, and managing test data and backups:

```bash
pnpm dashboard
```

This runs at `http://localhost:4300` by default (override with the `DASHBOARD_PORT` environment variable).

Everything the dashboard needs lives in this directory: it's a pnpm workspace package of its own, with its own
`package.json` and `tsconfig.json`, so its dependencies don't sit in the root manifest. `pnpm dashboard` at the
repo root is a passthrough to it; its other scripts are run with `--filter`, as below.

The dashboard has its own service definitions and dependency resolution, independent of the `start` script.
A command line equivalent of `start` using them is available too, which additionally starts any services a
requested service depends on (e.g. `admin` also starts `processor`/`publisher`) and resolves `PublicDataDbExists`
once across everything it's asked to start:

```bash
pnpm --filter ees-dashboard start:dashboard admin
```

The dependency resolution has tests, run with:

```bash
pnpm --filter ees-dashboard test:scripts
```

By default, the dashboard runs services from the same checkout it's started from. If you're using
[git worktrees](https://git-scm.com/docs/git-worktree) to work on multiple branches at once, you can instead point
it at services in a *different* checkout by setting `EES_PROJECT_ROOT`:

```bash
EES_PROJECT_ROOT=/path/to/other/checkout pnpm dashboard
```

This is useful, for example, if you're iterating on the dashboard itself on one branch, but want it to manage
services from a feature branch checked out elsewhere.

Note that `docker-compose.yml` pins the Compose project name (`name: explore-education-statistics`), so every
checkout shares one container stack regardless of its directory name. Pointing `EES_PROJECT_ROOT` at a different
checkout therefore manages the *same* containers, not a second set - but the bind-mounted paths
(`./data/ees-mssql` and friends) resolve relative to whichever checkout Compose is invoked from, so switching
between them will recreate those containers against the other checkout's data.
