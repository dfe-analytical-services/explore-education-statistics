#!/usr/bin/env ts-node

import { Argument, Command, Option } from '@commander-js/extra-typings';
import { $, ExecaChildProcess } from 'execa';
import fs from 'node:fs';
import { EOL } from 'node:os';
import path from 'node:path';
import process from 'node:process';
import { spawnSync } from 'node:child_process';
import splitLines from 'split2';
import kill from 'tree-kill';
import delay from './utils/delay';
import exitProcess from './utils/exitProcess';
import { logColours, logInfo } from './utils/logging';
import { getDirname } from './utils/nodeGlobals';
import onExitSignal from './utils/onExitSignal';
import createFileLock from './utils/createFileLock';
import { ExecaChildProcessWithoutNullStreams } from './utils/types';
import {
  buildRunCommand,
  DockerService,
  dotnetBuildLockFile,
  projectRoot,
  resolveDockerServices,
  resolvePublicDataDbAvailability,
  resolveServiceDependencies,
  ServiceName,
  serviceSchemas,
  StartOptions,
} from './services';

const __dirname = getDirname(import.meta.url);
const accountRoot = path.resolve(__dirname, '../..');

const screenerRepositoryName = 'ees-screener-api';
const screenerLocalDir = `${accountRoot}/${screenerRepositoryName}`;
const screenerRepoUrl =
  'https://github.com/dfe-analytical-services/ees-screener-api';

const program = new Command()
  .description(
    `Start one or more project services in parallel.

This script will also run prerequisite tasks to ensure that services will be able to startup:

- Starting Docker services that are required for any services to start correctly (e.g. the database)
- Starting any other app-process services a requested service depends on (e.g. admin also starts processor/publisher; frontend also starts content/data)
- Run a .NET build will be executed for any .NET services (e.g. admin)
`,
  )
  .addHelpText(
    'after',
    `
Examples:

Start frontend:
  $ start frontend

Start frontend in production mode:
  $ start frontendProd

Start frontend in production mode without build step:
  $ start frontendProd --skip-build

Start content and data APIs:
  $ start data content
  
Start public data API:
  $ start publicData

Start admin (processor/publisher are started automatically as dependencies):
  $ start admin

Start admin together with the frontend:
  $ start admin frontend

Start services without first starting any Docker services:
  $ start data content --skip-docker

Start Docker services directly:
  $ start db dataStorage

Restart Docker services:
  $ start db dataStorage --restart-docker  
`,
  )
  .addArgument(
    new Argument('<services...>', 'The services to start').choices(
      Object.keys(serviceSchemas) as ServiceName[],
    ),
  )
  .addOption(new Option('--restart-docker', 'Restart any Docker containers'))
  .addOption(new Option('--rebuild-docker', 'Rebuild any Docker containers'))
  .addOption(new Option('--skip-build', 'Skip build steps where possible'))
  .addOption(
    new Option(
      '--skip-docker',
      'Skip running Docker services that are dependencies of other services',
    ),
  );

program.parse();

const programOpts = program.opts();

const [requestedServices] = program.processedArgs;

// Expand with any app-process services these depend on (e.g. `frontend`
// needs `content`/`data` running, since it calls them directly over HTTP),
// dependencies-first, so they're already up by the time a dependent starts.
function expandServicesWithDependencies(
  services: readonly ServiceName[],
  options: StartOptions,
): ServiceName[] {
  const seen = new Set<ServiceName>();
  const expanded: ServiceName[] = [];

  services.forEach(service => {
    if (seen.has(service)) {
      return;
    }

    expanded.push(
      ...resolveServiceDependencies(service, options, seen),
      service,
    );
    seen.add(service);
  });

  return expanded;
}

// Resolve whether `public-api-db` will be available once, up front, for the
// whole invocation - rather than letting each service's schema work it out
// in isolation, which would make the outcome depend on whether e.g. `admin`
// happened to be listed before or after `publisher` on the command line.
const initialServicesToStart = expandServicesWithDependencies(
  requestedServices,
  programOpts,
);
const publicDataDbAvailable = resolvePublicDataDbAvailability(
  initialServicesToStart,
  programOpts,
);
const startOptions: StartOptions = publicDataDbAvailable
  ? { ...programOpts, env: { PublicDataDbExists: 'true' } }
  : programOpts;

// Re-expand with the resolved options - the override above can itself pull
// in further dependencies (e.g. admin only depends on publicProcessor/
// publicData once PublicDataDbExists is known to be true).
const servicesToStart = expandServicesWithDependencies(
  requestedServices,
  startOptions,
);

await startDockerServices();

const serviceProcesses = new Set<ExecaChildProcess>();

onExitSignal(() => {
  kill(process.pid);
});

// eslint-disable-next-line no-restricted-syntax
for await (const service of servicesToStart) {
  await startService(service);
  await delay(2000);
}

// Call shutdown to try and avoid residual processes hanging around after every build.
// These processes aren't cleaned up properly, resulting in excessive memory usage.
// See: https://github.com/dotnet/sdk/issues/9487
await $({
  cwd: path.join(projectRoot, 'src'),
  reject: false,
})`dotnet build-server shutdown`;

