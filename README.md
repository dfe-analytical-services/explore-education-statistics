# Explore education statistics service

[![Build Status](https://dfe-gov-uk.visualstudio.com/s101-Explore-Education-Statistics/_apis/build/status/Explore%20Education%20Statistics?branchName=master)](https://dfe-gov-uk.visualstudio.com/s101-Explore-Education-Statistics/_build/latest?definitionId=200&branchName=master)

## Project structure

The project is primarily composed of two areas:

### Public frontend (for general public users)

- **UI**
  - NextJS React app
  - Depends on:
    - Content API
    - Data API

- **Content API**
  - Depends on:
    - Publisher - to generate its cache

- **Data API**
  - Depends on:
    - SQLServer `statistics` database (known as `public-statistics` in non-local environments)

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

   - [NodeJS v12+](https://nodejs.org/)
   - [.NET Core v3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
   - [Azure Functions Core Tools v3+](https://github.com/Azure/azure-functions-core-tools)
   
2. To emulate azure storage you will require either:
   - [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)

  To run Azure Storage Emulator, you'll need to install LocalDB via SQL Express. You can find the installer for Windows x64 [here](https://download.microsoft.com/download/3/9/F/39F968FA-DEBB-4960-8F9E-0E7BB3035959/SQLEXPR_x64_ENU.exe).

   - [Azurite](https://github.com/Azure/Azurite)
   - [Azure Storage Account](https://azure.microsoft.com/en-gb/services/storage/) 
   - [LocalDB](https://download.microsoft.com/download/2/A/5/2A5260C3-4143-47D8-9823-E91BB0121F94/ENU/x64/SqlLocalDB.msi)
   
   You will most likely need to use Windows to run the service due to a dependency on Azure Storage Emulator
for development, specifically the table storage component which is not supported by [Azurite](https://github.com/Azure/Azurite). However this does not apply to every component of the service.

   Alternatively you could create your own Storage Account on Azure and ammend your storage connection string to point to this.

3. To run the databases, you can use either:

   - [SQL Server 2017+](https://www.microsoft.com/en-gb/sql-server/sql-server-downloads)
   - [Docker and Docker Compose](https://docs.docker.com/)

4. **Linux only** - Add symlinks to libmagic-1

   ```
   cd /usr/lib/x86_64-linux-gnu/
   sudo ln -s libmagic.so.1.0.0 libmagic-1.so
   sudo ln -s libmagic.so.1.0.0 libmagic-1.so.1
   ```

   See [bug raised with the library](https://github.com/hey-red/Mime/issues/36) for more info.

### Running the backend

1. Add the following to your `hosts` file:

   ```
   127.0.0.1    db
   127.0.0.1    data-storage
   ```

2. If using SQLServer (instead of Docker), you should create your public frontend databases:

   - `content`
   - `statistics`

3. Ensure you have Azure Storage Emulator running.

#### Running the applications

The recommended way of running backend applications/functions is through the [Rider IDE](https://www.jetbrains.com/rider/).
If this is not available to you then you will need to use one, or a combination, of the following:

##### Using `run` script

The `run` script is a simple wrapper around the various CLI commands you need to run the applications. You will need to
ensure you have all the project dependencies as specified in [Requirements](#requirements).

Examples:

- To run the public frontend:

  ```
  cd useful-scripts
  ./run.js data content publisher
  ```

- To run the admin:

  ```
  cd useful-scripts
  ./run.js admin processor notifier
  ```

##### Using Docker

Parts (but not all) of the service can be run using Docker. 

Examples:

- To run public frontend services:

  ```bash
  cd src/
  docker-compose up -d data-api content-api
  ```

- To run public frontend databases:

  ```
  cd src/
  docker-compose up -d db
  ```

### Running the frontend

1. Run the following from the project root to install all project dependencies:

   ```bash
   npm ci
   npm run bootstrap
   ```

2. Startup any required backend services (see [Running the backend](#running-the-backend))

3. Run the frontend applications by running from the project root

   ```bash
   # Admin frontend
   npm run start:admin

   # Public frontend
   npm run start:frontend
   ```

   Alternatively, go into one of the sub-project directories and start them directly:

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
dotnet tool install -g dotnet-ef --version 3.1.5
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
dotnet ef migrations add E1234MigrationNameHere --context StatisticsDbContext --project ../GovUk.Education.ExploreEducationStatistics.Data.Model -v
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

## Automated tests

Aside from unit tests for each project, we maintain suites of Robot Framework and Postman/Newman 
tests that can be found in `tests`.

See the relevant READMEs:

- [Postman/Newman tests](./tests/newman/README.md)
- [Robot Framework tests](./tests/robot-tests/README.md)

## Contributing

## License

This application is licensed under the MIT License.
