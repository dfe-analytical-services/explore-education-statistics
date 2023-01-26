# Explore Education Statistics service

[![Build Status](https://dfe-gov-uk.visualstudio.com/s101-Explore-Education-Statistics/_apis/build/status/Explore%20Education%20Statistics?branchName=master)](https://dfe-gov-uk.visualstudio.com/s101-Explore-Education-Statistics/_build/latest?definitionId=200&branchName=master)

## Project structure

The project is primarily composed of two areas:

### Public frontend (for general public users)

- **UI**
  - NextJS React app
  - Depends on:
    - Content API
    - Data API
    - Notifier

- **Content API**
  - Depends on:
    - Publisher - to generate its cache

- **Data API**
  - Depends on:
    - SQLServer `statistics` database (known as `public-statistics` in non-local environments)

- **Notifier**
    - Azure function for adding users to GOV.UK Notify

### Admin (for admins and analysts)

- **UI**
  - CRA React app
  - Depends on:
    - Admin API

- **Admin API**

  - Depends on:
    - SQLServer `content` database
    - SQLServer `statistics` database
    - Publisher
    - Notifier
    - Data Processor

- **Publisher**
  - Azure function for publishing admin content to the public frontend

- **Notifier**
  - Azure function for sending notifications

- **Data Processor**
  - Azure function for handling dataset imports into the admin

## Getting started

### Requirements

You will need the following groups of dependencies to run the project successfully:

1. To run applications in this service you will require the following:

   - [NodeJS v16+](https://nodejs.org/)
   - [.NET Core v6.0](https://dotnet.microsoft.com/download/dotnet-core/6.0)
   - [Azure Functions Core Tools v4+](https://github.com/Azure/azure-functions-core-tools)
   
2. To run the databases, you can use either:

   - [SQL Server 2017+](https://www.microsoft.com/en-gb/sql-server/sql-server-downloads)
   - [Docker and Docker Compose](https://docs.docker.com/) - see [Setting up the database](#setting-up-the-database-and-storage-emulator)

3. To emulate Azure storage services (blobs, tables and queues) you will require one of the following 
   options.
   
   - [Azurite for Docker and Docker Compose](https://docs.docker.com/) - recommended approach if 
     using Linux. See [Setting up the storage emulator](#setting-up-the-database-and-storage-emulator)
   - [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) -
     recommended approach if using Windows. The x64 installer can be found [here](https://download.microsoft.com/download/3/9/F/39F968FA-DEBB-4960-8F9E-0E7BB3035959/SQLEXPR_x64_ENU.exe).
    
   - Previous options that are **no longer** recommended: 
     - Azure Storage Emulator on a Windows VM - if you don't want to use Azurite for Docker, you can 
       install the Azure Storage Emulator on a [Windows 10 VM](https://developer.microsoft.com/en-us/windows/downloads/virtual-machines/).
       
       You will need to expose ports 10001, 10002 and 10003 for the host to access in the emulator 
       in the VM. You will most likely need to install SQL Server on this VM too, as the emulator 
       will need this to function.
       
     - Alternatively, if opting to not use Storage Explorer at all, you could create your own Storage 
       Account on Azure and amend your storage connection strings to point to this.
       - [Azure Storage Account](https://azure.microsoft.com/en-gb/services/storage/) 
       - [Running against other databases](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator#start-and-initialize-the-storage-emulator)
 
4. **Linux only** - Add symlinks to libmagic-1

   ```
   cd /usr/lib/x86_64-linux-gnu/
   sudo ln -s libmagic.so.1.0.0 libmagic-1.so
   sudo ln -s libmagic.so.1.0.0 libmagic-1.so.1
   ```

   See [bug raised with the library](https://github.com/hey-red/Mime/issues/36) for more info.

### Install PNPM via corepack

We use [PNPM](https://pnpm.io/) and [PNPM workspaces](https://pnpm.io/workspaces) to manage our dependencies. PNPM is a drop in replacement for [NPM](https://www.npmjs.com/) which has several advantages over it's predecessor. You can read more about the benefits of PNPM [here](https://pnpm.io/motivation). This is installed & managed via [corepack](https://github.com/nodejs/corepack).

Corepack is a tool installed as part of your Node.js installation that allows you to install and manage multiple package manager versions in your environment based on per-project configuration (via the `packageManager` field in `package.json`). We use corepack to ensure that everyone is using the same version of PNPM to avoid any issues when people are using different versions of PNPM. In order to configure corepack to use PNPM, run the following command:

```bash
corepack enable
```

This will install the package manager version specified in the `package.json` file. You can check that this has been installed by running:

```bash
pnpm -v
```

### Adding the local site domain to hosts file

Add the following to your `hosts` file:

```
127.0.0.1    ees.local
```

### Setting up the database and storage emulator

1. Start the database and storage emulator:

   - If using Docker:

     ```bash
     cd src
     docker-compose up -d db data-storage
     ```

   - If using Windows, ensure SQL Server is running then you can start the Storage Emulator, with the 
     default instance of SQL Server as its data source:

     ```
     AzureStorageEmulator.exe init /server .
     ```

2. Add the following to your `hosts` file:

   ```
   127.0.0.1    db
   127.0.0.1    data-storage
   ```
   
   If using a VM, the IP addresses in this file should be set to your VM's network IP address.
   
#### Use a pre-built development database

We regularly create new development databases that are uploaded to [Confluence](https://dfedigital.atlassian.net/wiki/spaces/EES/pages/2004189186/Testing) and Google Drive. Ask a team member if you need to 
request access.

These are already bootstrapped with seed data to run tests and start the project. This is the 
**recommended** way of running the project. 

This data will need to be loaded into SQL Server:

- Using Docker - copy the `ees-mssql` directory into the project's `data` directory. You **must** 
  give all OS users appropriate access to this directory.
  - In Linux:
    - The ees-mssql folder needs to be present in an unencrypted folder / partition. The 
      `ees-mssql` folder in the unencrypted location can then be symlinked in to the `data` folder
      using `ln -s /path/to/unencrypted/ees-mssql /path/to/ees/data/ees-mssql`.
    - The Docker container user needs ownership fo the ees-mssql folder. Run  
    `sudo chown -R 10001 /path/to/ees-mssql` to give this Docker user (with id 10001) appropriate 
     permissions.
  - In Windows
    - Attach the database using SSMS.

#### Use a bare database

The service can be started against a set of non-existent database. If no pre-existing `content` or 
`statistics` databases yet exist on the target SQL Server instance:

1. Create empty `content` and `statistics` databases.
2. Perform a one-off creation of database logins and users.  Using Azure Data Studio or similar, 
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
   This will create contained users for the `content` and `statistics` databases as well as allowing the `adminapp` user  
   to manage the permissions of the contained users.
3. Start the Admin project and this will configure the contained users' permissions via database migrations. The other 
   projects will then be able to be started, using their own contained users to connect to the databases. 

### Running a different identity provider (optional)

> For running the project day-to-day as a team member, you can ignore this step.

Currently, the project defaults to using Active Directory as its identity provider. Typically, this
will be used by components such as the admin service to allow users to log in.

If you wish use a different identity provider (e.g. working outside the team), you can use:

- Our out-of-the-box identity provider called [Keycloak](https://www.keycloak.org/) (as a Docker 
  container).
- Any OpenID Connect compatible identity provider e.g. Active Directory. It must have Implicit Flow 
  enabled and be using the OpenID Connect protocol. It must be set to issue ID Tokens.

#### Using Keycloak identity provider with standard seed data users

All the standard seed data users can be supported with Keycloak, and use their standard email addresses and the 
password `password` to log in.

The [Keycloak Admin login](http://ees.local:5030/auth/admin/) is available with username `admin` and password 
`admin`.  From here, users and Open ID Connect settings can be administered.

1. To run the out-of-the-box Keycloak identity provider:

  ```bash
  pnpm start idp 
  ```
   
2. To then get Admin to use Keycloak, run:

  ```bash
  
  pnpm start adminKeycloak # this sets the environment variable "IdpProviderConfiguration=Keycloak" for us
  ```

The environment variable `IdpProviderConfiguration` lets Admin know to use 
[appsettings.Keycloak.json](src/GovUk.Education.ExploreEducationStatistics.Admin\appsettings.Keycloak.json) 
for its Open ID Connect configuration.

Additional seed data users can be added to Keycloak by manually adding new entries to the "users" array in
[keycloak-ees-realm.json](src/keycloak/keycloak-ees-realm.json), ensuring to supply unique GUIDs to the `user` and
`credentials` Ids. If copying and pasting from an existing user record in the array, the new user password will be
"password" also.

After this, existing Keycloak Docker containers will need to be rebuilt in order to pick up the new user list. To
do this, run:

```bash
cd src/
docker-compose up --build --force-recreate idp
```

#### Using Keycloak identity provider with custom users

Additionally, if wanting to set up a set of Keycloak users automatically in the service in order
to start using the service against an empty set of databases, set the following environment variable:

```
BootstrapUsersConfiguration=KeycloakBootstrapUsers
```

The effect of setting these 2 environment variables together will allow authentication of users with 
Keycloak, and those users specified within the `src\GovUk.Education.ExploreEducationStatistics.Admin\appsettings.KeycloakBootstrapUsers.json` 
will be available for use as "BAU Users", who have the ability to create new Publications and Releases, 
and invite other users to the system to work on those Publications and Releases.

#### Using your own identity provider

Alternatively you can create an OpenID Connect compatible Identity Provider like Active Directory 
and provide its credentials in a file called `src\GovUk.Education.ExploreEducationStatistics.Admin\appsettings.{NameOfYourIdentityProvider}.json` 
and a set of users' email addresses who you want to access the system straight away in a file called 
`src\GovUk.Education.ExploreEducationStatistics.Admin\appsettings.{NameOfYourIdentityProvider}BootstrapUsers.json`.  

Then set the environment variables above like:

```
IdpProviderConfiguration={NameOfYourIdentityProvider}
BootstrapUsersConfiguration={NameOfYourIdentityProvider}BootstrapUsers
```

If choosing to provide your own OpenID Connect configuration file, you can use the existing Keycloak configuration file
as a reference, at [appsettings.Keycloak.json](src/GovUk.Education.ExploreEducationStatistics.Admin\appsettings.Keycloak.json).

### Running the backend

The recommended way of running backend applications/functions is through the [Rider IDE](https://www.jetbrains.com/rider/).
If this is not available to you then you will need to use one, or a combination, of the following:

#### Using `run` script

The `run` script is a simple wrapper around the various CLI commands you need to run the applications. We've aliased the `run.js` script for convenience in the root package.json.
You will need to ensure you have all the project dependencies as specified in [Requirements](#requirements).

Examples:

- To run the public frontend services:

  ```bash
  pnpm start data content
  ```

- To run the admin. Note you must set up the frontend first - see [Running the frontend](#running-the-frontend).

  ```bash
  pnpm start admin
  ```

- To run other services:
  
  ```bash
  pnpm start publisher processor
  ```

### Running the frontend

1. Run the following to install all project dependencies:

   ```bash
   pnpm i
   ```

2. Startup any required backend services (see [Running the backend](#running-the-backend))

3. Run the frontend applications using any of the following:
   
   - Running using the `run` script:
    
     ```bash
     # Admin frontend
     pnpm start admin   # or pnpm start adminKeycloak if using the Keycloak IdP
     
     # Public frontend
     pnpm start frontend
     ```

   - Running from the project root:

     ```bash
     # Admin frontend
     pnpm start:admin
    
     # Public frontend
     pnpm start:frontend
     ```
 
    - Going into each of the sub-project directories and starting it directly e.g.
    
     ```bash
     cd src/explore-education-statistics-frontend
     pnpm start
     ```

4. Access frontend applications at:

   - `http://localhost:3000` for the public frontend
   - `http://localhost:5021` for the admin frontend

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

- `pnpm start:admin` - Run admin frontend dev server.
- `pnpm start:frontend` - Run public frontend dev server.

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
The entity framework tool will need to be installed as follows:

```
dotnet tool install -g dotnet-ef --version 6.0.2
```

#### Content DB migrations

To generate a migration for the content db:

```
cd explore-education-statistics\src\GovUk.Education.ExploreEducationStatistics.Admin
dotnet ef migrations add EES1234MigrationNameHere --context ContentDbContext --output-dir Migrations/ContentMigrations -v
```

#### Statistics DB migrations

To generate a migration for the statistics db:

```
cd explore-education-statistics\src\GovUk.Education.ExploreEducationStatistics.Data.Api
dotnet ef migrations add EES1234MigrationNameHere --context StatisticsDbContext --project ../GovUk.Education.ExploreEducationStatistics.Data.Model -v
```

#### Users and Roles DB migrations

To generate a migration for the UsersAndRolesDbContext:

```
cd explore-education-statistics\src\GovUk.Education.ExploreEducationStatistics.Admin
dotnet ef migrations add EES1234MigrationNameGoesHere --context UsersAndRolesDbContext --output-dir Migrations/UsersAndRolesMigrations -v
```

### Resetting the storage emulator

During development you might want to reset your storage emulator to clear out all data from 
blobs, queues and tables. This is typically done at the same time as resetting the databases.

To delete all data in the storage emulator:

```
AzureStorageEmulator.exe clear blob queue table
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

### Robot Tests

Aside from unit tests for each project, we maintain suites of Robot Framework tests that can be found in `tests`.

See the [Robot Framework tests README](tests/robot-tests/README.md) for more information.

## Contributing

## License

This application is licensed under the MIT License.
