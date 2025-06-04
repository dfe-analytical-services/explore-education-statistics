# Explore Education Statistics service

[![Build Status](https://dfe-gov-uk.visualstudio.com/s101-Explore-Education-Statistics/_apis/build/status/Explore%20Education%20Statistics?branchName=master)](https://dfe-gov-uk.visualstudio.com/s101-Explore-Education-Statistics/_build/latest?definitionId=200&branchName=master)

## Project structure

The project is primarily composed of two areas:

### Public frontend (for public users)

- **UI** - `src/explore-education-statistics-frontend`
  - Next.js React app
  - Depends on:
    - Content API
    - Data API
    - Notifier

- **Content API** - `src/GovUk.Education.ExploreEducationStatistics.Content.Api`
  - Private API providing content for UI app 
  - Depends on:
    - Publisher - to generate its cache

- **Data API** - `src/GovUk.Education.ExploreEducationStatistics.Data.Api`
  - Private API providing statistics for UI app
  - Depends on:
    - SQLServer `statistics` database (known as `public-statistics` in non-local environments)

- **Notifier**
    - Azure function for adding users to GOV.UK Notify

- **Public Data API** - `src/GovUk.Education.ExploreEducationStatistics.Public.Data.Api`
  - Public API providing statistics for public users
  - Depends on:
    - Postgres `public_data` database


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
    - Public Data Processor

- **Publisher** - `src/GovUk.Education.ExploreEducationStatistics.Publisher`
  - Azure function for publishing admin content to the public frontend
  - Depends on:
    - SQLServer `content` database
    - SQLServer `statistics` database
  
- **Notifier** - `src/GovUk.Education.ExploreEducationStatistics.Notifier`
  - Azure function for sending notifications

- **Data Processor** - `src/GovUk.Education.ExploreEducationStatistics.Data.Processor`
  - Azure function for handling data set imports into the admin. 
  - Also referred to as the 'importer' or just 'processor'.
  - Depends on:
    - SQLServer `content` database
    - SQLServer `statistics` database

- **Public Data Processor** - `src/GovUk.Education.ExploreEducationStatistics.Public.Data.Processor`
  - Azure function for processing data sets so they can be used in the public API.
  - Depends on:
    - Postgres `public_data` database

## Getting started

### Requirements

You will need the following groups of dependencies to run the project successfully.

To run applications in this service you will require the following:

   - [NodeJS v18+](https://nodejs.org/)
   - [.NET SDK v8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
   - [Azure Functions Core Tools v4+](https://github.com/Azure/azure-functions-core-tools)

To run the databases:
   - [Docker and Docker Compose](https://docs.docker.com/) - see [Set up your database](#set-up-your-database)

To emulate Azure storage services (blobs, tables and queues) you will require one of the following options.
   - [Azurite for Docker](https://hub.docker.com/_/microsoft-azure-storage-azurite) and [Docker Compose](https://docs.docker.com/) - recommended, see [Setting up the storage emulator](#setting-up-the-storage-emulator).
     - You will also need to [configure the storage emulator host](#set-up-the-database-and-storage-emulator-hosts)
   - Alternatively, if opting to not use a storage emulator at all, you could create your own Storage
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

We use Corepack to ensure that everyone is using the same version of PNPM to avoid any issues when 
people are using different versions of PNPM. 

In order to install Corepack and the right version of PNPM, run the following command:

```bash
corepack install
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

If this fails with an integrity key check, install the latest version of Corepack first:

```bash
npm i -g corepack@latest
corepack install
pnpm i
```

### Set up the database and storage emulator hosts

Add the following to your `hosts` file:

```
127.0.0.1    db
127.0.0.1    data-storage
```

- On Unix this is located in `/etc/hosts`. 
- On Windows this is located in `C:\Windows\System32\drivers\etc\hosts`.

### Add the local site domain to hosts file

> This step is only required if you are using Keycloak as the identity provider, or if you are using 
a custom identity provider and prefer to use a 'nicer' URL instead of http://localhost.

Add the following to your `hosts` file:

```
127.0.0.1    ees.local
```

### Set up your database

There are two options for setting up your database:

#### Option 1 - Use a pre-built development database

We regularly create new development databases that are uploaded to Google Drive. Ask a team member if 
you need access.

These are already bootstrapped with seed data to run tests and start the project. This is the 
**recommended** way of running the project.

The seed data file names are suffixed with a number to identify the latest. Download the most recent
`ees-mssql-data-<number>.zip` and extract the `ees-mssql` directory into the project's `data` directory. 
You **must** give all OS users appropriate access to this directory.

In Linux:
  - The ees-mssql folder needs to be present in an unencrypted folder / partition. The 
    `ees-mssql` folder in the unencrypted location can then be symlinked in to the `data` folder
    using `ln -s /path/to/unencrypted/ees-mssql /path/to/ees/data/ees-mssql`.
  - The Docker container user needs ownership fo the ees-mssql folder. Run  
  `sudo chown -R 10001 /path/to/ees-mssql` to give this Docker user (with id 10001) appropriate 
   permissions.


All the data in the `data/ees-mssql` directory will be mounted and loaded automatically when the SQL Server Docker container starts.

#### Option 2 - Use a bare database

The service can be started against a set of non-existent database. If no pre-existing `content` or 
`statistics` databases yet exist on the target SQL Server instance:

1. Start the SQL Server Docker container:

   ```
   docker compose up -d db
   ```

2. Create empty `content` and `statistics` databases.
3. Perform a one-off creation of database logins and users. Using Azure Data Studio or similar, 
   connect to these new databases and run:
   ```sql
   -- Against the `master` database
   CREATE LOGIN [adminapp] WITH PASSWORD = 'Your_Password123';
   CREATE LOGIN [importer] WITH PASSWORD = 'Your_Password123';
   CREATE LOGIN [publisher] WITH PASSWORD = 'Your_Password123';
   CREATE LOGIN [notifier] WITH PASSWORD = 'Your_Password123';
   CREATE LOGIN [content] WITH PASSWORD = 'Your_Password123';
   CREATE LOGIN [data] WITH PASSWORD = 'Your_Password123';
   CREATE LOGIN [public_data_processor] WITH PASSWORD = 'Your_Password123';

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

### Setting up the storage emulator

The Azurite Docker container can be started by one of the following methods:

1. Indirectly by starting the admin via the start script

    ```bash
    pnpm start admin
    ```

2. Directly via the start script using:
    
    ```bash
    pnpm start dataStorage
    ```

3. Directly via Docker Compose

    ```bash
    docker compose up data-storage
    ```

### Setting up an Identity Provider (IdP)

The project uses an OpenID Connect Identity Provider (IdP) to allow login to the Admin service.

An out-of-the-box IdP is provided for ease of setup which runs [Keycloak](https://www.keycloak.org/) in a Docker container and contains a number of users for different roles. Alternatively, you can follow the steps to [use a custom Identity Provider](#using-a-custom-identity-provider).

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
   docker compose up idp
   ```

All the standard seed data users can be supported with Keycloak, and use their standard email addresses and the
password `password` to log in.

The standard accounts used day to day are:

  * bau1 - username `bau1` and password `password`
  * bau2 - username `bau2` and password `password`
  * analyst - username `analyst` and password `password`
  * analyst2 - username `analyst2` and password `password`

The [Keycloak Admin login](https://ees.local:5031/auth/admin/) is available with username `admin` and password
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
docker compose up --build --force-recreate idp
```

#### Using a custom Identity Provider

If you have your own custom OpenID Connect identity provider (IdP), you can provide its configuration by creating 
an `appsettings.Idp.json` file in the `src/GovUk.Education.ExploreEducationStatistics.Admin` project directory, which is 
excluded from Git. 

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

> Shared test credentials are available to team members for use during development and testing. This step is 
only required for creating *additional* users, or for non-team members within the open source community to create initial users.

If you are using your own IdP config (via `appsettings.Idp.json`), you can bootstrap users who you want to
have access to the system straight away by creating a `appsettings.IdpBootstrapUsers.json`, which is excluded from Git.
This should contain a set of user emails in a format similar to [appsettings.KeycloakBootstrapUsers.json](
src/GovUk.Education.ExploreEducationStatistics.Admin/appsettings.KeycloakBootstrapUsers.json). 

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

- To start admin dependency services:

  ```bash
  pnpm start publisher processor publicProcessor
  ```

- To start the public data API:

  ```bash
  pnpm start publicData
  ```

> If running the `start` script for the first time, and using the seed data downloaded for the recommended 
database setup, you may encounter an SQL error implying a login failure due to the server being in "script 
upgrade mode", this is due to the data still being processed by SQL Server. Depending on the size of the 
data, this may take a minute or two to complete, after which the `start` script can be re-ran.

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
    (cd your-ees-directory && pnpm start $*)
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

### Code Style & Formatting

.NET code style and formatting rules are imposed by [dotnet format](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) because it is IDE and platform agnostic, and natively included in the .NET SDK. Rule are dictated by an [.editorconfig file](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options).

The warnings and errors about violations of these rules are surfaced at build time by enabling [Enforce Code Style In Build (<EnforceCodeStyleInBuild>)](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#enforcecodestyleinbuild) in each `.csproj` file.

To that end - regardless of your IDE of choice - if your solution builds without warnings then your changes should not have caused any style violations. 

*NOTE* We currently have a large number of warnings, many of which we're present before introducing an `.editorconfig` file, and many more of which have been created by imposing style rules we previously weren't. Once the work has been done to resolve these warnings, we can turn on `TreatWarningsAsErrors` to make the build itself fail if there are style violations. 

#### How do I configure my IDE? 

Because the `.editorconfig` file is attached to the solution, both Rider and Visual Studio should automatically use its rules in place of their own defaults.

- on Visual Studio, you may need to visit `Analyze -> Code Cleanup -> Configure Code Cleanup`, then change your default profile to include only the `Fix all warnings and errors set in EditorConfig` step.
- on Rider, you may need to visit `Settings -> Editor -> Inspection Settings` then ensure `Read settings from editorconfig, project settings and rule sets` is ticked.

#### How do I format only one file?

The easiest way is probably through an IDE, configured as per the step above. Most IDEs can format single files if you right-click it in the explorer.
Failing that, the faffier harder way is to pass an `--include <PATH>` argument to `dotnet format` on the command line, providing it the files you want it to format.

#### How do I run the formatter manually?

You can perform the same check as the pre-commit hooks / GitHubs actions do and receive a report by running the following command from the repository root:

```bash
dotnet format src/GovUk.Education.ExploreEducationStatistics.sln --verify-no-changes --report dotnet-format-report.json
```

This will create a report named `dotnet-format-report.json` in the repository root. This has been added to the `.gitignore` file so should not cause a tracked change.

The `--verify-no-changes` argument tells `format` to make no changes. If you want it to automatically apply fixes, simply remove this argument:

```bash
dotnet format src/GovUk.Education.ExploreEducationStatistics.sln
```

If you want to run it against one or more specific directories in the solution (or indeed exclude one or more), these can be specified through the `--include <PATH>` and `--exclude <PATH>` arguments. 

If you want to see only errors, or include suggestions, pass a new value to the severity argument (accepted values are `error`, `warn`, and `info`):

```bash
dotnet format src/GovUk.Education.ExploreEducationStatistics.sln --severity <SEVERITY>
```

See the [docs](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format) for more on this.

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

#### Public Data API DB migrations

To generate a migration for the public data API db:

```sh
cd src/GovUk.Education.ExploreEducationStatistics.Public.Data.Api
dotnet ef migrations add EES1234_MigrationNameHere --context PublicDataDbContext --project ../GovUk.Education.ExploreEducationStatistics.Public.Data.Model -v
```

### Troubleshoot: `Unable to retrieve project metadata` error

when creating a new migration for Admin or public API databases, to workaround the following error:

```
Unable to retrieve project metadata. Ensure it's an SDK-style project. 
If you're using a custom BaseIntermediateOutputPath or MSBuildProjectExtensionsPath values,
Use the --msbuildprojectextensionspath option
```
Then what you should do is set the value of the option parameter `msbuildprojectextensionspath`  to the `artifacts` folder of the application you are making changes to; for example:
1) for the Admin applcation:
```sh
dotnet ef migrations add EES1234_TESTTTT --context ContentDbContext --output-dir Migrations/ContentMigrations -v --msbuildprojectextensionspath C:\Users\USERNAMERepo\explore-education-statistics\src\EES\src\artifacts\obj\GovUk.Education.ExploreEducationStatistics.Admin\
```

2) for the public data API:

```sh
dotnet ef migrations add EES1234_TESTTTT --context PublicDataDbContext --project ../GovUk.Education.ExploreEducationStatistics.Public.Data.Model -v --msbuildprojectextensionspath C:\Users\USERNAMERepo\explore-education-statistics\src\artifacts\obj\GovUk.Education.ExploreEducationStatistics.Public.Data.Model\
```

3) To update the public data API:
```sh
dotnet ef database update EES1234_TESTTTT --msbuildprojectextensionspath C:\Users\USERNAMERepo\explore-education-statistics\explore-education-statistics\src\artifacts\obj\GovUk.Education.ExploreEducationStatistics.Public.Data.Api\
```

### Running backend tests

The backend c# projects have a number of unit and integration tests. From the project root, run:

```sh
cd src
dotnet clean
dotnet test
```

Note that the `clean` is necessary due to an issue with [AspectInjector](https://github.com/pamidur/aspect-injector)
whereby compilation of code over already-compiled code will add AOP execution code on top of existing AOP execution
code, leading to AOP code being invoked multiple times rather than just once. This would result in test failures, as we 
assert that AOP code is executed only once.

#### Configuring Linux for running unit and integration tests

Due to the resource requirements of the integration tests, Linux users need to ensure that the system running the tests
is capable of doing so. The `max_user_watches` and `max_user_instances` settings must be set to a high enough limit,
for example by running:

```sh
echo fs.inotify.max_user_watches=524288 | sudo tee -a /etc/sysctl.conf
echo fs.inotify.max_user_instances=524288 | sudo tee -a /etc/sysctl.conf
sudo sysctl -p
```

### Resetting Azurite

During development you might want to reset your Azurite instance to clear out all data from 
blobs, queues and tables. This is typically done at the same time as resetting the databases.

To delete all data in Azurite simply delete the Azurite docker container, remove the Azurite volume and recreate it:

```bash
docker compose up data-storage
```


### Taking a backup of Keycloak users

If wanting to add more users to the standard set of users we use and are using Keycloak as the Identity Provider, the users will firstly need to be
added to Keycloak in the EES realm and then the realm exported. To export the realm you can run:

```
docker exec -it ees-idp /opt/jboss/keycloak/bin/standalone.sh -Djboss.socket.binding.port-offset=100 -Dkeycloak.migration.action=export \
-Dkeycloak.migration.provider=singleFile -Dkeycloak.migration.realmName=ees-realm -Dkeycloak.migration.usersExportStrategy=REALM_FILE -Dkeycloak.migration.file=/tmp/new-ees-realm.json
```

Wait for the above process to complete by waiting for the console output `Admin console listening on http://127.0.0.1:10090`, then shut it down. 

Then simply copy the file from the `/tmp/new-ees-realm.json` file in the `ees-idp` container to `src/keycloak-ees-realm.json` in order for future restarts 
of the IdP to use this new realm configuration. From the project root, run:

```bash
docker cp ees-idp:/tmp/new-ees-realm.json docker/keycloak/keycloak-ees-realm.json
```

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

## Troubleshooting

### The Function Apps aren't running

If any of the Function Apps (Processor, Publisher or Public Processor) won't spin up successfully, and you're hitting a `Value cannot be null. (Parameter: 'provider')` error,
try upgrading your [Azure Functions Core Tools version](https://github.com/Azure/azure-functions-core-tools).

## License

This application is licensed under the MIT License.
