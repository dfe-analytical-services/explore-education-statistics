# Ubuntu setup playbook

An Ansible playbook that takes a freshly formatted Ubuntu machine and leaves it
able to build, run and test the Explore Education Statistics service.

It automates the manual steps in the ["Getting started"](../README.md#getting-started)
section of the main README. That section remains the reference for *what* the
setup is and why; this playbook is just a repeatable way of applying it.

Tested against Ubuntu 22.04 and 24.04. It will run on other releases but warns
first, because the third party apt repositories and a couple of package names
vary between them.

## Usage

Clone the repository, then run the bootstrap script from the checkout. It
installs Ansible if it is missing and then runs the playbook:

```bash
./ansible/bootstrap.sh
```

If you already have Ansible, run the playbook directly. It must be run from this
directory so that `ansible.cfg` and `inventory.ini` are picked up:

```bash
cd ansible && ansible-playbook setup.yml
```

Run it as the user who will be developing on the machine, **not** as root or
with `sudo`. The toolchains are installed into that user's home directory and
that user is the one added to the `docker` group. Individual tasks escalate with
sudo where they need to, and you will be prompted for your password once.

The playbook is idempotent - re-running it is safe, and is the way to pick the
new versions up after `.nvmrc`, `.python-version` or `global.json` change.

### Useful variables

Everything below can be overridden with `-e name=value`:

```bash
cd ansible && ansible-playbook setup.yml \
  -e ees_github_username=your-github-username \
  -e ees_github_pat=ghp_xxx \
  -e ees_bootstrap_bare_database=true
```

| Variable | Default | Purpose |
| --- | --- | --- |
| `ees_project_dir` | the checkout containing this directory | Where the EES source lives. |
| `ees_github_username` / `ees_github_pat` | empty | Adds the GitHub Packages NuGet source. See below. |
| `ees_bootstrap_bare_database` | `false` | Create empty databases with the logins the service needs, instead of using a pre-built development database. |
| `ees_install_chrome` | `true` | Install Google Chrome for the Robot Framework UI tests. |
| `ees_pull_docker_images` | `true` | Pull the container images up front rather than on first use. |
| `ees_nvm_dir` | `~/.config/nvm` | Where nvm is installed. |
| `ees_pyenv_root` | `~/.pyenv` | Where pyenv is installed. |
| `ees_dotnet_root` | `~/.dotnet` | Where the .NET SDK is installed. |

See [`group_vars/all.yml`](group_vars/all.yml) for the full set.

## Existing tools are used, not replaced

The playbook does not assume a bare machine. Before installing anything it looks
for what is already there, and uses it:

| Tool | Where it looks | If found |
| --- | --- | --- |
| nvm | `$NVM_DIR`, `~/.nvm`, `~/.config/nvm` | Used as it is. Not moved, and not checked out at `ees_nvm_version`. |
| pyenv | `$PYENV_ROOT`, `~/.pyenv`, then `pyenv root` from the `PATH` | Used as it is. Not moved, and not checked out at `ees_pyenv_version`. |
| .NET SDK | `$DOTNET_ROOT`, `~/.dotnet`, `/usr/share/dotnet`, `/usr/lib/dotnet` | Used if `dotnet --version` succeeds inside the checkout, i.e. if it already satisfies `global.json` including its `rollForward` policy. |
| Docker | `docker --version` and `docker compose version` | Docker's apt repository is not added at all, so an install from the convenience script or from `docker.io` is left intact. |
| Chrome | `google-chrome` / `google-chrome-stable` on the `PATH` | Nothing installed. |
| pnpm | `pnpm --version` inside the checkout | corepack is not touched if the version already matches `packageManager`. |
| Azure Functions Core Tools | `func --version` | Nothing installed if it is already on v4. |
| Node, Python | the pinned version's directory under nvm/pyenv | Nothing built. |

The first run prints what it found and what it is going to install, before it
installs anything.

Two things it deliberately leaves alone when it finds an existing installation:

- **Your nvm default.** It installs the Node version `.nvmrc` asks for but does
  not repoint `nvm alias default`, and tells you so. Run `nvm use` in the
  checkout, or set the default yourself.
- **Your pyenv global.** Same reasoning, and it costs nothing because the
  committed `.python-version` already makes pyenv select the right interpreter
  inside the checkout.

On a machine the playbook set up itself, it does set both, because there is no
existing preference to respect.

Passing a location explicitly overrides discovery entirely:

```bash
cd ansible && ansible-playbook setup.yml -e ees_nvm_dir=/opt/nvm
```

## What it does

### System

- Installs build tooling, the libraries pyenv needs to compile CPython, and the
  shared libraries the Robot suite's Python packages link against.
- Adds `db`, `data-storage` and `ees.local` to `/etc/hosts`, pointing at
  `127.0.0.1`.
- Raises `fs.inotify.max_user_watches` and `fs.inotify.max_user_instances` via
  `/etc/sysctl.d`, which the .NET integration tests need.
- Installs Docker Engine and the Compose plugin from Docker's own apt
  repository, and adds you to the `docker` group.
- Installs Google Chrome, which the Robot Framework UI tests drive.

### Toolchains

Node, Python and the .NET SDK are all pinned by files in the repo. The playbook
reads those files at run time rather than repeating the versions, so it cannot
drift from the project:

| Tool | Pinned by | Installed with |
| --- | --- | --- |
| Node | `.nvmrc` | nvm |
| pnpm | the `packageManager` field in `package.json` | corepack |
| Python | `.python-version` | pyenv |
| .NET SDK | `global.json` | Microsoft's `dotnet-install.sh` |

When the playbook has to install them, all three go into your home directory
rather than being installed system wide, so nothing outside `$HOME` needs
unpicking to remove them. The Azure Functions Core Tools are installed globally
through npm.

An existing pyenv that predates the pinned Python release will not have a build
recipe for it. The playbook checks for this up front and fails with instructions
rather than letting it surface as a confusing error part way through the build.

### Shell

The playbook writes `~/.ees-env.sh`, which sets up nvm, pyenv and the .NET
paths, and adds a single marked block to `~/.bashrc` that sources it. That file
also defines the `ees` helper function from the main README, so you can run
`ees content data` from anywhere.

Everything the playbook adds to your shell is in that one file plus that one
block, so it is straightforward to see and to undo.

### Project

- `pnpm install --frozen-lockfile` for the workspace dependencies.
- `dotnet tool restore` for the local .NET tools (`dotnet-ef`, `csharpier`, the
  Swashbuckle CLI).
- `pipenv install --dev` for the Robot Framework suite and linkchecker.
- `dotnet dev-certs https --trust` so the admin frontend's HTTPS certificate is
  trusted. This one is best effort - if it fails the setup carries on and warns.

## What it deliberately does not do

### The database

The recommended setup uses a pre-built development database that lives in Google
Drive behind team access, so the playbook cannot fetch it. Once you have it:

```bash
ln -s /path/to/unencrypted/ees-mssql /path/to/ees/data/ees-mssql
sudo chown -R 10001 /path/to/unencrypted/ees-mssql
```

The directory has to sit on an unencrypted filesystem, and be owned by uid
`10001` because that is what the SQL Server container runs as. If
`data/ees-mssql` already exists when the playbook runs, it fixes the ownership
for you.

Alternatively, `-e ees_bootstrap_bare_database=true` starts the SQL Server
container against empty databases and creates the per-component logins and
contained users described by ["Option 2 - Use a bare
database"](../README.md#option-2---use-a-bare-database). The SQL it runs is
guarded so it is safe to re-run. Override `ees_sql_login_password` if you do not
want the README's example password.

### The GitHub Packages NuGet source

Some .NET dependencies are hosted on GitHub Packages and need a classic personal
access token with the `read:packages` scope to restore. The playbook will
configure the source if you pass `ees_github_username` and `ees_github_pat`,
and otherwise tells you it has skipped it.

The token is written to your **user** NuGet config (`~/.nuget/NuGet/NuGet.Config`,
created `0600`), never to the repo's source controlled `NuGet.Config`. Because
NuGet can only encrypt stored passwords on Windows, it is stored in clear text
there.

### Anything not on a clean machine

The playbook only configures the machine and the checkout. It does not clone the
repository - it lives inside it - and it does not start any of the applications.

## After it finishes

Open a new shell, or `source ~/.bashrc`, to pick the toolchains up. Log out and
back in - or `newgrp docker` - for the `docker` group to take effect. Then:

```bash
pnpm start admin
```

## Requirements

`ansible-core` only. The playbook uses nothing outside `ansible.builtin`, so
there are no collections to install and `apt install ansible-core` is enough.
