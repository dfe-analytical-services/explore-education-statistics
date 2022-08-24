#!/usr/bin/env node

/**
 * Script to run parts of the project in parallel.
 * It also labels the output so that everything can
 * conveniently run from the same process.
 *
 * Takes a list of project names e.g.
 * to run the data and content APIs:
 *
 *  ./run data content
 */

const path = require('path');
const chalk = require('chalk');
const spawn = require('cross-spawn');
const { StringStream } = require('scramjet');
const os = require('os');

const args = process.argv.slice(2);

// Set environment variables
if (!process.env.ASPNETCORE_ENVIRONMENT) {
  process.env.ASPNETCORE_ENVIRONMENT = 'Development';
}

const projectRoot = path.resolve(__dirname, '..');

const isWindows = os.platform() === 'win32';

const projects = {
  admin: {
    path: path.join(
      projectRoot,
      'src/GovUk.Education.ExploreEducationStatistics.Admin',
    ),
    command: 'dotnet clean && dotnet run',
    colour: chalk.green,
  },
  adminKeycloak: {
    path: path.join(
      projectRoot,
      'src/GovUk.Education.ExploreEducationStatistics.Admin',
    ),
    command: isWindows
      ? 'SET "IdpProviderConfiguration=Keycloak" && dotnet clean && dotnet run'
      : 'export IdpProviderConfiguration=Keycloak && dotnet clean && dotnet run',
    colour: chalk.green,
  },
  frontend: {
    path: path.join(projectRoot, 'src/explore-education-statistics-frontend'),
    command: 'npm run start:local',
    colour: chalk.greenBright,
  },
  frontendProd: {
    path: path.join(projectRoot, 'src/explore-education-statistics-frontend'),
    command: 'npm run build && npm start',
    colour: chalk.greenBright,
  },
  content: {
    path: path.join(
      projectRoot,
      'src/GovUk.Education.ExploreEducationStatistics.Content.Api',
    ),
    command: 'dotnet clean && dotnet run',
    colour: chalk.cyan,
  },
  data: {
    path: path.join(
      projectRoot,
      'src/GovUk.Education.ExploreEducationStatistics.Data.Api',
    ),
    command: 'dotnet clean && dotnet run',
    colour: chalk.magenta,
  },
  processor: {
    path: path.join(
      projectRoot,
      'src/GovUk.Education.ExploreEducationStatistics.Data.Processor',
    ),
    command: 'dotnet clean && func host start --port=7071 --pause-on-error',
    colour: chalk.rgb(255, 158, 165),
  },
  publisher: {
    path: path.join(
      projectRoot,
      'src/GovUk.Education.ExploreEducationStatistics.Publisher',
    ),
    command: 'dotnet clean && func host start --port=7072 --pause-on-error',
    colour: chalk.yellow,
  },
  notifier: {
    path: path.join(
      projectRoot,
      'src/GovUk.Education.ExploreEducationStatistics.Notifier',
    ),
    command: 'dotnet clean && func host start --port=7073 --pause-on-error',
    colour: chalk.blue,
  },
  idp: {
    path: path.join(projectRoot, 'src'),
    command: 'docker-compose up idp',
    colour: chalk.gray,
  },
  db: {
    path: path.join(projectRoot, 'src'),
    command: 'docker-compose up db',
    colour: chalk.blue,
  },
  dataStorage: {
    path: path.join(projectRoot, 'src'),
    command: 'docker-compose up data-storage',
    colour: chalk.green,
  },
};

const labelStream = (stream, project, transform) =>
  StringStream.from(stream)
    .lines()
    .map(
      line =>
        `${projects[project].colour(`[${project}]`)} ${
          typeof transform === 'function' ? transform(line) : line
        }`,
    )
    .toStringStream()
    .append('\n');

const matchingProjects = args.filter(project => !!projects[project]);

if (!matchingProjects.length) {
  console.log('No matching projects found');
  process.exit(0);
}

matchingProjects.forEach((project, index) => {
  setTimeout(() => {
    const subProcess = spawn(projects[project].command, {
      cwd: projects[project].path,
      shell: true,
    });

    labelStream(subProcess.stdout, project).pipe(process.stdout);
    labelStream(
      subProcess.stderr,
      project,
      line => `${chalk.red('[ERROR]')} ${line}`,
    ).pipe(process.stderr);

    process.on('SIGINT', () => {
      subProcess.kill('SIGINT');
      subProcess.on('close', process.exit);
    });
  }, 1000 * index);
});
