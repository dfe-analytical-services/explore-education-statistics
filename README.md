# Explore Education Statistics service

[![Build Status](https://dfe-gov-uk.visualstudio.com/s101-Explore-Education-Statistics/_apis/build/status/Explore%20Education%20Statistics?branchName=master)](https://dfe-gov-uk.visualstudio.com/s101-Explore-Education-Statistics/_build/latest?definitionId=200&branchName=master)

## Project structure

The project is primarily composed of two areas:

### Public frontend (for general public users)

- **UI** - `src/explore-education-statistics-frontend`
  - NextJS React app
  - Depends on:
    - Content API
    - Data API
    - Notifier

- **Content API** - `src/GovUk.Education.ExploreEducationStatistics.Content.Api`
  - Depends on:
    - Publisher - to generate its cache

- **Data API** - `src/GovUk.Education.ExploreEducationStatistics.Data.Api`
  - Depends on:
    - SQLServer `statistics` database (known as `public-statistics` in non-local environments)

- **Notifier**
    - Azure function for adding users to GOV.UK Notify

### Admin (for admins and analysts)

- **UI** - `src/explore-education-statistics-admin`
  - CRA React app
  - Depends on:
    - Admin API

- **Admin API** - `src/GovUk.Education.ExploreEducationStatistics.Admin`
  - Depends on:
    - SQLServer `content` database
    - SQLServer `statistics` database
    - Publisher
    - Notifier
    - Data Processor

- **Publisher** - `src/GovUk.Education.ExploreEducationStatistics.Publisher`
  - Azure function for publishing admin content to the public frontend

- **Notifier** - `src/GovUk.Education.ExploreEducationStatistics.Notifier`
  - Azure function for sending notifications

- **Data Processor** - `src/GovUk.Education.ExploreEducationStatistics.Data.Processor`
  - Azure function for handling dataset imports into the admin. Also referred to as the 'importer' or just 'processor'.

## Getting started

### Requirements

You will need the following groups of dependencies to run the project successfully.

To run applications in this service you will require the following:

   - [NodeJS v18+](https://nodejs.org/)
   - [.NET Core v6.0](https://dotnet.microsoft.com/download/dotnet-core/6.0)
   - [Azure Functions Core Tools v4+](https://github.com/Azure/azure-functions-core-tools)
   
To run the databases:
   - [Docker and Docker Compose](https://docs.docker.com/) - see [Setting up the database](#setting-up-the-database-and-storage-emulator)

To emulate Azure storage services (blobs, tables and queues) you will require one of the following options.
   - [Azurite for Docker and Docker Compose](https://docs.docker.com/) - recommended, see [Setting up the storage emulator](#setting-up-the-database-and-storage-emulator)
   - Alternatively, if opting to not use Storage Explorer at all, you could create your own Storage
     Account on Azure and amend your storage connection strings to point to this.
     - [Azure Storage Account](https://azure.microsoft.com/en-gb/services/storage/)
     - [Running against other databases](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator#start-and-initialize-the-storage-emulator)

### Install PNPM via corepack

We use [PNPM](https://pnpm.io/) and [PNPM workspaces](https://pnpm.io/workspaces) to manage our dependencies. PNPM is a drop in replacement 
for [NPM](https://www.npmjs.com/) which has several advantages over its predecessor. You can read more about the benefits 
of PNPM [here](https://pnpm.io/motivation). This is installed & managed via [corepack](https://github.com/nodejs/corepack).

Corepack is a tool installed as part of your Node.js installation that allows you to install and 
manage multiple package manager versions in your environment based on per-project configuration 
(via the `packageManager` field in `package.json`). 

We use corepack to ensure that everyone is using the same version of PNPM to avoid any issues when 
people are using different versions of PNPM. 

In order to install Corepack and the right version of PNPM, run the following command:

```bash
corepack enable
```

If for some reason the above corepack command doesn't work, you can install PNPM manually by running:

```bash
PNPM_VERSION=$(node -e "console.log(require('./package.json').engines.pnpm)")
curl -fsSL https://get.pnpm.io/install.sh | env PNPM_VERSION=$PNPM_VERSION sh -
```

This will install the package manager version specified in the `package.json` file. You can check
that this has been installed by running:

```bash
pnpm -v
```

### Install PNPM dependencies

Install the project's PNPM dependencies by simply running:

```sh
pnpm i
```

### Optional - Add the local site domain to hosts file

You can skip this step if you prefer to use http://localhost.

If you would like to use a 'nicer' URL instead of http://localhost, you can change your `hosts` file 
with the following entry (or similar):

```
127.0.0.1    ees.local
```

### Set up the database and storage emulator hosts

Firstly, you'll need to add the following to your `hosts` file:

```
127.0.0.1    db
127.0.0.1    data-storage
```
   
- On Unix this is located in `/etc/hosts`. 
- On Windows this is located in `C:\Windows\System32\drivers\etc\hosts`.

### Set up your database

There are two options for setting up your database:

#### Option 1 - Use a pre-built development database

We regularly create new development databases that are uploaded Google Drive. Ask a team member if 
you need to access.

These are already bootstrapped with seed data to run tests and start the project. This is the 
**recommended** way of running the project.

This data will need to be loaded into SQL Server by copying the `ees-mssql` directory into the 
project's `data` directory. You **must** give all OS users appropriate access to this directory.

In Linux:
  - The ees-mssql folder needs to be present in an unencrypted folder / partition. The 
    `ees-mssql` folder in the unencrypted location can then be symlinked in to the `data` folder
    using `ln -s /path/to/unencrypted/ees-mssql /path/to/ees/data/ees-mssql`.
  - The Docker container user needs ownership fo the ees-mssql folder. Run  
  `sudo chown -R 10001 /path/to/ees-mssql` to give this Docker user (with id 10001) appropriate 
   permissions.


All the data in the `data/ees-mssql` directory will be mounted and loaded when the SQL Server Docker
container starts. This cna be 

#### Option 2 - Use a bare database

The service can be started against a set of non-existent database. If no pre-existing `content` or 
`statistics` databases yet exist on the target SQL Server instance:

1. Start the SQL Server Docker container:

   ```
   docker-compose up -d db
   ```

2. Create empty `content` and `statistics` databases.
3. Perform a one-off creation of database logins and users. Using Azure Data Studio or similar, 
   connect to these new databases and run:
   ```sql
   -- Against the `master` database
   CREATE Login [adminapp] WITH PASSWORD = 'Your_Password123';
   CREATE Login [importer] WITH PASSWORD = 'Your_Password123';
   CREATE Login [publisher] WITH PASSWORD = 'Your_Password123';
   CREATE Login [content] WITH PASSWORD = 'Your_Password123';
   CREATE Login [data] WITH PASSWORD = 'Your_Password123';
   
   -- Against the `content` database
   CREATE USER [adminapp] FROM LOGIN [adminapp];
   ALTER ROLE [db_ddladmin] ADD MEMBER [adminapp];
   ALTER ROLE [db_datareader] ADD MEMBER [adminapp];
   ALTER ROLE [db_datawriter] ADD MEMBER [adminapp];
   ALTER ROLE [db_securityadmin] add member [adminapp];
   GRANT ALTER ANY USER TO [adminapp];
   
   -- Against the `statistics` database
   CREATE USER [adminapp] FROM LOGIN [adminapp];
   ALTER ROLE [db_ddladmin] ADD MEMBER [adminapp];
   ALTER ROLE [db_datareader] ADD MEMBER [adminapp];
   ALTER ROLE [db_datawriter] ADD MEMBER [adminapp];
   ALTER ROLE [db_securityadmin] add member [adminapp];
   GRANT ALTER ANY USER TO [adminapp];
   GRANT EXECUTE ON TYPE::IdListGuidType TO [adminapp];
   GRANT EXECUTE ON OBJECT::FilteredFootnotes TO [adminapp];
   GRANT SELECT ON OBJECT::geojson TO [adminapp];
   ```
   This will create contained users for the `content` and `statistics` databases as well as allowing 
   the `adminapp` user  to manage the permissions of the contained users.
4. Start the Admin project and this will configure the contained users' permissions via database migrations. 
   The other projects will then be able to be started, using their own contained users to connect to the databases. 

### Setting up an Identity Provider (IdP)

The project uses an OpenID Connect Identity Provider (IdP) to allow login to the Admin service.

> For team members, Azure AD configuration is available alongside other project passwords with the title 
> `Azure AD IdP configuration`. Place the contents of this into [appsettings.Idp.json](
src/GovUk.Education.ExploreEducationStatistics.Admin/appsettings.Idp.json) and start Admin normally.

An out-of-the-box IdP is provided for ease of setup which runs [Keycloak](https://www.keycloak.org/) in a Docker container 
and contains a number of users for different roles. The [appsettings.Keycloak.json](
src/GovUk.Education.ExploreEducationStatistics.Admin/appsettings.Keycloak.json) configuration file contains the details for 
connecting Admin to this IdP.

Alternatively, you can provide your own OpenID Connect configuration in the [appsettings.Idp.json](
src/GovUk.Education.ExploreEducationStatistics.Admin/appsettings.Idp.json) file that is ignored from Git. You can use the 
Keycloak equivalent as a template as to how the configuration in the file should be structured.

#### Using Keycloak

The Keycloak Docker container can be started by one of the following methods:

1. Indirectly by starting the admin via the `start` script:

   ```bash
   pnpm start admin
   ```
   
   This will start the Keycloak container before the admin starts if you haven't created a
   custom `appsettings.Idp.json` file (see earlier).
   
2. Directly via the `start` script:

   ```bash
   pnpm start idp
   ```

3. Directly via Docker Compose:

   ```bash
   src
   docker-compose up idp
   ```

All the standard seed data users can be supported with Keycloak, and use their standard email addresses and the
password `password` to log in.

The standard accounts used day to day are:

  * bau1 - username `bau1` and password `password`
  * bau2 - username `bau2` and password `password`
  * analyst - username `analyst` and password `password`
  * analyst2 - username `analyst2` and password `password`

The [Keycloak Admin login](http://localhost:5030/auth/admin/) is available with username `admin` and password
`admin`. From here, users and OpenID Connect settings can be administered.

##### Adding additional users to Keycloak manually

Additional seed data users can be added to Keycloak by manually adding new entries to the "users" array in
[keycloak-ees-realm.json](src/keycloak/keycloak-ees-realm.json), ensuring to supply unique GUIDs to the `user` and
`credentials` Ids. If copying and pasting from an existing user record in the array, the new user password will be
"password" also.

After this, existing Keycloak Docker containers will need to be rebuilt in order to pick up the new user list. 

To do this, you can run one of the following:

```bash
# Using start script
pnpm start idp --rebuild-docker

# Using Docker
docker-compose up --build --force-recreate idp
```

#### Using a custom Identity Provider

If you have your own custom OpenID Connect identity provider (IdP), you can provide its config in a 
[appsettings.Idp.json](src/GovUk.Education.ExploreEducationStatistics.Admin/appsettings.Idp.json) file 
that is ignored from Git.

You can use [appsettings.Keycloak.json](src/GovUk.Education.ExploreEducationStatistics.Admin/appsettings.Keycloak.json) 
as a template for how the configuration should be structured.

> Note that it must have Implicit Flow enabled and be using the OpenID Connect protocol. It must be set to issue 
ID Tokens.

If you wish, you can explicitly choose which config to load using the `IdpConfig` environment variable:

```
IdpConfig=Keycloak # To use default Keycloak
IdpConfig=Idp      # To use custom IdP
```

This might be useful if you want to toggle between Keycloak and your custom IdP config, but otherwise, 
just remove the `appsettings.Idp.json` to default back to Keycloak.

#### Bootstrapping Keycloak users into a blank database

If you are wanting to use Keycloak but with a fresh database, set the following environment variable:

```
BootstrapUsers=Keycloak
```

This environment variable tells the Admin application to generate a set of BAU users on startup that 
are specified in the [appsettings.KeycloakBootstrapUsers.json](
src/GovUk.Education.ExploreEducationStatistics.Admin/appsettings.KeycloakBootstrapUsers.json) file.

This allows immediate use of the service with Keycloak against an empty database, as corresponding users will
now be in both Keycloak and in the SQL Server database.

#### Bootstrapping different Identity Provider users

If you are using your own IdP config (via `appsettings.Idp.json`), you can bootstrap users who you want to
have access to the system straight away by creating a `appsettings.IdpBootstrapUsers.json` (which is ignored by Git).
This should contain a set of user emails in a format similar to `appsettings.Keycloak.json`. 

Then set the following environment variable before starting the Admin:

```
BootstrapUsers=Idp
```

## Running the service

A good way of running applications/functions is directly through an IDE like [Rider](https://www.jetbrains.com/rider/).

Alternatively, you can use our `start` script. This is a simple wrapper around the various CLI 
commands you need to start the applications. 

This can be most easily accessed via from the root package.json as `pnpm start`.

You will need to ensure you have all the project dependencies as specified in [Requirements](#requirements).

The script provides additional information about its usage by adding the `--help` flag:

```bash
pnpm start --help
```

Examples:

- To start the public frontend backend APIs:

  ```bash
  pnpm start data content
  ```

- To start the public frontend:

   ```bash
   pnpm start frontend
   ```

- To start the admin (front and backend):

  ```bash
  pnpm start admin
  ```

- To start other services:

  ```bash
  pnpm start publisher processor
  ```

The frontend applications can be accessed via:

- `http://localhost:3000` for the public frontend
- `https://localhost:5021` for the admin frontend

### Aliasing the start script

A nice and convenient way to access the start script from anywhere on your machine is to create a 
custom function in your `.bashrc`, `.zshrc` or similar.

This function would look like the following (change to your liking):

```sh
function ees()
{
    (cd your-ees-directory && pnpm start &*)
}
```

You would then be able to use this like:

```sh
ees content data
```

## Frontend development

### Environment variables

Out of the box, each sub-project should come with a default set of environment variables (in `.env`)
that should work for development.

If you need to change any environment variables only for your local environment, you can create a
corresponding `.env.local` file which will be loaded in preference to `.env`. `.env.local` is
ignored by Git.

Various `.env.{env}` files are checked into Git for use in our deployment environments, but will not
generally be used for local development.

#### Adding variables

Required environment variables should be supplied to both the specific `.env.{env}` file and the
`.env.example` file (example versions of variables should be placed here).

The `.env.example` file is used to validate that the `.env.{env}` file in use is not missing any
required variables and consequently needs to be in sync with any changes.

No secrets/keys etc. should be added to these environment variables.

### Dependency management with PNPM

The project currently uses [PNPM](https://pnpm.io) and [PNPM workspaces](https://pnpm.io/workspaces) to handle dependencies as we have
adopted a monorepo project structure and have dependencies between sub-projects. These dependencies
are established using symlinks that PNPM creates.

- `explore-education-statistics-admin`
  - Contains the admin frontend application.
  - Single page application based on Create React App.

- `explore-education-statistics-frontend`
  - Contains the public frontend application.
  - This is a server side rendered Next application.
    
- `explore-education-statistics-common`
  - Contains common code between the other sub-projects for re-use.

- `explore-education-statistics-ckeditor`
 - Contains the customised CKEditor build for the admin application.

#### Adding dependencies

When adding new NPM dependencies, be aware that we need to be careful about where we add them in the
`package.json` file. We very deliberately add our dependencies to either `devDependencies` or 
`dependencies` depending on the subproject.

- `explore-education-statistics-frontend` dependencies are in either `dependencies` or 
  `devDependencies` to avoid including build dependencies in the `node_modules` that are deployed to
  environments.
  
  This is beneficial for cutting down build times, but in the past, we've also experienced weird 
  issues with being unable to deploy to the Azure App Service when there are too many `node_modules` 
  (related to Windows).

- `explore-education-statistics-commmon` dependencies are in `dependencies` as these must all be 
  included in the final build (admin or public).
  
- `explore-education-statistics-admin` dependencies are in `dependencies` simply for consistency
  and simplicity. We need all dependencies to create the build, so it doesn't make sense to split 
  out separate `devDependencies`.

To install new dependencies, you will need to use PNPM to do this, with the following steps:

1. Directly add dependencies to any required `package.json` file(s).
2. Run the following:

```bash
pnpm i
```

#### Cleaning dependencies

During development, you might end up in an inconsistent state where your sub-project `node_modules`
are broken for whatever reason. Consequently, it is advisable to clean down your sub-project
`node_modules` by running the following from the project root.

```bash
pnpm clean
```

### Common PNPM scripts

These scripts can generally be run from most `package.json` files across the project.

- `pnpm test` - Run all tests.

- `pnpm tsc` - Run Typescript compiler to check types are correct. Does not build anything.

- `pnpm lint` - Lint projects using Stylelint and ESLint.
  - `pnpm lint:js` - Run ESLint only.
  - `pnpm lint:style` - Run Stylelint only.
- `pnpm fix` - Fix any lint that can be automatically fixed by the linters.

  - `pnpm fix:js` - Fix only ESLint lints.
  - `pnpm fix:style` - Fix only Stylelint lints.

- `pnpm format` - Format codebase using Prettier.

#### Project root scripts

These can only be run from the project root `package.json`.

- `pnpm clean` - Remove any `node_modules` directories across any sub-projects.

#### Sub-project scripts

These can only be run from a sub-project `package.json`.

- `pnpm start` - Start a sub-project dev server.

### Code style

We enforce the project code style via ESLint and Stylelint. Both are configured with the following
rule sets:

- [Airbnb style guide](https://github.com/airbnb/javascript) for TypeScript/JavaScript.
- [typescript-eslint](https://github.com/typescript-eslint/typescript-eslint/tree/master/packages/eslint-plugin) recommended rules for TypeScript.
- [stylelint-config-sass-guidelines](https://github.com/bjankord/stylelint-config-sass-guidelines) for SCSS.

We also combine this with [Prettier](https://prettier.io/) to format our code to avoid disagreements 
on formatting.

To enforce these code styles, we run linting and formatting tasks upon save, commit and build.

#### Code style exceptions

Typically, we should aim to stick as close as possible to the rule set defaults. However, we do
allow exceptions for rules that:

- Are incompatible with existing/legacy code
- Make the developer experience considerably worse
- Are buggy and don't work correctly
- The team generally disagrees with

#### Disabling linting upon save

If required, you can disable linting upon save by adding one (or both) of the following to your
`.env.local`:

```
ESLINT_DISABLE=true
STYLELINT_DISABLE=true
```

Of course, we don't recommend that you do this as it's usually a good idea to get immediate feedback
that something is wrong.

Note that linting will still run upon commit. Ideally, every commit should have the project in a
buildable state (rather than be completely broken).

## Backend development

### Migrations

The backend c# projects use code first migrations to generate the application's database schema. 
The migration tool is installed by running:

```sh
dotnet tool restore
```

#### Content DB migrations

To generate a migration for the content db:

```sh
cd src/GovUk.Education.ExploreEducationStatistics.Admin
dotnet ef migrations add EES1234_MigrationNameHere --context ContentDbContext --output-dir Migrations/ContentMigrations -v
```

#### Statistics DB migrations

To generate a migration for the statistics db:

```sh
cd src/GovUk.Education.ExploreEducationStatistics.Data.Api
dotnet ef migrations add EES1234_MigrationNameHere --context StatisticsDbContext --project ../GovUk.Education.ExploreEducationStatistics.Data.Model -v
```

#### Users and Roles DB migrations

To generate a migration for the UsersAndRolesDbContext:

```sh
cd src/GovUk.Education.ExploreEducationStatistics.Admin
dotnet ef migrations add EES1234_MigrationNameGoesHere --context UsersAndRolesDbContext --output-dir Migrations/UsersAndRolesMigrations -v
```

### Resetting Azurite

During development you might want to reset your Azurite instance to clear out all data from 
blobs, queues and tables. This is typically done at the same time as resetting the databases.

To delete all data in Azurite simply delete the Azurite docker container, remove the Azurite volume and recreate it:

```bash
docker-compose up data-storage
```


### Taking a backup of Keycloak users

If wanting to add more users to the standard set of users we use and are using Keycloak as the Identity Provider, the users will firstly need to be
added to Keycloak in the EES realm and then the realm exported. To export the realm you can run:

```
docker exec -it ees-idp /opt/jboss/keycloak/bin/standalone.sh -Djboss.socket.binding.port-offset=100 -Dkeycloak.migration.action=export \ 
-Dkeycloak.migration.provider=singleFile -Dkeycloak.migration.realmName=ees-realm -Dkeycloak.migration.usersExportStrategy=REALM_FILE -Dkeycloak.migration.file=/tmp/new-ees-realm.json
```

Then simply copy the file from the `/tmp/new-ees-realm.json` file in the `ees-idp` container to `src/keycloak-ees-realm.json` in order for future restarts of the IdP to use this new 
realm configuration.

### Forcing immediate publishing of scheduled Releases in test environments

During manual or automated testing, it is handy to have a way to schedule releases for publishing but to trigger that process to occur on demand, rather than having to wait for a lengthly
period before the scheduled Publisher Functions run. For this, we provide 2 Functions that can be triggered by HTTP requests; one stages scheduled Releases, whilst the other completes the
publishing process for any staged Releases and makes them live.

See the [Publisher Functions README](src/GovUk.Education.ExploreEducationStatistics.Publisher/README.md) for more information.

### Customise `magic.mgc` version for file validation

In some cases, it may be useful to change the version of the `magic.mgc` file that is used for 
file validation in a project.

You can create an `appsettings.Local.json` file in the relevant project e.g.

```
touch src/GovUk.Education.ExploreEducationStatistics.Data.Processor/appsettings.Local.json
```

Then ensure it has the following:

```json
{
  "MagicFilePath": "/usr/lib/file/magic.mgc"
}
```

The above example uses the default `magic.mgc` used by Ubuntu, but you can change this path to 
whatever you want on your filesystem.

### Robot Tests

Aside from unit tests for each project, we maintain suites of Robot Framework tests that can be found in `tests`.

See the [Robot Framework tests README](tests/robot-tests/README.md) for more information.

## License

This application is licensed under the MIT License.
