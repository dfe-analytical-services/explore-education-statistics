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

### Setting up an Identity Provider

The project uses an OpenID Connect Identity Provider to allow login to the Admin service.

> For team members, Azure AD configuration is available alongside other project passwords with the title `Azure AD IdP 
configuration`. Place the contents of this into [appsettings.Idp.json](
src/GovUk.Education.ExploreEducationStatistics.Admin\appsettings.Idp.json) and start Admin normally.

An out-of-the-box IdP is provided for ease of setup which runs [Keycloak](https://www.keycloak.org/) in a Docker container 
and contains a number of users for different roles. The [appsettings.Keycloak.json](
src/GovUk.Education.ExploreEducationStatistics.Admin\appsettings.Keycloak.json) configuration file contains the details for 
connecting Admin to this IdP.

Alternatively, you can provide your own OpenID Connect configuration in the [appsettings.Idp.json](
src/GovUk.Education.ExploreEducationStatistics.Admin\appsettings.Idp.json) file that is ignored from Git. You can use the 
Keycloak equivalent as a template as to how the configuration in the file should be structured.

#### Using Keycloak

1. Start up Keycloak:

  ```bash
  cd useful-scripts/
  ./run.js idp
  ```

2. Start up Admin with additional Keycloak configuration:

  ```bash
  cd useful-scripts/
  ./run.js adminKeycloak # this sets the environment variable "IdpProviderConfiguration=Keycloak" for us
  ```

All the standard seed data users can be supported with Keycloak, and use their standard email addresses and the
password `password` to log in.

The [Keycloak Admin login](http://ees.local:5030/auth/admin/) is available with username `admin` and password
`admin`. From here, users and OpenID Connect settings can be administered.

##### Adding additional users to Keycloak manually

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

#### Using a different Identity Provider

If you have your own OpenID Connect IdP set up, you can provide its configuration in the 
[appsettings.Idp.json](src/GovUk.Education.ExploreEducationStatistics.Admin\appsettings.Idp.json) file that is 
ignored from Git. You can use the Keycloak equivalent as a template as to how the configuration in the file should
be structured.

> Note that it must have Implicit Flow enabled and be using the OpenID Connect protocol. It must be set to issue 
ID Tokens.

#### Bootstrapping Keycloak users into a blank database

If you are wanting to use Keycloak but with a fresh database, set the following environment variable:

```
BootstrapUsersConfiguration=KeycloakBootstrapUsers
```

The effect of setting this environment variable tells the Admin application to generate a set of BAU users
on startup that are specified in the 
[src\GovUk.Education.ExploreEducationStatistics.Admin\appsettings.KeycloakBootstrapUsers.json](
src\GovUk.Education.ExploreEducationStatistics.Admin\appsettings.KeycloakBootstrapUsers.json) file.

This allows immediate use of the service with Keycloak against an empty database, as corresponding users will
now be in both Keycloak and in the SQL Server database.

#### Using multiple Identity Providers and user configurations

Alternatively you can provide any number of different OpenID Connect IdP configurations and bootstrap user lists 
by providing files like `src\GovUk.Education.ExploreEducationStatistics.Admin\appsettings.{NameOfYourIdentityProvider}.json` 
and optionally a set of users' email addresses who you want to access the system straight away in a file called 
`src\GovUk.Education.ExploreEducationStatistics.Admin\appsettings.{NameOfYourIdentityProvider}BootstrapUsers.json`.  

Then set the environment variables:

```
IdpProviderConfiguration={NameOfYourIdentityProvider}
BootstrapUsersConfiguration={NameOfYourIdentityProvider}BootstrapUsers
```

and start up Admin wih these environment variables set. This allows you to easily switch between different IdP 
configurations if you have need of more than one for easy reference.

### Running the backend

The recommended way of running backend applications/functions is through the [Rider IDE](https://www.jetbrains.com/rider/).
If this is not available to you then you will need to use one, or a combination, of the following:

#### Using `run` script

The `run` script is a simple wrapper around the various CLI commands you need to run the applications. 
You will need to ensure you have all the project dependencies as specified in [Requirements](#requirements).

Examples:

- To run the public frontend services:

  ```bash
  cd useful-scripts
  ./run.js data content
  ```

- To run the admin. Note you must set up the frontend first - see [Running the frontend](#running-the-frontend).

  ```bash
  cd useful-scripts
  ./run.js admin
  ```

- To run other services:
  
  ```bash
  cd useful-scripts
  ./run.js publisher processor
  ```

### Running the frontend

1. Run the following from the project root to install all project dependencies:

   ```bash
   npm ci
   npm run bootstrap
   ```

2. Startup any required backend services (see [Running the backend](#running-the-backend))

3. Run the frontend applications using any of the following:
   
   - Running using the `run` script:
    
     ```bash
     cd useful-scripts
          
     # Admin frontend
     ./run.js admin   # or ./run.js adminKeycloak if using the Keycloak IdP
     
     # Public frontend
     ./run.js frontend
     ```

   - Running from the project root:

     ```bash
     # Admin frontend
     npm run start:admin
    
     # Public frontend
     npm run start:frontend
     ```
 
    - Going into each of the sub-project directories and starting it directly e.g.
    
     ```bash
     cd src/explore-education-statistics-frontend
     npm start
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

### Dependency management with Lerna

The project currently uses [Lerna](https://github.com/lerna/lerna) to handle dependencies as we have
adopted a monorepo project structure and have dependencies between sub-projects. These dependencies
are established using symlinks that Lerna creates.

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

**DO NOT** install (`npm install`) any dependencies directly into the sub-projects as this will
most likely break the sub-project's `package-lock.json` and cause your installation to fail.

Instead, you will need to use Lerna to do this, with the following steps:

1. Directly add dependencies to any required `package.json` file(s).

2. Run the following from the project root:

    ```bash
    npm run bootstrap:install
    ```

#### Cleaning dependencies

During development, you might end up in an inconsistent state where your sub-project `node_modules`
are broken for whatever reason. Consequently, it is advisable to clean down your sub-project
`node_modules` by running the following from the project root.

```bash
npm run clean
```

### Common NPM scripts

These scripts can generally be run from most `package.json` files across the project.

- `npm test` - Run all tests.

- `npm run tsc` - Run Typescript compiler to check types are correct. Does not build anything.

- `npm run lint` - Lint projects using Stylelint and ESLint.
  - `npm run lint:js` - Run ESLint only.
  - `npm run lint:style` - Run Stylelint only.
- `npm run fix` - Fix any lint that can be automatically fixed by the linters.

  - `npm run fix:js` - Fix only ESLint lints.
  - `npm run fix:style` - Fix only Stylelint lints.

- `npm run format` - Format codebase using Prettier.

#### Project root scripts

These can only be run from the project root `package.json`.

- `npm run bootstrap` - Install NPM dependencies to match `package-lock.json` files across entire 
  project and symlink any dependent modules. This should be used when you want your dependencies to 
  exactly match the project's requirements (e.g. in a fresh repo, or you changed to a different 
  branch).

- `npm run bootstrap:install` - Install NPM dependencies to match `package.json` files across entire
  project and symlink any dependent modules. This should be used when you need to add new 
  dependencies to the project.

- `npm run clean` - Remove any `node_modules` directories across any sub-projects.

- `npm run start:admin` - Run admin frontend dev server.
- `npm run start:frontend` - Run public frontend dev server.

#### Sub-project scripts

These can only be run from a sub-project `package.json`.

- `npm start` - Start a sub-project dev server.

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
dotnet ef migrations add EES1234_MigrationNameHere --context ContentDbContext --output-dir Migrations/ContentMigrations -v
```

#### Statistics DB migrations

To generate a migration for the statistics db:

```
cd explore-education-statistics\src\GovUk.Education.ExploreEducationStatistics.Data.Api
dotnet ef migrations add EES1234_MigrationNameHere --context StatisticsDbContext --project ../GovUk.Education.ExploreEducationStatistics.Data.Model -v
```

#### Users and Roles DB migrations

To generate a migration for the UsersAndRolesDbContext:

```
cd explore-education-statistics\src\GovUk.Education.ExploreEducationStatistics.Admin
dotnet ef migrations add EES1234_MigrationNameGoesHere --context UsersAndRolesDbContext --output-dir Migrations/UsersAndRolesMigrations -v
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