async function startDockerServices() {
  if (programOpts.skipDocker) {
    return;
  }

  const dockerServicesToStart = new Set<DockerService>(
    servicesToStart.flatMap(service =>
      resolveDockerServices(service, startOptions),
    ),
  );

  if (dockerServicesToStart.size > 0) {
    const $$ = $({
      cwd: projectRoot,
      stdio: 'inherit',
    });

    if (programOpts.restartDocker) {
      logInfo('Stopping Docker services...');

      await $$`docker compose stop ${[...dockerServicesToStart]}`;
    }

    logInfo('Starting Docker services...');

    const args = ['-d'];

    if (programOpts.rebuildDocker) {
      args.push('--build', '--force-recreate');
    }

    if (
      servicesToStart.includes('admin') ||
      servicesToStart.includes('dataScreener')
    ) {
      await $$`docker compose down data-screener`;
      await $$`docker compose rm -f data-screener`;

      cloneRequiredRepository(
        screenerRepositoryName,
        screenerLocalDir,
        screenerRepoUrl,
      );

      // Pull the CRAN packages from a repository snapshot 3 weeks old to better ensure
      // that we're grabbing dependencies that have pre-compiled binaries during local
      // development.
      //
      // The Screener API build pipeline will continue to pull from the very latest CRAN
      // repositories as build speed in the build pipeline is not as crucial as it is locally.
      const cranSnapshotDate = getMondayDateStringForPriorWeek(3);
      await $$`docker build --build-arg CRAN_REPOSITORY_SNAPSHOT_VERSION=${cranSnapshotDate} -t explore-education-statistics-data-screener ${screenerLocalDir}`;
    }

    await $$`docker compose up ${[...args, ...dockerServicesToStart]}`;

    await delay(1000);
  }
}

async function startService(service: ServiceName): Promise<void> {
  const schema = serviceSchemas[service];

  let command: string;
  let args: string[] = [];

  let lockUntilReady = false;
  let checkReady: ((line: string) => boolean) | undefined;

  let env: NodeJS.ProcessEnv = {
    ...process.env,
  };

  if (schema.type === 'docker') {
    command = 'docker compose logs';
    args = ['-f', '--no-log-prefix'];

    if (programOpts.rebuildDocker) {
      args.push('--build', '--force-recreate');
    }

    args.push(schema.service);
  } else {
    const runCommand = buildRunCommand(service, startOptions, env);

    command = runCommand.command;
    args = runCommand.args;
    env = runCommand.env;
    checkReady = runCommand.checkReady;
    lockUntilReady = runCommand.lockUntilReady;
  }

  const message = logColours.info(
    'Waiting for another build to finish (started from the CLI or the dashboard)...',
  );

  const unlock = lockUntilReady
    ? await createFileLock({
        lockFile: dotnetBuildLockFile,
        lockTimeout: 300_000,
        waitTimeout: 300_000,
        onExistingLock: () => logService(service, message),
      })
    : undefined;

  logService(service, logColours.info('Starting service...'));

  const serviceProcess = $({
    cwd: path.join(projectRoot, schema.type === 'docker' ? '' : schema.root),
    env,
    shell: true,
    cleanup: false,
  })`${command} ${args}` as ExecaChildProcessWithoutNullStreams;

  serviceProcesses.add(serviceProcess);

  return new Promise<void>(resolve => {
    let isReady = false;

    const startNextService = async () => {
      isReady = true;
      await unlock?.();
      resolve();
    };

    if (!lockUntilReady) {
      startNextService();
    }

    serviceProcess.stdout
      .pipe(
        tagServiceStream(service, line => {
          if (!isReady && checkReady?.(line)) {
            // Don't need to await this
            startNextService();
          }

          return line;
        }),
      )
      .pipe(process.stdout);

    serviceProcess.stderr
      .pipe(tagServiceStream(service, logColours.error))
      .pipe(process.stderr);

    serviceProcess.on('exit', async (code, signal) => {
      serviceProcesses.delete(serviceProcess);

      if (!isReady) {
        // Let the next service run
        await startNextService();
        return;
      }

      if (serviceProcesses.size > 0) {
        return;
      }

      exitProcess({ signal, code });
    });
  });
}

function tagServiceStream(
  service: ServiceName,
  transform?: (line: string) => string,
) {
  return splitLines(line => {
    return `${serviceSchemas[service].colour(`[${service}]`)} ${
      typeof transform === 'function' ? transform(line) : line
    }${EOL}`;
  });
}

function logService(service: ServiceName, message: string): void {
  const { colour } = serviceSchemas[service];

  console.info(`${colour(`[${service}]`)} ${message}`);
}

function cloneRequiredRepository(
  repositoryName: string,
  localDirectory: string,
  repositoryUrl: string,
) {
  const localDirectoryExists = fs.existsSync(localDirectory);

  if (!localDirectoryExists) {
    console.log(`Cloning required repository '${repositoryName}'`);
    const clone = spawnSync('git', ['clone', repositoryUrl, localDirectory], {
      stdio: 'inherit',
    });
    if (clone.status !== 0) {
      console.error(`Failed to clone repository '${repositoryName}'`);
    }
  } else {
    console.log(
      `Repository '${repositoryName}' already exists locally, pulling latest changes`,
    );
    const pull = spawnSync('git', ['-C', localDirectory, 'pull'], {
      stdio: 'inherit',
    });
    if (pull.status !== 0) {
      console.error(`Failed to pull repository '${repositoryName}'`);
    }
  }
}

function getMondayDateStringForPriorWeek(numberOfWeeksPrior: number): string {
  const today = new Date();
  const dayOfWeek = today.getDay();
  const isoDayOfWeek = dayOfWeek === 0 ? 7 : dayOfWeek;

  const daysToSubtract = isoDayOfWeek + numberOfWeeksPrior * 7 - 1;

  const previousMonday = new Date(today);
  previousMonday.setDate(today.getDate() - daysToSubtract);
  return previousMonday.toISOString().split('T')[0];
}
